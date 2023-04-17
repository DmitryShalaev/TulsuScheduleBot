using Newtonsoft.Json.Linq;

namespace ScheduleBot.DB.Entity {

#pragma warning disable CS8618
    public class Progress : IEquatable<Progress?> {
        public long ID { get; set; }
        public string Discipline { get; set; }
        public string? MarkTitle { get; set; }
        public int? Mark { get; set; }
        public int Term { get; set; }

        public Progress() { }

        public Progress(JToken json) {
            Discipline = json.Value<string>("DISCIPLINE") ?? throw new NullReferenceException("DISCIPLINE");

            Mark = json.Value<int?>("MARK");

            Term = json.Value<int>("TERM");

            MarkTitle = json.Value<string>("MARK_TITLE");
        }

        public override bool Equals(object? obj) => Equals(obj as Progress);
        public bool Equals(Progress? progress) => progress is not null && Discipline == progress.Discipline && MarkTitle == progress.MarkTitle && Mark == progress.Mark && Term == progress.Term;

        public static bool operator ==(Progress? left, Progress? right) => left?.Equals(right) ?? false;
        public static bool operator !=(Progress? left, Progress? right) => !(left == right);

        public override int GetHashCode() {
            int hash = 17;

            hash += Discipline.GetHashCode();
            hash += MarkTitle?.GetHashCode() ?? 0;
            hash += Mark?.GetHashCode() ?? 0;
            hash += Term.GetHashCode();

            return hash.GetHashCode();
        }
    }
}
