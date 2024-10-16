using Core.DB;
using Core.DB.Entity;
using Core.Parser;

using Microsoft.EntityFrameworkCore;

using ScheduleBot;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;

namespace Core.Bot.Commands.Student {
    public static class InlineQueryMessage {
        private static ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        private static readonly UserCommands instance = UserCommands.Instance;
        private static readonly UserCommands.ConfigStruct config = UserCommands.Instance.Config;

        public static async Task InlineQuery(ScheduleDbContext dbContext, InlineQuery inlineQuery) {
            string str = inlineQuery.Query.ToLower();

            switch(str) {
                case var _ when str.Equals(instance.Message["Today"], StringComparison.CurrentCultureIgnoreCase):
                    await AnswerInlineQueryAsync(dbContext, inlineQuery, DateOnly.FromDateTime(DateTime.Now));
                    break;

                case var _ when str.Equals(instance.Message["Tomorrow"], StringComparison.CurrentCultureIgnoreCase):
                    await AnswerInlineQueryAsync(dbContext, inlineQuery, DateOnly.FromDateTime(DateTime.Now.AddDays(1)));
                    break;

                default:
                    if(Statics.DateRegex().IsMatch(str)) {
                        try {
                            DateOnly date = DateTime.TryParse(str, out DateTime _date)
                                ? DateOnly.FromDateTime(_date)
                                : DateOnly.FromDateTime(DateTime.Parse($"{str} {DateTime.Now.Month}"));
                            await AnswerInlineQueryAsync(dbContext, inlineQuery, date);
                        } catch(Exception) { }
                    }

                    break;
            }
        }

        private static async Task AnswerInlineQueryAsync(ScheduleDbContext dbContext, InlineQuery inlineQuery, DateOnly date) {
            TelegramUser? user = await dbContext.TelegramUsers.Include(u => u.Settings).Include(u => u.ScheduleProfile).FirstOrDefaultAsync(u => u.ChatID == inlineQuery.From.Id);

            if(user is not null && !string.IsNullOrWhiteSpace(user.ScheduleProfile.Group)) {
                if((DateTime.Now - dbContext.GroupLastUpdate.Single(i => i.Group == user.ScheduleProfile.Group).Update.ToLocalTime()).TotalMinutes > config.DisciplineUpdateTime)
                    await ScheduleParser.Instance.UpdatingDisciplines(dbContext, user.ScheduleProfile.Group, config.UpdateAttemptTime);

                MessagesQueue.Message.AnswerInlineQuery(inlineQuery.Id, [
                    new InlineQueryResultArticle(date.ToString(), date.ToString(), new InputTextMessageContent(Scheduler.GetScheduleByDate(dbContext, date, user, link: false).Item1)),
                ], cacheTime: 60, isPersonal: true);
            }
        }
    }
}
