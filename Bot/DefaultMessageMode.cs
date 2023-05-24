using System.Globalization;
using System.Text.RegularExpressions;

using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.Bot {
    public partial class TelegramBot {
        [GeneratedRegex("^\\d{1,2}[ ,./-](\\d{1,2}|\\w{3,8})([ ,./-](\\d{2}|\\d{4}))?$")]
        private static partial Regex DateRegex();

        #region ReplyKeyboardMarkup
        private readonly ReplyKeyboardMarkup MainKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { Constants.RK_Today, Constants.RK_Tomorrow },
                            new KeyboardButton[] { Constants.RK_ByDays, Constants.RK_ForAWeek },
                            new KeyboardButton[] { Constants.RK_Profile }
                        })
        { ResizeKeyboard = true };

        private readonly ReplyKeyboardMarkup DaysKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { Constants.RK_Monday, Constants.RK_Tuesday },
                            new KeyboardButton[] { Constants.RK_Wednesday, Constants.RK_Thursday },
                            new KeyboardButton[] { Constants.RK_Friday, Constants.RK_Saturday },
                            new KeyboardButton[] { Constants.RK_Back }
                        })
        { ResizeKeyboard = true };

        private readonly ReplyKeyboardMarkup CancelKeyboardMarkup = new(Constants.RK_Cancel)
        { ResizeKeyboard = true };

        private readonly ReplyKeyboardMarkup WeekKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { Constants.RK_ThisWeek, Constants.RK_NextWeek },
                            new KeyboardButton[] { Constants.RK_Back }
                        })
        { ResizeKeyboard = true };
        #endregion

        private async Task DefaultMessageModeAsync(Message message, ITelegramBotClient botClient, TelegramUser user, CancellationToken cancellationToken) {
            bool IsAdmin = user.ScheduleProfile.OwnerID == user.ChatID;
            string? group = user.ScheduleProfile.Group;
            string? studentID = user.ScheduleProfile.StudentID;

            switch(message.Text) {
                case "/start":
                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: "👋", replyMarkup: MainKeyboardMarkup);

                    if(string.IsNullOrWhiteSpace(user.ScheduleProfile.Group)) {
                        user.Mode = Mode.GroupСhange;
                        dbContext.SaveChanges();

                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Для начала работы с ботом необходимо указать номер учебной группы", replyMarkup: CancelKeyboardMarkup);
                    }
                    break;

                case Constants.RK_Back:
                case Constants.RK_Cancel:
                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Основное меню", replyMarkup: MainKeyboardMarkup);
                    break;

                case Constants.RK_Today:
                case Constants.RK_Tomorrow:
                    await ScheduleRelevance(message.Chat, botClient, MainKeyboardMarkup);
                    await TodayAndTomorrow(botClient, message.Chat, message.Text, IsAdmin, group, user.ScheduleProfileGuid);
                    break;

                case Constants.RK_ByDays:
                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: Constants.RK_ByDays, replyMarkup: DaysKeyboardMarkup);
                    break;

                case Constants.RK_Monday:
                case Constants.RK_Tuesday:
                case Constants.RK_Wednesday:
                case Constants.RK_Thursday:
                case Constants.RK_Friday:
                case Constants.RK_Saturday:
                    await ScheduleRelevance(message.Chat, botClient, DaysKeyboardMarkup);
                    await DayOfWeek(botClient, message.Chat, message.Text, IsAdmin, group, user.ScheduleProfileGuid);
                    break;

                case Constants.RK_ForAWeek:
                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: Constants.RK_ForAWeek, replyMarkup: WeekKeyboardMarkup);
                    break;

                case Constants.RK_ThisWeek:
                case Constants.RK_NextWeek:
                    await ScheduleRelevance(message.Chat, botClient, WeekKeyboardMarkup);
                    await Weeks(botClient, message.Chat, message.Text, IsAdmin, group, user.ScheduleProfileGuid);
                    break;

                case Constants.RK_AcademicPerformance:
                    if(!string.IsNullOrEmpty(user.ScheduleProfile.StudentID))
                        await ProgressRelevance(message.Chat, botClient, GetTermsKeyboardMarkup(user.ScheduleProfile.StudentID));
                    else
                        await StudentIdError(message, botClient);

                    break;

                case Constants.RK_Profile:
                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Профиль", replyMarkup: GetProfileKeyboardMarkup(user));
                    break;

                default:
                    if(message.Text?.Contains(Constants.RK_Semester) ?? false) {
                        if(!string.IsNullOrEmpty(studentID))
                            await AcademicPerformancePerSemester(botClient, message.Chat, message.Text, studentID);

                        return;
                    }

                    if(user.ScheduleProfile.OwnerID == user.ChatID) {
                        if(message.Text?.Contains("Номер группы") ?? false) {
                            user.Mode = Mode.GroupСhange;
                            dbContext.SaveChanges();

                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Хотите сменить номер учебной группы? Если да, то напишите новый номер", replyMarkup: CancelKeyboardMarkup);
                            return;
                        }

                        if(message.Text?.Contains("Номер зачётки") ?? false) {
                            user.Mode = Mode.StudentIDСhange;
                            dbContext.SaveChanges();

                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Хотите сменить номер зачётки? Если да, то напишите новый номер", replyMarkup: CancelKeyboardMarkup);
                            return;
                        }
                    }

                    if(message.Text != null)
                        await GetScheduleByDate(botClient, message.Chat, message.Text, IsAdmin, group, user.ScheduleProfileGuid);

                    break;
            }
        }

        private async Task GetScheduleByDate(ITelegramBotClient botClient, ChatId chatId, string text, bool IsAdmin, string? group, Guid scheduleProfileGuid) {
            if(string.IsNullOrWhiteSpace(group)) {
                await GroupError(botClient, chatId);
                return;
            }

            if(DateRegex().IsMatch(text)) {
                try {
                    var date = DateOnly.FromDateTime(DateTime.Parse(text));

                    await ScheduleRelevance(chatId, botClient, MainKeyboardMarkup);
                    await botClient.SendTextMessageAsync(chatId: chatId, text: scheduler.GetScheduleByDate(date, group, scheduleProfileGuid), replyMarkup: IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
                } catch(Exception) {
                    await botClient.SendTextMessageAsync(chatId: chatId, text: $"Сообщение распознано как дата, но не соответствует формату.", replyMarkup: MainKeyboardMarkup);
                }
            }
        }


        private async Task AcademicPerformancePerSemester(ITelegramBotClient botClient, ChatId chatId, string text, string StudentID) {
            var split = text.Split();
            if(split == null || split.Count() < 2) return;

            await botClient.SendTextMessageAsync(chatId: chatId, text: scheduler.GetProgressByTerm(int.Parse(split[0]), StudentID), replyMarkup: GetTermsKeyboardMarkup(StudentID));
            return;
        }

        private async Task Weeks(ITelegramBotClient botClient, ChatId chatId, string text, bool IsAdmin, string? group, Guid scheduleProfileGuid) {
            if(string.IsNullOrWhiteSpace(group)) {
                await GroupError(botClient, chatId);
                return;
            }

            switch(text) {
                case Constants.RK_ThisWeek:
                    foreach(var item in scheduler.GetScheduleByWeak(CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, System.DayOfWeek.Monday) - 1, group, scheduleProfileGuid))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: item, replyMarkup: IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
                case Constants.RK_NextWeek:
                    foreach(var item in scheduler.GetScheduleByWeak(CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, System.DayOfWeek.Monday), group, scheduleProfileGuid))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: item, replyMarkup: IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
            }
        }

        private ReplyKeyboardMarkup GetTermsKeyboardMarkup(string StudentID) {
            List<KeyboardButton[]> TermsKeyboardMarkup = new();

            var terms = dbContext.Progresses.Where(i => i.StudentID == StudentID && i.Mark != null).Select(i => i.Term).Distinct().OrderBy(i => i).ToArray();
            for(int i = 0; i < terms.Length; i++)
                TermsKeyboardMarkup.Add(new KeyboardButton[] { $"{terms[i]} {Constants.RK_Semester}", i + 1 < terms.Length ? $"{terms[++i]} {Constants.RK_Semester}" : "" });

            TermsKeyboardMarkup.Add(new KeyboardButton[] { Constants.RK_Back });

            return new(TermsKeyboardMarkup) { ResizeKeyboard = true };
        }

        private ReplyKeyboardMarkup GetProfileKeyboardMarkup(TelegramUser user) {
            List<KeyboardButton[]> ProfileKeyboardMarkup = new();

            if(user.ScheduleProfile.OwnerID == user.ChatID)
                ProfileKeyboardMarkup.AddRange(new[] { new KeyboardButton[] { $"Номер группы: {user.ScheduleProfile.Group}" }, new KeyboardButton[] { $"Номер зачётки: {user.ScheduleProfile.StudentID}" } });

            ProfileKeyboardMarkup.AddRange(new[] { new KeyboardButton[] { Constants.RK_AcademicPerformance }, new KeyboardButton[] { Constants.RK_Back } });

            return new(ProfileKeyboardMarkup) { ResizeKeyboard = true };
        }

        private async Task TodayAndTomorrow(ITelegramBotClient botClient, ChatId chatId, string text, bool IsAdmin, string? group, Guid scheduleProfileGuid) {
            if(string.IsNullOrWhiteSpace(group)) {
                await GroupError(botClient, chatId);
                return;
            }

            switch(text) {
                case Constants.RK_Today:
                    await botClient.SendTextMessageAsync(chatId: chatId, text: scheduler.GetScheduleByDate(DateOnly.FromDateTime(DateTime.Now), group, scheduleProfileGuid), replyMarkup: IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
                    break;

                case Constants.RK_Tomorrow:
                    await botClient.SendTextMessageAsync(chatId: chatId, text: scheduler.GetScheduleByDate(DateOnly.FromDateTime(DateTime.Now.AddDays(1)), group, scheduleProfileGuid), replyMarkup: IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
                    break;
            }
        }

        private async Task DayOfWeek(ITelegramBotClient botClient, ChatId chatId, string text, bool IsAdmin, string? group, Guid scheduleProfileGuid) {
            if(string.IsNullOrWhiteSpace(group)) {
                await GroupError(botClient, chatId);
                return;
            }

            switch(text) {
                case Constants.RK_Monday:
                    foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Monday, group, scheduleProfileGuid))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
                case Constants.RK_Tuesday:
                    foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Tuesday, group, scheduleProfileGuid))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
                case Constants.RK_Wednesday:
                    foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Wednesday, group, scheduleProfileGuid))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
                case Constants.RK_Thursday:
                    foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Thursday, group, scheduleProfileGuid))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
                case Constants.RK_Friday:
                    foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Friday, group, scheduleProfileGuid))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
                case Constants.RK_Saturday:
                    foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Saturday, group, scheduleProfileGuid))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
            }
        }

    }
}