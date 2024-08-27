using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;
namespace Core.Bot.Commands.Admin.Back.Message {
    public class AdminPanelBack : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["Back"]];

        public List<Mode> Modes => [Mode.Admin];

        public Manager.Check Check => Manager.Check.admin;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            user.TelegramUserTmp.Mode = Mode.Default;
            user.TelegramUserTmp.TmpData = null;

            await dbContext.SaveChangesAsync();

            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: "Основное меню", replyMarkup: DefaultMessage.GetMainKeyboardMarkup(user));
        }
    }
}
