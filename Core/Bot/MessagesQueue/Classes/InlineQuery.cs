using Core.Bot.MessagesQueue.Interfaces;

using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.ReplyMarkups;

namespace Core.Bot.MessagesQueue.Classes {
    public class InlineQuery(string inlineQueryId, IEnumerable<InlineQueryResult> results, int? cacheTime, bool? isPersonal) : IMessageQueue {
        public ChatId ChatId { get; } = 0;
        public string InlineQueryId { get; } = inlineQueryId;
        public IEnumerable<InlineQueryResult> Results { get; } = results;
        public int? CacheTime { get; } = cacheTime;
        public bool? IsPersonal { get; } = isPersonal;
    }
}
