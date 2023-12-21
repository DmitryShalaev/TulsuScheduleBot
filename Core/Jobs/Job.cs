using ScheduleBot.Jobs;

namespace Core.Jobs {
    public static class Job {
        public static async Task InitAsync() {
            await Task.Delay(TimeSpan.FromSeconds(30));

            await UpdatingDisciplinesJob.StartAsync();
            await ClearTemporaryJob.StartAsync();
        }
    }
}
