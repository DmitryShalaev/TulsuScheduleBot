using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;

namespace Core.Bot.Commands.Admin.Feedbacks.Message {
    internal class FeedbackAdmin : IMessageCommand {

        public List<string> Commands => ["Отзывы"];

        public List<Mode> Modes => [Mode.Admin];

        public Manager.Check Check => Manager.Check.admin;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) => await FeedbackMessage.GetFeedback(dbContext, chatId);
    }
}