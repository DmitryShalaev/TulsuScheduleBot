using Core.Bot.Commands.Interfaces;
using Core.Bot.MessagesQueue;
using Core.DB;
using Core.DB.Entity;

using ScheduleBot;

using Telegram.Bot.Types;
namespace Core.Bot.Commands.Teachers.Days.ByDays.Message {
    internal class TeachersWednesday : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["Wednesday"]];

        public List<Mode> Modes => [Mode.TeacherSelected];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            await Statics.TeacherWorkScheduleRelevanceAsync(dbContext, chatId, user.TelegramUserTmp.TmpData!, Statics.DaysKeyboardMarkup);
            foreach((string, DateOnly) day in Scheduler.GetTeacherWorkScheduleByDay(dbContext, DayOfWeek.Wednesday, user.TelegramUserTmp.TmpData!))
                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: day.Item1, replyMarkup: Statics.DaysKeyboardMarkup);
        }
    }
}
