using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using ScheduleBot;

using Telegram.Bot.Types;
namespace Core.Bot.Commands.Teachers.Days.ForAWeek.Message {

    internal class TeachersThisWeek : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["ThisWeek"]];

        public List<Mode> Modes => [Mode.TeacherSelected];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            await Statics.TeacherWorkScheduleRelevanceAsync(dbContext, chatId, user.TelegramUserTmp.TmpData!, replyMarkup: Statics.WeekKeyboardMarkup);
            foreach((string, DateOnly) item in Scheduler.GetTeacherWorkScheduleByWeak(dbContext, false, user.TelegramUserTmp.TmpData!))
                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: item.Item1, replyMarkup: Statics.WeekKeyboardMarkup);
        }
    }
}
