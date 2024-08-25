using Core.Bot.Commands.Interfaces;
using Core.Bot.MessagesQueue;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;

namespace Core.Bot.Commands.Student.Other.Profile.Settings.Notifications.Callback {
    public class DaysNotifications : ICallbackCommand {

        public string Command => "DaysNotifications";

        public Mode Mode => Mode.Default;

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args) {
            user.TelegramUserTmp.Mode = Mode.DaysNotifications;

            MessagesQueue.Message.DeleteMessage(chatId: chatId, messageId: messageId);
            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: "Хотите изменить количество дней? Если да, то напишите новое", replyMarkup: Statics.CancelKeyboardMarkup);
            await dbContext.SaveChangesAsync();
        }
    }
}
