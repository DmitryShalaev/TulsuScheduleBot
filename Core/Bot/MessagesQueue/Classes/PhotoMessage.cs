using Core.Bot.MessagesQueue.Interfaces;

using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Core.Bot.MessagesQueue.Classes {
    public class PhotoMessage(ChatId chatId, string path, IReplyMarkup? replyMarkup, bool deleteFile) : IMessageQueue {
        public ChatId ChatId { get; } = chatId;
        public string Path { get; } = path;
        public IReplyMarkup? ReplyMarkup { get; } = replyMarkup;
        public bool DeleteFile { get; } = deleteFile;
    }
}
