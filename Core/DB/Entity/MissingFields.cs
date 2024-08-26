using System.ComponentModel.DataAnnotations;

namespace Core.DB.Entity {
#pragma warning disable CS8618
    public class MissingFields {
        [Key]
        public string Field { get; set; }
    }
}
