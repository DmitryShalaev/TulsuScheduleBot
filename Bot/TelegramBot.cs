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

        private readonly InlineKeyboardMarkup inlineKeyboardMarkup = new(new[]{
            new [] { InlineKeyboardButton.WithCallbackData(text: "Edit", callbackData: "Edit") }
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
            Message? message = update.Message ?? update.EditedMessage;
            TelegramUser? user;

            if(message != null) {
                if(update.Message?.From is not null && (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message || update.Type == Telegram.Bot.Types.Enums.UpdateType.EditedMessage)) {

                    user = dbContext.TelegramUsers.FirstOrDefault(u => u.ChatId == message.Chat.Id);
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
                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: $"Расписание актуально на {Parser.lastUpdate.ToString("dd.MM.yyyy HH:mm")}", replyMarkup: MainKeyboardMarkup);
                            switch(text) {
                                case "сегодня":
                                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: scheduler.GetScheduleByDate(DateOnly.FromDateTime(DateTime.Now)), replyMarkup: user.IsAdmin ? inlineKeyboardMarkup : MainKeyboardMarkup);
                                    break;

                                case "завтра":
                                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: scheduler.GetScheduleByDate(DateOnly.FromDateTime(DateTime.Now.AddDays(1))), replyMarkup: user.IsAdmin ? inlineKeyboardMarkup : MainKeyboardMarkup);
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
                                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: day, replyMarkup: user.IsAdmin ? inlineKeyboardMarkup : DaysKeyboardMarkup);

                                    break;
                                case "вторник":
                                    foreach(var day in scheduler.GetScheduleByDay(DayOfWeek.Tuesday))
                                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: day, replyMarkup: user.IsAdmin ? inlineKeyboardMarkup : DaysKeyboardMarkup);

                                    break;
                                case "среда":
                                    foreach(var day in scheduler.GetScheduleByDay(DayOfWeek.Wednesday))
                                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: day, replyMarkup: user.IsAdmin ? inlineKeyboardMarkup : DaysKeyboardMarkup);

                                    break;
                                case "четверг":
                                    foreach(var day in scheduler.GetScheduleByDay(DayOfWeek.Thursday))
                                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: day, replyMarkup: user.IsAdmin ? inlineKeyboardMarkup : DaysKeyboardMarkup);

                                    break;
                                case "пятница":
                                    foreach(var day in scheduler.GetScheduleByDay(DayOfWeek.Friday))
                                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: day, replyMarkup: user.IsAdmin ? inlineKeyboardMarkup : DaysKeyboardMarkup);

                                    break;
                                case "суббота":
                                    foreach(var day in scheduler.GetScheduleByDay(DayOfWeek.Saturday))
                                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: day, replyMarkup: user.IsAdmin ? inlineKeyboardMarkup : DaysKeyboardMarkup);

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
                                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: item, replyMarkup: user.IsAdmin ? inlineKeyboardMarkup : WeekKeyboardMarkup);

                                    break;
                                case "следующая неделя":
                                    foreach(var item in scheduler.GetScheduleByWeak(CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday)))
                                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: item, replyMarkup: user.IsAdmin ? inlineKeyboardMarkup : WeekKeyboardMarkup);

                                    break;
                            }
                            break;
                    }
                    return;
                }
            }

            if(update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery) {
                message = update.CallbackQuery?.Message;
                if(message != null && update.CallbackQuery != null) {
                    if(DateOnly.TryParse(message.Text?.Split('-')[0].Trim()[2..] ?? "", out DateOnly date)) {

                        if(update.CallbackQuery.Data == "Edit") {
                            List<InlineKeyboardButton[]> buttons = new();

                            foreach(var item in dbContext.Disciplines.Where(i => i.Date == date && i.Subgroup != Parser.notSub))
                                buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: $"{item.StartTime.ToString()}-{item.EndTime.ToString()} {item.Lecturer?.Split(' ')[0]} {item.Subgroup} {item.IsCompleted}", callbackData: item.StartTime.ToString()) });

                            await botClient.EditMessageReplyMarkupAsync(chatId: message.Chat, messageId: message.MessageId, replyMarkup: (new(buttons.ToArray()) { }));

                        } else if(TimeOnly.TryParse(update.CallbackQuery.Data ?? "", out TimeOnly time)) {

                            var discipline = dbContext.Disciplines.First(i => i.Date == date && i.StartTime == time && i.Subgroup != Parser.notSub);
                            discipline.IsCompleted = !discipline.IsCompleted;

                            dbContext.SaveChanges();
                            await botClient.EditMessageReplyMarkupAsync(chatId: message.Chat, messageId: message.MessageId);
                        }

                    }
                }

            }
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
            return Task.CompletedTask;
        }

    }
}
