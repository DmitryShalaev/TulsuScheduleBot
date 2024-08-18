using Core.Bot;

using Microsoft.AspNetCore.Mvc;

using Telegram.Bot.Types;

namespace WebHook.Controllers {

    [ApiController]
    [Route("/")]
    public class BotController : ControllerBase {

       [HttpPost]
        public async Task PostAsync(Update update) => await TelegramBot.Instance.UpdateAsync(update);

        [HttpGet]
        public string Get() => "Telegram bot was started";
    }
}