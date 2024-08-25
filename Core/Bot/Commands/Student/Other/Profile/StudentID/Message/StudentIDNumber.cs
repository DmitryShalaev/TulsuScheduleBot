using Core.Bot.Commands.Interfaces;
using Core.Bot.MessagesQueue;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;
namespace Core.Bot.Commands.Student.Other.Profile.StudentID.Message {
    internal class StudentIDNumber : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["StudentIDNumber"]];

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            if(user.IsOwner()) {
                user.TelegramUserTmp.Mode = Mode.StudentIDСhange;

                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: "Хотите сменить номер зачётки? Если да, то напишите новый номер", replyMarkup: Statics.CancelKeyboardMarkup);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
