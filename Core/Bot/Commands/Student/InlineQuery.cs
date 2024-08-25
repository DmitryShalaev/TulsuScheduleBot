using Core.Bot.Commands;

using Microsoft.EntityFrameworkCore;

using ScheduleBot;
using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineQueryResults;

namespace Core.Bot.New.Commands.Student {
    public static class InlineQueryMessage {
        public static async Task InlineQuery(ScheduleDbContext dbContext, Update update) {
            InlineQuery? inlineQuery = update.InlineQuery;

            if(inlineQuery is not null) {
                string str = inlineQuery.Query.ToLower();

                UserCommands instance = UserCommands.Instance;

                switch(str) {
                    case var _ when str == instance.Message["Today"].ToLower():
                        await AnswerInlineQueryAsync(dbContext, inlineQuery, DateOnly.FromDateTime(DateTime.Now));
                        break;

                    case var _ when str == instance.Message["Tomorrow"].ToLower():
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
        }

        private static async Task AnswerInlineQueryAsync(ScheduleDbContext dbContext, InlineQuery inlineQuery, DateOnly date) {
            TelegramUser? user = dbContext.TelegramUsers.Include(u => u.Settings).Include(u => u.ScheduleProfile).FirstOrDefault(u => u.ChatID == inlineQuery.From.Id);

            UserCommands.ConfigStruct config = UserCommands.Instance.Config;

            if(user is not null && !string.IsNullOrWhiteSpace(user.ScheduleProfile.Group)) {
                if((DateTime.Now - dbContext.GroupLastUpdate.Single(i => i.Group == user.ScheduleProfile.Group).Update.ToLocalTime()).TotalMinutes > config.DisciplineUpdateTime)
                    await Parser.Instance.UpdatingDisciplines(dbContext, user.ScheduleProfile.Group, config.UpdateAttemptTime);

                await TelegramBot.Instance.botClient.AnswerInlineQueryAsync(inlineQuery.Id, [
                    new InlineQueryResultArticle(date.ToString(), date.ToString(), new InputTextMessageContent(Scheduler.GetScheduleByDate(dbContext, date, user, link: false).Item1)),
                ], cacheTime: 60, isPersonal: true);
            }
        }
    }
}
