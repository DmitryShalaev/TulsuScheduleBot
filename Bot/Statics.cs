using System.Text.RegularExpressions;

using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.Bot {
    public partial class TelegramBot {
        [GeneratedRegex("^[А-я]+[ ]?[а-я]*$")]
        private static partial Regex DefaultMessageRegex();
        [GeneratedRegex("^([0-9]+)[ ]([а-я]+)$")]
        private static partial Regex TermsMessageRegex();
        [GeneratedRegex("(^[А-я]+[ ]?[а-я]*):")]
        private static partial Regex GroupOrStudentIDMessageRegex();
        [GeneratedRegex("(^/[A-z]+)[ ]?([A-z0-9-]*)$")]
        private static partial Regex CommandMessageRegex();

        [GeneratedRegex("^([A-z]+)[ ]([0-9.:]+[|0-9.:]*)$")]
        private static partial Regex DisciplineCallbackRegex();

        [GeneratedRegex("^([A-z]+)[ ]([A-z]+)$")]
        private static partial Regex NotificationsCallbackRegex();

        [GeneratedRegex("^\\d{1,2}([ ,.-](\\d{1,2}|\\w{3,8}))?([ ,.-](\\d{2}|\\d{4}))?$")]
        private static partial Regex DateRegex();

        public static readonly BotCommands commands = new BotCommands();

        #region ReplyKeyboardMarkup
        public static readonly ReplyKeyboardMarkup MainKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { commands.Message["Today"], commands.Message["Tomorrow"] },
                            new KeyboardButton[] { commands.Message["ByDays"], commands.Message["ForAWeek"]},
                            new KeyboardButton[] { commands.Message["Exam"]},
                            new KeyboardButton[] { commands.Message["Other"]}
                        }) { ResizeKeyboard = true };

        private static readonly ReplyKeyboardMarkup AdditionalKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { commands.Message["Profile"]},
                            new KeyboardButton[] { commands.Message["AcademicPerformance"]},
                            new KeyboardButton[] { commands.Message["Corps"]},
                             new KeyboardButton[] { commands.Message["Back"]}
                        }) { ResizeKeyboard = true };

        private static readonly ReplyKeyboardMarkup ExamKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { commands.Message["NextExam"], commands.Message["AllExams"]},
                            new KeyboardButton[] { commands.Message["Back"]}
                        }) { ResizeKeyboard = true };

        private static readonly ReplyKeyboardMarkup DaysKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { commands.Message["Monday"], commands.Message["Tuesday"]},
                            new KeyboardButton[] { commands.Message["Wednesday"], commands.Message["Thursday"]},
                            new KeyboardButton[] { commands.Message["Friday"], commands.Message["Saturday"]},
                            new KeyboardButton[] { commands.Message["Back"]}
                        }) { ResizeKeyboard = true };

        private static readonly ReplyKeyboardMarkup CancelKeyboardMarkup = new(commands.Message["Cancel"]) { ResizeKeyboard = true };

        private static readonly ReplyKeyboardMarkup ResetProfileLinkKeyboardMarkup = new(new KeyboardButton[] { commands.Message["Reset"], commands.Message["Cancel"]}) { ResizeKeyboard = true };

        private static readonly ReplyKeyboardMarkup WeekKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { commands.Message["ThisWeek"], commands.Message["NextWeek"]},
                            new KeyboardButton[] { commands.Message["Back"] }
                        }) { ResizeKeyboard = true };

        private readonly ReplyKeyboardMarkup CorpsKeyboardMarkup = GetCorpsKeyboardMarkup();
        #endregion

        public async Task GroupErrorAdmin(ChatId chatId, TelegramUser user) {
            user.Mode = Mode.GroupСhange;
            dbContext.SaveChanges();

            await botClient.SendTextMessageAsync(chatId: chatId, text: $"Для того, чтобы узнать расписание, необходимо указать номер группы.", replyMarkup: CancelKeyboardMarkup);
        }
        public async Task GroupErrorUser(ChatId chatId) => await botClient.SendTextMessageAsync(chatId: chatId, text: $"Попросите владельца профиля указать номер группы в настройках профиля ({commands.Message["Other"]} -> {commands.Message["Profile"]}).", replyMarkup: MainKeyboardMarkup);

        public async Task StudentIdErrorAdmin(ChatId chatId, TelegramUser user) {
            user.Mode = Mode.StudentIDСhange;
            dbContext.SaveChanges();

            await botClient.SendTextMessageAsync(chatId: chatId, text: $"Для того, чтобы узнать успеваемость, необходимо указать номер зачетной книжки.", replyMarkup: CancelKeyboardMarkup);
        }
        public async Task StudentIdErrorUser(ChatId chatId) => await botClient.SendTextMessageAsync(chatId: chatId, text: $"Попросите владельца профиля указать номер зачетной книжки в настройках профиля ({commands.Message["Other"]} -> {commands.Message["Profile"]}).", replyMarkup: MainKeyboardMarkup);

        private async Task ScheduleRelevance(ITelegramBotClient botClient, ChatId chatId, string group, IReplyMarkup? replyMarkup) {
            var groupLastUpdate = dbContext.GroupLastUpdate.Single(i => i.Group == group).Update;
            if((DateTime.UtcNow - groupLastUpdate).TotalMinutes > commands.Config.GroupUpdateTime) {
                var messageId = (await botClient.SendTextMessageAsync(chatId: chatId, text: "Нужно подождать...")).MessageId;
                parser.UpdatingDisciplines(group);

                await botClient.DeleteMessageAsync(chatId: chatId, messageId: messageId);
            }

            await botClient.SendTextMessageAsync(chatId: chatId, text: $"Расписание актуально на {groupLastUpdate.ToLocalTime().ToString("dd.MM HH:mm")}", replyMarkup: replyMarkup);
        }

        private async Task ProgressRelevance(ITelegramBotClient botClient, ChatId chatId, string studentID, IReplyMarkup? replyMarkup, bool send = true) {
            var studentIDlastUpdate = dbContext.StudentIDLastUpdate.Single(i => i.StudentID == studentID).Update;
            if((DateTime.UtcNow - studentIDlastUpdate).TotalMinutes > commands.Config.StudentIDUpdateTime) {
                var messageId = (await botClient.SendTextMessageAsync(chatId: chatId, text: "Нужно подождать...")).MessageId;
                parser.UpdatingProgress(studentID);

                await botClient.DeleteMessageAsync(chatId: chatId, messageId: messageId);
            }

            if(send)
                await botClient.SendTextMessageAsync(chatId: chatId, text: $"Успеваемость актуально на {studentIDlastUpdate.ToLocalTime().ToString("dd.MM HH:mm")}", replyMarkup: replyMarkup);
        }
    }
}
