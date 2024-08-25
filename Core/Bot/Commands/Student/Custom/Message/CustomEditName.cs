using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using ScheduleBot;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace Core.Bot.Commands.Student.Custom.Message {
    internal class CustomEditName : IMessageCommand {

        public List<string>? Commands => null;

        public List<Mode> Modes => [Mode.CustomEditName];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            if(!string.IsNullOrWhiteSpace(user.TelegramUserTmp.TmpData)) {
                CustomDiscipline discipline = dbContext.CustomDiscipline.Single(i => i.ID == uint.Parse(user.TelegramUserTmp.TmpData));
                discipline.Name = args;

                user.TelegramUserTmp.Mode = Mode.Default;
                user.TelegramUserTmp.TmpData = null;

                await dbContext.SaveChangesAsync();

                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: "Название предмета успешно изменено.", replyMarkup: DefaultMessage.GetMainKeyboardMarkup(user));
                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: Scheduler.GetScheduleByDate(dbContext, discipline.Date, user, all: true).Item1, replyMarkup: DefaultCallback.GetCustomEditAdminInlineKeyboardButton(discipline), parseMode: ParseMode.Markdown, disableWebPagePreview: true);
            }
        }
    }
}
