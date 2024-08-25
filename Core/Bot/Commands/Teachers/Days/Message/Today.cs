using Core.Bot.Commands.Interfaces;
using Core.Bot.MessagesQueue;
using Core.DB;
using Core.DB.Entity;

using ScheduleBot;

using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
namespace Core.Bot.Commands.Teachers.Days.Message {

    internal class TeachersToday : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["Today"]];

        public List<Mode> Modes => [Mode.TeacherSelected];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            ReplyKeyboardMarkup teacherWorkSchedule = DefaultMessage.GetTeacherWorkScheduleSelectedKeyboardMarkup(user.TelegramUserTmp.TmpData!);

            await Statics.TeacherWorkScheduleRelevanceAsync(dbContext, chatId, user.TelegramUserTmp.TmpData!, teacherWorkSchedule);
            var date = DateOnly.FromDateTime(DateTime.Now);
            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: Scheduler.GetTeacherWorkScheduleByDate(dbContext, date, user.TelegramUserTmp.TmpData!), replyMarkup: teacherWorkSchedule);
        }
    }
}
