using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;
using Core.Parser;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

#pragma warning disable CA1862 // Используйте перегрузки метода "StringComparison" для сравнения строк без учета регистра

namespace Core.Bot.Commands.Teachers.Callback {
    public class TeacherSelect : ICallbackCommand {

        public string Command => "TeacherSelect";

        public Mode Mode => Mode.TeachersWorkSchedule;

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args) {
            user.TelegramUserTmp.Mode = Mode.TeacherSelected;

            TeacherLastUpdate teacher = dbContext.TeacherLastUpdate.First(i => i.Teacher.ToLower().StartsWith(args));

            string teacherName = user.TelegramUserTmp.TmpData = teacher.Teacher;
            await dbContext.SaveChangesAsync();

            if(string.IsNullOrWhiteSpace(teacher.LinkProfile))
                await ScheduleParser.Instance.UpdatingTeacherInfo(dbContext, teacherName);

            MessagesQueue.Message.DeleteMessage(chatId: chatId, messageId: messageId);

            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: $"{UserCommands.Instance.Message["CurrentTeacher"]}: [{teacherName}]({teacher.LinkProfile})", replyMarkup: DefaultMessage.GetTeacherWorkScheduleSelectedKeyboardMarkup(teacherName), parseMode: ParseMode.Markdown);
        }
    }
}
