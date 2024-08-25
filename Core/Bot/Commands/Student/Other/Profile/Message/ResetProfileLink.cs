using Core.Bot.Commands.Interfaces;
using Core.Bot.MessagesQueue;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;
namespace Core.Bot.Commands.Student.Other.Profile.Message {
    internal class ResetProfileLink : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["ResetProfileLink"]];

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            if(!user.IsOwner()) {
                user.TelegramUserTmp.Mode = Mode.ResetProfileLink;
                await dbContext.SaveChangesAsync();

                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: "Вы точно уверены что хотите восстановить свой профиль?", replyMarkup: Statics.ResetProfileLinkKeyboardMarkup);
            } else {
                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: "Владельцу профиля нет смысла его восстанавливать!", replyMarkup: Statics.MainKeyboardMarkup);
            }
        }
    }
}
