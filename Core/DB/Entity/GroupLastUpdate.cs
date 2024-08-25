using System.ComponentModel.DataAnnotations;

namespace Core.DB.Entity {

#pragma warning disable CS8618
    public class GroupLastUpdate {
        [Key]
        public string Group { get; set; }

        public DateTime Update { get; set; }

        public DateTime UpdateAttempt { get; set; }
    }
}
