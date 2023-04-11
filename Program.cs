using System.Globalization;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;
using ScheduleBot.Scheduler;

namespace ScheduleBot {
    public class Program {
        static void Main(string[] args) {
            while(true) {
                try {
                    CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("ru-RU");

                    ScheduleDbContext dbContext = new();
                    Parser parser = new(dbContext);
                    Scheduler.Scheduler scheduler = new(dbContext);
                    Bot.TelegramBot telegramBot = new(scheduler, dbContext);

                    Thread.Sleep(-1);
                } catch(Exception e) {

                    Console.WriteLine(e);
                }

            }
        }
    }
}