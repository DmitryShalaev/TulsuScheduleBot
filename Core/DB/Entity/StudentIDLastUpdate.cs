using System.ComponentModel.DataAnnotations;

namespace Core.DB.Entity {

#pragma warning disable CS8618
    public class StudentIDLastUpdate {
        [Key]
        public string StudentID { get; set; }

        public DateTime Update { get; set; }

        public DateTime UpdateAttempt { get; set; }
    }
}
