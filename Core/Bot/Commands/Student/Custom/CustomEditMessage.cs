using ScheduleBot;
using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Core.Bot.Commands.Student.Custom {
    public static class CustomEditMessage {
        public static async Task CustomEdit(ScheduleDbContext dbContext, ITelegramBotClient botClient, ChatId chatId, int messageId, TelegramUser user, string args, Mode mode, string text) {
            string[] tmp = args.Split('|');
            CustomDiscipline? discipline = dbContext.CustomDiscipline.FirstOrDefault(i => i.ID == uint.Parse(tmp[0]));
            if(discipline is not null) {
                if(user.IsOwner()) {
                    user.Mode = mode;
                    user.TempData = $"{discipline.ID}";

                    await botClient.DeleteMessageAsync(chatId: chatId, messageId: messageId);
                    user.RequestingMessageID = (await botClient.SendTextMessageAsync(chatId: chatId, text: text, replyMarkup: Statics.CancelKeyboardMarkup)).MessageId;

                    await dbContext.SaveChangesAsync();
                    return;
                }
            }

            if(DateOnly.TryParse(tmp[1], out DateOnly date)) {
                (string, bool) schedule = Scheduler.GetScheduleByDate(dbContext, date, user, all: true);
                await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: schedule.Item1, replyMarkup: DefaultCallback.GetInlineKeyboardButton(date, user, schedule.Item2));
            }
        }
    }
}