using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;
using Core.Parser;

using ScheduleBot;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
namespace Core.Bot.Commands.Teachers.Message {
    internal class TeachersWorkScheduleDefault : IMessageCommand {

        public List<string>? Commands => null;

        public List<Mode> Modes => [Mode.TeachersWorkSchedule];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            IEnumerable<string> find = NGramSearch.Instance.TeacherFindMatch(args);

            if(find.Any()) {
                if(find.Count() > 1) {
                    var buttons = new List<InlineKeyboardButton[]>();
                    foreach(string item in find) {
                        string callback = $"Select|{item}";

                        buttons.Add([InlineKeyboardButton.WithCallbackData(text: item, callbackData: callback[..Math.Min(callback.Length, 35)])]);
                    }

                    MessagesQueue.Message.SendTextMessage(chatId: chatId, text: "Выберите преподавателя.\nЕсли его нет уточните ФИО.", replyMarkup: new InlineKeyboardMarkup(buttons));
                } else {
                    user.TelegramUserTmp.Mode = Mode.TeacherSelected;
                    string teacherName = user.TelegramUserTmp.TmpData = find.First();

                    TeacherLastUpdate teacher = dbContext.TeacherLastUpdate.First(i => i.Teacher == teacherName);

                    if(string.IsNullOrWhiteSpace(teacher.LinkProfile))
                        await ScheduleParser.Instance.UpdatingTeacherInfo(dbContext, teacherName);

                    MessagesQueue.Message.SendTextMessage(chatId: chatId, text: $"{UserCommands.Instance.Message["CurrentTeacher"]}: [{teacherName}]({teacher.LinkProfile})", replyMarkup: DefaultMessage.GetTeacherWorkScheduleSelectedKeyboardMarkup(teacherName), parseMode: ParseMode.Markdown);
                }

                await dbContext.SaveChangesAsync();
            } else {
                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: "Преподаватель не найден!", replyMarkup: Statics.WorkScheduleBackKeyboardMarkup);
            }
        }
    }
}
