using Core.Bot.Commands.Interfaces;
using Core.Bot.MessagesQueue;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;
namespace Core.Bot.Commands.Student.Other.Profile.Message {
    internal class ResetProfileLinkConfirm : IMessageCommand {

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

            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["Profile"], replyMarkup: DefaultMessage.GetProfileKeyboardMarkup(user));
        }
    }
}
