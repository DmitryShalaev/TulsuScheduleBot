using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace ScheduleBot.Bot {
    public partial class TelegramBot {
        private async Task GroupСhangeMessageMode(ITelegramBotClient botClient, ChatId chatId, string message, TelegramUser user) {

            await botClient.SendTextMessageAsync(chatId: chatId, text: "Нужно подождать...", replyMarkup: CancelKeyboardMarkup);
            Parser parser = new(dbContext, false);
            bool flag = dbContext.ScheduleProfile.Select(i=>i.Group).Contains(message);

            if(flag || parser.GetDates(message) is not null) {
                user.Mode = Mode.Default;
                user.ScheduleProfile.Group = message;
                dbContext.SaveChanges();

                await botClient.SendTextMessageAsync(chatId: chatId, text: $"Номер группы успешно изменен на {message} ", replyMarkup: GetProfileKeyboardMarkup(user));

                if(!flag)
                    parser.UpdatingDisciplines(message);
            } else {
                await botClient.SendTextMessageAsync(chatId: chatId, text: "Сайт ТулГУ не отвечает или такой группы не существует", replyMarkup: CancelKeyboardMarkup);
            }
        }

        private async Task StudentIDСhangeMessageMode(ITelegramBotClient botClient, ChatId chatId, string message, TelegramUser user) {

            await botClient.SendTextMessageAsync(chatId: chatId, text: "Нужно подождать...", replyMarkup: CancelKeyboardMarkup);

            if(int.TryParse(message ?? "", out int studentID)) {
                Parser parser = new(dbContext, false);
                bool flag = dbContext.ScheduleProfile.Select(i => i.StudentID).Contains(message);

                if(flag || parser.GetProgress(message ?? "") is not null) {
                    user.Mode = Mode.Default;
                    user.ScheduleProfile.StudentID = studentID.ToString();
                    dbContext.SaveChanges();

                    await botClient.SendTextMessageAsync(chatId: chatId, text: $"Номер зачётки успешно изменен на {message} ", replyMarkup: GetProfileKeyboardMarkup(user));

                    if(!flag)
                        parser.UpdatingProgress(studentID.ToString());
                } else {
                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Сайт ТулГУ не отвечает или указан неверный номер зачётки", replyMarkup: CancelKeyboardMarkup);
                }

            } else {
                await botClient.SendTextMessageAsync(chatId: chatId, text: "Не удалось распознать введенный номер зачётной книжки", replyMarkup: CancelKeyboardMarkup);
            }
        }
    }
}
