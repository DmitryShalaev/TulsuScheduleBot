using Core.Bot.Commands.Interfaces;
using Core.Bot.Messages;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
namespace Core.Bot.Commands.Student.Other.Profile.Settings.Notifications.Message {
    internal class DaysNotifications : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => null;

        public List<Mode> Modes => [Mode.DaysNotifications];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            try {
                int _days = Math.Abs(int.Parse(args));

                int maxDaysNotifications = UserCommands.Instance.Config.MaxDaysNotifications;

                if(!user.IsAdmin && _days > maxDaysNotifications) {
                    MessageQueue.SendTextMessage(chatId: chatId, text: $"Максимальное количество дней: {maxDaysNotifications}", replyMarkup: Statics.CancelKeyboardMarkup);

                    return;
                }

                user.Settings.NotificationDays = Math.Abs(int.Parse(args));
                user.TelegramUserTmp.Mode = Mode.Default;

                await dbContext.SaveChangesAsync();

                MessageQueue.SendTextMessage(chatId: chatId, text: "Количество дней успешно изменено.", replyMarkup: DefaultMessage.GetSettingsKeyboardMarkup(user));
                MessageQueue.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["NotificationSettings"], replyMarkup: DefaultCallback.GetNotificationsInlineKeyboardButton(user));
            } catch(Exception) {
                MessageQueue.SendTextMessage(chatId: chatId, text: "Ошибка в формате количества дней!", replyMarkup: Statics.CancelKeyboardMarkup);
            }
        }
    }
}
