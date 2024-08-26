#if !DEBUG
using System.Net;
using System.Net.Mail;
#endif

namespace Core.Bot {
    public static class ErrorReport {

        public static async Task Send(string msg, Exception e) {
            await Console.Out.WriteLineAsync($"{msg}\n{new('-', 25)}");
            await Console.Out.WriteLineAsync($"{e.Message}");
#if !DEBUG
            MailAddress from = new(Environment.GetEnvironmentVariable("TelegramBot_FromEmail") ?? "", "Error");
            MailAddress to = new(Environment.GetEnvironmentVariable("TelegramBot_ToEmail") ?? "");
            MailMessage mailMessage = new(from, to) {
                Subject = "Error",
                Body = $"{msg}\n{new('-', 25)}\n{e.Message}\n{new('-', 25)}\n{e}"
            };

            SmtpClient smtp = new("smtp.yandex.ru", 25) {
                Credentials = new NetworkCredential(Environment.GetEnvironmentVariable("TelegramBot_FromEmail"), Environment.GetEnvironmentVariable("TelegramBot_PassEmail")),
                EnableSsl = true
            };
            smtp.SendMailAsync(mailMessage).Wait();
#endif
        }
    }
}