using ScheduleBot.Jobs;

namespace Core.Jobs {
    public static class Job {
        public static void Init() {
            UpdatingDisciplinesJob.StartAsync().Wait();
            ClearTemporaryJob.StartAsync().Wait();
        }
    }
}
