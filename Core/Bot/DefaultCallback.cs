using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.Bot {
    public partial class TelegramBot {
        private InlineKeyboardMarkup GetEditAdminInlineKeyboardButton(ScheduleDbContext dbContext, DateOnly date, ScheduleProfile scheduleProfile) {
            var editButtons = new List<InlineKeyboardButton[]>();

            var сompletedDisciplines = dbContext.CompletedDisciplines.Where(i => i.ScheduleProfileGuid == scheduleProfile.ID).ToList();

            IOrderedQueryable<Discipline> disciplines = dbContext.Disciplines.Where(i => i.Group == scheduleProfile.Group && i.Date == date).OrderBy(i => i.StartTime);
            if(disciplines.Any()) {
                editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: "В этот день", callbackData: "!"), InlineKeyboardButton.WithCallbackData(text: "Семестр", callbackData: "!") });

                foreach(Discipline? item in disciplines) {
                    CompletedDiscipline tmp = new(item, scheduleProfile.ID) { Date = null };
                    bool always = сompletedDisciplines.FirstOrDefault(i => i.Equals(tmp)) is not null;

                    editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: $"{item.StartTime} {item.Lecturer?.Split(' ')[0]} {(always ? "🚫" : (сompletedDisciplines.Contains((CompletedDiscipline)item) ? "❌" : "✅"))}", callbackData: $"{(always ? "!" : $"DisciplineDay {item.ID}|{item.Date}")}"),
                                            InlineKeyboardButton.WithCallbackData(text: always ? "❌" : "✅", callbackData: $"DisciplineAlways {item.ID}|{item.Date}")});
                }
            }

            IOrderedQueryable<CustomDiscipline> castom = dbContext.CustomDiscipline.Where(i => i.ScheduleProfileGuid == scheduleProfile.ID && i.Date == date).OrderBy(i => i.StartTime);
            if(castom.Any()) {
                editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: "Пользовательские", callbackData: "!") });

                foreach(CustomDiscipline? item in castom)
                    editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: $"{item.StartTime} {item.Lecturer?.Split(' ')[0]} 🔧", callbackData: $"CustomEdit {item.ID}|{item.Date}"),
                                            InlineKeyboardButton.WithCallbackData(text: $"🗑", callbackData: $"CustomDelete {item.ID}|{item.Date}"),});
            }

            editButtons.AddRange(new[] { new[] { InlineKeyboardButton.WithCallbackData(commands.Callback["Add"].text, $"{commands.Callback["Add"].callback} {date}") },
                                         new[] { InlineKeyboardButton.WithCallbackData(commands.Callback["Back"].text, $"{commands.Callback["Back"].callback} {date}") }});

            return new InlineKeyboardMarkup(editButtons);
        }

        private InlineKeyboardMarkup GetCustomEditAdminInlineKeyboardButton(CustomDiscipline customDiscipline) {
            var buttons = new List<InlineKeyboardButton[]> {
                new[] { InlineKeyboardButton.WithCallbackData($"Название: {customDiscipline.Name}", $"CustomEditName {customDiscipline.ID}|{customDiscipline.Date}") },
                new[] { InlineKeyboardButton.WithCallbackData($"Лектор: {customDiscipline.Lecturer}", $"CustomEditLecturer {customDiscipline.ID}|{customDiscipline.Date}") },
                new[] { InlineKeyboardButton.WithCallbackData($"Тип: {customDiscipline.Type}", $"CustomEditType {customDiscipline.ID}|{customDiscipline.Date}"),
                        InlineKeyboardButton.WithCallbackData($"Аудитория: {customDiscipline.LectureHall}", $"CustomEditLectureHall {customDiscipline.ID}|{customDiscipline.Date}") },
                new[] { InlineKeyboardButton.WithCallbackData($"Время начала: {customDiscipline.StartTime}", $"CustomEditStartTime {customDiscipline.ID}|{customDiscipline.Date}") ,
                        InlineKeyboardButton.WithCallbackData($"Время конца: {customDiscipline.EndTime}", $"CustomEditEndTime {customDiscipline.ID}|{customDiscipline.Date}") },

                new[] { InlineKeyboardButton.WithCallbackData(commands.Callback["CustomEditCancel"].text, $"{commands.Callback["CustomEditCancel"].callback} {customDiscipline.Date}") }
            };

            return new InlineKeyboardMarkup(buttons);
        }

        private InlineKeyboardMarkup GetInlineKeyboardButton(DateOnly date, TelegramUser user, bool all) {
            var editButtons = new List<InlineKeyboardButton[]>();

            if(user.IsOwner()) {
                if(all) {
                    editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: commands.Callback["All"].text, callbackData: $"{commands.Callback["All"].callback} {date}"),
                                            InlineKeyboardButton.WithCallbackData(text: commands.Callback["Edit"].text, callbackData: $"{commands.Callback["Edit"].callback} {date}") });
                } else {
                    editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: commands.Callback["Edit"].text, callbackData: $"{commands.Callback["Edit"].callback} {date}") });
                }
            } else if(all) {
                editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: commands.Callback["All"].text, callbackData: $"{commands.Callback["All"].callback} {date}") });
            }

            return new InlineKeyboardMarkup(editButtons);
        }

        private InlineKeyboardMarkup GetBackInlineKeyboardButton(DateOnly date, TelegramUser user) {
            return new(InlineKeyboardButton.WithCallbackData(commands.Callback["Back"].text, $"{commands.Callback["Back"].callback} {date}")); ;
        }

        private InlineKeyboardMarkup GetNotificationsInlineKeyboardButton(TelegramUser user) {
            var buttons = new List<InlineKeyboardButton[]>();

            if(user.Notifications.IsEnabled)
                buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("Выключить уведомления", "ToggleNotifications off") });
            else
                buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("Включить уведомления", "ToggleNotifications on") });

            static string via(int days) => days switch {
                1 => $"{days} день",
                2 or 3 or 4 => $"{days} дня",
                var _ when days > 4 => $"{days} дней",
                _ => "",
            };

            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData($"В период: {via(user.Notifications.Days)}", "DaysNotifications") });

            return new InlineKeyboardMarkup(buttons);
        }

        private static InlineKeyboardMarkup GetFeedbackInlineKeyboardButton(ScheduleDbContext dbContext, Feedback feedback) {
            bool previous = dbContext.Feedbacks.Any(i => !i.IsCompleted && i.ID < feedback.ID);
            bool next = dbContext.Feedbacks.Any(i => !i.IsCompleted && i.ID > feedback.ID); ;

            return new InlineKeyboardMarkup(new List<InlineKeyboardButton[]> {
                            new[] { InlineKeyboardButton.WithCallbackData(text: previous ? "⬅️":"❌", callbackData: $"FeedbackPrevious {feedback.ID}"),
                                    InlineKeyboardButton.WithCallbackData(text: "✅", callbackData: $"FeedbackAccept {feedback.ID}"),
                                    InlineKeyboardButton.WithCallbackData(text: next ? "➡️":"❌", callbackData: $"FeedbackNext {feedback.ID}") }
                        });
        }
    }
}