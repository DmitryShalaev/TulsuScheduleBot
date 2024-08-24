using Core.Bot.Commands.Interfaces;

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
                    await BotClient.SendTextMessageAsync(chatId: chatId, text: $"Максимальное количество дней: {maxDaysNotifications}", replyMarkup: Statics.CancelKeyboardMarkup);

                    return;
                }

                user.Settings.NotificationDays = Math.Abs(int.Parse(args));
                user.TelegramUserTmp.Mode = Mode.Default;

                await Statics.DeleteTempMessage(user, messageId);

                await dbContext.SaveChangesAsync();

                await BotClient.SendTextMessageAsync(chatId: chatId, text: "Количество дней успешно изменено.", replyMarkup: DefaultMessage.GetSettingsKeyboardMarkup(user));
                await BotClient.SendTextMessageAsync(chatId: chatId, text: UserCommands.Instance.Message["NotificationSettings"], replyMarkup: DefaultCallback.GetNotificationsInlineKeyboardButton(user));
            } catch(Exception) {
                await BotClient.SendTextMessageAsync(chatId: chatId, text: "Ошибка в формате количества дней!", replyMarkup: Statics.CancelKeyboardMarkup);
            }
        }
    }
}
