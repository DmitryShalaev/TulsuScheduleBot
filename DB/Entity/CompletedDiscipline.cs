namespace ScheduleBot.DB.Entity {

#pragma warning disable CS8618
    public class CompletedDiscipline : IEquatable<CompletedDiscipline?> {
        public long ID { get; set; }
        public string Name { get; set; }
        public string? Lecturer { get; set; }
        public Type Class { get; set; }
        public string? Subgroup { get; set; }

        public override bool Equals(object? obj) => Equals(obj as CompletedDiscipline);
        public bool Equals(CompletedDiscipline? discipline) => discipline is not null && Name == discipline.Name && Class == discipline.Class && discipline.Class != Type.other && Lecturer == discipline.Lecturer && Subgroup == discipline.Subgroup;

        public static bool operator ==(CompletedDiscipline? left, CompletedDiscipline? right) => left?.Equals(right) ?? false;
        public static bool operator !=(CompletedDiscipline? left, CompletedDiscipline? right) => !(left == right);

        public override int GetHashCode() {
            int hash = 17;

            hash += Name?.GetHashCode() ?? 0;
            hash += Class.GetHashCode();
            hash += Lecturer?.GetHashCode() ?? 0;
            hash += Subgroup?.GetHashCode() ?? 0;

            return hash.GetHashCode();
        }
    }

    public enum Type : byte {
        all,
        lab,
        practice,
        lecture,
        other
    }

    public class TypeDTO {
        public Type Id { get; set; }
        public string Name { get; set; }
    }
}
