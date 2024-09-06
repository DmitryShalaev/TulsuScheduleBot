using System.Globalization;

using Core.Bot.MessagesQueue;
using Core.DB;
using Core.DB.Entity;

using Microsoft.EntityFrameworkCore;

using Telegram.Bot.Types.ReplyMarkups;

namespace Core.Bot {
    public class ExtendedTelegramUser(TelegramUser telegramUser) : TelegramUser(telegramUser) {
        public bool Flag { get; set; } = false;
    }

    public static class Notifications {

        public static void UpdatedDisciplines(List<(string, DateOnly)> values) {
            using(ScheduleDbContext dbContext = new()) {

                var telegramUsers = dbContext.TelegramUsers.Include(u => u.Settings).Include(u => u.ScheduleProfile).Where(u => !u.IsDeactivated && u.Settings.NotificationEnabled).Select(u => new ExtendedTelegramUser(u)).ToList();

                foreach((string Group, DateOnly Date) in values) {
                    int weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Parse(Date.ToString()), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                    string str = $"{Date:dd.MM.yy} - {char.ToUpper(Date.ToString("dddd")[0]) + Date.ToString("dddd")[1..]} ({(weekNumber % 2 == 0 ? "чётная неделя" : "нечётная неделя")})";

                    double days = (DateTime.Parse(Date.ToString()) - DateTime.Now.Date).TotalDays;

                    foreach(ExtendedTelegramUser? user in telegramUsers.Where(i => i.ScheduleProfile.Group == Group && days <= i.Settings.NotificationDays)) {
                        if(!user.Flag) {
                            Message.SendTextMessage(chatId: user.ChatID, text: Commands.UserCommands.Instance.Message["NotificationMessage"], disableNotification: true);
                            user.Flag = true;
                        }

                        Message.SendTextMessage(chatId: user.ChatID, text: str,
                                replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(text: Commands.UserCommands.Instance.Callback["All"].text, callbackData: $"NotificationsAll {Date}")),
                                disableNotification: true);
                    }
                }
            }
        }
    }
}
