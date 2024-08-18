using Core.Bot.Interfaces;

using ScheduleBot;
using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace Core.Bot.Commands.AddingDiscipline.Message {
    internal class AddingDisciplineCancel : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => [UserCommands.Instance.Message["Cancel"]];

        public List<Mode> Modes => [Mode.AddingDiscipline];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            IOrderedQueryable<CustomDiscipline> tmp = dbContext.CustomDiscipline.Where(i => !i.IsAdded && i.ScheduleProfile == user.ScheduleProfile).OrderByDescending(i => i.AddDate);
            CustomDiscipline first = tmp.First();

            user.TelegramUserTmp.Mode = Mode.Default;
            dbContext.CustomDiscipline.RemoveRange(tmp);

            await Statics.DeleteTempMessage(user, messageId);
            await AddingDisciplineMode.DeleteInitialMessage(BotClient, chatId, user);

            await dbContext.SaveChangesAsync();

            await Statics.ScheduleRelevance(dbContext, BotClient, chatId, user.ScheduleProfile.Group!, Statics.MainKeyboardMarkup);

            (string, bool) schedule = Scheduler.GetScheduleByDate(dbContext, first.Date, user, true);
            await BotClient.SendTextMessageAsync(chatId: chatId, text: schedule.Item1, replyMarkup: DefaultCallback.GetEditAdminInlineKeyboardButton(dbContext, first.Date, user.ScheduleProfile), parseMode: ParseMode.Markdown, disableWebPagePreview: true);
        }
    }
}
