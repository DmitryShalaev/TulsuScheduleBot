using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;

namespace Core.Bot.Commands {
    public class Manager(Manager.GetCommand getMessageCommand, Manager.GetCommand getCallbackCommand) {
        public enum Check : byte {
            none,
            group,
            studentId,
            admin
        }

        public delegate Task MessageFunction(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args);
        public delegate Task CallbackFunction(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args);

        public delegate string GetCommand(string message, TelegramUser user, out string args);

        private readonly Dictionary<string, (Check, MessageFunction)> MessageCommands = [];
        private readonly Dictionary<string, (Check, CallbackFunction)> CallbackCommands = [];

        private readonly (Check, MessageFunction)[] DefaultMessageCommands = new (Check, MessageFunction)[Enum.GetValues(typeof(Mode)).Length];

        private readonly GetCommand getMessageCommand = getMessageCommand;
        private readonly GetCommand getCallbackCommand = getCallbackCommand;

        public void InitMessageCommands() {
            IEnumerable<Type> types = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(IMessageCommand).IsAssignableFrom(type))
                .Where(type => type.IsClass);

            foreach(Type? type in types) {
                if(Activator.CreateInstance(type) is IMessageCommand messageCommand) {

                    if(messageCommand.Commands is null) {
                        foreach(Mode mode in messageCommand.Modes)
                            DefaultMessageCommands[(byte)mode] = (messageCommand.Check, messageCommand.Execute);

                        continue;
                    }

                    foreach(string command in messageCommand.Commands!) {
                        foreach(Mode mode in messageCommand.Modes) {
                            MessageCommands.Add($"{command} {mode}".ToLower(), (messageCommand.Check, messageCommand.Execute));
                        }
                    }
                }
            }

            MessageCommands.TrimExcess();
        }

        public void InitCallbackCommands() {
            IEnumerable<Type> types = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(ICallbackCommand).IsAssignableFrom(type))
                .Where(type => type.IsClass);

            foreach(Type? type in types) {
                if(Activator.CreateInstance(type) is ICallbackCommand callbackCommand) {
                    if(callbackCommand.Mode == Mode.Default) {
                        foreach(Mode mode in Enum.GetValues(typeof(Mode)).Cast<Mode>().ToList()) {
                            CallbackCommands.Add($"{callbackCommand.Command} {mode}".ToLower(), (callbackCommand.Check, callbackCommand.Execute));
                        }
                    } else {
                        CallbackCommands.Add($"{callbackCommand.Command} {callbackCommand.Mode}".ToLower(), (callbackCommand.Check, callbackCommand.Execute));
                    }
                }
            }

            CallbackCommands.TrimExcess();
        }

        public void AddMessageCommand(string command, Mode mode, MessageFunction function, Check check = Check.none) => MessageCommands.Add($"{command} {mode}".ToLower(), (check, function));

        public async Task<bool> OnMessageAsync(ScheduleDbContext dbContext, ChatId chatId, int messageId, string message, TelegramUser user) {
            if(MessageCommands.TryGetValue(getMessageCommand(message.ToLower(), user, out string? args), out (Check, MessageFunction) func)) {
                if(await CheckAsync(dbContext, chatId, func.Item1, user)) {
                    await func.Item2(dbContext, chatId, messageId, user, args);
                    return true;

                } else {
                    return false;
                }
            }

            if(await CheckAsync(dbContext, chatId, DefaultMessageCommands[(byte)user.TelegramUserTmp.Mode].Item1, user)) {
                await DefaultMessageCommands[(byte)user.TelegramUserTmp.Mode].Item2(dbContext, chatId, messageId, user, message);
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

        private static async Task<bool> CheckAsync(ScheduleDbContext dbContext, ChatId chatId, Check check, TelegramUser user) {
            switch(check) {
                case Check.group:
                    if(string.IsNullOrWhiteSpace(user.ScheduleProfile.Group)) {
                        if(user.IsOwner())
                            await Statics.GroupErrorAdminAsync(dbContext, chatId, user);
                        else
                            Statics.GroupErrorUser(user);
                        return false;
                    }

                    break;

                case Check.studentId:
                    if(string.IsNullOrWhiteSpace(user.ScheduleProfile.StudentID)) {
                        if(user.IsOwner())
                            await Statics.StudentIdErrorAdminAsync(dbContext, chatId, user);
                        else
                            Statics.StudentIdErrorUser(user);
                        return false;
                    }

                    break;

                case Check.admin:
                    return user.IsAdmin;
            }

            return true;
        }
    }
}
