using Core.Bot.Interfaces;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
namespace Core.Bot.Commands.Student.Other.Profile.Settings.Notifications.Message {
    internal class Notifications : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => [UserCommands.Instance.Message["Notifications"]];

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) => await BotClient.SendTextMessageAsync(chatId: chatId, text: UserCommands.Instance.Message["NotificationSettings"], replyMarkup: DefaultCallback.GetNotificationsInlineKeyboardButton(user));
    }
}
