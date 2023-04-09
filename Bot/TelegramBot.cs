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

        private readonly ReplyKeyboardMarkup replyKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { "Сегодня", "Завтра" },

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
                            if(update.Message?.From != null){ 
                                TelegramUser user = new() { ChatId = message.Chat.Id, FirstName = update.Message.From.FirstName, Username = update.Message.From.Username, LastName = update.Message.From.LastName };
                               
                                if(!dbContext.TelegramUsers.ToList().Contains(user)) {
                                    dbContext.TelegramUsers.Add(user);
                                    dbContext.SaveChanges();
                                }
                            }
                            
                            await botClient.SendTextMessageAsync(
                            chatId: message.Chat,
                            text: $"👋 {telegramBot.GetMeAsync(cancellationToken: cancellationToken).Result.Username} 👋",
                            replyMarkup: replyKeyboardMarkup,
                            cancellationToken: cancellationToken);
                            break;

                        case "сегодня":
                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: GetScheduleByDate(DateOnly.FromDateTime(DateTime.Now)), replyMarkup: replyKeyboardMarkup, cancellationToken: cancellationToken);
                            break;

                        case "завтра":
                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: GetScheduleByDate(DateOnly.FromDateTime(DateTime.Now.AddDays(1))), replyMarkup: replyKeyboardMarkup, cancellationToken: cancellationToken);
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
