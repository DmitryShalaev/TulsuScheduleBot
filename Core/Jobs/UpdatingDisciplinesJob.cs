using Core.Bot.Commands;

using Microsoft.EntityFrameworkCore;

using Quartz;
using Quartz.Impl;

using ScheduleBot.DB;

namespace ScheduleBot.Jobs {
    public class UpdatingDisciplinesJob : IJob {
        private static DateTime dateTime = DateTime.Now;

        private static readonly UserCommands.ConfigStruct config = UserCommands.Instance.Config;

        public static async Task StartAsync() {
            using(ScheduleDbContext dbContext = new()) {
                Parser instance = Parser.Instance;

                foreach(string item in dbContext.ScheduleProfile.Where(i => !string.IsNullOrEmpty(i.Group) && (DateTime.Now - i.LastAppeal.ToLocalTime()).TotalDays <= config.DisciplineUpdateDays).Select(i => i.Group!).Distinct().ToList())
                    await instance.UpdatingDisciplines(dbContext, group: item, updateAttemptTime: 0);
            }

            dateTime = DateTime.Now;

            ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
            IScheduler scheduler = await schedulerFactory.GetScheduler();

            IJobDetail job = JobBuilder.Create<UpdatingDisciplinesJob>().WithIdentity("UpdatingDisciplinesJob", "group1").Build();

            ITrigger trigger = TriggerBuilder.Create().WithIdentity("UpdatingDisciplinesJobTrigger", "group1")
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(GetUpdateInterval())
                    .RepeatForever())
                .Build();

            await scheduler.Start();
            await scheduler.ScheduleJob(job, trigger);
        }

        async Task IJob.Execute(IJobExecutionContext context) {
            using(ScheduleDbContext dbContext = new()) {
                Parser instance = Parser.Instance;

                if((DateTime.Now - dateTime).Minutes >= config.DisciplineUpdateTime) {
                    dateTime = DateTime.Now;

                    ITrigger oldTrigger = context.Trigger;
                    ITrigger newTrigger = TriggerBuilder.Create()
                        .WithIdentity(oldTrigger.Key.Name, oldTrigger.Key.Group)
                        .WithSimpleSchedule(x => x
                            .WithIntervalInSeconds(GetUpdateInterval())
                            .RepeatForever())
                        .Build();

                    await context.Scheduler.RescheduleJob(oldTrigger.Key, newTrigger);

                    return;
                }

                int DisciplineUpdateDays = UserCommands.Instance.Config.DisciplineUpdateDays;

                IQueryable<string> tmp = dbContext.ScheduleProfile.Where(i => !string.IsNullOrEmpty(i.Group) && (DateTime.Now - i.LastAppeal.ToLocalTime()).TotalDays <= DisciplineUpdateDays).Select(i => i.Group!).Distinct();

                string group = dbContext.GroupLastUpdate.Where(i => tmp.Contains(i.Group)).OrderBy(i => i.UpdateAttempt).First().Group;

                await instance.UpdatingDisciplines(dbContext, group: group, updateAttemptTime: 0);
            }
        }

        private static int GetUpdateInterval() {
            using(ScheduleDbContext dbContext = new()) {
                int tmp = dbContext.ScheduleProfile.Where(i => !string.IsNullOrEmpty(i.Group) && (DateTime.Now - i.LastAppeal.ToLocalTime()).TotalDays <= config.DisciplineUpdateDays).Select(i => i.Group!).Distinct().Count();
                int sec = (int)Math.Floor((config.DisciplineUpdateTime - 1.0) * 60.0 / (tmp == 0 ? 1.0 : tmp));

                return sec;
            }
        }
    }
}
