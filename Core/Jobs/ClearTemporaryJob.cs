using Core.DB;
using Core.Parser;

using Quartz;
using Quartz.Impl;

namespace Core.Jobs {

    /// <summary>
    /// Класс задания для очистки временных данных.
    /// Реализует интерфейс <see cref="IJob"/> для выполнения запланированных задач.
    /// </summary>
    public class ClearTemporaryJob : IJob {

        /// <summary>
        /// Метод запускает задачу для очистки временных данных с использованием планировщика Quartz.
        /// Задание настроено на ежедневное выполнение в полночь (0:00).
        /// </summary>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        public static async Task StartAsync() {
            // Создание фабрики планировщиков
            var schedulerFactory = new StdSchedulerFactory();
            // Получение планировщика
            IScheduler scheduler = await schedulerFactory.GetScheduler();

            // Определение задания ClearTemporaryJob
            IJobDetail job = JobBuilder.Create<ClearTemporaryJob>()
                .WithIdentity("ClearTemporaryJob", "group1") // Уникальный идентификатор задания
                .Build();

            // Определение триггера для запуска задания в полночь
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("ClearTemporaryJobTrigger", "group1") // Уникальный идентификатор триггера
                .StartNow() // Запуск задания сразу после старта планировщика
                .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0, 0)) // Запуск ежедневно в 00:00
                .Build();

            // Запуск планировщика
            await scheduler.Start();
            // Назначение задания и триггера в планировщике
            await scheduler.ScheduleJob(job, trigger);
        }

        /// <summary>
        /// Метод, реализующий логику выполнения задания по очистке временных данных.
        /// Этот метод будет вызываться планировщиком согласно установленному расписанию.
        /// </summary>
        /// <param name="context">Контекст выполнения задания, предоставляемый Quartz.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        async Task IJob.Execute(IJobExecutionContext context) {
            // Выполнение очистки временных данных
            await ClearTemporary.ClearAsync();

            // Получение данных о преподавателях
            await ScheduleParser.Instance.GetTeachersData();
        }
    }
}
