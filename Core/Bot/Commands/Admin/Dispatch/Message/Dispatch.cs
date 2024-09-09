using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;

namespace Core.Bot.Commands.Admin.Dispatch.Message {
    public class Dispatch : IMessageCommand {

        public List<string> Commands => ["Рассылка"];

        public List<Mode> Modes => [Mode.Admin];

        public Manager.Check Check => Manager.Check.admin;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            user.TelegramUserTmp.Mode = Mode.Dispatch;
            user.TelegramUserTmp.TmpData = null;

            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: "Сообщение для массовой рассылки:", replyMarkup: Statics.CancelKeyboardMarkup);

            await dbContext.SaveChangesAsync();

        }
    }
}
