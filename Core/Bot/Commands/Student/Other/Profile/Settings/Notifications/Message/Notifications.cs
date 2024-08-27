using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;
namespace Core.Bot.Commands.Student.Other.Profile.Settings.Notifications.Message {
    internal class Notifications : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["Notifications"], "/notifications"];

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.none;

        public Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["NotificationSettings"], replyMarkup: DefaultCallback.GetNotificationsInlineKeyboardButton(user), saveMessageId: true);
            return Task.CompletedTask;
        }
    }
}
