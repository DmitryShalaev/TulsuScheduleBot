using ScheduleBot;
using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Core.Bot.Interfaces;
namespace Core.Bot.Commands.Teachers.Days.Message {
    internal class TeachersTomorrow : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => new() { UserCommands.Instance.Message["Tomorrow"] };

        public List<Mode> Modes => new() { Mode.TeacherSelected };

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            ReplyKeyboardMarkup teacherWorkSchedule = DefaultMessage.GetTeacherWorkScheduleSelectedKeyboardMarkup(user.TempData!);

            await Statics.TeacherWorkScheduleRelevance(dbContext, BotClient, chatId, user.TempData!, teacherWorkSchedule);
            var date = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
            await BotClient.SendTextMessageAsync(chatId: chatId, text: Scheduler.GetTeacherWorkScheduleByDate(dbContext, date, user.TempData!), replyMarkup: teacherWorkSchedule);
        }
    }
}
