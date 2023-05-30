using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace ScheduleBot.Bot {
    public partial class CommandManager {
        public enum Check : byte {
            none,
            group,
            studentId
        }

        public delegate Task Function(ITelegramBotClient botClient, ChatId chatId, TelegramUser user, string args);
        public delegate string GetCommand(string message, TelegramUser user, out string args);

        private readonly Dictionary<string, (Check, Function)> MessageCommands;

        private readonly ITelegramBotClient botClient;
        private readonly GetCommand getCommand;

        public CommandManager(ITelegramBotClient botClient, GetCommand getCommand) {
            this.botClient = botClient;
            this.getCommand = getCommand;
            MessageCommands = new();
        }

        public void AddMessageCommand(string command, Mode mode, Function function, Check check = Check.none) {
            MessageCommands.Add($"{command} {mode}", (check, function));
        }

        public void AddMessageCommand(string[] commands, Mode mode, Function function, Check check = Check.none) {
            foreach(var command in commands)
                AddMessageCommand(command, mode, function, check);
        }

        public async Task<bool> OnMessageAsync(ChatId chatId, string? message, TelegramUser user) {
            if(message is null) return false;

            if(MessageCommands.TryGetValue(getCommand(message, user, out var args), out var func)) {
                switch(func.Item1) {
                    case Check.group:
                        if(string.IsNullOrWhiteSpace(user.ScheduleProfile.Group)) {
                            if(user.IsAdmin())
                                await TelegramBot.GroupErrorAdmin(botClient, chatId);
                            else
                                await TelegramBot.GroupErrorUser(botClient, chatId);
                            return false;
                        }
                        break;

                    case Check.studentId:
                        if(string.IsNullOrWhiteSpace(user.ScheduleProfile.StudentID)) {
                            if(user.IsAdmin())
                                await TelegramBot.StudentIdErrorAdmin(botClient, chatId);
                            else
                                await TelegramBot.StudentIdErrorUser(botClient, chatId);
                            return false;
                        }
                        break;
                }

                await func.Item2.Invoke(botClient, chatId, user, args);

                return true;
            }

            return false;
        }
    }
}
