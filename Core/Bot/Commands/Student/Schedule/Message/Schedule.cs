using Core.Bot.Commands.Interfaces;
using Core.Bot.MessagesQueue;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;
namespace Core.Bot.Commands.Student.Schedule.Message {
    internal class Schedule : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["Schedule"]];

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            user.TelegramUserTmp.TmpData = UserCommands.Instance.Message["Schedule"];
            await dbContext.SaveChangesAsync();

            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["Schedule"], replyMarkup: Statics.ScheduleKeyboardMarkup);
        }
    }
}
