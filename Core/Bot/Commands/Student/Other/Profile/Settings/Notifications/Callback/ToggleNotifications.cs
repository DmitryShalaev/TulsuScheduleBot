using Core.Bot.Interfaces;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
namespace Core.Bot.Commands.Student.Other.Profile.Settings.Notifications.Callback {
    public class ToggleNotifications : ICallbackCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

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

            await BotClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: UserCommands.Instance.Message["NotificationSettings"], replyMarkup: DefaultCallback.GetNotificationsInlineKeyboardButton(user));
        }
    }
}
