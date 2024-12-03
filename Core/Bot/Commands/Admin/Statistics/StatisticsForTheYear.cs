using Core.DB;

using Microsoft.EntityFrameworkCore;

using Telegram.Bot.Types;

namespace Core.Bot.Commands.Admin.Statistics {
    public class StatisticsForTheYear {
        public class UserStatistics {
            // Общая информация
            public DateTime FirstMessageInYear { get; set; }
            public required string FirstMessageTextInYear { get; set; }
            public long TotalMessages { get; set; }
            public double AverageMessagesPerDay { get; set; }
            public DateTime MostActiveDay { get; set; }
            public long MessagesOnMostActiveDay { get; set; }

            // Запросы расписания
            public required Dictionary<string, long> ScheduleRequests { get; set; }

            // Запросы успеваемости
            public required Dictionary<string, long> PerformanceRequests { get; set; }

            // Достижения
            public long UniqueMessages { get; set; }

            // Интересные факты
            public required string FirstMessageEver { get; set; }
            public DateTime FirstMessageDateEver { get; set; }
            public required string MostPopularRequestType { get; set; }
            public long MostPopularRequestCount { get; set; }
            public required string PreferredInteractionTime { get; set; }
            public int LongestStreak { get; set; }
            public DayOfWeek MostActiveDayOfWeek { get; set; }
        }

        public static class RequestTypes {
            public static readonly HashSet<string> ScheduleRequests =
            [
                "сегодня",
                "завтра",
                "послезавтра",
                "по дням",
                "на неделю",
                "понедельник",
                "вторник",
                "среда",
                "четверг",
                "пятница",
                "суббота",
                "эта неделя",
                "следующая неделя",
                "экзамены",
                "ближайший экзамен",
                "все экзамены",
            ];

            public static readonly HashSet<string> PerformanceRequests =
            [
                "успеваемость",
                "семестр",
            ];
        }

        private static readonly string[] excludedPrefixes =
        [
            "Notifications%",
            "Discipline%",
            "Custom%",
            "SetEndTime%",
            "All%",
            "Back%",
            "Edit%"
        ];

        public static async Task<UserStatistics> GetUserStatisticsAsync(ScheduleDbContext dbContext, ChatId chatId) {
            int currentYear = DateTime.UtcNow.Year;

            // Фильтрация сообщений за текущий год
            DateTime startOfYear = new DateTime(currentYear, 1, 1).ToUniversalTime();
            DateTime endOfYear = new DateTime(currentYear + 1, 1, 1).ToUniversalTime();

            IQueryable<DB.Entity.MessageLog> messagesQuery = dbContext.MessageLog.AsQueryable();

            IQueryable<DB.Entity.MessageLog> messagesThisYear = messagesQuery
                .Where(m => m.From == chatId.Identifier)
                .Where(m => m.Date >= startOfYear && m.Date < endOfYear)
                .Where(m => !excludedPrefixes.Any(prefix => EF.Functions.ILike(m.Message, prefix)));

            // Первое сообщение в году
            DB.Entity.MessageLog? firstMessageInYear = await messagesThisYear
                .OrderBy(m => m.Date)
                .FirstOrDefaultAsync();

            // Общие сообщения за год
            int totalMessages = await messagesThisYear.CountAsync();

            // Количество активных дней
            int activeDays = await messagesThisYear
                .Select(m => m.Date.ToLocalTime().Date)
                .Distinct()
                .CountAsync();

            // Среднее количество сообщений в день
            double averageMessagesPerDay = activeDays > 0 ? (double)totalMessages / activeDays : 0;

            // Самый активный день
            var mostActiveDayGroup = await dbContext.MessageLog
                .Where(m => m.From == chatId.Identifier)
                .Where(m => m.Date >= startOfYear && m.Date < endOfYear)
                .GroupBy(m => new { m.Date.Year, m.Date.Month, m.Date.Day })
                .Select(g => new {
                    Date = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day),
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Count)
                .FirstOrDefaultAsync();

            DateTime mostActiveDay = DateTime.MinValue;
            long messagesOnMostActiveDay = 0;

            if(mostActiveDayGroup != null) {
                mostActiveDay = mostActiveDayGroup.Date;
                messagesOnMostActiveDay = mostActiveDayGroup.Count;
            }

            // Запросы расписания
            Dictionary<string, long> scheduleRequests = await messagesThisYear
                .Where(m => RequestTypes.ScheduleRequests.Contains(m.Message.ToLower()))
                .GroupBy(m => m.Message)
                .Select(g => new { Request = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Request, x => (long)x.Count);

            // Запросы успеваемости
            Dictionary<string, long> performanceRequests = await messagesThisYear
                .Where(m => RequestTypes.PerformanceRequests.Contains(m.Message.ToLower()) || RequestTypes.PerformanceRequests.Contains(m.Message.Substring(2).ToLower()))
                .GroupBy(m => m.Message)
                .Select(g => new { Request = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Request, x => (long)x.Count);

            // Уникальные сообщения
            int uniqueMessages = await messagesThisYear
                .Select(m => m.Message)
                .Distinct()
                .CountAsync();

            // Первое сообщение вообще
            DB.Entity.MessageLog? firstMessageEver = await dbContext.MessageLog
                .Where(m => m.From == chatId.Identifier)
                .OrderBy(m => m.Date)
                .FirstOrDefaultAsync();

            // Самый популярный тип запроса
            var mostPopularRequest = await messagesThisYear
                .GroupBy(m => m.Message)
                .Select(g => new { Message = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .FirstOrDefaultAsync();

            // Предпочтительное время для взаимодействия
            var preferredInteractionTime = await messagesThisYear
                .GroupBy(m => m.Date.ToLocalTime().Hour)
                .Select(g => new { Hour = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .FirstOrDefaultAsync();

            // Самый длинный период активности
            var userActivityDays = messagesThisYear
                .Select(m => m.Date.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            int longestStreak = 0, currentStreak = 1;
            for(int i = 1; i < userActivityDays.Count; i++) {
                if(userActivityDays[i] == userActivityDays[i - 1].AddDays(1))
                    currentStreak++;
                else {
                    longestStreak = Math.Max(longestStreak, currentStreak);
                    currentStreak = 1;
                }
            }

            longestStreak = Math.Max(longestStreak, currentStreak);

            DayOfWeek mostActiveDayOfWeek = await messagesThisYear
                .GroupBy(m => m.Date.DayOfWeek)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefaultAsync();

            // Формирование DTO
            var statistics = new UserStatistics {
                FirstMessageInYear = firstMessageInYear?.Date.ToLocalTime() ?? DateTime.MinValue,
                FirstMessageTextInYear = firstMessageInYear?.Message ?? "Нет сообщений",
                TotalMessages = totalMessages,
                AverageMessagesPerDay = averageMessagesPerDay,
                MostActiveDay = mostActiveDay.ToLocalTime(),
                MessagesOnMostActiveDay = messagesOnMostActiveDay,
                ScheduleRequests = scheduleRequests,
                PerformanceRequests = performanceRequests,
                UniqueMessages = uniqueMessages,
                LongestStreak = longestStreak,
                MostActiveDayOfWeek = mostActiveDayOfWeek,
                FirstMessageEver = firstMessageEver?.Message ?? "Нет сообщений",
                FirstMessageDateEver = firstMessageEver?.Date.ToLocalTime() ?? DateTime.MinValue,
                MostPopularRequestType = mostPopularRequest != null ? mostPopularRequest.Message : "Нет запросов",
                MostPopularRequestCount = mostPopularRequest?.Count ?? 0,
                PreferredInteractionTime = preferredInteractionTime != null ? $"{preferredInteractionTime.Hour}:00" : "Нет данных"
            };

            return statistics;
        }

        public static async Task<string> GetGlobalStats(ScheduleDbContext dbContext) {
            int totalUsers = await dbContext.TelegramUsers.CountAsync();
            int totalMessagesGlobal = await dbContext.MessageLog.CountAsync();

            var mostPopularRequestGlobal = await dbContext.MessageLog
                .Where(m => !excludedPrefixes.Any(prefix => EF.Functions.ILike(m.Message, prefix)))
                .GroupBy(m => m.Message)
                .Select(g => new { Request = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .FirstOrDefaultAsync();

            string mostPopularRequestGlobalType = mostPopularRequestGlobal?.Request ?? "Нет запросов";
            int mostPopularRequestGlobalCount = mostPopularRequestGlobal?.Count ?? 0;

            int scheduleRequestsGlobal = await dbContext.MessageLog
                .Where(m => RequestTypes.ScheduleRequests.Contains(m.Message.ToLower()))
            .CountAsync();

            int performanceRequestsGlobal = await dbContext.MessageLog
                .Where(m => RequestTypes.PerformanceRequests.Contains(m.Message.ToLower()) || RequestTypes.PerformanceRequests.Contains(m.Message.Substring(2).ToLower()))
                .CountAsync();

            // День с наибольшей активностью
            var busiestDay = await dbContext.MessageLog
                .Where(m => m.Date.Year == 2024)
                .GroupBy(m => m.Date.Date)
                .OrderByDescending(g => g.Count())
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .FirstOrDefaultAsync();

            string busiestDayText = busiestDay != null ? $"{busiestDay.Date:dd.MM.yyyy} ({busiestDay.Count} " +
                GetDeclension(busiestDay.Count, "сообщение", "сообщения", "сообщений") + ")" : "Нет данных";

            string totalUsersText = $"{totalUsers} " +
                GetDeclension(totalUsers, "пользователь", "пользователя", "пользователей");

            string totalMessagesGlobalText = $"{totalMessagesGlobal} " +
                GetDeclension(totalMessagesGlobal, "сообщение", "сообщения", "сообщений");

            string mostPopularRequestGlobalText = $"\"{mostPopularRequestGlobalType}\", к которому обратились {mostPopularRequestGlobalCount} " +
                GetDeclension(mostPopularRequestGlobalCount, "раз", "раза", "раз");

            string scheduleRequestsGlobalText = $"{scheduleRequestsGlobal} " +
                GetDeclension(scheduleRequestsGlobal, "раз", "раза", "раз");

            string performanceRequestsGlobalText = $"{performanceRequestsGlobal} " +
                GetDeclension(performanceRequestsGlobal, "раз", "раза", "раз");

            // Общая статистика
            string globalStats = $"🌍 ***Общая статистика за год:***\n" +
                              $"👥 Новых пользователей: {totalUsersText}.\n" +
                              $"📨 Всего сообщений: {totalMessagesGlobalText}.\n" +
                              $"🔥 Самый популярный запрос: {mostPopularRequestGlobalText}.\n" +
                              $"📚 Запросов расписания: {scheduleRequestsGlobalText}.\n" +
                              $"🎓 Запросов успеваемости: {performanceRequestsGlobalText}.\n" +
                              $"📈 День с наибольшей активностью: {busiestDayText}.\n\n";
            return globalStats;
        }

        public static string GetDeclension(long number, string nominative, string genitiveSingular, string genitivePlural) {
            number = Math.Abs(number) % 100;
            long num = number % 10;

            return number is > 10 and < 20 ? genitivePlural : num is > 1 and < 5 ? genitiveSingular : num == 1 ? nominative : genitivePlural;
        }

        private static readonly Dictionary<DayOfWeek, string> DayOfWeekInRussian = new Dictionary<DayOfWeek, string>
        {
            { DayOfWeek.Monday, "Понедельник" },
            { DayOfWeek.Tuesday, "Вторник" },
            { DayOfWeek.Wednesday, "Среда" },
            { DayOfWeek.Thursday, "Четверг" },
            { DayOfWeek.Friday, "Пятница" },
            { DayOfWeek.Saturday, "Суббота" },
            { DayOfWeek.Sunday, "Воскресенье" }
        };

        public static async Task<string> SendStatisticsMessageAsync(ScheduleDbContext dbContext, ChatId chatId, string globalStats) {
            UserStatistics stats = await GetUserStatisticsAsync(dbContext, chatId);

            string totalMessagesText = $"{stats.TotalMessages} " +
                GetDeclension(stats.TotalMessages, "сообщение", "сообщения", "сообщений");

            string messagesOnMostActiveDayText = $"{stats.MessagesOnMostActiveDay} " +
                GetDeclension(stats.MessagesOnMostActiveDay, "сообщение", "сообщения", "сообщений");

            long ScheduleRequestsSum = stats.ScheduleRequests.Values.Sum();
            string scheduleRequestsText = $"{ScheduleRequestsSum} " +
                GetDeclension(ScheduleRequestsSum, "раз", "раза", "раз");

            string uniqueMessagesText = $"{stats.UniqueMessages} " +
                GetDeclension(stats.UniqueMessages, "уникальное сообщение", "уникальных сообщения", "уникальных сообщений");

            string mostPopularRequestCountText = $"{stats.MostPopularRequestCount} " +
                GetDeclension(stats.MostPopularRequestCount, "раз", "раза", "раз");

            string LongestStreakText = $"{stats.LongestStreak} " +
                GetDeclension(stats.LongestStreak, "день", "дня", "дней");

            // Теперь используйте их в тексте:
            return $"🎉✨ Дорогой друг! ✨🎉\n\n" +
                    $"В этом году мы встретились впервые {stats.FirstMessageInYear:dd.MM.yyyy HH:mm}, и вы написали мне: \"{stats.FirstMessageTextInYear}\". " +
                    $"Это было началом нашей яркой совместной истории в 2024 году! 🌟💫\n\n" +
                    $"***За прошедший год мы с вами сделали так много:***\n\n" +
                    $"🎈 Всего сообщений: {totalMessagesText}.\n" +
                    $"📅 Самый активный день: {stats.MostActiveDay:dd.MM.yyyy} ({messagesOnMostActiveDayText}) 🎉\n" +
                    $"🔥 Самый длинный период активности: {LongestStreakText} подряд.\n" +
                    $"📊 Ваш самый активный день недели — {DayOfWeekInRussian[stats.MostActiveDayOfWeek]}.\n" +
                    $"🕒 Ваше любимое время для общения: {stats.PreferredInteractionTime} — отличный выбор для продуктивности!\n" +
                    $"🔁 Самый часто используемый запрос: \"{stats.MostPopularRequestType}\", вы обращались к нему {mostPopularRequestCountText}.\n\n" +
                    $"✨ ***Ваши достижения:***\n" +
                    $"📚 Вы запросили расписание {scheduleRequestsText}, всегда оставались на волне событий!\n" +
                    $"🎓 Проверили успеваемость {stats.PerformanceRequests.Values.Sum()} раз — невероятная целеустремлённость!\n" +
                    $"💡 Отправили {uniqueMessagesText} — вы настоящий исследователь!\n\n" +
                    $"{globalStats}" +
                    $"Спасибо вам за то, что были со мной этот год! Вы делаете наш диалог тёплым, интересным и таким важным! 💖\n\n" +
                    $"🎄✨ С наступающими праздниками! ✨🎄 Пусть ваш новый год будет наполнен радостью, смехом и счастьем! " +
                    $"Желаю вам успехов во всех начинаниях, больших побед и маленьких радостей каждый день! 🌟💫\n\n" +
                    $"С любовью и радостью,\nВаш верный Телеграм-бот 🤖💙🎁✨";

        }
    }
}
