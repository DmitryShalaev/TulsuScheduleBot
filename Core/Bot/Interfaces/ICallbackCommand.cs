using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;

using static Core.Bot.Commands.Manager;

namespace Core.Bot.Interfaces {
    public interface ICallbackCommand {
        public ITelegramBotClient BotClient { get; }

        public string Command { get; }

        public Mode Mode { get; }

        public Check Check { get; }

        public Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args);
    }
}
