using Core.Bot.Messages;

using ScheduleBot;
using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Core.Bot.Commands.AddingDiscipline {
    public static class AddingDisciplineMode {

        public static async Task SetStagesAddingDisciplineAsync(ScheduleDbContext dbContext, ITelegramBotClient botClient, ChatId chatId, string message, TelegramUser user) {
            CustomDiscipline customDiscipline = dbContext.CustomDiscipline.Where(i => !i.IsAdded && i.ScheduleProfile == user.ScheduleProfile).OrderByDescending(i => i.AddDate).First();

            try {
                switch(customDiscipline.Counter) {
                    case 0:
                        customDiscipline.Name = message;
                        break;
                    case 1:
                        customDiscipline.Type = message;
                        break;
                    case 2:
                        customDiscipline.Lecturer = message;
                        break;
                    case 3:
                        customDiscipline.LectureHall = message;
                        break;
                    case 4:
                        customDiscipline.StartTime = ParseTime(message);
                        break;
                    case 5:
                        customDiscipline.EndTime = ParseTime(message);
                        break;
                    default:
                        break;
                }
            } catch(Exception) {
                  MessageQueue.SendTextMessage(chatId: chatId, text: "Ошибка! " + GetStagesAddingDiscipline(dbContext, user, customDiscipline.Counter), replyMarkup: Statics.CancelKeyboardMarkup);
                return;
            }

            await dbContext.SaveChangesAsync();

            switch(++customDiscipline.Counter) {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                      MessageQueue.SendTextMessage(chatId: chatId, text: GetStagesAddingDiscipline(dbContext, user, customDiscipline.Counter), replyMarkup: Statics.CancelKeyboardMarkup);
                    break;

                case 5:
                    TimeOnly? endTime = customDiscipline.StartTime?.AddMinutes(95);
                      MessageQueue.SendTextMessage(chatId: chatId, text: GetStagesAddingDiscipline(dbContext, user, customDiscipline.Counter),
                            replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(text: endTime?.ToString() ?? "endTime Error", callbackData: $"{UserCommands.Instance.Callback["SetEndTime"].callback} {endTime}")));
                    break;

                case 6:
                    await SaveAddingDisciplineAsync(dbContext, botClient, chatId, user, customDiscipline);
                    break;
            }

            await dbContext.SaveChangesAsync();
        }

        private static async Task SaveAddingDisciplineAsync(ScheduleDbContext dbContext, ITelegramBotClient botClient, ChatId chatId, TelegramUser user, CustomDiscipline customDiscipline) {
            await DeleteInitialMessage(botClient, chatId, user);

            customDiscipline.IsAdded = true;
            user.TelegramUserTmp.Mode = Mode.Default;

            await dbContext.SaveChangesAsync();

            MessageQueue.SendTextMessage(chatId: chatId, text: GetStagesAddingDiscipline(dbContext, user, customDiscipline.Counter), replyMarkup: Statics.MainKeyboardMarkup);
            MessageQueue.SendTextMessage(chatId: chatId, text: Scheduler.GetScheduleByDate(dbContext, customDiscipline.Date, user, all: true).Item1, replyMarkup: DefaultCallback.GetEditAdminInlineKeyboardButton(dbContext, customDiscipline.Date, user.ScheduleProfile), parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
        }

        public static async Task DeleteInitialMessage(ITelegramBotClient botClient, ChatId chatId, TelegramUser user) {
            try {
                await botClient.DeleteMessageAsync(chatId: chatId, messageId: int.Parse(user.TelegramUserTmp.TmpData!));
            } catch(Exception) { }

            user.TelegramUserTmp.TmpData = null;
        }

        public static string GetStagesAddingDiscipline(ScheduleDbContext dbContext, TelegramUser user, int? counter = null) {
            return counter != null
                ? UserCommands.Instance.StagesOfAdding[(int)counter]
                : UserCommands.Instance.StagesOfAdding[dbContext.CustomDiscipline.Where(i => !i.IsAdded && i.ScheduleProfile == user.ScheduleProfile).OrderByDescending(i => i.AddDate).First().Counter];
        }

        public static TimeOnly ParseTime(string timeString) {
            string[] separators = [":", ";", ".", ",", " "];

            string[] parts = timeString.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            return parts.Length != 2
                ? throw new ArgumentException(timeString)
                : !int.TryParse(parts[0], out int hours) || !int.TryParse(parts[1], out int minutes)
                ? throw new ArgumentException(timeString)
                : new TimeOnly(hours, minutes);
        }
    }
}