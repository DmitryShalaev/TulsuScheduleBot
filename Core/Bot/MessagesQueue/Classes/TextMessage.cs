using Core.Bot.MessagesQueue.Interfaces;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Core.Bot.MessagesQueue.Classes {
    public class TextMessage(ChatId chatId, string text, IReplyMarkup? replyMarkup, ParseMode? parseMode, bool? disableWebPagePreview, bool? disableNotification, bool deletePrevious, bool saveMessageId) : IMessageQueue {
        public ChatId ChatId { get; set; } = chatId;
        public string Text { get; set; } = text;

        public IReplyMarkup? ReplyMarkup { get; set; } = replyMarkup;
        public ParseMode? ParseMode { get; set; } = parseMode;

        public bool? DisableWebPagePreview { get; set; } = disableWebPagePreview;
        public bool? DisableNotification { get; set; } = disableNotification;

        public bool DeletePrevious { get; set; } = deletePrevious;
        public bool SaveMessageId { get; set; } = saveMessageId;
    }
}
