using Core.Bot.Commands.Interfaces;

using ScheduleBot;
using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace Core.Bot.Commands.Classrooms.Days.ByDays.Message {
    internal class ClassroomTuesday : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => [UserCommands.Instance.Message["Tuesday"]];

        public List<Mode> Modes => [Mode.ClassroomSelected];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            await Statics.ClassroomWorkScheduleRelevance(dbContext, BotClient, chatId, user.TelegramUserTmp.TmpData!, Statics.DaysKeyboardMarkup);
            foreach((string, DateOnly) day in Scheduler.GetClassroomWorkScheduleByDay(dbContext, DayOfWeek.Tuesday, user.TelegramUserTmp.TmpData!, user))
                await BotClient.SendTextMessageAsync(chatId: chatId, text: day.Item1, replyMarkup: Statics.DaysKeyboardMarkup, parseMode: ParseMode.Markdown, disableWebPagePreview: true);
        }
    }
}
