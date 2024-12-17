﻿using Core.DB.Entity;

using Microsoft.EntityFrameworkCore;

using Telegram.Bot.Types.Enums;

namespace Core.DB {
    public class ScheduleDbContext : DbContext {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseNpgsql(Environment.GetEnvironmentVariable("TelegramBotConnectionString"));

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            foreach(Class type in Enum.GetValues<Class>().Cast<Class>())
                modelBuilder.Entity<ClassDTO>().HasData(new ClassDTO() { ID = type, Name = type.ToString() });

            foreach(Mode type in Enum.GetValues<Mode>().Cast<Mode>())
                modelBuilder.Entity<ModeDTO>().HasData(new ModeDTO() { ID = type, Name = type.ToString() });
        }

        public void ClearContext() {
            foreach(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry in ChangeTracker.Entries()) {
                if(entry.State == EntityState.Added) {
                    entry.State = EntityState.Detached;
                } else if(entry.State is EntityState.Modified or EntityState.Deleted) {
                    entry.State = EntityState.Unchanged;
                }
            }
        }

#pragma warning disable CS8618
        public DbSet<Discipline> Disciplines { get; set; }
        public DbSet<DeletedDisciplines> DeletedDisciplines { get; set; }
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
        public DbSet<Settings> Settings { get; set; }
        public DbSet<TelegramUsersTmp> TelegramUsersTmp { get; set; }
        public DbSet<TeacherWorkSchedule> TeacherWorkSchedule { get; set; }
        public DbSet<TeacherLastUpdate> TeacherLastUpdate { get; set; }
        public DbSet<ClassroomLastUpdate> ClassroomLastUpdate { get; set; }
        public DbSet<ClassroomWorkSchedule> ClassroomWorkSchedule { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<IntersectionOfSubgroups> IntersectionOfSubgroups { get; set; }
        public DbSet<MissingFields> MissingFields { get; set; }
        public DbSet<Messenger> Messenger { get; set; }
    }
}
