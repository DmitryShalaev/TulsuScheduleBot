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

            DateTime today = DateTime.UtcNow.Date;

            DateTime startOfWeek = today.AddDays(DayOfWeek.Monday - today.DayOfWeek).ToUniversalTime();
            DateTime startOfMonth = new DateTime(today.Year, today.Month, 1).ToUniversalTime();
            DateTime endOfWeek = startOfWeek.AddDays(7).AddTicks(-1).ToUniversalTime();
            DateTime endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1).ToUniversalTime();

            sb.AppendLine($"Всего пользователей: {dbContext.TelegramUsers.Count()}");
            sb.AppendLine($"Администраторы: {dbContext.TelegramUsers.Count(u => u.IsAdmin)}");
            sb.AppendLine();

            sb.AppendLine($"--Новых пользователей--");
            sb.AppendLine($"За сегодня: {dbContext.TelegramUsers.Count(u => u.DateOfRegistration.HasValue && u.DateOfRegistration.Value.Date == today)}");
            sb.AppendLine($"За неделю: {dbContext.TelegramUsers.Count(u => u.DateOfRegistration.HasValue && u.DateOfRegistration.Value >= startOfWeek && u.DateOfRegistration.Value <= endOfWeek)}");
            sb.AppendLine($"За месяц: {dbContext.TelegramUsers.Count(u => u.DateOfRegistration.HasValue && u.DateOfRegistration.Value >= startOfMonth && u.DateOfRegistration.Value <= endOfMonth)}");
            sb.AppendLine();

            sb.AppendLine($"--Активных пользователей--");
            sb.AppendLine($"За сегодня: {dbContext.TelegramUsers.Count(u => u.LastAppeal.Date == today)}");
            sb.AppendLine($"За неделю: {dbContext.TelegramUsers.Count(u => u.LastAppeal >= startOfWeek && u.LastAppeal <= endOfWeek)}");
            sb.AppendLine($"За месяц: {dbContext.TelegramUsers.Count(u => u.LastAppeal >= startOfMonth && u.LastAppeal <= endOfMonth)}");
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
            sb.AppendLine($"За неделю: {dbContext.MessageLog.Count(ml => ml.Date >= startOfWeek && ml.Date <= endOfWeek)}");
            sb.AppendLine($"За месяц: {dbContext.MessageLog.Count(ml => ml.Date >= startOfMonth && ml.Date <= endOfMonth)}");
            sb.AppendLine();

            sb.AppendLine($"Среднее количество запросов на пользователя: {dbContext.TelegramUsers.Average(u => u.TotalRequests):F2}");
            sb.AppendLine();

            sb.AppendLine($"--Распределение сообщений по времени--");
            int morningMessages = dbContext.MessageLog.Count(ml => ml.Date.TimeOfDay < TimeSpan.FromHours(12));
            int afternoonMessages = dbContext.MessageLog.Count(ml => ml.Date.TimeOfDay >= TimeSpan.FromHours(12) && ml.Date.TimeOfDay < TimeSpan.FromHours(18));
            int eveningMessages = dbContext.MessageLog.Count(ml => ml.Date.TimeOfDay >= TimeSpan.FromHours(18));
            sb.AppendLine($"Утро (00:00 - 12:00): {morningMessages}");
            sb.AppendLine($"День (12:00 - 18:00): {afternoonMessages}");
            sb.AppendLine($"Вечер (18:00 - 24:00): {eveningMessages}");
            var mostActiveHour = dbContext.MessageLog
                                        .GroupBy(ml => ml.Date.Hour)
                                        .OrderByDescending(g => g.Count())
                                        .Select(g => new { Hour = g.Key, Count = g.Count() })
                                        .FirstOrDefault();
            sb.AppendLine($"Самое активное время: {mostActiveHour?.Hour}:00 - {mostActiveHour?.Hour + 1}:00 с {mostActiveHour?.Count} сообщениями");

            sb.AppendLine();
            sb.AppendLine($"Общее количество обновляемых групп: {dbContext.ScheduleProfile.Where(i => !string.IsNullOrEmpty(i.Group) && (DateTime.Now - i.LastAppeal.ToLocalTime()).TotalDays <= config.DisciplineUpdateDays).Select(i => i.Group!).Distinct().Count()}");

            var groups = dbContext.GroupLastUpdate.OrderByDescending(i => i.Update).ToList();
            if(groups.Count > 1) {
                sb.AppendLine($"Время между последними обновлениями: {(groups[0].Update - groups[1].Update).ToString(@"hh\:mm\:ss")}");
            }

            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: sb.ToString(), replyMarkup: Statics.AdminPanelKeyboardMarkup, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);

            return Task.CompletedTask;
        }
    }
}