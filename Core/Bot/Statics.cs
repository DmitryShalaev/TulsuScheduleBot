using System.Text.RegularExpressions;

using ScheduleBot;
using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Core.Bot {
    public static partial class Statics {
        [GeneratedRegex("^[А-я]+[ ]?[а-я]*$")]
        public static partial Regex DefaultMessageRegex();
        [GeneratedRegex("^([0-9]+)[ ]([а-я]+)$")]
        public static partial Regex TermsMessageRegex();
        [GeneratedRegex("(^[А-я]+[а-я ]*):")]
        public static partial Regex GroupOrStudentIDMessageRegex();
        [GeneratedRegex("(^/[A-z]+)[ ]?([A-z0-9-]*)$")]
        public static partial Regex CommandMessageRegex();

        [GeneratedRegex("^([A-z]+)[ ]([0-9.:]+[|0-9.:]*)$")]
        public static partial Regex DisciplineCallbackRegex();

        [GeneratedRegex("^([A-z]+)[ ]([A-z]+)$")]
        public static partial Regex NotificationsCallbackRegex();

        [GeneratedRegex("^([A-z]+)[|]([А-яЁё. ]+)$")]
        public static partial Regex TeachersCallbackRegex();

        [GeneratedRegex("^(\\d{1,2})([ ,.-](\\d{1,2}|\\w{3,8}))?([ ,.-](\\d{2}|\\d{4}))?$")]
        public static partial Regex DateRegex();

        private static readonly Commands.UserCommands commands = Commands.UserCommands.Instance;

        private static readonly Parser parser = Parser.Instance;

        private static readonly TelegramBotClient botClient = TelegramBot.Instance.botClient;

        #region ReplyKeyboardMarkup
        public static readonly ReplyKeyboardMarkup MainKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { commands.Message["Today"], commands.Message["Tomorrow"] },
                            [commands.Message["ByDays"], commands.Message["ForAWeek"]],
                            [commands.Message["Other"]]
                        }) { ResizeKeyboard = true };

        public static readonly ReplyKeyboardMarkup ScheduleKeyboardMarkup = new(new[] {
                             new KeyboardButton[] { commands.Message["Exam"]},
                             [commands.Message["TeachersWorkSchedule"], commands.Message["ClassroomSchedule"]],
                             [commands.Message["Back"]]
                        }) { ResizeKeyboard = true };

        public static readonly ReplyKeyboardMarkup OtherKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { commands.Message["Schedule"] },
                            [commands.Message["Corps"], commands.Message["GroupList"]],
                            [commands.Message["Personal"]],
                            [commands.Message["Back"]]
                        }) { ResizeKeyboard = true };

        public static readonly ReplyKeyboardMarkup PersonalKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { commands.Message["AcademicPerformance"], commands.Message["Profile"] },
                            [commands.Message["Back"]]
                        }) { ResizeKeyboard = true };

        public static readonly ReplyKeyboardMarkup ExamKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { commands.Message["NextExam"], commands.Message["AllExams"] },
                            [commands.Message["Back"]]
                        }) { ResizeKeyboard = true };

        public static readonly ReplyKeyboardMarkup DaysKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { commands.Message["Monday"], commands.Message["Tuesday"] },
                            [commands.Message["Wednesday"], commands.Message["Thursday"]],
                            [commands.Message["Friday"], commands.Message["Saturday"]],
                            [commands.Message["Back"]]
                        }) { ResizeKeyboard = true };

        public static readonly ReplyKeyboardMarkup CancelKeyboardMarkup = new(commands.Message["Cancel"]) { ResizeKeyboard = true };

        public static readonly ReplyKeyboardMarkup ResetProfileLinkKeyboardMarkup = new(new KeyboardButton[] { commands.Message["Reset"], commands.Message["Cancel"] }) { ResizeKeyboard = true };

        public static readonly ReplyKeyboardMarkup WeekKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { commands.Message["ThisWeek"], commands.Message["NextWeek"] },
                            [commands.Message["Back"]]
                        }) { ResizeKeyboard = true };

        public static readonly ReplyKeyboardMarkup TeachersWorkScheduleBackKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { commands.Message["TeachersWorkScheduleBack"] }
                        }) { ResizeKeyboard = true };

        #region Corps
        public static readonly ReplyKeyboardMarkup CorpsKeyboardMarkup = GetCorpsKeyboardMarkup();

        private static ReplyKeyboardMarkup GetCorpsKeyboardMarkup() {
            List<KeyboardButton[]> ProfileKeyboardMarkup = [
                [commands.Corps[0].text]
            ];

            for(int i = 0; i < 3; i++) {
                List<KeyboardButton> keyboardButtonsLine = [];
                for(int j = 0; j < 5; j++)
                    keyboardButtonsLine.Add(commands.Corps[1 + i * 5 + j].text);

                ProfileKeyboardMarkup.Add([.. keyboardButtonsLine]);
            }

            for(int i = 16; i < commands.Corps.Length; i++)
                ProfileKeyboardMarkup.Add([commands.Corps[i].text]);

            ProfileKeyboardMarkup.AddRange([[commands.College.text], [commands.Message["Back"]]]);

            return new(ProfileKeyboardMarkup) { ResizeKeyboard = true };
        }
        #endregion
        #endregion

        public static async Task GroupErrorAdmin(ScheduleDbContext dbContext, ChatId chatId, TelegramUser user) {
            user.TelegramUserTmp.Mode = Mode.GroupСhange;
            await dbContext.SaveChangesAsync();

            await botClient.SendTextMessageAsync(chatId: chatId, text: $"Для того чтобы узнать расписание, необходимо указать номер группы.", replyMarkup: CancelKeyboardMarkup);
        }
        public static async Task GroupErrorUser(ChatId chatId) => await botClient.SendTextMessageAsync(chatId: chatId, text: $"Попросите владельца профиля указать номер группы в настройках профиля ({commands.Message["Other"]} -> {commands.Message["Profile"]}).", replyMarkup: MainKeyboardMarkup);

        public static async Task StudentIdErrorAdmin(ScheduleDbContext dbContext, ChatId chatId, TelegramUser user) {
            user.TelegramUserTmp.Mode = Mode.StudentIDСhange;
            await dbContext.SaveChangesAsync();

            await botClient.SendTextMessageAsync(chatId: chatId, text: $"Для того чтобы узнать успеваемость, необходимо указать номер зачетной книжки.", replyMarkup: CancelKeyboardMarkup);
        }
        public static async Task StudentIdErrorUser(ChatId chatId) => await botClient.SendTextMessageAsync(chatId: chatId, text: $"Попросите владельца профиля указать номер зачетной книжки в настройках профиля ({commands.Message["Other"]} -> {commands.Message["Profile"]}).", replyMarkup: MainKeyboardMarkup);

        public static async Task ScheduleRelevance(ScheduleDbContext dbContext, ITelegramBotClient botClient, ChatId chatId, string group, IReplyMarkup? replyMarkup) {
            DateTime? groupLastUpdate = dbContext.GroupLastUpdate.FirstOrDefault(i => i.Group == group)?.Update.ToLocalTime();

            if(groupLastUpdate is null || (DateTime.Now - groupLastUpdate)?.TotalMinutes > commands.Config.DisciplineUpdateTime) {
                int messageId = (await botClient.SendTextMessageAsync(chatId: chatId, text: commands.Message["WeNeedToWait"])).MessageId;
                if(!await parser.UpdatingDisciplines(dbContext, group, commands.Config.UpdateAttemptTime))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: commands.Message["SiteIsNotResponding"]);

                await botClient.DeleteMessageAsync(chatId: chatId, messageId: messageId);
                groupLastUpdate = dbContext.GroupLastUpdate.FirstOrDefault(i => i.Group == group)?.Update.ToLocalTime();
            }

            if(groupLastUpdate is not null)
                await botClient.SendTextMessageAsync(chatId: chatId, text: $"{commands.Message["ScheduleIsRelevantOn"]} {groupLastUpdate:dd.MM HH:mm}", replyMarkup: replyMarkup);
        }

        public static async Task TeacherWorkScheduleRelevance(ScheduleDbContext dbContext, ITelegramBotClient botClient, ChatId chatId, string teacher, IReplyMarkup? replyMarkup) {
            DateTime? teacherLastUpdate = dbContext.TeacherLastUpdate.FirstOrDefault(i => i.Teacher == teacher)?.Update.ToLocalTime();

            if(teacherLastUpdate is null || (DateTime.Now - teacherLastUpdate)?.TotalMinutes > commands.Config.TeacherWorkScheduleUpdateTime) {
                int messageId = (await botClient.SendTextMessageAsync(chatId: chatId, text: commands.Message["WeNeedToWait"])).MessageId;
                if(!await parser.UpdatingTeacherWorkSchedule(dbContext, teacher, commands.Config.UpdateAttemptTime))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: commands.Message["SiteIsNotResponding"]);

                await botClient.DeleteMessageAsync(chatId: chatId, messageId: messageId);
                teacherLastUpdate = dbContext.TeacherLastUpdate.FirstOrDefault(i => i.Teacher == teacher)?.Update.ToLocalTime();
            }

            if(teacherLastUpdate is not null)
                await botClient.SendTextMessageAsync(chatId: chatId, text: $"{commands.Message["ScheduleIsRelevantOn"]} {teacherLastUpdate:dd.MM HH:mm}", replyMarkup: replyMarkup);
        }

        public static async Task ProgressRelevance(ScheduleDbContext dbContext, ITelegramBotClient botClient, ChatId chatId, string studentID, IReplyMarkup? replyMarkup, bool send = true) {
            DateTime? studentIDlastUpdate = dbContext.StudentIDLastUpdate.FirstOrDefault(i => i.StudentID == studentID)?.Update.ToLocalTime();
            if(studentIDlastUpdate is null || (DateTime.Now - studentIDlastUpdate)?.TotalMinutes > commands.Config.StudentIDUpdateTime) {
                int messageId = (await botClient.SendTextMessageAsync(chatId: chatId, text: commands.Message["WeNeedToWait"])).MessageId;
                if(!await parser.UpdatingProgress(dbContext, studentID, commands.Config.UpdateAttemptTime))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: commands.Message["SiteIsNotResponding"]);

                await botClient.DeleteMessageAsync(chatId: chatId, messageId: messageId);
                studentIDlastUpdate = dbContext.StudentIDLastUpdate.FirstOrDefault(i => i.StudentID == studentID)?.Update.ToLocalTime();
            }

            if(send && studentIDlastUpdate is not null)
                await botClient.SendTextMessageAsync(chatId: chatId, text: $"{commands.Message["AcademicPerformanceIsRelevantOn"]} {studentIDlastUpdate:dd.MM HH:mm}", replyMarkup: replyMarkup);
        }

        public static async Task DeleteTempMessage(TelegramUser user, int? messageId = null) {
            try {
                if(user.TelegramUserTmp.RequestingMessageID is not null) {
                    await botClient.DeleteMessageAsync(chatId: user.ChatID, messageId: (int)user.TelegramUserTmp.RequestingMessageID);
                    user.TelegramUserTmp.RequestingMessageID = null;
                }

                if(messageId is not null)
                    await botClient.DeleteMessageAsync(chatId: user.ChatID, messageId: (int)messageId);

            } catch(Exception) { }
        }

        public static string GetFeedbackMessage(Feedback feedback) {
            return $"От: {feedback.TelegramUser.FirstName}{(string.IsNullOrWhiteSpace(feedback.TelegramUser.LastName) ? "" : $", {feedback.TelegramUser.LastName}")}{(string.IsNullOrWhiteSpace(feedback.TelegramUser.Username) ? "" : $", {feedback.TelegramUser.Username}")}\n" +
                   $"Дата: {feedback.Date.ToLocalTime():dd.MM.yy HH:mm:ss}\n\n" +
                   $"{feedback.Message}";
        }
    }
}
