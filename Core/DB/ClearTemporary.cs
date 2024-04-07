using Microsoft.EntityFrameworkCore;

using ScheduleBot.DB;

namespace Core.DB {
    public static class ClearTemporary {
        public static async Task ClearAsync() {
            using(ScheduleDbContext dbContext = new()) {
   
                foreach(ScheduleBot.DB.Entity.TelegramUser item in dbContext.TelegramUsers)
                    item.TodayRequests = 0;

                var date = DateOnly.FromDateTime(DateTime.Now);
                dbContext.CustomDiscipline.RemoveRange(dbContext.CustomDiscipline.Where(i => i.Date.AddMonths(1) < date));

                dbContext.DeletedDisciplines.RemoveRange(dbContext.DeletedDisciplines.Where(i => i.DeleteDate.AddDays(5) < date));

                if(date.Day == 1 && (date.Month == 2 || date.Month == 8))
                    dbContext.CompletedDisciplines.RemoveRange(dbContext.CompletedDisciplines);

                dbContext.MessageLog.RemoveRange(dbContext.MessageLog.Where(i => i.Date.AddMonths(1) < DateTime.UtcNow));

                try {
                    await dbContext.SaveChangesAsync();
                } catch(DbUpdateException ex) {
                    await Console.Out.WriteLineAsync(ex.Message);
                }

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
