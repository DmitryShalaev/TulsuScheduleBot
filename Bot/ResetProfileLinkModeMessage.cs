using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace ScheduleBot.Bot {
    public partial class TelegramBot {

        private async Task ResetProfileLink(ITelegramBotClient botClient, Message message, TelegramUser user) {

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

            await botClient.SendTextMessageAsync(chatId: message.Chat, text: Constants.RK_MainMenu, replyMarkup: MainKeyboardMarkup);
        }
    }
}
