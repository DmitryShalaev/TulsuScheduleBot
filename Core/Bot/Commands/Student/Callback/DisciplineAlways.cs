using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Microsoft.EntityFrameworkCore;

using ScheduleBot;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace Core.Bot.Commands.Student.Callback {
    public class DisciplineAlways : ICallbackCommand {

        public string Command => "DisciplineAlways";

        public Mode Mode => Mode.Default;

        public Manager.Check Check => Manager.Check.group;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args) {
            string[] tmp = args.Split('|');
            Discipline? discipline = await dbContext.Disciplines.FirstOrDefaultAsync(i => i.ID == uint.Parse(tmp[0]));
            if(discipline is not null) {
                if(user.IsOwner()) {
                    var completedDisciplines = dbContext.CompletedDisciplines.Where(i => i.ScheduleProfileGuid == user.ScheduleProfileGuid).ToList();

                    CompletedDiscipline alwaysTmp = new(discipline, user.ScheduleProfileGuid) { Date = null };
                    CompletedDiscipline? alwaysCompletedDisciplines = completedDisciplines.FirstOrDefault(i => i.Equals(alwaysTmp));

                    if(alwaysCompletedDisciplines is not null) {
                        dbContext.CompletedDisciplines.Remove(alwaysCompletedDisciplines);
                    } else {
                        dbContext.CompletedDisciplines.RemoveRange(dbContext.CompletedDisciplines.Where(i => i.ScheduleProfileGuid == user.ScheduleProfileGuid && i.Date != null && i.Name == discipline.Name && i.Lecturer == discipline.Lecturer && i.Class == discipline.Class && i.Subgroup == discipline.Subgroup && i.IntersectionMark == discipline.IntersectionMark));
                        dbContext.CompletedDisciplines.Add(alwaysTmp);
                    }

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
