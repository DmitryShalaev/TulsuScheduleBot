using Microsoft.AspNetCore.Mvc;

using ScheduleBot;

using Telegram.Bot.Types;

namespace WebHook.Controllers {

    [ApiController]
    [Route("/")]
    public class BotController : ControllerBase {

        [HttpPost]
        public async Task PostAsync(Update update) => await Core.GetInstance().UpdateAsync(update);

        [HttpGet]
        public string Get() => "Telegram bot was started";
    }
}