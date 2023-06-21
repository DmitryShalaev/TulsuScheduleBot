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

        public string? CurrentPath { get; set; }

        public DateTime LastAppeal { get; set; }
        public long TotalRequests { get; set; } = 0;
        public long TodayRequests { get; set; } = 0;

        [ForeignKey("ScheduleProfile")]
        public Guid ScheduleProfileGuid { get; set; }
        public ScheduleProfile ScheduleProfile { get; set; }

        [ForeignKey("Notifications")]
        public long NotificationsID { get; set; }
        public Notifications Notifications { get; set; }

        [ForeignKey("ModeDTO")]
        public Mode Mode { get; set; } = Mode.Default;
        public ModeDTO ModeDTO { get; set; }

        public override bool Equals(object? obj) => Equals(obj as TelegramUser);
        public bool Equals(TelegramUser? user) => user is not null && ChatID == user.ChatID;
        public static bool operator ==(TelegramUser? left, TelegramUser? right) => left?.Equals(right) ?? false;
        public static bool operator !=(TelegramUser? left, TelegramUser? right) => !(left == right);
        public override int GetHashCode() => ChatID.GetHashCode();

        public bool IsAdmin() => ChatID == ScheduleProfile.OwnerID;

        public TelegramUser() { }

        public TelegramUser(TelegramUser telegramUser) {
            ChatID = telegramUser.ChatID;
            ScheduleProfile = telegramUser.ScheduleProfile;
            Notifications = telegramUser.Notifications;
        }
    }

    public enum Mode : byte {
        Default,
        AddingDiscipline,
        GroupСhange,
        StudentIDСhange,
        ResetProfileLink,
        CustomEditName,
        CustomEditLecturer,
        CustomEditType,
        CustomEditLectureHall,
        CustomEditStartTime,
        CustomEditEndTime,
        DaysNotifications
    }

    public class ModeDTO {
        public Mode ID { get; set; }
        public string Name { get; set; }
    }
}
