using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using ScheduleBot;

using Telegram.Bot.Types;
namespace Core.Bot.Commands.Student.Schedule.Exam.Message {
    internal class NextExam : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["NextExam"]];

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.group;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            await Statics.ScheduleRelevanceAsync(dbContext, chatId, user.ScheduleProfile.Group!, Statics.ExamKeyboardMarkup);
            foreach(string item in Scheduler.GetExamse(dbContext, user.ScheduleProfile, false))
                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: item, replyMarkup: Statics.ExamKeyboardMarkup);
        }
    }
}
