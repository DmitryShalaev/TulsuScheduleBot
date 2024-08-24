using Core.Bot.Commands.Interfaces;

using ScheduleBot;
using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
namespace Core.Bot.Commands.Teachers.Days.ByDays.Message {
    internal class TeachersSaturday : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => [UserCommands.Instance.Message["Saturday"]];

        public List<Mode> Modes => [Mode.TeacherSelected];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            await Statics.TeacherWorkScheduleRelevance(dbContext, BotClient, chatId, user.TelegramUserTmp.TmpData!, Statics.DaysKeyboardMarkup);
            foreach((string, DateOnly) day in Scheduler.GetTeacherWorkScheduleByDay(dbContext, DayOfWeek.Saturday, user.TelegramUserTmp.TmpData!))
                await BotClient.SendTextMessageAsync(chatId: chatId, text: day.Item1, replyMarkup: Statics.DaysKeyboardMarkup);
        }
    }
}
