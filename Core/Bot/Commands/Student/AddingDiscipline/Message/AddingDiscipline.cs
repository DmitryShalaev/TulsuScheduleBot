using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;
namespace Core.Bot.Commands.AddingDiscipline.Message {
    internal class AddingDiscipline : IMessageCommand {

        public List<string>? Commands => null;

        public List<Mode> Modes => [Mode.AddingDiscipline];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) => await AddingDisciplineMode.SetStagesAddingDisciplineAsync(dbContext, chatId, args, user);
    }
}
