using Core.Bot.Commands.Interfaces;

using ScheduleBot;
using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace Core.Bot.Commands.Student.Days.Message {
    internal class Tomorrow : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => [UserCommands.Instance.Message["Tomorrow"]];

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.group;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            await Statics.ScheduleRelevance(dbContext, BotClient, chatId, user.ScheduleProfile.Group!, Statics.MainKeyboardMarkup);
            var date = DateOnly.FromDateTime(DateTime.Now.AddDays(1));

            (string, bool) schedule = Scheduler.GetScheduleByDate(dbContext, date, user);
            await BotClient.SendTextMessageAsync(chatId: chatId, text: schedule.Item1, replyMarkup: DefaultCallback.GetInlineKeyboardButton(date, user, schedule.Item2), parseMode: ParseMode.Markdown, disableWebPagePreview: true);
        }
    }
}
