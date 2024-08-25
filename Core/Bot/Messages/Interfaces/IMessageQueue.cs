using Telegram.Bot.Types;

namespace Core.Bot.Messages.Interfaces {
    public interface IMessageQueue {
        public ChatId ChatId { get; set; }
    }
}