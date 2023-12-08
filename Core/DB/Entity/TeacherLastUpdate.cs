using System.ComponentModel.DataAnnotations;

namespace ScheduleBot.DB.Entity {

#pragma warning disable CS8618
    public class TeacherLastUpdate {
        [Key]
        public string Teacher { get; set; }

        public string? LinkProfile { get; set; }

        public DateTime Update { get; set; }

        public DateTime UpdateAttempt { get; set; }
    }
}
