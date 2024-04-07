using Core.Bot.Interfaces;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Core.Bot.Commands.Student.Other.Profile.Message {
    internal class GetProfileLink : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => [UserCommands.Instance.Message["GetProfileLink"]];

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            if(user.IsOwner()) {
                await BotClient.SendTextMessageAsync(chatId: chatId, text: $"Если вы хотите поделиться своим расписанием с кем-то, просто отправьте им следующую команду: " +
                $"\n`/SetProfile {user.ScheduleProfileGuid}`" +
                $"\nЕсли другой пользователь введет эту команду, он сможет видеть расписание с вашими изменениями.", replyMarkup: DefaultMessage.GetProfileKeyboardMarkup(user), parseMode: ParseMode.Markdown);
            } else {
                await BotClient.SendTextMessageAsync(chatId: chatId, text: "Поделиться профилем может только его владелец!", replyMarkup: Statics.MainKeyboardMarkup);
            }
        }
    }
}
