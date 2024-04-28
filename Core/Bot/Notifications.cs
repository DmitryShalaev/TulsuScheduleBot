using System.Globalization;

using Microsoft.EntityFrameworkCore;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Core.Bot {
    public class ExtendedTelegramUser(TelegramUser telegramUser) : TelegramUser(telegramUser) {
        public bool Flag { get; set; } = false;
    }

    public static class Notifications {
        private static readonly ITelegramBotClient botClient = TelegramBot.Instance.botClient;

        public static async Task UpdatedDisciplinesAsync(ScheduleDbContext dbContext, List<(string, DateOnly)> values) {
            var telegramUsers = dbContext.TelegramUsers.Include(u => u.Settings).Where(u => u.Settings.NotificationEnabled).Include(u => u.ScheduleProfile).Select(u => new ExtendedTelegramUser(u)).ToList();

            foreach((string Group, DateOnly Date) in values) {
                int weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Parse(Date.ToString()), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                string str = $"{Date:dd.MM.yy} - {char.ToUpper(Date.ToString("dddd")[0]) + Date.ToString("dddd")[1..]} ({(weekNumber % 2 == 0 ? "чётная неделя" : "нечётная неделя")})";

                double days = (DateTime.Parse(Date.ToString()) - DateTime.Now.Date).TotalDays;

                foreach(ExtendedTelegramUser? user in telegramUsers.Where(i => i.ScheduleProfile.Group == Group && days <= i.Settings.NotificationDays)) {
                    try {
                        if(!user.Flag) {
                            await botClient.SendTextMessageAsync(chatId: user.ChatID, text: Commands.UserCommands.Instance.Message["NotificationMessage"], disableNotification: true);
                            user.Flag = true;
                        }

                        await botClient.SendTextMessageAsync(chatId: user.ChatID, text: str,
                                replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(text: Commands.UserCommands.Instance.Callback["All"].text, callbackData: $"NotificationsAll {Date}")),
                                disableNotification: true);

                    } catch(Exception) { }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }
        }
    }
}
