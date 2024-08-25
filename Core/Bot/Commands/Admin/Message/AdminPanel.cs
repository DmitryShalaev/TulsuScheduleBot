using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;

namespace Core.Bot.Commands.Admin.Message {
    public class AdminPanel : IMessageCommand {

        public List<string> Commands => [UserCommands.Instance.Message["AdminPanel"]];

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.admin;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            user.TelegramUserTmp.Mode = Mode.Admin;
            await dbContext.SaveChangesAsync();

            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["AdminPanel"], replyMarkup: Statics.AdminPanelKeyboardMarkup);
        }
    }
}
