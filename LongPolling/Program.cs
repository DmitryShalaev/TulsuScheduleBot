using System.Globalization;

using ScheduleBot;
using ScheduleBot.Bot;

using Telegram.Bot;
using Telegram.Bot.Polling;

#if !DEBUG
using System.Net.Mail;
using System.Net;
#endif

namespace LongPolling {
    internal class Program {
        static void Main() {
#if !DEBUG
            if(string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TelegramBot_FromEmail")) ||
               string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TelegramBot_ToEmail")) ||
               string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TelegramBot_PassEmail")))
                throw new NullReferenceException("Environment Variable is null");
#endif

            CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("ru-RU");

            try {
                TelegramBot bot = Core.InitBot();

                bot.botClient.ReceiveAsync(
                    async (botClient, update, cancellationToken) => await bot.UpdateAsync(botClient, update),
                    (botClient, update, cancellationToken) => Task.CompletedTask,
                    new ReceiverOptions {
                        AllowedUpdates = { },
#if DEBUG
                        ThrowPendingUpdates = true
#else
                        ThrowPendingUpdates = false
#endif
                    },
                    new CancellationTokenSource().Token
                ).Wait();

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