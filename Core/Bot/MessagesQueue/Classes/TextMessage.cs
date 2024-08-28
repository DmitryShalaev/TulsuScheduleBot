using Core.Bot.MessagesQueue.Interfaces;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Core.Bot.MessagesQueue.Classes {
    public class TextMessage(ChatId chatId, string text, IReplyMarkup? replyMarkup, ParseMode? parseMode, bool? disableNotification, bool deletePrevious, bool saveMessageId) : IMessageQueue {
        public ChatId ChatId { get; } = chatId;
        public string Text { get; } = text;

        public IReplyMarkup? ReplyMarkup { get; } = replyMarkup;
        public ParseMode? ParseMode { get; } = parseMode;

        public bool? DisableNotification { get; } = disableNotification;

        public bool DeletePrevious { get; } = deletePrevious;
        public bool SaveMessageId { get; } = saveMessageId;
    }
}
