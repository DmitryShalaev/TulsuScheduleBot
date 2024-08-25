using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using ScheduleBot;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace Core.Bot.Commands.Student.Days.ByDays.Message {
    internal class Thursday : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["Thursday"]];

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.group;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            await Statics.ScheduleRelevanceAsync(dbContext, chatId, user.ScheduleProfile.Group!, Statics.DaysKeyboardMarkup);
            foreach(((string, bool), DateOnly) day in Scheduler.GetScheduleByDay(dbContext, DayOfWeek.Thursday, user))
                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: day.Item1.Item1, replyMarkup: DefaultCallback.GetInlineKeyboardButton(day.Item2, user, day.Item1.Item2), parseMode: ParseMode.Markdown, disableWebPagePreview: true);
        }
    }
}
