using Microsoft.EntityFrameworkCore;

using ScheduleBot.Bot;
using ScheduleBot.DB;
using ScheduleBot.Jobs;

namespace ScheduleBot {
    public static class Core {

        public static TelegramBot InitBot() {
            if(string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TelegramBotToken")) ||
               string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TelegramBotConnectionString")))
                throw new NullReferenceException("Environment Variable is null");

            using(ScheduleDbContext dbContext = new())
                dbContext.Database.Migrate();

            ClearTemporaryJob.StartAsync().Wait();

            TelegramBot telegramBot = new();

            return telegramBot;
        }
    }
}