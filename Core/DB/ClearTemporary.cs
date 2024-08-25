using Core.DB.Entity;

namespace Core.DB {
    public static class ClearTemporary {
        public static async Task ClearAsync() {
            using(ScheduleDbContext dbContext = new()) {

                foreach(TelegramUser item in dbContext.TelegramUsers)
                    item.TodayRequests = 0;

                var date = DateOnly.FromDateTime(DateTime.Now);

                dbContext.DeletedDisciplines.RemoveRange(dbContext.DeletedDisciplines.Where(i => i.DeleteDate.AddDays(5) < date));

                if(date.Day == 1 && (date.Month == 2 || date.Month == 8))
                    dbContext.CompletedDisciplines.RemoveRange(dbContext.CompletedDisciplines);

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
