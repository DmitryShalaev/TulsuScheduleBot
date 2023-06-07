using ScheduleBot.DB.Entity;

using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.Bot {
    public partial class TelegramBot {
        private InlineKeyboardMarkup GetEditAdminInlineKeyboardButton(DateOnly date, ScheduleProfile scheduleProfile) {
            var editButtons = new List<InlineKeyboardButton[]>();

            var сompletedDisciplines = dbContext.CompletedDisciplines.Where(i => i.ScheduleProfileGuid == scheduleProfile.ID).ToList();

            var disciplines = dbContext.Disciplines.Where(i => i.Group == scheduleProfile.Group && i.Date == date).OrderBy(i => i.StartTime);
            if(disciplines.Any()) {
                editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: "В этот день", callbackData: "!"), InlineKeyboardButton.WithCallbackData(text: "Всегда", callbackData: "!") });

                foreach(var item in disciplines) {
                    CompletedDiscipline tmp = new(item, scheduleProfile.ID) { Date = null };
                    var always = сompletedDisciplines.FirstOrDefault(i => i.Equals(tmp)) is not null;

                    editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: $"{item.StartTime.ToString()} {item.Lecturer?.Split(' ')[0]} {(always ? "🚫" : (сompletedDisciplines.Contains((CompletedDiscipline)item) ? "❌" : "✅"))}", callbackData: $"{(always ? "!" : $"DisciplineDay {item.ID}|{item.Date}")}"),
                                            InlineKeyboardButton.WithCallbackData(text: always ? "❌" : "✅", callbackData: $"DisciplineAlways {item.ID}|{item.Date}")});
                }
            }

            var castom = dbContext.CustomDiscipline.Where(i => i.ScheduleProfileGuid == scheduleProfile.ID && i.Date == date).OrderBy(i => i.StartTime);
            if(castom.Any()) {
                editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: "Пользовательские", callbackData: "!") });

                foreach(var item in castom)
                    editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: $"{item.StartTime.ToString()} {item.Lecturer?.Split(' ')[0]} 🔧", callbackData: $"CustomEdit {item.ID}|{item.Date}"),
                                            InlineKeyboardButton.WithCallbackData(text: $"🗑", callbackData: $"CustomDelete {item.ID}|{item.Date}"),});
            }

            editButtons.AddRange(new[] { new[] { InlineKeyboardButton.WithCallbackData(commands.Callback["Add"].text, $"{commands.Callback["Add"].callback} {date}") },
                                         new[] { InlineKeyboardButton.WithCallbackData(commands.Callback["Back"].text, $"{commands.Callback["Back"].callback} {date}") }});

            return new InlineKeyboardMarkup(editButtons);
        }

        private InlineKeyboardMarkup GetCustomEditAdminInlineKeyboardButton(CustomDiscipline customDiscipline) {
            var buttons = new List<InlineKeyboardButton[]>();

            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData($"Название: {customDiscipline.Name}", $"CustomEditName {customDiscipline.ID}|{customDiscipline.Date}") });
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData($"Лектор: {customDiscipline.Lecturer}", $"CustomEditLecturer {customDiscipline.ID}|{customDiscipline.Date}") });
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData($"Тип: {customDiscipline.Type}", $"CustomEditType {customDiscipline.ID}|{customDiscipline.Date}"),
                                InlineKeyboardButton.WithCallbackData($"Аудитория: {customDiscipline.LectureHall}", $"CustomEditLectureHall {customDiscipline.ID}|{customDiscipline.Date}") });
            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData($"Время начала: {customDiscipline.StartTime}", $"CustomEditStartTime {customDiscipline.ID}|{customDiscipline.Date}") ,
                                InlineKeyboardButton.WithCallbackData($"Время конца: {customDiscipline.EndTime}", $"CustomEditEndTime {customDiscipline.ID}|{customDiscipline.Date}") });

            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData(commands.Callback["CustomEditCancel"].text, $"{commands.Callback["CustomEditCancel"].callback} {customDiscipline.Date}") });

            return new InlineKeyboardMarkup(buttons);
        }

        private InlineKeyboardMarkup GetInlineKeyboardButton(DateOnly date, TelegramUser user) {
            var editButtons = new List<InlineKeyboardButton[]>();

            if(user.IsAdmin())
                editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: commands.Callback["All"].text, callbackData: $"{commands.Callback["All"].callback} {date}"),
                                    InlineKeyboardButton.WithCallbackData(text: commands.Callback["Edit"].text, callbackData: $"{commands.Callback["Edit"].callback} {date}") });
            else
                editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: commands.Callback["All"].text, callbackData: $"{commands.Callback["All"].callback} {date}") });

            return new InlineKeyboardMarkup(editButtons);
        }

        private InlineKeyboardMarkup GetInlineBackKeyboardButton(DateOnly date, TelegramUser user) {
            return new(InlineKeyboardButton.WithCallbackData(commands.Callback["Back"].text, $"{commands.Callback["Back"].callback} {date}")); ;
        }

    }
}