using Core.Bot.Interfaces;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Core.Bot.Commands.Student.Other.Profile.Settings.Notifications.Callback {
    public class DaysNotifications : ICallbackCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public string Command => "DaysNotifications";

        public Mode Mode => Mode.Default;

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args) {
            user.TelegramUserTmp.Mode = Mode.DaysNotifications;

            await BotClient.DeleteMessageAsync(chatId: chatId, messageId: messageId);
            user.TelegramUserTmp.RequestingMessageID = (await BotClient.SendTextMessageAsync(chatId: chatId, text: "Хотите изменить количество дней? Если да, то напишите новое", replyMarkup: Statics.CancelKeyboardMarkup)).MessageId;
            await dbContext.SaveChangesAsync();
        }
    }
}
