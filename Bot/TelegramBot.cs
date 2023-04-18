using System.Globalization;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.Bot {
    public partial class TelegramBot {
        private readonly ITelegramBotClient telegramBot;
        private readonly Scheduler.Scheduler scheduler;
        private readonly ScheduleDbContext dbContext;

        #region KeyboardMarkup
        private readonly ReplyKeyboardMarkup MainKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { Constants.RK_Today, Constants.RK_Tomorrow },
                            new KeyboardButton[] { Constants.RK_ByDays, Constants.RK_ForAWeek },
                            new KeyboardButton[] { Constants.RK_AcademicPerformance }
                        })
        { ResizeKeyboard = true };

        private readonly ReplyKeyboardMarkup DaysKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { Constants.RK_Monday, Constants.RK_Tuesday },
                            new KeyboardButton[] { Constants.RK_Wednesday, Constants.RK_Thursday },
                            new KeyboardButton[] { Constants.RK_Friday, Constants.RK_Saturday },
                            new KeyboardButton[] { Constants.RK_Back }
                        })
        { ResizeKeyboard = true };

        private readonly ReplyKeyboardMarkup WeekKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { Constants.RK_ThisWeek, Constants.RK_NextWeek },
                            new KeyboardButton[] { Constants.RK_Back }
                        })
        { ResizeKeyboard = true };

        private readonly ReplyKeyboardMarkup CancelKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { Constants.RK_Cancel }
                        })
        { ResizeKeyboard = true };

        private readonly InlineKeyboardMarkup inlineAdminKeyboardMarkup = new(new[]{
            new [] { InlineKeyboardButton.WithCallbackData(Constants.IK_ViewAll.text, Constants.IK_ViewAll.callback) },
            new [] { InlineKeyboardButton.WithCallbackData(Constants.IK_Edit.text, Constants.IK_Edit.callback) }
        }){};

        private readonly InlineKeyboardMarkup inlineKeyboardMarkup = new(new[]{
            new [] { InlineKeyboardButton.WithCallbackData(text: Constants.IK_ViewAll.text, Constants.IK_ViewAll.callback) }
        }){};

        private readonly InlineKeyboardMarkup inlineBackKeyboardMarkup = new(new[]{
            new [] { InlineKeyboardButton.WithCallbackData(Constants.IK_Back.text, Constants.IK_Back.callback) }
        }){};
        #endregion

        public TelegramBot(Scheduler.Scheduler scheduler, ScheduleDbContext dbContext) {
            this.scheduler = scheduler;
            this.dbContext = dbContext;

            telegramBot = new TelegramBotClient(Environment.GetEnvironmentVariable("TelegramBotToken") ?? "");

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
            Message? message = update.Message ?? update.EditedMessage ?? update.CallbackQuery?.Message;
            TelegramUser? user;

            if(message is not null) {
                if(message.From is null) return;

                user = dbContext.TelegramUsers.FirstOrDefault(u => u.ChatID == message.Chat.Id);

                if(user is null) {
                    user = new() { ChatID = message.Chat.Id, FirstName = message.From.FirstName, Username = message.From.Username, LastName = message.From.LastName };
                    dbContext.TelegramUsers.Add(user);
                    dbContext.SaveChanges();
                }

                switch(user.Mode) {
                    case Mode.Default:
                        switch(update.Type) {
                            case Telegram.Bot.Types.Enums.UpdateType.Message:
                            case Telegram.Bot.Types.Enums.UpdateType.EditedMessage:
                                await MessageModeAsync(message, botClient, user, cancellationToken);
                                break;

                            case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:
                                if(update.CallbackQuery?.Data is null) return;

                                await CallbackModeAsync(message, botClient, user, cancellationToken, update.CallbackQuery.Data);
                                break;
                        }
                        break;

                    case Mode.AddingDiscipline:
                        switch(message.Text) {
                            case Constants.RK_Cancel:
                                user.Mode = Mode.Default;
                                dbContext.TemporaryAddition.Remove(dbContext.TemporaryAddition.Where(i => i.TelegramUser == user).OrderByDescending(i => i.AddDate).First());
                                dbContext.SaveChanges();
                                await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Основное меню", replyMarkup: MainKeyboardMarkup);
                                break;

                            default:
                                await SetStagesAddingDisciplineAsync(user, message, botClient);
                                break;
                        }
                        break;
                }
            }
        }

        private async Task MessageModeAsync(Message message, ITelegramBotClient botClient, TelegramUser user, CancellationToken cancellationToken) {
            switch(message.Text) {
                case "/start":
                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: $"👋 {telegramBot.GetMeAsync(cancellationToken).Result.Username} 👋", replyMarkup: MainKeyboardMarkup);
                    break;

                case Constants.RK_Back:
                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Основное меню", replyMarkup: MainKeyboardMarkup);
                    break;

                case Constants.RK_Today:
                case Constants.RK_Tomorrow:
                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: $"Расписание актуально на {Parser.scheduleLastUpdate.ToString("dd.MM.yyyy HH:mm")}", replyMarkup: MainKeyboardMarkup);
                    await TodayAndTomorrow(botClient, message.Chat, message.Text, user);
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
                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: $"Расписание актуально на {Parser.scheduleLastUpdate.ToString("dd.MM.yyyy HH:mm")}", replyMarkup: DaysKeyboardMarkup);
                    await DayOfWeek(botClient, message.Chat, message.Text, user);
                    break;

                case Constants.RK_ForAWeek:
                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: Constants.RK_ForAWeek, replyMarkup: WeekKeyboardMarkup);
                    break;

                case Constants.RK_ThisWeek:
                case Constants.RK_NextWeek:
                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: $"Расписание актуально на {Parser.scheduleLastUpdate.ToString("dd.MM.yyyy HH:mm")}", replyMarkup: WeekKeyboardMarkup);
                    await Weeks(botClient, message.Chat, message.Text, user);
                    break;

                case Constants.RK_AcademicPerformance:
                    await botClient.SendTextMessageAsync(chatId: message.Chat, text: $"Успеваемость актуальна на {Parser.progressLastUpdate.ToString("dd.MM.yyyy HH:mm")}", replyMarkup: GetTermsKeyboardMarkup());
                    break;

                default:
                    var split = message.Text?.Split();
                    if(split == null || split.Count() < 2) return;

                    switch(split[1]) {
                        case "семестр":
                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: scheduler.GetProgressByTerm(int.Parse(split[0])), replyMarkup: GetTermsKeyboardMarkup());
                            break;
                    }
                    break;
            }
        }

        private async Task CallbackModeAsync(Message message, ITelegramBotClient botClient, TelegramUser user, CancellationToken cancellationToken, string data) {
            if(DateOnly.TryParse(message.Text?.Split('-')[0].Trim()[2..] ?? "", out DateOnly date)) {

                switch(data) {
                    case Constants.IK_Edit.callback:
                        await botClient.EditMessageTextAsync(chatId: message.Chat, messageId: message.MessageId, text: scheduler.GetScheduleByDate(date, true), replyMarkup: GetEditAdminInlineKeyboardButton(date));
                        break;

                    case Constants.IK_ViewAll.callback:
                        await botClient.EditMessageTextAsync(chatId: message.Chat, messageId: message.MessageId, text: scheduler.GetScheduleByDate(date, true), replyMarkup: inlineBackKeyboardMarkup);
                        break;

                    case Constants.IK_Back.callback:
                        await botClient.EditMessageTextAsync(chatId: message.Chat, messageId: message.MessageId, text: scheduler.GetScheduleByDate(date), replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
                        break;

                    case Constants.IK_Add.callback:
                        user.Mode = Mode.AddingDiscipline;
                        dbContext.TemporaryAddition.Add(new(user, date));
                        dbContext.SaveChanges();
                        await botClient.EditMessageTextAsync(chatId: message.Chat, messageId: message.MessageId, text: scheduler.GetScheduleByDate(date));
                        await botClient.SendTextMessageAsync(chatId: message.Chat, text: GetStagesAddingDiscipline(user), replyMarkup: CancelKeyboardMarkup);
                        break;

                    default:
                        List<string> str = data.Split(' ').ToList() ?? new();
                        if(str.Count < 2) return;

                        var discipline = dbContext.Disciplines.FirstOrDefault(i => i.ID == int.Parse(str[1]));

                        if(discipline is not null) {
                            switch(str[0] ?? "") {
                                case "Day":
                                    discipline.IsCompleted = !discipline.IsCompleted;
                                    dbContext.SaveChanges();

                                    break;

                                case "Always":
                                    var completedDisciplines = dbContext.CompletedDisciplines.FirstOrDefault(i=> i.Name == discipline.Name && i.Lecturer == discipline.Lecturer && i.Class == discipline.Class && i.Subgroup == discipline.Subgroup);

                                    if(completedDisciplines is not null)
                                        dbContext.CompletedDisciplines.Remove(completedDisciplines);
                                    else
                                        dbContext.CompletedDisciplines.Add(discipline);

                                    dbContext.SaveChanges();
                                    Parser.SetDisciplineIsCompleted(dbContext.CompletedDisciplines.ToList(), dbContext.Disciplines);
                                    dbContext.SaveChanges();
                                    break;

                                case "Delete":
                                    dbContext.Disciplines.Remove(discipline);
                                    dbContext.SaveChanges();

                                    break;
                            }
                            await botClient.EditMessageReplyMarkupAsync(chatId: message.Chat, messageId: message.MessageId, replyMarkup: GetEditAdminInlineKeyboardButton(date));
                        }
                        break;
                }
            }

        }

        private string GetStagesAddingDiscipline(TelegramUser user, int? counter = null) {
            if(counter != null)
                return Constants.StagesOfAdding[(int)counter];

            return Constants.StagesOfAdding[dbContext.TemporaryAddition.Where(i => i.TelegramUser == user).OrderByDescending(i => i.AddDate).First().Counter];
        }

        private async Task SetStagesAddingDisciplineAsync(TelegramUser user, Message message, ITelegramBotClient botClient) {
            var temporaryAddition = dbContext.TemporaryAddition.Where(i => i.TelegramUser == user).OrderByDescending(i => i.AddDate).First();
            try {
                if(message.Text == null) throw new Exception();

                switch(temporaryAddition.Counter) {
                    case 0:
                        temporaryAddition.Name = message.Text;
                        break;
                    case 1:
                        temporaryAddition.Type = message.Text;
                        break;
                    case 2:
                        temporaryAddition.Lecturer = message.Text;
                        break;
                    case 3:
                        temporaryAddition.LectureHall = message.Text;
                        break;
                    case 4:
                        temporaryAddition.StartTime = TimeOnly.Parse(message.Text);
                        break;
                    case 5:
                        temporaryAddition.EndTime = TimeOnly.Parse(message.Text);
                        break;
                }
            } catch(Exception) {
                await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Ошибка! " + GetStagesAddingDiscipline(user, temporaryAddition.Counter), replyMarkup: CancelKeyboardMarkup);
                return;
            }

            temporaryAddition.Counter++;

            if(temporaryAddition.Counter <= 5) {
                dbContext.SaveChanges();
                await botClient.SendTextMessageAsync(chatId: message.Chat, text: GetStagesAddingDiscipline(user, temporaryAddition.Counter), replyMarkup: CancelKeyboardMarkup);
            } else {
                dbContext.Disciplines.Add(temporaryAddition);
                dbContext.TemporaryAddition.Remove(temporaryAddition);
                user.Mode = Mode.Default;

                dbContext.SaveChanges();

                await botClient.SendTextMessageAsync(chatId: message.Chat, text: GetStagesAddingDiscipline(user, temporaryAddition.Counter), replyMarkup: MainKeyboardMarkup);
                await botClient.SendTextMessageAsync(chatId: message.Chat, text: scheduler.GetScheduleByDate(temporaryAddition.Date), replyMarkup: inlineAdminKeyboardMarkup);
            }
        }

        private ReplyKeyboardMarkup GetTermsKeyboardMarkup() {
            List<KeyboardButton[]> TermsKeyboardMarkup = new();

            var terms = dbContext.Progresses.Where(i => i.Mark != null).Select(i => i.Term).Distinct().OrderBy(i => i).ToArray();
            for(int i = 0; i < terms.Length; i++)
                TermsKeyboardMarkup.Add(new KeyboardButton[] { $"{terms[i]} семестр", i + 1 < terms.Length ? $"{terms[++i]} семестр" : "" });

            TermsKeyboardMarkup.Add(new KeyboardButton[] { Constants.RK_Back });

            return new(TermsKeyboardMarkup) { ResizeKeyboard = true };
        }

        private async Task TodayAndTomorrow(ITelegramBotClient botClient, ChatId chatId, string text, TelegramUser user) {
            switch(text) {
                case Constants.RK_Today:
                    await botClient.SendTextMessageAsync(chatId: chatId, text: scheduler.GetScheduleByDate(DateOnly.FromDateTime(DateTime.Now)), replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
                    break;

                case Constants.RK_Tomorrow:
                    await botClient.SendTextMessageAsync(chatId: chatId, text: scheduler.GetScheduleByDate(DateOnly.FromDateTime(DateTime.Now.AddDays(1))), replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);
                    break;
            }
        }

        private async Task DayOfWeek(ITelegramBotClient botClient, ChatId chatId, string text, TelegramUser user) {
            switch(text) {
                case Constants.RK_Monday:
                    foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Monday))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
                case Constants.RK_Tuesday:
                    foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Tuesday))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
                case Constants.RK_Wednesday:
                    foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Wednesday))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
                case Constants.RK_Thursday:
                    foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Thursday))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
                case Constants.RK_Friday:
                    foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Friday))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
                case Constants.RK_Saturday:
                    foreach(var day in scheduler.GetScheduleByDay(System.DayOfWeek.Saturday))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: day, replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
            }
        }

        private async Task Weeks(ITelegramBotClient botClient, ChatId chatId, string text, TelegramUser user) {
            switch(text) {
                case Constants.RK_ThisWeek:
                    foreach(var item in scheduler.GetScheduleByWeak(CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, System.DayOfWeek.Monday) - 1))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: item, replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
                case Constants.RK_NextWeek:
                    foreach(var item in scheduler.GetScheduleByWeak(CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, System.DayOfWeek.Monday)))
                        await botClient.SendTextMessageAsync(chatId: chatId, text: item, replyMarkup: user.IsAdmin ? inlineAdminKeyboardMarkup : inlineKeyboardMarkup);

                    break;
            }
        }

        private InlineKeyboardMarkup GetEditAdminInlineKeyboardButton(DateOnly date) {
            var editButtons = new List<InlineKeyboardButton[]>();

            var completedDisciplinesList = dbContext.CompletedDisciplines.ToList();

            var disciplines = dbContext.Disciplines.Where(i => i.Date == date && !i.IsCastom).OrderBy(i => i.StartTime);
            if(disciplines.Any()) {
                editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: "В этот день", callbackData: "!"), InlineKeyboardButton.WithCallbackData(text: "Всегда", callbackData: "!") });

                foreach(var item in disciplines) {
                    var completedDisciplines = completedDisciplinesList.FirstOrDefault(i => i.Equals(item));

                    editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: $"{item.StartTime.ToString()} {item.Lecturer?.Split(' ')[0]} {(item.IsCompleted ? "❌" : "✅")}", callbackData: $"Day {item.ID}"),
                                            InlineKeyboardButton.WithCallbackData(text: completedDisciplines is not null ? "❌" : "✅", callbackData: $"Always {item.ID}")});
                }
            }

            var castom = dbContext.Disciplines.Where(i => i.Date == date && i.IsCastom).OrderBy(i => i.StartTime);
            if(castom.Any()) {
                editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: "Отображение", callbackData: "!"), InlineKeyboardButton.WithCallbackData(text: "Удалить", callbackData: "!") });

                foreach(var item in castom) {
                    var completedDisciplines = completedDisciplinesList.FirstOrDefault(i => i.Equals(item));

                    editButtons.Add(new[] { InlineKeyboardButton.WithCallbackData(text: $"{item.StartTime.ToString()} {item.Lecturer?.Split(' ')[0]} {(item.IsCompleted ? "❌" : "✅")}", callbackData: $"Day {item.ID}"),
                                            InlineKeyboardButton.WithCallbackData(text: "🗑", callbackData: $"Delete {item.ID}")});
                }
            }

            editButtons.AddRange(new[] { new[] { InlineKeyboardButton.WithCallbackData(Constants.IK_Add.text, Constants.IK_Add.callback) },
                                         new[] { InlineKeyboardButton.WithCallbackData(Constants.IK_Back.text, Constants.IK_Back.callback) }});

            return new InlineKeyboardMarkup(editButtons);
        }


        private Task HandleError(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
