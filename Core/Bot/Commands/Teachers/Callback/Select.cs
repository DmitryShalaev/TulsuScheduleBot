using Core.Bot.Interfaces;

using ScheduleBot;
using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Core.Bot.Commands.Teachers.Back.Callback {
    public class Select : ICallbackCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public string Command => "Select";

        public Mode Mode => Mode.TeachersWorkSchedule;

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args) {
            user.TelegramUserTmp.Mode = Mode.TeacherSelected;
            user.TelegramUserTmp.RequestingMessageID = null;

            TeacherLastUpdate teacher = dbContext.TeacherLastUpdate.First(i => i.Teacher.ToLower().StartsWith(args));

            string teacherName = user.TelegramUserTmp.TmpData = teacher.Teacher;

            if(string.IsNullOrWhiteSpace(teacher.LinkProfile))
                await Parser.Instance.UpdatingTeacherInfo(dbContext, teacherName);

            await BotClient.DeleteMessageAsync(chatId: chatId, messageId: messageId);

            await BotClient.SendTextMessageAsync(chatId: chatId, text: $"{UserCommands.Instance.Message["CurrentTeacher"]}: [{teacherName}]({teacher.LinkProfile})", replyMarkup: DefaultMessage.GetTeacherWorkScheduleSelectedKeyboardMarkup(teacherName), parseMode: ParseMode.Markdown);
        }
    }
}
