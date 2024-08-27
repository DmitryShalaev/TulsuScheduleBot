using System.Text;

using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Microsoft.EntityFrameworkCore;

using ScheduleBot;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace Core.Bot.Commands.Student.Custom.Callback {
    public class CustomDelete : ICallbackCommand {

        public string Command => "CustomDelete";

        public Mode Mode => Mode.Default;

        public Manager.Check Check => Manager.Check.group;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args) {
            string[] tmp = args.Split('|');
            CustomDiscipline? customDiscipline = await dbContext.CustomDiscipline.FirstOrDefaultAsync(i => i.ID == uint.Parse(tmp[0]));
            if(customDiscipline is not null) {
                if(user.IsOwner()) {
                    dbContext.CustomDiscipline.Remove(customDiscipline);
                    await dbContext.SaveChangesAsync();

                    StringBuilder sb = new(Scheduler.GetScheduleByDate(dbContext, customDiscipline.Date, user, true).Item1);
                    sb.AppendLine($"⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯\n<b>{UserCommands.Instance.Message["SelectAnAction"]}</b>");

                    MessagesQueue.Message.EditMessageText(chatId: chatId, messageId: messageId, text: sb.ToString(), replyMarkup: DefaultCallback.GetEditAdminInlineKeyboardButton(dbContext, customDiscipline.Date, user.ScheduleProfile), parseMode: ParseMode.Html, disableWebPagePreview: true);
                    return;
                }
            }

            if(DateOnly.TryParse(tmp[1], out DateOnly date)) {
                (string, bool) schedule = Scheduler.GetScheduleByDate(dbContext, date, user, all: true);
                MessagesQueue.Message.EditMessageText(chatId: chatId, messageId: messageId, text: schedule.Item1, replyMarkup: DefaultCallback.GetInlineKeyboardButton(date, user, schedule.Item2), parseMode: ParseMode.Markdown, disableWebPagePreview: true);
            }
        }
    }
}
