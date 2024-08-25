using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;
namespace Core.Bot.Commands.Teachers.EnterTeacherName.Message {
    public class TeachersWorkSchedule : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["TeachersWorkSchedule"]];

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            user.TelegramUserTmp.Mode = Mode.TeachersWorkSchedule;
            await dbContext.SaveChangesAsync();

            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["EnterTeacherName"], replyMarkup: Statics.WorkScheduleBackKeyboardMarkup);
        }
    }
}
