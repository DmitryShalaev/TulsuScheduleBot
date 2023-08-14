using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot.Types;

namespace ScheduleBot.Bot {
    public class CommandManager {
        public enum Check : byte {
            none,
            group,
            studentId,
            admin
        }

        public delegate Task MessageFunction(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args);
        public delegate Task CallbackFunction(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args);

        public delegate Task<bool> TryFunction(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args);

        public delegate string GetCommand(string message, TelegramUser user, out string args);

        private readonly Dictionary<string, (Check, MessageFunction)> MessageCommands;
        private readonly Dictionary<string, (Check, CallbackFunction)> CallbackCommands;

        private readonly List<(Check, TryFunction)>[] DefaultMessageCommands;

        private readonly TelegramBot telegramBot;
        private readonly GetCommand getMessageCommand;
        private readonly GetCommand getCallbackCommand;

        public CommandManager(TelegramBot telegramBot, GetCommand getMessageCommand, GetCommand getCallbackCommand) {
            this.telegramBot = telegramBot;
            this.getMessageCommand = getMessageCommand;
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

        public void AddMessageCommand(string command, Mode mode, MessageFunction function, Check check = Check.none) => MessageCommands.Add($"{command} {mode}".ToLower(), (check, function));

        public void AddMessageCommand(string[] commands, Mode mode, MessageFunction function, Check check = Check.none) {
            foreach(string command in commands)
                AddMessageCommand(command, mode, function, check);
        }

        public void AddMessageCommand(string[] commands, Mode[] modes, MessageFunction function, Check check = Check.none) {
            foreach(string command in commands)
                foreach(Mode mode in modes)
                    AddMessageCommand(command, mode, function, check);
        }

        public void AddMessageCommand(string command, Mode[] modes, MessageFunction function, Check check = Check.none) {
            foreach(Mode mode in modes)
                AddMessageCommand(command, mode, function, check);
        }

        public void AddCallbackCommand(string command, Mode mode, CallbackFunction function, Check check = Check.none) {
            CallbackCommands.Add($"{command} {mode}".ToLower(), (check, function)); ;
        }

        public async Task<bool> OnMessageAsync(ScheduleDbContext dbContext, ChatId chatId, int messageId, string message, TelegramUser user) {
            if(MessageCommands.TryGetValue(getMessageCommand(message.ToLower(), user, out string? args), out (Check, MessageFunction) func)) {
                if(await CheckAsync(dbContext, chatId, func.Item1, user)) {
                    await func.Item2(dbContext, chatId, messageId, user, args);
                    return true;

                } else {
                    return false;
                }
            }

            foreach((Check, TryFunction) item in DefaultMessageCommands[(byte)user.Mode] ?? new()) {
                if(await CheckAsync(dbContext, chatId, item.Item1, user)) {
                    if(await item.Item2(dbContext, chatId, messageId, user, message))
                        return true;
                }
            }

            return false;
        }

        public async Task<bool> OnCallbackAsync(ScheduleDbContext dbContext, ChatId chatId, int messageId, string command, string message, TelegramUser user) {
            if(CallbackCommands.TryGetValue(getCallbackCommand(command.ToLower(), user, out string? args), out (Check, CallbackFunction) func)) {
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
                        if(user.IsOwner())
                            await telegramBot.GroupErrorAdmin(dbContext, chatId, user);
                        else
                            await telegramBot.GroupErrorUser(chatId);
                        return false;
                    }

                    break;

                case Check.studentId:
                    if(string.IsNullOrWhiteSpace(user.ScheduleProfile.StudentID)) {
                        if(user.IsOwner())
                            await telegramBot.StudentIdErrorAdmin(dbContext, chatId, user);
                        else
                            await telegramBot.StudentIdErrorUser(chatId);
                        return false;
                    }

                    break;

                case Check.admin:
                    return user.IsAdmin;
            }

            return true;
        }

        public void TrimExcess() {
            MessageCommands.TrimExcess();
            CallbackCommands.TrimExcess();

            foreach(List<(Check, TryFunction)> item in DefaultMessageCommands)
                item?.TrimExcess();
        }
    }
}
