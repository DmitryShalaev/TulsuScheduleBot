using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;
namespace Core.Bot.Commands.Student.Other.Profile.Settings.Notifications.Message {
    internal class Notifications : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["Notifications"], "/notifications"];

        public List<Mode> Modes => Enum.GetValues(typeof(Mode)).Cast<Mode>().ToList();

        public Manager.Check Check => Manager.Check.none;

        public Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            if(user.TelegramUserTmp.Mode == Mode.AddingDiscipline)
                dbContext.CustomDiscipline.RemoveRange(dbContext.CustomDiscipline.Where(i => !i.IsAdded && i.ScheduleProfile == user.ScheduleProfile));

            user.TelegramUserTmp.TmpData = UserCommands.Instance.Message["Settings"];
            user.TelegramUserTmp.Mode = Mode.Default;

            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["NotificationSettings"], replyMarkup: DefaultCallback.GetNotificationsInlineKeyboardButton(user), saveMessageId: true);
            return Task.CompletedTask;
        }
    }
}

