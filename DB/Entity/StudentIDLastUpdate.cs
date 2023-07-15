using System.ComponentModel.DataAnnotations;

namespace ScheduleBot.DB.Entity
{

#pragma warning disable CS8618
    public class StudentIDLastUpdate
    {
        [Key]
        public string StudentID { get; set; }

        public DateTime Update { get; set; }
    }
}
