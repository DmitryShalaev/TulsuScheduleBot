using System.Text;

using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;

namespace Core.Bot.Commands.Admin.Statistics.Message {
    internal class Messages : IMessageCommand {

        public List<string> Commands => ["Сообщения"];

        public List<Mode> Modes => [Mode.Admin];

        public Manager.Check Check => Manager.Check.admin;

        private static readonly UserCommands.ConfigStruct config = UserCommands.Instance.Config;

        public Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            StringBuilder sb = new();

            DateTime today = DateTime.Now.Date;

            IQueryable<MessageLog> messageLogs = dbContext.MessageLog.AsQueryable();

            sb.AppendLine($"--Сообщения за сегодня--");
            var rareMessages = messageLogs
                .Where(ml => ml.Date.ToLocalTime() >= today)
                .GroupBy(ml => ml.Message)
                .OrderBy(g => g.Count())
                .ThenBy(g => g.Key)
                .Select(g => new { Message = g.Key, Count = g.Count() })
                .ToList();

            foreach(var msg in rareMessages) {
                string str = $"'{msg.Message}': {msg.Count}";
                if(sb.Length + str.Length > 4000) {
                    MessagesQueue.Message.SendTextMessage(chatId: chatId, text: sb.ToString(), replyMarkup: Statics.AdminPanelKeyboardMarkup, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                    sb.Clear();
                }

                sb.AppendLine(str);
            }

            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: sb.ToString(), replyMarkup: Statics.AdminPanelKeyboardMarkup, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);

            return Task.CompletedTask;
        }
    }
}