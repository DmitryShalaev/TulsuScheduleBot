using Microsoft.EntityFrameworkCore;

using ScheduleBot.DB.Entity;

namespace ScheduleBot.DB {
    public class ScheduleDbContext : DbContext {

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite($"Data Source={Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "ScheduleDB.db")}");

        protected override void OnModelCreating(ModelBuilder modelBuilder) {

        }

        public void CleanDB() {
            Database.EnsureDeleted();
            Database.EnsureCreated();
            SaveChanges();
        }

        public IQueryable<Discipline> GetDisciplinesBetweenDates((DateOnly min, DateOnly max) dates) => Disciplines.Where(i => i.Date >= dates.min && i.Date <= dates.max);


#pragma warning disable CS8618
        public DbSet<Discipline> Disciplines { get; set; }
    }
}
