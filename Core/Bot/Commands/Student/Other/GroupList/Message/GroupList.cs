using System.Text;

using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace Core.Bot.Commands.Student.Other.GroupList.Message {
    internal class GroupList : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["GroupList"]];

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.group;

        public Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            var sb = new StringBuilder();

            string? group = user.ScheduleProfile.Group;

            var users = dbContext.TelegramUsers.Where(u => u.Settings.DisplayingGroupList && u.ScheduleProfile.Group == group).ToList();

            sb.AppendLine($"{UserCommands.Instance.Message["GroupList"]}: {group}\n");

            if(users.Count == 0) sb.AppendLine("Здесь никого нет 😢😢😢");

            foreach(TelegramUser? u in users) {
                if(!string.IsNullOrWhiteSpace(u.Username)) {
                    sb.AppendLine($"[{Statics.EscapeSpecialCharacters($"{u.FirstName} {u.LastName}")}](https://t.me/{u.Username})");
                } else {
                    sb.AppendLine(Statics.EscapeSpecialCharacters($"{u.FirstName} {u.LastName}"));
                }
            }

            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: sb.ToString(), replyMarkup: Statics.OtherKeyboardMarkup, parseMode: ParseMode.Markdown);
            return Task.CompletedTask;
        }
    }
}
