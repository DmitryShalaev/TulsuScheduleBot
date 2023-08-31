using System.Globalization;

using ScheduleBot;

#if !DEBUG
using System.Net.Mail;
using System.Net;
#endif

namespace LongPolling {
    internal class Program {
        static void Main() {

            CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("ru-RU");

            try {
                Core.InitBot().ReceiveAsync().Wait();

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