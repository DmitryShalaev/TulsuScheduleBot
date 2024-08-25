using System.Text.RegularExpressions;

using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using ScheduleBot;

using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
namespace Core.Bot.Commands.Teachers.Message {

    internal class TeacherSelectedDefault : IMessageCommand {

        public List<string>? Commands => null;

        public List<Mode> Modes => [Mode.TeacherSelected];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            Match match = Statics.DateRegex().Match(args);
            if(match.Success) {
                DateTime now = DateTime.Now;
                string sDate = $"{match.Groups[1].Value} " +
                               $"{(string.IsNullOrWhiteSpace(match.Groups[3].Value) ? now.Month : match.Groups[3].Value)} " +
                               $"{(string.IsNullOrWhiteSpace(match.Groups[5].Value) ? now.Year : match.Groups[5].Value)}";

                ReplyKeyboardMarkup teacherWorkSchedule = DefaultMessage.GetTeacherWorkScheduleSelectedKeyboardMarkup(user.TelegramUserTmp.TmpData!);

                try {
                    var date = DateOnly.Parse(sDate);

                    await Statics.TeacherWorkScheduleRelevanceAsync(dbContext, chatId, user.TelegramUserTmp.TmpData!, teacherWorkSchedule);
                    MessagesQueue.Message.SendTextMessage(chatId: chatId, text: Scheduler.GetTeacherWorkScheduleByDate(dbContext, date, user.TelegramUserTmp.TmpData!));
                } catch(Exception) {
                    MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["CommandRecognizedAsADate"], replyMarkup: teacherWorkSchedule);
                }

                return;
            }

            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["CommandNotRecognized"], replyMarkup: DefaultMessage.GetTeacherWorkScheduleSelectedKeyboardMarkup(user.TelegramUserTmp.TmpData!));

        }
    }
}
