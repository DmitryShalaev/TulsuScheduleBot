using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Microsoft.EntityFrameworkCore;

using Telegram.Bot.Types;
namespace Core.Bot.Commands.Admin.Feedbacks.Callback {
    public class FeedbackReply : ICallbackCommand {

        public string Command => "FeedbackReply";

        public Mode Mode => Mode.Admin;

        public Manager.Check Check => Manager.Check.admin;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args) {
            TelegramUser? telegramUser = (await dbContext.Feedbacks.Include(i => i.TelegramUser).FirstOrDefaultAsync(i => i.ID == long.Parse(args)))?.TelegramUser;
            if(telegramUser is null) {
                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: "Отзыв не найден", replyMarkup: Statics.AdminPanelKeyboardMarkup);
                return;
            }

            user.TelegramUserTmp.TmpData = $"FeedbackReply|{args}";
            user.TelegramUserTmp.Mode = Mode.Messenger;
            await dbContext.SaveChangesAsync();

            string username = !string.IsNullOrWhiteSpace(telegramUser.Username)
                ? $"[{Statics.EscapeSpecialCharacters($"{telegramUser.FirstName} {telegramUser.LastName}")}](https://t.me/{telegramUser.Username})"
                : Statics.EscapeSpecialCharacters($"{telegramUser.FirstName} {telegramUser.LastName}");

            MessagesQueue.Message.DeleteMessage(chatId: chatId, messageId: messageId);
            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: $"Ответ пользователю: {username}", replyMarkup: Statics.CancelKeyboardMarkup, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
        }
    }
}
