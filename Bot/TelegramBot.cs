using System.Globalization;

using Microsoft.EntityFrameworkCore;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace ScheduleBot.Bot {
    public partial class TelegramBot {
        private readonly ITelegramBotClient telegramBot;
        private readonly Scheduler.Scheduler scheduler;
        private readonly ScheduleDbContext dbContext;

        private readonly CommandManager commandManager;

        public TelegramBot(Scheduler.Scheduler scheduler, ScheduleDbContext dbContext) {
            this.scheduler = scheduler;
            this.dbContext = dbContext;

            telegramBot = new TelegramBotClient(Environment.GetEnvironmentVariable("TelegramBotToken") ?? "");

            commandManager = new(telegramBot);

            #region Main
            commandManager.AddMessageCommand("/start", Mode.Default, async (botClient, chatId, user) => {
                await botClient.SendTextMessageAsync(chatId: chatId, text: "👋", replyMarkup: MainKeyboardMarkup);

                if(string.IsNullOrWhiteSpace(user.ScheduleProfile.Group)) {
                    user.Mode = Mode.GroupСhange;
                    dbContext.SaveChanges();

                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Для начала работы с ботом необходимо указать номер учебной группы", replyMarkup: CancelKeyboardMarkup);
                }
            });
            commandManager.AddMessageCommand(new[] { Constants.RK_Back, Constants.RK_Cancel }, null, async (botClient, chatId, user) => {
                switch(user.Mode) {
                    case Mode.Default:
                        switch(user.CurrentPath) {
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
                        break;

                    case Mode.AddingDiscipline:
                        user.Mode = Mode.Default;
                        dbContext.TemporaryAddition.Remove(dbContext.TemporaryAddition.Where(i => i.TelegramUser == user).OrderByDescending(i => i.AddDate).First());
                        dbContext.SaveChanges();
                        await botClient.SendTextMessageAsync(chatId: chatId, text: Constants.RK_MainMenu, replyMarkup: MainKeyboardMarkup);
                        break;

                    default:
                        user.Mode = Mode.Default;
                        dbContext.SaveChanges();

                        await botClient.SendTextMessageAsync(chatId: chatId, text: Constants.RK_Profile, replyMarkup: GetProfileKeyboardMarkup(user));
                        break;
                }
            });
            commandManager.AddMessageCommand(Constants.RK_Today, Mode.Default, async (botClient, chatId, user) => {
                await ScheduleRelevance(botClient, chatId, MainKeyboardMarkup);
                await botClient.SendTextMessageAsync(chatId: chatId, text: scheduler.GetScheduleByDate(DateOnly.FromDateTime(DateTime.Now), user.ScheduleProfile), replyMarkup: user.IsAdmin() ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
            });
            commandManager.AddMessageCommand(Constants.RK_Tomorrow, Mode.Default, async (botClient, chatId, user) => {
                await ScheduleRelevance(botClient, chatId, MainKeyboardMarkup);
                await botClient.SendTextMessageAsync(chatId: chatId, text: scheduler.GetScheduleByDate(DateOnly.FromDateTime(DateTime.Now.AddDays(1)), user.ScheduleProfile), replyMarkup: user.IsAdmin() ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(Constants.RK_ByDays, Mode.Default, async (botClient, chatId, user) => {
                await ScheduleRelevance(botClient, chatId, DaysKeyboardMarkup);
            });
            commandManager.AddMessageCommand(Constants.RK_Monday, Mode.Default, async (botClient, chatId, user) => {
                foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Monday, user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: user.IsAdmin() ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(Constants.RK_Tuesday, Mode.Default, async (botClient, chatId, user) => {
                foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Tuesday, user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: user.IsAdmin() ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(Constants.RK_Wednesday, Mode.Default, async (botClient, chatId, user) => {
                foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Wednesday, user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: user.IsAdmin() ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(Constants.RK_Thursday, Mode.Default, async (botClient, chatId, user) => {
                foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Thursday, user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: user.IsAdmin() ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(Constants.RK_Friday, Mode.Default, async (botClient, chatId, user) => {
                foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Friday, user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: user.IsAdmin() ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(Constants.RK_Saturday, Mode.Default, async (botClient, chatId, user) => {
                foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Saturday, user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: user.IsAdmin() ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(Constants.RK_ForAWeek, Mode.Default, async (botClient, chatId, user) => {
                await ScheduleRelevance(botClient, chatId, WeekKeyboardMarkup);
                await botClient.SendTextMessageAsync(chatId: chatId, text: Constants.RK_ForAWeek, replyMarkup: WeekKeyboardMarkup);
            });
            commandManager.AddMessageCommand(Constants.RK_ThisWeek, Mode.Default, async (botClient, chatId, user) => {
                foreach(var item in scheduler.GetScheduleByWeak(CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, System.DayOfWeek.Monday) - 1, user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: item, replyMarkup: user.IsAdmin() ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(Constants.RK_NextWeek, Mode.Default, async (botClient, chatId, user) => {
                foreach(var item in scheduler.GetScheduleByWeak(CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, System.DayOfWeek.Monday), user.ScheduleProfile))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: item, replyMarkup: user.IsAdmin() ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(Constants.RK_Exam, Mode.Default, async (botClient, chatId, user) => {
                if(dbContext.Disciplines.Any(i => i.Group == user.ScheduleProfile.Group && i.Class == Class.other && i.Date >= DateOnly.FromDateTime(DateTime.Now)))
                    await ScheduleRelevance(botClient, chatId, replyMarkup: ExamKeyboardMarkup);
                else
                    await botClient.SendTextMessageAsync(chatId: chatId, text: "В расписании нет будущих экзаменов.", replyMarkup: MainKeyboardMarkup);
            }, CommandManager.Check.group);
            commandManager.AddMessageCommand(Constants.RK_AllExams, Mode.Default, async (botClient, chatId, user) => {
                foreach(var item in scheduler.GetExamse(user.ScheduleProfile, true))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: item, replyMarkup: ExamKeyboardMarkup);
            });
            commandManager.AddMessageCommand(Constants.RK_NextExam, Mode.Default, async (botClient, chatId, user) => {
                foreach(var item in scheduler.GetExamse(user.ScheduleProfile, false))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: item, replyMarkup: ExamKeyboardMarkup);
            });
            commandManager.AddMessageCommand(Constants.RK_AcademicPerformance, Mode.Default, async (botClient, chatId, user) => {
                user.CurrentPath = Constants.RK_AcademicPerformance;
                dbContext.SaveChanges();
                await ProgressRelevance(botClient, chatId, GetTermsKeyboardMarkup(user.ScheduleProfile.StudentID ?? throw new NullReferenceException("StudentID")));
            }, CommandManager.Check.studentId);
            commandManager.AddMessageCommand(Constants.RK_Profile, Mode.Default, async (botClient, chatId, user) => {
                user.CurrentPath = Constants.RK_Profile;
                dbContext.SaveChanges();
                await botClient.SendTextMessageAsync(chatId: chatId, text: Constants.RK_Profile, replyMarkup: GetProfileKeyboardMarkup(user));
            });
            commandManager.AddMessageCommand(Constants.RK_GetProfileLink, Mode.Default, async (botClient, chatId, user) => {
                if(user.IsAdmin()) {
                    await botClient.SendTextMessageAsync(chatId: chatId, text: $"Если вы хотите поделиться своим расписанием с кем-то, просто отправьте им следующую команду: " +
                    $"\n`/SetProfile {user.ScheduleProfileGuid}`" +
                    $"\nЕсли другой пользователь введет эту команду, он сможет видеть расписание с вашими изменениями.", replyMarkup: GetProfileKeyboardMarkup(user), parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                } else {
                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Поделиться профилем может только его владелец!", replyMarkup: MainKeyboardMarkup);
                }
            });
            commandManager.AddMessageCommand(Constants.RK_ResetProfileLink, Mode.Default, async (botClient, chatId, user) => {
                if(!user.IsAdmin()) {
                    user.Mode = Mode.ResetProfileLink;
                    dbContext.SaveChanges();
                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Вы точно уверены что хотите восстановить свой профиль?", replyMarkup: ResetProfileLinkKeyboardMarkup);
                } else {
                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Владельцу профиля нет смысла его восстанавливать!", replyMarkup: MainKeyboardMarkup);
                }
            });
            commandManager.AddMessageCommand(Constants.RK_Other, Mode.Default, async (botClient, chatId, user) => {
                await botClient.SendTextMessageAsync(chatId: chatId, text: Constants.RK_Other, replyMarkup: AdditionalKeyboardMarkup);
            });
            #endregion
            #region Corps
            commandManager.AddMessageCommand(Constants.RK_Corps, Mode.Default, async (botClient, chatId, user) => {
                user.CurrentPath = Constants.RK_Corps;
                dbContext.SaveChanges();
                await botClient.SendTextMessageAsync(chatId: chatId, text: "Выберите корпус, и я покажу где он на карте", replyMarkup: CorpsKeyboardMarkup);
            });
            commandManager.AddMessageCommand(Constants.RK_FOC.text, Mode.Default, async (botClient, chatId, user) => {
                await SendCorpsInfo(botClient, chatId, new Constants.RK_FOC());
            });
            commandManager.AddMessageCommand(Constants.RK_1Corps.text, Mode.Default, async (botClient, chatId, user) => {
                await SendCorpsInfo(botClient, chatId, new Constants.RK_1Corps());
            });
            commandManager.AddMessageCommand(Constants.RK_2Corps.text, Mode.Default, async (botClient, chatId, user) => {
                await SendCorpsInfo(botClient, chatId, new Constants.RK_2Corps());
            });
            commandManager.AddMessageCommand(Constants.RK_3Corps.text, Mode.Default, async (botClient, chatId, user) => {
                await SendCorpsInfo(botClient, chatId, new Constants.RK_3Corps());
            });
            commandManager.AddMessageCommand(Constants.RK_4Corps.text, Mode.Default, async (botClient, chatId, user) => {
                await SendCorpsInfo(botClient, chatId, new Constants.RK_4Corps());
            });
            commandManager.AddMessageCommand(Constants.RK_5Corps.text, Mode.Default, async (botClient, chatId, user) => {
                await SendCorpsInfo(botClient, chatId, new Constants.RK_5Corps());
            });
            commandManager.AddMessageCommand(Constants.RK_6Corps.text, Mode.Default, async (botClient, chatId, user) => {
                await SendCorpsInfo(botClient, chatId, new Constants.RK_6Corps());
            });
            commandManager.AddMessageCommand(Constants.RK_7Corps.text, Mode.Default, async (botClient, chatId, user) => {
                await SendCorpsInfo(botClient, chatId, new Constants.RK_7Corps());
            });
            commandManager.AddMessageCommand(Constants.RK_8Corps.text, Mode.Default, async (botClient, chatId, user) => {
                await SendCorpsInfo(botClient, chatId, new Constants.RK_8Corps());
            });
            commandManager.AddMessageCommand(Constants.RK_9Corps.text, Mode.Default, async (botClient, chatId, user) => {
                await SendCorpsInfo(botClient, chatId, new Constants.RK_9Corps());
            });
            commandManager.AddMessageCommand(Constants.RK_10Corps.text, Mode.Default, async (botClient, chatId, user) => {
                await SendCorpsInfo(botClient, chatId, new Constants.RK_10Corps());
            });
            commandManager.AddMessageCommand(Constants.RK_11Corps.text, Mode.Default, async (botClient, chatId, user) => {
                await SendCorpsInfo(botClient, chatId, new Constants.RK_11Corps());
            });
            commandManager.AddMessageCommand(Constants.RK_12Corps.text, Mode.Default, async (botClient, chatId, user) => {
                await SendCorpsInfo(botClient, chatId, new Constants.RK_12Corps());
            });
            commandManager.AddMessageCommand(Constants.RK_13Corps.text, Mode.Default, async (botClient, chatId, user) => {
                await SendCorpsInfo(botClient, chatId, new Constants.RK_13Corps());
            });
            commandManager.AddMessageCommand(Constants.RK_Stadium.text, Mode.Default, async (botClient, chatId, user) => {
                await SendCorpsInfo(botClient, chatId, new Constants.RK_Stadium());
            });
            commandManager.AddMessageCommand(Constants.RK_MainCorps.text, Mode.Default, async (botClient, chatId, user) => {
                await SendCorpsInfo(botClient, chatId, new Constants.RK_MainCorps());
            });
            commandManager.AddMessageCommand(Constants.RK_PoolOnBoldin.text, Mode.Default, async (botClient, chatId, user) => {
                await SendCorpsInfo(botClient, chatId, new Constants.RK_PoolOnBoldin());
            });
            commandManager.AddMessageCommand(Constants.LaboratoryCorps.text, Mode.Default, async (botClient, chatId, user) => {
                await SendCorpsInfo(botClient, chatId, new Constants.LaboratoryCorps());
            });
            commandManager.AddMessageCommand(Constants.RK_SanatoriumDispensary.text, Mode.Default, async (botClient, chatId, user) => {
                await SendCorpsInfo(botClient, chatId, new Constants.RK_SanatoriumDispensary());
            });
            commandManager.AddMessageCommand(Constants.RK_SportsComplexOnBoldin.text, Mode.Default, async (botClient, chatId, user) => {
                await SendCorpsInfo(botClient, chatId, new Constants.RK_SportsComplexOnBoldin());
            });
            commandManager.AddMessageCommand(Constants.RK_TechnicalCollege, Mode.Default, async (botClient, chatId, user) => {
                await botClient.SendTextMessageAsync(chatId: chatId, text: "Технический колледж имени С.И. Мосина территориально расположен на трех площадках:", replyMarkup: CancelKeyboardMarkup);

                await botClient.SendVenueAsync(chatId: chatId, latitude: 54.200399f, longitude: 37.535350f, title: "", address: "поселок Мясново, 18-й проезд, 94", replyMarkup: CorpsKeyboardMarkup);
                await botClient.SendVenueAsync(chatId: chatId, latitude: 54.192146f, longitude: 37.588119f, title: "", address: "улица Вересаева, 12", replyMarkup: CorpsKeyboardMarkup);
                await botClient.SendVenueAsync(chatId: chatId, latitude: 54.199636f, longitude: 37.604477f, title: "", address: "улица Коминтерна, 21", replyMarkup: CorpsKeyboardMarkup);
            });
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
                    switch(user.Mode) {
                        case Mode.Default:
                            switch(update.Type) {
                                case Telegram.Bot.Types.Enums.UpdateType.Message:
                                case Telegram.Bot.Types.Enums.UpdateType.EditedMessage:

                                    if(message.Text?.Contains(Constants.RK_Semester) ?? false) {
                                        string? studentID = user.ScheduleProfile.StudentID;
                                        if(!string.IsNullOrWhiteSpace(studentID)) {
                                            await AcademicPerformancePerSemester(botClient, message.Chat, message.Text, studentID);
                                        } else {
                                            if(user.IsAdmin())
                                                await StudentIdErrorAdmin(botClient, message.Chat);
                                            else
                                                await StudentIdErrorUser(botClient, message.Chat);
                                        }
                                        return;
                                    }

                                    if(user.IsAdmin()) {
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
                                        await GetScheduleByDate(botClient, message.Chat, message.Text, user.IsAdmin(), user.ScheduleProfile);

                                    break;

                                case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:
                                    if(update.CallbackQuery?.Data is null) return;

                                    await DefaultCallbackModeAsync(message, botClient, user, update.CallbackQuery.Data);
                                    break;
                            }
                            break;

                        case Mode.AddingDiscipline:
                            switch(update.Type) {
                                case Telegram.Bot.Types.Enums.UpdateType.Message:
                                case Telegram.Bot.Types.Enums.UpdateType.EditedMessage:

                                    await SetStagesAddingDisciplineAsync(user, message, botClient);
                                    break;

                                case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:
                                    if(update.CallbackQuery?.Data is null) return;

                                    await AddingDisciplineCallbackModeAsync(message, botClient, user, update.CallbackQuery.Data);
                                    break;
                            }
                            break;

                        case Mode.GroupСhange:
                            switch(update.Type) {
                                case Telegram.Bot.Types.Enums.UpdateType.Message:
                                case Telegram.Bot.Types.Enums.UpdateType.EditedMessage:
                                    await GroupСhangeMessageMode(botClient, message, user);
                                    break;
                            }
                            break;

                        case Mode.StudentIDСhange:
                            switch(update.Type) {
                                case Telegram.Bot.Types.Enums.UpdateType.Message:
                                case Telegram.Bot.Types.Enums.UpdateType.EditedMessage:
                                    await StudentIDСhangeMessageMode(botClient, message, user);
                                    break;
                            }
                            break;

                        case Mode.ResetProfileLink:
                            switch(update.Type) {
                                case Telegram.Bot.Types.Enums.UpdateType.Message:
                                case Telegram.Bot.Types.Enums.UpdateType.EditedMessage:
                                    await ResetProfileLink(botClient, message, user);
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