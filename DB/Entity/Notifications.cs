using System.ComponentModel.DataAnnotations.Schema;

namespace ScheduleBot.DB.Entity
{
    public class Notifications
    {
        public long ID { get; set; }

        public bool IsEnabled { get; set; } = false;

        public int Days { get; set; } = 7;

        [ForeignKey("TelegramUser")]
        public long? OwnerID { get; set; }
        public TelegramUser? TelegramUser { get; set; }
    }
}
