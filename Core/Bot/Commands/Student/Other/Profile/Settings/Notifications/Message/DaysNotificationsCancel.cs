using Core.Bot.Commands.Interfaces;
using Core.Bot.MessagesQueue;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;
namespace Core.Bot.Commands.Student.Other.Profile.Settings.Notifications.Message {
    internal class DaysNotificationsCancel : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["Cancel"]];

        public List<Mode> Modes => [Mode.DaysNotifications];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            user.TelegramUserTmp.Mode = Mode.Default;

            await dbContext.SaveChangesAsync();

            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["Settings"], replyMarkup: DefaultMessage.GetSettingsKeyboardMarkup(user));
            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["NotificationSettings"], replyMarkup: DefaultCallback.GetNotificationsInlineKeyboardButton(user));
        }
    }
}
