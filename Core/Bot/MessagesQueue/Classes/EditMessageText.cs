using Core.Bot.MessagesQueue.Interfaces;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Core.Bot.MessagesQueue.Classes {
    public class EditMessageText(ChatId chatId, int messageId, string text, InlineKeyboardMarkup? replyMarkup, ParseMode? parseMode, bool? disableWebPagePreview) : IMessageQueue {
        public ChatId ChatId { get; set; } = chatId;
        public int MessageId { get; } = messageId;
        public string Text { get; set; } = text;

        public InlineKeyboardMarkup? ReplyMarkup { get; set; } = replyMarkup;
        public ParseMode? ParseMode { get; set; } = parseMode;

        public bool? DisableWebPagePreview { get; set; } = disableWebPagePreview;
    }
}
