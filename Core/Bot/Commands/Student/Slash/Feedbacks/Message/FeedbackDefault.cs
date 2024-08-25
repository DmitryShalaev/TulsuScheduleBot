using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;

namespace Core.Bot.Commands.Student.Slash.Feedbacks.Message {
    public class FeedbackDefault : IMessageCommand {

        public List<string>? Commands => null;

        public List<Mode> Modes => [Mode.Feedback];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            user.TelegramUserTmp.Mode = Mode.Default;
            user.TelegramUserTmp.TmpData = null;

            dbContext.Feedbacks.Add(new() { Message = args, TelegramUser = user });

            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["ThanksForTheFeedback"], replyMarkup: Statics.MainKeyboardMarkup);

            await dbContext.SaveChangesAsync();

            foreach(TelegramUser? item in dbContext.TelegramUsers.Where(i => i.IsAdmin))
                MessagesQueue.Message.SendTextMessage(chatId: item.ChatID, text: $"Получен новый отзыв или предложение. От {user.FirstName}\n/feedback", disableNotification: true);
        }
    }
}
