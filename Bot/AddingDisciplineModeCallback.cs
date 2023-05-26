using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace ScheduleBot.Bot {
    public partial class TelegramBot {

        private async Task AddingDisciplineCallbackModeAsync(Message message, ITelegramBotClient botClient, TelegramUser user, CancellationToken cancellationToken, string data) {

            List<string> str = data.Split(' ').ToList() ?? new();
            if(str.Count == 0) return;

            switch(str[0]) {
                case Constants.IK_SetEndTime.callback:
                    var temporaryAddition = dbContext.TemporaryAddition.Where(i => i.TelegramUser == user).OrderByDescending(i => i.AddDate).First();

                    temporaryAddition.EndTime = TimeOnly.Parse(str[1]);
                    temporaryAddition.Counter++;

                    await SaveAddingDisciplineAsync(user, message, botClient, temporaryAddition);

                    break;
            }
        }

    }
}