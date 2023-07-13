using Microsoft.EntityFrameworkCore;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;

namespace ScheduleBot.Bot {
    public partial class TelegramBot {
        private async Task InlineQuery(ScheduleDbContext dbContext, ITelegramBotClient botClient, Update update) {
            InlineQuery? inlineQuery = update.InlineQuery;

            if(inlineQuery is not null) {
                string str = inlineQuery.Query.ToLower();
                switch(str) {
                    case var _ when str == commands.Message["Today"].ToLower():
                        await AnswerInlineQueryAsync(dbContext, botClient, inlineQuery, DateOnly.FromDateTime(DateTime.Now));
                        break;

                    case var _ when str == commands.Message["Tomorrow"].ToLower():
                        await AnswerInlineQueryAsync(dbContext, botClient, inlineQuery, DateOnly.FromDateTime(DateTime.Now.AddDays(1)));
                        break;

                    default:
                        if(DateRegex().IsMatch(str)) {
                            try {
                                DateOnly date;
                                if(DateTime.TryParse(str, out DateTime _date))
                                    date = DateOnly.FromDateTime(_date);
                                else
                                    date = DateOnly.FromDateTime(DateTime.Parse($"{str} {DateTime.Now.Month}"));

                                await AnswerInlineQueryAsync(dbContext, botClient, inlineQuery, date);
                            } catch(Exception) { }
                        }

                        break;
                }
            }
        }

        private async Task AnswerInlineQueryAsync(ScheduleDbContext dbContext, ITelegramBotClient botClient, InlineQuery inlineQuery, DateOnly date) {
            TelegramUser? user = dbContext.TelegramUsers.Include(u => u.ScheduleProfile).FirstOrDefault(u => u.ChatID == inlineQuery.From.Id);

            if(user is not null && !string.IsNullOrWhiteSpace(user.ScheduleProfile.Group)) {
                if((DateTime.Now - dbContext.GroupLastUpdate.Single(i => i.Group == user.ScheduleProfile.Group).Update.ToLocalTime()).TotalMinutes > commands.Config.GroupUpdateTime)
                    parser.UpdatingDisciplines(dbContext, user.ScheduleProfile.Group);

                await botClient.AnswerInlineQueryAsync(inlineQuery.Id, new[] {
                    new InlineQueryResultArticle(date.ToString(), date.ToString(), new InputTextMessageContent(Scheduler.GetScheduleByDate(dbContext, date, user.ScheduleProfile))),
                }, cacheTime: 60, isPersonal: true);
            }
        }
    }
}
