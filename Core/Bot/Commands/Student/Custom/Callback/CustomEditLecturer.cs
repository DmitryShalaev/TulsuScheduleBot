using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;
namespace Core.Bot.Commands.Student.Custom.Callback {
    public class CustomEditLecturer : ICallbackCommand {

        public string Command => "CustomEditLecturer";

        public Mode Mode => Mode.Default;

        public Manager.Check Check => Manager.Check.group;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args) {
            await CustomEditMessage.CustomEdit(dbContext, chatId, messageId, user, args, Mode.CustomEditLecturer,
                "Хотите изменить лектора? Если да, то напишите нового");
        }
    }
}
