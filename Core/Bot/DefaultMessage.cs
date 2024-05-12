using Core.Bot.Commands;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot.Types.ReplyMarkups;

namespace Core.Bot {
    public static class DefaultMessage {
        public static ReplyKeyboardMarkup GetTermsKeyboardMarkup(ScheduleDbContext dbContext, string StudentID) {
            List<KeyboardButton[]> TermsKeyboardMarkup = [];

            int[] terms = [.. dbContext.Progresses.Where(i => i.StudentID == StudentID).Select(i => i.Term).Distinct().OrderBy(i => i)];
            for(int i = 0; i < terms.Length; i++)
                TermsKeyboardMarkup.Add([$"{terms[i]} {UserCommands.Instance.Message["Semester"]}", i + 1 < terms.Length ? $"{terms[++i]} {UserCommands.Instance.Message["Semester"]}" : ""]);

            TermsKeyboardMarkup.Add([UserCommands.Instance.Message["Back"]]);

            return new(TermsKeyboardMarkup) { ResizeKeyboard = true };
        }

        public static ReplyKeyboardMarkup GetProfileKeyboardMarkup(TelegramUser user) {
            List<KeyboardButton[]> ProfileKeyboardMarkup = [];

            if(user.IsOwner()) {
                ProfileKeyboardMarkup.AddRange([  [$"{UserCommands.Instance.Message["GroupNumber"]}:\n{user.ScheduleProfile.Group}", $"{UserCommands.Instance.Message["StudentIDNumber"]}:\n{user.ScheduleProfile.StudentID}"],
                                                        [UserCommands.Instance.Message["GetProfileLink"]]
                                                     ]);
            } else {
                ProfileKeyboardMarkup.Add([UserCommands.Instance.Message["ResetProfileLink"]]);
            }

            ProfileKeyboardMarkup.AddRange([
                [UserCommands.Instance.Message["Settings"]],
                [UserCommands.Instance.Message["Back"]]
            ]);

            return new(ProfileKeyboardMarkup) { ResizeKeyboard = true };
        }

        public static ReplyKeyboardMarkup GetSettingsKeyboardMarkup(TelegramUser user) {
            List<KeyboardButton[]> ProfileKeyboardMarkup = new([
                [UserCommands.Instance.Message["Notifications"]],
                [$"{UserCommands.Instance.Message["TeacherLincsEnabled"]}: {(user.Settings.TeacherLincsEnabled ? "вкл" : "выкл")}"],
                [UserCommands.Instance.Message["Back"]]
            ]);

            return new(ProfileKeyboardMarkup) { ResizeKeyboard = true };
        }

        public static ReplyKeyboardMarkup GetCorpsKeyboardMarkup() {
            List<KeyboardButton[]> ProfileKeyboardMarkup = [
                [UserCommands.Instance.Corps[0].text]
            ];

            for(int i = 0; i < 3; i++) {
                List<KeyboardButton> keyboardButtonsLine = [];
                for(int j = 0; j < 5; j++)
                    keyboardButtonsLine.Add(UserCommands.Instance.Corps[1 + i * 5 + j].text);

                ProfileKeyboardMarkup.Add([.. keyboardButtonsLine]);
            }

            for(int i = 16; i < UserCommands.Instance.Corps.Length; i++)
                ProfileKeyboardMarkup.Add([UserCommands.Instance.Corps[i].text]);

            ProfileKeyboardMarkup.AddRange([[UserCommands.Instance.College.text], [UserCommands.Instance.Message["Back"]]]);

            return new(ProfileKeyboardMarkup) { ResizeKeyboard = true };
        }

        public static ReplyKeyboardMarkup GetTeacherWorkScheduleSelectedKeyboardMarkup(string teacher) {
            List<KeyboardButton[]> KeyboardMarkup = [
                [$"{UserCommands.Instance.Message["CurrentTeacher"]}:\n{teacher}"],
                [UserCommands.Instance.Message["Today"], UserCommands.Instance.Message["Tomorrow"]],
                [UserCommands.Instance.Message["ByDays"], UserCommands.Instance.Message["ForAWeek"]],
                [UserCommands.Instance.Message["TeachersWorkScheduleBack"]]
            ];

            return new(KeyboardMarkup) { ResizeKeyboard = true };
        }
    }
}