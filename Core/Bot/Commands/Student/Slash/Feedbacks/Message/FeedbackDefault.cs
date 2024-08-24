using Core.Bot.Commands;
using Core.Bot.Commands.Interfaces;
using Core.Bot.Messages;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace Core.Bot.New.Commands.Student.Slash.Feedbacks.Message {
    public class FeedbackDefault : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => null;

        public List<Mode> Modes => [Mode.Feedback];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            user.TelegramUserTmp.Mode = Mode.Default;
            user.TelegramUserTmp.TmpData = null;

            dbContext.Feedbacks.Add(new() { Message = args, TelegramUser = user });

            MessageQueue.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["ThanksForTheFeedback"], replyMarkup: Statics.MainKeyboardMarkup);

            await dbContext.SaveChangesAsync();

            foreach(TelegramUser? item in dbContext.TelegramUsers.Where(i => i.IsAdmin))
                MessageQueue.SendTextMessage(chatId: item.ChatID, text: $"Получен новый отзыв или предложение. От {user.FirstName}\n/feedback", disableNotification: true);
        }
    }
}
