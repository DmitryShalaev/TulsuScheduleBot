using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace ScheduleBot.Bot {
    public partial class TelegramBot {
        private async Task CustomEdit(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args, Mode mode, string text) {
            string[] tmp = args.Split('|');
            CustomDiscipline? discipline = dbContext.CustomDiscipline.FirstOrDefault(i => i.ID == uint.Parse(tmp[0]));
            if(discipline is not null) {
                if(user.IsOwner()) {
                    user.Mode = mode;
                    user.TempData = $"{discipline.ID}";

                    await botClient.DeleteMessageAsync(chatId: chatId, messageId: messageId);
                    user.RequestingMessageID = (await botClient.SendTextMessageAsync(chatId: chatId, text: text, replyMarkup: CancelKeyboardMarkup)).MessageId;

                    await dbContext.SaveChangesAsync();
                    return;
                }
            }

            if(DateOnly.TryParse(tmp[1], out DateOnly date)) {
                (string, bool) schedule = Scheduler.GetScheduleByDate(dbContext, date, user.ScheduleProfile);
                await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: schedule.Item1, replyMarkup: GetInlineKeyboardButton(date, user, schedule.Item2));
            }
        }
    }
}