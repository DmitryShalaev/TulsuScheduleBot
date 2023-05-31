using System.Net;
using System.Text;
using System.Timers;

using Newtonsoft.Json.Linq;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

namespace ScheduleBot {
    public class Parser {
        private readonly ScheduleDbContext dbContext;
        private readonly HttpClientHandler clientHandler;
        private readonly System.Timers.Timer UpdatingDisciplinesTimer;
        private readonly System.Timers.Timer UpdatingProgressTimer;

        public static DateTime scheduleLastUpdate;
        public static DateTime progressLastUpdate;

        public Parser(ScheduleDbContext dbContext, bool updating = true) {
            this.dbContext = dbContext;

            clientHandler = new() {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.None,

                //Proxy = new WebProxy("127.0.0.1:8888"),
            };

            UpdatingDisciplinesTimer = new() {
                Interval = 30 * 60 * 1000, //Minutes Seconds Milliseconds
                AutoReset = false
            };

            UpdatingProgressTimer = new() {
                Interval = 60 * 60 * 1000, //Minutes Seconds Milliseconds
                AutoReset = false
            };

            if(updating) {
                UpdatingDisciplinesTimer.Elapsed += UpdatingDisciplines;
                UpdatingProgressTimer.Elapsed += UpdatingProgress;

                UpdatingDisciplines(sender: null, e: null);
                UpdatingProgress(sender: null, e: null);
            }
        }

        private void UpdatingDisciplines(object? sender = null, ElapsedEventArgs? e = null) {
            UpdatingDisciplines(group: null);

            UpdatingDisciplinesTimer.Start();
        }

        private void UpdatingProgress(object? sender = null, ElapsedEventArgs? e = null) {
            UpdatingProgress(studentID: null);

            UpdatingProgressTimer.Start();
        }

        public void UpdatingProgress(string? studentID = null) {
            void Get(string iStudentID) {
                var progress = GetProgress(iStudentID);
                if(progress != null) {
                    progressLastUpdate = DateTime.Now;

                    var _list = dbContext.Progresses.Where(i => i.StudentID == iStudentID).ToList();

                    var except = progress.Except(_list);
                    if(except.Any()) {
                        dbContext.Progresses.AddRange(except);

                        dbContext.SaveChanges();
                        _list = dbContext.Progresses.Where(i => i.StudentID == iStudentID).ToList();
                    }

                    except = _list.Except(progress);
                    if(except.Any()) {
                        dbContext.Progresses.RemoveRange(except);
                        dbContext.SaveChanges();
                    }
                }
            }

            if(string.IsNullOrWhiteSpace(studentID)) {
                foreach(var iStudentID in dbContext.ScheduleProfile.Select(i => i.StudentID).Distinct().ToList()) {
                    if(string.IsNullOrWhiteSpace(iStudentID)) continue;

                    Get(iStudentID);
                }
            } else {
                Get(studentID);
            }
        }

        public void UpdatingDisciplines(string? group = null) {
            void Get(string iGroup) {
                var disciplines = GetDisciplines(iGroup);

                if(disciplines != null) {
                    var dates = GetDates(iGroup);
                    if(dates != null) {
                        scheduleLastUpdate = DateTime.Now;

                        var _list = dbContext.Disciplines.Where(i => i.Group == iGroup && i.Date >= dates.Value.min && i.Date <= dates.Value.max).ToList();

                        var except = disciplines.Except(_list);
                        if(except.Any()) {
                            dbContext.Disciplines.AddRange(except);

                            dbContext.SaveChanges();
                            _list = dbContext.Disciplines.Where(i => i.Group == iGroup && i.Date >= dates.Value.min && i.Date <= dates.Value.max).ToList();
                        }

                        except = _list.Except(disciplines);
                        if(except.Any()) {
                            dbContext.Disciplines.RemoveRange(except);
                            dbContext.SaveChanges();
                        }
                    }
                }
            }

            if(string.IsNullOrWhiteSpace(group)) {
                foreach(var iGroup in dbContext.ScheduleProfile.Select(i => i.Group).Distinct().ToList()) {
                    if(string.IsNullOrWhiteSpace(iGroup)) continue;

                    Get(iGroup);
                }
            } else {
                Get(group);
            }
        }

        public List<Discipline>? GetDisciplines(string group) {
            try {
                using(var client = new HttpClient(clientHandler, false)) {
                    #region RequestHeaders
                    client.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
                    client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                    client.DefaultRequestHeaders.Add("Accept-Language", "ru,en;q=0.9,en-GB;q=0.8,en-US;q=0.7");
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36 Edg/112.0.1722.34");
                    client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                    client.DefaultRequestHeaders.Add("Referer", $"https://tulsu.ru/schedule/?search={group}");
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
                            if(jObject.Count == 0) throw new Exception();

                            return jObject.Select(j => new Discipline(j, group)).ToList();
                        }
                }
            } catch(Exception) {
                return null;
            }

            return null;
        }

        public (DateOnly min, DateOnly max)? GetDates(string group) {
            try {
                using(var client = new HttpClient(clientHandler, false)) {
                    #region RequestHeaders
                    client.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
                    client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                    client.DefaultRequestHeaders.Add("Accept-Language", "ru,en;q=0.9,en-GB;q=0.8,en-US;q=0.7");
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36 Edg/112.0.1722.34");
                    client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                    client.DefaultRequestHeaders.Add("Referer", $"https://tulsu.ru/schedule/?search={group}");
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

        public List<Progress>? GetProgress(string studentID) {
            try {
                using(var client = new HttpClient(clientHandler, false)) {
                    #region RequestHeaders
                    client.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
                    client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                    client.DefaultRequestHeaders.Add("Accept-Language", "ru,en;q=0.9,en-GB;q=0.8,en-US;q=0.7");
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36 Edg/112.0.1722.34");
                    client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                    client.DefaultRequestHeaders.Add("Referer", $"https://tulsu.ru/progress/?search={studentID}");
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
                            if(jObject.Count == 0) throw new Exception();

                            return jObject.Select(j => new Progress(j, studentID)).Where(i => i.Mark != null).ToList();
                        }
                }
            } catch(Exception) {
                return null;
            }

            return null;
        }
    }
}
