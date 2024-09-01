using System.Globalization;

using Core.Bot;

using Microsoft.AspNetCore.Localization;

using Telegram.Bot.Types;

namespace WebHook {

    public class TelegramUpdateBackgroundService {
        public async void ProcessUpdateAsync(Update update) => await TelegramBot.Instance.UpdateAsync(update);
    }

    public class Program {
        public static void Main(string[] args) {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.Logging.SetMinimumLevel(LogLevel.Warning);

            builder.Services.AddControllers().AddNewtonsoftJson();
            builder.Services.AddSingleton<TelegramUpdateBackgroundService>();

            WebApplication app = builder.Build();

            CultureInfo cultureInfo = new("ru-RU") { DateTimeFormat = { FirstDayOfWeek = DayOfWeek.Monday }, NumberFormat = { NumberDecimalSeparator = ".", CurrencyDecimalSeparator = "." } };

            app.UseRequestLocalization(new RequestLocalizationOptions {
                DefaultRequestCulture = new RequestCulture(cultureInfo),
                SupportedCultures = [cultureInfo],
                SupportedUICultures = [cultureInfo]
            });

            var standardTimeZone = TimeZoneInfo.CreateCustomTimeZone(
                id: "Russian Standard Time (No DST)",
                baseUtcOffset: new TimeSpan(3, 0, 0),
                displayName: "Moscow Standard Time (No DST)",
                standardDisplayName: "Moscow Standard Time"
            );

            app.Use(async (context, next) => {
                TimeZoneInfo.ClearCachedData();
                CultureInfo.CurrentCulture = cultureInfo;
                CultureInfo.CurrentUICulture = cultureInfo;
                await next.Invoke();
            });

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}