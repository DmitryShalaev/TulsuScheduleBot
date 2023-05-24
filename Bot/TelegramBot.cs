using Microsoft.EntityFrameworkCore;

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
        }

        private async Task GroupСhangeMessageMode(ITelegramBotClient botClient, Message message, TelegramUser user) {
            switch(message.Text) {
                case Constants.RK_Cancel:
                    user.Mode = Mode.Default;
                    dbContext.SaveChanges();

                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Основное меню", replyMarkup: MainKeyboardMarkup);
                    break;

                default:
                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Нужно подождать...", replyMarkup: CancelKeyboardMarkup);
                    Parser parser = new(dbContext, false);
                    if(parser.GetDates(message.Text ?? "") is not null) {
                        bool flag = dbContext.ScheduleProfile.Select(i=>i.Group).Contains(message.Text);

                        user.Mode = Mode.Default;
                        user.ScheduleProfile.Group = message.Text;
                        dbContext.SaveChanges();

                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: $"Номер группы успешно изменен на {message.Text} ", replyMarkup: MainKeyboardMarkup);

                        if(!flag)
                            parser.UpdatingDisciplines(message.Text);
                    } else {
                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Сайт ТулГУ не отвечает или такой группы не существует", replyMarkup: CancelKeyboardMarkup);
                    }

                    break;
            }

        }

        private async Task StudentIDСhangeMessageMode(ITelegramBotClient botClient, Message message, TelegramUser user) {
            switch(message.Text) {
                case Constants.RK_Cancel:
                    user.Mode = Mode.Default;
                    dbContext.SaveChanges();

                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Основное меню", replyMarkup: MainKeyboardMarkup);
                    break;

                default:
                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Нужно подождать...", replyMarkup: CancelKeyboardMarkup);

                    if(int.TryParse(message.Text ?? "", out int studentID)) {
                        Parser parser = new(dbContext, false);
                        if(parser.GetProgress(message.Text ?? "") is not null) {
                            bool flag = dbContext.ScheduleProfile.Select(i=>i.StudentID).Contains(message.Text);

                            user.Mode = Mode.Default;
                            user.ScheduleProfile.StudentID = studentID.ToString();
                            dbContext.SaveChanges();

                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: $"Номер зачётки успешно изменен на {message.Text} ", replyMarkup: MainKeyboardMarkup);

                            if(!flag)
                                parser.UpdatingProgress(studentID.ToString());
                        } else {
                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Сайт ТулГУ не отвечает или указан неверный номер зачётки", replyMarkup: CancelKeyboardMarkup);
                        }

                    } else {
                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Не удалось распознать введенный номер зачётной книжки", replyMarkup: CancelKeyboardMarkup);
                    }
                    break;
            }
        }

        private Task HandleError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
