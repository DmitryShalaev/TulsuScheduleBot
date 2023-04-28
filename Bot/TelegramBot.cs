using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace ScheduleBot.Bot {
    public partial class TelegramBot {
        private readonly ITelegramBotClient telegramBot;
        private readonly Scheduler.Scheduler scheduler;
        private readonly ScheduleDbContext dbContext;

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
                if(message.From is null) return;

                user = dbContext.TelegramUsers.FirstOrDefault(u => u.ChatID == message.Chat.Id);

                if(user is null) {
                    user = new() { ChatID = message.Chat.Id, FirstName = message.From.FirstName, Username = message.From.Username, LastName = message.From.LastName };
                    dbContext.TelegramUsers.Add(user);
                    dbContext.SaveChanges();
                }

                switch(user.Mode) {
                    case Mode.Default:
                        switch(update.Type) {
                            case Telegram.Bot.Types.Enums.UpdateType.Message:
                            case Telegram.Bot.Types.Enums.UpdateType.EditedMessage:
                                await MessageModeAsync(message, botClient, user, cancellationToken);
                                break;

                            case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:
                                if(update.CallbackQuery?.Data is null) return;

                                await CallbackModeAsync(message, botClient, user, cancellationToken, update.CallbackQuery.Data);
                                break;
                        }
                        break;

                    case Mode.AddingDiscipline:
                        switch(update.Type) {
                            case Telegram.Bot.Types.Enums.UpdateType.Message:
                            case Telegram.Bot.Types.Enums.UpdateType.EditedMessage:

                                switch(message.Text) {
                                    case Constants.RK_Cancel:
                                        user.Mode = Mode.Default;
                                        dbContext.TemporaryAddition.Remove(dbContext.TemporaryAddition.Where(i => i.TelegramUser == user).OrderByDescending(i => i.AddDate).First());
                                        dbContext.SaveChanges();
                                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Основное меню", replyMarkup: MainKeyboardMarkup);
                                        break;

                                    default:
                                        await SetStagesAddingDisciplineAsync(user, message, botClient);
                                        break;
                                }

                                break;

                            case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:
                                break;
                        }
                        break;
                }
            }
        }

        private Task HandleError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
