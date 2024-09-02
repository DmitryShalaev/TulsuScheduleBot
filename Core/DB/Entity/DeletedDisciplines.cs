using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace Core.DB.Entity {

#pragma warning disable CS8618
    [Index(nameof(Date))]
    public class DeletedDisciplines {
        public long ID { get; set; }

        public string Name { get; set; }

        [ForeignKey("TeacherLastUpdate")]
        public string? Lecturer { get; set; }
        public TeacherLastUpdate TeacherLastUpdate { get; set; }

        public string? LectureHall { get; set; }
        public string? Subgroup { get; set; } = null;
        public string? IntersectionMark { get; set; } = null;

        public DateOnly Date { get; set; }
        public DateOnly DeleteDate { get; set; }

        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        [ForeignKey("GroupLastUpdate")]
        public string Group { get; set; }
        public GroupLastUpdate GroupLastUpdate { get; set; }

        public string Type { get; set; }

        [ForeignKey("ClassDTO")]
        public Class Class { get; set; }
        public ClassDTO ClassDTO { get; set; }

        public DeletedDisciplines() { }

        public DeletedDisciplines(Discipline discipline) {
            Name = discipline.Name;
            Lecturer = discipline.Lecturer;
            TeacherLastUpdate = discipline.TeacherLastUpdate;
            LectureHall = discipline.LectureHall;
            Subgroup = discipline.Subgroup;
            IntersectionMark = discipline.IntersectionMark;
            Date = discipline.Date;
            StartTime = discipline.StartTime;
            EndTime = discipline.EndTime;
            Group = discipline.Group;
            GroupLastUpdate = discipline.GroupLastUpdate;
            Type = discipline.Type;
            ClassDTO = discipline.ClassDTO;

            DeleteDate = DateOnly.FromDateTime(DateTime.UtcNow);
        }
    }
}
