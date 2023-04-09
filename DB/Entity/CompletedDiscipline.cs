namespace ScheduleBot.DB.Entity {

    public enum Type : byte {
        all,
        lab,
        practice,
        lecture,
        other
    }

#pragma warning disable CS8618
    public class CompletedDiscipline : IEquatable<CompletedDiscipline?> {
        public int ID { get; set; }
        public string Name { get; set; }
        public Type Class { get; set; }

        public override bool Equals(object? obj) => Equals(obj as CompletedDiscipline);
        public bool Equals(CompletedDiscipline? discipline) => discipline is not null && Name == discipline.Name && (Class == discipline.Class || Class == Type.all) && discipline.Class != Type.other;

        public static bool operator ==(CompletedDiscipline? left, CompletedDiscipline? right) => left?.Equals(right) ?? false;
        public static bool operator !=(CompletedDiscipline? left, CompletedDiscipline? right) => !(left == right);

        public override int GetHashCode() {
            int hash = 17;

            hash += Name?.GetHashCode() ?? 0;
            hash += Class.GetHashCode();

            return hash.GetHashCode();
        }
    }

    public class TypeDTO {
        public Type Id { get; set; }
        public string Name { get; set; }
    }
}
