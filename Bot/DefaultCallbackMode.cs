using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.Bot {
    public partial class TelegramBot {
        #region InlineKeyboardMarkup
        private readonly InlineKeyboardMarkup inlineAdminKeyboardMarkup = new(new[]{
            new [] { InlineKeyboardButton.WithCallbackData(Constants.IK_ViewAll.text, Constants.IK_ViewAll.callback) },
            new [] { InlineKeyboardButton.WithCallbackData(Constants.IK_Edit.text, Constants.IK_Edit.callback) }
        }){};

        private readonly InlineKeyboardMarkup inlineKeyboardMarkup = new(new[]{
            new [] { InlineKeyboardButton.WithCallbackData(text: Constants.IK_ViewAll.text, Constants.IK_ViewAll.callback) }
        }){};

        private readonly InlineKeyboardMarkup inlineBackKeyboardMarkup = new(new[]{
            new [] { InlineKeyboardButton.WithCallbackData(Constants.IK_Back.text, Constants.IK_Back.callback) }
        }){};
        #endregion

        private async Task DefaultCallbackModeAsync(Message message, ITelegramBotClient botClient, TelegramUser user, CancellationToken cancellationToken, string data) {
            if(DateOnly.TryParse(message.Text?.Split('-')[0].Trim()[2..] ?? "", out DateOnly date)) {

                switch(data) {
                    case Constants.IK_Edit.callback:
                        await botClient.EditMessageTextAsync(chatId: message.Chat, messageId: message.MessageId, text: scheduler.GetScheduleByDate(date, true), replyMarkup: GetEditAdminInlineKeyboardButton(date));
                        break;

                    case Constants.IK_ViewAll.callback:
                        await botClient.EditMessageTextAsync(chatId: message.Chat, messageId: message.MessageId, text: scheduler.GetScheduleByDate(date, true), replyMarkup: inlineBackKeyboardMarkup);
                        break;

                    case Constants.IK_Back.callback:
                        await botClient.EditMessageTextAsync(chatId: message.Chat, messageId: message.MessageId, text: scheduler.GetScheduleByDate(date), replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
                        break;

                    case Constants.IK_Add.callback:
                        user.Mode = Mode.AddingDiscipline;
                        dbContext.TemporaryAddition.Add(new(user, date));
                        dbContext.SaveChanges();
                        await botClient.EditMessageTextAsync(chatId: message.Chat, messageId: message.MessageId, text: scheduler.GetScheduleByDate(date));
                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: GetStagesAddingDiscipline(user), replyMarkup: CancelKeyboardMarkup);
                        break;

                    default:
                        List<string> str = data.Split(' ').ToList() ?? new();
                        if(str.Count < 2) return;

                        var discipline = dbContext.Disciplines.FirstOrDefault(i => i.ID == int.Parse(str[1]));

                        if(discipline is not null) {
                            switch(str[0] ?? "") {
                                case "Day":
                                    discipline.IsCompleted = !discipline.IsCompleted;
                                    dbContext.SaveChanges();

                                    break;

                                case "Always":
                                    var completedDisciplines = dbContext.CompletedDisciplines.FirstOrDefault(i=> i.Name == discipline.Name && i.Lecturer == discipline.Lecturer && i.Class == discipline.Class && i.Subgroup == discipline.Subgroup);

                                    if(completedDisciplines is not null)
                                        dbContext.CompletedDisciplines.Remove(completedDisciplines);
                                    else
                                        dbContext.CompletedDisciplines.Add(discipline);

                                    dbContext.SaveChanges();
                                    Parser.SetDisciplineIsCompleted(dbContext.CompletedDisciplines.ToList(), dbContext.Disciplines);
                                    dbContext.SaveChanges();
                                    break;

                                case "Delete":
                                    dbContext.Disciplines.Remove(discipline);
                                    dbContext.SaveChanges();

                                    break;
                            }
                            await botClient.EditMessageReplyMarkupAsync(chatId: message.Chat, messageId: message.MessageId, replyMarkup: GetEditAdminInlineKeyboardButton(date));
                        }
                        break;
                }
            }

        }

        private InlineKeyboardMarkup GetEditAdminInlineKeyboardButton(DateOnly date) {
            var editButtons = new List<InlineKeyboardButton[]>();

            var completedDisciplinesList = dbContext.CompletedDisciplines.ToList();

            var disciplines = dbContext.Disciplines.Where(i => i.Date == date && !i.IsCastom).OrderBy(i => i.StartTime);
            if(disciplines.Any()) {
                editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: "В этот день", callbackData: "!"), InlineKeyboardButton.WithCallbackData(text: "Всегда", callbackData: "!") });

                foreach(var item in disciplines) {
                    var completedDisciplines = completedDisciplinesList.FirstOrDefault(i => i.Equals(item));

                    editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: $"{item.StartTime.ToString()} {item.Lecturer?.Split(' ')[0]} {(item.IsCompleted ? "❌" : "✅")}", callbackData: $"Day {item.ID}"),
                                            InlineKeyboardButton.WithCallbackData(text: completedDisciplines is not null ? "❌" : "✅", callbackData: $"Always {item.ID}")});
                }
            }

            var castom = dbContext.Disciplines.Where(i => i.Date == date && i.IsCastom).OrderBy(i => i.StartTime);
            if(castom.Any()) {
                editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: "Отображение", callbackData: "!"), InlineKeyboardButton.WithCallbackData(text: "Удалить", callbackData: "!") });

                foreach(var item in castom) {
                    var completedDisciplines = completedDisciplinesList.FirstOrDefault(i => i.Equals(item));

                    editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: $"{item.StartTime.ToString()} {item.Lecturer?.Split(' ')[0]} {(item.IsCompleted ? "❌" : "✅")}", callbackData: $"Day {item.ID}"),
                                            InlineKeyboardButton.WithCallbackData(text: "🗑", callbackData: $"Delete {item.ID}")});
                }
            }

            editButtons.AddRange(new[] { new[] { InlineKeyboardButton.WithCallbackData(Constants.IK_Add.text, Constants.IK_Add.callback) },
                                         new[] { InlineKeyboardButton.WithCallbackData(Constants.IK_Back.text, Constants.IK_Back.callback) }});

            return new InlineKeyboardMarkup(editButtons);
        }
    }
}