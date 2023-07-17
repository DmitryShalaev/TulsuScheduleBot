using System.ComponentModel.DataAnnotations;

namespace ScheduleBot.DB.Entity {

#pragma warning disable CS8618
    public class GroupLastUpdate {
        [Key]
        public string Group { get; set; }

        public DateTime Update { get; set; }
    }
}
