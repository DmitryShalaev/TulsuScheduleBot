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
                            new KeyboardButton[] { "По дням", "На неделю" }
                        })
        { ResizeKeyboard = true };


        private readonly ReplyKeyboardMarkup DaysKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { "Понедельник", "Вторник" },
                            new KeyboardButton[] { "Среда", "Четверг" },
                            new KeyboardButton[] { "Пятница", "Суббота" },
                            new KeyboardButton[] { "Назад", }
                        })
        { ResizeKeyboard = true };

        private readonly ReplyKeyboardMarkup WeekKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { "Эта неделя", "Следующая неделя" },
                            new KeyboardButton[] { "Назад", }
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

            telegramBot = new TelegramBotClient(Environment.GetEnvironmentVariable("TelegramBotToken") ?? "5942426712:AAEZZHTqmbzIUEXfPCakJ76VN57YXGmImA8");

            Console.WriteLine("Запущен бот " + telegramBot.GetMeAsync().Result.FirstName);

            telegramBot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                new ReceiverOptions {
                    AllowedUpdates = { },
                },
                new CancellationTokenSource().Token
            );
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            Message? message = update.Message ?? update.EditedMessage ?? update.CallbackQuery?.Message;
            TelegramUser? user;

            if(message is not null) {
                user = dbContext.TelegramUsers.FirstOrDefault(u => u.ChatId == message.Chat.Id);

                if((update.Type == Telegram.Bot.Types.Enums.UpdateType.Message || update.Type == Telegram.Bot.Types.Enums.UpdateType.EditedMessage)) {

                    if(update.Message?.From is not null) {
                        if(user is null) {
                            user = new() { ChatId = message.Chat.Id, FirstName = update.Message.From.FirstName, Username = update.Message.From.Username, LastName = update.Message.From.LastName, LastAppeal = DateTime.UtcNow };
                            dbContext.TelegramUsers.Add(user);
                            dbContext.SaveChanges();
                        }
                        user.LastAppeal = DateTime.UtcNow;
                        dbContext.SaveChanges();


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
                                await botClient.SendTextMessageAsync(chatId: message.Chat, text: $"Расписание актуально на {Parser.lastUpdate.ToString("dd.MM.yyyy HH:mm")}", replyMarkup: MainKeyboardMarkup);
                                switch(text) {
                                    case "сегодня":
                                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: scheduler.GetScheduleByDate(DateOnly.FromDateTime(DateTime.Now)), replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
                                        break;

                                    case "завтра":
                                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: scheduler.GetScheduleByDate(DateOnly.FromDateTime(DateTime.Now.AddDays(1))), replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
                                        break;
                                }
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
                                await botClient.SendTextMessageAsync(chatId: message.Chat, text: $"Расписание актуально на {Parser.lastUpdate.ToString("dd.MM.yyyy HH:mm")}", replyMarkup: DaysKeyboardMarkup);
                                switch(text) {
                                    case "понедельник":
                                        foreach(var day in scheduler.GetScheduleByDay(DayOfWeek.Monday))
                                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: day, replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                                        break;
                                    case "вторник":
                                        foreach(var day in scheduler.GetScheduleByDay(DayOfWeek.Tuesday))
                                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: day, replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                                        break;
                                    case "среда":
                                        foreach(var day in scheduler.GetScheduleByDay(DayOfWeek.Wednesday))
                                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: day, replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                                        break;
                                    case "четверг":
                                        foreach(var day in scheduler.GetScheduleByDay(DayOfWeek.Thursday))
                                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: day, replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                                        break;
                                    case "пятница":
                                        foreach(var day in scheduler.GetScheduleByDay(DayOfWeek.Friday))
                                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: day, replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                                        break;
                                    case "суббота":
                                        foreach(var day in scheduler.GetScheduleByDay(DayOfWeek.Saturday))
                                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: day, replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                                        break;
                                }
                                break;

                            case "на неделю":
                                await botClient.SendTextMessageAsync(chatId: message.Chat, text: "На неделю", replyMarkup: WeekKeyboardMarkup);
                                break;

                            case "эта неделя":
                            case "следующая неделя":
                                await botClient.SendTextMessageAsync(chatId: message.Chat, text: $"Расписание актуально на {Parser.lastUpdate.ToString("dd.MM.yyyy HH:mm")}", replyMarkup: WeekKeyboardMarkup);
                                switch(text) {
                                    case "эта неделя":
                                        foreach(var item in scheduler.GetScheduleByWeak(CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday) - 1))
                                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: item, replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                                        break;
                                    case "следующая неделя":
                                        foreach(var item in scheduler.GetScheduleByWeak(CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday)))
                                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: item, replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                                        break;
                                }
                                break;
                        }
                        return;

                    }

                } else if(update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery && user is not null) {

                    if(DateOnly.TryParse(message.Text?.Split('-')[0].Trim()[2..] ?? "", out DateOnly date)) {

                        switch(update.CallbackQuery?.Data) {
                            case "Edit":
                                InlineKeyboardMarkup ff = new(GetEditAdminInlineKeyboardButton(date)) { };
                                await botClient.EditMessageReplyMarkupAsync(chatId: message.Chat, messageId: message.MessageId, replyMarkup: ff);
                                break;

                            case "All":

                                await botClient.EditMessageTextAsync(chatId: message.Chat, messageId: message.MessageId, text: scheduler.GetScheduleByDate(date, true), replyMarkup: inlineBackKeyboardMarkup);
                                break;

                            case "Back":

                                await botClient.EditMessageTextAsync(chatId: message.Chat, messageId: message.MessageId, text: scheduler.GetScheduleByDate(date), replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
                                break;

                            default:
                                List<string> str = update.CallbackQuery?.Data?.Split(' ').ToList() ?? new();
                                if(str.Count < 3) return;

                                if(TimeOnly.TryParse(str[1], out TimeOnly time)) {

                                    var discipline = dbContext.Disciplines.ToList().First(i => i.Date == date && i.StartTime == time && i.Subgroup != Parser.notSub && (i.Lecturer?.Split()[0] ?? "") == str[2]);

                                    switch(str[0] ?? "") {
                                        case "Day":

                                            discipline.IsCompleted = !discipline.IsCompleted;

                                            dbContext.SaveChanges();
                                            await botClient.EditMessageReplyMarkupAsync(chatId: message.Chat, messageId: message.MessageId, replyMarkup: (new(GetEditAdminInlineKeyboardButton(date)) { }));

                                            break;

                                        case "Always":

                                            var completedDisciplines = dbContext.CompletedDisciplines.FirstOrDefault(i=> i.Name == discipline.Name && i.Lecturer == discipline.Lecturer && i.Class == discipline.Class);

                                            if(completedDisciplines is not null)
                                                dbContext.CompletedDisciplines.Remove(completedDisciplines);
                                            else
                                                dbContext.CompletedDisciplines.Add(new() { Name = discipline.Name, Lecturer = discipline.Lecturer, Class = discipline.Class });

                                            dbContext.SaveChanges();
                                            Parser.SetDisciplineIsCompleted(dbContext.CompletedDisciplines.ToList(), dbContext.Disciplines.Where(i => i.Subgroup != Parser.notSub));
                                            dbContext.SaveChanges();

                                            await botClient.EditMessageReplyMarkupAsync(chatId: message.Chat, messageId: message.MessageId, replyMarkup: (new(GetEditAdminInlineKeyboardButton(date)) { }));

                                            break;
                                    }
                                }
                                break;
                        }
                    }
                }
            }

        }

        private InlineKeyboardButton[][] GetEditAdminInlineKeyboardButton(DateOnly date) {
            List<InlineKeyboardButton[]> editButtons = new();

            editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: "В этот день", callbackData: "!"), InlineKeyboardButton.WithCallbackData(text: "Всегда", callbackData: "!") });

            foreach(var item in dbContext.Disciplines.ToList().Where(i => i.Date == date && i.Subgroup != Parser.notSub).OrderBy(i => i.StartTime)) {
                CompletedDiscipline? completedDisciplines = dbContext.CompletedDisciplines.FirstOrDefault(i => i.Name == item.Name && i.Lecturer == item.Lecturer && i.Class == item.Class );

                string callbackData = $"{item.StartTime.ToString()} {item.Lecturer?.Split()[0]}";

                editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: $"{item.StartTime.ToString()} {item.Lecturer?.Split(' ')[0]} {(item.IsCompleted ? "✅" : "❌")}", callbackData: $"Day {callbackData}"),
                                        InlineKeyboardButton.WithCallbackData(text: completedDisciplines is not null ? "✅" : "❌", callbackData: $"Always {callbackData}")});
            }

            editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: "Назад", callbackData: "Back") });

            var dd = editButtons.ToArray();
            return editButtons.ToArray();
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
            return Task.CompletedTask;
        }

    }
}
