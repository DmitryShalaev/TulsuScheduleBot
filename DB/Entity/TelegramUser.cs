using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScheduleBot.DB.Entity {
#pragma warning disable CS8618
    public class TelegramUser : IEquatable<TelegramUser?> {
        [Key]
        public long ChatID { get; set; }

        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }
        public bool IsAdmin { get; set; } = false;

        [ForeignKey("ModeDTO")]
        public Mode Mode { get; set; } = Mode.Default;
        public ModeDTO ModeDTO { get; set; }

        public override bool Equals(object? obj) => Equals(obj as TelegramUser);
        public bool Equals(TelegramUser? user) => user is not null && ChatID == user.ChatID;
        public static bool operator ==(TelegramUser? left, TelegramUser? right) => left?.Equals(right) ?? false;
        public static bool operator !=(TelegramUser? left, TelegramUser? right) => !(left == right);
        public override int GetHashCode() => ChatID.GetHashCode();
    }

    public enum Mode : byte {
        Default,
        AddingDiscipline
    }

    public class ModeDTO {
        public Mode ID { get; set; }
        public string Name { get; set; }
    }
}
