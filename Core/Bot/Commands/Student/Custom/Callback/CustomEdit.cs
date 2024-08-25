using Core.Bot.Commands.Interfaces;
using Core.Bot.MessagesQueue;
using Core.DB;
using Core.DB.Entity;

using ScheduleBot;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace Core.Bot.Commands.Student.Custom.Callback {
    public class CustomEdit : ICallbackCommand {

        public string Command => "CustomEdit";

        public Mode Mode => Mode.Default;

        public Manager.Check Check => Manager.Check.group;

        public Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args) {
            string[] tmp = args.Split('|');
            CustomDiscipline? customDiscipline = dbContext.CustomDiscipline.FirstOrDefault(i => i.ID == uint.Parse(tmp[0]));
            if(customDiscipline is not null) {
                if(user.IsOwner()) {
                    MessagesQueue.Message.EditMessageReplyMarkup(chatId: chatId, messageId: messageId, replyMarkup: DefaultCallback.GetCustomEditAdminInlineKeyboardButton(customDiscipline));
                    return Task.CompletedTask;
                }
            }

            if(DateOnly.TryParse(tmp[1], out DateOnly date)) {
                (string, bool) schedule = Scheduler.GetScheduleByDate(dbContext, date, user);
                MessagesQueue.Message.EditMessageText(chatId: chatId, messageId: messageId, text: schedule.Item1, replyMarkup: DefaultCallback.GetInlineKeyboardButton(date, user, schedule.Item2), parseMode: ParseMode.Markdown, disableWebPagePreview: true);
            }

            return Task.CompletedTask;
        }
    }
}
