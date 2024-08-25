using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using ScheduleBot;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace Core.Bot.Commands.Classrooms.Days.ByDays.Message {
    internal class ClassroomThursday : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["Thursday"]];

        public List<Mode> Modes => [Mode.ClassroomSelected];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            await Statics.ClassroomWorkScheduleRelevanceAsync(dbContext, chatId, user.TelegramUserTmp.TmpData!, Statics.DaysKeyboardMarkup);
            foreach((string, DateOnly) day in Scheduler.GetClassroomWorkScheduleByDay(dbContext, DayOfWeek.Thursday, user.TelegramUserTmp.TmpData!, user))
                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: day.Item1, replyMarkup: Statics.DaysKeyboardMarkup, parseMode: ParseMode.Markdown, disableWebPagePreview: true);
        }
    }
}
