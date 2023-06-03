using System.ComponentModel.DataAnnotations.Schema;

namespace ScheduleBot.DB.Entity {

#pragma warning disable CS8618
    public class CompletedDiscipline : IEquatable<CompletedDiscipline?> {
        public long ID { get; set; }

        public string Name { get; set; }
        public string? Lecturer { get; set; }
        public string? Subgroup { get; set; }

        public DateOnly? Date { get; set; } = null;

        [ForeignKey("ScheduleProfile")]
        public Guid ScheduleProfileGuid { get; set; }
        public ScheduleProfile ScheduleProfile { get; set; }

        [ForeignKey("ClassDTO")]
        public Class Class { get; set; }
        public ClassDTO ClassDTO { get; set; }

        public override bool Equals(object? obj) => Equals(obj as CompletedDiscipline);
        public bool Equals(CompletedDiscipline? discipline) => discipline is not null && Name == discipline.Name && Class == discipline.Class && Lecturer == discipline.Lecturer && Subgroup == discipline.Subgroup && (Date == null || Date.Equals(discipline.Date));

        public static bool operator ==(CompletedDiscipline? left, CompletedDiscipline? right) => left?.Equals(right) ?? false;
        public static bool operator !=(CompletedDiscipline? left, CompletedDiscipline? right) => !(left == right);

        public override int GetHashCode() {
            int hash = 17;

            hash += Name?.GetHashCode() ?? 0;
            hash += Class.GetHashCode();
            hash += Lecturer?.GetHashCode() ?? 0;
            hash += Subgroup?.GetHashCode() ?? 0;
            hash += ScheduleProfileGuid.GetHashCode();
            hash += Date.GetHashCode();

            return hash.GetHashCode();
        }

        public CompletedDiscipline() { }

        public CompletedDiscipline(Discipline discipline, Guid scheduleProfileGuid) {
            Name = discipline.Name;
            Lecturer = discipline.Lecturer;
            Class = discipline.Class;
            Subgroup = discipline.Subgroup;
            Date = discipline.Date;
            ScheduleProfileGuid = scheduleProfileGuid;
        }
    }
}
