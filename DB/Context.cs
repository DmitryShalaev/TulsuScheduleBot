using Microsoft.EntityFrameworkCore;

using ScheduleBot.DB.Entity;

namespace ScheduleBot.DB {
    public class ScheduleDbContext : DbContext {
        private System.Timers.Timer? ClearTemporaryTimer;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder.UseNpgsql(Environment.GetEnvironmentVariable("TelegramBotConnectionString"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            foreach(Entity.Class type in Enum.GetValues(typeof(Entity.Class)).Cast<Entity.Class>())
                modelBuilder.Entity<ClassDTO>().HasData(new ClassDTO() { ID = type, Name = type.ToString() });

            foreach(Entity.Mode type in Enum.GetValues(typeof(Entity.Mode)).Cast<Entity.Mode>())
                modelBuilder.Entity<ModeDTO>().HasData(new ModeDTO() { ID = type, Name = type.ToString() });
        }

        public void CleanDB() {
            Database.EnsureDeleted();
            Database.EnsureCreated();
            SaveChanges();
        }

        public void ClearTemporary() {
            if(ClearTemporaryTimer is not null) {
                ClearTemporaryTimer.Stop();
                ClearTemporaryTimer.Dispose();
                ClearTemporaryTimer = null;
            }

            TimeSpan delay = DateTime.Now.AddDays(1) - DateTime.Now;
            ClearTemporaryTimer = new(delay.TotalMilliseconds);
            ClearTemporaryTimer.Elapsed += (o, e) => {
                foreach(var item in TelegramUsers)
                    item.TodayRequests = 0;

                var date = DateOnly.FromDateTime(DateTime.Now);
                CustomDiscipline.RemoveRange(CustomDiscipline.Where(i => i.Date.AddDays(7) < date));

                if(date.Day == 1 && (date.Month == 2 || date.Month == 8))
                    CompletedDisciplines.RemoveRange(CompletedDisciplines);
                else
                    CompletedDisciplines.RemoveRange(CompletedDisciplines.Where(i => i.Date != null && i.Date.Value.AddDays(7) < date));

                SaveChanges();

                ClearTemporary();
            };
            ClearTemporaryTimer.AutoReset = false;
            ClearTemporaryTimer.Enabled = true;
        }

#pragma warning disable CS8618
        public DbSet<Discipline> Disciplines { get; set; }
        public DbSet<Progress> Progresses { get; set; }
        public DbSet<CompletedDiscipline> CompletedDisciplines { get; set; }
        public DbSet<CustomDiscipline> CustomDiscipline { get; set; }
        public DbSet<ClassDTO> Classes { get; set; }
        public DbSet<ModeDTO> Modes { get; set; }
        public DbSet<TelegramUser> TelegramUsers { get; set; }
        public DbSet<ScheduleProfile> ScheduleProfile { get; set; }
        public DbSet<GroupLastUpdate> GroupLastUpdate { get; set; }
        public DbSet<StudentIDLastUpdate> StudentIDLastUpdate { get; set; }
        public DbSet<MessageLog> MessageLog { get; set; }
        public DbSet<Notifications> Notifications { get; set; }
    }
}
