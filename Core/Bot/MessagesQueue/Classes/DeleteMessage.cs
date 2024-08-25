using Core.Bot.MessagesQueue.Interfaces;

using Telegram.Bot.Types;

namespace Core.Bot.MessagesQueue.Classes {

    public class DeleteMessage(ChatId chatId, int messageId) : IMessageQueue {
        public ChatId ChatId { get; set; } = chatId;
        public int MessageId { get; } = messageId;
    }
}
