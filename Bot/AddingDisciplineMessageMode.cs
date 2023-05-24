using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.Bot {
    public partial class TelegramBot {

        private async Task AddingDisciplineMessageModeAsync(ITelegramBotClient botClient, Message message, TelegramUser user) {
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
        }

        private async Task SetStagesAddingDisciplineAsync(TelegramUser user, Message message, ITelegramBotClient botClient) {
            var temporaryAddition = dbContext.TemporaryAddition.Where(i => i.TelegramUser == user).OrderByDescending(i => i.AddDate).First();

            try {
                if(message.Text == null) throw new Exception();

                switch(temporaryAddition.Counter) {
                    case 0:
                        temporaryAddition.Name = message.Text;
                        break;
                    case 1:
                        temporaryAddition.Type = message.Text;
                        break;
                    case 2:
                        temporaryAddition.Lecturer = message.Text;
                        break;
                    case 3:
                        temporaryAddition.LectureHall = message.Text;
                        break;
                    case 4:
                        temporaryAddition.StartTime = ParseTime(message.Text);
                        break;
                    case 5:
                        temporaryAddition.EndTime = ParseTime(message.Text);
                        break;
                }
            } catch(Exception) {
                await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Ошибка! " + GetStagesAddingDiscipline(user, temporaryAddition.Counter), replyMarkup: CancelKeyboardMarkup);
                return;
            }

            dbContext.SaveChanges();

            switch(++temporaryAddition.Counter) {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: GetStagesAddingDiscipline(user, temporaryAddition.Counter), replyMarkup: CancelKeyboardMarkup);
                    break;

                case 5:
                    var endTime = temporaryAddition.StartTime?.AddMinutes(95);
                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: GetStagesAddingDiscipline(user, temporaryAddition.Counter),
                            replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(text: endTime?.ToString() ?? "endTime Error", callbackData: $"{Constants.IK_SetEndTime.callback} {endTime}")) { });
                    break;

                case 6:
                    await SaveAddingDisciplineAsync(user, message, botClient, temporaryAddition);
                    break;
            }
        }

        private async Task SaveAddingDisciplineAsync(TelegramUser user, Message message, ITelegramBotClient botClient, TemporaryAddition temporaryAddition) {
            dbContext.CustomDiscipline.Add(new(temporaryAddition, user.ScheduleProfileGuid));
            dbContext.TemporaryAddition.Remove(temporaryAddition);
            user.Mode = Mode.Default;
            dbContext.SaveChanges();

            await botClient.SendTextMessageAsync(chatId: message.Chat, text: GetStagesAddingDiscipline(user, temporaryAddition.Counter), replyMarkup: MainKeyboardMarkup);

            string? group = user.ScheduleProfile.Group;
            if(!string.IsNullOrWhiteSpace(group))
                await botClient.SendTextMessageAsync(chatId: message.Chat, text: scheduler.GetScheduleByDate(temporaryAddition.Date, group, user.ScheduleProfileGuid), replyMarkup: inlineAdminKeyboardMarkup);
        }

        private string GetStagesAddingDiscipline(TelegramUser user, int? counter = null) {
            if(counter != null)
                return Constants.StagesOfAdding[(int)counter];

            return Constants.StagesOfAdding[dbContext.TemporaryAddition.Where(i => i.TelegramUser == user).OrderByDescending(i => i.AddDate).First().Counter];
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