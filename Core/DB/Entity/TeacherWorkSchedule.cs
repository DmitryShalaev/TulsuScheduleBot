using System.ComponentModel.DataAnnotations.Schema;

using Newtonsoft.Json.Linq;

namespace Core.DB.Entity {

#pragma warning disable CS8618
    public class TeacherWorkSchedule {
        public long ID { get; set; }

        public string Name { get; set; }

        [ForeignKey("TeacherLastUpdate")]
        public string Lecturer { get; set; }
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

        public TeacherWorkSchedule() { }

        public TeacherWorkSchedule(JToken json) {
            ArgumentNullException.ThrowIfNull(json);

            LectureHall = json.Value<string>("AUD") ?? throw new NullReferenceException("Field 'AUD' is missing in JSON");
            Date = DateOnly.Parse(json.Value<string>("DATE_Z") ?? throw new NullReferenceException("Field 'DATE_Z' is missing in JSON"));
            Name = json.Value<string>("DISCIP") ?? throw new NullReferenceException("Field 'DISCIP' is missing in JSON");
            Type = json.Value<string>("KOW") ?? throw new NullReferenceException("Field 'KOW' is missing in JSON");

            JToken? groupItems = json.Value<JToken>("GROUPS");
            if(groupItems != null && groupItems.HasValues) {
                var groupList = new List<string>();
                foreach(JToken? item in groupItems) {
                    string? groupP = item?.Value<string>("GROUP_P");
                    string? prim = item?.Value<string>("PRIM");

                    if(!string.IsNullOrWhiteSpace(groupP)) {
                        groupList.Add(string.IsNullOrWhiteSpace(prim) ? groupP : $"{groupP}: {prim}");
                    }
                }

                Groups = string.Join("; ", groupList);
            }

            Lecturer = json.Value<string>("PREP") ?? throw new NullReferenceException("Field 'PREP' is missing in JSON");

            Class = (Class)Enum.Parse(typeof(Class), (json.Value<string>("CLASS") ?? "other").Replace("default", "def"));

            string timeRange = json.Value<string>("TIME_Z") ?? throw new NullReferenceException("Field 'TIME_Z' is missing in JSON");
            string[] times = timeRange.Split('-');
            if(times.Length != 2)
                throw new FormatException($"Invalid time range format in 'TIME_Z': {timeRange}");

            StartTime = TimeOnly.Parse(times[0]);
            EndTime = TimeOnly.Parse(times[1]);
        }
    }
}
