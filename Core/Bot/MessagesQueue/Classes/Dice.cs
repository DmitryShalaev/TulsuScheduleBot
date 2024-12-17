using Core.Bot.MessagesQueue.Interfaces;

using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Core.Bot.MessagesQueue.Classes {
    public class Dice(ChatId chatId, string? emoji, IReplyMarkup? replyMarkup) : IMessageQueue {
        public ChatId ChatId { get; } = chatId;
        public string? Emoji { get; } = emoji;
        public IReplyMarkup? ReplyMarkup { get; } = replyMarkup;
    }
}
