using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;
namespace Core.Bot.Commands.Student.Days.ForAWeek.Message {
    internal class ForAWeek : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["ForAWeek"]];

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.group;

        public Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["ForAWeek"], replyMarkup: Statics.WeekKeyboardMarkup);
            return Task.CompletedTask;
        }
    }
}
