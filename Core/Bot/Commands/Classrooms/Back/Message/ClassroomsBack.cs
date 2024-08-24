using Core.Bot.Commands.Interfaces;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
namespace Core.Bot.Commands.Classrooms.Back.Message {
    public class ClassroomsBack : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => [UserCommands.Instance.Message["Back"]];

        public List<Mode> Modes => [Mode.ClassroomSelected];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) => await BotClient.SendTextMessageAsync(chatId: chatId, text: "Основное меню", replyMarkup: DefaultMessage.GetClassroomWorkScheduleSelectedKeyboardMarkup(user.TelegramUserTmp.TmpData!));
    }
}
