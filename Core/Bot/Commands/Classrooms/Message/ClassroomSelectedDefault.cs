using System.Text.RegularExpressions;

using Core.Bot.Commands.Interfaces;

using ScheduleBot;
using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
namespace Core.Bot.Commands.Classrooms.Message {

    internal class ClassroomSelectedDefault : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => null;

        public List<Mode> Modes => [Mode.ClassroomSelected];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            Match match = Statics.DateRegex().Match(args);
            if(match.Success) {
                DateTime now = DateTime.Now;
                string sDate = $"{match.Groups[1].Value} " +
                               $"{(string.IsNullOrWhiteSpace(match.Groups[3].Value) ? now.Month : match.Groups[3].Value)} " +
                               $"{(string.IsNullOrWhiteSpace(match.Groups[5].Value) ? now.Year : match.Groups[5].Value)}";

                ReplyKeyboardMarkup teacherWorkSchedule = DefaultMessage.GetClassroomWorkScheduleSelectedKeyboardMarkup(user.TelegramUserTmp.TmpData!);

                try {
                    var date = DateOnly.Parse(sDate);

                    await Statics.ClassroomWorkScheduleRelevance(dbContext, BotClient, chatId, user.TelegramUserTmp.TmpData!, teacherWorkSchedule);
                    await BotClient.SendTextMessageAsync(chatId: chatId, text: Scheduler.GetClassroomWorkScheduleByDate(dbContext, date, user.TelegramUserTmp.TmpData!, user), parseMode: ParseMode.Markdown, disableWebPagePreview: true);
                } catch(Exception) {
                    await BotClient.SendTextMessageAsync(chatId: chatId, text: UserCommands.Instance.Message["CommandRecognizedAsADate"], replyMarkup: teacherWorkSchedule);
                }

                return;
            }

            await BotClient.SendTextMessageAsync(chatId: chatId, text: UserCommands.Instance.Message["CommandNotRecognized"], replyMarkup: DefaultMessage.GetClassroomWorkScheduleSelectedKeyboardMarkup(user.TelegramUserTmp.TmpData!));

        }
    }
}
