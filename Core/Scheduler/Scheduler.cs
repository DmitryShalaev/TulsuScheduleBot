using System.Globalization;

using Microsoft.EntityFrameworkCore;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

namespace ScheduleBot {
    public static class Scheduler {
        public static List<((string, bool), DateOnly)> GetScheduleByWeak(ScheduleDbContext dbContext, int weeks, TelegramUser user) {
            var dateOnly = DateOnly.FromDateTime(new DateTime(DateTime.Now.Year, 1, 1));

            var schedules = new List<((string, bool), DateOnly)>();

            for(int i = 1; i < 7; i++) {
                DateOnly tmp = dateOnly.AddDays(7 * weeks + i);
                schedules.Add((GetScheduleByDate(dbContext, tmp, user), tmp));
            }

            return schedules;
        }

        public static List<(string, DateOnly)> GetTeacherWorkScheduleByWeak(ScheduleDbContext dbContext, int weeks, string teacher) {
            var dateOnly = DateOnly.FromDateTime(new DateTime(DateTime.Now.Year, 1, 1));

            var schedules = new List<(string, DateOnly)>();

            for(int i = 1; i < 7; i++) {
                DateOnly tmp = dateOnly.AddDays(7 * weeks + i);
                schedules.Add((GetTeacherWorkScheduleByDate(dbContext, tmp, teacher), tmp));
            }

            return schedules;
        }

        public static (string, bool) GetScheduleByDate(ScheduleDbContext dbContext, DateOnly date, TelegramUser user, bool all = false, bool link = true) {
            ScheduleProfile profile = user.ScheduleProfile;

            link &= user.Settings.TeacherLincsEnabled;

            var completedDisciplines = dbContext.CompletedDisciplines.Where(i => i.ScheduleProfileGuid == profile.ID && (i.Date == null || i.Date == date)).ToList();

            var list = dbContext.Disciplines.Include(i => i.TeacherLastUpdate).Where(i => i.Group == profile.Group && i.Date == date).ToList();

            int count = list.Count;

            list = list.Where(i => all || !completedDisciplines.Contains((CompletedDiscipline)i)).ToList();

            bool flag = list.Count < count;

            list.AddRange(dbContext.CustomDiscipline.Where(i => i.IsAdded && i.ScheduleProfileGuid == profile.ID && i.Date == date).Select(i => new Discipline(i)));

            list = list.OrderBy(i => i.StartTime).ToList();

            int weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Parse(date.ToString()), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            string str = $"📌 {date:dd.MM.yy} - {char.ToUpper(date.ToString("dddd")[0]) + date.ToString("dddd")[1..]} ({(weekNumber % 2 == 0 ? "чётная неделя" : "нечётная неделя")})\n⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯\n";

            if(list.Count == 0)
                return (str += "Ничего нет", flag);

            foreach(Discipline? item in list) {
                str += $"⏰ {item.StartTime:HH:mm}-{item.EndTime:HH:mm} | {item.LectureHall}\n" +
                       $"📎 {item.Name} ({item.Type}) {(!string.IsNullOrWhiteSpace(item.Subgroup) ? item.Subgroup : "")}\n" +
                       (link ? $"{(!string.IsNullOrWhiteSpace(item.Lecturer) ? $"✒ [{item.Lecturer}]({item.TeacherLastUpdate?.LinkProfile})\n" : "")}\n" :
                               $"{(!string.IsNullOrWhiteSpace(item.Lecturer) ? $"✒ {item.Lecturer}\n" : "")}\n");
            }

            return (str, flag);
        }

        public static string GetTeacherWorkScheduleByDate(ScheduleDbContext dbContext, DateOnly date, string teacher) {
            var list = dbContext.TeacherWorkSchedule.ToList().Where(i => i.Lecturer == teacher && i.Date == date).ToList();

            int weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Parse(date.ToString()), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            string str = $"📌 {date:dd.MM.yy} - {char.ToUpper(date.ToString("dddd")[0]) + date.ToString("dddd")[1..]} ({(weekNumber % 2 == 0 ? "чётная неделя" : "нечётная неделя")})\n" +
                            $"👤 {teacher}\n" +
                            $"⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯";

            if(list.Count == 0)
                return str += "\nНичего нет";

            foreach(TeacherWorkSchedule? item in list) {
                str += $"\n⏰ {item.StartTime:HH:mm}-{item.EndTime:HH:mm} | {item.LectureHall}\n" +
                       $"📎 {item.Name} ({item.Type})\n" +
                       $"{item.Groups}";
            }

            return str;
        }

        public static string GetProgressByTerm(ScheduleDbContext dbContext, int term, string StudentID) {
            IOrderedQueryable<Progress> progresses = dbContext.Progresses.Where(i => i.StudentID == StudentID && i.Term == term).OrderBy(i => i.Discipline);

            string str = $"📌 Семестр {term}\n" +
                            $"⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯\n";

            if(!progresses.Any())
                return str += "В этом семестре нет проставленных баллов";

            foreach(Progress? item in progresses)
                str += $"🔹 {item.Discipline} | {item.Mark} | {item.MarkTitle}\n";

            return str;
        }

        public static List<((string, bool), DateOnly)> GetScheduleByDay(ScheduleDbContext dbContext, DayOfWeek dayOfWeek, TelegramUser user) {
            int weeks = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            var dateOnly = DateOnly.FromDateTime(new DateTime(DateTime.Now.Year, 1, 1));

            var list = new List<((string, bool), DateOnly)>();
            for(int i = -1; i <= 1; i++) {
                DateOnly tmp = dateOnly.AddDays(7 * (weeks + i) + (byte)dayOfWeek);
                list.Add((GetScheduleByDate(dbContext, tmp, user), tmp));
            }

            return list;
        }

        public static List<(string, DateOnly)> GetTeacherWorkScheduleByDay(ScheduleDbContext dbContext, DayOfWeek dayOfWeek, string teacher) {
            int weeks = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            var dateOnly = DateOnly.FromDateTime(new DateTime(DateTime.Now.Year, 1, 1));

            var list = new List<(string, DateOnly)>();
            for(int i = -1; i <= 1; i++) {
                DateOnly tmp = dateOnly.AddDays(7 * (weeks + i) + (byte)dayOfWeek);
                list.Add((GetTeacherWorkScheduleByDate(dbContext, tmp, teacher), tmp));
            }

            return list;
        }

        public static List<string> GetExamse(ScheduleDbContext dbContext, ScheduleProfile profile, bool all) {
            var exams = new List<string>();

            var completedDisciplines = dbContext.CompletedDisciplines.Where(i => i.ScheduleProfileGuid == profile.ID).ToList();
            IOrderedEnumerable<Discipline> disciplines = dbContext.Disciplines.ToList().Where(i => i.Group == profile.Group && (i.Class == Class.def || i.Class == Class.other) && DateTime.Parse($"{i.Date} {i.EndTime}") >= DateTime.Now && !completedDisciplines.Contains((CompletedDiscipline)i)).OrderBy(i => i.Date);

            if(!disciplines.Any()) {
                exams.Add("Ничего нет");
                return exams;
            }

            static string Get(Discipline item) {
                int weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Parse(item.Date.ToString()), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                return $"📌{item.Date:dd.MM.yy} - {char.ToUpper(item.Date.ToString("dddd")[0]) + item.Date.ToString("dddd")[1..]} ({(weekNumber % 2 == 0 ? "чётная неделя" : "нечётная неделя")})\n⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯\n" +
                       $"⏰ {item.StartTime:HH:mm}-{item.EndTime:HH:mm} | {item.LectureHall}\n" +
                       $"📎 {item.Name} ({item.Type}) {(!string.IsNullOrWhiteSpace(item.Subgroup) ? item.Subgroup : "")}\n" +
                       $"{(!string.IsNullOrWhiteSpace(item.Lecturer) ? $"✒ {item.Lecturer}\n" : "")}\n";
            }

            if(all) {
                foreach(Discipline? item in disciplines)
                    exams.Add(Get(item));

            } else {
                Discipline item = disciplines.First();

                int via = (DateTime.Parse(item.Date.ToString()).Date - DateTime.Now.Date).Days;

                #region Via
                switch(via) {
                    case 0:
                        exams.Add($"Ближайший экзамен сегодня");
                        break;

                    case 1:
                        exams.Add($"Ближайший экзамен завтра");
                        break;

                    case 2:
                    case 3:
                    case 4:
                        exams.Add($"Ближайший экзамен через {via} дня.");
                        break;

                    case var _ when via > 4:
                        exams.Add($"Ближайший экзамен через {via} дней.");
                        break;
                }
                #endregion

                exams[0] += $"\n\n{Get(item)}";
            }

            return exams;
        }
    }
}
