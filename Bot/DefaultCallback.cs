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

                    editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: $"{item.StartTime.ToString()} {item.Lecturer?.Split(' ')[0]} {(always ? "🚫" : (сompletedDisciplines.Contains(item) ? "❌" : "✅"))}", callbackData: $"{(always ? "!" : $"DisciplineDay {item.ID}")}"),
                                            InlineKeyboardButton.WithCallbackData(text: always ? "❌" : "✅", callbackData: $"DisciplineAlways {item.ID}")});
                }
            }

            var castom = dbContext.CustomDiscipline.Where(i => i.ScheduleProfileGuid == scheduleProfile.ID && i.Date == date).OrderBy(i => i.StartTime);
            if(castom.Any()) {
                editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: "Пользовательские", callbackData: "!") });

                foreach(var item in castom)
                    editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: $"{item.StartTime.ToString()} {item.Lecturer?.Split(' ')[0]} 🗑", callbackData: $"Delete {item.ID}") });
            }

            editButtons.AddRange(new[] { new[] { InlineKeyboardButton.WithCallbackData(Constants.IK_Add.text, $"{Constants.IK_Add.callback} {date}") },
                                         new[] { InlineKeyboardButton.WithCallbackData(Constants.IK_Back.text, $"{Constants.IK_Back.callback} {date}") }});

            return new InlineKeyboardMarkup(editButtons);
        }

        private InlineKeyboardMarkup GetInlineKeyboardButton(DateOnly date, TelegramUser user) {
            var editButtons = new List<InlineKeyboardButton[]>();

            if(user.IsAdmin())
                editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: Constants.IK_ViewAll.text, callbackData: $"{Constants.IK_ViewAll.callback} {date}"),
                                    InlineKeyboardButton.WithCallbackData(text: Constants.IK_Edit.text, callbackData: $"{Constants.IK_Edit.callback} {date}") });
            else
                editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: Constants.IK_ViewAll.text, callbackData: $"{Constants.IK_ViewAll.callback} {date}") });

            return new InlineKeyboardMarkup(editButtons);
        }

        private InlineKeyboardMarkup GetInlineBackKeyboardButton(DateOnly date, TelegramUser user) {
            return new(InlineKeyboardButton.WithCallbackData(Constants.IK_Back.text, $"{Constants.IK_Back.callback} {date}")); ;
        }

    }
}