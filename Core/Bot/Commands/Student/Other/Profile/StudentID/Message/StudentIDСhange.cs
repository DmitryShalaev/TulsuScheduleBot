using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Microsoft.EntityFrameworkCore;

using ScheduleBot;

using Telegram.Bot.Types;
using Core.Parser;

namespace Core.Bot.Commands.Student.Other.Profile.StudentID.Message {
    internal class StudentIDСhange : IMessageCommand {

        public List<string>? Commands => null;

        public List<Mode> Modes => [Mode.StudentIDСhange];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {

            if(args.Length > 10) {
                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: "Номер зачетки не может содержать более 10 символов.", replyMarkup: Statics.CancelKeyboardMarkup);
                await dbContext.SaveChangesAsync();
                return;
            }

            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["WeNeedToWait"], replyMarkup: Statics.CancelKeyboardMarkup, saveMessageId: true);

            if(int.TryParse(args, out int id)) {
                StudentIDLastUpdate? studentID = await dbContext.StudentIDLastUpdate.FirstOrDefaultAsync(i => i.StudentID == args && i.Update != DateTime.MinValue);

                if(studentID is null && await ScheduleParser.Instance.UpdatingProgress(dbContext, args, 0))
                    studentID = await dbContext.StudentIDLastUpdate.FirstOrDefaultAsync(i => i.StudentID == args && i.Update != DateTime.MinValue);

                if(studentID is not null) {
                    user.TelegramUserTmp.Mode = Mode.Default;
                    user.ScheduleProfile.StudentIDLastUpdate = studentID;
                    await dbContext.SaveChangesAsync();

                    MessagesQueue.Message.SendTextMessage(chatId: chatId, text: $"Номер зачётки успешно изменен на {args} ", replyMarkup: DefaultMessage.GetProfileKeyboardMarkup(user), deletePrevious: true);

                } else {
                    MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["InvalidStudentIDNumber"], replyMarkup: Statics.CancelKeyboardMarkup, deletePrevious: true);
                    await dbContext.SaveChangesAsync();
                }
            } else {
                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: "Не удалось распознать введенный номер зачётной книжки", replyMarkup: Statics.CancelKeyboardMarkup, deletePrevious: true);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
