using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types.ReplyMarkups;

namespace Core.Bot.Commands {
    public static class DefaultCallback {
        public static InlineKeyboardMarkup GetEditAdminInlineKeyboardButton(ScheduleDbContext dbContext, DateOnly date, ScheduleProfile scheduleProfile) {
            var editButtons = new List<InlineKeyboardButton[]>();

            var сompletedDisciplines = dbContext.CompletedDisciplines.Where(i => i.ScheduleProfileGuid == scheduleProfile.ID).ToList();

            IOrderedQueryable<Discipline> disciplines = dbContext.Disciplines.Where(i => i.Group == scheduleProfile.Group && i.Date == date).OrderBy(i => i.StartTime);
            if(disciplines.Any()) {
                editButtons.Add([InlineKeyboardButton.WithCallbackData(text: "В этот день", callbackData: "!"), InlineKeyboardButton.WithCallbackData(text: "Семестр", callbackData: "!")]);

                foreach(Discipline? item in disciplines) {
                    CompletedDiscipline tmp = new(item, scheduleProfile.ID) { Date = null };
                    bool always = сompletedDisciplines.FirstOrDefault(i => i.Equals(tmp)) is not null;

                    editButtons.Add([ InlineKeyboardButton.WithCallbackData(text: $"{item.StartTime} {item.Lecturer?.Split(' ')[0]} {(always ? "🚫" : сompletedDisciplines.Contains((CompletedDiscipline)item) ? "❌" : "✅")}", callbackData: $"{(always ? "!" : $"DisciplineDay {item.ID}|{item.Date}")}"),
                                            InlineKeyboardButton.WithCallbackData(text: always ? "❌" : "✅", callbackData: $"DisciplineAlways {item.ID}|{item.Date}")]);
                }
            }

            IOrderedQueryable<CustomDiscipline> castom = dbContext.CustomDiscipline.Where(i => i.ScheduleProfileGuid == scheduleProfile.ID && i.Date == date).OrderBy(i => i.StartTime);
            if(castom.Any()) {
                editButtons.Add([InlineKeyboardButton.WithCallbackData(text: "Пользовательские", callbackData: "!")]);

                foreach(CustomDiscipline? item in castom)
                    editButtons.Add([ InlineKeyboardButton.WithCallbackData(text: $"{item.StartTime} {item.Lecturer?.Split(' ')[0]} 🔧", callbackData: $"CustomEdit {item.ID}|{item.Date}"),
                                            InlineKeyboardButton.WithCallbackData(text: $"🗑", callbackData: $"CustomDelete {item.ID}|{item.Date}"),]);
            }

            editButtons.AddRange([ [InlineKeyboardButton.WithCallbackData(UserCommands.Instance.Callback["Add"].text, $"{UserCommands.Instance.Callback["Add"].callback} {date}")],
                                         [InlineKeyboardButton.WithCallbackData(UserCommands.Instance.Callback["Back"].text, $"{UserCommands.Instance.Callback["Back"].callback} {date}")]]);

            return new InlineKeyboardMarkup(editButtons);
        }

        public static InlineKeyboardMarkup GetCustomEditAdminInlineKeyboardButton(CustomDiscipline customDiscipline) {
            var buttons = new List<InlineKeyboardButton[]> {
                new[] { InlineKeyboardButton.WithCallbackData($"Название: {customDiscipline.Name}", $"CustomEditName {customDiscipline.ID}|{customDiscipline.Date}") },
                new[] { InlineKeyboardButton.WithCallbackData($"Лектор: {customDiscipline.Lecturer}", $"CustomEditLecturer {customDiscipline.ID}|{customDiscipline.Date}") },
                new[] { InlineKeyboardButton.WithCallbackData($"Тип: {customDiscipline.Type}", $"CustomEditType {customDiscipline.ID}|{customDiscipline.Date}"),
                        InlineKeyboardButton.WithCallbackData($"Аудитория: {customDiscipline.LectureHall}", $"CustomEditLectureHall {customDiscipline.ID}|{customDiscipline.Date}") },
                new[] { InlineKeyboardButton.WithCallbackData($"Время начала: {customDiscipline.StartTime}", $"CustomEditStartTime {customDiscipline.ID}|{customDiscipline.Date}") ,
                        InlineKeyboardButton.WithCallbackData($"Время конца: {customDiscipline.EndTime}", $"CustomEditEndTime {customDiscipline.ID}|{customDiscipline.Date}") },

                new[] { InlineKeyboardButton.WithCallbackData(UserCommands.Instance.Callback["CustomEditCancel"].text, $"{UserCommands.Instance.Callback["CustomEditCancel"].callback} {customDiscipline.Date}") }
            };

            return new InlineKeyboardMarkup(buttons);
        }

        public static InlineKeyboardMarkup GetInlineKeyboardButton(DateOnly date, TelegramUser user, bool notAll) {
            var editButtons = new List<InlineKeyboardButton[]>();

            if(user.IsOwner() && !user.IsSupergroup()) {
                if(notAll) {
                    editButtons.Add([ InlineKeyboardButton.WithCallbackData(text: UserCommands.Instance.Callback["All"].text, callbackData: $"{UserCommands.Instance.Callback["All"].callback} {date}"),
                                      InlineKeyboardButton.WithCallbackData(text: UserCommands.Instance.Callback["Edit"].text, callbackData: $"{UserCommands.Instance.Callback["Edit"].callback} {date}") ]);
                } else {
                    editButtons.Add([InlineKeyboardButton.WithCallbackData(text: UserCommands.Instance.Callback["Edit"].text, callbackData: $"{UserCommands.Instance.Callback["Edit"].callback} {date}")]);
                }
            } else if(notAll) {
                editButtons.Add([InlineKeyboardButton.WithCallbackData(text: UserCommands.Instance.Callback["All"].text, callbackData: $"{UserCommands.Instance.Callback["All"].callback} {date}")]);
            }

            return new InlineKeyboardMarkup(editButtons);
        }

        public static InlineKeyboardMarkup GetBackInlineKeyboardButton(DateOnly date) {
            return new(InlineKeyboardButton.WithCallbackData(UserCommands.Instance.Callback["Back"].text, $"{UserCommands.Instance.Callback["Back"].callback} {date}")); ;
        }

        public static InlineKeyboardMarkup GetNotificationsInlineKeyboardButton(TelegramUser user) {
            var buttons = new List<InlineKeyboardButton[]>();

            bool _notificationEnabled = user.Settings.NotificationEnabled;
            string notificationEnabled = _notificationEnabled ? "\U0001f7e2" : "🔴";

            buttons.Add([InlineKeyboardButton.WithCallbackData($"{notificationEnabled} Уведомления {notificationEnabled} \n({(_notificationEnabled ? "Выключить" : "Включить")})", $"ToggleNotifications {(_notificationEnabled ? "off" : "on")}")]);

            static string via(int days) => days switch {
                1 => $"{days} день",
                2 or 3 or 4 => $"{days} дня",
                var _ when days > 4 => $"{days} дней",
                _ => "",
            };

            buttons.Add([InlineKeyboardButton.WithCallbackData($"В период: {via(user.Settings.NotificationDays)}", "DaysNotifications")]);

            return new InlineKeyboardMarkup(buttons);
        }
    }
}