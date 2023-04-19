using System.Globalization;

using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.Bot {
    public partial class TelegramBot {
        #region ReplyKeyboardMarkup
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
        private readonly ReplyKeyboardMarkup CancelKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { Constants.RK_Cancel }
                        })
        { ResizeKeyboard = true };

        private readonly ReplyKeyboardMarkup WeekKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { Constants.RK_ThisWeek, Constants.RK_NextWeek },
                            new KeyboardButton[] { Constants.RK_Back }
                        })
        { ResizeKeyboard = true };
        #endregion

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
    }
}