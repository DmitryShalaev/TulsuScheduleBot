using System.Globalization;

using Microsoft.EntityFrameworkCore;

using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.Bot {
    public class ExtendedTelegramUser : TelegramUser {
        public ExtendedTelegramUser(TelegramUser telegramUser) : base(telegramUser) { }
        public bool Flag { get; set; } = false;
    }

    public partial class TelegramBot {

        private async Task UpdatedDisciplinesAsync(List<(string Group, DateOnly Date)> values) {
            var telegramUsers = dbContext.TelegramUsers.Include(u => u.Notifications).Where(u => u.Notifications.IsEnabled).Include(u => u.ScheduleProfile).Select(u => new ExtendedTelegramUser(u)).ToList();

            foreach(var item in values) {
                int weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Parse(item.Date.ToString()), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                string str = $"{item.Date.ToString("dd.MM.yy")} - {char.ToUpper(item.Date.ToString("dddd")[0]) + item.Date.ToString("dddd").Substring(1)} ({(weekNumber % 2 == 0 ? "чётная неделя" : "нечётная неделя")})";

                var days = (DateTime.Parse(item.Date.ToString()) - DateTime.Now.Date).TotalDays;

                foreach(var user in telegramUsers.Where(i => i.ScheduleProfile.Group == item.Group && days <= i.Notifications.Days)) {
                    if(!user.Flag) {
                        await botClient.SendTextMessageAsync(chatId: user.ChatID, text: commands.Message["NotificationMessage"], disableNotification: true);
                        user.Flag = true;
                    }

                    await botClient.SendTextMessageAsync(chatId: user.ChatID, text: str,
                            replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(text: commands.Callback["All"].text, callbackData: $"{commands.Callback["All"].callback} {item.Date}")),
                            disableNotification: true);
                }
            }
        }
    }
}
