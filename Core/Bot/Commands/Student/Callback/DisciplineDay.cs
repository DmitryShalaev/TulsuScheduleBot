using Core.Bot.Interfaces;

using ScheduleBot;
using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Core.Bot.Commands.Student.Callback {
    public class DisciplineDay : ICallbackCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public string Command => "DisciplineDay";

        public Mode Mode => Mode.Default;

        public Manager.Check Check => Manager.Check.group;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args) {
            string[] tmp = args.Split('|');
            Discipline? discipline = dbContext.Disciplines.FirstOrDefault(i => i.ID == uint.Parse(tmp[0]));
            if(discipline is not null) {
                if(user.IsOwner()) {
                    var completedDisciplines = dbContext.CompletedDisciplines.Where(i => i.ScheduleProfileGuid == user.ScheduleProfileGuid).ToList();

                    CompletedDiscipline dayTmp = new(discipline, user.ScheduleProfileGuid);
                    CompletedDiscipline? dayCompletedDisciplines = completedDisciplines.FirstOrDefault(i => i.Equals(dayTmp));

                    if(dayCompletedDisciplines is not null)
                        dbContext.CompletedDisciplines.Remove(dayCompletedDisciplines);
                    else
                        dbContext.CompletedDisciplines.Add(dayTmp);

                    await dbContext.SaveChangesAsync();
                    await BotClient.EditMessageReplyMarkupAsync(chatId: chatId, messageId: messageId, replyMarkup: DefaultCallback.GetEditAdminInlineKeyboardButton(dbContext, discipline.Date, user.ScheduleProfile));
                    return;
                }
            }

            if(DateOnly.TryParse(tmp[1], out DateOnly date)) {
                (string, bool) schedule = Scheduler.GetScheduleByDate(dbContext, date, user);
                await BotClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: schedule.Item1, replyMarkup: DefaultCallback.GetInlineKeyboardButton(date, user, schedule.Item2), parseMode: ParseMode.Markdown, disableWebPagePreview: true);
            }
        }
    }
}
