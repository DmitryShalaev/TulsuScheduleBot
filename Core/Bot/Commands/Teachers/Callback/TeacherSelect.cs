using Core.Bot.Commands.Interfaces;
using Core.Bot.Messages;

using ScheduleBot;
using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

#pragma warning disable CA1862 // Используйте перегрузки метода "StringComparison" для сравнения строк без учета регистра

namespace Core.Bot.Commands.Teachers.Callback {
    public class TeacherSelect : ICallbackCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public string Command => "Select";

        public Mode Mode => Mode.TeachersWorkSchedule;

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args) {
            user.TelegramUserTmp.Mode = Mode.TeacherSelected;

            TeacherLastUpdate teacher = dbContext.TeacherLastUpdate.First(i => i.Teacher.ToLower().StartsWith(args));

            string teacherName = user.TelegramUserTmp.TmpData = teacher.Teacher;
            await dbContext.SaveChangesAsync();

            if(string.IsNullOrWhiteSpace(teacher.LinkProfile))
                await Parser.Instance.UpdatingTeacherInfo(dbContext, teacherName);

            await BotClient.DeleteMessageAsync(chatId: chatId, messageId: messageId);

            MessageQueue.SendTextMessage(chatId: chatId, text: $"{UserCommands.Instance.Message["CurrentTeacher"]}: [{teacherName}]({teacher.LinkProfile})", replyMarkup: DefaultMessage.GetTeacherWorkScheduleSelectedKeyboardMarkup(teacherName), parseMode: ParseMode.Markdown, disableWebPagePreview: true);
        }
    }
}
