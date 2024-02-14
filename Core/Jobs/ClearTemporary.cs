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

        async Task IJob.Execute(IJobExecutionContext context) {
            using(ScheduleDbContext dbContext = new()) {
                foreach(DB.Entity.TelegramUser item in dbContext.TelegramUsers)
                    item.TodayRequests = 0;

                var date = DateOnly.FromDateTime(DateTime.Now);
                dbContext.CustomDiscipline.RemoveRange(dbContext.CustomDiscipline.Where(i => i.Date.AddMonths(1) < date));

                dbContext.DeletedDisciplines.RemoveRange(dbContext.DeletedDisciplines.Where(i => i.DeleteDate.AddDays(5) < date));

                if(date.Day == 1 && (date.Month == 2 || date.Month == 8))
                    dbContext.CompletedDisciplines.RemoveRange(dbContext.CompletedDisciplines);

                dbContext.MessageLog.RemoveRange(dbContext.MessageLog.Where(i => i.Date.AddMonths(1) < DateTime.UtcNow));

                await Parser.Instance.GetTeachersData();

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
