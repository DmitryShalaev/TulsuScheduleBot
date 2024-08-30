using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;

namespace Core.Bot.Commands.Messenger.Back {
    public class MessengerBack : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["Cancel"]];

        public List<Mode> Modes => [Mode.Messenger];

        public Manager.Check Check => Manager.Check.none;

        private readonly static Dictionary<string, string> commands = UserCommands.Instance.Message;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {

            if(!string.IsNullOrEmpty(user.TelegramUserTmp.TmpData) && user.TelegramUserTmp.TmpData.Contains("FeedbackReply")) {
                user.TelegramUserTmp.TmpData = null;
                user.TelegramUserTmp.Mode = Mode.Admin;
                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["AdminPanel"], replyMarkup: Statics.AdminPanelKeyboardMarkup);
            } else {
                user.TelegramUserTmp.TmpData = null;
                user.TelegramUserTmp.Mode = Mode.Default;
                MessagesQueue.Message.SendTextMessage(chatId, commands["MainMenu"], replyMarkup: DefaultMessage.GetMainKeyboardMarkup(user));
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
