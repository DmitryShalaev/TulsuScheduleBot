namespace Core.Jobs {

    /// <summary>
    /// Статический класс <c>Job</c> используется для инициализации и запуска запланированных задач.
    /// </summary>
    public static class Job {

        /// <summary>
        /// Асинхронный метод для инициализации и запуска запланированных заданий.
        /// </summary>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        public static async Task InitAsync() {
            // Задержка выполнения на 30 секунд
            await Task.Delay(TimeSpan.FromSeconds(30));

            // Запуск задания обновления дисциплин
            await UpdatingDisciplinesJob.StartAsync();
            // Запуск задания очистки временных данных
            await ClearTemporaryJob.StartAsync();
        }
    }
}
