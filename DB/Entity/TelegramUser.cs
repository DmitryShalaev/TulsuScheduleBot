using System.ComponentModel.DataAnnotations;

namespace ScheduleBot.DB.Entity {
#pragma warning disable CS8618
    public class TelegramUser : IEquatable<TelegramUser?> {
        [Key]
        public long ChatId { get; set; }
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }
        public bool IsAdmin { get; set; } = false;

        public override bool Equals(object? obj) => Equals(obj as TelegramUser);
        public bool Equals(TelegramUser? user) => user is not null && ChatId == user.ChatId && FirstName == user.FirstName && Username == user.Username && LastName == user.LastName;
        public static bool operator ==(TelegramUser? left, TelegramUser? right) => left?.Equals(right) ?? false;
        public static bool operator !=(TelegramUser? left, TelegramUser? right) => !(left == right);
        public override int GetHashCode() {
            int hash = 17;

            hash += ChatId.GetHashCode();
            hash += FirstName.GetHashCode();
            hash += Username?.GetHashCode() ?? 0;
            hash += LastName?.GetHashCode() ?? 0;

            return hash.GetHashCode();
        }

    }
}
