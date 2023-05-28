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
                            new KeyboardButton[] { Constants.RK_Corps, Constants.RK_Exam },
                            new KeyboardButton[] { Constants.RK_Profile }
                        }) { ResizeKeyboard = true };

        private readonly ReplyKeyboardMarkup ExamKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { Constants.RK_NextExam, Constants.RK_AllExams },
                            new KeyboardButton[] { Constants.RK_Back }
                        }) { ResizeKeyboard = true };

        private readonly ReplyKeyboardMarkup DaysKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { Constants.RK_Monday, Constants.RK_Tuesday },
                            new KeyboardButton[] { Constants.RK_Wednesday, Constants.RK_Thursday },
                            new KeyboardButton[] { Constants.RK_Friday, Constants.RK_Saturday },
                            new KeyboardButton[] { Constants.RK_Back }
                        }) { ResizeKeyboard = true };

        private readonly ReplyKeyboardMarkup CancelKeyboardMarkup = new(Constants.RK_Cancel) { ResizeKeyboard = true };

        private readonly ReplyKeyboardMarkup ResetProfileLinkKeyboardMarkup = new(new KeyboardButton[] { Constants.RK_Reset, Constants.RK_Cancel }) { ResizeKeyboard = true };

        private readonly ReplyKeyboardMarkup WeekKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { Constants.RK_ThisWeek, Constants.RK_NextWeek },
                            new KeyboardButton[] { Constants.RK_Back }
                        }) { ResizeKeyboard = true };

        private readonly ReplyKeyboardMarkup CorpsKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { Constants.RK_MainCorps.text },
                            new KeyboardButton[] { Constants.RK_1Corps.text, Constants.RK_2Corps.text, Constants.RK_3Corps.text, Constants.RK_4Corps.text, Constants.RK_5Corps.text },
                            new KeyboardButton[] { Constants.RK_6Corps.text, Constants.RK_7Corps.text, Constants.RK_8Corps.text, Constants.RK_9Corps.text, Constants.RK_10Corps.text },
                            new KeyboardButton[] { Constants.RK_11Corps.text, Constants.RK_12Corps.text, Constants.RK_13Corps.text, Constants.LaboratoryCorps.text, Constants.RK_FOC.text },
                            new KeyboardButton[] { Constants.RK_SanatoriumDispensary.text },
                            new KeyboardButton[] { Constants.RK_Stadium.text },
                            new KeyboardButton[] { Constants.RK_PoolOnBoldin.text },
                            new KeyboardButton[] { Constants.RK_SportsComplexOnBoldin.text },
                            new KeyboardButton[] { Constants.RK_TechnicalCollege },
                            new KeyboardButton[] { Constants.RK_Back }
                        }) { ResizeKeyboard = true };
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
                    switch(user.CurrentPath) {
                        case Constants.RK_AcademicPerformance:
                            user.CurrentPath = null;
                            dbContext.SaveChanges();

                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Профиль", replyMarkup: GetProfileKeyboardMarkup(user));
                            break;

                        default:
                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Основное меню", replyMarkup: MainKeyboardMarkup);
                            break;
                    }
                    break;

                case Constants.RK_Today:
                case Constants.RK_Tomorrow:
                    await ScheduleRelevance(botClient, message.Chat, MainKeyboardMarkup);
                    await TodayAndTomorrow(botClient, message.Chat, message.Text, IsAdmin, user.ScheduleProfile);
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
                    await ScheduleRelevance(botClient, message.Chat, DaysKeyboardMarkup);
                    await DayOfWeek(botClient, message.Chat, message.Text, IsAdmin, user.ScheduleProfile);
                    break;

                case Constants.RK_ForAWeek:
                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: Constants.RK_ForAWeek, replyMarkup: WeekKeyboardMarkup);
                    break;

                case Constants.RK_ThisWeek:
                case Constants.RK_NextWeek:
                    await ScheduleRelevance(botClient, message.Chat, WeekKeyboardMarkup);
                    await Weeks(botClient, message.Chat, message.Text, IsAdmin, user.ScheduleProfile);
                    break;

                case Constants.RK_Exam:
                    if(!string.IsNullOrWhiteSpace(user.ScheduleProfile.Group)) {
                        if(dbContext.Disciplines.Any(i => i.Group == user.ScheduleProfile.Group && i.Class == Class.other && i.Date >= DateOnly.FromDateTime(DateTime.Now)))
                            await ScheduleRelevance(botClient, message.Chat, replyMarkup: ExamKeyboardMarkup);
                        else
                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: "В расписании нет будущих экзаменов.", replyMarkup: MainKeyboardMarkup);

                    } else {
                        if(IsAdmin)
                            await GroupErrorAdmin(botClient, message.Chat);
                        else
                            await GroupErrorUser(botClient, message.Chat);
                    }
                    break;

                case Constants.RK_AllExams:
                    await Exams(botClient, message.Chat, user.ScheduleProfile, IsAdmin);
                    break;

                case Constants.RK_NextExam:
                    await Exams(botClient, message.Chat, user.ScheduleProfile, IsAdmin, false);
                    break;

                case Constants.RK_AcademicPerformance:
                    if(!string.IsNullOrWhiteSpace(user.ScheduleProfile.StudentID)) {
                        user.CurrentPath = Constants.RK_AcademicPerformance;
                        dbContext.SaveChanges();
                        await ProgressRelevance(botClient, message.Chat, GetTermsKeyboardMarkup(user.ScheduleProfile.StudentID));
                    } else {
                        if(IsAdmin)
                            await StudentIdErrorAdmin(botClient, message.Chat);
                        else
                            await StudentIdErrorUser(botClient, message.Chat);
                    }

                    break;

                case Constants.RK_Profile:
                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Профиль", replyMarkup: GetProfileKeyboardMarkup(user));
                    break;

                case Constants.RK_GetProfileLink:
                    if(IsAdmin) {
                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: $"Если вы хотите поделиться своим расписанием с кем-то, просто отправьте им следующую команду: " +
                        $"\n`/SetProfile {user.ScheduleProfileGuid}`" +
                        $"\nЕсли другой пользователь введет эту команду, он сможет видеть расписание с вашими изменениями.", replyMarkup: GetProfileKeyboardMarkup(user), parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                    } else {
                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Поделиться профилем может только его владелец!", replyMarkup: MainKeyboardMarkup);
                    }
                    break;

                case Constants.RK_ResetProfileLink:
                    if(!IsAdmin) {
                        user.Mode = Mode.ResetProfileLink;
                        dbContext.SaveChanges();
                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Вы точно уверены что хотите восстановить свой профиль?", replyMarkup: ResetProfileLinkKeyboardMarkup);
                    } else {
                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Владельцу профиля нет смысла его восстанавливать!", replyMarkup: MainKeyboardMarkup);
                    }

                    break;

                case Constants.RK_Corps:
                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Выберите корпус, и я покажу где он на карте", replyMarkup: CorpsKeyboardMarkup);
                    break;

                #region Corps
                case Constants.RK_FOC.text: await SendCorpsInfo(botClient, message.Chat, new Constants.RK_FOC()); break;
                case Constants.RK_1Corps.text: await SendCorpsInfo(botClient, message.Chat, new Constants.RK_1Corps()); break;
                case Constants.RK_2Corps.text: await SendCorpsInfo(botClient, message.Chat, new Constants.RK_2Corps()); break;
                case Constants.RK_3Corps.text: await SendCorpsInfo(botClient, message.Chat, new Constants.RK_3Corps()); break;
                case Constants.RK_4Corps.text: await SendCorpsInfo(botClient, message.Chat, new Constants.RK_4Corps()); break;
                case Constants.RK_5Corps.text: await SendCorpsInfo(botClient, message.Chat, new Constants.RK_5Corps()); break;
                case Constants.RK_6Corps.text: await SendCorpsInfo(botClient, message.Chat, new Constants.RK_6Corps()); break;
                case Constants.RK_7Corps.text: await SendCorpsInfo(botClient, message.Chat, new Constants.RK_7Corps()); break;
                case Constants.RK_8Corps.text: await SendCorpsInfo(botClient, message.Chat, new Constants.RK_8Corps()); break;
                case Constants.RK_9Corps.text: await SendCorpsInfo(botClient, message.Chat, new Constants.RK_9Corps()); break;
                case Constants.RK_10Corps.text: await SendCorpsInfo(botClient, message.Chat, new Constants.RK_10Corps()); break;
                case Constants.RK_11Corps.text: await SendCorpsInfo(botClient, message.Chat, new Constants.RK_11Corps()); break;
                case Constants.RK_12Corps.text: await SendCorpsInfo(botClient, message.Chat, new Constants.RK_12Corps()); break;
                case Constants.RK_13Corps.text: await SendCorpsInfo(botClient, message.Chat, new Constants.RK_13Corps()); break;
                case Constants.RK_Stadium.text: await SendCorpsInfo(botClient, message.Chat, new Constants.RK_Stadium()); break;
                case Constants.RK_MainCorps.text: await SendCorpsInfo(botClient, message.Chat, new Constants.RK_MainCorps()); break;
                case Constants.RK_PoolOnBoldin.text: await SendCorpsInfo(botClient, message.Chat, new Constants.RK_PoolOnBoldin()); break;
                case Constants.LaboratoryCorps.text: await SendCorpsInfo(botClient, message.Chat, new Constants.LaboratoryCorps()); break;
                case Constants.RK_SanatoriumDispensary.text: await SendCorpsInfo(botClient, message.Chat, new Constants.RK_SanatoriumDispensary()); break;
                case Constants.RK_SportsComplexOnBoldin.text: await SendCorpsInfo(botClient, message.Chat, new Constants.RK_SportsComplexOnBoldin()); break;
                case Constants.RK_TechnicalCollege:
                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Технический колледж имени С.И. Мосина территориально расположен на трех площадках:", replyMarkup: CancelKeyboardMarkup);

                    await botClient.SendVenueAsync(chatId: message.Chat, latitude: 54.200399f, longitude: 37.535350f, title: "", address: "поселок Мясново, 18-й проезд, 94", replyMarkup: CorpsKeyboardMarkup);
                    await botClient.SendVenueAsync(chatId: message.Chat, latitude: 54.192146f, longitude: 37.588119f, title: "", address: "улица Вересаева, 12", replyMarkup: CorpsKeyboardMarkup);
                    await botClient.SendVenueAsync(chatId: message.Chat, latitude: 54.199636f, longitude: 37.604477f, title: "", address: "улица Коминтерна, 21", replyMarkup: CorpsKeyboardMarkup);
                    break;
                #endregion

                default:
                    if(message.Text?.Contains(Constants.RK_Semester) ?? false) {
                        if(!string.IsNullOrWhiteSpace(studentID)) {
                            await AcademicPerformancePerSemester(botClient, message.Chat, message.Text, studentID);
                        } else {
                            if(IsAdmin)
                                await StudentIdErrorAdmin(botClient, message.Chat);
                            else
                                await StudentIdErrorUser(botClient, message.Chat);
                        }
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

                    if(message.Text?.Contains("/SetProfile") ?? false) {
                        try {
                            if(Guid.TryParse(message.Text?.Split(' ')[1] ?? "", out Guid profile)) {
                                if(profile != user.ScheduleProfileGuid && dbContext.ScheduleProfile.Any(i => i.ID == profile)) {
                                    user.ScheduleProfileGuid = profile;
                                    dbContext.SaveChanges();
                                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Вы успешно сменили профиль", replyMarkup: MainKeyboardMarkup);
                                } else {
                                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Вы пытаетесь изменить свой профиль на текущий или на профиль, который не существует", replyMarkup: MainKeyboardMarkup);
                                }
                            } else {
                                await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Идентификатор профиля не распознан", replyMarkup: MainKeyboardMarkup);
                            }
                        } catch(IndexOutOfRangeException) { }

                        return;
                    }

                    if(message.Text != null)
                        await GetScheduleByDate(botClient, message.Chat, message.Text, IsAdmin, user.ScheduleProfile);

                    break;
            }
        }

        private async Task SendCorpsInfo(ITelegramBotClient botClient, ChatId chatId, Constants.Corps corps) => await botClient.SendVenueAsync(chatId: chatId, latitude: corps.Latitude, longitude: corps.Longitude, title: corps.Title, address: corps.Address, replyMarkup: CorpsKeyboardMarkup);

        private async Task GetScheduleByDate(ITelegramBotClient botClient, ChatId chatId, string text, bool IsAdmin, ScheduleProfile profile) {
            if(string.IsNullOrWhiteSpace(profile.Group)) {
                if(IsAdmin)
                    await GroupErrorAdmin(botClient, chatId);
                else
                    await GroupErrorUser(botClient, chatId);
                return;
            }

            if(DateRegex().IsMatch(text)) {
                try {
                    var date = DateOnly.FromDateTime(DateTime.Parse(text));

                    await ScheduleRelevance(botClient, chatId, MainKeyboardMarkup);
                    await botClient.SendTextMessageAsync(chatId: chatId, text: scheduler.GetScheduleByDate(date, profile), replyMarkup: IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
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

        private async Task Weeks(ITelegramBotClient botClient, ChatId chatId, string text, bool IsAdmin, ScheduleProfile profile) {
            if(string.IsNullOrWhiteSpace(profile.Group)) {
                if(IsAdmin)
                    await GroupErrorAdmin(botClient, chatId);
                else
                    await GroupErrorUser(botClient, chatId);
                return;
            }

            switch(text) {
                case Constants.RK_ThisWeek:
                    foreach(var item in scheduler.GetScheduleByWeak(CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, System.DayOfWeek.Monday) - 1, profile))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: item, replyMarkup: IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
                case Constants.RK_NextWeek:
                    foreach(var item in scheduler.GetScheduleByWeak(CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, System.DayOfWeek.Monday), profile))
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

            if(user.ScheduleProfile.OwnerID == user.ChatID) {
                ProfileKeyboardMarkup.AddRange(new[] {  new KeyboardButton[] { $"Номер группы: {user.ScheduleProfile.Group}" },
                                                        new KeyboardButton[] { $"Номер зачётки: {user.ScheduleProfile.StudentID}" },
                                                        new KeyboardButton[] { Constants.RK_GetProfileLink }
                                                     });
            } else {
                ProfileKeyboardMarkup.Add(new KeyboardButton[] { Constants.RK_ResetProfileLink });
            }

            ProfileKeyboardMarkup.AddRange(new[] { new KeyboardButton[] { Constants.RK_AcademicPerformance }, new KeyboardButton[] { Constants.RK_Back } });

            return new(ProfileKeyboardMarkup) { ResizeKeyboard = true };
        }

        private async Task TodayAndTomorrow(ITelegramBotClient botClient, ChatId chatId, string text, bool IsAdmin, ScheduleProfile profile) {
            if(string.IsNullOrWhiteSpace(profile.Group)) {
                if(IsAdmin)
                    await GroupErrorAdmin(botClient, chatId);
                else
                    await GroupErrorUser(botClient, chatId);
                return;
            }

            switch(text) {
                case Constants.RK_Today:
                    await botClient.SendTextMessageAsync(chatId: chatId, text: scheduler.GetScheduleByDate(DateOnly.FromDateTime(DateTime.Now), profile), replyMarkup: IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
                    break;

                case Constants.RK_Tomorrow:
                    await botClient.SendTextMessageAsync(chatId: chatId, text: scheduler.GetScheduleByDate(DateOnly.FromDateTime(DateTime.Now.AddDays(1)), profile), replyMarkup: IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
                    break;
            }
        }

        private async Task DayOfWeek(ITelegramBotClient botClient, ChatId chatId, string text, bool IsAdmin, ScheduleProfile profile) {
            if(string.IsNullOrWhiteSpace(profile.Group)) {
                if(IsAdmin)
                    await GroupErrorAdmin(botClient, chatId);
                else
                    await GroupErrorUser(botClient, chatId);
                return;
            }

            switch(text) {
                case Constants.RK_Monday:
                    foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Monday, profile))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
                case Constants.RK_Tuesday:
                    foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Tuesday, profile))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
                case Constants.RK_Wednesday:
                    foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Wednesday, profile))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
                case Constants.RK_Thursday:
                    foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Thursday, profile))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
                case Constants.RK_Friday:
                    foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Friday, profile))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
                case Constants.RK_Saturday:
                    foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Saturday, profile))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
            }
        }

        private async Task Exams(ITelegramBotClient botClient, ChatId chatId, ScheduleProfile profile, bool IsAdmin, bool all = true) {
            if(string.IsNullOrWhiteSpace(profile.Group)) {
                if(IsAdmin)
                    await GroupErrorAdmin(botClient, chatId);
                else
                    await GroupErrorUser(botClient, chatId);
                return;
            }

            foreach(var item in scheduler.GetExamse(profile, all))
                await botClient.SendTextMessageAsync(chatId: chatId, text: item, replyMarkup: ExamKeyboardMarkup);
        }
    }
}