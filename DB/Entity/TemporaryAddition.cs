using System.ComponentModel.DataAnnotations.Schema;

namespace ScheduleBot.DB.Entity {

#pragma warning disable CS8618
    public class TemporaryAddition {
        public long ID { get; set; }

        [ForeignKey("TelegramUser")]
        public long User { get; set; }
        public TelegramUser TelegramUser { get; set; }

        public DateTime AddDate { get; set; } = DateTime.UtcNow;

        public int Counter { get; set; } = 0;

        public string? Name { get; set; }
        public string? Lecturer { get; set; }
        public string? LectureHall { get; set; }
        public string? Type { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }

        public TemporaryAddition() { }

        public TemporaryAddition(TelegramUser telegramUser, DateOnly date) {
            TelegramUser = telegramUser;
            Date = date;
        }
    }
}


