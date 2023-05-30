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
                    return $"{message} {user.Mode.ToString()}";

                var match = TermsMessageRegex().Match(message);
                if(match.Success) {
                    args = match.Groups[1].ToString();
                    return $"{match.Groups[2]} {user.Mode.ToString()}";
                }

                match = GroupOrStudentIDMessageRegex().Match(message);
                if(match.Success)
                    return $"{match.Groups[1]} {user.Mode.ToString()}";

                match = CommandMessageRegex().Match(message);
                if(match.Success) {
                    args = match.Groups[2].ToString();
                    return $"{match.Groups[1]} {user.Mode.ToString()}";
                }

                return $"{message} {user.Mode.ToString()}";
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
                await botClient.SendTextMessageAsync(chatId: chatId, text: scheduler.GetScheduleByDate(DateOnly.FromDateTime(DateTime.Now), user.ScheduleProfile), replyMarkup: user.IsAdmin() ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
            });
            commandManager.AddMessageCommand(Constants.RK_Tomorrow, Mode.Default, async (botClient, chatId, user, args) => {
                await ScheduleRelevance(botClient, chatId, MainKeyboardMarkup);
                await botClient.SendTextMessageAsync(chatId: chatId, text: scheduler.GetScheduleByDate(DateOnly.FromDateTime(DateTime.Now.AddDays(1)), user.ScheduleProfile), replyMarkup: user.IsAdmin() ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(Constants.RK_ByDays, Mode.Default, async (botClient, chatId, user, args) => {
                await ScheduleRelevance(botClient, chatId, DaysKeyboardMarkup);
            });
            commandManager.AddMessageCommand(Constants.RK_Monday, Mode.Default, async (botClient, chatId, user, args) => {
                foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Monday, user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: user.IsAdmin() ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(Constants.RK_Tuesday, Mode.Default, async (botClient, chatId, user, args) => {
                foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Tuesday, user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: user.IsAdmin() ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(Constants.RK_Wednesday, Mode.Default, async (botClient, chatId, user, args) => {
                foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Wednesday, user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: user.IsAdmin() ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(Constants.RK_Thursday, Mode.Default, async (botClient, chatId, user, args) => {
                foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Thursday, user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: user.IsAdmin() ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(Constants.RK_Friday, Mode.Default, async (botClient, chatId, user, args) => {
                foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Friday, user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: user.IsAdmin() ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(Constants.RK_Saturday, Mode.Default, async (botClient, chatId, user, args) => {
                foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Saturday, user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: user.IsAdmin() ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(Constants.RK_ForAWeek, Mode.Default, async (botClient, chatId, user, args) => {
                await ScheduleRelevance(botClient, chatId, WeekKeyboardMarkup);
                await botClient.SendTextMessageAsync(chatId: chatId, text: Constants.RK_ForAWeek, replyMarkup: WeekKeyboardMarkup);
            });
            commandManager.AddMessageCommand(Constants.RK_ThisWeek, Mode.Default, async (botClient, chatId, user, args) => {
                foreach(var item in scheduler.GetScheduleByWeak(CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, System.DayOfWeek.Monday) - 1, user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: item, replyMarkup: user.IsAdmin() ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(Constants.RK_NextWeek, Mode.Default, async (botClient, chatId, user, args) => {
                foreach(var item in scheduler.GetScheduleByWeak(CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, System.DayOfWeek.Monday), user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: item, replyMarkup: user.IsAdmin() ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
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
                        await botClient.SendTextMessageAsync(chatId: chatId, text: scheduler.GetScheduleByDate(date, user.ScheduleProfile), replyMarkup: user.IsAdmin() ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

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
                await GroupСhangeMessageMode(botClient, chatId, args, user);
                return true;
            });
            commandManager.AddMessageCommand(Mode.StudentIDСhange, async (botClient, chatId, user, args) => {
                await StudentIDСhangeMessageMode(botClient, chatId, args, user);
                return true;
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

            foreach(var item in corps)
                commandManager.AddMessageCommand(item.Text, Mode.Default, async (botClient, chatId, user, args) => {
                    await botClient.SendVenueAsync(chatId: chatId, latitude: item.Latitude, longitude: item.Longitude, title: item.Title, address: item.Address, replyMarkup: CorpsKeyboardMarkup);
                });

            commandManager.AddMessageCommand(Constants.RK_TechnicalCollege, Mode.Default, async (botClient, chatId, user, args) => {
                await botClient.SendTextMessageAsync(chatId: chatId, text: "Технический колледж имени С.И. Мосина территориально расположен на трех площадках:", replyMarkup: CancelKeyboardMarkup);

                await botClient.SendVenueAsync(chatId: chatId, latitude: 54.200399f, longitude: 37.535350f, title: "", address: "поселок Мясново, 18-й проезд, 94", replyMarkup: CorpsKeyboardMarkup);
                await botClient.SendVenueAsync(chatId: chatId, latitude: 54.192146f, longitude: 37.588119f, title: "", address: "улица Вересаева, 12", replyMarkup: CorpsKeyboardMarkup);
                await botClient.SendVenueAsync(chatId: chatId, latitude: 54.199636f, longitude: 37.604477f, title: "", address: "улица Коминтерна, 21", replyMarkup: CorpsKeyboardMarkup);
            });
            #endregion
            #endregion

            Console.WriteLine("Запущен бот " + telegramBot.GetMeAsync().Result.FirstName);

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

        private CommandManager.Function SetDefaultMode(ScheduleDbContext dbContext) => async (botClient, chatId, user, args) => {
            user.Mode = Mode.Default;
            dbContext.SaveChanges();

            await botClient.SendTextMessageAsync(chatId: chatId, text: Constants.RK_Profile, replyMarkup: GetProfileKeyboardMarkup(user));
        };

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
#if DEBUG
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
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

                bool processed = false;

                switch(update.Type) {
                    case Telegram.Bot.Types.Enums.UpdateType.Message:
                    case Telegram.Bot.Types.Enums.UpdateType.EditedMessage:
                        processed = await commandManager.OnMessageAsync(message.Chat, message.Text, user);

                        break;
                }

                if(!processed) {
                    switch(update.Type) {
                        case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:

                            switch(user.Mode) {
                                case Mode.Default:

                                    if(update.CallbackQuery?.Data is null) return;

                                    await DefaultCallbackModeAsync(message, botClient, user, update.CallbackQuery.Data);
                                    break;

                                case Mode.AddingDiscipline:

                                    if(update.CallbackQuery?.Data is null) return;

                                    await AddingDisciplineCallbackModeAsync(message, botClient, user, update.CallbackQuery.Data);
                                    break;
                            }
                            break;
                    }
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