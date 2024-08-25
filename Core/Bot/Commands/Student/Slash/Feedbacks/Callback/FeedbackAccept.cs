using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Microsoft.EntityFrameworkCore;

using Telegram.Bot.Types;
namespace Core.Bot.Commands.Student.Slash.Feedbacks.Callback {
    public class FeedbackAccept : ICallbackCommand {

        public string Command => "FeedbackAccept";

        public Mode Mode => Mode.Default;

        public Manager.Check Check => Manager.Check.admin;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args) {
            Feedback? feedback = dbContext.Feedbacks.Include(i => i.TelegramUser).FirstOrDefault(i => i.ID == long.Parse(args));

            if(feedback is not null) {
                feedback.IsCompleted = true;
                await dbContext.SaveChangesAsync();
            }

            feedback = dbContext.Feedbacks.Include(i => i.TelegramUser).Where(i => !i.IsCompleted).OrderBy(i => i.Date).FirstOrDefault();
            if(feedback is not null) MessagesQueue.Message.EditMessageText(chatId: chatId, messageId: messageId, text: FeedbackMessage.GetFeedbackMessage(feedback), replyMarkup: DefaultCallback.GetFeedbackInlineKeyboardButton(dbContext, feedback));
            else {
                MessagesQueue.Message.DeleteMessage(chatId: chatId, messageId: messageId);
                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: "Нет новых отзывов и предложений.", replyMarkup: Statics.MainKeyboardMarkup);
            }
        }
    }
}
