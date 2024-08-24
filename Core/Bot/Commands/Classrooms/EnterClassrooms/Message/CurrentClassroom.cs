using Core.Bot.Commands.Interfaces;
using Core.Bot.Messages;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
namespace Core.Bot.Commands.Classrooms.EnterClassrooms.Message {
    internal class CurrentClassroom : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => [UserCommands.Instance.Message["CurrentClassroom"]];

        public List<Mode> Modes => [Mode.ClassroomSelected];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            user.TelegramUserTmp.Mode = Mode.ClassroomSchedule;
            await dbContext.SaveChangesAsync();

            MessageQueue.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["EnterClassroom"], replyMarkup: Statics.WorkScheduleBackKeyboardMarkup);
        }
    }
}
