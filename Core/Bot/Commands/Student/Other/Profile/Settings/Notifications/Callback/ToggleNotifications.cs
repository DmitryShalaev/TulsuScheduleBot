using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;
namespace Core.Bot.Commands.Student.Other.Profile.Settings.Notifications.Callback {
    public class ToggleNotifications : ICallbackCommand {

        public string Command => "ToggleNotifications";

        public Mode Mode => Mode.Default;

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args) {
            switch(args) {
                case "on":
                    user.Settings.NotificationEnabled = true;
                    break;
                case "off":
                    user.Settings.NotificationEnabled = false;
                    break;
            }

            await dbContext.SaveChangesAsync();

            MessagesQueue.Message.DeleteMessage(chatId: chatId, messageId: messageId);
            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["Settings"], replyMarkup: DefaultMessage.GetSettingsKeyboardMarkup(user));
            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["NotificationSettings"], replyMarkup: DefaultCallback.GetNotificationsInlineKeyboardButton(user), saveMessageId: true);
        }
    }
}
