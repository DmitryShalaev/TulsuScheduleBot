using System.ComponentModel.DataAnnotations.Schema;

namespace ScheduleBot.DB.Entity
{

#pragma warning disable CS8618
    public class CustomDiscipline : IEquatable<CustomDiscipline?>
    {
        public long ID { get; set; }

        public DateTime AddDate { get; set; } = DateTime.UtcNow;

        public int Counter { get; set; } = 0;
        public bool IsAdded { get; set; } = false;

        public string? Name { get; set; }
        public string? Lecturer { get; set; }
        public string? LectureHall { get; set; }
        public string? Type { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }

        [ForeignKey("ScheduleProfile")]
        public Guid ScheduleProfileGuid { get; set; }
        public ScheduleProfile ScheduleProfile { get; set; }

        public CustomDiscipline() { }

        public CustomDiscipline(ScheduleProfile scheduleProfile, DateOnly date)
        {
            ScheduleProfile = scheduleProfile;
            Date = date;
        }

        public override bool Equals(object? obj) => Equals(obj as CustomDiscipline);
        public bool Equals(CustomDiscipline? discipline) => discipline is not null && Name == discipline.Name && Lecturer == discipline.Lecturer && Date.Equals(discipline.Date) && StartTime.Equals(discipline.StartTime) && EndTime.Equals(discipline.EndTime) && ScheduleProfileGuid == discipline.ScheduleProfileGuid;

        public static bool operator ==(CustomDiscipline? left, CustomDiscipline? right) => left?.Equals(right) ?? false;
        public static bool operator !=(CustomDiscipline? left, CustomDiscipline? right) => !(left == right);

        public override int GetHashCode()
        {
            int hash = 17;

            hash += Name?.GetHashCode() ?? 0;
            hash += Lecturer?.GetHashCode() ?? 0;
            hash += LectureHall?.GetHashCode() ?? 0;
            hash += Date.GetHashCode();
            hash += StartTime.GetHashCode();
            hash += EndTime.GetHashCode();
            hash += ScheduleProfileGuid.GetHashCode();

            return hash.GetHashCode();
        }
    }
}
