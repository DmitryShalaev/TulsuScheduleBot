using Core.Bot.Interfaces;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
namespace Core.Bot.Commands.Student.Additional.Exam.Message {
    internal class Exam : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => [UserCommands.Instance.Message["Exam"]];

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.group;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            user.TelegramUserTmp.TmpData = UserCommands.Instance.Message["Exam"];
            await dbContext.SaveChangesAsync();

            await BotClient.SendTextMessageAsync(chatId: chatId, text: UserCommands.Instance.Message["Exam"], replyMarkup: Statics.ExamKeyboardMarkup);
        }
    }
}
