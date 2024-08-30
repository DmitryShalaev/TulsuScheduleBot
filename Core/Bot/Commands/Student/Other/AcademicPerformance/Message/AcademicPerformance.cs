using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;
namespace Core.Bot.Commands.Student.Other.AcademicPerformance.Message {
    internal class AcademicPerformance : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["AcademicPerformance"]];

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.studentId;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            if(user.IsSupergroup()) return;

            user.TelegramUserTmp.TmpData = UserCommands.Instance.Message["AcademicPerformance"];

            string StudentID = user.ScheduleProfile.StudentID!;

            await Statics.ProgressRelevanceAsync(dbContext, chatId, StudentID, null, false);
            await dbContext.SaveChangesAsync();

            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["AcademicPerformance"], replyMarkup: DefaultMessage.GetTermsKeyboardMarkup(dbContext, StudentID));
        }
    }
}
