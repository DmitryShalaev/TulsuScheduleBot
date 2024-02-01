using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScheduleBot.DB.Entity {
#pragma warning disable CS8618

    public class TelegramUserTmp {
        [Key]
        [ForeignKey("TelegramUser")]
        public long OwnerID { get; set; }
        public TelegramUser? TelegramUser { get; set; }

        [ForeignKey("ModeDTO")]
        public Mode Mode { get; set; } = Mode.Default;
        public ModeDTO ModeDTO { get; set; }

        public int? RequestingMessageID { get; set; }
        public string? TmpData { get; set; }
    }
}