using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.Bot {
    public partial class TelegramBot {
        #region InlineKeyboardMarkup
        private readonly InlineKeyboardMarkup inlineAdminKeyboardMarkup = new(new[]{
             InlineKeyboardButton.WithCallbackData(Constants.IK_ViewAll.text, Constants.IK_ViewAll.callback),
             InlineKeyboardButton.WithCallbackData(Constants.IK_Edit.text, Constants.IK_Edit.callback)
        }){};

        private readonly InlineKeyboardMarkup inlineKeyboardMarkup = new(InlineKeyboardButton.WithCallbackData(text: Constants.IK_ViewAll.text, Constants.IK_ViewAll.callback)){};

        private readonly InlineKeyboardMarkup inlineBackKeyboardMarkup = new(InlineKeyboardButton.WithCallbackData(Constants.IK_Back.text, Constants.IK_Back.callback)){};
        #endregion

        private async Task DefaultCallbackModeAsync(Message message, ITelegramBotClient botClient, TelegramUser user, CancellationToken cancellationToken, string data) {
            bool IsAdmin = user.ScheduleProfile.OwnerID == user.ChatID;
            string? group = user.ScheduleProfile.Group;

            if(string.IsNullOrWhiteSpace(group)) return;

            if(DateOnly.TryParse(message.Text?.Split('-')[0].Trim()[2..] ?? "", out DateOnly date)) {

                switch(data) {
                    case Constants.IK_Edit.callback:
                        await botClient.EditMessageTextAsync(chatId: message.Chat, messageId: message.MessageId, text: scheduler.GetScheduleByDate(date, user.ScheduleProfile, true), replyMarkup: GetEditAdminInlineKeyboardButton(date, group, user.ScheduleProfileGuid));
                        break;

                    case Constants.IK_ViewAll.callback:
                        await botClient.EditMessageTextAsync(chatId: message.Chat, messageId: message.MessageId, text: scheduler.GetScheduleByDate(date, user.ScheduleProfile, true), replyMarkup: inlineBackKeyboardMarkup);
                        break;

                    case Constants.IK_Back.callback:
                        await botClient.EditMessageTextAsync(chatId: message.Chat, messageId: message.MessageId, text: scheduler.GetScheduleByDate(date, user.ScheduleProfile), replyMarkup: IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
                        break;

                    case Constants.IK_Add.callback:
                        try {
                            user.Mode = Mode.AddingDiscipline;
                            dbContext.TemporaryAddition.Add(new(user, date));
                            dbContext.SaveChanges();
                            await botClient.EditMessageTextAsync(chatId: message.Chat, messageId: message.MessageId, text: scheduler.GetScheduleByDate(date, user.ScheduleProfile));
                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: GetStagesAddingDiscipline(user), replyMarkup: CancelKeyboardMarkup);
                        } catch(Exception e) {

                            await Console.Out.WriteLineAsync(e.Message);
                        }

                        break;

                    default:
                        List<string> str = data.Split(' ').ToList() ?? new();
                        if(str.Count < 2) return;

                        if(str[0].Contains("Discipline")) {
                            var discipline = dbContext.Disciplines.FirstOrDefault(i => i.ID == uint.Parse(str[1]));

                            if(discipline is not null) {
                                switch(str[0] ?? "") {
                                    case "DisciplineDay":
                                        var dayCompletedDisciplines = dbContext.CompletedDisciplines.FirstOrDefault(i => i.ScheduleProfileGuid == user.ScheduleProfileGuid && i.Date == discipline.Date && i.Name == discipline.Name && i.Lecturer == discipline.Lecturer && i.Class == discipline.Class && i.Subgroup == discipline.Subgroup);

                                        if(dayCompletedDisciplines is not null)
                                            dbContext.CompletedDisciplines.Remove(dayCompletedDisciplines);
                                        else {
                                            dbContext.CompletedDisciplines.RemoveRange(dbContext.CompletedDisciplines.Where(i => i.ScheduleProfileGuid == user.ScheduleProfileGuid && i.Date == null && i.Name == discipline.Name && i.Lecturer == discipline.Lecturer && i.Class == discipline.Class && i.Subgroup == discipline.Subgroup));
                                            dbContext.CompletedDisciplines.Add(new(discipline, user.ScheduleProfileGuid));
                                        }

                                        dbContext.SaveChanges();

                                        break;

                                    case "DisciplineAlways":
                                        var completedDisciplines = dbContext.CompletedDisciplines.FirstOrDefault(i => i.ScheduleProfileGuid == user.ScheduleProfileGuid && i.Date == null && i.Name == discipline.Name && i.Lecturer == discipline.Lecturer && i.Class == discipline.Class && i.Subgroup == discipline.Subgroup);

                                        if(completedDisciplines is not null)
                                            dbContext.CompletedDisciplines.Remove(completedDisciplines);
                                        else {
                                            dbContext.CompletedDisciplines.RemoveRange(dbContext.CompletedDisciplines.Where(i => i.ScheduleProfileGuid == user.ScheduleProfileGuid && i.Date != null && i.Name == discipline.Name && i.Lecturer == discipline.Lecturer && i.Class == discipline.Class && i.Subgroup == discipline.Subgroup));
                                            dbContext.CompletedDisciplines.Add(new(discipline, user.ScheduleProfileGuid) { Date = null });
                                        }

                                        dbContext.SaveChanges();
                                        break;

                                }
                            }
                        } else if(str[0] == "Delete") {
                            var customDiscipline = dbContext.CustomDiscipline.FirstOrDefault(i => i.ID == uint.Parse(str[1]));

                            if(customDiscipline is not null) {
                                dbContext.CustomDiscipline.Remove(customDiscipline);
                                dbContext.SaveChanges();
                            }
                        }

                        await botClient.EditMessageReplyMarkupAsync(chatId: message.Chat, messageId: message.MessageId, replyMarkup: GetEditAdminInlineKeyboardButton(date, group, user.ScheduleProfileGuid));

                        break;
                }
            }

        }

        private InlineKeyboardMarkup GetEditAdminInlineKeyboardButton(DateOnly date, string group, Guid scheduleProfileGuid) {
            var editButtons = new List<InlineKeyboardButton[]>();

            var completedDiscipline = dbContext.CompletedDisciplines.Where(i => i.ScheduleProfileGuid == scheduleProfileGuid).ToList();

            var disciplines = dbContext.Disciplines.Where(i => i.Group == group && i.Date == date).OrderBy(i => i.StartTime);
            if(disciplines.Any()) {
                editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: "В этот день", callbackData: "!"), InlineKeyboardButton.WithCallbackData(text: "Всегда", callbackData: "!") });

                foreach(var item in disciplines) {
                    var always = completedDiscipline.FirstOrDefault(i => i.Equals(new(item, scheduleProfileGuid) { Date = null })) is not null;

                    editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: $"{item.StartTime.ToString()} {item.Lecturer?.Split(' ')[0]} {(always ? "🚫": (completedDiscipline.Contains(item) ? "❌" : "✅"))}", callbackData: $"{(always ? "!" : $"DisciplineDay {item.ID}")}"),
                                            InlineKeyboardButton.WithCallbackData(text: always ? "❌" : "✅", callbackData: $"DisciplineAlways {item.ID}")});
                }
            }

            var castom = dbContext.CustomDiscipline.Where(i => i.ScheduleProfileGuid == scheduleProfileGuid && i.Date == date).OrderBy(i => i.StartTime);
            if(castom.Any()) {
                editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: "Пользовательские", callbackData: "!") });

                foreach(var item in castom)
                    editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: $"{item.StartTime.ToString()} {item.Lecturer?.Split(' ')[0]} 🗑", callbackData: $"Delete {item.ID}") });
            }

            editButtons.AddRange(new[] { new[] { InlineKeyboardButton.WithCallbackData(Constants.IK_Add.text, Constants.IK_Add.callback) },
                                         new[] { InlineKeyboardButton.WithCallbackData(Constants.IK_Back.text, Constants.IK_Back.callback) }});

            return new InlineKeyboardMarkup(editButtons);
        }
    }
}