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

        public DateTime LastAppeal { get; set; } = DateTime.UtcNow;
        public DateTime? DateOfRegistration { get; set; }

        public long TotalRequests { get; set; } = 0;
        public long TodayRequests { get; set; } = 0;

        public bool IsAdmin { get; set; } = false;

        [ForeignKey("ScheduleProfile")]
        public Guid ScheduleProfileGuid { get; set; }
        public ScheduleProfile ScheduleProfile { get; set; }

        public Settings Settings { get; set; }

        public TelegramUsersTmp TelegramUserTmp { get; set; }

        public override bool Equals(object? obj) => Equals(obj as TelegramUser);
        public bool Equals(TelegramUser? user) => user is not null && ChatID == user.ChatID;
        public static bool operator ==(TelegramUser? left, TelegramUser? right) => left?.Equals(right) ?? false;
        public static bool operator !=(TelegramUser? left, TelegramUser? right) => !(left == right);
        public override int GetHashCode() => ChatID.GetHashCode();

        public bool IsOwner() => ChatID == ScheduleProfile.OwnerID;

        public TelegramUser() { }

        public TelegramUser(TelegramUser telegramUser) {
            ChatID = telegramUser.ChatID;
            ScheduleProfile = telegramUser.ScheduleProfile;
            Settings = telegramUser.Settings;
            TelegramUserTmp = telegramUser.TelegramUserTmp;
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
        DaysNotifications,

        TeachersWorkSchedule,
        TeacherSelected,

        Feedback
    }

    public class ModeDTO {
        public Mode ID { get; set; }
        public string Name { get; set; }
    }
}
