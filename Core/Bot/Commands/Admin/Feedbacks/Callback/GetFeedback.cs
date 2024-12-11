using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;
namespace Core.Bot.Commands.Admin.Feedbacks.Callback {
    public class GetFeedback : ICallbackCommand {

        public string Command => "GetFeedback";

        public Mode Mode => Mode.Default;

        public Manager.Check Check => Manager.Check.admin;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args) {
            user.TelegramUserTmp.Mode = Mode.Admin;
            dbContext.SaveChanges();
            MessagesQueue.Message.EditMessageReplyMarkup(chatId: chatId, messageId: messageId, replyMarkup: null);
            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["AdminPanel"], replyMarkup: Statics.AdminPanelKeyboardMarkup);

            await FeedbackMessage.GetFeedback(dbContext, chatId);
        }
    }
}
