using System.Text.RegularExpressions;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.Bot {
    public partial class TelegramBot {
        [GeneratedRegex("^[А-Я][а-я]*[ ]?[а-я]*$")]
        private static partial Regex DefaultMessageRegex();
        [GeneratedRegex("^([0-9]+)[ ]([а-я]+)$")]
        private static partial Regex TermsMessageRegex();
        [GeneratedRegex("(^[А-Я][а-я]*[ ]?[а-я]*):")]
        private static partial Regex GroupOrStudentIDMessageRegex();
        [GeneratedRegex("(^/[A-z]+)[ ]?([A-z0-9-]*)$")]
        private static partial Regex CommandMessageRegex();

        [GeneratedRegex("^([A-z][A-z]+)[ ]([0-9.:]+)$")]
        private static partial Regex DisciplineCallbackRegex();

        [GeneratedRegex("^\\d{1,2}[ ,./-](\\d{1,2}|\\w{3,8})([ ,./-](\\d{2}|\\d{4}))?$")]
        private static partial Regex DateRegex();

        private static readonly BotCommands commands = new BotCommands();

        #region ReplyKeyboardMarkup
        public static readonly ReplyKeyboardMarkup MainKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { commands.Message["Today"], commands.Message["Tomorrow"] },
                            new KeyboardButton[] { commands.Message["ByDays"], commands.Message["ForAWeek"]},
                            new KeyboardButton[] { commands.Message["Exam"]},
                            new KeyboardButton[] { commands.Message["Other"]}
                        }) { ResizeKeyboard = true };

        private readonly ReplyKeyboardMarkup AdditionalKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { commands.Message["Profile"]},
                            new KeyboardButton[] { commands.Message["AcademicPerformance"]},
                            new KeyboardButton[] { commands.Message["Corps"]},
                             new KeyboardButton[] { commands.Message["Back"]}
                        }) { ResizeKeyboard = true };

        private readonly ReplyKeyboardMarkup ExamKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { commands.Message["NextExam"], commands.Message["AllExams"]},
                            new KeyboardButton[] { commands.Message["Back"]}
                        }) { ResizeKeyboard = true };

        private readonly ReplyKeyboardMarkup DaysKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { commands.Message["Monday"], commands.Message["Tuesday"]},
                            new KeyboardButton[] { commands.Message["Wednesday"], commands.Message["Thursday"]},
                            new KeyboardButton[] { commands.Message["Friday"], commands.Message["Saturday"]},
                            new KeyboardButton[] { commands.Message["Back"]}
                        }) { ResizeKeyboard = true };

        private readonly ReplyKeyboardMarkup CancelKeyboardMarkup = new(commands.Message["Cancel"]) { ResizeKeyboard = true };

        private readonly ReplyKeyboardMarkup ResetProfileLinkKeyboardMarkup = new(new KeyboardButton[] { commands.Message["Reset"], commands.Message["Cancel"]}) { ResizeKeyboard = true };

        private readonly ReplyKeyboardMarkup WeekKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { commands.Message["ThisWeek"], commands.Message["NextWeek"]},
                            new KeyboardButton[] { commands.Message["Back"] }
                        }) { ResizeKeyboard = true };

        #endregion

        public static async Task GroupErrorAdmin(ITelegramBotClient botClient, ChatId chatId) => await botClient.SendTextMessageAsync(chatId: chatId, text: $"Для того, чтобы узнать расписание, необходимо указать номер группы в настройках профиля ({commands.Message["Other"]} -> {commands.Message["Profile"]}).", replyMarkup: MainKeyboardMarkup);
        public static async Task GroupErrorUser(ITelegramBotClient botClient, ChatId chatId) => await botClient.SendTextMessageAsync(chatId: chatId, text: $"Попросите владельца профиля указать номер группы в настройках профиля ({commands.Message["Other"]} -> {commands.Message["Profile"]}).", replyMarkup: MainKeyboardMarkup);
        public static async Task StudentIdErrorAdmin(ITelegramBotClient botClient, ChatId chatId) => await botClient.SendTextMessageAsync(chatId: chatId, text: $"Для того, чтобы узнать успеваемость, необходимо указать номер зачетной книжки в настройках профиля ({commands.Message["Other"]} -> {commands.Message["Profile"]}).", replyMarkup: MainKeyboardMarkup);
        public static async Task StudentIdErrorUser(ITelegramBotClient botClient, ChatId chatId) => await botClient.SendTextMessageAsync(chatId: chatId, text: $"Попросите владельца профиля указать номер зачетной книжки в настройках профиля ({commands.Message["Other"]} -> {commands.Message["Profile"]}).", replyMarkup: MainKeyboardMarkup);
        private async Task ScheduleRelevance(ITelegramBotClient botClient, ChatId chatId, IReplyMarkup? replyMarkup) => await botClient.SendTextMessageAsync(chatId: chatId, text: $"Расписание актуально на {Parser.scheduleLastUpdate.ToString("dd.MM HH:mm")}", replyMarkup: replyMarkup);
        private async Task ProgressRelevance(ITelegramBotClient botClient, ChatId chatId, IReplyMarkup? replyMarkup) => await botClient.SendTextMessageAsync(chatId: chatId, text: $"Успеваемость актуально на {Parser.scheduleLastUpdate.ToString("dd.MM HH:mm")}", replyMarkup: replyMarkup);
    }
}
