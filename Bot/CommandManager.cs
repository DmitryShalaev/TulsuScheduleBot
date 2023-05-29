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

        public delegate Task Function(ITelegramBotClient botClient, ChatId chatId, TelegramUser user);

        private readonly Dictionary<string, (Mode?, Check, Function)> MessageCommands;

        private readonly ITelegramBotClient botClient;

        public CommandManager(ITelegramBotClient botClient) {
            this.botClient = botClient;
            MessageCommands = new();
        }

        public void AddMessageCommand(string command, Mode? mode, Function function, Check check = Check.none) {
            MessageCommands.Add(command, (mode, check, function));
        }

        public void AddMessageCommand(string[] commands, Mode? mode, Function function, Check check = Check.none) {
            foreach(var command in commands)
                AddMessageCommand(command, mode, function, check);
        }

        public async Task<bool> OnMessageAsync(ChatId chatId, string? message, TelegramUser user) {
            if(message is null) return false;

            if(MessageCommands.TryGetValue(message, out var command)) {
                if(command.Item1 is null || command.Item1 == user.Mode) {
                    switch(command.Item2) {
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

                    await command.Item3.Invoke(botClient, chatId, user);

                    return true;
                }
            }
            return false;
        }
    }
}
