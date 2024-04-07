using System.Data;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

using Core.Bot;
using Core.Bot.Commands;
using Core.DB.Entity;

using HtmlAgilityPack;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json.Linq;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

namespace ScheduleBot {
    public partial class Parser {
        private readonly HttpClientHandler clientHandler;

        private static Parser? instance;

        public static Parser Instance => instance ??= new Parser();

        private Parser() {
            clientHandler = new() {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.None,

                //Proxy = new WebProxy("127.0.0.1:8888"),
            };

            Task.Run(GetTeachersData);
        }

        public async Task GetTeachersData() {
            using(ScheduleDbContext dbContext = new()) {
                await UpdatingTeachers(dbContext);

                var teachers = dbContext.Disciplines.Include(i => i.TeacherLastUpdate).Where(i => i.Lecturer != null && string.IsNullOrEmpty(i.TeacherLastUpdate.LinkProfile)).Select(i => i.Lecturer!).Distinct().ToList();
                foreach(string item in teachers) {
                    await UpdatingTeacherInfo(dbContext, item);
                    await Task.Delay(TimeSpan.FromSeconds(10));
                }
            }
        }

        public async Task<bool> UpdatingProgress(ScheduleDbContext dbContext, string studentID, int updateAttemptTime) {
            StudentIDLastUpdate? studentIDLastUpdate = dbContext.StudentIDLastUpdate.FirstOrDefault(i => i.StudentID == studentID);
            if(studentIDLastUpdate is null) {
                studentIDLastUpdate = new() { StudentID = studentID, Update = DateTime.MinValue.ToUniversalTime(), UpdateAttempt = DateTime.UtcNow };
                dbContext.StudentIDLastUpdate.Add(studentIDLastUpdate);
            } else {
                if((DateTime.Now - studentIDLastUpdate.UpdateAttempt.ToLocalTime()).TotalMinutes > updateAttemptTime)
                    studentIDLastUpdate.UpdateAttempt = DateTime.UtcNow;
                else
                    return false;
            }

            await dbContext.SaveChangesAsync();

            List<Progress>? progress = await GetProgress(studentID);
            if(progress is not null) {

                studentIDLastUpdate!.Update = DateTime.UtcNow;

                var _list = dbContext.Progresses.Where(i => i.StudentID == studentID).ToList();

                IEnumerable<Progress> except = progress.Except(_list);
                if(except.Any()) {
                    dbContext.Progresses.AddRange(except);

                    await dbContext.SaveChangesAsync();
                    _list = dbContext.Progresses.Where(i => i.StudentID == studentID).ToList();
                }

                except = _list.Except(progress);
                if(except.Any())
                    dbContext.Progresses.RemoveRange(except);

                await dbContext.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<bool> UpdatingDisciplines(ScheduleDbContext dbContext, string group, int updateAttemptTime) {
            GroupLastUpdate? groupLastUpdate = dbContext.GroupLastUpdate.FirstOrDefault(i => i.Group == group);
            if(groupLastUpdate is null) {
                groupLastUpdate = new() { Group = group, Update = DateTime.MinValue.ToUniversalTime(), UpdateAttempt = DateTime.UtcNow };
                dbContext.GroupLastUpdate.Add(groupLastUpdate);
            } else {
                if((DateTime.Now - groupLastUpdate.UpdateAttempt.ToLocalTime()).TotalMinutes >= updateAttemptTime)
                    groupLastUpdate.UpdateAttempt = DateTime.UtcNow;
                else
                    return false;
            }

            await dbContext.SaveChangesAsync();

            (DateOnly min, DateOnly max)? dates = await GetDates(group);

            if(dates is not null) {

                List<Discipline>? disciplines = await GetDisciplines(group);

                if(disciplines is not null) {
                    List<Discipline> updatedDisciplines = [];

                    groupLastUpdate.Update = DateTime.UtcNow;

                    if(dbContext.Disciplines.Any(i => i.Group == group && (i.Date < dates.Value.min || i.Date > dates.Value.max))) {
                        dbContext.Disciplines.RemoveRange(dbContext.Disciplines.Where(i => i.Group == group && (i.Date < dates.Value.min || i.Date > dates.Value.max)));
                        await dbContext.SaveChangesAsync();
                    }

                    var _list = dbContext.Disciplines.Where(i => i.Group == group).ToList();

                    IEnumerable<Discipline> except = disciplines.Except(_list);
                    if(except.Any()) {
                        dbContext.Disciplines.AddRange(except);

                        if(_list.Any())
                            updatedDisciplines.AddRange(except);

                        await dbContext.SaveChangesAsync();
                        _list = dbContext.Disciplines.Where(i => i.Group == group).ToList();
                    }

                    except = _list.Except(disciplines);
                    if(except.Any()) {
                        dbContext.Disciplines.RemoveRange(except);

                        updatedDisciplines.AddRange(except);

                        dbContext.DeletedDisciplines.AddRange(except.Select(i => new DeletedDisciplines(i)));
                    }

                    await dbContext.SaveChangesAsync();

                    if(updatedDisciplines.Any()) {
                        var date = DateOnly.FromDateTime(DateTime.Now);
                        await Notifications.UpdatedDisciplinesAsync(dbContext, updatedDisciplines.Where(i => i.Date >= date).Select(i => (i.Group, i.Date)).Distinct().OrderBy(i => i.Date).ToList());
                    }

                    await IntersectionOfSubgroups(dbContext, group);

                    return true;
                }
            }

            return false;
        }

        public async Task IntersectionOfSubgroups(ScheduleDbContext dbContext, string group) {
            IntersectionOfSubgroups? intersection = dbContext.IntersectionOfSubgroups.SingleOrDefault(i => i.Group == group);

            if(intersection is not null) {
                await UpdatingDisciplines(dbContext, intersection.IntersectionWith, UserCommands.Instance.Config.DisciplineUpdateTime);

                dbContext.Disciplines.Where(d => (d.Group == group || d.Group == intersection.IntersectionWith) && d.Class == intersection.Class)
                                     .ToList()
                                     .GroupBy(d => new { d.Name, d.Lecturer, d.LectureHall, d.Date, d.StartTime, d.EndTime })
                                     .Where(g => g.Count() > 1)
                                     .SelectMany(g => g)
                                     .Where(i => i.Group == group)
                                     .ToList()
                                     .ForEach(d => d.IntersectionMark = intersection.Mark);

                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<bool> UpdatingTeacherWorkSchedule(ScheduleDbContext dbContext, string teacher, int updateAttemptTime) {
            TeacherLastUpdate? teacherLastUpdate = dbContext.TeacherLastUpdate.FirstOrDefault(i => i.Teacher == teacher);
            if(teacherLastUpdate is null) {
                teacherLastUpdate = new() { Teacher = teacher, Update = DateTime.MinValue.ToUniversalTime(), UpdateAttempt = DateTime.UtcNow };
                dbContext.TeacherLastUpdate.Add(teacherLastUpdate);
            } else {
                if((DateTime.Now - teacherLastUpdate.UpdateAttempt.ToLocalTime()).TotalMinutes > updateAttemptTime)
                    teacherLastUpdate.UpdateAttempt = DateTime.UtcNow;
                else
                    return false;
            }

            await dbContext.SaveChangesAsync();

            await UpdatingTeacherInfo(dbContext, teacher);

            (DateOnly min, DateOnly max)? dates = await GetDates(teacher);
            if(dates is not null) {

                List<TeacherWorkSchedule>? teacherWorkSchedule = await GetTeachersWorkSchedule(teacher);

                if(teacherWorkSchedule is not null) {

                    teacherLastUpdate!.Update = DateTime.UtcNow;

                    if(dbContext.TeacherWorkSchedule.Any(i => i.Lecturer == teacher && (i.Date < dates.Value.min || i.Date > dates.Value.max))) {
                        dbContext.TeacherWorkSchedule.RemoveRange(dbContext.TeacherWorkSchedule.Where(i => i.Lecturer == teacher && (i.Date < dates.Value.min || i.Date > dates.Value.max)));
                        await dbContext.SaveChangesAsync();
                    }

                    var _list = dbContext.TeacherWorkSchedule.Where(i => i.Lecturer == teacher).ToList();

                    IEnumerable<TeacherWorkSchedule> except = teacherWorkSchedule.Except(_list);
                    if(except.Any()) {
                        dbContext.TeacherWorkSchedule.AddRange(except);

                        await dbContext.SaveChangesAsync();
                        _list = dbContext.TeacherWorkSchedule.Where(i => i.Lecturer == teacher).ToList();
                    }

                    except = _list.Except(teacherWorkSchedule);
                    if(except.Any())
                        dbContext.TeacherWorkSchedule.RemoveRange(except);

                    await dbContext.SaveChangesAsync();

                    return true;
                }
            }

            return false;
        }

        public async Task<bool> UpdatingTeachers(ScheduleDbContext dbContext) {
            List<string>? teachers = await GetTeachers();

            if(teachers is not null) {
                var _list = dbContext.TeacherLastUpdate.Select(i => i.Teacher).ToList();

                IEnumerable<string> except = teachers.Except(_list);
                if(except.Any()) {
                    DateTime updDate = new DateTime(2000, 1, 1).ToUniversalTime();
                    dbContext.TeacherLastUpdate.AddRange(except.Select(i => new TeacherLastUpdate() { Teacher = i, Update = updDate }));

                    await dbContext.SaveChangesAsync();
                    _list = dbContext.TeacherLastUpdate.Select(i => i.Teacher).ToList();
                }

                except = _list.Except(teachers);

                if(except.Any())
                    dbContext.TeacherLastUpdate.RemoveRange(dbContext.TeacherLastUpdate.Where(i => except.Contains(i.Teacher)));

                await dbContext.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<List<Discipline>?> GetDisciplines(string group) {
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
                    using(HttpResponseMessage response = await client.PostAsync("https://tulsu.ru/schedule/queries/GetSchedule.php", content))
                        if(response.IsSuccessStatusCode) {
                            var jObject = JArray.Parse(await response.Content.ReadAsStringAsync());
                            return jObject.Count == 0 ? throw new Exception() : jObject.Select(j => new Discipline(j, group)).ToList();
                        }
                }
            } catch(Exception) {
                return null;
            }

            return null;
        }

        public async Task<List<TeacherWorkSchedule>?> GetTeachersWorkSchedule(string fio) {
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
                    using(HttpResponseMessage response = await client.PostAsync("https://tulsu.ru/schedule/queries/GetSchedule.php", content))
                        if(response.IsSuccessStatusCode) {
                            var jObject = JArray.Parse(await response.Content.ReadAsStringAsync());
                            return jObject.Count == 0 ? throw new Exception() : jObject.Select(j => new TeacherWorkSchedule(j)).ToList();
                        }
                }
            } catch(Exception) {
                return null;
            }

            return null;
        }

        [GeneratedRegex("^[А-ЯЁ][а-яё]+\\s*[А-ЯЁ](?:[а-яё.]+)?(?:\\s[А-ЯЁа-яё.]+)*$")]
        private static partial Regex TeachersRegex();

        public async Task<List<string>?> GetTeachers() {
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

                    using(HttpResponseMessage response = await client.GetAsync("https://tulsu.ru/schedule/queries/GetDictionaries.php")) {
                        if(response.IsSuccessStatusCode) {
                            var jObject = JArray.Parse(await response.Content.ReadAsStringAsync());
                            return jObject.Count == 0 ? throw new Exception() : jObject?.Where(i => regex.IsMatch(i.Value<string>("value")?.Trim() ?? "")).Select(j => j.Value<string>("value")?.Trim() ?? "").ToList();
                        }
                    }
                }
            } catch(Exception) {
                return null;
            }

            return null;
        }

        public async Task<(DateOnly min, DateOnly max)?> GetDates(string group) {
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
                    using(HttpResponseMessage response = await client.PostAsync("https://tulsu.ru/schedule/queries/GetDates.php", content))
                        if(response.IsSuccessStatusCode) {
                            var jObject = JObject.Parse(await response.Content.ReadAsStringAsync());

                            return (DateOnly.Parse(jObject.Value<string>("MIN_DATE") ?? throw new NullReferenceException("MIN_DATE")),
                                    DateOnly.Parse(jObject.Value<string>("MAX_DATE") ?? throw new NullReferenceException("MAX_DATE")));
                        }
                }
            } catch(Exception) {
                return null;
            }

            return null;
        }

        public async Task<List<Progress>?> GetProgress(string studentID) {
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
                    using(HttpResponseMessage response = await client.PostAsync("https://tulsu.ru/progress/queries/GetMarks.php", content))
                        if(response.IsSuccessStatusCode) {
                            JArray jObject = JObject.Parse(await response.Content.ReadAsStringAsync()).Value<JArray>("data") ?? throw new Exception();
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

        public async Task<string?> GetTeacherInfo(string teacher) {
            try {
                using(var client = new HttpClient(clientHandler, false)) {
                    #region RequestHeaders
                    client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                    client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                    client.DefaultRequestHeaders.Add("Accept-Language", "ru,en;q=0.9,en-GB;q=0.8,en-US;q=0.7");
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36 Edg/119.0.0.0");

                    client.DefaultRequestHeaders.Add("sec-ch-ua", "\"Microsoft Edge\";v=\"119\", \"Chromium\";v=\"119\", \"Not?A_Brand\";v=\"24\"");
                    client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
                    client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "\"Windows\"");
                    client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
                    client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
                    client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "none");
                    client.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
                    client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
                    client.DefaultRequestHeaders.Add("Connection", "keep-alive");
                    client.DefaultRequestHeaders.Add("Host", "tulsu.ru");

                    client.Timeout = TimeSpan.FromSeconds(10);

                    #endregion

                    using(HttpResponseMessage response = await client.GetAsync($"https://tulsu.ru/polytech/search?text={teacher}&accurate=on")) {
                        if(response.IsSuccessStatusCode) {
                            var html = new HtmlDocument();
                            html.LoadHtml(await response.Content.ReadAsStringAsync());

                            HtmlNodeCollection? nodes = html.DocumentNode.SelectNodes($"//a[contains(@href, 'https://tulsu.ru/employees/')]");

                            if(nodes is not null) {
                                return nodes.First().GetAttributeValue("href", ""); ;
                            }
                        }
                    }
                }
            } catch(Exception) {
                return null;
            }

            return null;
        }

        public async Task UpdatingTeacherInfo(ScheduleDbContext dbContext, string teacher) {
            TeacherLastUpdate? teacherLastUpdate = dbContext.TeacherLastUpdate.FirstOrDefault(i => i.Teacher == teacher);
            if(teacherLastUpdate is not null) {

                string? info = await GetTeacherInfo(teacher);

                if(info is not null)
                    teacherLastUpdate.LinkProfile = info;

                await dbContext.SaveChangesAsync();
            }
        }
    }
}

