using Core.Bot.Commands;
using Core.DB;
using Core.Parser;

using Microsoft.EntityFrameworkCore;

using Quartz;
using Quartz.Impl;

namespace Core.Jobs {

    /// <summary>
    /// Класс задания для обновления дисциплин.
    /// Реализует интерфейс <see cref="IJob"/> для выполнения запланированных задач.
    /// </summary>
    public class UpdatingDisciplinesJob : IJob {

        /// <summary>
        /// Поле для отслеживания последнего времени выполнения задания.
        /// </summary>
        private static DateTime dateTime = DateTime.Now;

        /// <summary>
        /// Конфигурационные параметры для обновления дисциплин.
        /// </summary>
        private static readonly UserCommands.ConfigStruct config = UserCommands.Instance.Config;

        /// <summary>
        /// Асинхронный метод для запуска задания обновления дисциплин.
        /// Инициализирует планировщик Quartz и настраивает триггер для периодического выполнения.
        /// </summary>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        public static async Task StartAsync() {
            using(ScheduleDbContext dbContext = new()) {
                ScheduleParser instance = ScheduleParser.Instance;

                // Обновление дисциплин для групп, которые обновлялись не позже, чем config.DisciplineUpdateDays дней назад
                foreach(string item in dbContext.ScheduleProfile
                    .Where(i => !string.IsNullOrEmpty(i.Group) && (DateTime.Now - i.LastAppeal.ToLocalTime()).TotalDays <= config.DisciplineUpdateDays)
                    .Select(i => i.Group!)
                    .Distinct()
                    .ToList()) {
                    await instance.UpdatingDisciplines(dbContext, group: item, updateAttemptTime: 0);
                }
            }

            // Обновление времени последнего выполнения задания
            dateTime = DateTime.Now;

            // Настройка планировщика Quartz
            var schedulerFactory = new StdSchedulerFactory();
            IScheduler scheduler = await schedulerFactory.GetScheduler();

            // Определение задания
            IJobDetail job = JobBuilder.Create<UpdatingDisciplinesJob>()
                .WithIdentity("UpdatingDisciplinesJob", "group1")
                .Build();

            // Определение триггера для выполнения задания через определенные интервалы
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("UpdatingDisciplinesJobTrigger", "group1")
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(GetUpdateInterval()) // Интервал обновления
                    .RepeatForever())
                .Build();

            // Запуск планировщика и задание триггера
            await scheduler.Start();
            await scheduler.ScheduleJob(job, trigger);
        }

        /// <summary>
        /// Метод, реализующий логику выполнения задания по обновлению дисциплин.
        /// Этот метод будет вызываться планировщиком согласно установленному расписанию.
        /// </summary>
        /// <param name="context">Контекст выполнения задания, предоставляемый Quartz.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        async Task IJob.Execute(IJobExecutionContext context) {
            using(ScheduleDbContext dbContext = new()) {
                ScheduleParser instance = ScheduleParser.Instance;

                // Проверка, прошло ли достаточно времени с момента последнего обновления
                if((DateTime.Now - dateTime).Minutes >= config.DisciplineUpdateTime) {
                    // Обновление времени последнего выполнения задания
                    dateTime = DateTime.Now;

                    // Перенастройка триггера для задания
                    ITrigger oldTrigger = context.Trigger;
                    ITrigger newTrigger = TriggerBuilder.Create()
                        .WithIdentity(oldTrigger.Key.Name, oldTrigger.Key.Group)
                        .WithSimpleSchedule(x => x
                            .WithIntervalInSeconds(GetUpdateInterval())
                            .RepeatForever())
                        .Build();

                    // Перезапуск триггера с новым интервалом
                    await context.Scheduler.RescheduleJob(oldTrigger.Key, newTrigger);

                    return;
                }

                // Получение групп для обновления дисциплин
                int DisciplineUpdateDays = UserCommands.Instance.Config.DisciplineUpdateDays;
                IQueryable<string> tmp = dbContext.ScheduleProfile
                    .Where(i => !string.IsNullOrEmpty(i.Group) && (DateTime.Now - i.LastAppeal.ToLocalTime()).TotalDays <= DisciplineUpdateDays)
                    .Select(i => i.Group!)
                    .Distinct();

                // Получение группы с минимальным количеством попыток обновления
                string group = dbContext.GroupLastUpdate
                    .Where(i => tmp.Contains(i.Group))
                    .OrderBy(i => i.UpdateAttempt)
                    .First()
                    .Group;

                // Обновление дисциплин для выбранной группы
                await instance.UpdatingDisciplines(dbContext, group: group, updateAttemptTime: 0);
            }
        }

        /// <summary>
        /// Метод для вычисления интервала обновления дисциплин в секундах.
        /// Интервал зависит от количества групп, которые требуют обновления.
        /// </summary>
        /// <returns>Интервал обновления в секундах.</returns>
        private static int GetUpdateInterval() {
            using(ScheduleDbContext dbContext = new()) {
                // Подсчет количества групп, требующих обновления дисциплин
                int tmp = dbContext.ScheduleProfile
                    .Where(i => !string.IsNullOrEmpty(i.Group) && (DateTime.Now - i.LastAppeal.ToLocalTime()).TotalDays <= config.DisciplineUpdateDays)
                    .Select(i => i.Group!)
                    .Distinct()
                    .Count();

                return Math.Max((int)Math.Floor((config.DisciplineUpdateTime - 1.0) * 60.0 / (tmp == 0 ? 1.0 : tmp)), 30);
            }
        }
    }
}
