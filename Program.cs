using System.Globalization;

#if !DEBUG
using System.Net.Mail;
using System.Net;
#endif

using Microsoft.EntityFrameworkCore;

using ScheduleBot.DB;
using ScheduleBot.Jobs;

namespace ScheduleBot {
    public class Program {
        static void Main(string[] args) {
            if(string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TelegramBotToken")) ||
                string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TelegramBotConnectionString"))
#if !DEBUG
                || string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TelegramBot_FromEmail")) ||
                string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TelegramBot_ToEmail")) ||
                string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TelegramBot_PassEmail"))
#endif
                ) {
                Console.Error.WriteLine("Environment Variable is null");
                return;
            }

            try {
                CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("ru-RU");

                using(ScheduleDbContext dbContext = new()) {
                    dbContext.Database.Migrate();

                    ClearTemporaryJob.StartAsync().Wait();

                    Bot.TelegramBot telegramBot = new();
                }
            } catch(Exception e) {
                Console.WriteLine(e);
#if !DEBUG
                MailAddress from = new(Environment.GetEnvironmentVariable("TelegramBot_FromEmail") ?? "", "Error");
                MailAddress to = new(Environment.GetEnvironmentVariable("TelegramBot_ToEmail") ?? "");
                MailMessage message = new(from, to)
                {
                    Subject = "Error",
                    Body = e.ToString()
                };

                SmtpClient smtp = new("smtp.yandex.ru", 25)
                {
                    Credentials = new NetworkCredential(Environment.GetEnvironmentVariable("TelegramBot_FromEmail"), Environment.GetEnvironmentVariable("TelegramBot_PassEmail")),
                    EnableSsl = true
                };
                smtp.SendMailAsync(message).Wait();
#endif
            }
        }
    }
}