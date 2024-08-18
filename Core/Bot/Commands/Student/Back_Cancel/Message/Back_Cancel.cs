using Core.Bot.Interfaces;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Core.Bot.Commands.Student.Back_Cancel.Message {
    public class Back_Cancel : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => [UserCommands.Instance.Message["Back"], UserCommands.Instance.Message["Cancel"]];

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.none;

        private readonly static Dictionary<string, string> commands = UserCommands.Instance.Message;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            var transitions = new Dictionary<string, (string nextState, IReplyMarkup replyMarkup)> {
                                    { commands["AcademicPerformance"], (commands["Other"], Statics.OtherKeyboardMarkup) },
                                    { commands["Profile"], (commands["Other"], Statics.OtherKeyboardMarkup) },
                                    { commands["Settings"], (commands["Profile"], DefaultMessage.GetProfileKeyboardMarkup(user)) },
                                    { commands["Exam"], (commands["Schedule"], Statics.ScheduleKeyboardMarkup) }
                                };

            if(user.TelegramUserTmp.TmpData is not null && transitions.TryGetValue(user.TelegramUserTmp.TmpData, out var transition)) {
                user.TelegramUserTmp.TmpData = transition.nextState;
                await BotClient.SendTextMessageAsync(chatId, transition.nextState, replyMarkup: transition.replyMarkup);
            } else {
                user.TelegramUserTmp.TmpData = null;
                await BotClient.SendTextMessageAsync(chatId, commands["MainMenu"], replyMarkup: Statics.MainKeyboardMarkup);
            }

            await Statics.DeleteTempMessage(user, messageId);
            await dbContext.SaveChangesAsync();
        }
    }
}
