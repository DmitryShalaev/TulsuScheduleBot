using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace ScheduleBot.Bot {
    public partial class TelegramBot {
        private async Task CustomEdit(Scheduler.Scheduler scheduler, ScheduleDbContext dbContext, ITelegramBotClient botClient, ChatId chatId, int messageId, TelegramUser user, string args, Mode mode, string text) {
            var discipline = dbContext.CustomDiscipline.FirstOrDefault(i => i.ID == uint.Parse(args));
            if(discipline is not null) {
                if(user.IsAdmin()) {
                    user.Mode = mode;
                    user.CurrentPath = $"{discipline.ID}";
                    dbContext.SaveChanges();

                    await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: scheduler.GetScheduleByDate(discipline.Date, user.ScheduleProfile));
                    await botClient.SendTextMessageAsync(chatId: chatId, text: text, replyMarkup: CancelKeyboardMarkup);
                } else {
                    await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: scheduler.GetScheduleByDate(discipline.Date, user.ScheduleProfile), replyMarkup: GetInlineKeyboardButton(discipline.Date, user));
                }
            } else {
                await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: "Редактируемой дисциплины не существует!!!");
            }
        }
    }
}