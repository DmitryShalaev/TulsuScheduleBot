using Core.Bot.Interfaces;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Core.Bot.Commands.Teachers.Days.ByDays.Message {
    internal class TeachersByDays : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => [UserCommands.Instance.Message["ByDays"]];

        public List<Mode> Modes => [Mode.TeacherSelected];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) => await BotClient.SendTextMessageAsync(chatId: chatId, text: UserCommands.Instance.Message["ByDays"], replyMarkup: Statics.DaysKeyboardMarkup);
    }
}
