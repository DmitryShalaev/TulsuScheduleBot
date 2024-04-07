using Core.Bot.Interfaces;

using ScheduleBot;
using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
namespace Core.Bot.Commands.Teachers.Message {
    internal class TeachersWorkScheduleDefault : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => null;

        public List<Mode> Modes => [Mode.TeachersWorkSchedule];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            IEnumerable<string> find = NGramSearch.Instance.FindMatch(args);

            if(find.Any()) {
                if(find.Count() > 1) {
                    var buttons = new List<InlineKeyboardButton[]>();
                    foreach(string item in find) {
                        string callback = $"Select|{item}";

                        buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: item, callbackData: callback[..Math.Min(callback.Length, 35)]) });
                    }

                    user.TelegramUserTmp.RequestingMessageID = (await BotClient.SendTextMessageAsync(chatId: chatId, text: "Выберите преподавателя.\nЕсли его нет уточните ФИО.", replyMarkup: new InlineKeyboardMarkup(buttons))).MessageId;
                } else {
                    user.TelegramUserTmp.Mode = Mode.TeacherSelected;
                    string teacherName = user.TelegramUserTmp.TmpData = find.First();

                    TeacherLastUpdate teacher = dbContext.TeacherLastUpdate.First(i => i.Teacher == teacherName);

                    if(string.IsNullOrWhiteSpace(teacher.LinkProfile))
                        await Parser.Instance.UpdatingTeacherInfo(dbContext, teacherName);

                    await BotClient.SendTextMessageAsync(chatId: chatId, text: $"{UserCommands.Instance.Message["CurrentTeacher"]}: [{teacherName}]({teacher.LinkProfile})", replyMarkup: DefaultMessage.GetTeacherWorkScheduleSelectedKeyboardMarkup(teacherName), parseMode: ParseMode.Markdown);
                }
            } else {
                await BotClient.SendTextMessageAsync(chatId: chatId, text: "Преподаватель не найден!", replyMarkup: Statics.TeachersWorkScheduleBackKeyboardMarkup);
            }
        }
    }
}
