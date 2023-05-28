using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;

namespace ScheduleBot.Bot {
    public partial class TelegramBot {
        private async Task GroupСhangeMessageMode(ITelegramBotClient botClient, Message message, TelegramUser user) {
            switch(message.Text) {
                case Constants.RK_Cancel:
                    user.Mode = Mode.Default;
                    dbContext.SaveChanges();

                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Профиль", replyMarkup: GetProfileKeyboardMarkup(user));
                    break;

                default:
                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Нужно подождать...", replyMarkup: CancelKeyboardMarkup);
                    Parser parser = new(dbContext, false);
                    bool flag = dbContext.ScheduleProfile.Select(i=>i.Group).Contains(message.Text);

                    if(flag || parser.GetDates(message.Text ?? "") is not null) {
                        user.Mode = Mode.Default;
                        user.ScheduleProfile.Group = message.Text;
                        dbContext.SaveChanges();

                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: $"Номер группы успешно изменен на {message.Text} ", replyMarkup: GetProfileKeyboardMarkup(user));

                        if(!flag)
                            parser.UpdatingDisciplines(message.Text);
                    } else {
                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Сайт ТулГУ не отвечает или такой группы не существует", replyMarkup: CancelKeyboardMarkup);
                    }

                    break;
            }

        }

        private async Task StudentIDСhangeMessageMode(ITelegramBotClient botClient, Message message, TelegramUser user) {
            switch(message.Text) {
                case Constants.RK_Cancel:
                    user.Mode = Mode.Default;
                    dbContext.SaveChanges();

                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Профиль", replyMarkup: GetProfileKeyboardMarkup(user));
                    break;

                default:
                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Нужно подождать...", replyMarkup: CancelKeyboardMarkup);

                    if(int.TryParse(message.Text ?? "", out int studentID)) {
                        Parser parser = new(dbContext, false);
                        bool flag = dbContext.ScheduleProfile.Select(i => i.StudentID).Contains(message.Text);

                        if(flag || parser.GetProgress(message.Text ?? "") is not null) {
                            user.Mode = Mode.Default;
                            user.ScheduleProfile.StudentID = studentID.ToString();
                            dbContext.SaveChanges();

                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: $"Номер зачётки успешно изменен на {message.Text} ", replyMarkup: GetProfileKeyboardMarkup(user));

                            if(!flag)
                                parser.UpdatingProgress(studentID.ToString());
                        } else {
                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Сайт ТулГУ не отвечает или указан неверный номер зачётки", replyMarkup: CancelKeyboardMarkup);
                        }

                    } else {
                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Не удалось распознать введенный номер зачётной книжки", replyMarkup: CancelKeyboardMarkup);
                    }
                    break;
            }
        }
    }
}
