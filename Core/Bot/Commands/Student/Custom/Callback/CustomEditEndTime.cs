using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;

namespace Core.Bot.Commands.Student.Custom.Callback {
    public class CustomEditEndTime : ICallbackCommand {

        public string Command => "CustomEditEndTime";

        public Mode Mode => Mode.Default;

        public Manager.Check Check => Manager.Check.group;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args) {
            await CustomEditMessage.CustomEdit(dbContext, chatId, messageId, user, args, Mode.CustomEditEndTime,
               "Хотите изменить время конца пары? Если да, то напишите новое");
        }
    }
}
