using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;
namespace Core.Bot.Commands.Student.Other.Profile.Settings.Notifications.Message {
    internal class DaysNotifications : IMessageCommand {

        public List<string>? Commands => null;

        public List<Mode> Modes => [Mode.DaysNotifications];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            try {
                int _days = Math.Abs(int.Parse(args));

                int maxDaysNotifications = UserCommands.Instance.Config.MaxDaysNotifications;

                if(!user.IsAdmin && _days > maxDaysNotifications) {
                    MessagesQueue.Message.SendTextMessage(chatId: chatId, text: $"Максимальное количество дней: {maxDaysNotifications}", replyMarkup: Statics.CancelKeyboardMarkup);

                    return;
                }

                user.Settings.NotificationDays = Math.Abs(int.Parse(args));
                user.TelegramUserTmp.Mode = Mode.Default;
                user.TelegramUserTmp.TmpData = UserCommands.Instance.Message["Settings"];

                await dbContext.SaveChangesAsync();

                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: "Количество дней успешно изменено.", replyMarkup: DefaultMessage.GetSettingsKeyboardMarkup(user));
                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["NotificationSettings"], replyMarkup: DefaultCallback.GetNotificationsInlineKeyboardButton(user), saveMessageId: true);
            } catch(Exception) {
                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: "Ошибка в формате количества дней!", replyMarkup: Statics.CancelKeyboardMarkup);
            }
        }
    }
}
