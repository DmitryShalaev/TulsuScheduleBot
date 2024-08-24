using Core.Bot.Commands.Interfaces;

using ScheduleBot;
using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
namespace Core.Bot.Commands.Teachers.Days.ForAWeek.Message {

    internal class TeachersNextWeek : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => [UserCommands.Instance.Message["NextWeek"]];

        public List<Mode> Modes => [Mode.TeacherSelected];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            await Statics.TeacherWorkScheduleRelevance(dbContext, BotClient, chatId, user.TelegramUserTmp.TmpData!, replyMarkup: Statics.WeekKeyboardMarkup);
            foreach((string, DateOnly) item in Scheduler.GetTeacherWorkScheduleByWeak(dbContext, true, user.TelegramUserTmp.TmpData!))
                await BotClient.SendTextMessageAsync(chatId: chatId, text: item.Item1, replyMarkup: Statics.WeekKeyboardMarkup);
        }
    }
}
