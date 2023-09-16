using Quartz;
using Quartz.Impl;

using ScheduleBot.Bot;
using ScheduleBot.DB;

namespace ScheduleBot.Jobs {
    public class UpdatingDisciplinesJob : IJob {
        public static async Task StartAsync() {
            ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
            IScheduler scheduler = await schedulerFactory.GetScheduler();

            IJobDetail job = JobBuilder.Create<UpdatingDisciplinesJob>().WithIdentity("UpdatingDisciplinesJob", "group1").Build();

            ITrigger trigger = TriggerBuilder.Create().WithIdentity("UpdatingDisciplinesJobTrigger", "group1")
            .WithSimpleSchedule(x => x
                .WithIntervalInMinutes(BotCommands.GetInstance().Config.DisciplineUpdateTime)
                .RepeatForever())
            .Build();

            await scheduler.Start();
            await scheduler.ScheduleJob(job, trigger);
        }

        async Task IJob.Execute(IJobExecutionContext context) {
            using(ScheduleDbContext dbContext = new()) {
                (DateOnly min, DateOnly max)? dates = null;

                string? group = dbContext.GroupLastUpdate.FirstOrDefault()?.Group;
                if(group is not null)
                    dates = await Parser.Instance!.GetDates(group);

                foreach(string item in dbContext.ScheduleProfile.Where(i => !string.IsNullOrEmpty(i.Group) && (DateTime.Now - i.LastAppeal.ToLocalTime()).TotalDays <= 7).Select(i => i.Group!).Distinct().ToList())
                    await Parser.Instance!.UpdatingDisciplines(dbContext, group: item, updateAttemptTime: 0, dates: dates);
            }
        }
    }
}
