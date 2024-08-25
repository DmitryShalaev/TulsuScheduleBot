using Core.Bot.Commands.Interfaces;
using Core.Bot.MessagesQueue;
using Core.DB;
using Core.DB.Entity;

using ScheduleBot;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace Core.Bot.Commands.Student.Callback {
    public class Edit : ICallbackCommand {

        public string Command => UserCommands.Instance.Callback["Edit"].callback;

        public Mode Mode => Mode.Default;

        public Manager.Check Check => Manager.Check.group;

        public Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args) {
            if(DateOnly.TryParse(args, out DateOnly date)) {
                if(user.IsOwner())
                    MessagesQueue.Message.EditMessageText(chatId: chatId, messageId: messageId, text: Scheduler.GetScheduleByDate(dbContext, date, user, true).Item1, replyMarkup: DefaultCallback.GetEditAdminInlineKeyboardButton(dbContext, date, user.ScheduleProfile), parseMode: ParseMode.Markdown, disableWebPagePreview: true);
                else {
                    (string, bool) schedule = Scheduler.GetScheduleByDate(dbContext, date, user);
                    MessagesQueue.Message.EditMessageText(chatId: chatId, messageId: messageId, text: schedule.Item1, replyMarkup: DefaultCallback.GetInlineKeyboardButton(date, user, schedule.Item2), parseMode: ParseMode.Markdown, disableWebPagePreview: true);
                }
            }

            return Task.CompletedTask;
        }
    }
}
