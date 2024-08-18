using Core.Bot.Interfaces;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
namespace Core.Bot.Commands.Student.Personal.Profile.Settings.Notifications.Message {
    internal class DaysNotificationsCancel : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => [UserCommands.Instance.Message["Cancel"]];

        public List<Mode> Modes => [Mode.DaysNotifications];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            user.TelegramUserTmp.Mode = Mode.Default;

            await Statics.DeleteTempMessage(user, messageId);
            await dbContext.SaveChangesAsync();

            await BotClient.SendTextMessageAsync(chatId: chatId, text: UserCommands.Instance.Message["Settings"], replyMarkup: DefaultMessage.GetSettingsKeyboardMarkup(user));
            await BotClient.SendTextMessageAsync(chatId: chatId, text: UserCommands.Instance.Message["NotificationSettings"], replyMarkup: DefaultCallback.GetNotificationsInlineKeyboardButton(user));
        }
    }
}
