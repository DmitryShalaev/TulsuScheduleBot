using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json.Linq;

namespace Core.DB.Entity {

#pragma warning disable CS8618
    [Index(nameof(Date))]
    public class Discipline : IEquatable<Discipline?> {
        public long ID { get; set; }

        public string Name { get; set; }

        [ForeignKey("TeacherLastUpdate")]
        public string? Lecturer { get; set; }
        public TeacherLastUpdate TeacherLastUpdate { get; set; }

        [ForeignKey("ClassroomLastUpdate")]
        public string? LectureHall { get; set; }
        public ClassroomLastUpdate ClassroomLastUpdate { get; set; }

        public string? Subgroup { get; set; } = null;
        public string? IntersectionMark { get; set; } = null;

        public DateOnly Date { get; set; }

        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        [ForeignKey("GroupLastUpdate")]
        public string Group { get; set; }
        public GroupLastUpdate GroupLastUpdate { get; set; }

        public string Type { get; set; }

        [ForeignKey("ClassDTO")]
        public Class Class { get; set; }
        public ClassDTO ClassDTO { get; set; }

        public Discipline() { }

        public Discipline(JToken json, string group) {
            LectureHall = json.Value<string>("AUD") ?? throw new NullReferenceException("AUD");
            Date = DateOnly.Parse(json.Value<string>("DATE_Z") ?? throw new NullReferenceException("DATE_Z"));
            Name = json.Value<string>("DISCIP") ?? throw new NullReferenceException("DISCIP");
            Type = json.Value<string>("KOW") ?? throw new NullReferenceException("KOW");

            string? _subgroup = json.Value<JToken>("GROUPS")?[0]?.Value<string>("PRIM");
            Subgroup = string.IsNullOrWhiteSpace(_subgroup) ? null : _subgroup;

            Lecturer = json.Value<string?>("PREP");

            Class = (Class)Enum.Parse(typeof(Class), (json.Value<string>("CLASS") ?? "other").Replace("default", "def"));

            string[] times = (json.Value<string>("TIME_Z") ?? throw new NullReferenceException("TIME_Z")).Split('-');
            StartTime = TimeOnly.Parse(times[0]);
            EndTime = TimeOnly.Parse(times[1]);

            Group = group;
        }

        public Discipline(CustomDiscipline discipline) {
            Name = discipline.Name ?? throw new NullReferenceException("Name");
            Class = Class.other;
            Lecturer = discipline.Lecturer?.Trim();
            LectureHall = discipline.LectureHall ?? throw new NullReferenceException("LectureHall");
            StartTime = discipline.StartTime ?? throw new NullReferenceException("StartTime");
            EndTime = discipline.EndTime ?? throw new NullReferenceException("EndTime");
            Date = discipline.Date;
            Type = discipline.Type ?? throw new NullReferenceException("Type");
        }

        public Discipline(Discipline discipline) {
            Name = discipline.Name;
            Lecturer = discipline.Lecturer;
            TeacherLastUpdate = discipline.TeacherLastUpdate;
            LectureHall = discipline.LectureHall;
            Subgroup = discipline.Subgroup;
            Date = discipline.Date;
            StartTime = discipline.StartTime;
            EndTime = discipline.EndTime;
            Group = discipline.Group;
            GroupLastUpdate = discipline.GroupLastUpdate;
            Type = discipline.Type;
            Class = discipline.Class;
            ClassDTO = discipline.ClassDTO;
        }

        public Discipline(DeletedDisciplines discipline) {
            Name = discipline.Name;
            Lecturer = discipline.Lecturer;
            TeacherLastUpdate = discipline.TeacherLastUpdate;
            LectureHall = discipline.LectureHall;
            Subgroup = discipline.Subgroup;
            Date = discipline.Date;
            StartTime = discipline.StartTime;
            EndTime = discipline.EndTime;
            Group = discipline.Group;
            GroupLastUpdate = discipline.GroupLastUpdate;
            Type = discipline.Type;
            Class = discipline.Class;
            ClassDTO = discipline.ClassDTO;
        }

        public override bool Equals(object? obj) => Equals(obj as Discipline);
        public bool Equals(Discipline? discipline) => discipline is not null && Name == discipline.Name && Lecturer == discipline.Lecturer && Subgroup == discipline.Subgroup && Date.Equals(discipline.Date) && StartTime.Equals(discipline.StartTime) && EndTime.Equals(discipline.EndTime) && Class == discipline.Class && Group == discipline.Group;

        public static bool operator ==(Discipline? left, Discipline? right) => left?.Equals(right) ?? false;
        public static bool operator !=(Discipline? left, Discipline? right) => !(left == right);

        public override int GetHashCode() {
            int hash = 17;

            hash += Name?.GetHashCode() ?? 0;
            hash += Lecturer?.GetHashCode() ?? 0;
            hash += LectureHall?.GetHashCode() ?? 0;
            hash += Subgroup?.GetHashCode() ?? 0;
            hash += Date.GetHashCode();
            hash += StartTime.GetHashCode();
            hash += EndTime.GetHashCode();
            hash += Class.GetHashCode();
            hash += Group.GetHashCode();

            return hash.GetHashCode();
        }

        public static explicit operator CompletedDiscipline(Discipline discipline) => new() { Name = discipline.Name, Class = discipline.Class, Lecturer = discipline.Lecturer, Subgroup = discipline.Subgroup, Date = discipline.Date, IntersectionMark = discipline.IntersectionMark };
    }

    public enum Class : byte {
        all,
        lab,
        practice,
        lecture,
        other,
        custom,
        def
    }

    public class ClassDTO {
        public Class ID { get; set; }
        public string Name { get; set; }
    }
}
