using System.ComponentModel.DataAnnotations;

namespace Core.DB.Entity {

#pragma warning disable CS8618
    public class ClassroomLastUpdate {
        [Key]
        public string Classroom { get; set; }

        public DateTime Update { get; set; }

        public DateTime UpdateAttempt { get; set; }
    }
}
