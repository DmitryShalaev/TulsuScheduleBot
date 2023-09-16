using System.Globalization;

using Microsoft.AspNetCore;

using ScheduleBot;

namespace WebHook {
    public class Program {
        public static void Main(string[] args) {
            CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("ru-RU");

            WebHost.CreateDefaultBuilder(args).UseUrls("https://*:5000");

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers().AddNewtonsoftJson();
            
            builder.Services.AddScoped(p => Core.GetInstance());

            WebApplication app = builder.Build();
          
            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}