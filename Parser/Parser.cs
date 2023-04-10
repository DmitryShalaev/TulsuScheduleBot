using System.Net;
using System.Text;
using System.Timers;

using Newtonsoft.Json.Linq;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Timer = System.Timers.Timer;

namespace ScheduleBot {
    public class Parser {
        private readonly ScheduleDbContext dbContext;
        private readonly string group;
        private readonly HttpClientHandler clientHandler;
        private readonly Timer UpdatingTimer;

        public static string notSub = "";
        public static DateTime lastUpdate;

        public Parser(ScheduleDbContext dbContext, string group = "220611", string notSub = "1 пг") {
            this.dbContext = dbContext;
            this.group = group;

            Parser.notSub = notSub;

            clientHandler = new() {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.None,

                //Proxy = new WebProxy("127.0.0.1:8888"),
            };

            UpdatingTimer = new() {
                Interval = 60 * 60 * 1000 //Minutes Seconds Milliseconds
            };
            UpdatingTimer.Elapsed += Updating;

            Updating();
        }

        private void Updating(object? sender = null, ElapsedEventArgs? e = null) {
            var disciplines = GetDisciplines();

            if(disciplines != null) {
                var dates = GetDates();
                if(dates != null) {
                    var _list = dbContext.GetDisciplinesBetweenDates(dates.Value).ToList();

                    var except = disciplines.Except(_list);
                    if(except.Any()) {
                        AddToSchedule(except);
                        dbContext.SaveChanges();
                        _list = dbContext.GetDisciplinesBetweenDates(dates.Value).ToList();
                    }

                    except = _list.Except(disciplines);
                    if(except.Any()) {
                        dbContext.Disciplines.RemoveRange(except);
                        dbContext.SaveChanges();
                    }

                    lastUpdate = DateTime.Now;
                }
            }

            UpdatingTimer.Start();
        }

        private void AddToSchedule(IEnumerable<Discipline> list) {
            SetDisciplineIsCompleted(dbContext.CompletedDisciplines.ToList(), list);

            dbContext.Disciplines.AddRange(list);
        }

        public static void SetDisciplineIsCompleted(List<CompletedDiscipline> completedDiscipline, IEnumerable<Discipline> list) {
            foreach(var discipline in list)
                discipline.IsCompleted = (discipline.Class == DB.Entity.Type.lab && discipline.Subgroup == notSub) || completedDiscipline.Contains(new() { Name = discipline.Name, Class = discipline.Class });
        }

        public List<Discipline>? GetDisciplines() {
            try {
                using(var client = new HttpClient(clientHandler, false)) {
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
            } catch(Exception) {
                return null;
            }

            return null;
        }

        public (DateOnly min, DateOnly max)? GetDates() {
            try {
                using(var client = new HttpClient(clientHandler, false)) {
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

                    using(var content = new StringContent($"search_value={group}", Encoding.UTF8, "application/x-www-form-urlencoded"))
                    using(HttpResponseMessage response = client.PostAsync("https://tulsu.ru/schedule/queries/GetDates.php", content).Result)
                        if(response.IsSuccessStatusCode) {
                            JObject jObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);

                            return (DateOnly.Parse(jObject.Value<string>("MIN_DATE") ?? throw new NullReferenceException("MIN_DATE")),
                                    DateOnly.Parse(jObject.Value<string>("MAX_DATE") ?? throw new NullReferenceException("MAX_DATE")));
                        }
                }
            } catch(Exception) {
                return null;
            }
            return null;
        }
    }
}
