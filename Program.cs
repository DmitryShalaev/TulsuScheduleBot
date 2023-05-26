using System.Globalization;

#if !DEBUG
using System.Net.Mail;
using System.Net;
#endif

using Microsoft.EntityFrameworkCore;

using ScheduleBot.DB;

namespace ScheduleBot {
    public class Program {
        static void Main(string[] args) {
            if(string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TelegramBotToken")) || string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TelegramBotConnectionString"))
#if !DEBUG
                || string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TelegramBot_FromEmail")) ||
                string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TelegramBot_ToEmail")) ||
                string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TelegramBot_PassEmail"))
#endif
) {
                Console.Error.WriteLine("Environment Variable is null");
                return;
            }

            while(true) {
                try {
                    CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("ru-RU");

                    ScheduleDbContext dbContext = new();
                    dbContext.Database.Migrate();

                    Parser parser = new(dbContext);
                    Scheduler.Scheduler scheduler = new(dbContext);
                    Bot.TelegramBot telegramBot = new(scheduler, dbContext);

                } catch(Exception e) {
                    Console.WriteLine(e.Message);

#if !DEBUG
                    MailAddress from = new MailAddress(Environment.GetEnvironmentVariable("TelegramBot_FromEmail") ?? "", "Error");
                    MailAddress to = new MailAddress(Environment.GetEnvironmentVariable("TelegramBot_ToEmail") ?? "");
                    MailMessage message = new MailMessage(from, to);
                    message.Subject = "Error";
                    message.Body = e.Message;

                    SmtpClient smtp = new SmtpClient("smtp.yandex.ru", 25);
                    smtp.Credentials = new NetworkCredential(Environment.GetEnvironmentVariable("TelegramBot_FromEmail"), Environment.GetEnvironmentVariable("TelegramBot_PassEmail"));
                    smtp.EnableSsl = true;
                    smtp.SendMailAsync(message).Wait();
#endif
                }

            }
        }
    }
}