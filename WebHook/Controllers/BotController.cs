using Microsoft.AspNetCore.Mvc;

using Telegram.Bot.Types;

namespace WebHook.Controllers {

    [ApiController]
    [Route("/")]
    public class BotController(TelegramUpdateBackgroundService backgroundService) : ControllerBase {

        private readonly TelegramUpdateBackgroundService _backgroundService = backgroundService;

        [HttpPost]
        public void Post([FromBody] Update update) => _backgroundService.ProcessUpdateAsync(update);

        [HttpGet]
        public string Get() => "Telegram bot was started";
    }
}