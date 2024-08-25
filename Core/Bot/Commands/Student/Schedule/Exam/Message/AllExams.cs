using Core.Bot.Commands.Interfaces;
using Core.Bot.MessagesQueue;
using Core.DB;
using Core.DB.Entity;

using ScheduleBot;

using Telegram.Bot.Types;

namespace Core.Bot.Commands.Student.Schedule.Exam.Message {
    internal class AllExams : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["AllExams"]];

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.group;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            await Statics.ScheduleRelevanceAsync(dbContext, chatId, user.ScheduleProfile.Group!, Statics.ExamKeyboardMarkup);
            foreach(string item in Scheduler.GetExamse(dbContext, user.ScheduleProfile, true))
                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: item, replyMarkup: Statics.ExamKeyboardMarkup);
        }
    }
}
