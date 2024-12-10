using System.Globalization;

using Core.Bot;

using Telegram.Bot;
using Telegram.Bot.Polling;

namespace LongPolling {
    internal class Program {
        static void Main() {
            CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("ru-RU");

            TelegramBot bot = TelegramBot.Instance;

            bot.botClient.ReceiveAsync(
                (botClient, update, cancellationToken) => bot.UpdateAsync(update),
                (botClient, update, cancellationToken) => Task.CompletedTask,
                new ReceiverOptions {
                    AllowedUpdates = { },
#if DEBUG
                    DropPendingUpdates = true
#else
                    DropPendingUpdates = false
#endif
                },
                new CancellationTokenSource().Token
            ).Wait();
        }
    }
}