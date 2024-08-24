using System.Collections.Concurrent;

using Core.Bot.Messages.Interfaces;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Core.Bot.Messages {

    public static class MessageQueue {
        private static ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        // Очереди сообщений для каждого пользователя
        private static readonly ConcurrentDictionary<ChatId, ConcurrentQueue<IMessageQueue>> userQueues = new();

        // Флаги активности обработки очередей
        private static readonly ConcurrentDictionary<ChatId, bool> isProcessing = new();

        // Семафор для ограничения глобального числа сообщений в секунду
        private static readonly SemaphoreSlim globalSemaphore = new(30, 30);

        // Лимиты для всплесков
        private const int BurstLimit = 5;
        private const int DelayAfterBurst = 1000;

        public static void SendTextMessage(ChatId chatId, string text, IReplyMarkup? replyMarkup = null, ParseMode? parseMode = null, bool? disableWebPagePreview = null, bool? disableNotification = null) {
            var message = new TextMessage(chatId, text, replyMarkup, parseMode, disableWebPagePreview, disableNotification);

            userQueues.GetOrAdd(chatId, _ => new ConcurrentQueue<IMessageQueue>()).Enqueue(message);

            if(isProcessing.TryAdd(chatId, true))
                Task.Run(() => ProcessUserQueue(chatId));
        }

        private static async Task ProcessUserQueue(ChatId chatId) {
            try {
                ConcurrentQueue<IMessageQueue> userQueue = userQueues[chatId];
                int burstCounter = 0;

                while(!userQueue.IsEmpty) {
                    if(userQueue.TryDequeue(out IMessageQueue? message)) {
                        // Ограничение на всплеск сообщений
                        if(burstCounter >= BurstLimit) {
                            burstCounter = 0;
                            await Task.Delay(DelayAfterBurst);
                        }

                        // Отправляем сообщение
                        if(message is TextMessage textMessage)
                            await ThrottleMessageSending(textMessage);

                        burstCounter++;
                    }

                    // Маленькая задержка для избежания перегрузки процессора при пустой очереди
                    await Task.Delay(50);
                }
            } finally {
                // Снимаем флаг активности после завершения обработки очереди
                isProcessing.TryRemove(chatId, out _);
            }
        }

        private static async Task ThrottleMessageSending(TextMessage message) {
            // Ограничение глобальной отправки: не более 30 сообщений в секунду
            await globalSemaphore.WaitAsync();
            try {
                await BotClient.SendTextMessageAsync(
                    chatId: message.ChatId,
                    text: message.Text,
                    parseMode: message.ParseMode,
                    disableWebPagePreview: message.DisableWebPagePreview,
                    disableNotification: message.DisableNotification,
                    replyMarkup: message.ReplyMarkup
                );

            } finally {
                globalSemaphore.Release();
            }
        }
    }
}
