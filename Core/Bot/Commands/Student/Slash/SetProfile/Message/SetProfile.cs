using Core.Bot.Commands;
using Core.Bot.Commands.Interfaces;
using Core.Bot.Messages;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
namespace Core.Bot.New.Commands.Student.Slash.SetProfile.Message {
    public class SetProfile : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string> Commands => ["/SetProfile"];

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            try {
                if(Guid.TryParse(args, out Guid profile)) {
                    if(profile != user.ScheduleProfileGuid && dbContext.ScheduleProfile.Any(i => i.ID == profile)) {
                        user.ScheduleProfileGuid = profile;
                        await dbContext.SaveChangesAsync();

                        MessageQueue.SendTextMessage(chatId: chatId, text: "Вы успешно сменили профиль", replyMarkup: Statics.MainKeyboardMarkup);
                    } else {
                        MessageQueue.SendTextMessage(chatId: chatId, text: "Вы пытаетесь изменить свой профиль на текущий или на профиль, который не существует", replyMarkup: Statics.MainKeyboardMarkup);
                    }
                } else {
                    MessageQueue.SendTextMessage(chatId: chatId, text: "Идентификатор профиля не распознан", replyMarkup: Statics.MainKeyboardMarkup);
                }
            } catch(IndexOutOfRangeException) { }
        }
    }
}
