using System.Text.RegularExpressions;

using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Microsoft.EntityFrameworkCore;

using Telegram.Bot.Types;

namespace Core.Bot.Commands.Messenger.Message {
    public partial class MessengerDefault : IMessageCommand {

        public List<string>? Commands => null;

        public List<Mode> Modes => [Mode.Messenger];

        public Manager.Check Check => Manager.Check.none;

        [GeneratedRegex("^([A-z]+)?\\|?([0-9]+)$")]
        public static partial Regex MessengerRegex();

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            Match match = MessengerRegex().Match(user.TelegramUserTmp.TmpData ?? "");
            if(match.Success) {

                if(match.Groups[1].ToString() == "FeedbackReply") {
                    long feedbackID = long.Parse(match.Groups[2].ToString());
                    Feedback? feedback = await dbContext.Feedbacks.Include(i => i.TelegramUser).FirstOrDefaultAsync(i => i.ID == feedbackID);
                    if(feedback is null) {
                        MessagesQueue.Message.SendTextMessage(chatId: chatId, text: "Отзыв не найден", replyMarkup: Statics.AdminPanelKeyboardMarkup);
                        return;
                    }

                    feedback.IsCompleted = true;

                    DB.Entity.Messenger messenger = new() { FeedbackID = feedbackID, TelegramUser = user, Message = args };

                    dbContext.Messenger.Add(messenger);
                    dbContext.SaveChanges();

                    MessagesQueue.Message.SendTextMessage(feedback.From, $"Новое сообщение от: Администратор\n\n{args}\n\nЧтобы ответить, используйте: /feedback", replyMarkup: DefaultMessage.GetMainKeyboardMarkup(user), disableNotification: true);
                }

                MessagesQueue.Message.SendTextMessage(chatId, "Сообщение отправлено!", replyMarkup: DefaultMessage.GetMainKeyboardMarkup(user));
            } else {
                MessagesQueue.Message.SendTextMessage(chatId, UserCommands.Instance.Message["MainMenu"], replyMarkup: DefaultMessage.GetMainKeyboardMarkup(user));
            }

            user.TelegramUserTmp.TmpData = null;
            user.TelegramUserTmp.Mode = Mode.Default;

            await dbContext.SaveChangesAsync();
        }
    }
}
