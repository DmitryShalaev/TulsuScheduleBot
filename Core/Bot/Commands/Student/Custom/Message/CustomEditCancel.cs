using ScheduleBot;
using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Core.Bot.Interfaces;
namespace Core.Bot.Commands.Student.Custom.Message {
    internal class CustomEditCancel : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => new() { UserCommands.Instance.Message["Cancel"] };

        public List<Mode> Modes => new() { Mode.CustomEditName, Mode.CustomEditLecturer, Mode.CustomEditLectureHall, Mode.CustomEditType, Mode.CustomEditStartTime, Mode.CustomEditEndTime };

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            await BotClient.SendTextMessageAsync(chatId: chatId, text: UserCommands.Instance.Message["MainMenu"], replyMarkup: Statics.MainKeyboardMarkup);

            if(!string.IsNullOrWhiteSpace(user.TempData)) {
                if(user.IsOwner()) {
                    CustomDiscipline discipline = dbContext.CustomDiscipline.Single(i => i.ID == uint.Parse(user.TempData));

                    (string, bool) schedule = Scheduler.GetScheduleByDate(dbContext, discipline.Date, user, all: true);
                    await BotClient.SendTextMessageAsync(chatId: chatId, text: schedule.Item1, replyMarkup: DefaultCallback.GetCustomEditAdminInlineKeyboardButton(discipline), parseMode: ParseMode.Markdown);
                }
            }

            user.Mode = Mode.Default;
            user.TempData = null;

            await Statics.DeleteTempMessage(user, messageId);
        }
    }
}
