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

        private readonly Manager commandManager;

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

                if(Statics.DefaultMessageRegex().IsMatch(message))
                    return $"{message} {user.TelegramUserTmp.Mode}".ToLower();

                Match match = Statics.TermsMessageRegex().Match(message);
                if(match.Success) {
                    args = match.Groups[1].ToString();
                    return $"{match.Groups[2]} {user.TelegramUserTmp.Mode}".ToLower();
                }

                match = Statics.GroupOrStudentIDMessageRegex().Match(message);
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
                commandManager.AddMessageCommand(item.text, Mode.Default, async (dbContext, chatId, messageId, user, args) => {
                    if(!string.IsNullOrWhiteSpace(item.map))
                        MessagesQueue.Message.SendTextMessage(chatId: chatId, text: $"[Схема корпуса]({item.map})", parseMode: ParseMode.Markdown, disableWebPagePreview: true);

                    await botClient.SendVenueAsync(chatId: chatId, latitude: item.latitude, longitude: item.longitude, title: item.title, address: item.address, replyMarkup: Statics.CorpsKeyboardMarkup);
                });
            }

            commandManager.AddMessageCommand(UserCommands.Instance.College.text, Mode.Default, async (dbContext, chatId, messageId, user, args) => {
                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.College.title, replyMarkup: Statics.CancelKeyboardMarkup);

                foreach(UserCommands.CorpsStruct item in UserCommands.Instance.College.corps)
                    await botClient.SendVenueAsync(chatId: chatId, latitude: item.latitude, longitude: item.longitude, title: "", address: item.address, replyMarkup: Statics.CorpsKeyboardMarkup);
            });
            #endregion

            Console.WriteLine("Запущен бот " + botClient.GetMeAsync().Result.FirstName + "\n");
        }

        public async Task UpdateAsync(Update update) {
            string msg = Newtonsoft.Json.JsonConvert.SerializeObject(update) + "\n";
#if DEBUG
            Console.WriteLine(msg);
#endif

            try {
                using(ScheduleDbContext dbContext = new()) {
                    Message? message = update.Message ?? update.EditedMessage ?? update.CallbackQuery?.Message;

                    if(message is not null) {
                        if(message.From is null) return;

                        TelegramUser? user = await dbContext.TelegramUsers.Include(u => u.ScheduleProfile).Include(u => u.Settings).Include(u => u.TelegramUserTmp).FirstOrDefaultAsync(u => u.ChatID == message.Chat.Id);

                        if(user is null) {

                            user = new() {
                                ChatID = message.Chat.Id,
                                Username = message.From.Username,
                                FirstName = message.From.FirstName,
                                LastName = message.From.LastName,
                                ScheduleProfile = new() { OwnerID = message.Chat.Id },
                                Settings = new() { OwnerID = message.Chat.Id },
                                TelegramUserTmp = new() { OwnerID = message.Chat.Id },
                                DateOfRegistration = DateTime.UtcNow
                            };

                            dbContext.TelegramUsers.Add(user);

                            await dbContext.SaveChangesAsync();
                        }

                        user.LastAppeal = user.ScheduleProfile.LastAppeal = DateTime.UtcNow;
                        user.TodayRequests++;
                        user.TotalRequests++;

                        await dbContext.SaveChangesAsync();

                        switch(update.Type) {
                            case UpdateType.Message:
                            case UpdateType.EditedMessage:
                                if(message.Text is null) return;

                                user.Username = message.From.Username;
                                user.FirstName = message.From.FirstName;
                                user.LastName = message.From.LastName;

                                await commandManager.OnMessageAsync(dbContext, message.Chat, message.MessageId, message.Text, user);
                                dbContext.MessageLog.Add(new() { Message = message.Text, TelegramUser = user });
                                break;

                            case UpdateType.CallbackQuery:
                                if(update.CallbackQuery?.Data is null || message.Text is null) return;

                                await commandManager.OnCallbackAsync(dbContext, message.Chat, message.MessageId, update.CallbackQuery.Data, message.Text, user);
                                dbContext.MessageLog.Add(new() { Message = update.CallbackQuery.Data, TelegramUser = user });
                                break;
                        }

                        await dbContext.SaveChangesAsync();

                    } else {
                        if(update.Type == UpdateType.InlineQuery) {
                            await InlineQueryMessage.InlineQuery(dbContext, update);
                            return;
                        }
                    }
                }
            } catch(Exception e) {
                await ErrorReport.Send(msg, e);
            } finally {
                GC.Collect();
            }
        }
    }
}
