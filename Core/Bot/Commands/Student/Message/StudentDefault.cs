using System.Text.RegularExpressions;

using Core.Bot.Commands.Interfaces;
using Core.Bot.Messages;

using ScheduleBot;
using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace Core.Bot.Commands.Student.Message {
    internal class StudentDefault : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => null;

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.group;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            user.TelegramUserTmp.TmpData = null;

            Match match = Statics.DateRegex().Match(args);
            if(match.Success) {
                DateTime now = DateTime.Now;
                string sDate = $"{match.Groups[1].Value} " +
                               $"{(string.IsNullOrWhiteSpace(match.Groups[3].Value) ? now.Month : match.Groups[3].Value)} " +
                               $"{(string.IsNullOrWhiteSpace(match.Groups[5].Value) ? now.Year : match.Groups[5].Value)}";
                try {
                    var date = DateOnly.Parse(sDate);
                    await Statics.ScheduleRelevance(dbContext,  chatId, user.ScheduleProfile.Group!, Statics.MainKeyboardMarkup);

                    (string, bool) schedule = Scheduler.GetScheduleByDate(dbContext, date, user);
                    MessageQueue.SendTextMessage(chatId: chatId, text: schedule.Item1, replyMarkup: DefaultCallback.GetInlineKeyboardButton(date, user, schedule.Item2), parseMode: ParseMode.Markdown, disableWebPagePreview: true);
                } catch(Exception) {
                    MessageQueue.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["CommandRecognizedAsADate"], replyMarkup: Statics.MainKeyboardMarkup);
                }

                return;
            }

            MessageQueue.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["CommandNotRecognized"], replyMarkup: Statics.MainKeyboardMarkup);

        }
    }
}
