using System.Globalization;

using Microsoft.AspNetCore.Localization;

using ScheduleBot;

namespace WebHook {
    public class Program {
        public static void Main(string[] args) {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.Logging.SetMinimumLevel(LogLevel.Warning);

            builder.Services.AddControllers().AddNewtonsoftJson();

            _ = Core.GetInstance();
            builder.Services.AddScoped(p => Core.GetInstance());

            WebApplication app = builder.Build();

            CultureInfo cultureInfo = new("ru-RU");
            cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
            cultureInfo.NumberFormat.CurrencyDecimalSeparator = ".";

            app.UseRequestLocalization(new RequestLocalizationOptions {
                DefaultRequestCulture = new RequestCulture(cultureInfo),
                SupportedCultures = new List<CultureInfo> { cultureInfo },
                SupportedUICultures = new List<CultureInfo> { cultureInfo }
            });

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}