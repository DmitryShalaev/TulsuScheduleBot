using System.Globalization;

using Microsoft.AspNetCore.Localization;

namespace WebHook {
    public class Program {
        public static void Main(string[] args) {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.Logging.SetMinimumLevel(LogLevel.Warning);

            builder.Services.AddControllers().AddNewtonsoftJson();

            WebApplication app = builder.Build();

            CultureInfo cultureInfo = new("ru-RU");
            cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
            cultureInfo.NumberFormat.CurrencyDecimalSeparator = ".";

            app.UseRequestLocalization(new RequestLocalizationOptions {
                DefaultRequestCulture = new RequestCulture(cultureInfo),
                SupportedCultures = [cultureInfo],
                SupportedUICultures = [cultureInfo]
            });

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}