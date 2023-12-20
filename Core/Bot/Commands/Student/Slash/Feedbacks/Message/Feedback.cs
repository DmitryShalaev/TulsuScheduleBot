using Microsoft.EntityFrameworkCore;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Core.Bot.Interfaces;
using Core.Bot.Commands;
namespace Core.Bot.New.Commands.Student.Slash.Feedbacks.Message {
    public class Feedback : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string> Commands => new() { "/feedback" };

        public List<Mode> Modes => Enum.GetValues(typeof(Mode)).Cast<Mode>().ToList();

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            if(user.Mode == Mode.AddingDiscipline)
                dbContext.CustomDiscipline.RemoveRange(dbContext.CustomDiscipline.Where(i => !i.IsAdded && i.ScheduleProfile == user.ScheduleProfile));

            user.TempData = null;
            await Statics.DeleteTempMessage(user);

            if(user.IsAdmin) {
                ScheduleBot.DB.Entity.Feedback? feedback = dbContext.Feedbacks.Include(i => i.TelegramUser).Where(i => !i.IsCompleted).OrderBy(i => i.Date).FirstOrDefault();

                if(feedback is not null) {
                    await BotClient.SendTextMessageAsync(chatId: chatId, text: FeedbackMessage.GetFeedbackMessage(feedback), replyMarkup: FeedbackMessage.GetFeedbackInlineKeyboardButton(dbContext, feedback));
                } else {
                    await BotClient.SendTextMessageAsync(chatId: chatId, text: "Нет новых отзывов и предложений.", replyMarkup: Statics.MainKeyboardMarkup);
                }

                return;
            }

            user.Mode = Mode.Feedback;
            user.RequestingMessageID = (await BotClient.SendTextMessageAsync(chatId: chatId, text: UserCommands.Instance.Message["FeedbackMessage"], replyMarkup: Statics.CancelKeyboardMarkup)).MessageId;
        }
    }
}
