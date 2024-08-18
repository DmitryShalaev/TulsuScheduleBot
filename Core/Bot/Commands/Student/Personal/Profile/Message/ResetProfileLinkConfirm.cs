using Core.Bot.Interfaces;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
namespace Core.Bot.Commands.Student.Personal.Profile.Message {
    internal class ResetProfileLinkConfirm : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => [UserCommands.Instance.Message["Reset"]];

        public List<Mode> Modes => [Mode.ResetProfileLink];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            user.TelegramUserTmp.Mode = Mode.Default;

            ScheduleProfile? profile = dbContext.ScheduleProfile.FirstOrDefault(i => i.OwnerID == user.ChatID);
            if(profile is not null) {
                user.ScheduleProfile = profile;
            } else {
                profile = new() { OwnerID = user.ChatID };
                dbContext.ScheduleProfile.Add(profile);
                user.ScheduleProfile = profile;
            }

            await dbContext.SaveChangesAsync();

            await BotClient.SendTextMessageAsync(chatId: chatId, text: UserCommands.Instance.Message["Profile"], replyMarkup: DefaultMessage.GetProfileKeyboardMarkup(user));
        }
    }
}
