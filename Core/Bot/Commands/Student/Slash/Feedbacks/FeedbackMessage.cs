using ScheduleBot.DB;

using Telegram.Bot.Types.ReplyMarkups;

namespace Core.Bot.New.Commands.Student.Slash.Feedbacks {
    public static class FeedbackMessage {
        public static string GetFeedbackMessage(ScheduleBot.DB.Entity.Feedback feedback) {
            return $"От: {feedback.TelegramUser.FirstName}{(string.IsNullOrWhiteSpace(feedback.TelegramUser.LastName) ? "" : $", {feedback.TelegramUser.LastName}")}{(string.IsNullOrWhiteSpace(feedback.TelegramUser.Username) ? "" : $", {feedback.TelegramUser.Username}")}\n" +
                   $"Дата: {feedback.Date.ToLocalTime():dd.MM.yy HH:mm:ss}\n\n" +
                   $"{feedback.Message}";
        }

        public static InlineKeyboardMarkup GetFeedbackInlineKeyboardButton(ScheduleDbContext dbContext, ScheduleBot.DB.Entity.Feedback feedback) {
            bool previous = dbContext.Feedbacks.Any(i => !i.IsCompleted && i.ID < feedback.ID);
            bool next = dbContext.Feedbacks.Any(i => !i.IsCompleted && i.ID > feedback.ID); ;

            return new InlineKeyboardMarkup(new List<InlineKeyboardButton[]> {
                            new[] { InlineKeyboardButton.WithCallbackData(text: previous ? "⬅️":"❌", callbackData: $"FeedbackPrevious {feedback.ID}"),
                                    InlineKeyboardButton.WithCallbackData(text: "✅", callbackData: $"FeedbackAccept {feedback.ID}"),
                                    InlineKeyboardButton.WithCallbackData(text: next ? "➡️":"❌", callbackData: $"FeedbackNext {feedback.ID}") }
                        });
        }
    }
}
