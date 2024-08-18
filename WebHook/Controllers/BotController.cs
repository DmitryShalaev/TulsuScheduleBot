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

                Task.Run(() => {
            Task.Factory.StartNew(TelegramBot.Instance.UpdateAsync(update), TaskCreationOptions.LongRunning);
});

            } catch(Exception e) {
                Console.WriteLine(e.Message);
            }
        }

        [HttpGet]
        public string Get() => "Telegram bot was started";
    }
}