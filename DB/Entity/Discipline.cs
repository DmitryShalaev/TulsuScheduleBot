using Newtonsoft.Json.Linq;

namespace ScheduleBot.DB.Entity {

#pragma warning disable CS8618
    public class Discipline : IEquatable<Discipline?> {
        public long Id { get; set; }

        public string Name { get; set; }
        public string? Lecturer { get; set; } = null;
        public string LectureHall { get; set; }
        public string? Subgroup { get; set; } = null;
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public Type Class { get; set; }
        public string Type { get; set; }

        public bool IsCompleted { get; set; } = false;
        public bool IsCastom { get; set; } = false;

        public Discipline() { }

        public Discipline(JToken json) {
            LectureHall = json.Value<string>("AUD") ?? throw new NullReferenceException("AUD");
            Date = DateOnly.Parse(json.Value<string>("DATE_Z") ?? throw new NullReferenceException("DATE_Z"));
            Name = json.Value<string>("DISCIP") ?? throw new NullReferenceException("DISCIP");
            Type = json.Value<string>("KOW") ?? throw new NullReferenceException("KOW");

            var _subgroup = json.Value<JToken>("GROUPS")?[0]?.Value<string>("PRIM");
            Subgroup = string.IsNullOrEmpty(_subgroup) ? null : _subgroup;

            Lecturer = json.Value<string?>("PREP");

            Class = (Type)Enum.Parse(typeof(Type), json.Value<string>("CLASS") ?? "other");

            var times = (json.Value<string>("TIME_Z") ?? throw new NullReferenceException("TIME_Z")).Split('-');
            StartTime = TimeOnly.Parse(times[0]);
            EndTime = TimeOnly.Parse(times[1]);
        }

        public override bool Equals(object? obj) => Equals(obj as Discipline);
        public bool Equals(Discipline? discipline) => discipline is not null && Name == discipline.Name && Lecturer == discipline.Lecturer && LectureHall == discipline.LectureHall && Subgroup == discipline.Subgroup && Date.Equals(discipline.Date) && StartTime.Equals(discipline.StartTime) && EndTime.Equals(discipline.EndTime) && Class == discipline.Class;

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

            return hash.GetHashCode();
        }

        public static implicit operator CompletedDiscipline(Discipline discipline) => new() { Name = discipline.Name, Class = discipline.Class, Lecturer = discipline.Lecturer, Subgroup = discipline.Subgroup };
    }
}
