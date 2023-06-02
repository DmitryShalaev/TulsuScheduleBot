using System.ComponentModel.DataAnnotations.Schema;

namespace ScheduleBot.DB.Entity {

#pragma warning disable CS8618
    public class CustomDiscipline : IEquatable<CustomDiscipline?> {
        public long ID { get; set; }

        public string Name { get; set; }
        public string? Lecturer { get; set; } = null;
        public string LectureHall { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public string Type { get; set; }

        [ForeignKey("ScheduleProfile")]
        public Guid ScheduleProfileGuid { get; set; }
        public ScheduleProfile ScheduleProfile { get; set; }

        [ForeignKey("ClassDTO")]
        public Class Class { get; set; }
        public ClassDTO ClassDTO { get; set; }

        public CustomDiscipline() { }

        public CustomDiscipline(TemporaryAddition discipline, Guid scheduleProfileGuid) {
            Name = discipline.Name ?? throw new NullReferenceException("Name");
            Class = Entity.Class.other;
            Lecturer = discipline.Lecturer;
            LectureHall = discipline.LectureHall ?? throw new NullReferenceException("LectureHall");
            StartTime = discipline.StartTime ?? throw new NullReferenceException("StartTime");
            EndTime = discipline.EndTime ?? throw new NullReferenceException("EndTime");
            Date = discipline.Date;
            Type = discipline.Type ?? throw new NullReferenceException("Type");

            ScheduleProfileGuid = scheduleProfileGuid;
        }

        public override bool Equals(object? obj) => Equals(obj as CustomDiscipline);
        public bool Equals(CustomDiscipline? discipline) => discipline is not null && Name == discipline.Name && Lecturer == discipline.Lecturer && Date.Equals(discipline.Date) && StartTime.Equals(discipline.StartTime) && EndTime.Equals(discipline.EndTime) && Class == discipline.Class && ScheduleProfileGuid == discipline.ScheduleProfileGuid;

        public static bool operator ==(CustomDiscipline? left, CustomDiscipline? right) => left?.Equals(right) ?? false;
        public static bool operator !=(CustomDiscipline? left, CustomDiscipline? right) => !(left == right);

        public override int GetHashCode() {
            int hash = 17;

            hash += Name?.GetHashCode() ?? 0;
            hash += Lecturer?.GetHashCode() ?? 0;
            hash += LectureHall.GetHashCode();
            hash += Date.GetHashCode();
            hash += StartTime.GetHashCode();
            hash += EndTime.GetHashCode();
            hash += Class.GetHashCode();
            hash += ScheduleProfileGuid.GetHashCode();

            return hash.GetHashCode();
        }
    }
}
