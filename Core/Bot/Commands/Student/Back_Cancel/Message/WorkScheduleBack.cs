using Core.Bot.Commands.Interfaces;
using Core.Bot.MessagesQueue;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;

namespace Core.Bot.Commands.Student.Back_Cancel.Message {
    public class WorkScheduleBack : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["WorkScheduleBack"]];

        public List<Mode> Modes => [Mode.TeachersWorkSchedule, Mode.TeacherSelected, Mode.ClassroomSchedule, Mode.ClassroomSelected];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["MainMenu"], replyMarkup: Statics.MainKeyboardMarkup);

            user.TelegramUserTmp.Mode = Mode.Default;
            user.TelegramUserTmp.TmpData = null;

            await dbContext.SaveChangesAsync();
        }
    }
}
