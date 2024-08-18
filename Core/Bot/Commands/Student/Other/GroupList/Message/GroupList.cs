using System.Text;

using Core.Bot.Interfaces;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace Core.Bot.Commands.Student.Other.GroupList.Message {
    internal class GroupList : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => [UserCommands.Instance.Message["GroupList"]];

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.group;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            var sb = new StringBuilder();

            var group = user.ScheduleProfile.Group;

            var users = dbContext.TelegramUsers.Where(u => u.Settings.DisplayingGroupList && u.ScheduleProfile.Group == group).ToList();

            sb.AppendLine($"{UserCommands.Instance.Message["GroupList"]}: {group}\n");

            if(users.Count == 0) {
                sb.AppendLine("Здесь никого нет 😢😢😢");
            }

            foreach(var u in users) {
                if(!string.IsNullOrWhiteSpace(u.Username)) {
                    sb.AppendLine($"[{EscapeSpecialCharacters($"{u.FirstName} {u.LastName}")}](https://t.me/{u.Username})");
                } else {
                    sb.AppendLine(EscapeSpecialCharacters($"{u.FirstName} {u.LastName}"));
                }
            }

            await BotClient.SendTextMessageAsync(chatId: chatId, text: sb.ToString(), replyMarkup: Statics.OtherKeyboardMarkup, parseMode: ParseMode.Markdown, disableWebPagePreview: true);
        }

        public static string EscapeSpecialCharacters(string input) {
            // Перечень символов, которые нужно экранировать
            char[] specialChars = { '_', '*', '[', ']', '(', ')', '~', '`', '>', '#', '+', '-', '=', '|', '{', '}', '.', '!' };

            StringBuilder escapedString = new StringBuilder();

            foreach(char c in input) {
                // Если символ является специальным, добавляем перед ним обратный слэш
                if(Array.Exists(specialChars, element => element == c)) {
                    escapedString.Append('\\');
                }
                escapedString.Append(c);
            }

            return escapedString.ToString();
        }
    }
}
