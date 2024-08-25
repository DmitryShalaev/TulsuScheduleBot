using Core.Bot.Commands.Interfaces;
using Core.Bot.MessagesQueue;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;
namespace Core.Bot.Commands.Student.Other.Profile.Message {
    internal class ProfileCancel : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["Cancel"]];

        public List<Mode> Modes => [Mode.GroupСhange, Mode.StudentIDСhange, Mode.ResetProfileLink];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            user.TelegramUserTmp.Mode = Mode.Default;

            await dbContext.SaveChangesAsync();

            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["Profile"], replyMarkup: DefaultMessage.GetProfileKeyboardMarkup(user));
        }
    }
}
