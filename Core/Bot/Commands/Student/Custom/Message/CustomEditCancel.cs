using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using ScheduleBot;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace Core.Bot.Commands.Student.Custom.Message {
    internal class CustomEditCancel : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["Cancel"]];

        public List<Mode> Modes => [Mode.CustomEditName, Mode.CustomEditLecturer, Mode.CustomEditLectureHall, Mode.CustomEditType, Mode.CustomEditStartTime, Mode.CustomEditEndTime];

        public Manager.Check Check => Manager.Check.none;

        public Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["MainMenu"], replyMarkup: Statics.MainKeyboardMarkup);

            if(!string.IsNullOrWhiteSpace(user.TelegramUserTmp.TmpData)) {
                if(user.IsOwner()) {
                    CustomDiscipline discipline = dbContext.CustomDiscipline.Single(i => i.ID == uint.Parse(user.TelegramUserTmp.TmpData));

                    (string, bool) schedule = Scheduler.GetScheduleByDate(dbContext, discipline.Date, user, all: true);
                    MessagesQueue.Message.SendTextMessage(chatId: chatId, text: schedule.Item1, replyMarkup: DefaultCallback.GetCustomEditAdminInlineKeyboardButton(discipline), parseMode: ParseMode.Markdown, disableWebPagePreview: true);
                }
            }

            user.TelegramUserTmp.Mode = Mode.Default;
            user.TelegramUserTmp.TmpData = null;
            return Task.CompletedTask;
        }
    }
}
