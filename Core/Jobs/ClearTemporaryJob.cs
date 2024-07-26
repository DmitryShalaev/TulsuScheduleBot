using Core.DB;

using Quartz;
using Quartz.Impl;

namespace ScheduleBot.Jobs {
    public class ClearTemporaryJob : IJob {
        public static async Task StartAsync() {
            var schedulerFactory = new StdSchedulerFactory();
            IScheduler scheduler = await schedulerFactory.GetScheduler();

            IJobDetail job = JobBuilder.Create<ClearTemporaryJob>().WithIdentity("ClearTemporaryJob", "group1").Build();

            ITrigger trigger = TriggerBuilder.Create().WithIdentity("ClearTemporaryJobTrigger", "group1")
            .StartNow()
            .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0, 0))
            .Build();

            await scheduler.Start();
            await scheduler.ScheduleJob(job, trigger);
        }

        async Task IJob.Execute(IJobExecutionContext context) {
            await ClearTemporary.ClearAsync();

            await Parser.Instance.GetTeachersData();
        }
    }
}
