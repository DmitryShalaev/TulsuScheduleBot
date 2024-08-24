using Core.Bot.Commands.Interfaces;

using ScheduleBot;
using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
namespace Core.Bot.Commands.Student.Other.Profile.GroupNumber.Message {
    internal class GroupСhange : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => null;

        public List<Mode> Modes => [Mode.GroupСhange];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            await Statics.DeleteTempMessage(user, messageId);

            if(args.Length > 15) {
                user.TelegramUserTmp.RequestingMessageID = (await BotClient.SendTextMessageAsync(chatId: chatId, text: "Номер группы не может содержать более 15 символов.", replyMarkup: Statics.CancelKeyboardMarkup)).MessageId;
                await dbContext.SaveChangesAsync();
                return;
            }

            int _messageId = (await BotClient.SendTextMessageAsync(chatId: chatId, text: UserCommands.Instance.Message["WeNeedToWait"], replyMarkup: Statics.CancelKeyboardMarkup)).MessageId;
            GroupLastUpdate? group = dbContext.GroupLastUpdate.FirstOrDefault(i => i.Group == args && i.Update != DateTime.MinValue);

            if(group is null && await Parser.Instance.UpdatingDisciplines(dbContext, args, 0))
                group = dbContext.GroupLastUpdate.FirstOrDefault(i => i.Group == args && i.Update != DateTime.MinValue);

            if(group is not null) {
                user.TelegramUserTmp.Mode = Mode.Default;
                user.ScheduleProfile.GroupLastUpdate = group;
                await dbContext.SaveChangesAsync();

                await BotClient.SendTextMessageAsync(chatId: chatId, text: $"Номер группы успешно изменен на {args} ", replyMarkup: DefaultMessage.GetProfileKeyboardMarkup(user));

            } else {
                user.TelegramUserTmp.RequestingMessageID = (await BotClient.SendTextMessageAsync(chatId: chatId, text:
                    $"{UserCommands.Instance.Message["IsNoSuchGroup"]}{(DateTime.Now.Month == 8 ? UserCommands.Instance.Message["FreshmanSchedule"] : "")}",
                    replyMarkup: Statics.CancelKeyboardMarkup)).MessageId;
                await dbContext.SaveChangesAsync();
            }

            await BotClient.DeleteMessageAsync(chatId: chatId, messageId: _messageId);
        }
    }
}
