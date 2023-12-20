using Microsoft.EntityFrameworkCore;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Core.Bot.Interfaces;
using Core.Bot.Commands;
namespace Core.Bot.New.Commands.Student.Slash.Feedbacks.Callback {
    public class FeedbackNext : ICallbackCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public string Command => "FeedbackNext";

        public Mode Mode => Mode.Default;

        public Manager.Check Check => Manager.Check.admin;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args) {
            Feedback? feedback = dbContext.Feedbacks.Include(i => i.TelegramUser).Where(i => !i.IsCompleted && i.ID > long.Parse(args)).OrderBy(i => i.Date).FirstOrDefault();

            if(feedback is not null)
                await BotClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: FeedbackMessage.GetFeedbackMessage(feedback), replyMarkup: DefaultCallback.GetFeedbackInlineKeyboardButton(dbContext, feedback));
        }
    }
}
