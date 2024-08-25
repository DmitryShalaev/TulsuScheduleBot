using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;
namespace Core.Bot.Commands.Student.Other.Profile.Settings.GroupList.Message {
    internal class DisplayingGroupList : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["DisplayingGroupList"]];

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            user.Settings.DisplayingGroupList = !user.Settings.DisplayingGroupList;

            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["Settings"], replyMarkup: DefaultMessage.GetSettingsKeyboardMarkup(user));

            await dbContext.SaveChangesAsync();
        }
    }
}
