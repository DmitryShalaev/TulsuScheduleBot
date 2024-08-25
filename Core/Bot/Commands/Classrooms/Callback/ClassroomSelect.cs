using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;

#pragma warning disable CA1862 // Используйте перегрузки метода "StringComparison" для сравнения строк без учета регистра

namespace Core.Bot.Commands.Classrooms.Callback {
    public class ClassroomSelect : ICallbackCommand {

        public string Command => "Select";

        public Mode Mode => Mode.ClassroomSchedule;

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args) {
            user.TelegramUserTmp.Mode = Mode.ClassroomSelected;

            ClassroomLastUpdate classroom = dbContext.ClassroomLastUpdate.First(i => i.Classroom.ToLower().StartsWith(args));

            string _classroom = user.TelegramUserTmp.TmpData = classroom.Classroom;
            await dbContext.SaveChangesAsync();

            MessagesQueue.Message.DeleteMessage(chatId: chatId, messageId: messageId);

            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: $"{UserCommands.Instance.Message["CurrentClassroom"]}: {_classroom}", replyMarkup: DefaultMessage.GetClassroomWorkScheduleSelectedKeyboardMarkup(_classroom), disableWebPagePreview: true);
        }
    }
}
