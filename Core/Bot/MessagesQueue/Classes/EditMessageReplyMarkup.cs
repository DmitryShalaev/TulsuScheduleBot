using Core.Bot.MessagesQueue.Interfaces;

using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Core.Bot.MessagesQueue.Classes {
    public class EditMessageReplyMarkup(ChatId chatId, int messageId, InlineKeyboardMarkup? replyMarkup) : IMessageQueue {
        public ChatId ChatId { get; } = chatId;
        public int MessageId { get; } = messageId;
        public InlineKeyboardMarkup? ReplyMarkup { get; } = replyMarkup;
    }
}
