namespace Core.Bot {
    public class UserActivityTracker {
        private readonly Dictionary<long, Queue<DateTime>> userMessageQueue = [];
        private readonly int maxMessagesPerSecond = 3;

        public bool IsAllowed(long userId) {
            if(!userMessageQueue.ContainsKey(userId))
                userMessageQueue[userId] = new Queue<DateTime>();

            Queue<DateTime> userQueue = userMessageQueue[userId];
            DateTime currentTime = DateTime.UtcNow;

            while(userQueue.Count > 0 && (currentTime - userQueue.Peek()).TotalSeconds >= 1)
                userQueue.Dequeue();

            if(userQueue.Count >= maxMessagesPerSecond)
                return false;

            userQueue.Enqueue(currentTime);
            return true;
        }
    }
}
