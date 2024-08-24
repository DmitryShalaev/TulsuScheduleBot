using Core.Bot.Commands.AddingDiscipline;
using Core.Bot.Commands.Interfaces;

using ScheduleBot;
using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace Core.Bot.Commands.Student.Custom.Message {
    internal class CustomEditStartTime : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => null;

        public List<Mode> Modes => [Mode.CustomEditStartTime];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            if(!string.IsNullOrWhiteSpace(user.TelegramUserTmp.TmpData)) {
                CustomDiscipline discipline = dbContext.CustomDiscipline.Single(i => i.ID == uint.Parse(user.TelegramUserTmp.TmpData));
                try {
                    discipline.StartTime = AddingDisciplineMode.ParseTime(args);
                    user.TelegramUserTmp.Mode = Mode.Default;
                    user.TelegramUserTmp.TmpData = null;

                    await Statics.DeleteTempMessage(user, messageId);

                    await dbContext.SaveChangesAsync();

                    await BotClient.SendTextMessageAsync(chatId: chatId, text: "Время начала успешно изменено.", replyMarkup: Statics.MainKeyboardMarkup);
                    await BotClient.SendTextMessageAsync(chatId: chatId, text: Scheduler.GetScheduleByDate(dbContext, discipline.Date, user, all: true).Item1, replyMarkup: DefaultCallback.GetCustomEditAdminInlineKeyboardButton(discipline), parseMode: ParseMode.Markdown, disableWebPagePreview: true);
                } catch(Exception) {
                    await BotClient.SendTextMessageAsync(chatId: chatId, text: "Ошибка в формате времени!", replyMarkup: Statics.CancelKeyboardMarkup);
                }
            }
        }
    }
}
