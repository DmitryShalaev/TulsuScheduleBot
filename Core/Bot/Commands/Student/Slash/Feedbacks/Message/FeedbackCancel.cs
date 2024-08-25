using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;
namespace Core.Bot.Commands.Student.Slash.Feedbacks.Message {
    public class FeedbackCancel : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["Cancel"]];

        public List<Mode> Modes => [Mode.Feedback];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            user.TelegramUserTmp.Mode = Mode.Default;

            await dbContext.SaveChangesAsync();

            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["MainMenu"], replyMarkup: DefaultMessage.GetMainKeyboardMarkup(user));
        }
    }
}
