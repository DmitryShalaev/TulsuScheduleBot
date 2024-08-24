using Core.Bot.Commands.Interfaces;
using Core.DB;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Core.Bot.Commands.Admin.Message {
    public class Clear : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string> Commands => ["/clear"];

        public List<Mode> Modes => Enum.GetValues(typeof(Mode)).Cast<Mode>().ToList();

        public Manager.Check Check => Manager.Check.admin;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) => await ClearTemporary.ClearAsync();
    }
}
