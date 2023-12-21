using System.Globalization;

using Core.Bot;

using Telegram.Bot;
using Telegram.Bot.Polling;

namespace LongPolling {
    internal class Program {
        static void Main() {
            CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("ru-RU");

            TelegramBot bot = TelegramBot.Instance;

            Core.Jobs.Job.Init();

            bot.botClient.ReceiveAsync(
                (botClient, update, cancellationToken) => bot.UpdateAsync(update),
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
        }
    }
}