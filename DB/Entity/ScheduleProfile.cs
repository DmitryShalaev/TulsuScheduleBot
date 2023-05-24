using System.ComponentModel.DataAnnotations;

namespace ScheduleBot.DB.Entity {

    public class ScheduleProfile {
        [Key]
        public Guid ID { get; set; }

        public long OwnerID { get; set; }

        public string? Group { get; set; }
        public string? StudentID { get; set; }
    }
}
