using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Microsoft.EntityFrameworkCore;

using Telegram.Bot.Types;
namespace Core.Bot.Commands.Admin.Feedbacks.Callback {
    public class FeedbackAccept : ICallbackCommand {

        public string Command => "FeedbackAccept";

        public Mode Mode => Mode.Admin;

        public Manager.Check Check => Manager.Check.admin;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args) {
            Feedback? feedback = await dbContext.Feedbacks.Include(i => i.TelegramUser).FirstOrDefaultAsync(i => i.ID == long.Parse(args));

            if(feedback is not null) {
                feedback.IsCompleted = true;
                await dbContext.SaveChangesAsync();
            }

            feedback = await dbContext.Feedbacks.Include(i => i.TelegramUser).Where(i => !i.IsCompleted).OrderBy(i => i.Date).FirstOrDefaultAsync();
            if(feedback is not null) {

                MessagesQueue.Message.EditMessageText(
                    chatId: chatId,
                    messageId: messageId,
                    text: FeedbackMessage.GetFeedbackMessage(feedback),
                    replyMarkup: FeedbackMessage.GetFeedbackInlineKeyboardButton(dbContext, feedback),
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                    disableWebPagePreview: true);

            } else {
                MessagesQueue.Message.DeleteMessage(chatId: chatId, messageId: messageId);
                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: "Нет новых отзывов и предложений.", replyMarkup: Statics.AdminPanelKeyboardMarkup);
            }
        }
    }
}
