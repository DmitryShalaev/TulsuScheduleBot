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

            DateTime startOfWeek = today.AddDays(-7);

            DateTime startOfMonth = today.AddMonths(-1);

            IQueryable<TelegramUser> telegramUsers = dbContext.TelegramUsers.AsQueryable();
            IQueryable<MessageLog> messageLogs = dbContext.MessageLog.AsQueryable();

            sb.AppendLine($"Всего пользователей: {telegramUsers.Count()} ({telegramUsers.Where(i => !i.IsDeactivated).Count()})");
            sb.AppendLine($"Администраторы: {telegramUsers.Count(u => u.IsAdmin)}");
            sb.AppendLine();

            AppendNewUsersStats(sb, telegramUsers, today, startOfWeek, startOfMonth);
            AppendActiveUsersStats(sb, telegramUsers, today, startOfWeek, startOfMonth);
            AppendTopUsers(sb, telegramUsers);
            AppendMessageStats(sb, messageLogs, today, startOfWeek, startOfMonth);
            AppendAverageMessagesPerHourStats(sb, telegramUsers, messageLogs, today, startOfWeek, startOfMonth);
            AppendScheduleProfileStats(sb, dbContext);

            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: sb.ToString(), replyMarkup: Statics.AdminPanelKeyboardMarkup, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);

            return Task.CompletedTask;
        }

        private static void AppendNewUsersStats(StringBuilder sb, IQueryable<TelegramUser> users, DateTime today, DateTime startOfWeek, DateTime startOfMonth) {
            sb.AppendLine($"--Новых пользователей--");
            sb.AppendLine($"За сегодня: {users.Count(u => u.DateOfRegistration.HasValue && u.DateOfRegistration.Value.ToLocalTime() >= today)}");
            sb.AppendLine($"За неделю: {users.Count(u => u.DateOfRegistration.HasValue && u.DateOfRegistration.Value.ToLocalTime() >= startOfWeek)}");
            sb.AppendLine($"За месяц: {users.Count(u => u.DateOfRegistration.HasValue && u.DateOfRegistration.Value.ToLocalTime() >= startOfMonth)}");
            sb.AppendLine();
        }

        private static void AppendActiveUsersStats(StringBuilder sb, IQueryable<TelegramUser> users, DateTime today, DateTime startOfWeek, DateTime startOfMonth) {
            sb.AppendLine($"--Активных пользователей--");
            sb.AppendLine($"За сегодня: {users.Count(u => u.LastAppeal.ToLocalTime() >= today)}");
            sb.AppendLine($"За неделю: {users.Count(u => u.LastAppeal.ToLocalTime() >= startOfWeek)}");
            sb.AppendLine($"За месяц: {users.Count(u => u.LastAppeal.ToLocalTime() >= startOfMonth)}");
            sb.AppendLine();
        }

        private static void AppendTopUsers(StringBuilder sb, IQueryable<TelegramUser> users) {
            sb.AppendLine($"--Топ пользователей по активности--");
            var topUsers = users.OrderByDescending(u => u.TotalRequests).Take(5).ToList();

            foreach(TelegramUser? topUser in topUsers) {
                string userName = Statics.EscapeSpecialCharacters($"{topUser.FirstName} {topUser.LastName}");
                sb.AppendLine(!string.IsNullOrWhiteSpace(topUser.Username)
                    ? $"[{userName}](https://t.me/{topUser.Username}) ({topUser.TotalRequests})"
                    : $"{userName} ({topUser.TotalRequests})");
            }

            sb.AppendLine();
        }

        private static void AppendMessageStats(StringBuilder sb, IQueryable<MessageLog> messages, DateTime today, DateTime startOfWeek, DateTime startOfMonth) {
            sb.AppendLine($"--Получено сообщений--");
            sb.AppendLine($"Всего: {messages.Count()}");
            sb.AppendLine($"За сегодня: {messages.Count(ml => ml.Date.ToLocalTime() >= today)}");
            sb.AppendLine($"За неделю: {messages.Count(ml => ml.Date.ToLocalTime() >= startOfWeek)}");
            sb.AppendLine($"За месяц: {messages.Count(ml => ml.Date.ToLocalTime() >= startOfMonth)}");
            sb.AppendLine();
        }

        private static void AppendScheduleProfileStats(StringBuilder sb, ScheduleDbContext dbContext) {
            sb.AppendLine($"Общее количество обновляемых групп: {dbContext.ScheduleProfile.Where(i => !string.IsNullOrEmpty(i.Group) && (DateTime.Now - i.LastAppeal.ToLocalTime()).TotalDays <= config.DisciplineUpdateDays).Select(i => i.Group!).Distinct().Count()}");

            var groups = dbContext.GroupLastUpdate.OrderByDescending(i => i.Update.ToLocalTime()).ToList();
            if(groups.Count > 1) {
                sb.AppendLine($"Время между последними обновлениями: {(groups[0].Update.ToLocalTime() - groups[1].Update.ToLocalTime()).ToString(@"hh\:mm\:ss")}");
            }
        }

        private static void AppendAverageMessagesPerHourStats(StringBuilder sb, IQueryable<TelegramUser> users, IQueryable<MessageLog> messages, DateTime today, DateTime startOfWeek, DateTime startOfMonth) {
            sb.AppendLine($"--Среднее число сообщений в час--");

            int hoursToday = (int)(DateTime.Now - today).TotalHours;
            int hoursWeek = (int)(DateTime.Now - startOfWeek).TotalHours;
            int hoursMonth = (int)(DateTime.Now - startOfMonth).TotalHours;

            sb.AppendLine($"За сегодня: {messages.Count(ml => ml.Date.ToLocalTime() >= today) / (double)hoursToday:F2}");
            sb.AppendLine($"За неделю: {messages.Count(ml => ml.Date.ToLocalTime() >= startOfWeek) / (double)hoursWeek:F2}");
            sb.AppendLine($"За месяц: {messages.Count(ml => ml.Date.ToLocalTime() >= startOfMonth) / (double)hoursMonth:F2}");

            sb.AppendLine();
            sb.AppendLine($"Среднее количество запросов на пользователя: {users.Average(u => u.TotalRequests):F2}");
            sb.AppendLine();

            var spikes = messages
                .Where(ml => ml.Date.ToLocalTime() >= today)
                .GroupBy(ml => ml.Date.ToLocalTime().Hour)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new { Hour = g.Key, Count = g.Count() })
                .ToList();

            sb.AppendLine("Всплески активности по часам:");
            foreach(var spike in spikes) {
                sb.AppendLine($"{spike.Hour}:00 - {spike.Hour + 1}:00 с {spike.Count} сообщениями");
            }

            sb.AppendLine();
        }
    }
}