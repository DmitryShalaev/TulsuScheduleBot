using System.Text.RegularExpressions;

using Core.Bot.Commands;
using Core.Bot.Commands.Student;
using Core.DB;
using Core.DB.Entity;

using Microsoft.EntityFrameworkCore;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Core.Bot {
    public class TelegramBot {
        private static TelegramBot? instance;
        public static TelegramBot Instance => instance ??= new TelegramBot();

        public readonly TelegramBotClient botClient;

        public readonly Manager commandManager;

        private TelegramBot() {
            if(string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TelegramBotToken")) ||
               string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TelegramBotConnectionString"))
#if !DEBUG
               || string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TelegramBot_FromEmail")) ||
               string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TelegramBot_ToEmail")) ||
               string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TelegramBot_PassEmail"))
#endif
              ) throw new NullReferenceException("Environment Variable is null");

            using(ScheduleDbContext dbContext = new())
                dbContext.Database.Migrate();

            botClient = new TelegramBotClient(Environment.GetEnvironmentVariable("TelegramBotToken")!);

            Task.Factory.StartNew(Jobs.Job.InitAsync, TaskCreationOptions.LongRunning);

            commandManager = new((string message, TelegramUser user, out string args) => {
                args = "";

                Match match = Statics.DefaultMessageRegex().Match(message);
                if(match.Success)
                    return $"{match.Groups[1]} {user.TelegramUserTmp.Mode}".ToLower();

                match = Statics.TermsMessageRegex().Match(message);
                if(match.Success) {
                    args = match.Groups[1].ToString();
                    return $"{match.Groups[2]} {user.TelegramUserTmp.Mode}".ToLower();
                }

                match = Statics.GroupOrStudentIDMessageRegex().Match(message);
                if(match.Success)
                    return $"{match.Groups[1]} {user.TelegramUserTmp.Mode}".ToLower();

                match = Statics.MarkedMessageRegex().Match(message);
                if(match.Success)
                    return $"{match.Groups[1]} {user.TelegramUserTmp.Mode}".ToLower();

                match = Statics.CommandMessageRegex().Match(message);
                if(match.Success) {
                    args = match.Groups[2].ToString();
                    return $"{match.Groups[1]} {user.TelegramUserTmp.Mode}".ToLower();
                }

                return $"{message} {user.TelegramUserTmp.Mode}".ToLower();

            }, (string message, TelegramUser user, out string args) => {
                args = "";

                Match match = Statics.DisciplineCallbackRegex().Match(message);
                if(match.Success) {
                    args = match.Groups[2].ToString();

                    return $"{match.Groups[1]} {user.TelegramUserTmp.Mode}".ToLower();
                }

                match = Statics.NotificationsCallbackRegex().Match(message);
                if(match.Success) {
                    args = match.Groups[2].ToString();
                    return $"{match.Groups[1]} {user.TelegramUserTmp.Mode}".ToLower();
                }

                match = Statics.WorksCallbackRegex().Match(message);
                if(match.Success) {
                    args = match.Groups[2].ToString();
                    return $"{match.Groups[1]} {user.TelegramUserTmp.Mode}".ToLower();
                }

                match = Statics.TeacherCallbackRegex().Match(message);
                if(match.Success) {
                    args = match.Groups[2].ToString();
                    return $"{match.Groups[1]} {user.TelegramUserTmp.Mode}".ToLower();
                }

                match = Statics.ClassroomCallbackRegex().Match(message);
                if(match.Success) {
                    args = match.Groups[2].ToString();
                    return $"{match.Groups[1]} {user.TelegramUserTmp.Mode}".ToLower();
                }

                return $"{message} {user.TelegramUserTmp.Mode}".ToLower();
            });

            commandManager.InitMessageCommands();
            commandManager.InitCallbackCommands();

            #region Corps
            commandManager.AddMessageCommand(UserCommands.Instance.Message["Corps"], Mode.Default, async (dbContext, chatId, messageId, user, args) => {
                user.TelegramUserTmp.TmpData = UserCommands.Instance.Message["Corps"];
                await dbContext.SaveChangesAsync();
                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: "Выберите корпус, и я покажу где он на карте", replyMarkup: Statics.CorpsKeyboardMarkup);
            });

            foreach(UserCommands.CorpsStruct item in UserCommands.Instance.Corps) {
                commandManager.AddMessageCommand(item.text, Mode.Default, (dbContext, chatId, messageId, user, args) => {
                    if(!string.IsNullOrWhiteSpace(item.map))
                        MessagesQueue.Message.SendTextMessage(chatId: chatId, text: $"[Схема корпуса]({item.map})", parseMode: ParseMode.Markdown);

                    MessagesQueue.Message.SendVenue(chatId: chatId, latitude: item.latitude, longitude: item.longitude, title: item.title, address: item.address, replyMarkup: Statics.CorpsKeyboardMarkup);

                    return Task.CompletedTask;
                });
            }

            commandManager.AddMessageCommand(UserCommands.Instance.College.text, Mode.Default, (dbContext, chatId, messageId, user, args) => {
                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.College.title, replyMarkup: Statics.CancelKeyboardMarkup);

                foreach(UserCommands.CorpsStruct item in UserCommands.Instance.College.corps)
                    MessagesQueue.Message.SendVenue(chatId: chatId, latitude: item.latitude, longitude: item.longitude, title: "", address: item.address, replyMarkup: Statics.CorpsKeyboardMarkup);

                return Task.CompletedTask;
            });
            #endregion

            Console.WriteLine($"Запущен бот {botClient.GetMe().Result.FirstName}\n");
        }

        public async Task UpdateAsync(Update update) {
            string msg = Newtonsoft.Json.JsonConvert.SerializeObject(update) + "\n";
#if DEBUG
            Console.WriteLine(msg);
#endif

            try {
                using(ScheduleDbContext dbContext = new()) {
                    long messageFrom = update.Message?.Chat.Id ??
                                        update.EditedMessage?.Chat.Id ??
                                        update.CallbackQuery?.Message?.Chat.Id ??
                                        update.InlineQuery?.From.Id ??
                                        throw new ArgumentException("messageFrom cannot be null", nameof(update));

                    TelegramUser? user = await dbContext.TelegramUsers.Include(u => u.ScheduleProfile).Include(u => u.Settings).Include(u => u.TelegramUserTmp).FirstOrDefaultAsync(u => u.ChatID == messageFrom);

                    if(user is not null) {
                        user.LastAppeal = user.ScheduleProfile.LastAppeal = DateTime.UtcNow;
                        user.TodayRequests++;
                        user.TotalRequests++;

                        user.IsDeactivated = false;

                        await dbContext.SaveChangesAsync();
                    }

                    Message? message;
                    switch(update.Type) {
                        case UpdateType.Message:
                        case UpdateType.EditedMessage:
                            message = update.Message ?? update.EditedMessage ?? throw new ArgumentException("message cannot be null", nameof(update));

                            if(user is null) {

                                user = new() {
                                    ChatID = messageFrom,
                                    FirstName = "",
                                    ScheduleProfile = new() { OwnerID = messageFrom },
                                    Settings = new() { OwnerID = messageFrom },
                                    TelegramUserTmp = new() { OwnerID = messageFrom },
                                    DateOfRegistration = DateTime.UtcNow
                                };

                                dbContext.TelegramUsers.Add(user);

                                await dbContext.SaveChangesAsync();
                            }

                            switch(message.Type) {
                                case MessageType.Text:

                                    await commandManager.OnMessageAsync(dbContext, message.Chat, message.MessageId, message.Text!, user);
                                    dbContext.MessageLog.Add(new() { Message = message.Text!, TelegramUser = user });

                                    break;

                                case MessageType.Dice:

                                    Message dice = await botClient.SendDice(chatId: message.Chat, emoji: message.Dice!.Emoji);
                                    dbContext.MessageLog.Add(new() { Message = $"{message.Dice!.Emoji} {message.Dice!.Value}", TelegramUser = user });

                                    break;
                            }

                            break;

                        case UpdateType.CallbackQuery:
                            message = update.CallbackQuery?.Message ?? throw new ArgumentException("message cannot be null", nameof(update));

                            if(user is null || update.CallbackQuery?.Data is null || message.Text is null) return;

                            await commandManager.OnCallbackAsync(dbContext, message.Chat, message.MessageId, update.CallbackQuery.Data, message.Text, user);
                            dbContext.MessageLog.Add(new() { Message = update.CallbackQuery.Data, TelegramUser = user });

                            break;

                        case UpdateType.InlineQuery:
                            InlineQuery inlineQuery = update.InlineQuery ?? throw new ArgumentException("inlineQuery cannot be null", nameof(update));

                            if(user is null || string.IsNullOrEmpty(inlineQuery.Query)) return;

                            await InlineQueryMessage.InlineQuery(dbContext, inlineQuery);
                            dbContext.MessageLog.Add(new() { Message = inlineQuery.Query, TelegramUser = user });

                            break;
                    }

                    await dbContext.SaveChangesAsync();

                }
            } catch(Exception e) {
                await ErrorReport.Send(msg, e);
            } finally {
                GC.Collect();
            }
        }
    }
}
