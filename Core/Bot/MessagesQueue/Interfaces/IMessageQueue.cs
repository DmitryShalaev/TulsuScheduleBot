using Telegram.Bot.Types;

namespace Core.Bot.MessagesQueue.Interfaces {
    public interface IMessageQueue {
        public ChatId ChatId { get; }
    }
}