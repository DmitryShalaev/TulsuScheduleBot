using System.Collections.Concurrent;

using Core.Bot.MessagesQueue.Classes;
using Core.Bot.MessagesQueue.Interfaces;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Core.Bot.MessagesQueue {

    public static class Message {
        private static ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        // Очереди сообщений для каждого пользователя
        private static readonly ConcurrentDictionary<ChatId, (SemaphoreSlim semaphore, ConcurrentQueue<IMessageQueue> queue)> userQueues = new();

        // Переменные для глобального ограничения количества сообщений
        private static readonly SemaphoreSlim globalLock = new(1, 1);

        private const int globalRateLimit = 30; // Максимальное количество сообщений в секунду
        private static int globalMessageCount = 0;

        private static DateTime lastGlobalResetTime = DateTime.UtcNow;
        private static readonly TimeSpan globalRateLimitInterval = TimeSpan.FromSeconds(1); // Интервал времени для лимита

        // Дополнительный словарь для контроля всплесков
        private static readonly ConcurrentDictionary<ChatId, (int burstCount, DateTime lastMessageTime)> userBurstControl = new();

        // Настройки всплесков
        private static readonly int burstLimit = 5; // Максимальное количество сообщений в всплеске
        private static readonly TimeSpan burstInterval = TimeSpan.FromSeconds(1); // Интервал времени для всплеска

        #region Messag
        public static void SendTextMessage(ChatId chatId, string text, IReplyMarkup? replyMarkup = null, ParseMode? parseMode = null, bool? disableWebPagePreview = null, bool? disableNotification = null) {
            var message = new TextMessage(chatId, text, replyMarkup, parseMode, disableWebPagePreview, disableNotification);

            AddMessageToQueue(chatId, message);
        }

        public static void EditMessageText(ChatId chatId, int messageId, string text, InlineKeyboardMarkup? replyMarkup = null, ParseMode? parseMode = null, bool? disableWebPagePreview = null) {
            var message = new EditMessageText(chatId, messageId, text, replyMarkup, parseMode, disableWebPagePreview);

            AddMessageToQueue(chatId, message);
        }

        public static void EditMessageReplyMarkup(ChatId chatId, int messageId, InlineKeyboardMarkup? replyMarkup = null) {
            var message = new EditMessageReplyMarkup(chatId, messageId, replyMarkup);

            AddMessageToQueue(chatId, message);
        }

        public static void DeleteMessage(ChatId chatId, int messageId) {
            var message = new DeleteMessage(chatId, messageId);

            AddMessageToQueue(chatId, message);
        }
        #endregion

        private static void AddMessageToQueue(ChatId chatId, IMessageQueue message) {
            (SemaphoreSlim semaphore, ConcurrentQueue<IMessageQueue> queue) user = userQueues.GetOrAdd(chatId, _ => (new SemaphoreSlim(1, 1), new ConcurrentQueue<IMessageQueue>()));

            user.queue.Enqueue(message);

            // Запускаем обработку очереди 
            if(user.semaphore.CurrentCount > 0) {
                _ = Task.Run(() => ProcessQueue(user));
            }
        }

        private static async Task ProcessQueue((SemaphoreSlim semaphore, ConcurrentQueue<IMessageQueue> queue) user) {
            await user.semaphore.WaitAsync();
            try {

                while(!user.queue.IsEmpty) {
                    if(user.queue.TryDequeue(out IMessageQueue? message)) {

                        // Контроль всплесков для пользователя
                        await ControlUserBurst(message.ChatId);

                        // Учитываем глобальный лимит
                        await EnsureGlobalRateLimit();

                        // Отправляем сообщение
                        await SendMessageAsync(message);
                    }
                }
            } finally {
                user.semaphore.Release(); // Освобождаем чат для следующего сообщения
            }
        }

        private static async Task SendMessageAsync(IMessageQueue message) {
            string msg = " ";

            try {
                switch(message) {
                    case TextMessage textMessage:
                        msg = textMessage.Text;

                        await BotClient.SendTextMessageAsync(
                            chatId: textMessage.ChatId,
                            text: textMessage.Text,
                            parseMode: textMessage.ParseMode,
                            disableWebPagePreview: textMessage.DisableWebPagePreview,
                            disableNotification: textMessage.DisableNotification,
                            replyMarkup: textMessage.ReplyMarkup
                        );
                        break;

                    case EditMessageText editMessageText:
                        msg = editMessageText.Text;

                        await BotClient.EditMessageTextAsync(
                            chatId: editMessageText.ChatId,
                            text: editMessageText.Text,
                            messageId: editMessageText.MessageId,
                            parseMode: editMessageText.ParseMode,
                            replyMarkup: editMessageText.ReplyMarkup,
                            disableWebPagePreview: editMessageText.DisableWebPagePreview
                        );
                        break;

                    case DeleteMessage deleteMessage:
                        msg = $"DeleteMessageAsync {deleteMessage.MessageId}";

                        await BotClient.DeleteMessageAsync(
                            chatId: deleteMessage.ChatId,
                            messageId: deleteMessage.MessageId
                        );
                        break;

                    case EditMessageReplyMarkup editMessageReplyMarkup:
                        msg = $"EditMessageReplyMarkupAsync {editMessageReplyMarkup.MessageId}";

                        await BotClient.EditMessageReplyMarkupAsync(
                            chatId: editMessageReplyMarkup.ChatId,
                            replyMarkup: editMessageReplyMarkup.ReplyMarkup,
                            messageId: editMessageReplyMarkup.MessageId
                        );
                        break;
                }
            } catch(Exception e) {
                await ErrorReport.Send(msg, e);
            }
        }

        private static async Task ControlUserBurst(ChatId chatId) {
            DateTime currentTime = DateTime.UtcNow;

            (int burstCount, DateTime lastMessageTime) = userBurstControl.GetOrAdd(chatId, _ => (0, DateTime.MinValue));

            if(currentTime - lastMessageTime > burstInterval) {
                // Сбрасываем счетчик всплесков, если интервал превышен
                burstCount = 0;
            }

            if(burstCount >= burstLimit) {
                // Если достигнут лимит всплесков, ждем до следующего разрешенного интервала
                TimeSpan delay = burstInterval - (currentTime - lastMessageTime);
                if(delay > TimeSpan.Zero) {
                    await Task.Delay(delay);
                }

                // Обновляем время последнего сообщения после задержки
                lastMessageTime = DateTime.UtcNow;
                burstCount = 0; // Сбрасываем счетчик всплесков после задержки
            }

            // Обновляем данные всплесков для пользователя
            userBurstControl[chatId] = (burstCount + 1, currentTime);
        }

        private static async Task EnsureGlobalRateLimit() {
            await globalLock.WaitAsync(); // Блокируем глобальный счётчик

            try {
                DateTime currentTime = DateTime.UtcNow;

                // Если прошло больше секунды с последнего сброса, сбрасываем счётчик
                if((currentTime - lastGlobalResetTime).TotalSeconds >= 1) {
                    globalMessageCount = 0;
                    lastGlobalResetTime = currentTime;
                }

                // Если достигли лимита в 30 сообщений в секунду, делаем задержку
                if(globalMessageCount >= globalRateLimit) {
                    TimeSpan delay = globalRateLimitInterval - (currentTime - lastGlobalResetTime);
                    if(delay > TimeSpan.Zero) {
                        await Task.Delay(delay);
                    }

                    globalMessageCount = 0;
                    lastGlobalResetTime = DateTime.UtcNow;
                }

                globalMessageCount++; // Увеличиваем счётчик сообщений

            } finally {
                globalLock.Release(); // Освобождаем глобальную блокировку
            }
        }
    }
}
