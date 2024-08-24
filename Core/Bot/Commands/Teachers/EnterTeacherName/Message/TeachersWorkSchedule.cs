using Core.Bot.Commands.Interfaces;
using Core.Bot.Messages;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
namespace Core.Bot.Commands.Teachers.EnterTeacherName.Message {
    public class TeachersWorkSchedule : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => [UserCommands.Instance.Message["TeachersWorkSchedule"]];

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            user.TelegramUserTmp.Mode = Mode.TeachersWorkSchedule;
            await dbContext.SaveChangesAsync();

            MessageQueue.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["EnterTeacherName"], replyMarkup: Statics.WorkScheduleBackKeyboardMarkup);
        }
    }
}
