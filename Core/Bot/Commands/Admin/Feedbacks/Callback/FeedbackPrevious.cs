using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Microsoft.EntityFrameworkCore;

using Telegram.Bot.Types;
namespace Core.Bot.Commands.Admin.Feedbacks.Callback {
    public class FeedbackPrevious : ICallbackCommand {

        public string Command => "FeedbackPrevious";

        public Mode Mode => Mode.Admin;

        public Manager.Check Check => Manager.Check.admin;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args) {
            Feedback? feedback = await dbContext.Feedbacks.Include(i => i.TelegramUser).Where(i => !i.IsCompleted && i.ID < long.Parse(args)).OrderByDescending(i => i.Date).FirstOrDefaultAsync();

            if(feedback is not null)
                MessagesQueue.Message.EditMessageText(
                    chatId: chatId,
                    messageId: messageId,
                    text: FeedbackMessage.GetFeedbackMessage(feedback),
                    replyMarkup: DefaultCallback.GetFeedbackInlineKeyboardButton(dbContext, feedback),
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                    disableWebPagePreview: true);

        }
    }
}
