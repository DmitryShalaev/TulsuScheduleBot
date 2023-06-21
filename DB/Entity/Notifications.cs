using System.ComponentModel.DataAnnotations.Schema;

namespace ScheduleBot.DB.Entity {
    public class Notifications {
        public long ID { get; set; }

        public bool IsEnabled { get; set; } = false;

        public TimeOnly? DNDStart { get; set; }
        public TimeOnly? DNDStop { get; set; }

        public int Days { get; set; } = 7;

        [ForeignKey("TelegramUser")]
        public long? OwnerID { get; set; }
        public TelegramUser? TelegramUser { get; set; }
    }
}
