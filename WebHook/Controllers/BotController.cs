using Core.Bot;

using Microsoft.AspNetCore.Mvc;

using Telegram.Bot.Types;

namespace WebHook.Controllers {

    [ApiController]
    [Route("/")]
    public class BotController : ControllerBase {

        [HttpPost]
        public void Post(Update update) {
            try {

                Task.Run(async () => await TelegramBot.Instance.UpdateAsync(update));

            } catch(Exception e) {
                Console.WriteLine(e.Message);
            }
        }

        [HttpGet]
        public string Get() => "Telegram bot was started";
    }
}