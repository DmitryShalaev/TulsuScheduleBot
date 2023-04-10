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
        private readonly ScheduleDbContext dbContext;

        private bool addFlag = false;

        private readonly ReplyKeyboardMarkup MainKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { "Сегодня", "Завтра" },
                            new KeyboardButton[] { "По дням", "На неделю" },
                            new KeyboardButton[] { "Сданные предметы" },

                        })
        { ResizeKeyboard = true };

        private readonly ReplyKeyboardMarkup DaysKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { "Понедельник", "Вторник" },
                            new KeyboardButton[] { "Среда", "Четверг" },
                            new KeyboardButton[] { "Пятница", "Суббота" },
                            new KeyboardButton[] { "Назад", },

                        })
        { ResizeKeyboard = true };

        private readonly ReplyKeyboardMarkup WeekKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { "Эта неделя", "Следующая неделя" },
                            new KeyboardButton[] { "Назад", },

                        })
        { ResizeKeyboard = true };


        public TelegramBot(ScheduleDbContext dbContext, string token = "5588441792:AAFsoUQdu5_hd9Ccz34ZVNPhK9d5Z7Nx9VM") {
            this.dbContext = dbContext;

            telegramBot = new TelegramBotClient(token);

            Console.WriteLine("Запущен бот " + telegramBot.GetMeAsync().Result.FirstName);

            telegramBot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                new ReceiverOptions {
                    AllowedUpdates = { },
                },
                new CancellationTokenSource().Token
            );
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            Message? message = update.Message ?? update.EditedMessage;

            TelegramUser? user;

            if(message != null) {
                if(update.Type == Telegram.Bot.Types.Enums.UpdateType.Message || update.Type == Telegram.Bot.Types.Enums.UpdateType.EditedMessage) {
                    switch(message.Text?.ToLower()) {
                        case "/start":
                            if(update.Message?.From != null) {
                                user = new() { ChatId = message.Chat.Id, FirstName = update.Message.From.FirstName, Username = update.Message.From.Username, LastName = update.Message.From.LastName };

                                if(!dbContext.TelegramUsers.ToList().Contains(user)) {
                                    dbContext.TelegramUsers.Add(user);
                                    dbContext.SaveChanges();
                                }
                            }

                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: $"👋 {telegramBot.GetMeAsync(cancellationToken: cancellationToken).Result.Username} 👋", replyMarkup: MainKeyboardMarkup, cancellationToken: cancellationToken);
                            break;

                        case "сегодня":
                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: $"Расписание актуально на {Parser.lastUpdate.ToString("dd.MM.yyyy HH:mm")}", replyMarkup: MainKeyboardMarkup, cancellationToken: cancellationToken);
                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: GetScheduleByDate(DateOnly.FromDateTime(DateTime.Now)), replyMarkup: MainKeyboardMarkup, cancellationToken: cancellationToken);
                            break;

                        case "завтра":
                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: $"Расписание актуально на {Parser.lastUpdate.ToString("dd.MM.yyyy HH:mm")}", replyMarkup: MainKeyboardMarkup, cancellationToken: cancellationToken);
                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: GetScheduleByDate(DateOnly.FromDateTime(DateTime.Now.AddDays(1))), replyMarkup: MainKeyboardMarkup, cancellationToken: cancellationToken);
                            break;

                        case "по дням":
                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: "По дням", replyMarkup: DaysKeyboardMarkup, cancellationToken: cancellationToken);
                            break;

                        case "назад":
                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Основное меню", replyMarkup: MainKeyboardMarkup, cancellationToken: cancellationToken);
                            break;

                        case "понедельник":
                            await GetScheduleByDayAsync(botClient, message, cancellationToken, DayOfWeek.Monday);
                            break;

                        case "вторник":
                            await GetScheduleByDayAsync(botClient, message, cancellationToken, DayOfWeek.Tuesday);
                            break;

                        case "среда":
                            await GetScheduleByDayAsync(botClient, message, cancellationToken, DayOfWeek.Wednesday);
                            break;

                        case "четверг":
                            await GetScheduleByDayAsync(botClient, message, cancellationToken, DayOfWeek.Thursday);
                            break;

                        case "пятница":
                            await GetScheduleByDayAsync(botClient, message, cancellationToken, DayOfWeek.Friday);
                            break;

                        case "суббота":
                            await GetScheduleByDayAsync(botClient, message, cancellationToken, DayOfWeek.Saturday);
                            break;

                        case "на неделю":
                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: "На неделю", replyMarkup: WeekKeyboardMarkup, cancellationToken: cancellationToken);
                            break;

                        case "эта неделя":
                            await GetScheduleByWeakAsync(botClient, message, cancellationToken, CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday) - 1);
                            break;

                        case "следующая неделя":
                            await GetScheduleByWeakAsync(botClient, message, cancellationToken, CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday));
                            break;

                        case "сданные предметы":
                            string str = "";

                            foreach(var item in dbContext.CompletedDisciplines.ToList())
                                str += $"{item.Name} ({TypeToString[item.Class]})\n\n";


                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: str, replyMarkup: MainKeyboardMarkup, cancellationToken: cancellationToken);
                            break;

                        case "add":
                            user = dbContext.TelegramUsers.FirstOrDefault(i => i.ChatId == message.Chat.Id);
                            if(user is not null && user.IsAdmin) {
                                addFlag = true;
                                await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Name | Class\nall: 0\nlab:1\npractice:2", cancellationToken: cancellationToken);
                            } else
                                await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Permission denied", cancellationToken: cancellationToken);

                            break;

                        case "end":
                            user = dbContext.TelegramUsers.FirstOrDefault(i => i.ChatId == message.Chat.Id);
                            if(user is not null && user.IsAdmin && addFlag) {
                                addFlag = false;

                                var completedDiscipline = dbContext.CompletedDisciplines.ToList();
                                foreach(var discipline in dbContext.Disciplines)
                                    discipline.IsCompleted = (discipline.Class == DB.Entity.Type.lab && discipline.Subgroup != Parser.subgroup) || completedDiscipline.Contains(new() { Name = discipline.Name, Class = discipline.Class });

                                dbContext.SaveChanges();
                            }
                            await botClient.SendTextMessageAsync(chatId: message.Chat, text: "Основное меню", replyMarkup: MainKeyboardMarkup, cancellationToken: cancellationToken);
                            break;

                        default:
                            user = dbContext.TelegramUsers.FirstOrDefault(i => i.ChatId == message.Chat.Id);
                            if(user is not null && user.IsAdmin && addFlag && !string.IsNullOrEmpty(message.Text)) {
                                var tmp = message.Text.Split('|').Select(s => s.Trim()).ToList();

                                dbContext.CompletedDisciplines.Add(new() { Name = tmp[0], Class = (DB.Entity.Type)byte.Parse(tmp[1]) }); ;
                                dbContext.SaveChanges();
                            }
                            break;
                    }
                }
            }
        }

        private Dictionary<DB.Entity.Type, string> TypeToString = new(){ { DB.Entity.Type.all, "Все"}, { DB.Entity.Type.lab, "Лаб. занятия" }, { DB.Entity.Type.practice, "Практические занятия" } };

        private async Task GetScheduleByWeakAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, int weeks) {
            await botClient.SendTextMessageAsync(chatId: message.Chat, text: $"Расписание актуально на {Parser.lastUpdate.ToString("dd.MM.yyyy HH:mm")}", replyMarkup: MainKeyboardMarkup, cancellationToken: cancellationToken);

            var dateOnly = DateOnly.FromDateTime(new DateTime(DateTime.Now.Year, 1, 1));

            for(int i = 1; i < 7; i++) {
                await botClient.SendTextMessageAsync(chatId: message.Chat, text: GetScheduleByDate(dateOnly.AddDays(7 * weeks + i)),
                replyMarkup: WeekKeyboardMarkup, cancellationToken: cancellationToken);
            }
        }

        private async Task GetScheduleByDayAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken, DayOfWeek dayOfWeek) {
            await botClient.SendTextMessageAsync(chatId: message.Chat, text: $"Расписание актуально на {Parser.lastUpdate.ToString("dd.MM.yyyy HH:mm")}", replyMarkup: MainKeyboardMarkup, cancellationToken: cancellationToken);

            int weeks = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            var dateOnly = DateOnly.FromDateTime(new DateTime(DateTime.Now.Year, 1, 1));

            for(int i = -1; i < 2; i++) {
                await botClient.SendTextMessageAsync(chatId: message.Chat, text: GetScheduleByDate(dateOnly.AddDays(7 * (weeks + i) + (byte)dayOfWeek)),
                replyMarkup: DaysKeyboardMarkup, cancellationToken: cancellationToken);
            }
        }

        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken) {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
            return Task.CompletedTask;
        }

    }
}
