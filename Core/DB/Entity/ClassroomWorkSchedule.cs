using System.ComponentModel.DataAnnotations.Schema;

using Newtonsoft.Json.Linq;

using ScheduleBot.DB.Entity;

namespace Core.DB.Entity {

#pragma warning disable CS8618

    public class ClassroomWorkSchedule {
        public long ID { get; set; }

        public string Name { get; set; }

        [ForeignKey("TeacherLastUpdate")]
        public string? Lecturer { get; set; }
        public TeacherLastUpdate TeacherLastUpdate { get; set; }

        [ForeignKey("ClassroomLastUpdate")]
        public string LectureHall { get; set; }
        public ClassroomLastUpdate ClassroomLastUpdate { get; set; }

        public string? Groups { get; set; } = null;
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public string Type { get; set; }

        [ForeignKey("ClassDTO")]
        public Class Class { get; set; }
        public ClassDTO ClassDTO { get; set; }

        public ClassroomWorkSchedule() { }

        public ClassroomWorkSchedule(JToken json) {
            LectureHall = json.Value<string>("AUD") ?? throw new NullReferenceException("AUD");
            Date = DateOnly.Parse(json.Value<string>("DATE_Z") ?? throw new NullReferenceException("DATE_Z"));
            Name = json.Value<string>("DISCIP") ?? throw new NullReferenceException("DISCIP");
            Type = json.Value<string>("KOW") ?? throw new NullReferenceException("KOW");

            foreach(JToken? item in json.Value<JToken>("GROUPS") ?? "") {
                string? GROUP_P = item?.Value<string>("GROUP_P");
                string? PRIM = item?.Value<string>("PRIM");
                Groups += $"{GROUP_P}{$"{(string.IsNullOrWhiteSpace(PRIM) ? "" : $": {PRIM}")}"}; ";
            }

            Groups = Groups?[..^2];

            Lecturer = json.Value<string?>("PREP");

            Class = (Class)Enum.Parse(typeof(Class), (json.Value<string>("CLASS") ?? "other").Replace("default", "def"));

            string[] times = (json.Value<string>("TIME_Z") ?? throw new NullReferenceException("TIME_Z")).Split('-');
            StartTime = TimeOnly.Parse(times[0]);
            EndTime = TimeOnly.Parse(times[1]);
        }
    }
}
