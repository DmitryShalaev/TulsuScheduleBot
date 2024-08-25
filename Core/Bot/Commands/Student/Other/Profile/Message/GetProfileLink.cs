using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Core.Bot.Commands.Student.Other.Profile.Message {
    internal class GetProfileLink : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["GetProfileLink"]];

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.none;

        public Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            if(user.IsOwner()) {
                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: $"Если вы хотите поделиться своим расписанием с кем-то, просто отправьте им следующую команду: " +
                $"\n`/SetProfile {user.ScheduleProfileGuid}`" +
                $"\nЕсли другой пользователь введет эту команду, он сможет видеть расписание с вашими изменениями.", replyMarkup: DefaultMessage.GetProfileKeyboardMarkup(user), parseMode: ParseMode.Markdown, disableWebPagePreview: true);
            } else {
                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: "Поделиться профилем может только его владелец!", replyMarkup: Statics.MainKeyboardMarkup);
            }

            return Task.CompletedTask;
        }
    }
}
