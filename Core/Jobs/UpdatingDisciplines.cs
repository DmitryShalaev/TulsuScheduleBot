using Quartz;
using Quartz.Impl;

using ScheduleBot.Bot;
using ScheduleBot.DB;

namespace ScheduleBot.Jobs {
    public class UpdatingDisciplinesJob : IJob {
        public static async Task StartAsync() {
            using(ScheduleDbContext dbContext = new()) {
                (DateOnly min, DateOnly max)? dates = null;

                Parser parser = Parser.Instance!;

                string? group = dbContext.GroupLastUpdate.FirstOrDefault()?.Group;
                if(group is not null)
                    dates = await parser.GetDates(group);

                BotCommands.ConfigStruct config = BotCommands.GetInstance().Config;

                foreach(string item in dbContext.ScheduleProfile.Where(i => !string.IsNullOrEmpty(i.Group) && (DateTime.Now - i.LastAppeal.ToLocalTime()).TotalDays <= config.DisciplineUpdateDays).Select(i => i.Group!).Distinct().ToList())
                    await parser.UpdatingDisciplines(dbContext, group: item, updateAttemptTime: config.DisciplineUpdateTime - 1, dates: dates);
            }

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

        private static (DateOnly min, DateOnly max)? dates = null;
        private static DateTime dateTime = DateTime.Now;

        async Task IJob.Execute(IJobExecutionContext context) {
            using(ScheduleDbContext dbContext = new()) {
                Parser parser = Parser.Instance!;

                if(dates is null || (DateTime.Now - dateTime).Hours >= 1) {
                    dateTime = DateTime.Now;

                    string? group = dbContext.GroupLastUpdate.FirstOrDefault()?.Group;
                    if(group is not null)
                        dates = await parser.GetDates(group);
                }

                int DisciplineUpdateDays = BotCommands.GetInstance().Config.DisciplineUpdateDays;

                IQueryable<string> tmp = dbContext.ScheduleProfile.Where(i => !string.IsNullOrEmpty(i.Group) && (DateTime.Now - i.LastAppeal.ToLocalTime()).TotalDays <= DisciplineUpdateDays).Select(i => i.Group!).Distinct();

                await parser.UpdatingDisciplines(dbContext, group: dbContext.GroupLastUpdate.Where(i => tmp.Contains(i.Group)).OrderBy(i => i.Update).First().Group, updateAttemptTime: 1, dates: dates);
            }
        }

        private static int GetUpdateInterval() {
            using(ScheduleDbContext dbContext = new()) {
                BotCommands.ConfigStruct config = BotCommands.GetInstance().Config;

                int tmp = dbContext.ScheduleProfile.Where(i => !string.IsNullOrEmpty(i.Group) && (DateTime.Now - i.LastAppeal.ToLocalTime()).TotalDays <= config.DisciplineUpdateDays).Select(i => i.Group!).Distinct().Count();

                return (int)Math.Floor((config.DisciplineUpdateTime - 1.0) * 60.0 / (tmp == 0 ? 1.0 : tmp));
            }
        }
    }
}
