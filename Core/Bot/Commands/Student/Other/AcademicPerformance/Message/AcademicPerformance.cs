using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Core.Bot.Interfaces;
namespace Core.Bot.Commands.Student.AcademicPerformance.Message {
    internal class AcademicPerformance : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => new() { UserCommands.Instance.Message["AcademicPerformance"] };

        public List<Mode> Modes => new() { Mode.Default };

        public Manager.Check Check => Manager.Check.studentId;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            user.TempData = UserCommands.Instance.Message["AcademicPerformance"];

            string StudentID = user.ScheduleProfile.StudentID!;

            await Statics.ProgressRelevance(dbContext, BotClient, chatId, StudentID, null, false);
            await BotClient.SendTextMessageAsync(chatId: chatId, text: UserCommands.Instance.Message["AcademicPerformance"], replyMarkup: DefaultMessage.GetTermsKeyboardMarkup(dbContext, StudentID));
        }
    }
}
