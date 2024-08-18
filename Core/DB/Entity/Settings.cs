using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScheduleBot.DB.Entity {
    public class Settings {
        public bool NotificationEnabled { get; set; } = true;
        public bool DisplayingGroupList { get; set; } = true;
        public int NotificationDays { get; set; } = 7;

        public bool TeacherLincsEnabled { get; set; } = true;

        [Key]
        [ForeignKey("TelegramUser")]
        public long OwnerID { get; set; }
        public TelegramUser? TelegramUser { get; set; }
    }
}
