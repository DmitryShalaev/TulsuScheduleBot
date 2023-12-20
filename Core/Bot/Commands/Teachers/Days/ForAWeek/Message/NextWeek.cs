using System.Globalization;

using Core.Bot.Interfaces;

using ScheduleBot;
using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
namespace Core.Bot.Commands.Teachers.Days.ForAWeek.Message {

    internal class TeachersNextWeek : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => new() { UserCommands.Instance.Message["NextWeek"] };

        public List<Mode> Modes => new() { Mode.TeacherSelected };

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            await Statics.TeacherWorkScheduleRelevance(dbContext, BotClient, chatId, user.TempData!, replyMarkup: Statics.WeekKeyboardMarkup);
            foreach((string, DateOnly) item in Scheduler.GetTeacherWorkScheduleByWeak(dbContext, CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday), user.TempData!))
                await BotClient.SendTextMessageAsync(chatId: chatId, text: item.Item1, replyMarkup: Statics.WeekKeyboardMarkup);
        }
    }
}
