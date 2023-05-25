using Microsoft.EntityFrameworkCore;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;

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
            TelegramUser? user = null;

            switch(update.Type) {
                case Telegram.Bot.Types.Enums.UpdateType.Message:
                case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:
                case Telegram.Bot.Types.Enums.UpdateType.EditedMessage:
                    #region Message
                    Message? message = update.Message ?? update.EditedMessage ?? update.CallbackQuery?.Message;

                    if(message is not null) {
                        if(message.From is null) return;

                        user = dbContext.TelegramUsers.Include(u => u.ScheduleProfile).FirstOrDefault(u => u.ChatID == message.Chat.Id);

                        if(user is null) {
                            ScheduleProfile scheduleProfile = new ScheduleProfile(){ OwnerID = message.Chat.Id };
                            dbContext.ScheduleProfile.Add(scheduleProfile);

                            user = new() { ChatID = message.Chat.Id, FirstName = message.From.FirstName, Username = message.From.Username, LastName = message.From.LastName, ScheduleProfile = scheduleProfile };

                            dbContext.TelegramUsers.Add(user);
                            dbContext.SaveChanges();
                        }

                        switch(user.Mode) {
                            case Mode.Default:
                                switch(update.Type) {
                                    case Telegram.Bot.Types.Enums.UpdateType.Message:
                                    case Telegram.Bot.Types.Enums.UpdateType.EditedMessage:
                                        await DefaultMessageModeAsync(message, botClient, user, cancellationToken);
                                        break;

                                    case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:
                                        if(update.CallbackQuery?.Data is null) return;

                                        await DefaultCallbackModeAsync(message, botClient, user, cancellationToken, update.CallbackQuery.Data);
                                        break;
                                }
                                break;

                            case Mode.AddingDiscipline:
                                switch(update.Type) {
                                    case Telegram.Bot.Types.Enums.UpdateType.Message:
                                    case Telegram.Bot.Types.Enums.UpdateType.EditedMessage:

                                        await AddingDisciplineMessageModeAsync(botClient, message, user);
                                        break;

                                    case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:
                                        if(update.CallbackQuery?.Data is null) return;

                                        await AddingDisciplineCallbackModeAsync(message, botClient, user, cancellationToken, update.CallbackQuery.Data);
                                        break;
                                }
                                break;

                            case Mode.GroupСhange:
                                switch(update.Type) {
                                    case Telegram.Bot.Types.Enums.UpdateType.Message:
                                    case Telegram.Bot.Types.Enums.UpdateType.EditedMessage:
                                        await GroupСhangeMessageMode(botClient, message, user);
                                        break;
                                }
                                break;

                            case Mode.StudentIDСhange:
                                switch(update.Type) {
                                    case Telegram.Bot.Types.Enums.UpdateType.Message:
                                    case Telegram.Bot.Types.Enums.UpdateType.EditedMessage:
                                        await StudentIDСhangeMessageMode(botClient, message, user);
                                        break;
                                }
                                break;
                        }
                    }
                    #endregion
                    break;

                case Telegram.Bot.Types.Enums.UpdateType.InlineQuery:
                    InlineQuery? inlineQuery = update.InlineQuery;

                    if(inlineQuery is not null) {
                        user = dbContext.TelegramUsers.Include(u => u.ScheduleProfile).FirstOrDefault(u => u.ChatID == inlineQuery.From.Id);

                        if(user is not null) {

                            await botClient.AnswerInlineQueryAsync(inlineQuery.Id, new[] {
                                new InlineQueryResultArticle(Constants.RK_Today, Constants.RK_Today, new InputTextMessageContent(scheduler.GetScheduleByDate(DateOnly.FromDateTime(DateTime.Now), user.ScheduleProfile))),
                                new InlineQueryResultArticle(Constants.RK_Tomorrow, Constants.RK_Tomorrow, new InputTextMessageContent(scheduler.GetScheduleByDate(DateOnly.FromDateTime(DateTime.Now.AddDays(1)), user.ScheduleProfile)))
                            });
                        }
                    }
                    break;
            }


        }

        private Task HandleError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
