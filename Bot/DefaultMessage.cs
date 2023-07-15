using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.Bot
{
    public partial class TelegramBot
    {
        private ReplyKeyboardMarkup GetTermsKeyboardMarkup(ScheduleDbContext dbContext, string StudentID)
        {
            List<KeyboardButton[]> TermsKeyboardMarkup = new();

            int[] terms = dbContext.Progresses.Where(i => i.StudentID == StudentID).Select(i => i.Term).Distinct().OrderBy(i => i).ToArray();
            for (int i = 0; i < terms.Length; i++)
                TermsKeyboardMarkup.Add(new KeyboardButton[] { $"{terms[i]} {commands.Message["Semester"]}", i + 1 < terms.Length ? $"{terms[++i]} {commands.Message["Semester"]}" : "" });

            TermsKeyboardMarkup.Add(new KeyboardButton[] { commands.Message["Back"] });

            return new(TermsKeyboardMarkup) { ResizeKeyboard = true };
        }

        private ReplyKeyboardMarkup GetProfileKeyboardMarkup(TelegramUser user)
        {
            List<KeyboardButton[]> ProfileKeyboardMarkup = new();

            if (user.IsOwner())
            {
                ProfileKeyboardMarkup.AddRange(new[] {  new KeyboardButton[] { $"{commands.Message["GroupNumber"]}:\n{user.ScheduleProfile.Group}", $"{commands.Message["StudentIDNumber"]}:\n{user.ScheduleProfile.StudentID}" },
                                                        new KeyboardButton[] { commands.Message["GetProfileLink"] }
                                                     });
            }
            else
            {
                ProfileKeyboardMarkup.Add(new KeyboardButton[] { commands.Message["ResetProfileLink"] });
            }

            ProfileKeyboardMarkup.AddRange(new[] {
                new KeyboardButton[] { commands.Message["Notifications"] },
                new KeyboardButton[] { commands.Message["Back"] }
            });

            return new(ProfileKeyboardMarkup) { ResizeKeyboard = true };
        }

        private static ReplyKeyboardMarkup GetCorpsKeyboardMarkup()
        {
            List<KeyboardButton[]> ProfileKeyboardMarkup = new() {
                new KeyboardButton[] { commands.Corps[0].text }
            };

            for (int i = 0; i < 3; i++)
            {
                List<KeyboardButton> keyboardButtonsLine = new();
                for (int j = 0; j < 5; j++)
                    keyboardButtonsLine.Add(commands.Corps[1 + i * 5 + j].text);

                ProfileKeyboardMarkup.Add(keyboardButtonsLine.ToArray());
            }

            for (int i = 16; i < commands.Corps.Length; i++)
                ProfileKeyboardMarkup.Add(new KeyboardButton[] { commands.Corps[i].text });

            ProfileKeyboardMarkup.AddRange(new[] { new KeyboardButton[] { commands.College.text }, new KeyboardButton[] { commands.Message["Back"] } });

            return new(ProfileKeyboardMarkup) { ResizeKeyboard = true };
        }
    }
}