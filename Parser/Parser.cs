using System.Text;
using System.Timers;

using Newtonsoft.Json.Linq;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Timer = System.Timers.Timer;

namespace ScheduleBot {
    public class Parser {
        private readonly ScheduleDbContext dbContext;
        private readonly Timer UpdatingTimer;

        public Parser(ScheduleDbContext dbContext) {
            this.dbContext = dbContext;

            UpdatingTimer = new() {
                Interval = 60 * 60 * 1000 //Minutes Seconds Milliseconds
            };
            UpdatingTimer.Elapsed += Updating;

            Updating();
        }

        private void Updating(object? sender = null, ElapsedEventArgs? e = null) {
            var disciplines = GetDisciplines();
            var _list = dbContext.Disciplines.ToList();

            if(disciplines != null) {
                
                dbContext.SaveChanges();
            }

            UpdatingTimer.Start();
        }

        public List<Discipline>? GetDisciplines(string group = "220611") {
            using(var client = new HttpClient()) {
                #region RequestHeaders
                client.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                client.DefaultRequestHeaders.Add("Accept-Language", "ru,en;q=0.9,en-GB;q=0.8,en-US;q=0.7");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36 Edg/112.0.1722.34");
                client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                client.DefaultRequestHeaders.Add("Referer", "https://tulsu.ru/schedule/?search=220611");
                client.DefaultRequestHeaders.Add("Origin", "https://tulsu.ru");
                client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Chromium\";v=\"112\", \"Microsoft Edge\";v=\"112\", \"Not:A-Brand\";v=\"99\"");
                client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
                client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
                client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
                client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
                client.DefaultRequestHeaders.Add("Connection", "keep-alive");
                client.DefaultRequestHeaders.Add("Host", "tulsu.ru");

                #endregion

                using(var content = new StringContent($"search_field=GROUP_P&search_value={group}", Encoding.UTF8, "application/x-www-form-urlencoded"))
                using(HttpResponseMessage response = client.PostAsync("https://tulsu.ru/schedule/queries/GetSchedule.php", content).Result)
                    if(response.IsSuccessStatusCode) {
                        JArray jObject = JArray.Parse(response.Content.ReadAsStringAsync().Result);

                        return jObject.Select(j => new Discipline(j)).ToList();
                    }
            }
            return null;
        }
    }
}
