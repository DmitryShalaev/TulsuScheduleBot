using Core.Bot.Commands;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot.Types.ReplyMarkups;

namespace Core.Bot {
    public static class DefaultMessage {
        public static ReplyKeyboardMarkup GetTermsKeyboardMarkup(ScheduleDbContext dbContext, string StudentID) {
            List<KeyboardButton[]> TermsKeyboardMarkup = new();

            int[] terms = dbContext.Progresses.Where(i => i.StudentID == StudentID).Select(i => i.Term).Distinct().OrderBy(i => i).ToArray();
            for(int i = 0; i < terms.Length; i++)
                TermsKeyboardMarkup.Add(new KeyboardButton[] { $"{terms[i]} {UserCommands.Instance.Message["Semester"]}", i + 1 < terms.Length ? $"{terms[++i]} {UserCommands.Instance.Message["Semester"]}" : "" });

            TermsKeyboardMarkup.Add(new KeyboardButton[] { UserCommands.Instance.Message["Back"] });

            return new(TermsKeyboardMarkup) { ResizeKeyboard = true };
        }

        public static ReplyKeyboardMarkup GetProfileKeyboardMarkup(TelegramUser user) {
            List<KeyboardButton[]> ProfileKeyboardMarkup = new();

            if(user.IsOwner()) {
                ProfileKeyboardMarkup.AddRange(new[] {  new KeyboardButton[] { $"{UserCommands.Instance.Message["GroupNumber"]}:\n{user.ScheduleProfile.Group}", $"{UserCommands.Instance.Message["StudentIDNumber"]}:\n{user.ScheduleProfile.StudentID}" },
                                                        new KeyboardButton[] { UserCommands.Instance.Message["GetProfileLink"] }
                                                     });
            } else {
                ProfileKeyboardMarkup.Add(new KeyboardButton[] { UserCommands.Instance.Message["ResetProfileLink"] });
            }

            ProfileKeyboardMarkup.AddRange(new[] {
                new KeyboardButton[] { UserCommands.Instance.Message["Settings"] },
                new KeyboardButton[] { UserCommands.Instance.Message["Back"] }
            });

            return new(ProfileKeyboardMarkup) { ResizeKeyboard = true };
        }

        public static ReplyKeyboardMarkup GetSettingsKeyboardMarkup(TelegramUser user) {
            List<KeyboardButton[]> ProfileKeyboardMarkup = new(new[] {
                new KeyboardButton[] { UserCommands.Instance.Message["Notifications"] },
                new KeyboardButton[] { $"{UserCommands.Instance.Message["TeacherLincsEnabled"]}: {(user.Settings.TeacherLincsEnabled ? "вкл" : "выкл")}" },
                new KeyboardButton[] { UserCommands.Instance.Message["Back"] }
            });

            return new(ProfileKeyboardMarkup) { ResizeKeyboard = true };
        }

        public static ReplyKeyboardMarkup GetCorpsKeyboardMarkup() {
            List<KeyboardButton[]> ProfileKeyboardMarkup = new() {
                new KeyboardButton[] { UserCommands.Instance.Corps[0].text }
            };

            for(int i = 0; i < 3; i++) {
                List<KeyboardButton> keyboardButtonsLine = new();
                for(int j = 0; j < 5; j++)
                    keyboardButtonsLine.Add(UserCommands.Instance.Corps[1 + i * 5 + j].text);

                ProfileKeyboardMarkup.Add(keyboardButtonsLine.ToArray());
            }

            for(int i = 16; i < UserCommands.Instance.Corps.Length; i++)
                ProfileKeyboardMarkup.Add(new KeyboardButton[] { UserCommands.Instance.Corps[i].text });

            ProfileKeyboardMarkup.AddRange(new[] { new KeyboardButton[] { UserCommands.Instance.College.text }, new KeyboardButton[] { UserCommands.Instance.Message["Back"] } });

            return new(ProfileKeyboardMarkup) { ResizeKeyboard = true };
        }

        public static ReplyKeyboardMarkup GetTeacherWorkScheduleSelectedKeyboardMarkup(string teacher) {
            List<KeyboardButton[]> KeyboardMarkup = new() {
                new KeyboardButton[] { $"{UserCommands.Instance.Message["CurrentTeacher"]}:\n{teacher}" },
                new KeyboardButton[] { UserCommands.Instance.Message["Today"], UserCommands.Instance.Message["Tomorrow"] },
                new KeyboardButton[] { UserCommands.Instance.Message["ByDays"], UserCommands.Instance.Message["ForAWeek"] },
                new KeyboardButton[] { UserCommands.Instance.Message["TeachersWorkScheduleBack"] }
            };

            return new(KeyboardMarkup) { ResizeKeyboard = true };
        }
    }
}