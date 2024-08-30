using System.Text;

using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;

namespace Core.Bot.Commands.Admin.Statistics.Message {
    internal class Statistics : IMessageCommand {

        public List<string> Commands => ["Статистика"];

        public List<Mode> Modes => [Mode.Admin];

        public Manager.Check Check => Manager.Check.none;

        private static readonly UserCommands.ConfigStruct config = UserCommands.Instance.Config;

        public Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            StringBuilder sb = new();

            DateTime today = DateTime.Now.Date;

            DateTime startOfWeek = today.AddDays(DayOfWeek.Monday - today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            DateTime endOfWeek = startOfWeek.AddDays(7).AddTicks(-1);
            DateTime endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);

            sb.AppendLine($"Всего пользователей: {dbContext.TelegramUsers.Count()}");
            sb.AppendLine($"Администраторы: {dbContext.TelegramUsers.Count(u => u.IsAdmin)}");
            sb.AppendLine();

            sb.AppendLine($"--Новых пользователей--");
            sb.AppendLine($"За сегодня: {dbContext.TelegramUsers.Count(u => u.DateOfRegistration.HasValue && u.DateOfRegistration.Value.ToLocalTime().Date == today)}");
            sb.AppendLine($"За неделю: {dbContext.TelegramUsers.Count(u => u.DateOfRegistration.HasValue && u.DateOfRegistration.Value.ToLocalTime() >= startOfWeek && u.DateOfRegistration.Value.ToLocalTime() <= endOfWeek)}");
            sb.AppendLine($"За месяц: {dbContext.TelegramUsers.Count(u => u.DateOfRegistration.HasValue && u.DateOfRegistration.Value.ToLocalTime() >= startOfMonth && u.DateOfRegistration.Value.ToLocalTime() <= endOfMonth)}");
            sb.AppendLine();

            sb.AppendLine($"--Активных пользователей--");
            sb.AppendLine($"За сегодня: {dbContext.TelegramUsers.Count(u => u.LastAppeal.ToLocalTime().Date == today)}");
            sb.AppendLine($"За неделю: {dbContext.TelegramUsers.Count(u => u.LastAppeal.ToLocalTime() >= startOfWeek && u.LastAppeal.ToLocalTime() <= endOfWeek)}");
            sb.AppendLine($"За месяц: {dbContext.TelegramUsers.Count(u => u.LastAppeal.ToLocalTime() >= startOfMonth && u.LastAppeal.ToLocalTime() <= endOfMonth)}");
            sb.AppendLine();

            sb.AppendLine($"--Топ пользователей по активности--");
            var topUsers = dbContext.TelegramUsers.OrderByDescending(u => u.TotalRequests).Take(5).ToList();
            foreach(TelegramUser? topUser in topUsers) {
                if(!string.IsNullOrWhiteSpace(topUser.Username)) {
                    sb.AppendLine($"[{Statics.EscapeSpecialCharacters($"{topUser.FirstName} {topUser.LastName}")}](https://t.me/{topUser.Username})");
                } else {
                    sb.AppendLine($"{Statics.EscapeSpecialCharacters($"{topUser.FirstName} {topUser.LastName}")}");
                }
            }

            sb.AppendLine();

            sb.AppendLine($"--Получено сообщений--");
            sb.AppendLine($"Всего сообщений: {dbContext.MessageLog.Count()}");
            sb.AppendLine($"За сегодня: {dbContext.TelegramUsers.Sum(u => u.TodayRequests)}");
            sb.AppendLine($"За неделю: {dbContext.MessageLog.Count(ml => ml.Date.ToLocalTime() >= startOfWeek && ml.Date.ToLocalTime() <= endOfWeek)}");
            sb.AppendLine($"За месяц: {dbContext.MessageLog.Count(ml => ml.Date.ToLocalTime() >= startOfMonth && ml.Date.ToLocalTime() <= endOfMonth)}");
            sb.AppendLine();

            sb.AppendLine($"Среднее количество запросов на пользователя: {dbContext.TelegramUsers.Average(u => u.TotalRequests):F2}");
            sb.AppendLine();

            sb.AppendLine($"--Распределение сообщений по времени--");
            int morningMessages = dbContext.MessageLog.Count(ml => ml.Date.ToLocalTime().TimeOfDay < TimeSpan.FromHours(12));
            int afternoonMessages = dbContext.MessageLog.Count(ml => ml.Date.ToLocalTime().TimeOfDay >= TimeSpan.FromHours(12) && ml.Date.ToLocalTime().TimeOfDay < TimeSpan.FromHours(18));
            int eveningMessages = dbContext.MessageLog.Count(ml => ml.Date.ToLocalTime().TimeOfDay >= TimeSpan.FromHours(18));
            sb.AppendLine($"Утро (00:00 - 12:00): {morningMessages}");
            sb.AppendLine($"День (12:00 - 18:00): {afternoonMessages}");
            sb.AppendLine($"Вечер (18:00 - 24:00): {eveningMessages}");
            var mostActiveHour = dbContext.MessageLog
                                        .GroupBy(ml => ml.Date.ToLocalTime().Hour)
                                        .OrderByDescending(g => g.Count())
                                        .Select(g => new { Hour = g.Key, Count = g.Count() })
                                        .FirstOrDefault();
            sb.AppendLine($"Самое активное время: {mostActiveHour?.Hour}:00 - {mostActiveHour?.Hour + 1}:00 с {mostActiveHour?.Count} сообщениями");

            sb.AppendLine();
            sb.AppendLine($"Общее количество обновляемых групп: {dbContext.ScheduleProfile.Where(i => !string.IsNullOrEmpty(i.Group) && (DateTime.Now - i.LastAppeal.ToLocalTime()).TotalDays <= config.DisciplineUpdateDays).Select(i => i.Group!).Distinct().Count()}");

            var groups = dbContext.GroupLastUpdate.OrderByDescending(i => i.Update.ToLocalTime()).ToList();
            if(groups.Count > 1) {
                sb.AppendLine($"Время между последними обновлениями: {(groups[0].Update.ToLocalTime() - groups[1].Update.ToLocalTime()).ToString(@"hh\:mm\:ss")}");
            }

            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: sb.ToString(), replyMarkup: Statics.AdminPanelKeyboardMarkup, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);

            return Task.CompletedTask;
        }
    }
}