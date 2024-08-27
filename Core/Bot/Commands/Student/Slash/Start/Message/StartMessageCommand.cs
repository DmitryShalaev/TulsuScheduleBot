using Core.Bot.Commands;
using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;
namespace Core.Bot.New.Commands.Student.Slash.Start.Message {
    public class StartMessageCommand : IMessageCommand {

        public List<string> Commands => ["/start"];

        public List<Mode> Modes => Enum.GetValues(typeof(Mode)).Cast<Mode>().ToList();

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: "👋", replyMarkup: DefaultMessage.GetMainKeyboardMarkup(user), deletePrevious: true);

            if(user.TelegramUserTmp.Mode == Mode.AddingDiscipline)
                dbContext.CustomDiscipline.RemoveRange(dbContext.CustomDiscipline.Where(i => !i.IsAdded && i.ScheduleProfile == user.ScheduleProfile));

            user.TelegramUserTmp.TmpData = null;
            user.TelegramUserTmp.Mode = Mode.Default;

            if(string.IsNullOrWhiteSpace(user.ScheduleProfile.Group)) {
                user.TelegramUserTmp.Mode = Mode.GroupСhange;

                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: "Для начала работы с ботом укажите номер учебной группы.", replyMarkup: Statics.CancelKeyboardMarkup);
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
