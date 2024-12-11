using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;
namespace Core.Bot.Commands.Student.Slash.Feedbacks.Message {
    public class Feedback : IMessageCommand {

        public List<string> Commands => ["/feedback"];

        public List<Mode> Modes => Enum.GetValues<Mode>().Cast<Mode>().ToList();

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            if(user.TelegramUserTmp.Mode == Mode.AddingDiscipline)
                dbContext.CustomDiscipline.RemoveRange(dbContext.CustomDiscipline.Where(i => !i.IsAdded && i.ScheduleProfile == user.ScheduleProfile));

            user.TelegramUserTmp.TmpData = null;

            user.TelegramUserTmp.Mode = Mode.Feedback;
            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["FeedbackMessage"], replyMarkup: Statics.CancelKeyboardMarkup, deletePrevious: true);

            await dbContext.SaveChangesAsync();
        }
    }
}
