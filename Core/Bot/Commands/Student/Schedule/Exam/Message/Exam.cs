using Core.Bot.Commands.Interfaces;
using Core.Bot.MessagesQueue;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;
namespace Core.Bot.Commands.Student.Schedule.Exam.Message {
    internal class Exam : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["Exam"]];

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.group;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            user.TelegramUserTmp.TmpData = UserCommands.Instance.Message["Exam"];
            await dbContext.SaveChangesAsync();

            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["Exam"], replyMarkup: Statics.ExamKeyboardMarkup);
        }
    }
}
