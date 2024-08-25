using Core.Bot.Commands.AddingDiscipline;
using Core.Bot.Commands.Interfaces;
using Core.Bot.MessagesQueue;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;

namespace Core.Bot.Commands.Student.AddingDiscipline.Callback {
    public class SetEndTime : ICallbackCommand {

        public string Command => UserCommands.Instance.Callback["SetEndTime"].callback;

        public Mode Mode => Mode.AddingDiscipline;

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args) {
            MessagesQueue.Message.EditMessageReplyMarkup(chatId, messageId);
            await AddingDisciplineMode.SetStagesAddingDisciplineAsync(dbContext, chatId, args, user);
        }
    }
}
