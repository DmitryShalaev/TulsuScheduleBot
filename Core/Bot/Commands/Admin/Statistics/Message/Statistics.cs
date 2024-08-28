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
            DateTime startOfMonth = new DateTime(today.Year, today.Month, 1).ToUniversalTime(); ;
            DateTime endOfWeek = startOfWeek.AddDays(7).AddTicks(-1).ToUniversalTime(); ;
            DateTime endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1).ToUniversalTime();

            sb.AppendLine($"Всего пользователей: {dbContext.TelegramUsers.Count()}");
            sb.AppendLine();

            sb.AppendLine($"--Новых пользователей--");
            sb.AppendLine($"За сегодня: {dbContext.TelegramUsers.Where(user => user.DateOfRegistration.HasValue && user.DateOfRegistration.Value.Date == today).Count()}");

            sb.AppendLine($"За неделю: {dbContext.TelegramUsers.Where(user => user.DateOfRegistration.HasValue &&
                                                                             user.DateOfRegistration.Value >= startOfWeek &&
                                                                             user.DateOfRegistration.Value <= endOfWeek).Count()}");

            sb.AppendLine($"За месяц: {dbContext.TelegramUsers.Where(user => user.DateOfRegistration.HasValue &&
                                                                            user.DateOfRegistration.Value >= startOfMonth &&
                                                                            user.DateOfRegistration.Value <= endOfMonth).Count()}");

            sb.AppendLine();
            sb.AppendLine($"--Получено сообщений--");
            sb.AppendLine($"За сегодня: {dbContext.MessageLog.Where(user => user.Date.Date == today).Count()}");
            sb.AppendLine($"За неделю: {dbContext.MessageLog.Where(user => user.Date >= startOfWeek && user.Date <= endOfWeek).Count()}");
            sb.AppendLine($"За месяц: {dbContext.MessageLog.Where(user => user.Date >= startOfMonth && user.Date <= endOfMonth).Count()}");

            sb.AppendLine();
            sb.AppendLine($"Общее количество обновляемых групп: {dbContext.ScheduleProfile.Where(i => !string.IsNullOrEmpty(i.Group) && (DateTime.Now - i.LastAppeal.ToLocalTime()).TotalDays <= config.DisciplineUpdateDays).Select(i => i.Group!).Distinct().Count()}");

            var groups = dbContext.GroupLastUpdate.OrderByDescending(i => i.Update).ToList();
            sb.AppendLine($"Время между обновлениями: {(groups[0].Update - groups[1].Update).ToString(@"hh\:mm\:ss")}");

            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: sb.ToString(), replyMarkup: Statics.AdminPanelKeyboardMarkup);

            return Task.CompletedTask;
        }
    }
}