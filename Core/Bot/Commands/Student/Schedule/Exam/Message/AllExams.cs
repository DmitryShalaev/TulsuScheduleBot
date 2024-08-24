using Core.Bot.Commands.Interfaces;

using ScheduleBot;
using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Core.Bot.Commands.Student.Additional.Exam.Message {
    internal class AllExams : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => [UserCommands.Instance.Message["AllExams"]];

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.group;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            await Statics.ScheduleRelevance(dbContext, BotClient, chatId, user.ScheduleProfile.Group!, Statics.ExamKeyboardMarkup);
            foreach(string item in Scheduler.GetExamse(dbContext, user.ScheduleProfile, true))
                await BotClient.SendTextMessageAsync(chatId: chatId, text: item, replyMarkup: Statics.ExamKeyboardMarkup);
        }
    }
}
