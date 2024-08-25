using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;
namespace Core.Bot.Commands.Classrooms.Back.Message {
    public class ClassroomsBack : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["Back"]];

        public List<Mode> Modes => [Mode.ClassroomSelected];

        public Manager.Check Check => Manager.Check.none;

        public Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: "Основное меню", replyMarkup: DefaultMessage.GetClassroomWorkScheduleSelectedKeyboardMarkup(user.TelegramUserTmp.TmpData!));
            return Task.CompletedTask;
        }
    }
}
