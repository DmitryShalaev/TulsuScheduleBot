using System.Globalization;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

namespace ScheduleBot.Scheduler {
    public class Scheduler {
        public static Dictionary<DB.Entity.Type, string> TypeToString = new(){ { DB.Entity.Type.all, "Все"}, { DB.Entity.Type.lab, "Лаб. занятия" }, { DB.Entity.Type.practice, "Практические занятия" } };
        private readonly ScheduleDbContext dbContext;

        public Scheduler(ScheduleDbContext dbContext) => this.dbContext = dbContext;

        public List<string> GetScheduleByWeak(int weeks, ScheduleProfile profile) {
            var dateOnly = DateOnly.FromDateTime(new DateTime(DateTime.Now.Year, 1, 1));

            var schedules = new List<string>();

            for(int i = 1; i < 7; i++)
                schedules.Add(GetScheduleByDate(dateOnly.AddDays(7 * weeks + i), profile));

            return schedules;
        }

        public string GetScheduleByDate(DateOnly date, ScheduleProfile profile, bool all = false) {

            var completedDisciplines = dbContext.CompletedDisciplines.Where(i => i.ScheduleProfileGuid == profile.ID).ToList();

            var list = dbContext.Disciplines.ToList().Where(i => i.Group == profile.Group && i.Date == date && (all || !completedDisciplines.Contains(i))).ToList();

            list.AddRange(dbContext.CustomDiscipline.Where(i => i.ScheduleProfileGuid == profile.ID && i.Date == date).Select(i => new Discipline(i)));

            list = list.OrderBy(i => i.StartTime).ToList();

            int weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Parse(date.ToString()), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            string str = $"📌{date.ToString("dd.MM.yy")} - {char.ToUpper(date.ToString("dddd")[0]) + date.ToString("dddd").Substring(1)} ({(weekNumber % 2 == 0 ? "чётная неделя":"нечётная неделя")})\n⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯\n";

            if(!list.Any())
                return str += "Ничего нет";

            foreach(var item in list) {
                str += $"⏰ {item.StartTime.ToString("HH:mm")}-{item.EndTime.ToString("HH:mm")} | {item.LectureHall}\n" +
                       $"📎 {item.Name} ({item.Type}) {(!string.IsNullOrEmpty(item.Subgroup) ? item.Subgroup : "")}\n" +
                       $"{(!string.IsNullOrWhiteSpace(item.Lecturer) ? $"✒ {item.Lecturer}\n" : "")}\n";
            }

            return str;
        }

        public string GetProgressByTerm(int term, string StudentID) {
            var list = dbContext.Progresses.Where(i => i.StudentID == StudentID && i.Term == term && i.Mark != null);

            string str = $"📌 Семестр {term}\n⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯\n";

            if(!list.Any())
                return str += "В этом семестре нет проставленных баллов";

            foreach(var item in list)
                str += $"🔹 {item.Discipline} | {item.Mark} | {item.MarkTitle}\n";

            return str;
        }

        public List<string> GetScheduleByDay(DayOfWeek dayOfWeek, ScheduleProfile profile) {
            int weeks = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            var dateOnly = DateOnly.FromDateTime(new DateTime(DateTime.Now.Year, 1, 1));
            var list = new List<string>();

            for(int i = -1; i < 2; i++)
                list.Add(GetScheduleByDate(dateOnly.AddDays(7 * (weeks + i) + (byte)dayOfWeek), profile));

            return list;
        }
    }
}
