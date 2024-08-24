using Core.Bot.Commands.Interfaces;
using Core.Bot.Messages;

using ScheduleBot;
using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace Core.Bot.Commands.Student.Days.ForAWeek.Message {
    internal class NextWeek : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => [UserCommands.Instance.Message["NextWeek"]];

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.group;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            await Statics.ScheduleRelevance(dbContext,  chatId, user.ScheduleProfile.Group!, Statics.WeekKeyboardMarkup);
            foreach(((string, bool), DateOnly) item in Scheduler.GetScheduleByWeak(dbContext, true, user))
                MessageQueue.SendTextMessage(chatId: chatId, text: item.Item1.Item1, replyMarkup: DefaultCallback.GetInlineKeyboardButton(item.Item2, user, item.Item1.Item2), parseMode: ParseMode.Markdown, disableWebPagePreview: true);
        }
    }
}
