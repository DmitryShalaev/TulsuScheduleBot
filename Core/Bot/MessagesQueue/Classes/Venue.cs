using Core.Bot.MessagesQueue.Interfaces;

using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Core.Bot.MessagesQueue.Classes {
    public class Venue(ChatId chatId, double latitude, double longitude, string title, string address, IReplyMarkup? replyMarkup) : IMessageQueue {
        public ChatId ChatId { get; } = chatId;
        public double Latitude { get; } = latitude;
        public double Longitude { get; } = longitude;
        public string Title { get; } = title;
        public string Address { get; } = address;

        public IReplyMarkup? ReplyMarkup { get; } = replyMarkup;
    }
}
