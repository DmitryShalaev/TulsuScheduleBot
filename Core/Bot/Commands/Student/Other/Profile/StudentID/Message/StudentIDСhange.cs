using Core.Bot.Interfaces;

using ScheduleBot;
using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
namespace Core.Bot.Commands.Student.Other.Profile.StudentID.Message {
    internal class StudentIDСhange : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => null;

        public List<Mode> Modes => [Mode.StudentIDСhange];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            await Statics.DeleteTempMessage(user, messageId);

            if(args.Length > 10) {
                user.TelegramUserTmp.RequestingMessageID = (await BotClient.SendTextMessageAsync(chatId: chatId, text: "Номер зачетки не может содержать более 10 символов.", replyMarkup: Statics.CancelKeyboardMarkup)).MessageId;
                await dbContext.SaveChangesAsync();
                return;
            }

            int _messageId = (await BotClient.SendTextMessageAsync(chatId: chatId, text: UserCommands.Instance.Message["WeNeedToWait"], replyMarkup: Statics.CancelKeyboardMarkup)).MessageId;

            if(int.TryParse(args, out int id)) {
                StudentIDLastUpdate? studentID = dbContext.StudentIDLastUpdate.FirstOrDefault(i => i.StudentID == args && i.Update != DateTime.MinValue);

                if(studentID is null && await Parser.Instance.UpdatingProgress(dbContext, args, 0))
                    studentID = dbContext.StudentIDLastUpdate.FirstOrDefault(i => i.StudentID == args && i.Update != DateTime.MinValue);

                if(studentID is not null) {
                    user.TelegramUserTmp.Mode = Mode.Default;
                    user.ScheduleProfile.StudentIDLastUpdate = studentID;
                    await dbContext.SaveChangesAsync();

                    await BotClient.SendTextMessageAsync(chatId: chatId, text: $"Номер зачётки успешно изменен на {args} ", replyMarkup: DefaultMessage.GetProfileKeyboardMarkup(user));

                } else {
                    user.TelegramUserTmp.RequestingMessageID = (await BotClient.SendTextMessageAsync(chatId: chatId, text: UserCommands.Instance.Message["InvalidStudentIDNumber"], replyMarkup: Statics.CancelKeyboardMarkup)).MessageId;
                    await dbContext.SaveChangesAsync();
                }
            } else {
                user.TelegramUserTmp.RequestingMessageID = (await BotClient.SendTextMessageAsync(chatId: chatId, text: "Не удалось распознать введенный номер зачётной книжки", replyMarkup: Statics.CancelKeyboardMarkup)).MessageId;
                await dbContext.SaveChangesAsync();
            }

            await BotClient.DeleteMessageAsync(chatId: chatId, messageId: _messageId);
        }
    }
}
