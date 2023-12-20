using Core.Bot.Interfaces;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Core.Bot.Commands.Student.Back_Cancel.Message {
    public class Back_Cancel : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => new() { UserCommands.Instance.Message["Back"], UserCommands.Instance.Message["Cancel"] };

        public List<Mode> Modes => new() { Mode.Default };

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            if(user.TempData == UserCommands.Instance.Message["AcademicPerformance"] ||
                        user.TempData == UserCommands.Instance.Message["Profile"] ||
                        user.TempData == UserCommands.Instance.Message["Corps"] ||
                        user.TempData == UserCommands.Instance.Message["Exam"]
                        ) {

                await BotClient.SendTextMessageAsync(chatId: chatId, text: UserCommands.Instance.Message["Other"], replyMarkup: Statics.AdditionalKeyboardMarkup);

            } else if(user.TempData == UserCommands.Instance.Message["Settings"]) {
                await BotClient.SendTextMessageAsync(chatId: chatId, text: UserCommands.Instance.Message["Profile"], replyMarkup: DefaultMessage.GetProfileKeyboardMarkup(user));

            } else {
                await BotClient.SendTextMessageAsync(chatId: chatId, text: UserCommands.Instance.Message["MainMenu"], replyMarkup: Statics.MainKeyboardMarkup);
            }

            user.TempData = null;

            await Statics.DeleteTempMessage(user, messageId);
        }
    }
}
