using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;

using static Core.Bot.Commands.Manager;

namespace Core.Bot.Commands.Interfaces {

    /// <summary>
    /// Интерфейс для команд, обрабатывающих callback запросы в Telegram-боте.
    /// </summary>
    public interface ICallbackCommand {
        /// <summary>
        /// Команда соответствующая callback запросу.
        /// </summary>
        public string Command { get; }

        /// <summary>
        /// Режим выполнения команды.
        /// Определяет контекст выполнения и параметры обработки.
        /// </summary>
        public Mode Mode { get; }

        /// <summary>
        /// Проверка, необходимая для выполнения команды.
        /// Определяет, нужно ли выполнять дополнительные проверки перед обработкой команды.
        /// </summary>
        public Check Check { get; }

        /// <summary>
        /// Выполняет основную логику команды.
        /// </summary>
        /// <param name="dbContext">Контекст базы данных для сохранения изменений.</param>
        /// <param name="chatId">ID чата, в котором была вызвана команда.</param>
        /// <param name="messageId">ID сообщения, связанного с callback.</param>
        /// <param name="user">Объект пользователя Telegram, для которого выполняется команда.</param>
        /// <param name="message">Сообщение, с которым ассоциирован callback запрос.</param>
        /// <param name="args">Дополнительные аргументы, переданные команде.</param>
        /// <returns>Асинхронная задача, представляющая выполнение команды.</returns>
        public Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args);
    }
}
