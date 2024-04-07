using Core.Bot.Interfaces;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Core.Bot.Commands.Teachers.Back.Message {
    public class TeachersWorkScheduleBack : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => [UserCommands.Instance.Message["TeachersWorkScheduleBack"]];

        public List<Mode> Modes => [Mode.TeachersWorkSchedule, Mode.TeacherSelected];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            await BotClient.SendTextMessageAsync(chatId: chatId, text: UserCommands.Instance.Message["MainMenu"], replyMarkup: Statics.MainKeyboardMarkup);

            user.TelegramUserTmp.Mode = Mode.Default;
            user.TelegramUserTmp.TmpData = null;

            await Statics.DeleteTempMessage(user, messageId);
        }
    }
}
