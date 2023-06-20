using System.Globalization;

using Microsoft.EntityFrameworkCore;

using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.Bot {
    public partial class TelegramBot {

        async Task UpdatedDisciplinesAsync(List<(string Group, DateOnly Date)> values) {
            var telegramUsers = dbContext.TelegramUsers.Include(u => u.Notifications).Where(i => i.Notifications != null).Include(u => u.ScheduleProfile).ToList();

            foreach(var item in values) {
                int weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Parse(item.Date.ToString()), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                string str = $"{item.Date.ToString("dd.MM.yy")} - {char.ToUpper(item.Date.ToString("dddd")[0]) + item.Date.ToString("dddd").Substring(1)} ({(weekNumber % 2 == 0 ? "чётная неделя" : "нечётная неделя")})";

                var days = (DateTime.Parse(item.Date.ToString()) - DateTime.Now).TotalDays;

                foreach(var user in telegramUsers.Where(i => i.ScheduleProfile.Group == item.Group && days <= i.Notifications?.Days)) {
                    await botClient.SendTextMessageAsync(chatId: user.ChatID, text: str,
                            replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(text: commands.Callback["All"].text, callbackData: $"{commands.Callback["All"].callback} {item.Date}")),
                            disableNotification: true);
                }
            }
        }
    }
}
