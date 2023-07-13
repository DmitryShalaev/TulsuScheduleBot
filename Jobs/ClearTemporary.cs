using Quartz;
using Quartz.Impl;

using ScheduleBot.DB;

namespace ScheduleBot.Jobs {
    public class ClearTemporaryJob : IJob {
        public static async Task StartAsync() {
            ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
            IScheduler scheduler = await schedulerFactory.GetScheduler();

            IJobDetail job = JobBuilder.Create<ClearTemporaryJob>().WithIdentity("ClearTemporaryJob", "group1").Build();

            ITrigger trigger = TriggerBuilder.Create().WithIdentity("ClearTemporaryJobTrigger", "group1")
            .StartAt(DateBuilder.TomorrowAt(0, 0, 0)).WithSimpleSchedule(x => x
                .WithIntervalInHours(24)
                .RepeatForever())
            .Build();

            await scheduler.Start();
            await scheduler.ScheduleJob(job, trigger);
        }

        Task IJob.Execute(IJobExecutionContext context) {
            using(ScheduleDbContext dbContext = new()) {
                foreach(DB.Entity.TelegramUser item in dbContext.TelegramUsers)
                    item.TodayRequests = 0;

                var date = DateOnly.FromDateTime(DateTime.Now);
                dbContext.CustomDiscipline.RemoveRange(dbContext.CustomDiscipline.Where(i => i.Date.AddDays(7) < date));

                dbContext.CompletedDisciplines.RemoveRange(
                    date.Day == 1 && (date.Month == 2 || date.Month == 8) ?
                    dbContext.CompletedDisciplines : dbContext.CompletedDisciplines.Where(i => i.Date != null && i.Date.Value.AddDays(7) < date)
                );

                dbContext.SaveChanges();
            }

            return Task.CompletedTask;
        }
    }
}
