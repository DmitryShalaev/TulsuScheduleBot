using System.Globalization;

using Microsoft.EntityFrameworkCore;

using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace ScheduleBot.Bot {
    public partial class TelegramBot {
        private readonly ITelegramBotClient botClient;
        private readonly CommandManager commandManager;
        private readonly Parser parser;

        public TelegramBot() {
            botClient = new TelegramBotClient(Environment.GetEnvironmentVariable("TelegramBotToken") ?? "");

            parser = new(commands, UpdatedDisciplinesAsync);

            commandManager = new(this, (string message, TelegramUser user, out string args) => {
                args = "";

                if(DefaultMessageRegex().IsMatch(message))
                    return $"{message} {user.Mode}".ToLower();

                var match = TermsMessageRegex().Match(message);
                if(match.Success) {
                    args = match.Groups[1].ToString();
                    return $"{match.Groups[2]} {user.Mode}".ToLower();
                }

                match = GroupOrStudentIDMessageRegex().Match(message);
                if(match.Success)
                    return $"{match.Groups[1]} {user.Mode}".ToLower();

                match = CommandMessageRegex().Match(message);
                if(match.Success) {
                    args = match.Groups[2].ToString();
                    return $"{match.Groups[1]} {user.Mode}".ToLower();
                }

                return $"{message} {user.Mode}".ToLower();

            }, (string message, TelegramUser user, out string args) => {
                args = "";

                var match = DisciplineCallbackRegex().Match(message);
                if(match.Success) {
                    args = match.Groups[2].ToString();
                    return $"{match.Groups[1]} {user.Mode}".ToLower();
                }

                match = NotificationsCallbackRegex().Match(message);
                if(match.Success) {
                    args = match.Groups[2].ToString();
                    return $"{match.Groups[1]} {user.Mode}".ToLower();
                }

                return $"{message} {user.Mode}".ToLower();
            });

            #region Message
            #region Main
            commandManager.AddMessageCommand("/start", Mode.Default, async (dbContext, chatId, user, args) => {
                await botClient.SendTextMessageAsync(chatId: chatId, text: "👋", replyMarkup: MainKeyboardMarkup);

                if(string.IsNullOrWhiteSpace(user.ScheduleProfile.Group)) {
                    user.Mode = Mode.GroupСhange;
                    dbContext.SaveChanges();

                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Для начала работы с ботом необходимо указать номер учебной группы", replyMarkup: CancelKeyboardMarkup);
                }
            });
            commandManager.AddMessageCommand("/SetProfile", Mode.Default, async (dbContext, chatId, user, args) => {
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

            commandManager.AddMessageCommand(new[] { commands.Message["Back"], commands.Message["Cancel"] }, Mode.Default, async (dbContext, chatId, user, args) => {
                if(user.CurrentPath == commands.Message["AcademicPerformance"] ||
                    user.CurrentPath == commands.Message["Profile"] ||
                    user.CurrentPath == commands.Message["Corps"] ||
                    user.CurrentPath == commands.Message["Exam"]) {

                    await botClient.SendTextMessageAsync(chatId: chatId, text: commands.Message["Other"], replyMarkup: AdditionalKeyboardMarkup);
                } else {
                    await botClient.SendTextMessageAsync(chatId: chatId, text: commands.Message["MainMenu"], replyMarkup: MainKeyboardMarkup);
                }

                user.CurrentPath = null;
                dbContext.SaveChanges();
            });
            commandManager.AddMessageCommand(commands.Message["Cancel"], Mode.AddingDiscipline, async (dbContext, chatId, user, args) => {
                var tmp = dbContext.CustomDiscipline.Where(i => !i.IsAdded && i.ScheduleProfile == user.ScheduleProfile).OrderByDescending(i => i.AddDate).First();

                user.Mode = Mode.Default;
                user.CurrentPath = null;
                dbContext.CustomDiscipline.Remove(tmp);
                dbContext.SaveChanges();

                await botClient.SendTextMessageAsync(chatId: chatId, text: commands.Message["MainMenu"], replyMarkup: MainKeyboardMarkup);
                await botClient.SendTextMessageAsync(chatId: chatId, text: Scheduler.GetScheduleByDate(dbContext, tmp.Date, user.ScheduleProfile, true), replyMarkup: GetEditAdminInlineKeyboardButton(dbContext, tmp.Date, user.ScheduleProfile));
            });
            commandManager.AddMessageCommand(commands.Message["Cancel"], new[] { Mode.GroupСhange, Mode.StudentIDСhange, Mode.ResetProfileLink }, async (dbContext, chatId, user, args) => {
                user.Mode = Mode.Default;
                dbContext.SaveChanges();

                await botClient.SendTextMessageAsync(chatId: chatId, text: commands.Message["Profile"], replyMarkup: GetProfileKeyboardMarkup(user));
            });
            commandManager.AddMessageCommand(commands.Message["Cancel"], new[] { Mode.CustomEditName, Mode.CustomEditLecturer, Mode.CustomEditLectureHall, Mode.CustomEditType, Mode.CustomEditStartTime, Mode.CustomEditEndTime }, async (dbContext, chatId, user, args) => {
                await botClient.SendTextMessageAsync(chatId: chatId, text: commands.Message["MainMenu"], replyMarkup: MainKeyboardMarkup);

                if(!string.IsNullOrWhiteSpace(user.CurrentPath)) {
                    if(user.IsOwner()) {
                        var discipline = dbContext.CustomDiscipline.Single(i => i.ID == uint.Parse(user.CurrentPath));
                        await botClient.SendTextMessageAsync(chatId: chatId, text: Scheduler.GetScheduleByDate(dbContext, discipline.Date, user.ScheduleProfile, true), replyMarkup: GetCustomEditAdminInlineKeyboardButton(discipline));
                    }
                }

                user.Mode = Mode.Default;
                user.CurrentPath = null;
                dbContext.SaveChanges();
            });
            commandManager.AddMessageCommand(commands.Message["Cancel"], Mode.DaysNotifications, async (dbContext, chatId, user, args) => {
                user.Mode = Mode.Default;
                dbContext.SaveChanges();

                await botClient.SendTextMessageAsync(chatId: chatId, text: commands.Message["Profile"], replyMarkup: GetProfileKeyboardMarkup(user));
                await botClient.SendTextMessageAsync(chatId: chatId, text: commands.Message["NotificationSettings"], replyMarkup: GetNotificationsInlineKeyboardButton(user));
            });

            commandManager.AddMessageCommand(commands.Message["Today"], Mode.Default, async (dbContext, chatId, user, args) => {
                await ScheduleRelevance(dbContext, botClient, chatId, user.ScheduleProfile.Group ?? throw new NullReferenceException("Group"), MainKeyboardMarkup);
                var date = DateOnly.FromDateTime(DateTime.Now);
                await botClient.SendTextMessageAsync(chatId: chatId, text: Scheduler.GetScheduleByDate(dbContext, date, user.ScheduleProfile), replyMarkup: GetInlineKeyboardButton(date, user));
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(commands.Message["Tomorrow"], Mode.Default, async (dbContext, chatId, user, args) => {
                await ScheduleRelevance(dbContext, botClient, chatId, user.ScheduleProfile.Group ?? throw new NullReferenceException("Group"), MainKeyboardMarkup);
                var date = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
                await botClient.SendTextMessageAsync(chatId: chatId, text: Scheduler.GetScheduleByDate(dbContext, date, user.ScheduleProfile), replyMarkup: GetInlineKeyboardButton(date, user));
            }, CommandManager.Check.group);

            commandManager.AddMessageCommand(commands.Message["ByDays"], Mode.Default, async (dbContext, chatId, user, args) => {
                await botClient.SendTextMessageAsync(chatId: chatId, text: commands.Message["ByDays"], replyMarkup: DaysKeyboardMarkup);
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(commands.Message["Monday"], Mode.Default, async (dbContext, chatId, user, args) => {
                await ScheduleRelevance(dbContext, botClient, chatId, user.ScheduleProfile.Group ?? throw new NullReferenceException("Group"), DaysKeyboardMarkup);
                foreach(var day in Scheduler.GetScheduleByDay(dbContext, System.DayOfWeek.Monday, user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: day.Item1, replyMarkup: GetInlineKeyboardButton(day.Item2, user));
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(commands.Message["Tuesday"], Mode.Default, async (dbContext, chatId, user, args) => {
                await ScheduleRelevance(dbContext, botClient, chatId, user.ScheduleProfile.Group ?? throw new NullReferenceException("Group"), DaysKeyboardMarkup);
                foreach(var day in Scheduler.GetScheduleByDay(dbContext, System.DayOfWeek.Tuesday, user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: day.Item1, replyMarkup: GetInlineKeyboardButton(day.Item2, user));
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(commands.Message["Wednesday"], Mode.Default, async (dbContext, chatId, user, args) => {
                await ScheduleRelevance(dbContext, botClient, chatId, user.ScheduleProfile.Group ?? throw new NullReferenceException("Group"), DaysKeyboardMarkup);
                foreach(var day in Scheduler.GetScheduleByDay(dbContext, System.DayOfWeek.Wednesday, user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: day.Item1, replyMarkup: GetInlineKeyboardButton(day.Item2, user));
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(commands.Message["Thursday"], Mode.Default, async (dbContext, chatId, user, args) => {
                await ScheduleRelevance(dbContext, botClient, chatId, user.ScheduleProfile.Group ?? throw new NullReferenceException("Group"), DaysKeyboardMarkup);
                foreach(var day in Scheduler.GetScheduleByDay(dbContext, System.DayOfWeek.Thursday, user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: day.Item1, replyMarkup: GetInlineKeyboardButton(day.Item2, user));
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(commands.Message["Friday"], Mode.Default, async (dbContext, chatId, user, args) => {
                await ScheduleRelevance(dbContext, botClient, chatId, user.ScheduleProfile.Group ?? throw new NullReferenceException("Group"), DaysKeyboardMarkup);
                foreach(var day in Scheduler.GetScheduleByDay(dbContext, System.DayOfWeek.Friday, user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: day.Item1, replyMarkup: GetInlineKeyboardButton(day.Item2, user));
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(commands.Message["Saturday"], Mode.Default, async (dbContext, chatId, user, args) => {
                await ScheduleRelevance(dbContext, botClient, chatId, user.ScheduleProfile.Group ?? throw new NullReferenceException("Group"), DaysKeyboardMarkup);
                foreach(var day in Scheduler.GetScheduleByDay(dbContext, System.DayOfWeek.Saturday, user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: day.Item1, replyMarkup: GetInlineKeyboardButton(day.Item2, user));
            }, CommandManager.Check.group);

            commandManager.AddMessageCommand(commands.Message["ForAWeek"], Mode.Default, async (dbContext, chatId, user, args) => {
                await botClient.SendTextMessageAsync(chatId: chatId, text: commands.Message["ForAWeek"], replyMarkup: WeekKeyboardMarkup);
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(commands.Message["ThisWeek"], Mode.Default, async (dbContext, chatId, user, args) => {
                await ScheduleRelevance(dbContext, botClient, chatId, user.ScheduleProfile.Group ?? throw new NullReferenceException("Group"), WeekKeyboardMarkup);
                foreach(var item in Scheduler.GetScheduleByWeak(dbContext, CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, System.DayOfWeek.Monday) - 1, user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: item.Item1, replyMarkup: GetInlineKeyboardButton(item.Item2, user));
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(commands.Message["NextWeek"], Mode.Default, async (dbContext, chatId, user, args) => {
                await ScheduleRelevance(dbContext, botClient, chatId, user.ScheduleProfile.Group ?? throw new NullReferenceException("Group"), WeekKeyboardMarkup);
                foreach(var item in Scheduler.GetScheduleByWeak(dbContext, CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, System.DayOfWeek.Monday), user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: item.Item1, replyMarkup: GetInlineKeyboardButton(item.Item2, user));
            }, CommandManager.Check.group);

            commandManager.AddMessageCommand(commands.Message["Exam"], Mode.Default, async (dbContext, chatId, user, args) => {
                user.CurrentPath = commands.Message["Exam"];
                dbContext.SaveChanges();

                await botClient.SendTextMessageAsync(chatId: chatId, text: commands.Message["Exam"], replyMarkup: ExamKeyboardMarkup);
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(commands.Message["AllExams"], Mode.Default, async (dbContext, chatId, user, args) => {
                await ScheduleRelevance(dbContext, botClient, chatId, user.ScheduleProfile.Group ?? throw new NullReferenceException("Group"), ExamKeyboardMarkup);
                foreach(var item in Scheduler.GetExamse(dbContext, user.ScheduleProfile, true))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: item, replyMarkup: ExamKeyboardMarkup);
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(commands.Message["NextExam"], Mode.Default, async (dbContext, chatId, user, args) => {
                await ScheduleRelevance(dbContext, botClient, chatId, user.ScheduleProfile.Group ?? throw new NullReferenceException("Group"), ExamKeyboardMarkup);
                foreach(var item in Scheduler.GetExamse(dbContext, user.ScheduleProfile, false))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: item, replyMarkup: ExamKeyboardMarkup);
            }, CommandManager.Check.group);

            commandManager.AddMessageCommand(commands.Message["Other"], Mode.Default, async (dbContext, chatId, user, args) => {
                await botClient.SendTextMessageAsync(chatId: chatId, text: commands.Message["Other"], replyMarkup: AdditionalKeyboardMarkup);
            });

            commandManager.AddMessageCommand(commands.Message["AcademicPerformance"], Mode.Default, async (dbContext, chatId, user, args) => {
                user.CurrentPath = commands.Message["AcademicPerformance"];
                dbContext.SaveChanges();

                var StudentID = user.ScheduleProfile.StudentID ?? throw new NullReferenceException("StudentID");

                await ProgressRelevance(dbContext, botClient, chatId, StudentID, null, false);
                await botClient.SendTextMessageAsync(chatId: chatId, text: commands.Message["AcademicPerformance"], replyMarkup: GetTermsKeyboardMarkup(dbContext, StudentID));
            }, CommandManager.Check.studentId);
            commandManager.AddMessageCommand(commands.Message["Semester"], Mode.Default, async (dbContext, chatId, user, args) => {
                var StudentID = user.ScheduleProfile.StudentID ?? throw new NullReferenceException("StudentID");

                await ProgressRelevance(dbContext, botClient, chatId, StudentID, GetTermsKeyboardMarkup(dbContext, StudentID));
                await botClient.SendTextMessageAsync(chatId: chatId, text: Scheduler.GetProgressByTerm(dbContext, int.Parse(args), StudentID), replyMarkup: GetTermsKeyboardMarkup(dbContext, StudentID));
            }, CommandManager.Check.studentId);

            commandManager.AddMessageCommand(commands.Message["Profile"], Mode.Default, async (dbContext, chatId, user, args) => {
                user.CurrentPath = commands.Message["Profile"];
                dbContext.SaveChanges();
                await botClient.SendTextMessageAsync(chatId: chatId, text: commands.Message["Profile"], replyMarkup: GetProfileKeyboardMarkup(user));
            });
            commandManager.AddMessageCommand(commands.Message["GetProfileLink"], Mode.Default, async (dbContext, chatId, user, args) => {
                if(user.IsOwner()) {
                    await botClient.SendTextMessageAsync(chatId: chatId, text: $"Если вы хотите поделиться своим расписанием с кем-то, просто отправьте им следующую команду: " +
                    $"\n`/SetProfile {user.ScheduleProfileGuid}`" +
                    $"\nЕсли другой пользователь введет эту команду, он сможет видеть расписание с вашими изменениями.", replyMarkup: GetProfileKeyboardMarkup(user), parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                } else {
                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Поделиться профилем может только его владелец!", replyMarkup: MainKeyboardMarkup);
                }
            });
            commandManager.AddMessageCommand(commands.Message["ResetProfileLink"], Mode.Default, async (dbContext, chatId, user, args) => {
                if(!user.IsOwner()) {
                    user.Mode = Mode.ResetProfileLink;
                    dbContext.SaveChanges();
                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Вы точно уверены что хотите восстановить свой профиль?", replyMarkup: ResetProfileLinkKeyboardMarkup);
                } else {
                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Владельцу профиля нет смысла его восстанавливать!", replyMarkup: MainKeyboardMarkup);
                }
            });
            commandManager.AddMessageCommand(commands.Message["Reset"], Mode.ResetProfileLink, async (dbContext, chatId, user, args) => {
                user.Mode = Mode.Default;

                var profile = dbContext.ScheduleProfile.FirstOrDefault(i => i.OwnerID == user.ChatID);
                if(profile is not null) {
                    user.ScheduleProfile = profile;
                } else {
                    profile = new() { TelegramUser = user };
                    dbContext.ScheduleProfile.Add(profile);
                    user.ScheduleProfile = profile;
                }

                dbContext.SaveChanges();

                await botClient.SendTextMessageAsync(chatId: chatId, text: commands.Message["Profile"], replyMarkup: GetProfileKeyboardMarkup(user));
            });

            commandManager.AddMessageCommand(commands.Message["GroupNumber"], Mode.Default, async (dbContext, chatId, user, args) => {
                if(user.IsOwner()) {
                    user.Mode = Mode.GroupСhange;
                    dbContext.SaveChanges();

                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Хотите сменить номер учебной группы? Если да, то напишите новый номер", replyMarkup: CancelKeyboardMarkup);
                }
            });
            commandManager.AddMessageCommand(commands.Message["StudentIDNumber"], Mode.Default, async (dbContext, chatId, user, args) => {
                if(user.IsOwner()) {
                    user.Mode = Mode.StudentIDСhange;
                    dbContext.SaveChanges();

                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Хотите сменить номер зачётки? Если да, то напишите новый номер", replyMarkup: CancelKeyboardMarkup);
                }
            });
            commandManager.AddMessageCommand(commands.Message["Notifications"], Mode.Default, async (dbContext, chatId, user, args) => {
                await botClient.SendTextMessageAsync(chatId: chatId, text: commands.Message["NotificationSettings"], replyMarkup: GetNotificationsInlineKeyboardButton(user));
            });

            commandManager.AddMessageCommand(Mode.Default, async (dbContext, chatId, user, args) => {
                if(DateRegex().IsMatch(args)) {
                    try {
                        DateOnly date;
                        if(DateTime.TryParse(args, out var _date))
                            date = DateOnly.FromDateTime(_date);
                        else
                            date = DateOnly.FromDateTime(DateTime.Parse($"{args} {DateTime.Now.Month}"));

                        await ScheduleRelevance(dbContext, botClient, chatId, user.ScheduleProfile.Group ?? "", MainKeyboardMarkup);
                        await botClient.SendTextMessageAsync(chatId: chatId, text: Scheduler.GetScheduleByDate(dbContext, date, user.ScheduleProfile), replyMarkup: GetInlineKeyboardButton(date, user));
                    } catch(Exception) {
                        await botClient.SendTextMessageAsync(chatId: chatId, text: $"Команда распознана как дата, но не соответствует формату \"день месяц год\".\nНапример: \"1 мая 2023\", \"1 05 23\", \"1 5\", \"1\"", replyMarkup: MainKeyboardMarkup);
                    }
                    return true;
                }

                await botClient.SendTextMessageAsync(chatId: chatId, text: $"Команда не распознана пожалуйста используйте кнопки или укажите дату в формате \"день месяц год\".\nНапример: \"1 мая 2023\", \"1 05 23\", \"1 5\", \"1\"", replyMarkup: MainKeyboardMarkup);

                return false;
            }, CommandManager.Check.group);

            commandManager.AddMessageCommand(Mode.AddingDiscipline, async (dbContext, chatId, user, args) => {
                await SetStagesAddingDisciplineAsync(dbContext, botClient, chatId, args, user);
                return true;
            });

            commandManager.AddMessageCommand(Mode.GroupСhange, async (dbContext, chatId, user, args) => {
                if(args.Length > 15) {
                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Номер группы не может содержать более 15 символов.", replyMarkup: CancelKeyboardMarkup);
                    return false;
                }

                var messageId = (await botClient.SendTextMessageAsync(chatId: chatId, text: "Нужно подождать...", replyMarkup: CancelKeyboardMarkup)).MessageId;
                bool flag = dbContext.GroupLastUpdate.Select(i => i.Group).Contains(args);

                if(flag || parser.GetDates(args) is not null) {
                    user.Mode = Mode.Default;
                    user.ScheduleProfile.Group = args;
                    dbContext.SaveChanges();

                    await botClient.SendTextMessageAsync(chatId: chatId, text: $"Номер группы успешно изменен на {args} ", replyMarkup: GetProfileKeyboardMarkup(user));

                    if(!flag)
                        parser.UpdatingDisciplines(dbContext, args);
                } else {
                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Сайт ТулГУ не отвечает или такой группы не существует", replyMarkup: CancelKeyboardMarkup);
                }

                await botClient.DeleteMessageAsync(chatId: chatId, messageId: messageId);
                return true;
            });
            commandManager.AddMessageCommand(Mode.StudentIDСhange, async (dbContext, chatId, user, args) => {
                if(args.Length > 10) {
                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Номер зачетки не может содержать более 10 символов.", replyMarkup: CancelKeyboardMarkup);
                    return false;
                }

                var messageId = (await botClient.SendTextMessageAsync(chatId: chatId, text: "Нужно подождать...", replyMarkup: CancelKeyboardMarkup)).MessageId;

                if(int.TryParse(args, out int studentID)) {
                    bool flag = dbContext.StudentIDLastUpdate.Select(i => i.StudentID).Contains(args);

                    if(flag || parser.GetProgress(args) is not null) {
                        user.Mode = Mode.Default;
                        user.ScheduleProfile.StudentID = studentID.ToString();
                        dbContext.SaveChanges();

                        await botClient.SendTextMessageAsync(chatId: chatId, text: $"Номер зачётки успешно изменен на {args} ", replyMarkup: GetProfileKeyboardMarkup(user));

                        if(!flag)
                            parser.UpdatingProgress(dbContext, studentID.ToString());
                    } else {
                        await botClient.SendTextMessageAsync(chatId: chatId, text: "Сайт ТулГУ не отвечает или указан неверный номер зачётки", replyMarkup: CancelKeyboardMarkup);
                    }

                } else {
                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Не удалось распознать введенный номер зачётной книжки", replyMarkup: CancelKeyboardMarkup);
                }

                await botClient.DeleteMessageAsync(chatId: chatId, messageId: messageId);
                return true;
            });

            commandManager.AddMessageCommand(Mode.ResetProfileLink, async (dbContext, chatId, user, args) => {
                await botClient.SendTextMessageAsync(chatId: chatId, text: "Выберите один из представленных вариантов!", replyMarkup: ResetProfileLinkKeyboardMarkup);
                return true;
            });

            commandManager.AddMessageCommand(Mode.CustomEditName, async (dbContext, chatId, user, args) => {
                if(!string.IsNullOrWhiteSpace(user.CurrentPath)) {
                    var discipline = dbContext.CustomDiscipline.Single(i => i.ID == uint.Parse(user.CurrentPath));
                    discipline.Name = args;

                    user.Mode = Mode.Default;
                    user.CurrentPath = null;
                    dbContext.SaveChanges();

                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Название предмета успешно изменено.", replyMarkup: MainKeyboardMarkup);
                    await botClient.SendTextMessageAsync(chatId: chatId, text: Scheduler.GetScheduleByDate(dbContext, discipline.Date, user.ScheduleProfile, true), replyMarkup: GetCustomEditAdminInlineKeyboardButton(discipline));
                }

                return true;
            });
            commandManager.AddMessageCommand(Mode.CustomEditLecturer, async (dbContext, chatId, user, args) => {
                if(!string.IsNullOrWhiteSpace(user.CurrentPath)) {
                    var discipline = dbContext.CustomDiscipline.Single(i => i.ID == uint.Parse(user.CurrentPath));
                    discipline.Lecturer = args;

                    user.Mode = Mode.Default;
                    user.CurrentPath = null;
                    dbContext.SaveChanges();

                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Лектор успешно изменен.", replyMarkup: MainKeyboardMarkup);
                    await botClient.SendTextMessageAsync(chatId: chatId, text: Scheduler.GetScheduleByDate(dbContext, discipline.Date, user.ScheduleProfile, true), replyMarkup: GetCustomEditAdminInlineKeyboardButton(discipline));
                }

                return true;
            });
            commandManager.AddMessageCommand(Mode.CustomEditType, async (dbContext, chatId, user, args) => {
                if(!string.IsNullOrWhiteSpace(user.CurrentPath)) {
                    var discipline = dbContext.CustomDiscipline.Single(i => i.ID == uint.Parse(user.CurrentPath));
                    discipline.Type = args;

                    user.Mode = Mode.Default;
                    user.CurrentPath = null;
                    dbContext.SaveChanges();

                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Тип предмета успешно изменен.", replyMarkup: MainKeyboardMarkup);
                    await botClient.SendTextMessageAsync(chatId: chatId, text: Scheduler.GetScheduleByDate(dbContext, discipline.Date, user.ScheduleProfile, true), replyMarkup: GetCustomEditAdminInlineKeyboardButton(discipline));
                }

                return true;
            });
            commandManager.AddMessageCommand(Mode.CustomEditLectureHall, async (dbContext, chatId, user, args) => {
                if(!string.IsNullOrWhiteSpace(user.CurrentPath)) {
                    var discipline = dbContext.CustomDiscipline.Single(i => i.ID == uint.Parse(user.CurrentPath));
                    discipline.LectureHall = args;

                    user.Mode = Mode.Default;
                    user.CurrentPath = null;
                    dbContext.SaveChanges();

                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Аудитория успешно изменена.", replyMarkup: MainKeyboardMarkup);
                    await botClient.SendTextMessageAsync(chatId: chatId, text: Scheduler.GetScheduleByDate(dbContext, discipline.Date, user.ScheduleProfile, true), replyMarkup: GetCustomEditAdminInlineKeyboardButton(discipline));
                }

                return true;
            });
            commandManager.AddMessageCommand(Mode.CustomEditStartTime, async (dbContext, chatId, user, args) => {
                if(!string.IsNullOrWhiteSpace(user.CurrentPath)) {
                    var discipline = dbContext.CustomDiscipline.Single(i => i.ID == uint.Parse(user.CurrentPath));
                    try {
                        discipline.StartTime = ParseTime(args);
                        user.Mode = Mode.Default;
                        user.CurrentPath = null;
                        dbContext.SaveChanges();

                        await botClient.SendTextMessageAsync(chatId: chatId, text: "Время начала успешно изменен.", replyMarkup: MainKeyboardMarkup);
                        await botClient.SendTextMessageAsync(chatId: chatId, text: Scheduler.GetScheduleByDate(dbContext, discipline.Date, user.ScheduleProfile, true), replyMarkup: GetCustomEditAdminInlineKeyboardButton(discipline));
                    } catch(Exception) {
                        await botClient.SendTextMessageAsync(chatId: chatId, text: "Ошибка в формате времени!", replyMarkup: CancelKeyboardMarkup);
                    }
                }

                return true;
            });
            commandManager.AddMessageCommand(Mode.CustomEditEndTime, async (dbContext, chatId, user, args) => {
                if(!string.IsNullOrWhiteSpace(user.CurrentPath)) {
                    var discipline = dbContext.CustomDiscipline.Single(i => i.ID == uint.Parse(user.CurrentPath));
                    try {
                        discipline.EndTime = ParseTime(args);
                        user.Mode = Mode.Default;
                        user.CurrentPath = null;
                        dbContext.SaveChanges();

                        await botClient.SendTextMessageAsync(chatId: chatId, text: "Время конца успешно изменен.", replyMarkup: MainKeyboardMarkup);
                        await botClient.SendTextMessageAsync(chatId: chatId, text: Scheduler.GetScheduleByDate(dbContext, discipline.Date, user.ScheduleProfile, true), replyMarkup: GetCustomEditAdminInlineKeyboardButton(discipline));
                    } catch(Exception) {
                        await botClient.SendTextMessageAsync(chatId: chatId, text: "Ошибка в формате времени!", replyMarkup: CancelKeyboardMarkup);
                    }
                }

                return true;
            });

            commandManager.AddMessageCommand(Mode.DaysNotifications, async (dbContext, chatId, user, args) => {
                try {
                    user.Notifications.Days = Math.Abs(int.Parse(args));
                    user.Mode = Mode.Default;

                    dbContext.SaveChanges();

                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Количество дней успешно изменено.", replyMarkup: GetProfileKeyboardMarkup(user));
                    await botClient.SendTextMessageAsync(chatId: chatId, text: commands.Message["NotificationSettings"], replyMarkup: GetNotificationsInlineKeyboardButton(user));
                } catch(Exception) {
                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Ошибка в формате количества дней!", replyMarkup: CancelKeyboardMarkup);
                }

                return true;
            });

            commandManager.AddCallbackCommand(commands.Callback["Edit"].callback, Mode.Default, async (dbContext, chatId, messageId, user, message, args) => {
                if(DateOnly.TryParse(args, out DateOnly date)) {
                    if(user.IsOwner())
                        await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: Scheduler.GetScheduleByDate(dbContext, date, user.ScheduleProfile, true), replyMarkup: GetEditAdminInlineKeyboardButton(dbContext, date, user.ScheduleProfile));
                    else
                        await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: Scheduler.GetScheduleByDate(dbContext, date, user.ScheduleProfile), replyMarkup: GetInlineKeyboardButton(date, user));
                }
            }, CommandManager.Check.group);
            commandManager.AddCallbackCommand(commands.Callback["All"].callback, Mode.Default, async (dbContext, chatId, messageId, user, message, args) => {
                if(DateOnly.TryParse(args, out DateOnly date))
                    await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: Scheduler.GetScheduleByDate(dbContext, date, user.ScheduleProfile, true), replyMarkup: GetBackInlineKeyboardButton(date, user));
            }, CommandManager.Check.group);
            commandManager.AddCallbackCommand(commands.Callback["Back"].callback, Mode.Default, async (dbContext, chatId, messageId, user, message, args) => {
                if(DateOnly.TryParse(args, out DateOnly date))
                    await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: Scheduler.GetScheduleByDate(dbContext, date, user.ScheduleProfile), replyMarkup: GetInlineKeyboardButton(date, user));
            }, CommandManager.Check.group);
            commandManager.AddCallbackCommand(commands.Callback["Add"].callback, Mode.Default, async (dbContext, chatId, messageId, user, message, args) => {
                if(DateOnly.TryParse(args, out DateOnly date)) {
                    if(user.IsOwner()) {
                        user.Mode = Mode.AddingDiscipline;
                        user.CurrentPath = $"{messageId} {date}";
                        dbContext.CustomDiscipline.Add(new(user.ScheduleProfile, date));
                        dbContext.SaveChanges();
                        await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: Scheduler.GetScheduleByDate(dbContext, date, user.ScheduleProfile));
                        await botClient.SendTextMessageAsync(chatId: chatId, text: GetStagesAddingDiscipline(dbContext, user), replyMarkup: CancelKeyboardMarkup);
                    } else {
                        await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: Scheduler.GetScheduleByDate(dbContext, date, user.ScheduleProfile), replyMarkup: GetInlineKeyboardButton(date, user));
                    }
                }
            }, CommandManager.Check.group);

            commandManager.AddCallbackCommand(commands.Callback["SetEndTime"].callback, Mode.AddingDiscipline, async (dbContext, chatId, messageId, user, message, args) => {
                var temporaryAddition = dbContext.CustomDiscipline.Where(i => !i.IsAdded && i.ScheduleProfile == user.ScheduleProfile).OrderByDescending(i => i.AddDate).First();

                temporaryAddition.EndTime = TimeOnly.Parse(args);
                temporaryAddition.Counter++;

                await SaveAddingDisciplineAsync(dbContext, botClient, chatId, user, temporaryAddition);
            });

            commandManager.AddCallbackCommand("DisciplineDay", Mode.Default, async (dbContext, chatId, messageId, user, message, args) => {
                var tmp = args.Split('|');
                var discipline = dbContext.Disciplines.FirstOrDefault(i => i.ID == uint.Parse(tmp[0]));
                if(discipline is not null) {
                    if(user.IsOwner()) {
                        var completedDisciplines = dbContext.CompletedDisciplines.Where(i => i.ScheduleProfileGuid == user.ScheduleProfileGuid).ToList();

                        CompletedDiscipline dayTmp = new(discipline, user.ScheduleProfileGuid);
                        var dayCompletedDisciplines = completedDisciplines.FirstOrDefault(i => i.Equals(dayTmp));

                        if(dayCompletedDisciplines is not null)
                            dbContext.CompletedDisciplines.Remove(dayCompletedDisciplines);
                        else
                            dbContext.CompletedDisciplines.Add(dayTmp);

                        dbContext.SaveChanges();
                        await botClient.EditMessageReplyMarkupAsync(chatId: chatId, messageId: messageId, replyMarkup: GetEditAdminInlineKeyboardButton(dbContext, discipline.Date, user.ScheduleProfile));

                        return;
                    }
                }
                if(DateOnly.TryParse(tmp[1], out DateOnly date))
                    await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: Scheduler.GetScheduleByDate(dbContext, date, user.ScheduleProfile), replyMarkup: GetInlineKeyboardButton(date, user));
            }, CommandManager.Check.group);
            commandManager.AddCallbackCommand("DisciplineAlways", Mode.Default, async (dbContext, chatId, messageId, user, message, args) => {
                var tmp = args.Split('|');
                var discipline = dbContext.Disciplines.FirstOrDefault(i => i.ID == uint.Parse(tmp[0]));
                if(discipline is not null) {
                    if(user.IsOwner()) {
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
                        await botClient.EditMessageReplyMarkupAsync(chatId: chatId, messageId: messageId, replyMarkup: GetEditAdminInlineKeyboardButton(dbContext, discipline.Date, user.ScheduleProfile));
                        return;
                    }
                }
                if(DateOnly.TryParse(tmp[1], out DateOnly date))
                    await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: Scheduler.GetScheduleByDate(dbContext, date, user.ScheduleProfile), replyMarkup: GetInlineKeyboardButton(date, user));
            }, CommandManager.Check.group);

            commandManager.AddCallbackCommand("CustomDelete", Mode.Default, async (dbContext, chatId, messageId, user, message, args) => {
                var tmp = args.Split('|');
                var customDiscipline = dbContext.CustomDiscipline.FirstOrDefault(i => i.ID == uint.Parse(tmp[0]));
                if(customDiscipline is not null) {
                    if(user.IsOwner()) {
                        dbContext.CustomDiscipline.Remove(customDiscipline);
                        dbContext.SaveChanges();

                        await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: Scheduler.GetScheduleByDate(dbContext, customDiscipline.Date, user.ScheduleProfile), replyMarkup: GetEditAdminInlineKeyboardButton(dbContext, customDiscipline.Date, user.ScheduleProfile));
                        return;
                    }
                }
                if(DateOnly.TryParse(tmp[1], out DateOnly date))
                    await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: Scheduler.GetScheduleByDate(dbContext, date, user.ScheduleProfile), replyMarkup: GetInlineKeyboardButton(date, user));
            }, CommandManager.Check.group);
            commandManager.AddCallbackCommand(commands.Callback["CustomEditCancel"].callback, Mode.Default, async (dbContext, chatId, messageId, user, message, args) => {
                if(DateOnly.TryParse(args, out DateOnly date))
                    if(user.IsOwner())
                        await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: Scheduler.GetScheduleByDate(dbContext, date, user.ScheduleProfile), replyMarkup: GetEditAdminInlineKeyboardButton(dbContext, date, user.ScheduleProfile));
                    else
                        await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: Scheduler.GetScheduleByDate(dbContext, date, user.ScheduleProfile), replyMarkup: GetInlineKeyboardButton(date, user));
            }, CommandManager.Check.group);
            commandManager.AddCallbackCommand("CustomEdit", Mode.Default, async (dbContext, chatId, messageId, user, message, args) => {
                var tmp = args.Split('|');
                var customDiscipline = dbContext.CustomDiscipline.FirstOrDefault(i => i.ID == uint.Parse(tmp[0]));
                if(customDiscipline is not null) {
                    if(user.IsOwner()) {
                        await botClient.EditMessageReplyMarkupAsync(chatId: chatId, messageId: messageId, replyMarkup: GetCustomEditAdminInlineKeyboardButton(customDiscipline));
                        return;
                    }
                }
                if(DateOnly.TryParse(tmp[1], out DateOnly date))
                    await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: Scheduler.GetScheduleByDate(dbContext, date, user.ScheduleProfile), replyMarkup: GetInlineKeyboardButton(date, user));
            }, CommandManager.Check.group);
            commandManager.AddCallbackCommand("CustomEditName", Mode.Default, async (dbContext, chatId, messageId, user, message, args) => {
                await CustomEdit(dbContext, chatId, messageId, user, args, Mode.CustomEditName,
                "Хотите изменить название предмета? Если да, то напишите новое");
            });
            commandManager.AddCallbackCommand("CustomEditLecturer", Mode.Default, async (dbContext, chatId, messageId, user, message, args) => {
                await CustomEdit(dbContext, chatId, messageId, user, args, Mode.CustomEditLecturer,
                "Хотите изменить лектора? Если да, то напишите нового");
            });
            commandManager.AddCallbackCommand("CustomEditType", Mode.Default, async (dbContext, chatId, messageId, user, message, args) => {
                await CustomEdit(dbContext, chatId, messageId, user, args, Mode.CustomEditType,
                "Хотите изменить тип предмета? Если да, то напишите новый");
            });
            commandManager.AddCallbackCommand("CustomEditLectureHall", Mode.Default, async (dbContext, chatId, messageId, user, message, args) => {
                await CustomEdit(dbContext, chatId, messageId, user, args, Mode.CustomEditLectureHall,
                "Хотите изменить аудиторию? Если да, то напишите новую");
            });
            commandManager.AddCallbackCommand("CustomEditStartTime", Mode.Default, async (dbContext, chatId, messageId, user, message, args) => {
                await CustomEdit(dbContext, chatId, messageId, user, args, Mode.CustomEditStartTime,
                "Хотите изменить время начала пары? Если да, то напишите новое");
            });
            commandManager.AddCallbackCommand("CustomEditEndTime", Mode.Default, async (dbContext, chatId, messageId, user, message, args) => {
                await CustomEdit(dbContext, chatId, messageId, user, args, Mode.CustomEditEndTime,
                "Хотите изменить время конца пары? Если да, то напишите новое");
            });

            commandManager.AddCallbackCommand("ToggleNotifications", Mode.Default, async (dbContext, chatId, messageId, user, message, args) => {
                switch(args) {
                    case "on":
                        user.Notifications.IsEnabled = true;
                        break;
                    case "off":
                        user.Notifications.IsEnabled = false;
                        break;
                }

                dbContext.SaveChanges();

                await botClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: commands.Message["NotificationSettings"], replyMarkup: GetNotificationsInlineKeyboardButton(user));
            });
            commandManager.AddCallbackCommand("DaysNotifications", Mode.Default, async (dbContext, chatId, messageId, user, message, args) => {
                user.Mode = Mode.DaysNotifications;
                dbContext.SaveChanges();

                await botClient.DeleteMessageAsync(chatId: chatId, messageId: messageId);
                await botClient.SendTextMessageAsync(chatId: chatId, text: "Хотите изменить количество дней? Если да, то напишите новое", replyMarkup: CancelKeyboardMarkup);
            });

            #region Admin
            commandManager.AddMessageCommand("/GetJob", Mode.Default, async (dbContext, chatId, user, args) => {
                ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
                IScheduler scheduler = await schedulerFactory.GetScheduler();

                var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
                foreach(var jobKey in jobKeys) {
                    var triggers = await scheduler.GetTriggersOfJob(jobKey);
                    foreach(var trigger in triggers) {
                        var triggerState = await scheduler.GetTriggerState(trigger.Key);
                        await botClient.SendTextMessageAsync(chatId: chatId, text: $"Job {jobKey} with trigger {trigger.Key} is {triggerState}");
                    }
                }

            }, CommandManager.Check.admin);
            #endregion
            #endregion
            #region Corps
            commandManager.AddMessageCommand(commands.Message["Corps"], Mode.Default, async (dbContext, chatId, user, args) => {
                user.CurrentPath = commands.Message["Corps"];
                dbContext.SaveChanges();
                await botClient.SendTextMessageAsync(chatId: chatId, text: "Выберите корпус, и я покажу где он на карте", replyMarkup: CorpsKeyboardMarkup);
            });

            foreach(var item in commands.Corps) {
                commandManager.AddMessageCommand(item.text, Mode.Default, async (dbContext, chatId, user, args) => {
                    await botClient.SendVenueAsync(chatId: chatId, latitude: item.latitude, longitude: item.longitude, title: item.title, address: item.address, replyMarkup: CorpsKeyboardMarkup);
                });
            }

            commandManager.AddMessageCommand(commands.College.text, Mode.Default, async (dbContext, chatId, user, args) => {
                await botClient.SendTextMessageAsync(chatId: chatId, text: commands.College.title, replyMarkup: CancelKeyboardMarkup);

                foreach(var item in commands.College.corps)
                    await botClient.SendVenueAsync(chatId: chatId, latitude: item.latitude, longitude: item.longitude, title: "", address: item.address, replyMarkup: CorpsKeyboardMarkup);
            });
            #endregion
            #endregion

            commandManager.TrimExcess();

            Console.WriteLine("Запущен бот " + botClient.GetMeAsync().Result.FirstName + "\n");

            botClient.ReceiveAsync(
                HandleUpdateAsync,
                HandleError,
            new ReceiverOptions {
                AllowedUpdates = { },
#if DEBUG
                ThrowPendingUpdates = true
#else
                ThrowPendingUpdates = false
#endif
            },
            new CancellationTokenSource().Token
           );
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
#if DEBUG
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update) + "\n");
#endif
            Message? message = update.Message ?? update.EditedMessage ?? update.CallbackQuery?.Message;

            using(ScheduleDbContext dbContext = new()) {
                if(message is not null) {
                    if(message.From is null) return;

                    TelegramUser? user = dbContext.TelegramUsers.Include(u => u.ScheduleProfile).Include(u => u.Notifications).FirstOrDefault(u => u.ChatID == message.Chat.Id);

                    if(user is null) {
                        ScheduleProfile scheduleProfile = new();
                        dbContext.ScheduleProfile.Add(scheduleProfile);

                        Notifications notifications = new();
                        dbContext.Notifications.Add(notifications);
                        dbContext.SaveChanges();

                        user = new() {
                            ChatID = message.Chat.Id,
                            Username = message.From.Username,
                            FirstName = message.From.FirstName,
                            LastName = message.From.LastName,
                            ScheduleProfile = scheduleProfile,
                            Notifications = notifications
                        };

                        dbContext.TelegramUsers.Add(user);

                        notifications.TelegramUser = scheduleProfile.TelegramUser = user;

                        dbContext.SaveChanges();
                    }

                    switch(update.Type) {
                        case Telegram.Bot.Types.Enums.UpdateType.Message:
                        case Telegram.Bot.Types.Enums.UpdateType.EditedMessage:
                            if(message.Text is null) return;

                            await commandManager.OnMessageAsync(dbContext, message.Chat, message.Text, user);
                            dbContext.MessageLog.Add(new() { Message = message.Text, TelegramUser = user });
                            break;

                        case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:
                            if(update.CallbackQuery?.Data is null || message.Text is null) return;

                            await commandManager.OnCallbackAsync(dbContext, message.Chat, message.MessageId, update.CallbackQuery.Data, message.Text, user);
                            dbContext.MessageLog.Add(new() { Message = update.CallbackQuery.Data, TelegramUser = user });
                            break;
                    }

                    user.LastAppeal = user.ScheduleProfile.LastAppeal = DateTime.UtcNow;
                    user.TodayRequests++;
                    user.TotalRequests++;

                    dbContext.SaveChanges();
                } else {
                    if(update.Type == Telegram.Bot.Types.Enums.UpdateType.InlineQuery) {
                        await InlineQuery(dbContext, botClient, update);
                        return;
                    }
                }
            }
        }

        private Task HandleError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}