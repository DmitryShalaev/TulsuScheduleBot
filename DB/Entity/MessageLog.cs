using System.ComponentModel.DataAnnotations.Schema;

namespace ScheduleBot.DB.Entity {
#pragma warning disable CS8618
    public class MessageLog {
        public long ID { get; set; }

        public TelegramUser TelegramUser { get; set; }

        public string Message { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}
