using System.Text.RegularExpressions;

using Core.Bot.Interfaces;

using ScheduleBot;
using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
namespace Core.Bot.Commands.Teachers.Message {

    internal class TeacherSelectedDefault : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => null;

        public List<Mode> Modes => [Mode.TeacherSelected];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            Match match = Statics.DateRegex().Match(args);
            if(match.Success) {
                DateTime now = DateTime.Now;
                string sDate = $"{match.Groups[1].Value} " +
                               $"{(string.IsNullOrWhiteSpace(match.Groups[3].Value) ? now.Month : match.Groups[3].Value)} " +
                               $"{(string.IsNullOrWhiteSpace(match.Groups[5].Value) ? now.Year : match.Groups[5].Value)}";

                ReplyKeyboardMarkup teacherWorkSchedule = DefaultMessage.GetTeacherWorkScheduleSelectedKeyboardMarkup(user.TelegramUserTmp.TmpData!);

                try {
                    var date = DateOnly.Parse(sDate);

                    await Statics.TeacherWorkScheduleRelevance(dbContext, BotClient, chatId, user.TelegramUserTmp.TmpData!, teacherWorkSchedule);
                    await BotClient.SendTextMessageAsync(chatId: chatId, text: Scheduler.GetTeacherWorkScheduleByDate(dbContext, date, user.TelegramUserTmp.TmpData!));
                } catch(Exception) {
                    await BotClient.SendTextMessageAsync(chatId: chatId, text: UserCommands.Instance.Message["CommandRecognizedAsADate"], replyMarkup: teacherWorkSchedule);
                }

                return;
            }

            await BotClient.SendTextMessageAsync(chatId: chatId, text: UserCommands.Instance.Message["CommandNotRecognized"], replyMarkup: DefaultMessage.GetTeacherWorkScheduleSelectedKeyboardMarkup(user.TelegramUserTmp.TmpData!));

        }
    }
}
