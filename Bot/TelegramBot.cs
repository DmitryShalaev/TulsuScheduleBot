using System.Globalization;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.Bot {
    public partial class TelegramBot {
        private readonly ITelegramBotClient telegramBot;
        private readonly Scheduler.Scheduler scheduler;
        private readonly ScheduleDbContext dbContext;

        private readonly ReplyKeyboardMarkup MainKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { "Сегодня", "Завтра" },
                            new KeyboardButton[] { "По дням", "На неделю" },
                            new KeyboardButton[] { "Успеваемость" }
                        })
        { ResizeKeyboard = true };

        private readonly ReplyKeyboardMarkup DaysKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { "Понедельник", "Вторник" },
                            new KeyboardButton[] { "Среда", "Четверг" },
                            new KeyboardButton[] { "Пятница", "Суббота" },
                            new KeyboardButton[] { "Назад" }
                        })
        { ResizeKeyboard = true };

        private readonly ReplyKeyboardMarkup WeekKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { "Эта неделя", "Следующая неделя" },
                            new KeyboardButton[] { "Назад" }
                        })
        { ResizeKeyboard = true };

        private readonly InlineKeyboardMarkup inlineAdminKeyboardMarkup = new(new[]{
            new [] { InlineKeyboardButton.WithCallbackData(text: "Посмотреть все", callbackData: "All") },
            new [] { InlineKeyboardButton.WithCallbackData(text: "Редактировать", callbackData: "Edit") }
        }){};

        private readonly InlineKeyboardMarkup inlineKeyboardMarkup = new(new[]{
            new [] { InlineKeyboardButton.WithCallbackData(text: "Посмотреть все", callbackData: "All") }
        }){};

        private readonly InlineKeyboardMarkup inlineBackKeyboardMarkup = new(new[]{
            new [] { InlineKeyboardButton.WithCallbackData(text: "Назад", callbackData: "Back") }
        }){};

        public TelegramBot(Scheduler.Scheduler scheduler, ScheduleDbContext dbContext) {
            this.scheduler = scheduler;
            this.dbContext = dbContext;

            telegramBot = new TelegramBotClient(Environment.GetEnvironmentVariable("TelegramBotToken") ?? "");

            Console.WriteLine("Запущен бот " + telegramBot.GetMeAsync().Result.FirstName);

            telegramBot.ReceiveAsync(
                HandleUpdateAsync,
                HandleError,
            new ReceiverOptions {
                AllowedUpdates = { },
                ThrowPendingUpdates = true
            },
            new CancellationTokenSource().Token
           ).Wait();
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
#if DEBUG
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
#endif
            Message? message = update.Message ?? update.EditedMessage ?? update.CallbackQuery?.Message;
            TelegramUser? user;

            if(message is not null) {
                user = dbContext.TelegramUsers.FirstOrDefault(u => u.ChatId == message.Chat.Id);

                if((update.Type == Telegram.Bot.Types.Enums.UpdateType.Message || update.Type == Telegram.Bot.Types.Enums.UpdateType.EditedMessage)) {

                    if(update.Message?.From is not null) {
                        if(user is null) {
                            user = new() { ChatId = message.Chat.Id, FirstName = update.Message.From.FirstName, Username = update.Message.From.Username, LastName = update.Message.From.LastName };
                            dbContext.TelegramUsers.Add(user);
                            dbContext.SaveChanges();
                        }

                        var text = message.Text?.ToLower();
                        switch(text) {
                            case "/start":
                                await botClient.SendTextMessageAsync(chatId: message.Chat, text: $"👋 {telegramBot.GetMeAsync(cancellationToken: cancellationToken).Result.Username} 👋", replyMarkup: MainKeyboardMarkup);
                                break;

                            case "назад":
                                await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Основное меню", replyMarkup: MainKeyboardMarkup);
                                break;

                            case "сегодня":
                            case "завтра":
                                await TodayAndTomorrow(botClient, message.Chat, text, user);
                                break;

                            case "по дням":
                                await botClient.SendTextMessageAsync(chatId: message.Chat, text: "По дням", replyMarkup: DaysKeyboardMarkup);
                                break;

                            case "понедельник":
                            case "вторник":
                            case "среда":
                            case "четверг":
                            case "пятница":
                            case "суббота":
                                await DayOfWeek(botClient, message.Chat, text, user);
                                break;

                            case "на неделю":
                                await botClient.SendTextMessageAsync(chatId: message.Chat, text: "На неделю", replyMarkup: WeekKeyboardMarkup);
                                break;

                            case "эта неделя":
                            case "следующая неделя":
                                await Weeks(botClient, message.Chat, text, user);
                                break;

                            case "успеваемость":
                                await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Семестр", replyMarkup: GetTermsKeyboardMarkup());
                                break;

                            default:
                                var split = text?.Split();
                                if(split == null || split.Count() < 2) return;

                                switch(split[1]) {
                                    case "семестр":
                                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: scheduler.GetProgressByTerm(int.Parse(split[0])), replyMarkup: GetTermsKeyboardMarkup());
                                        break;
                                }

                                break;
                        }
                    }

                } else if(update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery && user is not null) {

                    if(DateOnly.TryParse(message.Text?.Split('-')[0].Trim()[2..] ?? "", out DateOnly date)) {

                        switch(update.CallbackQuery?.Data) {
                            case "Edit":
                                await botClient.EditMessageTextAsync(chatId: message.Chat, messageId: message.MessageId, text: scheduler.GetScheduleByDate(date, true), replyMarkup: GetEditAdminInlineKeyboardButton(date));
                                break;

                            case "All":

                                await botClient.EditMessageTextAsync(chatId: message.Chat, messageId: message.MessageId, text: scheduler.GetScheduleByDate(date, true), replyMarkup: inlineBackKeyboardMarkup);
                                break;

                            case "Back":

                                await botClient.EditMessageTextAsync(chatId: message.Chat, messageId: message.MessageId, text: scheduler.GetScheduleByDate(date), replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
                                break;

                            default:
                                List<string> str = update.CallbackQuery?.Data?.Split(' ').ToList() ?? new();
                                if(str.Count < 2) return;

                                var discipline = dbContext.Disciplines.FirstOrDefault(i => i.Id == int.Parse(str[1]));

                                if(discipline is not null) {
                                    switch(str[0] ?? "") {
                                        case "Day":

                                            discipline.IsCompleted = !discipline.IsCompleted;

                                            dbContext.SaveChanges();
                                            await botClient.EditMessageReplyMarkupAsync(chatId: message.Chat, messageId: message.MessageId, replyMarkup: GetEditAdminInlineKeyboardButton(date));

                                            break;

                                        case "Always":

                                            var completedDisciplines = dbContext.CompletedDisciplines.FirstOrDefault(i=> i.Name == discipline.Name && i.Lecturer == discipline.Lecturer && i.Class == discipline.Class && i.Subgroup == discipline.Subgroup);

                                            if(completedDisciplines is not null)
                                                dbContext.CompletedDisciplines.Remove(completedDisciplines);
                                            else
                                                dbContext.CompletedDisciplines.Add(discipline);

                                            dbContext.SaveChanges();
                                            Parser.SetDisciplineIsCompleted(dbContext.CompletedDisciplines.ToList(), dbContext.Disciplines);
                                            dbContext.SaveChanges();

                                            await botClient.EditMessageReplyMarkupAsync(chatId: message.Chat, messageId: message.MessageId, replyMarkup: GetEditAdminInlineKeyboardButton(date));

                                            break;
                                    }
                                }
                                break;
                        }
                    }
                }
            }

        }

        private ReplyKeyboardMarkup GetTermsKeyboardMarkup() {
            List<KeyboardButton[]> TermsKeyboardMarkup = new();

            var terms = dbContext.Progresses.Where(i => i.Mark != null).Select(i => i.Term).Distinct().OrderBy(i => i).ToArray();
            for(int i = 0; i < terms.Length; i++)
                TermsKeyboardMarkup.Add(new KeyboardButton[] { $"{terms[i]} семестр", i + 1 < terms.Length ? $"{terms[++i]} семестр" : "" });

            TermsKeyboardMarkup.Add(new KeyboardButton[] { "Назад" });

            return new(TermsKeyboardMarkup) { ResizeKeyboard = true };
        }

        private async Task TodayAndTomorrow(ITelegramBotClient botClient, ChatId chatId, string text, TelegramUser user) {
            await botClient.SendTextMessageAsync(chatId: chatId, text: $"Расписание актуально на {Parser.lastUpdate.ToString("dd.MM.yyyy HH:mm")}", replyMarkup: MainKeyboardMarkup);

            switch(text) {
                case "сегодня":
                    await botClient.SendTextMessageAsync(chatId: chatId, text: scheduler.GetScheduleByDate(DateOnly.FromDateTime(DateTime.Now)), replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
                    break;

                case "завтра":
                    await botClient.SendTextMessageAsync(chatId: chatId, text: scheduler.GetScheduleByDate(DateOnly.FromDateTime(DateTime.Now.AddDays(1))), replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
                    break;
            }
        }

        private async Task DayOfWeek(ITelegramBotClient botClient, ChatId chatId, string text, TelegramUser user) {
            await botClient.SendTextMessageAsync(chatId: chatId, text: $"Расписание актуально на {Parser.lastUpdate.ToString("dd.MM.yyyy HH:mm")}", replyMarkup: DaysKeyboardMarkup);
            switch(text) {
                case "понедельник":
                    foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Monday))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
                case "вторник":
                    foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Tuesday))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
                case "среда":
                    foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Wednesday))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
                case "четверг":
                    foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Thursday))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
                case "пятница":
                    foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Friday))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
                case "суббота":
                    foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Saturday))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
            }
        }

        private async Task Weeks(ITelegramBotClient botClient, ChatId chatId, string text, TelegramUser user) {
            await botClient.SendTextMessageAsync(chatId: chatId, text: $"Расписание актуально на {Parser.lastUpdate.ToString("dd.MM.yyyy HH:mm")}", replyMarkup: WeekKeyboardMarkup);
            switch(text) {
                case "эта неделя":
                    foreach(var item in scheduler.GetScheduleByWeak(CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, System.DayOfWeek.Monday) - 1))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: item, replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
                case "следующая неделя":
                    foreach(var item in scheduler.GetScheduleByWeak(CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, System.DayOfWeek.Monday)))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: item, replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
            }
        }

        private InlineKeyboardMarkup GetEditAdminInlineKeyboardButton(DateOnly date) {
            List<InlineKeyboardButton[]> editButtons = new();

            editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: "В этот день", callbackData: "!"), InlineKeyboardButton.WithCallbackData(text: "Всегда", callbackData: "!") });

            var completedDisciplinesList = dbContext.CompletedDisciplines.ToList();
            foreach(var item in dbContext.Disciplines.Where(i => i.Date == date).OrderBy(i => i.StartTime).ToList()) {
                var completedDisciplines = completedDisciplinesList.FirstOrDefault(i => i.Equals(item));

                editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: $"{item.StartTime.ToString()} {item.Lecturer?.Split(' ')[0]} {(item.IsCompleted ? "✅" : "❌")}", callbackData: $"Day {item.Id}"),
                                        InlineKeyboardButton.WithCallbackData(text: completedDisciplines is not null ? "✅" : "❌", callbackData: $"Always {item.Id}")});
            }

            editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: "Назад", callbackData: "Back") });

            return editButtons.ToArray();
        }

        private Task HandleError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) {
            return Task.CompletedTask;
        }
    }
}
