namespace Core.DB.Entity {
#pragma warning disable CS8618
    public class Feedback {
        public long ID { get; set; }

        public TelegramUser TelegramUser { get; set; }

        public string Message { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public bool IsCompleted { get; set; } = false;
    }
}
