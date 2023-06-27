using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot.Types;

namespace ScheduleBot.Bot {
    public class CommandManager {
        public enum Check : byte {
            none,
            group,
            studentId
        }

        public delegate Task MessageFunction(ScheduleDbContext dbContext, ChatId chatId, TelegramUser user, string args);
        public delegate Task CallbackFunction(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args);

        public delegate Task<bool> TryFunction(ScheduleDbContext dbContext, ChatId chatId, TelegramUser user, string args);

        public delegate string GetCommand(string message, TelegramUser user, out string args);

        private readonly Dictionary<string, (Check, MessageFunction)> MessageCommands;
        private readonly Dictionary<string, (Check, CallbackFunction)> CallbackCommands;

        private readonly List<(Check, TryFunction)>[] DefaultMessageCommands;

        private readonly TelegramBot telegramBot;
        private readonly GetCommand getMessageCommand;
        private readonly GetCommand getCallbackCommand;

        public CommandManager(TelegramBot telegramBot, GetCommand getCommand, GetCommand getCallbackCommand) {
            this.telegramBot = telegramBot;
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
            MessageCommands.Add($"{command} {mode}".ToLower(), (check, function));
        }

        public void AddMessageCommand(string[] commands, Mode mode, MessageFunction function, Check check = Check.none) {
            foreach(var command in commands)
                AddMessageCommand(command, mode, function, check);
        }

        public void AddMessageCommand(string[] commands, Mode[] modes, MessageFunction function, Check check = Check.none) {
            foreach(var command in commands)
                foreach(var mode in modes)
                    AddMessageCommand(command, mode, function, check);
        }

        public void AddMessageCommand(string command, Mode[] modes, MessageFunction function, Check check = Check.none) {
            foreach(var mode in modes)
                AddMessageCommand(command, mode, function, check);
        }

        public void AddCallbackCommand(string command, Mode mode, CallbackFunction function, Check check = Check.none) {
            CallbackCommands.Add($"{command} {mode}".ToLower(), (check, function)); ;
        }

        public async Task<bool> OnMessageAsync(ScheduleDbContext dbContext, ChatId chatId, string message, TelegramUser user) {
            if(MessageCommands.TryGetValue(getMessageCommand(message.ToLower(), user, out var args), out var func)) {
                if(await CheckAsync(dbContext, chatId, func.Item1, user)) {
                    await func.Item2(dbContext, chatId, user, args);
                    return true;

                } else {
                    return false;
                }
            }

            foreach(var item in DefaultMessageCommands[(byte)user.Mode]) {
                if(await CheckAsync(dbContext, chatId, item.Item1, user)) {
                    if(await item.Item2(dbContext, chatId, user, message))
                        return true;
                }
            }
            return false;
        }

        public async Task<bool> OnCallbackAsync(ScheduleDbContext dbContext, ChatId chatId, int messageId, string command, string message, TelegramUser user) {
            if(CallbackCommands.TryGetValue(getCallbackCommand(command.ToLower(), user, out var args), out var func)) {
                if(await CheckAsync(dbContext, chatId, func.Item1, user)) {
                    await func.Item2(dbContext, chatId, messageId, user, message, args);
                    return true;
                }
            }
            return false;
        }

        private async Task<bool> CheckAsync(ScheduleDbContext dbContext, ChatId chatId, Check check, TelegramUser user) {
            switch(check) {
                case Check.group:
                    if(string.IsNullOrWhiteSpace(user.ScheduleProfile.Group)) {
                        if(user.IsAdmin())
                            await telegramBot.GroupErrorAdmin(dbContext, chatId, user);
                        else
                            await telegramBot.GroupErrorUser(chatId);
                        return false;
                    }
                    break;

                case Check.studentId:
                    if(string.IsNullOrWhiteSpace(user.ScheduleProfile.StudentID)) {
                        if(user.IsAdmin())
                            await telegramBot.StudentIdErrorAdmin(dbContext, chatId, user);
                        else
                            await telegramBot.StudentIdErrorUser(chatId);
                        return false;
                    }
                    break;
            }
            return true;
        }

        public void TrimExcess() {
            MessageCommands.TrimExcess();
            CallbackCommands.TrimExcess();

            foreach(var item in DefaultMessageCommands) {
                if(item is not null)
                    item.TrimExcess();
            }
        }
    }
}
