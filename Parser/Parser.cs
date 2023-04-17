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
        private readonly string studentID;
        private readonly HttpClientHandler clientHandler;
        private readonly Timer UpdatingTimer;

        public static DateTime lastUpdate;

        public Parser(ScheduleDbContext dbContext, string group = "220611", string studentID = "201305") {
            this.dbContext = dbContext;
            this.group = group;
            this.studentID = studentID;

            clientHandler = new() {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.None,

                //Proxy = new WebProxy("127.0.0.1:8888"),
            };

            UpdatingTimer = new() {
                Interval = 30 * 60 * 1000 //Minutes Seconds Milliseconds
            };
            UpdatingTimer.Elapsed += Updating;

            Updating();
        }

        private void Updating(object? sender = null, ElapsedEventArgs? e = null) {
            var disciplines = GetDisciplines();

            if(disciplines != null) {
                var dates = GetDates();
                if(dates != null) {
                    lastUpdate = DateTime.Now;

                    var _list = dbContext.GetDisciplinesBetweenDates(dates.Value).ToList();

                    var except = disciplines.Except(_list);
                    if(except.Any()) {
                        SetDisciplineIsCompleted(dbContext.CompletedDisciplines.ToList(), except);
                        dbContext.Disciplines.AddRange(except);

                        dbContext.SaveChanges();
                        _list = dbContext.GetDisciplinesBetweenDates(dates.Value).ToList();
                    }

                    except = _list.Except(disciplines).Where(i => !i.IsCastom);
                    if(except.Any()) {
                        dbContext.Disciplines.RemoveRange(except);
                        dbContext.SaveChanges();
                    }
                }
            }

            var progress = GetProgress();
            if(progress != null) {
                var _list = dbContext.Progresses.ToList();

                var except = progress.Except(_list);
                if(except.Any()) {
                    dbContext.Progresses.AddRange(except);

                    dbContext.SaveChanges();
                    _list = dbContext.Progresses.ToList();
                }

                except = _list.Except(progress);
                if(except.Any()) {
                    dbContext.Progresses.RemoveRange(except);
                    dbContext.SaveChanges();
                }
            }

            UpdatingTimer.Start();
        }

        public static void SetDisciplineIsCompleted(List<CompletedDiscipline> completedDiscipline, IEnumerable<Discipline> list) {
            foreach(var discipline in list)
                discipline.IsCompleted = completedDiscipline.Contains(discipline);
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

        public List<Progress>? GetProgress() {
            try {
                using(var client = new HttpClient(clientHandler, false)) {
                    #region RequestHeaders
                    client.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
                    client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                    client.DefaultRequestHeaders.Add("Accept-Language", "ru,en;q=0.9,en-GB;q=0.8,en-US;q=0.7");
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36 Edg/112.0.1722.34");
                    client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                    client.DefaultRequestHeaders.Add("Referer", "https://tulsu.ru/progress/?search=201305");
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

                    using(var content = new StringContent($"SEARCH={studentID}", Encoding.UTF8, "application/x-www-form-urlencoded"))
                    using(HttpResponseMessage response = client.PostAsync("https://tulsu.ru/progress/queries/GetMarks.php", content).Result)
                        if(response.IsSuccessStatusCode) {
                            JArray jObject = JObject.Parse(response.Content.ReadAsStringAsync().Result).Value<JArray>("data") ?? throw new Exception();

                            return jObject.Select(j => new Progress(j)).Where(i => i.Mark != null).ToList();
                        }
                }
            } catch(Exception) {
                return null;
            }

            return null;
        }

    }
}
