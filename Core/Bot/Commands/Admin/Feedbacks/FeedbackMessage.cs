using Core.DB;
using Core.DB.Entity;

using Microsoft.EntityFrameworkCore;

using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Core.Bot.Commands.Admin.Feedbacks {
    public static class FeedbackMessage {
        public static string GetFeedbackMessage(Feedback feedback) {
            return $"От: {feedback.TelegramUser.FirstName}{(string.IsNullOrWhiteSpace(feedback.TelegramUser.LastName) ? "" : $", {feedback.TelegramUser.LastName}")}{(string.IsNullOrWhiteSpace(feedback.TelegramUser.Username) ? "" : $", [{feedback.TelegramUser.Username}](https://t.me/{feedback.TelegramUser.Username})")}\n" +
                   $"Дата: {feedback.Date.ToLocalTime():dd.MM.yy HH:mm:ss}\n\n" +
                   $"{feedback.Message}";
        }

        public static InlineKeyboardMarkup GetFeedbackInlineKeyboardButton(ScheduleDbContext dbContext, Feedback feedback) {
            bool previous = dbContext.Feedbacks.Any(i => !i.IsCompleted && i.ID < feedback.ID);
            bool next = dbContext.Feedbacks.Any(i => !i.IsCompleted && i.ID > feedback.ID); ;

            return new InlineKeyboardMarkup(new List<InlineKeyboardButton[]> {
                            new[] { InlineKeyboardButton.WithCallbackData(text: previous ? "⬅️":"❌", callbackData: $"FeedbackPrevious {feedback.ID}"),
                                    InlineKeyboardButton.WithCallbackData(text: "✅", callbackData: $"FeedbackAccept {feedback.ID}"),
                                    InlineKeyboardButton.WithCallbackData(text: "✏️", callbackData: $"FeedbackReply {feedback.ID}"),
                                    InlineKeyboardButton.WithCallbackData(text: next ? "➡️":"❌", callbackData: $"FeedbackNext {feedback.ID}") }
                        });
        }

        public static async Task GetFeedback(ScheduleDbContext dbContext, ChatId chatId) {
            Feedback? feedback = await dbContext.Feedbacks.Include(i => i.TelegramUser).Where(i => !i.IsCompleted).OrderBy(i => i.Date).FirstOrDefaultAsync();

            if(feedback is not null) {
                MessagesQueue.Message.SendTextMessage(
                    chatId: chatId,
                    text: FeedbackMessage.GetFeedbackMessage(feedback),
                    replyMarkup: FeedbackMessage.GetFeedbackInlineKeyboardButton(dbContext, feedback),
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);

            } else {
                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: "Нет новых отзывов и предложений.", replyMarkup: Statics.AdminPanelKeyboardMarkup);
            }
        }
    }
}
