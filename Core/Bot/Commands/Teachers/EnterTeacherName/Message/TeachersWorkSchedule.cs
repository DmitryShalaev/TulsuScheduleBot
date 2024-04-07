using Core.Bot.Interfaces;

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

            await BotClient.SendTextMessageAsync(chatId: chatId, text: UserCommands.Instance.Message["EnterTeacherName"], replyMarkup: Statics.TeachersWorkScheduleBackKeyboardMarkup);
        }
    }
}
