using Core.Bot.Messages.Interfaces;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Core.Bot.Messages {
    public class TextMessage(ChatId chatId, string text, IReplyMarkup? replyMarkup, ParseMode? parseMode) : IMessageQueue {
        public ChatId ChatId { get; set; } = chatId;
        public string Text { get; set; } = text;

        public IReplyMarkup? ReplyMarkup { get; set; } = replyMarkup;
        public ParseMode? ParseMode { get; set; } = parseMode;
    }
}
