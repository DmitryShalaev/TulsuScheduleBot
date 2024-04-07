using Core.Bot.Interfaces;

using ScheduleBot;
using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
namespace Core.Bot.Commands.Student.AcademicPerformance.Semester.Message {
    internal class Semester : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => [UserCommands.Instance.Message["Semester"]];

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.studentId;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            string StudentID = user.ScheduleProfile.StudentID!;

            await Statics.ProgressRelevance(dbContext, BotClient, chatId, StudentID, DefaultMessage.GetTermsKeyboardMarkup(dbContext, StudentID));
            await BotClient.SendTextMessageAsync(chatId: chatId, text: Scheduler.GetProgressByTerm(dbContext, int.Parse(args), StudentID), replyMarkup: DefaultMessage.GetTermsKeyboardMarkup(dbContext, StudentID));
        }
    }
}
