using System.ComponentModel.DataAnnotations.Schema;

using Telegram.Bot.Types.Enums;

namespace Core.DB.Entity {
#pragma warning disable CS8618
    public class MessageLog {
        public long ID { get; set; }

        [ForeignKey("TelegramUser")]
        public long From { get; set; }
        public TelegramUser TelegramUser { get; set; }

        public string Message { get; set; }

        [ForeignKey("UpdateTypeDTO")]
        public UpdateType? UpdateType { get; set; }
        public UpdateTypeDTO UpdateTypeDTO { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;
    }

    public class UpdateTypeDTO {
        public UpdateType ID { get; set; }
        public string Name { get; set; }
    }
}

