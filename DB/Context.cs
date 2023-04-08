using Microsoft.EntityFrameworkCore;

using ScheduleBot.DB.Entity;

namespace ScheduleBot.DB {
    public class ScheduleDbContext : DbContext {

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlite($"Data Source={Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "MyListDB.db")}");

        protected override void OnModelCreating(ModelBuilder modelBuilder) {

        }

        public void CleanDB() {
            Database.EnsureDeleted();
            Database.EnsureCreated();
            SaveChanges();
        }

#pragma warning disable CS8618
        public DbSet<Disciplines> Disciplines { get; set; }

    }
}
