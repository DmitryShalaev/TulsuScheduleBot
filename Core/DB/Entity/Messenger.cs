using System.ComponentModel.DataAnnotations.Schema;

namespace Core.DB.Entity {
#pragma warning disable CS8618
    public class Messenger {
        public long ID { get; set; }

        [ForeignKey("TelegramUser")]
        public long From { get; set; }
        public TelegramUser TelegramUser { get; set; }

        public long? Previous { get; set; }
        [ForeignKey("Previous")]
        public Messenger PreviousMessenger { get; set; }

        public long? Following { get; set; }
        [ForeignKey("Following")]
        public Messenger FollowingMessenger { get; set; }

        public long? FeedbackID { get; set; }
        [ForeignKey("FeedbackID")]
        public Feedback Feedback { get; set; }

        public string Message { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}
