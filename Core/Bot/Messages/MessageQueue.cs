using System.Collections.Concurrent;

using Core.Bot.Messages.Interfaces;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Core.Bot.Messages {

    public static class MessageQueue {
        private static readonly ConcurrentQueue<IMessageQueue> stack = new();

        public static void SendTextMessage(ChatId chatId, string text, IReplyMarkup? replyMarkup, ParseMode? parseMode) {
            var message = new TextMessage(chatId, text, replyMarkup, parseMode);

            stack.Enqueue(message);
        }
    }
}
