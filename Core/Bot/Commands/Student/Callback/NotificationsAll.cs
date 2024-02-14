using Core.Bot.Interfaces;

using ScheduleBot;
using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace Core.Bot.Commands.Student.Callback {

    public class NotificationsAll : ICallbackCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public string Command => "NotificationsAll";

        public Mode Mode => Mode.Default;

        public Manager.Check Check => Manager.Check.group;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args) {
            if(DateOnly.TryParse(args, out DateOnly date))
                await BotClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: Scheduler.GetScheduleByDateNotification(dbContext, date, user), parseMode: ParseMode.Html);
        }
    }
}
