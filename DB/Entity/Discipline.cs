using Newtonsoft.Json.Linq;

namespace ScheduleBot.DB.Entity {

#pragma warning disable CS8618
    public class Discipline {
        public int Id { get; set; }

        public string Name { get; set; }
        public string? Lecturer { get; set; }
        public string LectureHall { get; set; }
        public string? Subgroup { get; set; } = null;
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public Discipline(JToken json) {
            LectureHall = json.Value<string>("AUD") ?? throw new NullReferenceException("AUD");
            Date = DateOnly.Parse(json.Value<string>("DATE_Z") ?? throw new NullReferenceException("DATE_Z"));
            Name = json.Value<string>("DISCIP") ?? throw new NullReferenceException("DISCIP");
            Subgroup = json.Value<JToken>("GROUPS")?[0]?.Value<string>("PRIM") ?? null;
            Lecturer = json.Value<string>("PREP");

            var times = (json.Value<string>("TIME_Z") ?? throw new NullReferenceException("TIME_Z")).Split('-');
            StartTime = TimeOnly.Parse(times[0]);
            EndTime = TimeOnly.Parse(times[1]);
        }
    }
}
