using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace ScheduleBot.Bot {
    public partial class TelegramBot {

        private async Task ResetProfileLink(ITelegramBotClient botClient, Message message, TelegramUser user) {
            switch(message.Text) {
                case Constants.RK_Cancel:
                    user.Mode = Mode.Default;
                    dbContext.SaveChanges();

                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Основное меню", replyMarkup: MainKeyboardMarkup);
                    break;

                case Constants.RK_Reset:
                    user.Mode = Mode.Default;

                    var profile = dbContext.ScheduleProfile.FirstOrDefault(i => i.OwnerID == user.ChatID);
                    if(profile is not null) {
                        user.ScheduleProfile = profile;
                    } else {
                        profile = new() { OwnerID = user.ChatID };
                        dbContext.ScheduleProfile.Add(profile);
                        user.ScheduleProfile = profile;
                    }

                    dbContext.SaveChanges();

                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Основное меню", replyMarkup: MainKeyboardMarkup);
                    break;
            }
        }
    }
}
