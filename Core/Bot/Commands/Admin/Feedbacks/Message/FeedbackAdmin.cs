using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Microsoft.EntityFrameworkCore;

using Telegram.Bot.Types;

namespace Core.Bot.Commands.Admin.Feedbacks.Message {
    internal class FeedbackAdmin : IMessageCommand {

        public List<string> Commands => ["Отзывы"];

        public List<Mode> Modes => [Mode.Admin];

        public Manager.Check Check => Manager.Check.admin;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
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