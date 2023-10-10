using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScheduleBot.DB.Entity {
    public class Notifications {
        public bool IsEnabled { get; set; } = false;

        public int Days { get; set; } = 7;

        [Key]
        [ForeignKey("TelegramUser")]
        public long OwnerID { get; set; }
        public TelegramUser? TelegramUser { get; set; }
    }
}
