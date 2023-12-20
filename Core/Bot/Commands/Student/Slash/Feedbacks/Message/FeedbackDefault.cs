using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Core.Bot.Interfaces;
using Core.Bot.Commands;

namespace Core.Bot.New.Commands.Student.Slash.Feedbacks.Message {
    public class FeedbackDefault : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => null;

        public List<Mode> Modes => new() { Mode.Feedback };

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            user.Mode = Mode.Default;
            user.TempData = null;

            dbContext.Feedbacks.Add(new() { Message = args, TelegramUser = user });

            await BotClient.SendTextMessageAsync(chatId: chatId, text: UserCommands.Instance.Message["ThanksForTheFeedback"], replyMarkup: Statics.MainKeyboardMarkup);
            await Statics.DeleteTempMessage(user);

            foreach(TelegramUser? item in dbContext.TelegramUsers.Where(i => i.IsAdmin))
                await BotClient.SendTextMessageAsync(chatId: item.ChatID, text: $"Получен новый отзыв или предложение. От {user.FirstName}\n/feedback", disableNotification: true);
        }
    }
}
