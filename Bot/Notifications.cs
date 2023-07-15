using Microsoft.EntityFrameworkCore;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using System.Globalization;

using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.Bot
{
    public class ExtendedTelegramUser : TelegramUser
    {
        public ExtendedTelegramUser(TelegramUser telegramUser) : base(telegramUser) { }
        public bool Flag { get; set; } = false;
    }

    public partial class TelegramBot
    {

        private async Task UpdatedDisciplinesAsync(ScheduleDbContext dbContext, List<(string Group, DateOnly Date)> values)
        {
            var telegramUsers = dbContext.TelegramUsers.Include(u => u.Notifications).Where(u => u.Notifications.IsEnabled).Include(u => u.ScheduleProfile).Select(u => new ExtendedTelegramUser(u)).ToList();

            foreach ((string Group, DateOnly Date) in values)
            {
                int weekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Parse(Date.ToString()), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                string str = $"{Date:dd.MM.yy} - {char.ToUpper(Date.ToString("dddd")[0]) + Date.ToString("dddd")[1..]} ({(weekNumber % 2 == 0 ? "чётная неделя" : "нечётная неделя")})";

                double days = (DateTime.Parse(Date.ToString()) - DateTime.Now.Date).TotalDays;

                foreach (ExtendedTelegramUser? user in telegramUsers.Where(i => i.ScheduleProfile.Group == Group && days <= i.Notifications.Days))
                {
                    if (!user.Flag)
                    {
                        await botClient.SendTextMessageAsync(chatId: user.ChatID, text: commands.Message["NotificationMessage"], disableNotification: true);
                        user.Flag = true;
                    }

                    await botClient.SendTextMessageAsync(chatId: user.ChatID, text: str,
                            replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(text: commands.Callback["All"].text, callbackData: $"{commands.Callback["All"].callback} {Date}")),
                            disableNotification: true);
                }
            }
        }
    }
}
