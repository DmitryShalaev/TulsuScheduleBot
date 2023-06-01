using ScheduleBot.DB.Entity;

using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.Bot {
    public partial class TelegramBot {
        private ReplyKeyboardMarkup GetTermsKeyboardMarkup(string StudentID) {
            List<KeyboardButton[]> TermsKeyboardMarkup = new();

            var terms = dbContext.Progresses.Where(i => i.StudentID == StudentID && i.Mark != null).Select(i => i.Term).Distinct().OrderBy(i => i).ToArray();
            for(int i = 0; i < terms.Length; i++)
                TermsKeyboardMarkup.Add(new KeyboardButton[] { $"{terms[i]} {commands.Message["Semester"]}", i + 1 < terms.Length ? $"{terms[++i]} {commands.Message["Semester"]}" : "" });

            TermsKeyboardMarkup.Add(new KeyboardButton[] { commands.Message["Back"] });

            return new(TermsKeyboardMarkup) { ResizeKeyboard = true };
        }

        private ReplyKeyboardMarkup GetProfileKeyboardMarkup(TelegramUser user) {
            List<KeyboardButton[]> ProfileKeyboardMarkup = new();

            if(user.ScheduleProfile.OwnerID == user.ChatID) {
                ProfileKeyboardMarkup.AddRange(new[] {  new KeyboardButton[] { $"{commands.Message["GroupNumber"]}: {user.ScheduleProfile.Group}" },
                                                        new KeyboardButton[] { $"{commands.Message["StudentIDNumber"]}: {user.ScheduleProfile.StudentID}" },
                                                        new KeyboardButton[] { commands.Message["GetProfileLink"]}
                                                     });
            } else {
                ProfileKeyboardMarkup.Add(new KeyboardButton[] { commands.Message["ResetProfileLink"] });
            }

            ProfileKeyboardMarkup.Add(new KeyboardButton[] { commands.Message["Back"] });

            return new(ProfileKeyboardMarkup) { ResizeKeyboard = true };
        }

        private static ReplyKeyboardMarkup GetCorpsKeyboardMarkup() {
            List<KeyboardButton[]> ProfileKeyboardMarkup = new();
            ProfileKeyboardMarkup.Add(new KeyboardButton[] { commands.Corps[0].text });

            for(int i = 0; i < 3; i++) {
                List<KeyboardButton> keyboardButtonsLine = new();
                for(int j = 0; j < 5; j++)
                    keyboardButtonsLine.Add(commands.Corps[1 + i * 5 + j].text);

                ProfileKeyboardMarkup.Add(keyboardButtonsLine.ToArray());
            }

            for(int i = 16; i < commands.Corps.Length; i++)
                ProfileKeyboardMarkup.Add(new KeyboardButton[] { commands.Corps[i].text });

            ProfileKeyboardMarkup.AddRange(new[] { new KeyboardButton[] { commands.College.text }, new KeyboardButton[] { commands.Message["Back"] } });

            return new(ProfileKeyboardMarkup) { ResizeKeyboard = true };
        }
    }
}