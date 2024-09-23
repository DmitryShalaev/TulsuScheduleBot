using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using ScheduleBot;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace Core.Bot.Commands.Student.Days.Message {
    internal class Today : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["Today"]];

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.group;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            await Statics.ScheduleRelevanceAsync(dbContext, chatId, user.ScheduleProfile.Group!, DefaultMessage.GetMainKeyboardMarkup(user));
            var date = DateOnly.FromDateTime(DateTime.Now);

            (string, bool) schedule = Scheduler.GetScheduleByDate(dbContext, date, user);

            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: schedule.Item1, replyMarkup: DefaultCallback.GetInlineKeyboardButton(date, user, schedule.Item2), parseMode: ParseMode.Markdown);
        }
    }
}
