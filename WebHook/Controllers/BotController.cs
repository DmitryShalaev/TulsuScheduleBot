using Microsoft.AspNetCore.Mvc;

using ScheduleBot.Bot;

using Telegram.Bot.Types;

namespace WebHook.Controllers {
    [ApiController]
    [Route("/")]
    public class BotController : Controller {
        private readonly TelegramBot _bot;

        public BotController(TelegramBot bot) => _bot = bot;

        [HttpPost]
        public async Task PostAsync(Update update) {

            try {
                await _bot.UpdateAsync(update);
            } catch(Exception e) {

                await Console.Out.WriteLineAsync(e.Message);
            }
        }

        [HttpGet]
        public string Get() => "Telegram bot was started";
    }
}
