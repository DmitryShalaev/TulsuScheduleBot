using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using ScheduleBot;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace Core.Bot.Commands.Classrooms.Days.ForAWeek.Message {

    internal class ClassroomThisWeek : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["ThisWeek"]];

        public List<Mode> Modes => [Mode.ClassroomSelected];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            await Statics.ClassroomWorkScheduleRelevanceAsync(dbContext, chatId, user.TelegramUserTmp.TmpData!, replyMarkup: Statics.WeekKeyboardMarkup);
            foreach((string, DateOnly) item in Scheduler.GetClassroomWorkScheduleByWeak(dbContext, false, user.TelegramUserTmp.TmpData!, user))
                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: item.Item1, replyMarkup: Statics.WeekKeyboardMarkup, parseMode: ParseMode.Markdown);
        }
    }
}
