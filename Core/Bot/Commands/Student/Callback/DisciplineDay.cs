using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Microsoft.EntityFrameworkCore;

using ScheduleBot;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Core.Bot.Commands.Student.Callback {
    public class DisciplineDay : ICallbackCommand {

        public string Command => "DisciplineDay";

        public Mode Mode => Mode.Default;

        public Manager.Check Check => Manager.Check.group;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args) {
            string[] tmp = args.Split('|');
            Discipline? discipline = await dbContext.Disciplines.FirstOrDefaultAsync(i => i.ID == uint.Parse(tmp[0]));
            if(discipline is not null) {
                if(user.IsOwner() && !user.IsSupergroup()) {
                    var completedDisciplines = dbContext.CompletedDisciplines.Where(i => i.ScheduleProfileGuid == user.ScheduleProfileGuid).ToList();

                    CompletedDiscipline dayTmp = new(discipline, user.ScheduleProfileGuid);
                    CompletedDiscipline? dayCompletedDisciplines = completedDisciplines.FirstOrDefault(i => i.Equals(dayTmp));

                    if(dayCompletedDisciplines is not null)
                        dbContext.CompletedDisciplines.Remove(dayCompletedDisciplines);
                    else
                        dbContext.CompletedDisciplines.Add(dayTmp);

                    await dbContext.SaveChangesAsync();
                    MessagesQueue.Message.EditMessageReplyMarkup(chatId: chatId, messageId: messageId, replyMarkup: DefaultCallback.GetEditAdminInlineKeyboardButton(dbContext, discipline.Date, user.ScheduleProfile));
                    return;
                }
            }

            if(DateOnly.TryParse(tmp[1], out DateOnly date)) {
                (string, bool) schedule = Scheduler.GetScheduleByDate(dbContext, date, user);
                MessagesQueue.Message.EditMessageText(chatId: chatId, messageId: messageId, text: schedule.Item1, replyMarkup: DefaultCallback.GetInlineKeyboardButton(date, user, schedule.Item2), parseMode: ParseMode.Markdown);
            }
        }
    }
}
