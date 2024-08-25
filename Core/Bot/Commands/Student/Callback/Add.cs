using Core.Bot.Commands.AddingDiscipline;
using Core.Bot.Commands.Interfaces;
using Core.Bot.MessagesQueue;
using Core.DB;
using Core.DB.Entity;

using ScheduleBot;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Core.Bot.Commands.Student.Callback {
    public class Add : ICallbackCommand {

        public string Command => UserCommands.Instance.Callback["Add"].callback;

        public Mode Mode => Mode.Default;

        public Manager.Check Check => Manager.Check.group;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args) {
            if(DateOnly.TryParse(args, out DateOnly date)) {
                if(user.IsOwner()) {
                    user.TelegramUserTmp.Mode = Mode.AddingDiscipline;
                    user.TelegramUserTmp.TmpData = $"{messageId}";
                    dbContext.CustomDiscipline.Add(new(user.ScheduleProfile, date));
                    await dbContext.SaveChangesAsync();

                    MessagesQueue.Message.EditMessageText(chatId: chatId, messageId: messageId, text: Scheduler.GetScheduleByDate(dbContext, date, user).Item1, parseMode: ParseMode.Markdown, disableWebPagePreview: true);
                    MessagesQueue.Message.SendTextMessage(chatId: chatId, text: AddingDisciplineMode.GetStagesAddingDiscipline(dbContext, user), replyMarkup: Statics.CancelKeyboardMarkup, parseMode: ParseMode.Markdown, disableWebPagePreview: true);

                } else {
                    (string, bool) schedule = Scheduler.GetScheduleByDate(dbContext, date, user);
                    MessagesQueue.Message.EditMessageText(chatId: chatId, messageId: messageId, text: schedule.Item1, replyMarkup: DefaultCallback.GetInlineKeyboardButton(date, user, schedule.Item2), parseMode: ParseMode.Markdown, disableWebPagePreview: true);
                }
            }
        }
    }
}
