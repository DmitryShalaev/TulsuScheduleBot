using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.Bot {
    public partial class TelegramBot {

        private async Task SetStagesAddingDisciplineAsync(ScheduleDbContext dbContext, ITelegramBotClient botClient, ChatId chatId, string message, TelegramUser user) {
            var customDiscipline = dbContext.CustomDiscipline.Where(i => !i.IsAdded && i.ScheduleProfile == user.ScheduleProfile).OrderByDescending(i => i.AddDate).First();

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
                }
            } catch(Exception) {
                await botClient.SendTextMessageAsync(chatId: chatId, text: "Ошибка! " + GetStagesAddingDiscipline(dbContext, user, customDiscipline.Counter), replyMarkup: CancelKeyboardMarkup);
                return;
            }

            dbContext.SaveChanges();

            switch(++customDiscipline.Counter) {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                    await botClient.SendTextMessageAsync(chatId: chatId, text: GetStagesAddingDiscipline(dbContext, user, customDiscipline.Counter), replyMarkup: CancelKeyboardMarkup);
                    break;

                case 5:
                    var endTime = customDiscipline.StartTime?.AddMinutes(95);
                    await botClient.SendTextMessageAsync(chatId: chatId, text: GetStagesAddingDiscipline(dbContext, user, customDiscipline.Counter),
                            replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(text: endTime?.ToString() ?? "endTime Error", callbackData: $"{commands.Callback["SetEndTime"].callback} {endTime}")) { });
                    break;

                case 6:
                    await SaveAddingDisciplineAsync(dbContext, botClient, chatId, user, customDiscipline);
                    break;
            }
        }

        private async Task SaveAddingDisciplineAsync(ScheduleDbContext dbContext, ITelegramBotClient botClient, ChatId chatId, TelegramUser user, CustomDiscipline customDiscipline) {
            customDiscipline.IsAdded = true;
            user.Mode = Mode.Default;
            dbContext.SaveChanges();

            await botClient.SendTextMessageAsync(chatId: chatId, text: GetStagesAddingDiscipline(dbContext, user, customDiscipline.Counter), replyMarkup: MainKeyboardMarkup);
            await botClient.SendTextMessageAsync(chatId: chatId, text: Scheduler.GetScheduleByDate(dbContext, customDiscipline.Date, user.ScheduleProfile), replyMarkup: GetEditAdminInlineKeyboardButton(dbContext, customDiscipline.Date, user.ScheduleProfile));
        }

        private string GetStagesAddingDiscipline(ScheduleDbContext dbContext, TelegramUser user, int? counter = null) {
            if(counter != null)
                return commands.StagesOfAdding[(int)counter];

            return commands.StagesOfAdding[dbContext.CustomDiscipline.Where(i => !i.IsAdded && i.ScheduleProfile == user.ScheduleProfile).OrderByDescending(i => i.AddDate).First().Counter];
        }

        public TimeOnly ParseTime(string timeString) {
            string[] separators = { ":", ";", ".", "," };

            string[] parts = timeString.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            if(parts.Length != 2)
                throw new ArgumentException(timeString);

            int hours, minutes;
            if(!int.TryParse(parts[0], out hours) || !int.TryParse(parts[1], out minutes))
                throw new ArgumentException(timeString);

            return new TimeOnly(hours, minutes);
        }
    }
}