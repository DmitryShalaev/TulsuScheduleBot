using Core.Bot.Commands.Interfaces;
using Core.Bot.MessagesQueue;
using Core.DB;
using Core.DB.Entity;

using ScheduleBot;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace Core.Bot.Commands.AddingDiscipline.Message {
    internal class AddingDisciplineCancel : IMessageCommand {

        public List<string>? Commands => [UserCommands.Instance.Message["Cancel"]];

        public List<Mode> Modes => [Mode.AddingDiscipline];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            IOrderedQueryable<CustomDiscipline> tmp = dbContext.CustomDiscipline.Where(i => !i.IsAdded && i.ScheduleProfile == user.ScheduleProfile).OrderByDescending(i => i.AddDate);
            CustomDiscipline first = tmp.First();

            user.TelegramUserTmp.Mode = Mode.Default;
            dbContext.CustomDiscipline.RemoveRange(tmp);

            AddingDisciplineMode.DeleteInitialMessage(chatId, user);

            await dbContext.SaveChangesAsync();

            await Statics.ScheduleRelevanceAsync(dbContext, chatId, user.ScheduleProfile.Group!, Statics.MainKeyboardMarkup);

            (string, bool) schedule = Scheduler.GetScheduleByDate(dbContext, first.Date, user, true);
            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: schedule.Item1, replyMarkup: DefaultCallback.GetEditAdminInlineKeyboardButton(dbContext, first.Date, user.ScheduleProfile), parseMode: ParseMode.Markdown, disableWebPagePreview: true);
        }
    }
}
