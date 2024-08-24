using Core.Bot.Commands.Interfaces;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Core.Bot.Commands.AddingDiscipline.Callback {
    public class SetEndTime : ICallbackCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public string Command => UserCommands.Instance.Callback["SetEndTime"].callback;

        public Mode Mode => Mode.AddingDiscipline;

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args) => await AddingDisciplineMode.SetStagesAddingDisciplineAsync(dbContext, BotClient, chatId, messageId, args, user);
    }
}
