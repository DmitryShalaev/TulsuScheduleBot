using Core.Bot.Commands.Interfaces;
using Core.Bot.MessagesQueue;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;
namespace Core.Bot.Commands.Student.Other.Profile.Message {
    internal class ResetProfileLinkDefault : IMessageCommand {

        public List<string>? Commands => null;

        public List<Mode> Modes => [Mode.ResetProfileLink];

        public Manager.Check Check => Manager.Check.none;

        public Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: "Выберите один из представленных вариантов!", replyMarkup: Statics.ResetProfileLinkKeyboardMarkup);
            return Task.CompletedTask;
        }
    }
}
