using System.ComponentModel.DataAnnotations.Schema;

namespace ScheduleBot.DB.Entity {
    public class Notifications {
        public long ID { get; set; }

        public TimeOnly? DNDStart { get; set; }
        public TimeOnly? DNDStop { get; set; }

        public int Days { get; set; }

        [ForeignKey("TelegramUser")]
        public long? OwnerID { get; set; }
        public TelegramUser? TelegramUser { get; set; }
    }
}
