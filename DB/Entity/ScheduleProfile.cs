namespace ScheduleBot.DB.Entity {

    public class ScheduleProfile {
        public Guid ID { get; set; }

        public long OwnerID { get; set; }

        public string? Group { get; set; }
        public string? StudentID { get; set; }

        public DateTime LastAppeal { get; set; }
    }
}
