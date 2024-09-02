using System.Globalization;
using System.Text;

using Core.DB;
using Core.DB.Entity;

using Microsoft.EntityFrameworkCore;

#pragma warning disable IDE0130 // Пространство имен (namespace) не соответствует структуре папок.

namespace ScheduleBot {

    public static class Scheduler {

        private static DateOnly GetFirstDayOfWeek(DateOnly currentDate) {
            int diff = (7 + (currentDate.DayOfWeek - DayOfWeek.Monday)) % 7;
            return currentDate.AddDays(-1 * diff);
        }

        public static List<((string, bool), DateOnly)> GetScheduleByWeak(ScheduleDbContext dbContext, bool next, TelegramUser user) {
            DateOnly firstDayOfWeek = GetFirstDayOfWeek(DateOnly.FromDateTime(!next ? DateTime.Now : DateTime.Now.AddDays(7)));

            var schedules = new List<((string, bool), DateOnly)>();

            for(int i = 0; i < 7; i++) {
                DateOnly tmp = firstDayOfWeek.AddDays(i);
                schedules.Add((GetScheduleByDate(dbContext, tmp, user), tmp));
            }

            return schedules;
        }

        public static List<(string, DateOnly)> GetTeacherWorkScheduleByWeak(ScheduleDbContext dbContext, bool next, string teacher) {
            DateOnly firstDayOfWeek = GetFirstDayOfWeek(DateOnly.FromDateTime(!next ? DateTime.Now : DateTime.Now.AddDays(7)));

            var schedules = new List<(string, DateOnly)>();

            for(int i = 0; i < 7; i++) {
                DateOnly tmp = firstDayOfWeek.AddDays(i);
                schedules.Add((GetTeacherWorkScheduleByDate(dbContext, tmp, teacher), tmp));
            }

            return schedules;
        }

        public static List<(string, DateOnly)> GetClassroomWorkScheduleByWeak(ScheduleDbContext dbContext, bool next, string classroom, TelegramUser user) {
            DateOnly firstDayOfWeek = GetFirstDayOfWeek(DateOnly.FromDateTime(!next ? DateTime.Now : DateTime.Now.AddDays(7)));

            var schedules = new List<(string, DateOnly)>();

            for(int i = 0; i < 7; i++) {
                DateOnly tmp = firstDayOfWeek.AddDays(i);
                schedules.Add((GetClassroomWorkScheduleByDate(dbContext, tmp, classroom, user), tmp));
            }

            return schedules;
        }

        public static (string, bool) GetScheduleByDate(ScheduleDbContext dbContext, DateOnly date, TelegramUser user, bool all = false, bool link = true) {
            ScheduleProfile profile = user.ScheduleProfile;
            link &= user.Settings.TeacherLincsEnabled;

            // Получаем завершенные дисциплины и дисциплины на указанную дату
            var completedDisciplines = dbContext.CompletedDisciplines
                .Where(i => i.ScheduleProfileGuid == profile.ID && (i.Date == null || i.Date == date))
                .ToList();

            var disciplines = dbContext.Disciplines
                .Include(i => i.TeacherLastUpdate)
                .Where(i => i.Group == profile.Group && i.Date == date)
                .ToList();

            int initialCount = disciplines.Count;

            // Фильтруем дисциплины в зависимости от параметра "all"
            disciplines = [.. disciplines
                .Where(i => all || !completedDisciplines.Contains((CompletedDiscipline)i))
                .OrderBy(i => i.StartTime)];

            bool hasExcludedDisciplines = disciplines.Count < initialCount;

            // Добавляем пользовательские дисциплины
            var customDisciplines = dbContext.CustomDiscipline
                .Where(i => i.IsAdded && i.ScheduleProfileGuid == profile.ID && i.Date == date)
                .Select(i => new Discipline(i))
                .ToList();

            disciplines.AddRange(customDisciplines);
            disciplines = [.. disciplines.OrderBy(i => i.StartTime)];

            // Формируем заголовок уведомления
            int weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                date.ToDateTime(TimeOnly.MinValue),
                CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);

            StringBuilder sb = new StringBuilder()
                .AppendLine($"📌 {date:dd.MM.yy} - {char.ToUpper(date.ToString("dddd")[0]) + date.ToString("dddd")[1..]} ({(weekNumber % 2 == 0 ? "чётная неделя" : "нечётная неделя")})")
                .AppendLine("⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯");

            if(disciplines.Count == 0) {
                return (sb.AppendLine("Ничего нет").ToString(), hasExcludedDisciplines);
            }

            // Формирование строк для каждого предмета
            foreach(Discipline? item in disciplines) {
                sb.AppendLine($"⏰ {item.StartTime:HH:mm}-{item.EndTime:HH:mm} | {item.LectureHall}")
                  .AppendLine($"📎 {item.Name} ({item.Type}) {(string.IsNullOrWhiteSpace(item.Subgroup) ? item.IntersectionMark : item.Subgroup)}");

                if(!string.IsNullOrWhiteSpace(item.Lecturer)) {
                    if(link && !string.IsNullOrWhiteSpace(item.TeacherLastUpdate?.LinkProfile)) {
                        sb.AppendLine($"✒ [{item.Lecturer}]({item.TeacherLastUpdate?.LinkProfile})");
                    } else {
                        sb.AppendLine($"✒ {item.Lecturer}");
                    }
                }

                sb.AppendLine();
            }

            return (sb.ToString(), hasExcludedDisciplines);
        }

        public class ExtendedDiscipline : Discipline {
            public ExtendedDiscipline(Discipline discipline, bool deleted = false) : base(discipline) => Deleted = deleted;

            public ExtendedDiscipline(DeletedDisciplines discipline, bool deleted = false) : base(discipline) => Deleted = deleted;

            public bool Deleted { get; set; }
        }

        public static string GetScheduleByDateNotification(ScheduleDbContext dbContext, DateOnly date, TelegramUser user) {
            ScheduleProfile profile = user.ScheduleProfile;

            // Получение списка дисциплин и удаленных дисциплин
            var disciplines = dbContext.Disciplines
                .Include(i => i.TeacherLastUpdate)
                .Where(i => i.Group == profile.Group && i.Date == date)
                .Select(i => new ExtendedDiscipline(i, false))
                .ToList();

            var deletedDisciplines = dbContext.DeletedDisciplines
                .Include(i => i.TeacherLastUpdate)
                .Where(i => i.Group == profile.Group && i.Date == date)
                .Select(i => new ExtendedDiscipline(i, true))
                .ToList();

            // Объединение и сортировка списка
            var scheduleList = disciplines
                .Concat(deletedDisciplines)
                .OrderBy(i => i.StartTime)
                .ToList();

            // Формирование заголовка уведомления
            int weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                date.ToDateTime(TimeOnly.MinValue),
                CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);

            var sb = new StringBuilder();
            sb.AppendLine($"📌 {date:dd.MM.yy} - {char.ToUpper(date.ToString("dddd")[0]) + date.ToString("dddd")[1..]} ({(weekNumber % 2 == 0 ? "чётная неделя" : "нечётная неделя")})")
              .AppendLine("⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯");

            if(scheduleList.Count == 0) {
                return sb.AppendLine("Ничего нет").ToString();
            }

            bool linkEnabled = user.Settings.TeacherLincsEnabled;

            // Формирование строк для каждого предмета
            foreach(ExtendedDiscipline? item in scheduleList) {
                sb.Append(item.Deleted ? "<s>" : "")
                  .AppendLine($"⏰ {item.StartTime:HH:mm}-{item.EndTime:HH:mm} | {item.LectureHall}")
                  .AppendLine($"📎 {item.Name} ({item.Type}) {(string.IsNullOrWhiteSpace(item.Subgroup) ? item.IntersectionMark : item.Subgroup)}");

                if(!string.IsNullOrWhiteSpace(item.Lecturer)) {
                    if(linkEnabled && !string.IsNullOrWhiteSpace(item.TeacherLastUpdate?.LinkProfile)) {
                        sb.AppendLine($"✒ <a href=\"{item.TeacherLastUpdate.LinkProfile}\">{item.Lecturer}</a>");
                    } else {
                        sb.AppendLine($"✒ {item.Lecturer}");
                    }
                }

                sb.AppendLine(item.Deleted ? "</s>" : "");
            }

            return sb.ToString();
        }

        public static string GetTeacherWorkScheduleByDate(ScheduleDbContext dbContext, DateOnly date, string teacher) {
            var schedules = dbContext.TeacherWorkSchedule
                .Where(i => i.Lecturer == teacher && i.Date == date)
                .OrderBy(i => i.StartTime)
                .ToList();

            int weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                date.ToDateTime(TimeOnly.MinValue),
                CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);

            var sb = new StringBuilder();
            sb.AppendLine($"📌 {date:dd.MM.yy} - {char.ToUpper(date.ToString("dddd")[0]) + date.ToString("dddd")[1..]} ({(weekNumber % 2 == 0 ? "чётная неделя" : "нечётная неделя")})")
              .AppendLine($"👤 {teacher}")
              .AppendLine("⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯");

            if(schedules.Count == 0) {
                return sb.AppendLine("Ничего нет").ToString();
            }

            foreach(TeacherWorkSchedule? item in schedules) {
                sb.AppendLine($"⏰ {item.StartTime:HH:mm}-{item.EndTime:HH:mm} | {item.LectureHall}")
                  .AppendLine($"📎 {item.Name} ({item.Type})")
                  .AppendLine($"👥 {item.Groups}").AppendLine();
            }

            return sb.ToString();
        }

        public static string GetClassroomWorkScheduleByDate(ScheduleDbContext dbContext, DateOnly date, string classroom, TelegramUser user) {
            var schedules = dbContext.ClassroomWorkSchedule.Include(i => i.TeacherLastUpdate)
                .Where(i => i.LectureHall == classroom && i.Date == date)
                .OrderBy(i => i.StartTime)
                .ToList();

            int weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                date.ToDateTime(TimeOnly.MinValue),
                CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);

            var sb = new StringBuilder();
            sb.AppendLine($"📌 {date:dd.MM.yy} - {char.ToUpper(date.ToString("dddd")[0]) + date.ToString("dddd")[1..]} ({(weekNumber % 2 == 0 ? "чётная неделя" : "нечётная неделя")})")
              .AppendLine($"🚪 {classroom}")
              .AppendLine("⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯");

            if(schedules.Count == 0) {
                return sb.AppendLine("Ничего нет").ToString();
            }

            bool linkEnabled = user.Settings.TeacherLincsEnabled;

            foreach(Core.DB.Entity.ClassroomWorkSchedule? item in schedules) {
                sb.AppendLine($"⏰ {item.StartTime:HH:mm}-{item.EndTime:HH:mm}")
                  .AppendLine($"📎 {item.Name} ({item.Type})");

                if(!string.IsNullOrWhiteSpace(item.Lecturer)) {
                    if(linkEnabled && !string.IsNullOrWhiteSpace(item.TeacherLastUpdate?.LinkProfile)) {
                        sb.AppendLine($"✒ [{item.Lecturer}]({item.TeacherLastUpdate?.LinkProfile})");
                    } else {
                        sb.AppendLine($"✒ {item.Lecturer}");
                    }
                }

                sb.AppendLine($"👥 {item.Groups}").AppendLine();
            }

            return sb.ToString();
        }

        public static string GetProgressByTerm(ScheduleDbContext dbContext, int term, string studentID) {
            var progresses = dbContext.Progresses
                .Where(i => i.StudentID == studentID && i.Term == term)
                .OrderBy(i => i.Discipline)
                .ToList();

            var sb = new StringBuilder();

            sb.AppendLine($"📌 Семестр {term}")
              .AppendLine("⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯");

            if(progresses.Count == 0) {
                sb.AppendLine("В этом семестре нет проставленных баллов");
            } else {
                foreach(Progress? item in progresses) {
                    sb.AppendLine($"🔹 {item.Discipline} | {item.Mark} | {item.MarkTitle}");
                }
            }

            return sb.ToString();
        }

        public static List<((string, bool), DateOnly)> GetScheduleByDay(ScheduleDbContext dbContext, DayOfWeek dayOfWeek, TelegramUser user) {
            int weeks = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            var dateOnly = DateOnly.FromDateTime(new DateTime(DateTime.Now.Year, 1, 1));

            var list = new List<((string, bool), DateOnly)>();
            for(int i = -1; i <= 1; i++) {
                DateOnly tmp = dateOnly.AddDays(7 * (weeks + i) + ((byte)dayOfWeek - 1));
                list.Add((GetScheduleByDate(dbContext, tmp, user), tmp));
            }

            return list;
        }

        public static List<(string, DateOnly)> GetTeacherWorkScheduleByDay(ScheduleDbContext dbContext, DayOfWeek dayOfWeek, string teacher) {
            int weeks = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            var dateOnly = DateOnly.FromDateTime(new DateTime(DateTime.Now.Year, 1, 1));

            var list = new List<(string, DateOnly)>();
            for(int i = -1; i <= 1; i++) {
                DateOnly tmp = dateOnly.AddDays(7 * (weeks + i) + ((byte)dayOfWeek - 1));
                list.Add((GetTeacherWorkScheduleByDate(dbContext, tmp, teacher), tmp));
            }

            return list;
        }

        public static List<(string, DateOnly)> GetClassroomWorkScheduleByDay(ScheduleDbContext dbContext, DayOfWeek dayOfWeek, string classroom, TelegramUser user) {
            int weeks = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            var dateOnly = DateOnly.FromDateTime(new DateTime(DateTime.Now.Year, 1, 1));

            var list = new List<(string, DateOnly)>();
            for(int i = -1; i <= 1; i++) {
                DateOnly tmp = dateOnly.AddDays(7 * (weeks + i) + ((byte)dayOfWeek - 1));
                list.Add((GetClassroomWorkScheduleByDate(dbContext, tmp, classroom, user), tmp));
            }

            return list;
        }

        public static List<string> GetExamse(ScheduleDbContext dbContext, ScheduleProfile profile, bool all) {
            var exams = new List<string>();

            var completedDisciplines = dbContext.CompletedDisciplines.Where(i => i.ScheduleProfileGuid == profile.ID).ToList();

            var disciplines = dbContext.Disciplines.ToList().Where(i => i.Group == profile.Group && (i.Class == Class.def || i.Class == Class.other) && DateTime.Parse($"{i.Date} {i.EndTime}") >= DateTime.Now && !completedDisciplines.Contains((CompletedDiscipline)i)).OrderBy(i => i.Date).ToList();

            if(disciplines.Count == 0) {
                exams.Add("Ничего нет");
                return exams;
            }

            static string Get(Discipline item) {
                int weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Parse(item.Date.ToString()), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                return new StringBuilder()
                    .AppendLine($"📌{item.Date:dd.MM.yy} - {char.ToUpper(item.Date.ToString("dddd")[0]) + item.Date.ToString("dddd")[1..]} ({(weekNumber % 2 == 0 ? "чётная неделя" : "нечётная неделя")})")
                    .AppendLine("⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯")
                    .AppendLine($"⏰ {item.StartTime:HH:mm}-{item.EndTime:HH:mm} | {item.LectureHall}")
                    .AppendLine($"📎 {item.Name} ({item.Type}) {(!string.IsNullOrWhiteSpace(item.Subgroup) ? item.Subgroup : "")}")
                    .AppendLine(!string.IsNullOrWhiteSpace(item.Lecturer) ? $"✒ {item.Lecturer}" : string.Empty)
                    .ToString();
            }

            if(all) {
                exams.AddRange(disciplines.Select(Get));

            } else {
                Discipline nearestExam = disciplines.First();
                int daysUntilExam = (DateTime.Parse(nearestExam.Date.ToString()).Date - DateTime.Now.Date).Days;

                // Определяем сообщение о времени до ближайшего экзамена
                string message = daysUntilExam switch {
                    0 => "Ближайший экзамен сегодня",
                    1 => "Ближайший экзамен завтра",
                    2 or 3 or 4 => $"Ближайший экзамен через {daysUntilExam} дня.",
                    _ => $"Ближайший экзамен через {daysUntilExam} дней."
                };

                exams.Add($"{message}\n\n{Get(nearestExam)}");
            }

            return exams;
        }
    }
}
