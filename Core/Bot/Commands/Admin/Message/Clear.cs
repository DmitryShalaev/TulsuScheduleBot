using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;

namespace Core.Bot.Commands.Admin.Message {
    public class Clear : IMessageCommand {

        public List<string> Commands => ["/clear"];

        public List<Mode> Modes => Enum.GetValues(typeof(Mode)).Cast<Mode>().ToList();

        public Manager.Check Check => Manager.Check.admin;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) => await ClearTemporary.ClearAsync();
    }
}
