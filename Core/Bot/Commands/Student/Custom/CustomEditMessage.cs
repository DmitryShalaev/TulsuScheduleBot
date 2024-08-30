using Core.DB;
using Core.DB.Entity;

using Microsoft.EntityFrameworkCore;

using ScheduleBot;

using Telegram.Bot.Types;

namespace Core.Bot.Commands.Student.Custom {
    public static class CustomEditMessage {
        public static async Task CustomEdit(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args, Mode mode, string text) {
            string[] tmp = args.Split('|');
            CustomDiscipline? discipline = await dbContext.CustomDiscipline.FirstOrDefaultAsync(i => i.ID == uint.Parse(tmp[0]));
            if(discipline is not null) {
                if(user.IsOwner() && !user.IsSupergroup()) {
                    user.TelegramUserTmp.Mode = mode;
                    user.TelegramUserTmp.TmpData = $"{discipline.ID}";

                    MessagesQueue.Message.DeleteMessage(chatId: chatId, messageId: messageId);
                    MessagesQueue.Message.SendTextMessage(chatId: chatId, text: text, replyMarkup: Statics.CancelKeyboardMarkup);

                    await dbContext.SaveChangesAsync();
                    return;
                }
            }

            if(DateOnly.TryParse(tmp[1], out DateOnly date)) {
                (string, bool) schedule = Scheduler.GetScheduleByDate(dbContext, date, user, all: true);
                MessagesQueue.Message.EditMessageText(chatId: chatId, messageId: messageId, text: schedule.Item1, replyMarkup: DefaultCallback.GetInlineKeyboardButton(date, user, schedule.Item2));
            }
        }
    }
}