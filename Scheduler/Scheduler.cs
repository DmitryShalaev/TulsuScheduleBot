using System.Globalization;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

namespace ScheduleBot.Scheduler {
    public class Scheduler {
        public static Dictionary<DB.Entity.Class, string> TypeToString = new(){ { DB.Entity.Class.all, "Все"}, { DB.Entity.Class.lab, "Лаб. занятия" }, { DB.Entity.Class.practice, "Практические занятия" } };
        private readonly ScheduleDbContext dbContext;

        public Scheduler(ScheduleDbContext dbContext) => this.dbContext = dbContext;

        public List<(string, DateOnly)> GetScheduleByWeak(int weeks, ScheduleProfile profile) {
            var dateOnly = DateOnly.FromDateTime(new DateTime(DateTime.Now.Year, 1, 1));

            var schedules = new List<(string, DateOnly)>();

            for(int i = 1; i < 7; i++) {
                var tmp = dateOnly.AddDays(7 * weeks + i);
                schedules.Add((GetScheduleByDate(tmp, profile), tmp));
            }

            return schedules;
        }

        public string GetScheduleByDate(DateOnly date, ScheduleProfile profile, bool all = false) {
            var completedDisciplines = dbContext.CompletedDisciplines.Where(i => i.ScheduleProfileGuid == profile.ID).ToList();

            var list = dbContext.Disciplines.ToList().Where(i => i.Group == profile.Group && i.Date == date && (all || !completedDisciplines.Contains((CompletedDiscipline)i))).ToList();

            list.AddRange(dbContext.CustomDiscipline.Where(i => i.ScheduleProfileGuid == profile.ID && i.Date == date).Select(i => new Discipline(i)));

            list = list.OrderBy(i => i.StartTime).ToList();

            int weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Parse(date.ToString()), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            string str = $"📌{date.ToString("dd.MM.yy")} - {char.ToUpper(date.ToString("dddd")[0]) + date.ToString("dddd").Substring(1)} ({(weekNumber % 2 == 0 ? "чётная неделя":"нечётная неделя")})\n⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯\n";

            if(list.Count() == 0)
                return str += "Ничего нет";

            foreach(var item in list) {
                str += $"⏰ {item.StartTime.ToString("HH:mm")}-{item.EndTime.ToString("HH:mm")} | {item.LectureHall}\n" +
                       $"📎 {item.Name} ({item.Type}) {(!string.IsNullOrWhiteSpace(item.Subgroup) ? item.Subgroup : "")}\n" +
                       $"{(!string.IsNullOrWhiteSpace(item.Lecturer) ? $"✒ {item.Lecturer}\n" : "")}\n";
            }
            return str;
        }

        public string GetProgressByTerm(int term, string StudentID) {
            var progresses = dbContext.Progresses.Where(i => i.StudentID == StudentID && i.Term == term && i.Mark != null);

            string str = $"📌 Семестр {term}\n⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯\n";

            if(progresses.Count() == 0)
                return str += "В этом семестре нет проставленных баллов";

            foreach(var item in progresses)
                str += $"🔹 {item.Discipline} | {item.Mark} | {item.MarkTitle}\n";

            return str;
        }

        public List<(string, DateOnly)> GetScheduleByDay(DayOfWeek dayOfWeek, ScheduleProfile profile) {
            int weeks = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            var dateOnly = DateOnly.FromDateTime(new DateTime(DateTime.Now.Year, 1, 1));

            var list = new List<(string, DateOnly)>();
            for(int i = -1; i <= 1; i++) {
                var tmp = dateOnly.AddDays(7 * (weeks + i) + (byte)dayOfWeek);
                list.Add((GetScheduleByDate(tmp, profile), tmp));
            }
            return list;
        }

        public List<string> GetExamse(ScheduleProfile profile, bool all) {
            var exams = new List<string>();

            var completedDisciplines = all ? new() : dbContext.CompletedDisciplines.Where(i => i.ScheduleProfileGuid == profile.ID).ToList();
            var disciplines = dbContext.Disciplines.ToList().Where(i => i.Group == profile.Group && i.Class == Class.other && i.Date >= DateOnly.FromDateTime(DateTime.Now.Date) && !completedDisciplines.Contains((CompletedDiscipline)i)).OrderBy(i => i.Date);

            if(disciplines.Count() == 0) {
                exams.Add("Ничего нет");
                return exams;
            }

            if(all) {
                foreach(var item in disciplines)
                    exams.Add(Get(item));

            } else {
                var item = disciplines.First();

                var via = (DateTime.Parse(item.Date.ToString()).Date - DateTime.Now.Date).Days;

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

                    case var _ when via > 0:
                        exams.Add($"Ближайший экзамен через {via} дней.");
                        break;
                }
                #endregion

                exams[0]+=$"\n\n{Get(item)}";
            }
            return exams;
        }

        string Get(Discipline item) {
            int weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Parse(item.Date.ToString()), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            return $"📌{item.Date.ToString("dd.MM.yy")} - {char.ToUpper(item.Date.ToString("dddd")[0]) + item.Date.ToString("dddd").Substring(1)} ({(weekNumber % 2 == 0 ? "чётная неделя" : "нечётная неделя")})\n⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯\n" +
                   $"⏰ {item.StartTime.ToString("HH:mm")}-{item.EndTime.ToString("HH:mm")} | {item.LectureHall}\n" +
                   $"📎 {item.Name} ({item.Type}) {(!string.IsNullOrWhiteSpace(item.Subgroup) ? item.Subgroup : "")}\n" +
                   $"{(!string.IsNullOrWhiteSpace(item.Lecturer) ? $"✒ {item.Lecturer}\n" : "")}\n";
        }
    }
}
