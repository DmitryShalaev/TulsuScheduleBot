using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;

namespace Core.Bot.Commands.Admin.Dispatch.Message {
    public class DispatchMessage : IMessageCommand {

        public List<string>? Commands => null;

        public List<Mode> Modes => [Mode.Dispatch];

        public Manager.Check Check => Manager.Check.admin;

        private static string? password;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            user.TelegramUserTmp.Mode = Mode.Dispatch;

            if(user.TelegramUserTmp.TmpData is null) {
                user.TelegramUserTmp.TmpData = args;

                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: $"Введите пароль: ||{password = GeneratePassword(5)}||", replyMarkup: Statics.CancelKeyboardMarkup, parseMode: Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
            } else if(args == password) {

                foreach(TelegramUser? item in dbContext.TelegramUsers.Where(i => !i.IsDeactivated)) {
                    MessagesQueue.Message.SendTextMessage(chatId: item.ChatID, text: user.TelegramUserTmp.TmpData, disableNotification: true);
                }

                user.TelegramUserTmp.Mode = Mode.Admin;
                user.TelegramUserTmp.TmpData = null;

                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["AdminPanel"], replyMarkup: Statics.AdminPanelKeyboardMarkup);
            }

            await dbContext.SaveChangesAsync();
        }

        static string GeneratePassword(int length) {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            char[] password = new char[length];

            for(int i = 0; i < length; i++) {
                password[i] = chars[random.Next(chars.Length)];
            }

            return new string(password);
        }
    }
}
