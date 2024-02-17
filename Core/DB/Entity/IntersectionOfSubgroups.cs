using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

using ScheduleBot.DB.Entity;

namespace Core.DB.Entity {
#pragma warning disable CS8618

    [Index(nameof(IntersectionWith), IsUnique = true)]
    public class IntersectionOfSubgroups {
        [Key]
        public string Group { get; set; }

        public string IntersectionWith { get; set; }

        [ForeignKey("ClassDTO")]
        public Class Class { get; set; }
        public ClassDTO ClassDTO { get; set; }

        public string Mark { get; set; }
    }
}
