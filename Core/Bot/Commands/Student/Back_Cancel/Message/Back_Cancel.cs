using Core.Bot.Interfaces;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Core.Bot.Commands.Student.Back_Cancel.Message {
    public class Back_Cancel : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => [UserCommands.Instance.Message["Back"], UserCommands.Instance.Message["Cancel"]];

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            if(user.TelegramUserTmp.TmpData == UserCommands.Instance.Message["Personal"] ||
               user.TelegramUserTmp.TmpData == UserCommands.Instance.Message["Schedule"] ||
               user.TelegramUserTmp.TmpData == UserCommands.Instance.Message["Corps"]) {

                user.TelegramUserTmp.TmpData = UserCommands.Instance.Message["Other"];
                await BotClient.SendTextMessageAsync(chatId: chatId, text: UserCommands.Instance.Message["Other"], replyMarkup: Statics.OtherKeyboardMarkup);

            } else if(user.TelegramUserTmp.TmpData == UserCommands.Instance.Message["Settings"]) {

                user.TelegramUserTmp.TmpData = UserCommands.Instance.Message["Profile"];
                await BotClient.SendTextMessageAsync(chatId: chatId, text: UserCommands.Instance.Message["Profile"], replyMarkup: DefaultMessage.GetProfileKeyboardMarkup(user));

            } else if(user.TelegramUserTmp.TmpData == UserCommands.Instance.Message["Exam"]) {

                user.TelegramUserTmp.TmpData = UserCommands.Instance.Message["Schedule"];
                await BotClient.SendTextMessageAsync(chatId: chatId, text: UserCommands.Instance.Message["Schedule"], replyMarkup: Statics.ScheduleKeyboardMarkup);

            } else if(user.TelegramUserTmp.TmpData == UserCommands.Instance.Message["AcademicPerformance"] ||
                      user.TelegramUserTmp.TmpData == UserCommands.Instance.Message["Profile"]) {

                user.TelegramUserTmp.TmpData = UserCommands.Instance.Message["Personal"];
                await BotClient.SendTextMessageAsync(chatId: chatId, text: UserCommands.Instance.Message["Personal"], replyMarkup: Statics.PersonalKeyboardMarkup);

            } else {

                user.TelegramUserTmp.TmpData = null;
                await BotClient.SendTextMessageAsync(chatId: chatId, text: UserCommands.Instance.Message["MainMenu"], replyMarkup: Statics.MainKeyboardMarkup);

            }



            await Statics.DeleteTempMessage(user, messageId);

            await dbContext.SaveChangesAsync();
        }
    }
}
