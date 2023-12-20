using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;

using static Core.Bot.Commands.Manager;

namespace Core.Bot.Interfaces {
    public interface IMessageCommand {
        public ITelegramBotClient BotClient { get; }

        public List<string>? Commands { get; }

        public List<Mode> Modes { get; }

        public Check Check { get; }

        public Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args);
    }
}
