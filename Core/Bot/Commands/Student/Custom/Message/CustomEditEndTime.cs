using System.Text;

using Core.Bot.Commands.AddingDiscipline;
using Core.Bot.Commands.Interfaces;
using Core.Bot.Commands.Student.Schedule.Message;
using Core.DB;
using Core.DB.Entity;

using ScheduleBot;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace Core.Bot.Commands.Student.Custom.Message {
    internal class CustomEditEndTime : IMessageCommand {

        public List<string>? Commands => null;

        public List<Mode> Modes => [Mode.CustomEditEndTime];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            if(!string.IsNullOrWhiteSpace(user.TelegramUserTmp.TmpData)) {
                CustomDiscipline discipline = dbContext.CustomDiscipline.Single(i => i.ID == uint.Parse(user.TelegramUserTmp.TmpData));
                try {
                    discipline.EndTime = AddingDisciplineMode.ParseTime(args);
                    user.TelegramUserTmp.Mode = Mode.Default;
                    user.TelegramUserTmp.TmpData = null;

                    await dbContext.SaveChangesAsync();

                    MessagesQueue.Message.SendTextMessage(chatId: chatId, text: "Время конца успешно изменено.", replyMarkup: Statics.MainKeyboardMarkup);

                    StringBuilder sb = new(Scheduler.GetScheduleByDate(dbContext, discipline.Date, user, all: true).Item1);
                    sb.AppendLine($"⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯\n<b>{UserCommands.Instance.Message["SelectAnAction"]}</b>");

                    MessagesQueue.Message.SendTextMessage(chatId: chatId, text: sb.ToString(), replyMarkup: DefaultCallback.GetCustomEditAdminInlineKeyboardButton(discipline), parseMode: ParseMode.Html, disableWebPagePreview: true);
                } catch(Exception) {
                    MessagesQueue.Message.SendTextMessage(chatId: chatId, text: "Ошибка в формате времени!", replyMarkup: Statics.CancelKeyboardMarkup);
                }
            }
        }
    }
}
