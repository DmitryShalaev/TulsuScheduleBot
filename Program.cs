using System.Linq;

using ScheduleBot.DB;

using static System.Net.Mime.MediaTypeNames;

namespace ScheduleBot {
    public class Program {
        static void Main(string[] args) {
            ScheduleDbContext dbContext = new();
            //dbContext.CleanDB();
            //dbContext.SaveChanges();

            Parser parser = new(dbContext);


        }
    }
}