using ScheduleBot.Bot;
using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

namespace ScheduleBot {
    public class Program {
        static void Main(string[] args) {
            ScheduleDbContext dbContext = new();

            dbContext.CleanDB();
            
            dbContext.CompletedDisciplines.AddRange(new List<CompletedDiscipline>(){
                new() { Name = "Прикладные компьютерные технологии", Class = DB.Entity.Type.lab },
                new() { Name = "Численные методы", Class = DB.Entity.Type.lab },
                new() { Name = "Компьютерные сети и телекоммуникации", Class = DB.Entity.Type.all },
                new() { Name = "Специальные разделы высшей математики", Class = DB.Entity.Type.all },
                new() { Name = "Объектно-ориентированное программирование", Class = DB.Entity.Type.lab },
            });
            
            dbContext.SaveChanges();

            Parser parser = new(dbContext);
            Bot.TelegramBot telegramBot = new(dbContext);

            Console.ReadLine();
        }
    }
}