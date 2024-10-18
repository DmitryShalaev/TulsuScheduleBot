using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;

using static Core.Bot.Commands.Manager;

namespace Core.Bot.Commands.Interfaces {
    /// <summary>
    /// Интерфейс для команд, обрабатывающих текстовые сообщения в Telegram-боте.
    /// </summary>
    public interface IMessageCommand {
        /// <summary>
        /// Список строковых команд, которые может обрабатывать данный обработчик сообщений.
        /// Может быть пустым, если команда не привязана к конкретному тексту.
        /// </summary>
        public List<string>? Commands { get; }

        /// <summary>
        /// Список режимов, в которых эта команда может быть выполнена.
        /// Например, разные режимы взаимодействия с пользователем.
        /// </summary>
        public List<Mode> Modes { get; }

        /// <summary>
        /// Проверка, выполняемая перед запуском команды.
        /// Определяет, нужно ли выполнять какие-либо проверки перед выполнением команды.
        /// </summary>
        public Check Check { get; }

        /// <summary>
        /// Выполняет основную логику команды.
        /// </summary>
        /// <param name="dbContext">Контекст базы данных для сохранения изменений.</param>
        /// <param name="chatId">ID чата, в котором было получено сообщение.</param>
        /// <param name="messageId">ID сообщения, связанного с командой.</param>
        /// <param name="user">Объект пользователя Telegram, для которого выполняется команда.</param>
        /// <param name="args">Аргументы, переданные вместе с командой (обычно текст сообщения).</param>
        /// <returns>Асинхронная задача, представляющая выполнение команды.</returns>
        public Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args);
    }
}
