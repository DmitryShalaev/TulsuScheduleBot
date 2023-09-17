using System.Globalization;

using Microsoft.AspNetCore.Localization;

using Newtonsoft.Json;

using ScheduleBot;
using ScheduleBot.Bot;

using Telegram.Bot.Types;

namespace WebHook {
    public class Program {
        private static readonly TelegramBot _bot = Core.GetInstance();

        public static void Main(string[] args) {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.Logging.SetMinimumLevel(LogLevel.Warning);

            builder.Services.AddAuthorization();

            WebApplication app = builder.Build();

            CultureInfo cultureInfo = new("ru-RU");
            cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
            cultureInfo.NumberFormat.CurrencyDecimalSeparator = ".";

            app.UseRequestLocalization(new RequestLocalizationOptions {
                DefaultRequestCulture = new RequestCulture(cultureInfo),
                SupportedCultures = new List<CultureInfo> { cultureInfo },
                SupportedUICultures = new List<CultureInfo> { cultureInfo }
            });

            app.UseAuthorization();

            app.MapPost("/", async (HttpContext context) => {
                try {
                    using StreamReader streamReader = new(context.Request.Body);
                    string str = await streamReader.ReadToEndAsync();

                    await _bot.UpdateAsync(JsonConvert.DeserializeObject<Update>(str)!);

                } catch(Exception e) {
                    await Console.Out.WriteLineAsync(e.Message);
                }
            });

            app.MapGet("/", () => "Telegram bot was started");

            app.Run();
        }
    }
}