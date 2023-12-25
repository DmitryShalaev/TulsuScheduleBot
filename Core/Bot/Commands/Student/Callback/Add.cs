using Core.Bot.Commands.AddingDiscipline;
using Core.Bot.Interfaces;

using ScheduleBot;
using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Core.Bot.Commands.Student.Callback {
    public class Add : ICallbackCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public string Command => UserCommands.Instance.Callback["Add"].callback;

        public Mode Mode => Mode.Default;

        public Manager.Check Check => Manager.Check.group;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args) {
            if(DateOnly.TryParse(args, out DateOnly date)) {
                if(user.IsOwner()) {
                    user.Mode = Mode.AddingDiscipline;
                    user.TempData = $"{messageId}";
                    dbContext.CustomDiscipline.Add(new(user.ScheduleProfile, date));
                    await dbContext.SaveChangesAsync();

                    await BotClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: Scheduler.GetScheduleByDate(dbContext, date, user).Item1, parseMode: ParseMode.Markdown);
                    user.RequestingMessageID = (await BotClient.SendTextMessageAsync(chatId: chatId, text: AddingDisciplineMode.GetStagesAddingDiscipline(dbContext, user), replyMarkup: Statics.CancelKeyboardMarkup, parseMode: ParseMode.Markdown)).MessageId;

                } else {
                    (string, bool) schedule = Scheduler.GetScheduleByDate(dbContext, date, user);
                    await BotClient.EditMessageTextAsync(chatId: chatId, messageId: messageId, text: schedule.Item1, replyMarkup: DefaultCallback.GetInlineKeyboardButton(date, user, schedule.Item2), parseMode: ParseMode.Markdown);
                }
            }
        }
    }
}
