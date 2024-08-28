using System.Text.RegularExpressions;

using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using ScheduleBot;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace Core.Bot.Commands.Student.Message {
    internal class StudentDefault : IMessageCommand {

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
                    await Statics.ScheduleRelevanceAsync(dbContext, chatId, user.ScheduleProfile.Group!, DefaultMessage.GetMainKeyboardMarkup(user));

                    (string, bool) schedule = Scheduler.GetScheduleByDate(dbContext, date, user);
                    MessagesQueue.Message.SendTextMessage(chatId: chatId, text: schedule.Item1, replyMarkup: DefaultCallback.GetInlineKeyboardButton(date, user, schedule.Item2), parseMode: ParseMode.Markdown);
                } catch(Exception) {
                    MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["CommandRecognizedAsADate"], replyMarkup: DefaultMessage.GetMainKeyboardMarkup(user));
                }

                return;
            }

            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["CommandNotRecognized"], replyMarkup: DefaultMessage.GetMainKeyboardMarkup(user));

        }
    }
}
