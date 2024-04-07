using Core.Bot.Interfaces;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
namespace Core.Bot.Commands.AddingDiscipline.Message {
    internal class AddingDiscipline : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => null;

        public List<Mode> Modes => [Mode.AddingDiscipline];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) => await AddingDisciplineMode.SetStagesAddingDisciplineAsync(dbContext, BotClient, chatId, messageId, args, user);
    }
}
