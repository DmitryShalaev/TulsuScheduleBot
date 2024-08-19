using Microsoft.AspNetCore.Mvc;

using Telegram.Bot.Types;

namespace WebHook.Controllers {

    [ApiController]
    [Route("/")]
    public class BotController : ControllerBase {

        private readonly TelegramUpdateBackgroundService _backgroundService;

        public BotController(TelegramUpdateBackgroundService backgroundService) => _backgroundService = backgroundService;

        [HttpPost]
        public void Post([FromBody] Update update) => _backgroundService.ProcessUpdateAsync(update);

        [HttpGet]
        public string Get() => "Telegram bot was started";
    }
}