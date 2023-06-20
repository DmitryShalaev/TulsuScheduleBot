using Microsoft.EntityFrameworkCore;

using ScheduleBot.DB.Entity;

namespace ScheduleBot.DB {
    public class ScheduleDbContext : DbContext {

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder.UseNpgsql(Environment.GetEnvironmentVariable("TelegramBotConnectionString"));
#if DEBUG
            //optionsBuilder.LogTo(Console.WriteLine, new[] { RelationalEventId.CommandExecuted });
#endif
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
