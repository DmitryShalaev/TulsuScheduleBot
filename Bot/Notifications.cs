using System.Globalization;

using Microsoft.EntityFrameworkCore;

using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ScheduleBot.Bot {
    public partial class TelegramBot {

        async Task UpdatedDisciplinesAsync(List<(string Group, DateOnly Date)> values) {
            var telegramUsers = dbContext.TelegramUsers.Include(u => u.ScheduleProfile);

            foreach(var item in values) {
                int weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Parse(item.Date.ToString()), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                string str = $"{item.Date.ToString("dd.MM.yy")} - {char.ToUpper(item.Date.ToString("dddd")[0]) + item.Date.ToString("dddd").Substring(1)} ({(weekNumber % 2 == 0 ? "чётная неделя" : "нечётная неделя")})";

                foreach(var user in telegramUsers.Where(i => i.ScheduleProfile.Group == item.Group)) {
                    await botClient.SendTextMessageAsync(chatId: user.ChatID, text: str,
                            replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(text: commands.Callback["All"].text, callbackData: $"{commands.Callback["All"].callback} {item.Date}")),
                            disableNotification: true);
                }
            }
        }
    }
}
