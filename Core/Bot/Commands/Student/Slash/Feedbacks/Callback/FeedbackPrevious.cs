using Core.Bot.Commands;
using Core.Bot.Commands.Interfaces;
using Core.Bot.Commands.Student.Slash.Feedbacks;
using Core.DB;
using Core.DB.Entity;

using Microsoft.EntityFrameworkCore;

using Telegram.Bot.Types;
namespace Core.Bot.New.Commands.Student.Slash.Feedbacks.Callback {
    public class FeedbackPrevious : ICallbackCommand {

        public string Command => "FeedbackPrevious";

        public Mode Mode => Mode.Default;

        public Manager.Check Check => Manager.Check.admin;

        public Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args) {
            Feedback? feedback = dbContext.Feedbacks.Include(i => i.TelegramUser).Where(i => !i.IsCompleted && i.ID < long.Parse(args)).OrderByDescending(i => i.Date).FirstOrDefault();

            if(feedback is not null)
                MessagesQueue.Message.EditMessageText(chatId: chatId, messageId: messageId, text: FeedbackMessage.GetFeedbackMessage(feedback), replyMarkup: DefaultCallback.GetFeedbackInlineKeyboardButton(dbContext, feedback));
            return Task.CompletedTask;
        }
    }
}
