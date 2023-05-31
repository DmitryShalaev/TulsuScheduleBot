using System.Globalization;
using System.Text.RegularExpressions;

using Microsoft.EntityFrameworkCore;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

using static ScheduleBot.Bot.Constants;

namespace ScheduleBot.Bot {
    public partial class TelegramBot {
        [GeneratedRegex("^[А-Я][а-я]*[ ]?[а-я]*$")]
        private static partial Regex DefaultMessageRegex();
        [GeneratedRegex("^([0-9]+)[ ]([а-я]+)$")]
        private static partial Regex TermsMessageRegex();
        [GeneratedRegex("(^[А-Я][а-я]*[ ]?[а-я]*):")]
        private static partial Regex GroupOrStudentIDMessageRegex();
        [GeneratedRegex("(^/[A-z]+)[ ]?([A-z0-9-]*)$")]
        private static partial Regex CommandMessageRegex();

        [GeneratedRegex("^([A-z][A-z]+)[ ]([0-9.:]+)$")]
        private static partial Regex DisciplineCallbackRegex();

        [GeneratedRegex("^\\d{1,2}[ ,./-](\\d{1,2}|\\w{3,8})([ ,./-](\\d{2}|\\d{4}))?$")]
        private static partial Regex DateRegex();

        private readonly ITelegramBotClient telegramBot;
        private readonly Scheduler.Scheduler scheduler;
        private readonly ScheduleDbContext dbContext;

        private readonly CommandManager commandManager;

        public TelegramBot(Scheduler.Scheduler scheduler, ScheduleDbContext dbContext) {
            this.scheduler = scheduler;
            this.dbContext = dbContext;

            telegramBot = new TelegramBotClient(Environment.GetEnvironmentVariable("TelegramBotToken") ?? "");

            commandManager = new(telegramBot, (string message, TelegramUser user, out string args) => {
                args = "";

                if(DefaultMessageRegex().IsMatch(message))
                    return $"{message} {user.Mode}";

                var match = TermsMessageRegex().Match(message);
                if(match.Success) {
                    args = match.Groups[1].ToString();
                    return $"{match.Groups[2]} {user.Mode}";
                }

                match = GroupOrStudentIDMessageRegex().Match(message);
                if(match.Success)
                    return $"{match.Groups[1]} {user.Mode}";

                match = CommandMessageRegex().Match(message);
                if(match.Success) {
                    args = match.Groups[2].ToString();
                    return $"{match.Groups[1]} {user.Mode}";
                }

                return $"{message} {user.Mode.ToString()}";

            }, (string message, TelegramUser user, out string args) => {
                args = "";

                var match = DisciplineCallbackRegex().Match(message);
                if(match.Success) {
                    args = match.Groups[2].ToString();
                    return $"{match.Groups[1]} {user.Mode}";
                }

                return $"{message} {user.Mode}";
            });

            #region Message
            #region Main
            commandManager.AddMessageCommand("/start", Mode.Default, async (botClient, chatId, user, args) => {
                await botClient.SendTextMessageAsync(chatId: chatId, text: "👋", replyMarkup: MainKeyboardMarkup);

                if(string.IsNullOrWhiteSpace(user.ScheduleProfile.Group)) {
                    user.Mode = Mode.GroupСhange;
                    dbContext.SaveChanges();

                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Для начала работы с ботом необходимо указать номер учебной группы", replyMarkup: CancelKeyboardMarkup);
                }
            });
            commandManager.AddMessageCommand(new[] { Constants.RK_Back, Constants.RK_Cancel }, Mode.Default, async (botClient, chatId, user, args) => {
                switch(user.CurrentPath ?? "") {
                    case Constants.RK_AcademicPerformance:
                    case Constants.RK_Profile:
                    case Constants.RK_Corps:
                        user.CurrentPath = null;
                        dbContext.SaveChanges();

                        await botClient.SendTextMessageAsync(chatId: chatId, text: Constants.RK_Other, replyMarkup: AdditionalKeyboardMarkup);
                        break;

                    default:
                        await botClient.SendTextMessageAsync(chatId: chatId, text: Constants.RK_MainMenu, replyMarkup: MainKeyboardMarkup);
                        break;
                }
            });
            commandManager.AddMessageCommand(new[] { Constants.RK_Back, Constants.RK_Cancel }, Mode.AddingDiscipline, async (botClient, chatId, user, args) => {
                user.Mode = Mode.Default;
                dbContext.TemporaryAddition.Remove(dbContext.TemporaryAddition.Where(i => i.TelegramUser == user).OrderByDescending(i => i.AddDate).First());
                dbContext.SaveChanges();

                await botClient.SendTextMessageAsync(chatId: chatId, text: Constants.RK_MainMenu, replyMarkup: MainKeyboardMarkup);
            });
            commandManager.AddMessageCommand(new[] { Constants.RK_Back, Constants.RK_Cancel }, Mode.GroupСhange, SetDefaultMode(dbContext));
            commandManager.AddMessageCommand(new[] { Constants.RK_Back, Constants.RK_Cancel }, Mode.StudentIDСhange, SetDefaultMode(dbContext));
            commandManager.AddMessageCommand(new[] { Constants.RK_Back, Constants.RK_Cancel }, Mode.ResetProfileLink, SetDefaultMode(dbContext));
            commandManager.AddMessageCommand(Constants.RK_Today, Mode.Default, async (botClient, chatId, user, args) => {
                await ScheduleRelevance(botClient, chatId, MainKeyboardMarkup);
                var date = DateOnly.FromDateTime(DateTime.Now);
                await botClient.SendTextMessageAsync(chatId: chatId, text: scheduler.GetScheduleByDate(date, user.ScheduleProfile), replyMarkup: GetInlineKeyboardButton(date, user));
            });
            commandManager.AddMessageCommand(Constants.RK_Tomorrow, Mode.Default, async (botClient, chatId, user, args) => {
                await ScheduleRelevance(botClient, chatId, MainKeyboardMarkup);
                var date = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
                await botClient.SendTextMessageAsync(chatId: chatId, text: scheduler.GetScheduleByDate(date, user.ScheduleProfile), replyMarkup: GetInlineKeyboardButton(date, user));
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(Constants.RK_ByDays, Mode.Default, async (botClient, chatId, user, args) => {
                await ScheduleRelevance(botClient, chatId, DaysKeyboardMarkup);
            });
            commandManager.AddMessageCommand(Constants.RK_Monday, Mode.Default, async (botClient, chatId, user, args) => {
                foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Monday, user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: day.Item1, replyMarkup: GetInlineKeyboardButton(day.Item2, user));
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(Constants.RK_Tuesday, Mode.Default, async (botClient, chatId, user, args) => {
                foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Tuesday, user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: day.Item1, replyMarkup: GetInlineKeyboardButton(day.Item2, user));
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(Constants.RK_Wednesday, Mode.Default, async (botClient, chatId, user, args) => {
                foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Wednesday, user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: day.Item1, replyMarkup: GetInlineKeyboardButton(day.Item2, user));
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(Constants.RK_Thursday, Mode.Default, async (botClient, chatId, user, args) => {
                foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Thursday, user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: day.Item1, replyMarkup: GetInlineKeyboardButton(day.Item2, user));
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(Constants.RK_Friday, Mode.Default, async (botClient, chatId, user, args) => {
                foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Friday, user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: day.Item1, replyMarkup: GetInlineKeyboardButton(day.Item2, user));
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(Constants.RK_Saturday, Mode.Default, async (botClient, chatId, user, args) => {
                foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Saturday, user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: day.Item1, replyMarkup: GetInlineKeyboardButton(day.Item2, user));
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(Constants.RK_ForAWeek, Mode.Default, async (botClient, chatId, user, args) => {
                await ScheduleRelevance(botClient, chatId, WeekKeyboardMarkup);
                await botClient.SendTextMessageAsync(chatId: chatId, text: Constants.RK_ForAWeek, replyMarkup: WeekKeyboardMarkup);
            });
            commandManager.AddMessageCommand(Constants.RK_ThisWeek, Mode.Default, async (botClient, chatId, user, args) => {
                foreach(var item in scheduler.GetScheduleByWeak(CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, System.DayOfWeek.Monday) - 1, user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: item.Item1, replyMarkup: GetInlineKeyboardButton(item.Item2, user));
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(Constants.RK_NextWeek, Mode.Default, async (botClient, chatId, user, args) => {
                foreach(var item in scheduler.GetScheduleByWeak(CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, System.DayOfWeek.Monday), user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: item.Item1, replyMarkup: GetInlineKeyboardButton(item.Item2, user));
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(Constants.RK_Exam, Mode.Default, async (botClient, chatId, user, args) => {
                if(dbContext.Disciplines.Any(i => i.Group == user.ScheduleProfile.Group && i.Class == Class.other && i.Date >= DateOnly.FromDateTime(DateTime.Now)))
                    await ScheduleRelevance(botClient, chatId, replyMarkup: ExamKeyboardMarkup);
                else
                    await botClient.SendTextMessageAsync(chatId: chatId, text: "В расписании нет будущих экзаменов.", replyMarkup: MainKeyboardMarkup);
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(Constants.RK_AllExams, Mode.Default, async (botClient, chatId, user, args) => {
                foreach(var item in scheduler.GetExamse(user.ScheduleProfile, true))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: item, replyMarkup: ExamKeyboardMarkup);
            });
            commandManager.AddMessageCommand(Constants.RK_NextExam, Mode.Default, async (botClient, chatId, user, args) => {
                foreach(var item in scheduler.GetExamse(user.ScheduleProfile, false))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: item, replyMarkup: ExamKeyboardMarkup);
            });
            commandManager.AddMessageCommand(Constants.RK_AcademicPerformance, Mode.Default, async (botClient, chatId, user, args) => {
                user.CurrentPath = Constants.RK_AcademicPerformance;
                dbContext.SaveChanges();
                await ProgressRelevance(botClient, chatId, GetTermsKeyboardMarkup(user.ScheduleProfile.StudentID ?? throw new NullReferenceException("StudentID")));
            }, CommandManager.Check.studentId);
            commandManager.AddMessageCommand(Constants.RK_Profile, Mode.Default, async (botClient, chatId, user, args) => {
                user.CurrentPath = Constants.RK_Profile;
                dbContext.SaveChanges();
                await botClient.SendTextMessageAsync(chatId: chatId, text: Constants.RK_Profile, replyMarkup: GetProfileKeyboardMarkup(user));
            });
            commandManager.AddMessageCommand(Constants.RK_GetProfileLink, Mode.Default, async (botClient, chatId, user, args) => {
                if(user.IsAdmin()) {
                    await botClient.SendTextMessageAsync(chatId: chatId, text: $"Если вы хотите поделиться своим расписанием с кем-то, просто отправьте им следующую команду: " +
                    $"\n`/SetProfile {user.ScheduleProfileGuid}`" +
                    $"\nЕсли другой пользователь введет эту команду, он сможет видеть расписание с вашими изменениями.", replyMarkup: GetProfileKeyboardMarkup(user), parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                } else {
                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Поделиться профилем может только его владелец!", replyMarkup: MainKeyboardMarkup);
                }
            });
            commandManager.AddMessageCommand(Constants.RK_ResetProfileLink, Mode.Default, async (botClient, chatId, user, args) => {
                if(!user.IsAdmin()) {
                    user.Mode = Mode.ResetProfileLink;
                    dbContext.SaveChanges();
                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Вы точно уверены что хотите восстановить свой профиль?", replyMarkup: ResetProfileLinkKeyboardMarkup);
                } else {
                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Владельцу профиля нет смысла его восстанавливать!", replyMarkup: MainKeyboardMarkup);
                }
            });
            commandManager.AddMessageCommand(Constants.RK_Reset, Mode.ResetProfileLink, async (botClient, chatId, user, args) => {
                user.Mode = Mode.Default;

                var profile = dbContext.ScheduleProfile.FirstOrDefault(i => i.OwnerID == user.ChatID);
                if(profile is not null) {
                    user.ScheduleProfile = profile;
                } else {
                    profile = new() { OwnerID = user.ChatID };
                    dbContext.ScheduleProfile.Add(profile);
                    user.ScheduleProfile = profile;
                }

                dbContext.SaveChanges();

                await botClient.SendTextMessageAsync(chatId: chatId, text: Constants.RK_Profile, replyMarkup: GetProfileKeyboardMarkup(user));
            });
            commandManager.AddMessageCommand(Constants.RK_Other, Mode.Default, async (botClient, chatId, user, args) => {
                await botClient.SendTextMessageAsync(chatId: chatId, text: Constants.RK_Other, replyMarkup: AdditionalKeyboardMarkup);
            });
            commandManager.AddMessageCommand(Constants.RK_Semester, Mode.Default, async (botClient, chatId, user, args) => {
                var StudentID = user.ScheduleProfile.StudentID ?? throw new NullReferenceException("StudentID");
                await botClient.SendTextMessageAsync(chatId: chatId, text: scheduler.GetProgressByTerm(int.Parse(args), StudentID), replyMarkup: GetTermsKeyboardMarkup(StudentID));
            }, CommandManager.Check.studentId);
            commandManager.AddMessageCommand(Constants.RK_GroupNumber, Mode.Default, async (botClient, chatId, user, args) => {
                if(user.IsAdmin()) {
                    user.Mode = Mode.GroupСhange;
                    dbContext.SaveChanges();

                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Хотите сменить номер учебной группы? Если да, то напишите новый номер", replyMarkup: CancelKeyboardMarkup);
                }
            });
            commandManager.AddMessageCommand(Constants.RK_StudentIDNumber, Mode.Default, async (botClient, chatId, user, args) => {
                if(user.IsAdmin()) {
                    user.Mode = Mode.StudentIDСhange;
                    dbContext.SaveChanges();

                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Хотите сменить номер зачётки? Если да, то напишите новый номер", replyMarkup: CancelKeyboardMarkup);
                }
            });
            commandManager.AddMessageCommand("/SetProfile", Mode.Default, async (botClient, chatId, user, args) => {
                try {
                    if(Guid.TryParse(args, out Guid profile)) {
                        if(profile != user.ScheduleProfileGuid && dbContext.ScheduleProfile.Any(i => i.ID == profile)) {
                            user.ScheduleProfileGuid = profile;
                            dbContext.SaveChanges();
                            await botClient.SendTextMessageAsync(chatId: chatId, text: "Вы успешно сменили профиль", replyMarkup: MainKeyboardMarkup);
                        } else {
                            await botClient.SendTextMessageAsync(chatId: chatId, text: "Вы пытаетесь изменить свой профиль на текущий или на профиль, который не существует", replyMarkup: MainKeyboardMarkup);
                        }
                    } else {
                        await botClient.SendTextMessageAsync(chatId: chatId, text: "Идентификатор профиля не распознан", replyMarkup: MainKeyboardMarkup);
                    }
                } catch(IndexOutOfRangeException) { }
            });
            commandManager.AddMessageCommand(Mode.Default, async (botClient, chatId, user, args) => {
                if(DateRegex().IsMatch(args)) {
                    try {
                        var date = DateOnly.FromDateTime(DateTime.Parse(args));

                        await ScheduleRelevance(botClient, chatId, MainKeyboardMarkup);
                        await botClient.SendTextMessageAsync(chatId: chatId, text: scheduler.GetScheduleByDate(date, user.ScheduleProfile), replyMarkup: GetInlineKeyboardButton(date, user));

                        return true;
                    } catch(Exception) {
                        await botClient.SendTextMessageAsync(chatId: chatId, text: $"Сообщение распознано как дата, но не соответствует формату.", replyMarkup: MainKeyboardMarkup);
                    }
                }
                return false;
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(Mode.AddingDiscipline, async (botClient, chatId, user, args) => {
                await SetStagesAddingDisciplineAsync(botClient, chatId, args, user);
                return true;
            });
            commandManager.AddMessageCommand(Mode.GroupСhange, async (botClient, chatId, user, args) => {
                await botClient.SendTextMessageAsync(chatId: chatId, text: "Нужно подождать...", replyMarkup: CancelKeyboardMarkup);
                Parser parser = new(dbContext, false);
                bool flag = dbContext.ScheduleProfile.Select(i=>i.Group).Contains(args);

                if(flag || parser.GetDates(args) is not null) {
                    user.Mode = Mode.Default;
                    user.ScheduleProfile.Group = args;
                    dbContext.SaveChanges();

                    await botClient.SendTextMessageAsync(chatId: chatId, text: $"Номер группы успешно изменен на {args} ", replyMarkup: GetProfileKeyboardMarkup(user));

                    if(!flag)
                        parser.UpdatingDisciplines(args);
                } else {
                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Сайт ТулГУ не отвечает или такой группы не существует", replyMarkup: CancelKeyboardMarkup);
                }
                return true;
            });
            commandManager.AddMessageCommand(Mode.StudentIDСhange, async (botClient, chatId, user, args) => {
                await botClient.SendTextMessageAsync(chatId: chatId, text: "Нужно подождать...", replyMarkup: CancelKeyboardMarkup);

                if(int.TryParse(args, out int studentID)) {
                    Parser parser = new(dbContext, false);
                    bool flag = dbContext.ScheduleProfile.Select(i => i.StudentID).Contains(args);

                    if(flag || parser.GetProgress(args) is not null) {
                        user.Mode = Mode.Default;
                        user.ScheduleProfile.StudentID = studentID.ToString();
                        dbContext.SaveChanges();

                        await botClient.SendTextMessageAsync(chatId: chatId, text: $"Номер зачётки успешно изменен на {args} ", replyMarkup: GetProfileKeyboardMarkup(user));

                        if(!flag)
                            parser.UpdatingProgress(studentID.ToString());
                    } else {
                        await botClient.SendTextMessageAsync(chatId: chatId, text: "Сайт ТулГУ не отвечает или указан неверный номер зачётки", replyMarkup: CancelKeyboardMarkup);
                    }

                } else {
                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Не удалось распознать введенный номер зачётной книжки", replyMarkup: CancelKeyboardMarkup);
                }
                return true;
            });

            commandManager.AddCallbackCommand(Constants.IK_Edit.callback, Mode.Default, async (botClient, chatId, messageId, user, message, args) => {
                if(DateOnly.TryParse(args, out DateOnly date)) {
                    if(user.IsAdmin())
                        await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: scheduler.GetScheduleByDate(date, user.ScheduleProfile, true), replyMarkup: GetEditAdminInlineKeyboardButton(date, user.ScheduleProfile));
                    else
                        await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: scheduler.GetScheduleByDate(date, user.ScheduleProfile), replyMarkup: GetEditAdminInlineKeyboardButton(date, user.ScheduleProfile));
                }
            }, CommandManager.Check.group);
            commandManager.AddCallbackCommand(Constants.IK_ViewAll.callback, Mode.Default, async (botClient, chatId, messageId, user, message, args) => {
                if(DateOnly.TryParse(args, out DateOnly date))
                    await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: scheduler.GetScheduleByDate(date, user.ScheduleProfile, true), replyMarkup: GetInlineBackKeyboardButton(date, user));
            }, CommandManager.Check.group);
            commandManager.AddCallbackCommand(Constants.IK_Back.callback, Mode.Default, async (botClient, chatId, messageId, user, message, args) => {
                if(DateOnly.TryParse(args, out DateOnly date))
                    await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: scheduler.GetScheduleByDate(date, user.ScheduleProfile), replyMarkup: GetInlineKeyboardButton(date, user));
            }, CommandManager.Check.group);
            commandManager.AddCallbackCommand(Constants.IK_Add.callback, Mode.Default, async (botClient, chatId, messageId, user, message, args) => {
                if(DateOnly.TryParse(args, out DateOnly date)) {
                    if(user.IsAdmin()) {
                        try {
                            user.Mode = Mode.AddingDiscipline;
                            dbContext.TemporaryAddition.Add(new(user, date));
                            dbContext.SaveChanges();
                            await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: scheduler.GetScheduleByDate(date, user.ScheduleProfile));
                            await botClient.SendTextMessageAsync(chatId: chatId, text: GetStagesAddingDiscipline(user), replyMarkup: CancelKeyboardMarkup);
                        } catch(Exception e) {

                            await Console.Out.WriteLineAsync(e.Message);
                        }
                    } else {
                        await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: scheduler.GetScheduleByDate(date, user.ScheduleProfile), replyMarkup: GetEditAdminInlineKeyboardButton(date, user.ScheduleProfile));
                    }
                }
            }, CommandManager.Check.group);
            commandManager.AddCallbackCommand("DisciplineDay", Mode.Default, async (botClient, chatId, messageId, user, message, args) => {
                var discipline = dbContext.Disciplines.FirstOrDefault(i => i.ID == uint.Parse(args));
                if(discipline is not null) {
                    if(user.IsAdmin()) {
                        var completedDisciplines = dbContext.CompletedDisciplines.Where(i => i.ScheduleProfileGuid == user.ScheduleProfileGuid).ToList();

                        CompletedDiscipline dayTmp = new(discipline, user.ScheduleProfileGuid);
                        var dayCompletedDisciplines = completedDisciplines.FirstOrDefault(i => i.Equals(dayTmp));

                        if(dayCompletedDisciplines is not null)
                            dbContext.CompletedDisciplines.Remove(dayCompletedDisciplines);
                        else
                            dbContext.CompletedDisciplines.Add(dayTmp);

                        dbContext.SaveChanges();
                        await botClient.EditMessageReplyMarkupAsync(chatId: chatId, messageId: messageId, replyMarkup: GetEditAdminInlineKeyboardButton(discipline.Date, user.ScheduleProfile));
                    } else {
                        await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: scheduler.GetScheduleByDate(discipline.Date, user.ScheduleProfile), replyMarkup: GetEditAdminInlineKeyboardButton(discipline.Date, user.ScheduleProfile));
                    }
                }
            }, CommandManager.Check.group);
            commandManager.AddCallbackCommand("DisciplineAlways", Mode.Default, async (botClient, chatId, messageId, user, message, args) => {
                var discipline = dbContext.Disciplines.FirstOrDefault(i => i.ID == uint.Parse(args));
                if(discipline is not null) {
                    if(user.IsAdmin()) {
                        var completedDisciplines = dbContext.CompletedDisciplines.Where(i => i.ScheduleProfileGuid == user.ScheduleProfileGuid).ToList();

                        CompletedDiscipline alwaysTmp = new(discipline, user.ScheduleProfileGuid) { Date = null };
                        var alwaysCompletedDisciplines = completedDisciplines.FirstOrDefault(i => i.Equals(alwaysTmp));

                        if(alwaysCompletedDisciplines is not null) {
                            dbContext.CompletedDisciplines.Remove(alwaysCompletedDisciplines);
                        } else {
                            dbContext.CompletedDisciplines.RemoveRange(dbContext.CompletedDisciplines.Where(i => i.ScheduleProfileGuid == user.ScheduleProfileGuid && i.Date != null && i.Name == discipline.Name && i.Lecturer == discipline.Lecturer && i.Class == discipline.Class && i.Subgroup == discipline.Subgroup));
                            dbContext.CompletedDisciplines.Add(alwaysTmp);
                        }

                        dbContext.SaveChanges();
                        await botClient.EditMessageReplyMarkupAsync(chatId: chatId, messageId: messageId, replyMarkup: GetEditAdminInlineKeyboardButton(discipline.Date, user.ScheduleProfile));
                    } else {
                        await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: scheduler.GetScheduleByDate(discipline.Date, user.ScheduleProfile), replyMarkup: GetEditAdminInlineKeyboardButton(discipline.Date, user.ScheduleProfile));
                    }
                }
            }, CommandManager.Check.group);
            commandManager.AddCallbackCommand("Delete", Mode.Default, async (botClient, chatId, messageId, user, message, args) => {
                var customDiscipline = dbContext.CustomDiscipline.FirstOrDefault(i => i.ID == uint.Parse(args));

                if(customDiscipline is not null) {
                    if(user.IsAdmin()) {
                        dbContext.CustomDiscipline.Remove(customDiscipline);
                        dbContext.SaveChanges();

                        await botClient.EditMessageReplyMarkupAsync(chatId: chatId, messageId: messageId, replyMarkup: GetEditAdminInlineKeyboardButton(customDiscipline.Date, user.ScheduleProfile));
                    } else {
                        await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: scheduler.GetScheduleByDate(customDiscipline.Date, user.ScheduleProfile), replyMarkup: GetEditAdminInlineKeyboardButton(customDiscipline.Date, user.ScheduleProfile));
                    }
                }
            }, CommandManager.Check.group);
            commandManager.AddCallbackCommand(Constants.IK_SetEndTime.callback, Mode.AddingDiscipline, async (botClient, chatId, messageId, user, message, args) => {
                var temporaryAddition = dbContext.TemporaryAddition.Where(i => i.TelegramUser == user).OrderByDescending(i => i.AddDate).First();

                temporaryAddition.EndTime = TimeOnly.Parse(args);
                temporaryAddition.Counter++;

                await SaveAddingDisciplineAsync(botClient, chatId, user, temporaryAddition);
            });
            #endregion
            #region Corps
            commandManager.AddMessageCommand(Constants.RK_Corps, Mode.Default, async (botClient, chatId, user, args) => {
                user.CurrentPath = Constants.RK_Corps;
                dbContext.SaveChanges();
                await botClient.SendTextMessageAsync(chatId: chatId, text: "Выберите корпус, и я покажу где он на карте", replyMarkup: CorpsKeyboardMarkup);
            });

            Corps[] corps = new Corps[]{new Constants.RK_FOC(), new Constants.RK_1Corps(), new Constants.RK_2Corps(), new Constants.RK_3Corps(), new Constants.RK_4Corps(), new Constants.RK_5Corps(), new Constants.RK_6Corps(),
                                        new Constants.RK_7Corps(), new Constants.RK_8Corps(), new Constants.RK_9Corps(), new Constants.RK_10Corps(), new Constants.RK_11Corps(), new Constants.RK_12Corps(),
                                        new Constants.RK_13Corps(), new Constants.RK_Stadium(), new Constants.RK_MainCorps(), new Constants.RK_PoolOnBoldin(), new Constants.LaboratoryCorps(),
                                        new Constants.RK_SanatoriumDispensary(), new Constants.RK_SportsComplexOnBoldin()
                                        };

            foreach(var item in corps) {
                commandManager.AddMessageCommand(item.Text, Mode.Default, async (botClient, chatId, user, args) => {
                    await botClient.SendVenueAsync(chatId: chatId, latitude: item.Latitude, longitude: item.Longitude, title: item.Title, address: item.Address, replyMarkup: CorpsKeyboardMarkup);
                });
            }

            commandManager.AddMessageCommand(Constants.RK_TechnicalCollege, Mode.Default, async (botClient, chatId, user, args) => {
                await botClient.SendTextMessageAsync(chatId: chatId, text: "Технический колледж имени С.И. Мосина территориально расположен на трех площадках:", replyMarkup: CancelKeyboardMarkup);

                await botClient.SendVenueAsync(chatId: chatId, latitude: 54.200399f, longitude: 37.535350f, title: "", address: "поселок Мясново, 18-й проезд, 94", replyMarkup: CorpsKeyboardMarkup);
                await botClient.SendVenueAsync(chatId: chatId, latitude: 54.192146f, longitude: 37.588119f, title: "", address: "улица Вересаева, 12", replyMarkup: CorpsKeyboardMarkup);
                await botClient.SendVenueAsync(chatId: chatId, latitude: 54.199636f, longitude: 37.604477f, title: "", address: "улица Коминтерна, 21", replyMarkup: CorpsKeyboardMarkup);
            });
            #endregion
            #endregion

            Console.WriteLine("Запущен бот " + telegramBot.GetMeAsync().Result.FirstName + "\n");

            telegramBot.ReceiveAsync(
                HandleUpdateAsync,
                HandleError,
            new ReceiverOptions {
                AllowedUpdates = { },
                ThrowPendingUpdates = true
            },
            new CancellationTokenSource().Token
           ).Wait();
        }

        private CommandManager.MessageFunction SetDefaultMode(ScheduleDbContext dbContext) => async (botClient, chatId, user, args) => {
            user.Mode = Mode.Default;
            dbContext.SaveChanges();

            await botClient.SendTextMessageAsync(chatId: chatId, text: Constants.RK_Profile, replyMarkup: GetProfileKeyboardMarkup(user));
        };

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
#if DEBUG
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update) + "\n");
#endif
            if(update.Type == Telegram.Bot.Types.Enums.UpdateType.InlineQuery) {
                await InlineQuery(botClient, update);
                return;
            }

            Message? message = update.Message ?? update.EditedMessage ?? update.CallbackQuery?.Message;

            if(message is not null) {
                if(message.From is null) return;

                TelegramUser? user = dbContext.TelegramUsers.Include(u => u.ScheduleProfile).FirstOrDefault(u => u.ChatID == message.Chat.Id);

                if(user is null) {
                    ScheduleProfile scheduleProfile = new ScheduleProfile(){ OwnerID = message.Chat.Id };
                    dbContext.ScheduleProfile.Add(scheduleProfile);

                    user = new() { ChatID = message.Chat.Id, FirstName = message.From.FirstName, Username = message.From.Username, LastName = message.From.LastName, ScheduleProfile = scheduleProfile };

                    dbContext.TelegramUsers.Add(user);
                    dbContext.SaveChanges();
                }

                switch(update.Type) {
                    case Telegram.Bot.Types.Enums.UpdateType.Message:
                    case Telegram.Bot.Types.Enums.UpdateType.EditedMessage:
                        if(message.Text is null) return;

                        await commandManager.OnMessageAsync(message.Chat, message.Text, user);
                        break;

                    case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:
                        if(update.CallbackQuery?.Data is null || message.Text is null) return;

                        await commandManager.OnCallbackAsync(message.Chat, message.MessageId, update.CallbackQuery.Data, message.Text, user);
                        break;
                }

                user.LastUpdate = DateTime.UtcNow;
                user.TodayRequests++;
                user.TotalRequests++;

                dbContext.SaveChanges();
            }
        }

        private Task HandleError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}