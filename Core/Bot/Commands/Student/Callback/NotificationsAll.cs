using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using ScheduleBot;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace Core.Bot.Commands.Student.Callback {

    public class NotificationsAll : ICallbackCommand {

        public string Command => "NotificationsAll";

        public Mode Mode => Mode.Default;

        public Manager.Check Check => Manager.Check.group;

        public Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args) {
            if(DateOnly.TryParse(args, out DateOnly date))
                MessagesQueue.Message.EditMessageText(chatId: chatId, messageId: messageId, text: Scheduler.GetScheduleByDateNotification(dbContext, date, user), parseMode: ParseMode.Html);
            return Task.CompletedTask;
        }
    }
}
