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
        private readonly ScheduleDbContext dbContext;

        private readonly ReplyKeyboardMarkup MainKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { "Сегодня", "Завтра" },
                            new KeyboardButton[] { "По дням", },

                        })
        { ResizeKeyboard = true };

        private readonly ReplyKeyboardMarkup DaysKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { "Понедельник", "Вторник" },
                            new KeyboardButton[] { "Среда", "Четверг" },
                            new KeyboardButton[] { "Пятница", "Суббота" },
                            new KeyboardButton[] { "Назад", },

                        })
        { ResizeKeyboard = true };


        public TelegramBot(ScheduleDbContext dbContext, string token = "5588441792:AAFsoUQdu5_hd9Ccz34ZVNPhK9d5Z7Nx9VM") {
            this.dbContext = dbContext;

            telegramBot = new TelegramBotClient(token);

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

            if(message != null) {
                if(update.Type == Telegram.Bot.Types.Enums.UpdateType.Message || update.Type == Telegram.Bot.Types.Enums.UpdateType.EditedMessage) {
                    switch(message.Text?.ToLower()) {
                        case "/start":
                            if(update.Message?.From != null) {
                                TelegramUser user = new() { ChatId = message.Chat.Id, FirstName = update.Message.From.FirstName, Username = update.Message.From.Username, LastName = update.Message.From.LastName };

                                if(!dbContext.TelegramUsers.ToList().Contains(user)) {
                                    dbContext.TelegramUsers.Add(user);
                                    dbContext.SaveChanges();
                                }
                            }

                            await botClient.SendTextMessageAsync(
                            chatId: message.Chat,
                            text: $"👋 {telegramBot.GetMeAsync(cancellationToken: cancellationToken).Result.Username} 👋",
                            replyMarkup: MainKeyboardMarkup,
                            cancellationToken: cancellationToken);
                            break;

                        case "сегодня":
                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: GetScheduleByDate(DateOnly.FromDateTime(DateTime.Now)), replyMarkup: MainKeyboardMarkup, cancellationToken: cancellationToken);
                            break;

                        case "завтра":
                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: GetScheduleByDate(DateOnly.FromDateTime(DateTime.Now.AddDays(1))), replyMarkup: MainKeyboardMarkup, cancellationToken: cancellationToken);
                            break;

                        case "по дням":
                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: "По дням", replyMarkup: DaysKeyboardMarkup, cancellationToken: cancellationToken);
                            break;

                        case "назад":
                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Основное меню", replyMarkup: MainKeyboardMarkup, cancellationToken: cancellationToken);
                            break;

                        case "понедельник":
                            await GetScheduleByDayAsync(botClient, message, cancellationToken, DayOfWeek.Monday);
                            break;

                        case "вторник":
                            await GetScheduleByDayAsync(botClient, message, cancellationToken, DayOfWeek.Tuesday);
                            break;

                        case "среда":
                            await GetScheduleByDayAsync(botClient, message, cancellationToken, DayOfWeek.Wednesday);
                            break;

                        case "четверг":
                            await GetScheduleByDayAsync(botClient, message, cancellationToken, DayOfWeek.Thursday);
                            break;

                        case "пятница":
                            await GetScheduleByDayAsync(botClient, message, cancellationToken, DayOfWeek.Friday);
                            break;

                        case "суббота":
                            await GetScheduleByDayAsync(botClient, message, cancellationToken, DayOfWeek.Saturday);
                            break;
                    }
                }
            }
        }

        private async Task GetScheduleByDayAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, DayOfWeek dayOfWeek) {
            for(int i = -1; i < 2; i++) {
                int weeks = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                await botClient.SendTextMessageAsync(chatId: message.Chat, text: GetScheduleByDate(DateOnly.FromDateTime(new DateTime(DateTime.Now.Year, 1, 1).AddDays(7 * (weeks + i) + (byte)dayOfWeek))),
                replyMarkup: DaysKeyboardMarkup, cancellationToken: cancellationToken);
            }
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
            return Task.CompletedTask;
        }

    }
}
