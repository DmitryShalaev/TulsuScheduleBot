using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace ScheduleBot.Bot {
    public class CommandManager {
        public enum Check : byte {
            none,
            group,
            studentId
        }

        public delegate Task MessageFunction(ITelegramBotClient botClient, ChatId chatId, TelegramUser user, string args);
        public delegate Task CallbackFunction(ITelegramBotClient botClient, ChatId chatId, int messageId, TelegramUser user, string message, string args);

        public delegate Task<bool> TryFunction(ITelegramBotClient botClient, ChatId chatId, TelegramUser user, string args);

        public delegate string GetMessageCommand(string message, TelegramUser user, out string args);
        public delegate string GetCallbackCommand(string message, TelegramUser user, out string args);

        private readonly Dictionary<string, (Check, MessageFunction)> MessageCommands;
        private readonly Dictionary<string, (Check, CallbackFunction)> CallbackCommands;

        private readonly List<(Check, TryFunction)>[] DefaultMessageCommands;

        private readonly ITelegramBotClient botClient;
        private readonly GetMessageCommand getMessageCommand;
        private readonly GetCallbackCommand getCallbackCommand;

        public CommandManager(ITelegramBotClient botClient, GetMessageCommand getCommand, GetCallbackCommand getCallbackCommand) {
            this.botClient = botClient;
            this.getMessageCommand = getCommand;
            this.getCallbackCommand = getCallbackCommand;

            MessageCommands = new();
            CallbackCommands = new();

            DefaultMessageCommands = new List<(Check, TryFunction)>[Enum.GetValues(typeof(Mode)).Length];
        }

        public void AddMessageCommand(Mode mode, TryFunction function, Check check = Check.none) {
            if(DefaultMessageCommands[(byte)mode] is null)
                DefaultMessageCommands[(byte)mode] = new() { (check, function) };
            else
                DefaultMessageCommands[(byte)mode].Add((check, function));
        }

        public void AddMessageCommand(string command, Mode mode, MessageFunction function, Check check = Check.none) {
            MessageCommands.Add($"{command} {mode}", (check, function));
        }

        public void AddMessageCommand(string[] commands, Mode mode, MessageFunction function, Check check = Check.none) {
            foreach(var command in commands)
                AddMessageCommand(command, mode, function, check);
        }

        public void AddCallbackCommand(string command, Mode mode, CallbackFunction function, Check check = Check.none) {
            CallbackCommands.Add($"{command} {mode}", (check, function)); ;
        }

        public async Task<bool> OnMessageAsync(ChatId chatId, string message, TelegramUser user) {
            if(MessageCommands.TryGetValue(getMessageCommand(message, user, out var args), out var func)) {
                if(await CheckAsync(botClient, chatId, func.Item1, user)) {
                    await func.Item2(botClient, chatId, user, args);
                    return true;

                } else {
                    return false;
                }
            }

            foreach(var item in DefaultMessageCommands[(byte)user.Mode]) {
                if(await CheckAsync(botClient, chatId, item.Item1, user)) {
                    if(await item.Item2(botClient, chatId, user, message))
                        return true;
                }
            }

            return false;
        }

        public async Task<bool> OnCallbackAsync(ChatId chatId, int messageId, string command, string message, TelegramUser user) {
            if(CallbackCommands.TryGetValue(getCallbackCommand(command, user, out var args), out var func)) {
                if(await CheckAsync(botClient, chatId, func.Item1, user)) {
                    await func.Item2(botClient, chatId, messageId, user, message, args);
                    return true;

                }
            }

            return false;
        }

        private async Task<bool> CheckAsync(ITelegramBotClient botClient, ChatId chatId, Check check, TelegramUser user) {
            switch(check) {
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
            return true;
        }
    }
}
