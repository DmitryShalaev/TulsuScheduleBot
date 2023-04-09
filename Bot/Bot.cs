using System.Globalization;

namespace ScheduleBot.Bot {
    public partial class TelegramBot {

        public string GetScheduleByDate(DateOnly date) {
            var list = dbContext.Disciplines.Where(i => i.Date == date && !i.IsCompleted);

            if(!list.Any())
                return "Сегодня ничего нет";

            int weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Parse(date.ToString()), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            string str = $"📌{date.ToString("dd.MM.yy")} - {char.ToUpper(date.ToString("dddd")[0]) + date.ToString("dddd").Substring(1)} ({(weekNumber % 2 == 0 ? "чётная неделя":"нечётная неделя")})\n" +
                         $"⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯⋯\n";

            foreach(var item in list) {
                str += $"⏰ {item.StartTime.ToString("HH:mm")}-{item.EndTime.ToString("HH:mm")} | {item.LectureHall}\n" +
                       $"📎 {item.Name} ({item.Type}) {(!string.IsNullOrEmpty(item.Subgroup) ? $"({item.Subgroup})" : "")}\n" +
                       $"✒ {item.Lecturer}\n\n";
            }

            return str;
        }

    }
}
