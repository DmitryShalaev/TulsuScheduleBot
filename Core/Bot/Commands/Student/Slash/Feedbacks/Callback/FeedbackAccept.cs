using Core.Bot.Commands;
using Core.Bot.Commands.Interfaces;

using Microsoft.EntityFrameworkCore;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
namespace Core.Bot.New.Commands.Student.Slash.Feedbacks.Callback {
    public class FeedbackAccept : ICallbackCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public string Command => "FeedbackAccept";

        public Mode Mode => Mode.Default;

        public Manager.Check Check => Manager.Check.admin;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args) {
            Feedback? feedback = dbContext.Feedbacks.Include(i => i.TelegramUser).FirstOrDefault(i => i.ID == long.Parse(args));

            if(feedback is not null) {
                feedback.IsCompleted = true;
                await dbContext.SaveChangesAsync();
            }

            feedback = dbContext.Feedbacks.Include(i => i.TelegramUser).Where(i => !i.IsCompleted).OrderBy(i => i.Date).FirstOrDefault();
            if(feedback is not null) {
                await BotClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: FeedbackMessage.GetFeedbackMessage(feedback), replyMarkup: DefaultCallback.GetFeedbackInlineKeyboardButton(dbContext, feedback));
            } else {
                await BotClient.DeleteMessageAsync(chatId: chatId, messageId: messageId);
                await BotClient.SendTextMessageAsync(chatId: chatId, text: "Нет новых отзывов и предложений.", replyMarkup: Statics.MainKeyboardMarkup);
            }
        }
    }
}
