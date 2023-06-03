using System.ComponentModel.DataAnnotations.Schema;

using Newtonsoft.Json.Linq;

namespace ScheduleBot.DB.Entity {

#pragma warning disable CS8618
    public class Discipline : IEquatable<Discipline?> {
        public long ID { get; set; }

        public string Name { get; set; }
        public string? Lecturer { get; set; } = null;
        public string LectureHall { get; set; }
        public string? Subgroup { get; set; } = null;
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Group { get; set; }

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

            var _subgroup = json.Value<JToken>("GROUPS")?[0]?.Value<string>("PRIM");
            Subgroup = string.IsNullOrWhiteSpace(_subgroup) ? null : _subgroup;

            Lecturer = json.Value<string?>("PREP");

            Class = (Class)Enum.Parse(typeof(Class), json.Value<string>("CLASS") ?? "other");

            var times = (json.Value<string>("TIME_Z") ?? throw new NullReferenceException("TIME_Z")).Split('-');
            StartTime = TimeOnly.Parse(times[0]);
            EndTime = TimeOnly.Parse(times[1]);

            Group = group;
        }

        public Discipline(CustomDiscipline discipline) {
            Name = discipline.Name;
            Class = Entity.Class.other;
            Lecturer = discipline.Lecturer;
            LectureHall = discipline.LectureHall;
            StartTime = discipline.StartTime;
            EndTime = discipline.EndTime;
            Date = discipline.Date;
            Type = discipline.Type;
        }

        public override bool Equals(object? obj) => Equals(obj as Discipline);
        public bool Equals(Discipline? discipline) => discipline is not null && Name == discipline.Name && Lecturer == discipline.Lecturer && Subgroup == discipline.Subgroup && Date.Equals(discipline.Date) && StartTime.Equals(discipline.StartTime) && EndTime.Equals(discipline.EndTime) && Class == discipline.Class && Group == discipline.Group;

        public static bool operator ==(Discipline? left, Discipline? right) => left?.Equals(right) ?? false;
        public static bool operator !=(Discipline? left, Discipline? right) => !(left == right);

        public override int GetHashCode() {
            int hash = 17;

            hash += Name?.GetHashCode() ?? 0;
            hash += Lecturer?.GetHashCode() ?? 0;
            hash += LectureHall.GetHashCode();
            hash += Subgroup?.GetHashCode() ?? 0;
            hash += Date.GetHashCode();
            hash += StartTime.GetHashCode();
            hash += EndTime.GetHashCode();
            hash += Class.GetHashCode();
            hash += Group.GetHashCode();

            return hash.GetHashCode();
        }

        public static explicit operator CompletedDiscipline(Discipline discipline) => new() { Name = discipline.Name, Class = discipline.Class, Lecturer = discipline.Lecturer, Subgroup = discipline.Subgroup, Date = discipline.Date };
    }

    public enum Class : byte {
        all,
        lab,
        practice,
        lecture,
        other,
        custom
    }

    public class ClassDTO {
        public Class ID { get; set; }
        public string Name { get; set; }
    }
}
