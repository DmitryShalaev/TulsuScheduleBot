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

        private bool addFlag = false;

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
                if(update.Type == Telegram.Bot.Types.Enums.UpdateType.Message || update.Type == Telegram.Bot.Types.Enums.UpdateType.EditedMessage) {
                    if(update.Message?.From == null) return;

                    user = dbContext.TelegramUsers.FirstOrDefault(u => u.ChatId == message.Chat.Id);
                    if(user is null) {
                        user = new() { ChatId = message.Chat.Id, FirstName = update.Message.From.FirstName, Username = update.Message.From.Username, LastName = update.Message.From.LastName };
                        dbContext.TelegramUsers.Add(user);
                        dbContext.SaveChanges();
                    }

                    switch(message.Text?.ToLower()) {
                        case "/start":
                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: $"👋 {telegramBot.GetMeAsync(cancellationToken: cancellationToken).Result.Username} 👋", replyMarkup: MainKeyboardMarkup, cancellationToken: cancellationToken);
                            break;

                        case "по дням":
                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: "По дням", replyMarkup: DaysKeyboardMarkup, cancellationToken: cancellationToken);
                            break;

                        case "назад":
                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Основное меню", replyMarkup: MainKeyboardMarkup, cancellationToken: cancellationToken);
                            break;

                        case "сегодня":
                        case "завтра":
                        case "понедельник":
                        case "вторник":
                        case "среда":
                        case "четверг":
                        case "пятница":
                        case "суббота":
                        case "эта неделя":
                        case "следующая неделя":
                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: $"Расписание актуально на {Parser.lastUpdate.ToString("dd.MM.yyyy HH:mm")}", replyMarkup: MainKeyboardMarkup, cancellationToken: cancellationToken);
                            switch(message.Text?.ToLower()) {
                                case "сегодня":
                                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: scheduler.GetScheduleByDate(DateOnly.FromDateTime(DateTime.Now)), replyMarkup: MainKeyboardMarkup, cancellationToken: cancellationToken);

                                    break;
                                case "завтра":
                                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: scheduler.GetScheduleByDate(DateOnly.FromDateTime(DateTime.Now.AddDays(1))), replyMarkup: MainKeyboardMarkup, cancellationToken: cancellationToken);

                                    break;
                                case "понедельник":
                                    var monday = scheduler.GetScheduleByDay(DayOfWeek.Monday);
                                    foreach(var day in monday)
                                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: day, replyMarkup: DaysKeyboardMarkup, cancellationToken: cancellationToken);

                                    break;
                                case "вторник":
                                    var tuesday = scheduler.GetScheduleByDay(DayOfWeek.Tuesday);
                                    foreach(var day in tuesday)
                                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: day, replyMarkup: DaysKeyboardMarkup, cancellationToken: cancellationToken);

                                    break;
                                case "среда":
                                    var wednesday = scheduler.GetScheduleByDay(DayOfWeek.Wednesday);
                                    foreach(var day in wednesday)
                                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: day, replyMarkup: DaysKeyboardMarkup, cancellationToken: cancellationToken);

                                    break;
                                case "четверг":
                                    var thursday = scheduler.GetScheduleByDay(DayOfWeek.Thursday);
                                    foreach(var day in thursday)
                                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: day, replyMarkup: DaysKeyboardMarkup, cancellationToken: cancellationToken);

                                    break;
                                case "пятница":
                                    var friday = scheduler.GetScheduleByDay(DayOfWeek.Friday);
                                    foreach(var day in friday)
                                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: day, replyMarkup: DaysKeyboardMarkup, cancellationToken: cancellationToken);

                                    break;
                                case "суббота":
                                    var saturday = scheduler.GetScheduleByDay(DayOfWeek.Saturday);
                                    foreach(var day in saturday)
                                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: day, replyMarkup: DaysKeyboardMarkup, cancellationToken: cancellationToken);

                                    break;
                                case "эта неделя":
                                    var thisWeek = scheduler.GetScheduleByWeak(CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday) - 1);
                                    foreach(var item in thisWeek)
                                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: item, replyMarkup: WeekKeyboardMarkup, cancellationToken: cancellationToken);

                                    break;
                                case "следующая неделя":
                                    var nextWeek = scheduler.GetScheduleByWeak(CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday));
                                    foreach(var item in nextWeek)
                                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: item, replyMarkup: WeekKeyboardMarkup, cancellationToken: cancellationToken);

                                    break;
                            }
                            break;

                        case "на неделю":
                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: "На неделю", replyMarkup: WeekKeyboardMarkup, cancellationToken: cancellationToken);
                            break;
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
