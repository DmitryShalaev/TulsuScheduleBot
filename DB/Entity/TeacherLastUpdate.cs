using System.ComponentModel.DataAnnotations;

namespace ScheduleBot.DB.Entity {

#pragma warning disable CS8618
    public class TeacherLastUpdate {
        [Key]
        public string Teacher { get; set; }

        public DateTime Update { get; set; }
    }
}
