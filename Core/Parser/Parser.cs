using System.Data;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json.Linq;

using ScheduleBot.Bot;
using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

namespace ScheduleBot {
    public partial class Parser {
        private readonly HttpClientHandler clientHandler;
        private readonly System.Timers.Timer UpdatingDisciplinesTimer;

        public delegate Task UpdatedDisciplines(ScheduleDbContext dbContext, List<(string Group, DateOnly Date)> values);
        private event UpdatedDisciplines Notify;

        public static Parser? Instance { get; private set; }

        public Parser(UpdatedDisciplines updatedDisciplines) {
            Notify += updatedDisciplines;

            clientHandler = new() {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.None,

                //Proxy = new WebProxy("127.0.0.1:8888"),
            };

            UpdatingDisciplinesTimer = new() {
                Interval = BotCommands.GetInstance().Config.GroupUpdateTime * 60 * 1000, //Minutes Seconds Milliseconds
                AutoReset = false
            };

            UpdatingDisciplinesTimer.Elapsed += UpdatingDisciplines;

            Instance = this;

            Task.Run(() => {
                using(ScheduleDbContext dbContext = new())
                    UpdatingTeachers(dbContext);

                UpdatingDisciplines(sender: null, e: null);
            });
        }

        private void UpdatingDisciplines(object? sender = null, ElapsedEventArgs? e = null) {
            using(ScheduleDbContext dbContext = new()) {
                foreach(string item in dbContext.ScheduleProfile.Where(i => !string.IsNullOrEmpty(i.Group) && (DateTime.Now - i.LastAppeal.ToLocalTime()).TotalDays <= 7).Select(i => i.Group!).Distinct().ToList())
                    UpdatingDisciplines(dbContext, group: item, updateAttemptTime: 0);

                UpdatingDisciplinesTimer.Start();
            }
        }

        public bool UpdatingProgress(ScheduleDbContext dbContext, string studentID, int updateAttemptTime) {
            StudentIDLastUpdate? studentIDLastUpdate = dbContext.StudentIDLastUpdate.FirstOrDefault(i => i.StudentID == studentID);
            if(studentIDLastUpdate is null) {
                studentIDLastUpdate = new() { StudentID = studentID, Update = DateTime.MinValue.ToUniversalTime(), UpdateAttempt = DateTime.UtcNow };
                dbContext.StudentIDLastUpdate.Add(studentIDLastUpdate);

                dbContext.SaveChanges();
            } else {
                if((DateTime.Now - studentIDLastUpdate.UpdateAttempt.ToLocalTime()).TotalMinutes > updateAttemptTime)
                    studentIDLastUpdate.UpdateAttempt = DateTime.UtcNow;
                else
                    return false;
            }

            List<Progress>? progress = GetProgress(studentID);
            if(progress != null) {

                studentIDLastUpdate!.Update = DateTime.UtcNow;

                var _list = dbContext.Progresses.Where(i => i.StudentID == studentID).ToList();

                IEnumerable<Progress> except = progress.Except(_list);
                if(except.Any()) {
                    dbContext.Progresses.AddRange(except);

                    dbContext.SaveChanges();
                    _list = dbContext.Progresses.Where(i => i.StudentID == studentID).ToList();
                }

                except = _list.Except(progress);
                if(except.Any())
                    dbContext.Progresses.RemoveRange(except);

                dbContext.SaveChanges();

                return true;
            }

            return false;
        }

        public bool UpdatingDisciplines(ScheduleDbContext dbContext, string group, int updateAttemptTime) {
            GroupLastUpdate? groupLastUpdate = dbContext.GroupLastUpdate.FirstOrDefault(i => i.Group == group);
            if(groupLastUpdate is null) {
                groupLastUpdate = new() { Group = group, Update = DateTime.MinValue.ToUniversalTime(), UpdateAttempt = DateTime.UtcNow };
                dbContext.GroupLastUpdate.Add(groupLastUpdate);

                dbContext.SaveChanges();
            } else {
                if((DateTime.Now - groupLastUpdate.UpdateAttempt.ToLocalTime()).TotalMinutes > updateAttemptTime)
                    groupLastUpdate.UpdateAttempt = DateTime.UtcNow;
                else
                    return false;
            }

            (DateOnly min, DateOnly max)? dates = GetDates(group);
            if(dates != null) {

                List<Discipline>? disciplines = GetDisciplines(group);
                if(disciplines != null) {
                    List<Discipline> updatedDisciplines = new();

                    groupLastUpdate.Update = DateTime.UtcNow;

                    if(dbContext.Disciplines.Any(i => i.Group == group && i.Date < dates.Value.min)) {
                        dbContext.Disciplines.RemoveRange(dbContext.Disciplines.Where(i => i.Group == group && i.Date < dates.Value.min));
                        dbContext.SaveChanges();
                    }

                    var _list = dbContext.Disciplines.Where(i => i.Group == group && i.Date >= dates.Value.min && i.Date <= dates.Value.max).ToList();

                    IEnumerable<Discipline> except = disciplines.Except(_list);
                    if(except.Any()) {
                        dbContext.Disciplines.AddRange(except);

                        if(_list.Any())
                            updatedDisciplines.AddRange(except);

                        dbContext.SaveChanges();
                        _list = dbContext.Disciplines.Where(i => i.Group == group && i.Date >= dates.Value.min && i.Date <= dates.Value.max).ToList();
                    }

                    except = _list.Except(disciplines);
                    if(except.Any()) {
                        dbContext.Disciplines.RemoveRange(except);

                        updatedDisciplines.AddRange(except);
                    }

                    dbContext.SaveChanges();

                    if(updatedDisciplines.Any()) {
                        var date = DateOnly.FromDateTime(DateTime.Now);
                        Notify.Invoke(dbContext, updatedDisciplines.Where(i => i.Date >= date).Select(i => (i.Group, i.Date)).Distinct().OrderBy(i => i.Date).ToList()).Wait();
                    }

                    return true;
                }
            }

            return false;
        }

        public bool UpdatingTeacherWorkSchedule(ScheduleDbContext dbContext, string teacher, int updateAttemptTime) {
            TeacherLastUpdate? teacherLastUpdate = dbContext.TeacherLastUpdate.FirstOrDefault(i => i.Teacher == teacher);
            if(teacherLastUpdate is null) {
                teacherLastUpdate = new() { Teacher = teacher, Update = DateTime.MinValue.ToUniversalTime(), UpdateAttempt = DateTime.UtcNow };
                dbContext.TeacherLastUpdate.Add(teacherLastUpdate);

                dbContext.SaveChanges();
            } else {
                if((DateTime.Now - teacherLastUpdate.UpdateAttempt.ToLocalTime()).TotalMinutes > updateAttemptTime)
                    teacherLastUpdate.UpdateAttempt = DateTime.UtcNow;
                else
                    return false;
            }

            (DateOnly min, DateOnly max)? dates = GetDates(teacher);
            if(dates != null) {

                List<TeacherWorkSchedule>? teacherWorkSchedule = GetTeachersWorkSchedule(teacher);

                if(teacherWorkSchedule != null) {

                    teacherLastUpdate!.Update = DateTime.UtcNow;

                    if(dbContext.TeacherWorkSchedule.Any(i => i.Lecturer == teacher && i.Date < dates.Value.min)) {
                        dbContext.TeacherWorkSchedule.RemoveRange(dbContext.TeacherWorkSchedule.Where(i => i.Lecturer == teacher && i.Date < dates.Value.min));
                        dbContext.SaveChanges();
                    }

                    var _list = dbContext.TeacherWorkSchedule.Where(i => i.Lecturer == teacher && i.Date >= dates.Value.min && i.Date <= dates.Value.max).ToList();

                    IEnumerable<TeacherWorkSchedule> except = teacherWorkSchedule.Except(_list);
                    if(except.Any()) {
                        dbContext.TeacherWorkSchedule.AddRange(except);

                        dbContext.SaveChanges();
                        _list = dbContext.TeacherWorkSchedule.Where(i => i.Lecturer == teacher && i.Date >= dates.Value.min && i.Date <= dates.Value.max).ToList();
                    }

                    except = _list.Except(teacherWorkSchedule);
                    if(except.Any())
                        dbContext.TeacherWorkSchedule.RemoveRange(except);

                    dbContext.SaveChanges();

                    return true;
                }
            }

            return false;
        }

        public bool UpdatingTeachers(ScheduleDbContext dbContext) {
            List<string>? teachers = GetTeachers();

            if(teachers != null) {
                var _list = dbContext.TeacherLastUpdate.Select(i => i.Teacher).ToList();

                IEnumerable<string> except = teachers.Except(_list);
                if(except.Any()) {
                    DateTime updDate = new DateTime(2000, 1, 1).ToUniversalTime();
                    dbContext.TeacherLastUpdate.AddRange(except.Select(i => new TeacherLastUpdate() { Teacher = i, Update = updDate }));

                    dbContext.SaveChanges();

                    return true;
                }
            }

            return false;
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

                    client.Timeout = TimeSpan.FromSeconds(10);

                    #endregion

                    using(var content = new StringContent($"search_field=GROUP_P&search_value={group}", Encoding.UTF8, "application/x-www-form-urlencoded"))
                    using(HttpResponseMessage response = client.PostAsync("https://tulsu.ru/schedule/queries/GetSchedule.php", content).Result)
                        if(response.IsSuccessStatusCode) {
                            var jObject = JArray.Parse(response.Content.ReadAsStringAsync().Result);
                            return jObject.Count == 0 ? throw new Exception() : jObject.Select(j => new Discipline(j, group)).ToList();
                        }
                }
            } catch(Exception) {
                return null;
            }

            return null;
        }

        public List<TeacherWorkSchedule>? GetTeachersWorkSchedule(string fio) {
            try {
                using(var client = new HttpClient(clientHandler, false)) {
                    #region RequestHeaders
                    client.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
                    client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                    client.DefaultRequestHeaders.Add("Accept-Language", "ru,en;q=0.9,en-GB;q=0.8,en-US;q=0.7");
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36 Edg/112.0.1722.34");
                    client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                    client.DefaultRequestHeaders.Add("Referer", $"https://tulsu.ru/schedule/?search={fio}");
                    client.DefaultRequestHeaders.Add("Origin", "https://tulsu.ru");
                    client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Chromium\";v=\"112\", \"Microsoft Edge\";v=\"112\", \"Not:A-Brand\";v=\"99\"");
                    client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                    client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
                    client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
                    client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
                    client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
                    client.DefaultRequestHeaders.Add("Connection", "keep-alive");
                    client.DefaultRequestHeaders.Add("Host", "tulsu.ru");

                    client.Timeout = TimeSpan.FromSeconds(10);

                    #endregion

                    using(var content = new StringContent($"search_field=PREP&search_value={fio}", Encoding.UTF8, "application/x-www-form-urlencoded"))
                    using(HttpResponseMessage response = client.PostAsync("https://tulsu.ru/schedule/queries/GetSchedule.php", content).Result)
                        if(response.IsSuccessStatusCode) {
                            var jObject = JArray.Parse(response.Content.ReadAsStringAsync().Result);
                            return jObject.Count == 0 ? throw new Exception() : jObject.Select(j => new TeacherWorkSchedule(j)).ToList();
                        }
                }
            } catch(Exception) {
                return null;
            }

            return null;
        }

        [GeneratedRegex("^[А-ЯЁ][а-яё]+\\s*[А-ЯЁ](?:[а-яё\\.]+)?(?:\\s*[А-ЯЁ][а-яё]+)?(?:\\s[А-ЯЁ]\\\\.)?\\s*(?:\\s*[А-ЯЁ]\\.)?(?:\\s*[А-ЯЁ][а-яё]+)?$")]
        private static partial Regex TeachersRegex();

        public List<string>? GetTeachers() {
            try {
                using(var client = new HttpClient(clientHandler, false)) {
                    #region RequestHeaders
                    client.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
                    client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                    client.DefaultRequestHeaders.Add("Accept-Language", "ru,en;q=0.9,en-GB;q=0.8,en-US;q=0.7");
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36 Edg/112.0.1722.34");
                    client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                    client.DefaultRequestHeaders.Add("Referer", $"https://tulsu.ru/schedule/");
                    client.DefaultRequestHeaders.Add("Origin", "https://tulsu.ru");
                    client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Chromium\";v=\"112\", \"Microsoft Edge\";v=\"112\", \"Not:A-Brand\";v=\"99\"");
                    client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                    client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
                    client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
                    client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
                    client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
                    client.DefaultRequestHeaders.Add("Connection", "keep-alive");
                    client.DefaultRequestHeaders.Add("Host", "tulsu.ru");

                    client.Timeout = TimeSpan.FromSeconds(10);

                    #endregion

                    Regex regex = TeachersRegex();

                    using(HttpResponseMessage response = client.GetAsync("https://tulsu.ru/schedule/queries/GetDictionaries.php").Result)
                        if(response.IsSuccessStatusCode) {
                            var jObject = JArray.Parse(response.Content.ReadAsStringAsync().Result);
                            return jObject.Count == 0 ? throw new Exception() : jObject?.Where(i => regex.IsMatch(i.Value<string>("value") ?? "")).Select(j => j.Value<string>("value")?.Trim() ?? "").ToList();
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

                    client.Timeout = TimeSpan.FromSeconds(10);

                    #endregion

                    using(var content = new StringContent($"search_value={group}", Encoding.UTF8, "application/x-www-form-urlencoded"))
                    using(HttpResponseMessage response = client.PostAsync("https://tulsu.ru/schedule/queries/GetDates.php", content).Result)
                        if(response.IsSuccessStatusCode) {
                            var jObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);

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

                    client.Timeout = TimeSpan.FromSeconds(10);

                    #endregion

                    using(var content = new StringContent($"SEARCH={studentID}", Encoding.UTF8, "application/x-www-form-urlencoded"))
                    using(HttpResponseMessage response = client.PostAsync("https://tulsu.ru/progress/queries/GetMarks.php", content).Result)
                        if(response.IsSuccessStatusCode) {
                            JArray jObject = JObject.Parse(response.Content.ReadAsStringAsync().Result).Value<JArray>("data") ?? throw new Exception();
                            return jObject.Count == 0
                                ? throw new Exception()
                                : jObject.Select(j => new Progress(j, studentID)).Where(i => i.Mark != null).ToList();
                        }
                }
            } catch(Exception) {
                return null;
            }

            return null;
        }
    }
}
