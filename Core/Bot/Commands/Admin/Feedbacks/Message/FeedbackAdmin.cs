using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Microsoft.EntityFrameworkCore;

using Telegram.Bot.Types;

namespace Core.Bot.Commands.Admin.Feedbacks.Message {
    internal class FeedbackAdmin : IMessageCommand {

        public List<string> Commands => ["Отзывы"];

        public List<Mode> Modes => Enum.GetValues(typeof(Mode)).Cast<Mode>().ToList();

        public Manager.Check Check => Manager.Check.none;

        public Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            Feedback? feedback = dbContext.Feedbacks.Include(i => i.TelegramUser).Where(i => !i.IsCompleted).OrderBy(i => i.Date).FirstOrDefault();

            if(feedback is not null) MessagesQueue.Message.SendTextMessage(chatId: chatId, text: FeedbackMessage.GetFeedbackMessage(feedback), replyMarkup: FeedbackMessage.GetFeedbackInlineKeyboardButton(dbContext, feedback));
            else {
                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: "Нет новых отзывов и предложений.", replyMarkup: Statics.AdminPanelKeyboardMarkup);
            }

            return Task.CompletedTask;
        }
    }
}