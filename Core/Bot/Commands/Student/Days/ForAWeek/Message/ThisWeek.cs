using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using ScheduleBot;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace Core.Bot.Commands.Student.Days.ForAWeek.Message {
    internal class ThisWeek : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["ThisWeek"]];

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.group;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            await Statics.ScheduleRelevanceAsync(dbContext, chatId, user.ScheduleProfile.Group!, Statics.WeekKeyboardMarkup);
            foreach(((string, bool), DateOnly) item in Scheduler.GetScheduleByWeak(dbContext, false, user))
                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: item.Item1.Item1, replyMarkup: DefaultCallback.GetInlineKeyboardButton(item.Item2, user, item.Item1.Item2), parseMode: ParseMode.Markdown);
        }
    }
}
