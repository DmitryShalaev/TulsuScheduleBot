using System.Data;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

using Core.Bot;
using Core.Bot.Commands;
using Core.DB;
using Core.DB.Entity;

using HtmlAgilityPack;

using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json.Linq;

using Npgsql;

namespace ScheduleBot {
    public partial class ScheduleParser {
        private readonly HttpClientHandler clientHandler;

        private static ScheduleParser? instance;

        public static ScheduleParser Instance => instance ??= new ScheduleParser();

        private ScheduleParser() {
            clientHandler = new() {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip | DecompressionMethods.None,

                //Proxy = new WebProxy("127.0.0.1:8888"),
            };

            Task.Run(GetTeachersData);
        }

        public async Task GetTeachersData() {
            using(ScheduleDbContext dbContext = new()) {

                await UpdatingData(dbContext);

                var teachers = dbContext.Disciplines.Include(i => i.TeacherLastUpdate).Where(i => i.Lecturer != null && string.IsNullOrEmpty(i.TeacherLastUpdate.LinkProfile)).Select(i => i.Lecturer!).Distinct().ToList();
                foreach(string item in teachers) {
                    await UpdatingTeacherInfo(dbContext, item);
                    await Task.Delay(TimeSpan.FromSeconds(10));
                }
            }
        }

        public async Task<bool> UpdatingProgress(ScheduleDbContext dbContext, string studentID, int updateAttemptTime) {
            StudentIDLastUpdate? studentIDLastUpdate = await dbContext.StudentIDLastUpdate.FirstOrDefaultAsync(i => i.StudentID == studentID);
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
                    _list = [.. dbContext.Progresses.Where(i => i.StudentID == studentID)];
                }

                except = _list.Except(progress);
                if(except.Any())
                    dbContext.Progresses.RemoveRange(except);

                await dbContext.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<bool> UpdatingDisciplines(ScheduleDbContext dbContext, string group, int updateAttemptTime, (DateOnly min, DateOnly max, string searchField)? lastDates = null, List<Discipline>? lastDisciplines = null) {
            GroupLastUpdate? groupLastUpdate = await dbContext.GroupLastUpdate.FirstOrDefaultAsync(i => i.Group == group);
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

            (DateOnly min, DateOnly max, string searchField)? dates = lastDates ?? await GetDates(group);

            if(dates is not null && dates.Value.searchField == "GROUP_P") {

                List<Discipline>? disciplines = lastDisciplines ?? await GetDisciplines(group);

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
                        var dd = except.ToList();
                        await dbContext.Disciplines.AddRangeAsync(except);

                        if(_list.Count != 0)
                            updatedDisciplines.AddRange(except);

                        try {
                            await dbContext.SaveChangesAsync();
                        } catch(DbUpdateException ex) {
                            return await PostgresExceptionHandling(dbContext, ex) && await UpdatingDisciplines(dbContext, group, 0, dates, lastDisciplines);
                        }

                        _list = [.. dbContext.Disciplines.Where(i => i.Group == group)];
                    }

                    except = _list.Except(disciplines);
                    if(except.Any()) {
                        dbContext.Disciplines.RemoveRange(except);

                        updatedDisciplines.AddRange(except);

                        await dbContext.DeletedDisciplines.AddRangeAsync(except.Select(i => new DeletedDisciplines(i)));
                    }

                    await dbContext.SaveChangesAsync();

                    await IntersectionOfSubgroups(dbContext, group);

                    if(updatedDisciplines.Count != 0) {
                        var date = DateOnly.FromDateTime(DateTime.Now);
                        var _updatedDisciplines = updatedDisciplines.Where(i => i.Date >= date).Select(i => (i.Group, i.Date)).Distinct().OrderBy(i => i.Date).ToList();

                        if(_updatedDisciplines.Count != 0)
                            _ = Task.Run(() => Notifications.UpdatedDisciplines(_updatedDisciplines));
                    }

                    return true;
                }
            }

            return false;
        }

        private async Task<bool> PostgresExceptionHandling(ScheduleDbContext dbContext, DbUpdateException ex) {
            dbContext.ClearContext();

            // Получаем исключение, содержащее подробности ошибки
            if(ex.InnerException is PostgresException innerException) {
                // Проверяем код ошибки (23503 указывает на нарушение внешнего ключа)
                if(innerException.SqlState == "23503" && innerException.Detail is not null) {
                    string? missingValue = ExtractMissingValue(innerException.Detail);
                    if(missingValue is null)
                        return false;

                    DateTime updDate = new DateTime(2000, 1, 1).ToUniversalTime();
                    await dbContext.MissingFields.AddAsync(new MissingFields { Field = missingValue });

                    if(innerException.MessageText.Contains("ClassroomLastUpdate")) {

                        await dbContext.ClassroomLastUpdate.AddAsync(new ClassroomLastUpdate { Classroom = missingValue, Update = updDate });

                    } else if(innerException.MessageText.Contains("TeacherLastUpdate")) {

                        await dbContext.TeacherLastUpdate.AddAsync(new TeacherLastUpdate { Teacher = missingValue, Update = updDate });
                    }

                    await dbContext.SaveChangesAsync();

                    foreach(TelegramUser? item in dbContext.TelegramUsers.Where(i => i.IsAdmin))
                        Core.Bot.MessagesQueue.Message.SendTextMessage(chatId: item.ChatID, text: $"Обнаружена ошибка парсера: {missingValue}", disableNotification: true);

                    await UpdatingData(dbContext);

                    return true;
                }
            }

            return false;
        }

        [GeneratedRegex(@"\(.+?\)=\((.+?)\)")]
        private static partial Regex ExtractMissingValueRegex();

        private static string? ExtractMissingValue(string detail) {
            Match match = ExtractMissingValueRegex().Match(detail);
            return match.Success ? match.Groups[1].Value : null;
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

        public async Task<bool> UpdatingTeacherWorkSchedule(ScheduleDbContext dbContext, string teacher, int updateAttemptTime, (DateOnly min, DateOnly max, string searchField)? lastDates = null, List<TeacherWorkSchedule>? lastTeacherWorkSchedule = null) {
            TeacherLastUpdate? teacherLastUpdate = await dbContext.TeacherLastUpdate.FirstOrDefaultAsync(i => i.Teacher == teacher);
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

            (DateOnly min, DateOnly max, string searchField)? dates = lastDates ?? await GetDates(teacher);
            if(dates is not null && dates.Value.searchField == "PREP") {

                List<TeacherWorkSchedule>? teacherWorkSchedule = lastTeacherWorkSchedule ?? await GetTeachersWorkSchedule(teacher);

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

                        try {
                            await dbContext.SaveChangesAsync();
                        } catch(DbUpdateException ex) {
                            return await PostgresExceptionHandling(dbContext, ex) && await UpdatingTeacherWorkSchedule(dbContext, teacher, 0, dates, teacherWorkSchedule);
                        }

                        _list = [.. dbContext.TeacherWorkSchedule.Where(i => i.Lecturer == teacher)];
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

        public async Task<bool> UpdatingClassroomWorkSchedule(ScheduleDbContext dbContext, string classroom, int updateAttemptTime, (DateOnly min, DateOnly max, string searchField)? lastDates = null, List<ClassroomWorkSchedule>? lastClassroomWorkSchedule = null) {
            ClassroomLastUpdate? classroomLastUpdate = await dbContext.ClassroomLastUpdate.FirstOrDefaultAsync(i => i.Classroom == classroom);
            if(classroomLastUpdate is null) {
                classroomLastUpdate = new() { Classroom = classroom, Update = DateTime.MinValue.ToUniversalTime(), UpdateAttempt = DateTime.UtcNow };
                dbContext.ClassroomLastUpdate.Add(classroomLastUpdate);
            } else {
                if((DateTime.Now - classroomLastUpdate.UpdateAttempt.ToLocalTime()).TotalMinutes > updateAttemptTime)
                    classroomLastUpdate.UpdateAttempt = DateTime.UtcNow;
                else
                    return false;
            }

            await dbContext.SaveChangesAsync();

            (DateOnly min, DateOnly max, string searchField)? dates = lastDates ?? await GetDates(classroom);
            if(dates is not null && dates.Value.searchField == "AUD") {

                List<ClassroomWorkSchedule>? classroomWorkSchedule = lastClassroomWorkSchedule ?? await GetClassroomWorkSchedule(classroom);

                if(classroomWorkSchedule is not null) {

                    classroomLastUpdate!.Update = DateTime.UtcNow;

                    if(dbContext.ClassroomWorkSchedule.Any(i => i.LectureHall == classroom && (i.Date < dates.Value.min || i.Date > dates.Value.max))) {
                        dbContext.ClassroomWorkSchedule.RemoveRange(dbContext.ClassroomWorkSchedule.Where(i => i.LectureHall == classroom && (i.Date < dates.Value.min || i.Date > dates.Value.max)));
                        await dbContext.SaveChangesAsync();
                    }

                    var _list = dbContext.ClassroomWorkSchedule.Where(i => i.LectureHall == classroom).ToList();

                    IEnumerable<ClassroomWorkSchedule> except = classroomWorkSchedule.Except(_list);
                    if(except.Any()) {
                        dbContext.ClassroomWorkSchedule.AddRange(except);

                        try {
                            await dbContext.SaveChangesAsync();
                        } catch(DbUpdateException ex) {
                            return await PostgresExceptionHandling(dbContext, ex) && await UpdatingClassroomWorkSchedule(dbContext, classroom, 0, dates, classroomWorkSchedule);
                        }

                        _list = [.. dbContext.ClassroomWorkSchedule.Where(i => i.LectureHall == classroom)];
                    }

                    except = _list.Except(classroomWorkSchedule);
                    if(except.Any())
                        dbContext.ClassroomWorkSchedule.RemoveRange(except);

                    await dbContext.SaveChangesAsync();

                    return true;
                }
            }

            return false;
        }

        public async Task UpdatingData(ScheduleDbContext dbContext) {
            JArray? jObject = await GetDictionaries();
            if(jObject == null) return;

            await UpdatingTeachers(dbContext, GetTeachers(dbContext, jObject));

            await UpdatingClassrooms(dbContext, GetClassrooms(dbContext, jObject));
        }

        private static async Task<bool> UpdatingTeachers(ScheduleDbContext dbContext, List<string>? teachers) {
            if(teachers is not null) {
                var _list = dbContext.TeacherLastUpdate.Select(i => i.Teacher).ToList();

                IEnumerable<string> except = teachers.Except(_list);
                var ff = except.ToList();
                if(except.Any()) {
                    DateTime updDate = new DateTime(2000, 1, 1).ToUniversalTime();
                    await dbContext.TeacherLastUpdate.AddRangeAsync(except.Select(i => new TeacherLastUpdate() { Teacher = i, Update = updDate }));

                    await dbContext.SaveChangesAsync();
                    _list = [.. dbContext.TeacherLastUpdate.Select(i => i.Teacher)];
                }

                except = _list.Except(teachers);

                if(except.Any()) {
                    var fd = dbContext.TeacherLastUpdate.Where(i => except.Contains(i.Teacher)).ToList();
                    dbContext.TeacherLastUpdate.RemoveRange(fd);
                }

                await dbContext.SaveChangesAsync();

                return true;
            }

            return false;
        }

        private static async Task<bool> UpdatingClassrooms(ScheduleDbContext dbContext, List<string>? teachers) {
            if(teachers is not null) {
                var _list = dbContext.ClassroomLastUpdate.Select(i => i.Classroom).ToList();

                IEnumerable<string> except = teachers.Except(_list);
                if(except.Any()) {
                    DateTime updDate = new DateTime(2000, 1, 1).ToUniversalTime();
                    await dbContext.ClassroomLastUpdate.AddRangeAsync(except.Select(i => new ClassroomLastUpdate() { Classroom = i, Update = updDate }));

                    await dbContext.SaveChangesAsync();
                    _list = [.. dbContext.ClassroomLastUpdate.Select(i => i.Classroom)];
                }

                except = _list.Except(teachers);

                if(except.Any()) {
                    var fd = dbContext.ClassroomLastUpdate.Where(i => except.Contains(i.Classroom)).ToList();
                    dbContext.ClassroomLastUpdate.RemoveRange(fd);
                }

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

        public async Task<List<ClassroomWorkSchedule>?> GetClassroomWorkSchedule(string classroom) {
            try {
                using(var client = new HttpClient(clientHandler, false)) {
                    #region RequestHeaders
                    client.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
                    client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                    client.DefaultRequestHeaders.Add("Accept-Language", "ru,en;q=0.9,en-GB;q=0.8,en-US;q=0.7");
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36 Edg/112.0.1722.34");
                    client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                    client.DefaultRequestHeaders.Add("Referer", $"https://tulsu.ru/schedule/?search={classroom}");
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

                    using(var content = new StringContent($"search_field=AUD&search_value={classroom}", Encoding.UTF8, "application/x-www-form-urlencoded"))
                    using(HttpResponseMessage response = await client.PostAsync("https://tulsu.ru/schedule/queries/GetSchedule.php", content))
                        if(response.IsSuccessStatusCode) {
                            var jObject = JArray.Parse(await response.Content.ReadAsStringAsync());
                            return jObject.Count == 0 ? throw new Exception() : jObject.Select(j => new ClassroomWorkSchedule(j)).ToList();
                        }
                }
            } catch(Exception) {
                return null;
            }

            return null;
        }

        private async Task<JArray?> GetDictionaries() {
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

                    using(HttpResponseMessage response = await client.GetAsync("https://tulsu.ru/schedule/queries/GetDictionaries.php")) {
                        if(response.IsSuccessStatusCode) {
                            return JArray.Parse(await response.Content.ReadAsStringAsync());
                        }
                    }
                }
            } catch(Exception) {
                return null;
            }

            return null;
        }

        [GeneratedRegex("^[А-ЯЁ][а-яё]+(?:-[А-ЯЁ][а-яё]+)?\\s[А-ЯЁ][а-яё]+(?:-[А-ЯЁ][а-яё]+)?(?:\\s[А-ЯЁа-яё]+(?:\\s[А-ЯЁа-яё]+)*)?$")]
        private static partial Regex TeachersRegex();

        public static List<string>? GetTeachers(ScheduleDbContext dbContext, JArray jObject) {
            try {
                Regex regex = TeachersRegex();
                return jObject.Count == 0 ? throw new Exception() : jObject?.Where(i => {
                    string str = i.Value<string>("value")?.Trim() ?? "";
                    return regex.IsMatch(str) || dbContext.MissingFields.Any(m => m.Field == str);

                }).Select(j => j.Value<string>("value") ?? "").ToList();

            } catch(Exception) {
                return null;
            }
        }

        #region Regex
        [GeneratedRegex("^\\w{1,2}\\d{1,2}-\\d{1,2}-\\d{1,2}$")]
        private static partial Regex ClassroomRegex1();
        [GeneratedRegex("^\\.*\\w+\\d+\\.*$")]
        private static partial Regex ClassroomRegex2();
        [GeneratedRegex("[:/]\\d{2}$")]
        private static partial Regex ClassroomRegex3();
        [GeneratedRegex("^\\.?[А-Я]?\\d{6,7}")]
        private static partial Regex ClassroomRegex4();
        [GeneratedRegex("^(?:[А-я]{3,10}|о)/\\d{2}\\.\\d{2}\\.\\d{2}")]
        private static partial Regex ClassroomRegex5();
        [GeneratedRegex("^\\d\\w?-\\d{6}")]
        private static partial Regex ClassroomRegex6();
        [GeneratedRegex("^[.]{3}\\d{5,6}[а-я]{1,2}")]
        private static partial Regex ClassroomRegex7();
        [GeneratedRegex("^[А-Я][а-яё]+\\s[А-Я]")]
        private static partial Regex ClassroomRegex8();
        [GeneratedRegex("^[КБПМЗИТЦ]{1,3}-\\d{1,3}$")]
        private static partial Regex ClassroomRegex9();

        private static readonly List<Regex> regexes = [
                ClassroomRegex1(),
                ClassroomRegex2(),
                ClassroomRegex3(),
                ClassroomRegex4(),
                ClassroomRegex5(),
                ClassroomRegex6(),
                ClassroomRegex7(),
                ClassroomRegex8(),
                ClassroomRegex9(),
                TeachersRegex()
            ];
        #endregion

        public static List<string>? GetClassrooms(ScheduleDbContext dbContext, JArray jObject) {
            try {
                return jObject?.Count == 0 ? throw new Exception() : (jObject?.Where(i => {
                    string str = i.Value<string>("value")?.Trim() ?? "";

                    return dbContext.MissingFields.Any(m => m.Field == str) || !regexes.Any(r => r.IsMatch(str));

                }).Select(j => j.Value<string>("value") ?? "").ToList());

            } catch(Exception) {
                return null;
            }
        }

        public async Task<(DateOnly min, DateOnly max, string searchField)?> GetDates(string search_value) {
            try {
                using(var client = new HttpClient(clientHandler, false)) {
                    #region RequestHeaders
                    client.DefaultRequestHeaders.Add("Accept", "application/json, text/javascript, */*; q=0.01");
                    client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                    client.DefaultRequestHeaders.Add("Accept-Language", "ru,en;q=0.9,en-GB;q=0.8,en-US;q=0.7");
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36 Edg/112.0.1722.34");
                    client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                    client.DefaultRequestHeaders.Add("Referer", $"https://tulsu.ru/schedule/?search={search_value}");
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

                    using(var content = new StringContent($"search_value={search_value}", Encoding.UTF8, "application/x-www-form-urlencoded"))
                    using(HttpResponseMessage response = await client.PostAsync("https://tulsu.ru/schedule/queries/GetDates.php", content))
                        if(response.IsSuccessStatusCode) {
                            var jObject = JObject.Parse(await response.Content.ReadAsStringAsync());

                            return (DateOnly.Parse(jObject.Value<string>("MIN_DATE") ?? throw new NullReferenceException("MIN_DATE")),
                                    DateOnly.Parse(jObject.Value<string>("MAX_DATE") ?? throw new NullReferenceException("MAX_DATE")),
                                    jObject.Value<string>("SEARCH_FIELD") ?? throw new NullReferenceException("SEARCH_FIELD"));
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
            TeacherLastUpdate? teacherLastUpdate = await dbContext.TeacherLastUpdate.FirstOrDefaultAsync(i => i.Teacher == teacher);
            if(teacherLastUpdate is not null) {

                string? info = await GetTeacherInfo(teacher);

                if(info is not null)
                    teacherLastUpdate.LinkProfile = info;

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
