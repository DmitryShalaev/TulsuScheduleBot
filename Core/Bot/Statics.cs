using System.Text.RegularExpressions;

using ScheduleBot.DB;
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

        [GeneratedRegex("^([A-z]+)[|]([А-яЁё. ]+)$")]
        private static partial Regex TeachersCallbackRegex();

        [GeneratedRegex("^\\d{1,2}([ ,.-](\\d{1,2}|\\w{3,8}))?([ ,.-](\\d{2}|\\d{4}))?$")]
        private static partial Regex DateRegex();

        public static readonly BotCommands commands = BotCommands.GetInstance();

        #region ReplyKeyboardMarkup
        public static readonly ReplyKeyboardMarkup MainKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { commands.Message["Today"], commands.Message["Tomorrow"] },
                            new KeyboardButton[] { commands.Message["ByDays"], commands.Message["ForAWeek"] },
                            new KeyboardButton[] { commands.Message["Other"] }
                        }) { ResizeKeyboard = true };

        private static readonly ReplyKeyboardMarkup AdditionalKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { commands.Message["Profile"] },
                            new KeyboardButton[] { commands.Message["Exam"], commands.Message["AcademicPerformance"] },
                            new KeyboardButton[] { commands.Message["Corps"], commands.Message["TeachersWorkSchedule"] },
                            new KeyboardButton[] { commands.Message["Back"] }
                        }) { ResizeKeyboard = true };

        private static readonly ReplyKeyboardMarkup ExamKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { commands.Message["NextExam"], commands.Message["AllExams"] },
                            new KeyboardButton[] { commands.Message["Back"] }
                        }) { ResizeKeyboard = true };

        private static readonly ReplyKeyboardMarkup DaysKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { commands.Message["Monday"], commands.Message["Tuesday"] },
                            new KeyboardButton[] { commands.Message["Wednesday"], commands.Message["Thursday"] },
                            new KeyboardButton[] { commands.Message["Friday"], commands.Message["Saturday"] },
                            new KeyboardButton[] { commands.Message["Back"] }
                        }) { ResizeKeyboard = true };

        private static readonly ReplyKeyboardMarkup CancelKeyboardMarkup = new(commands.Message["Cancel"]) { ResizeKeyboard = true };

        private static readonly ReplyKeyboardMarkup ResetProfileLinkKeyboardMarkup = new(new KeyboardButton[] { commands.Message["Reset"], commands.Message["Cancel"] }) { ResizeKeyboard = true };

        private static readonly ReplyKeyboardMarkup WeekKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { commands.Message["ThisWeek"], commands.Message["NextWeek"] },
                            new KeyboardButton[] { commands.Message["Back"] }
                        }) { ResizeKeyboard = true };

        private static readonly ReplyKeyboardMarkup TeachersWorkScheduleBackKeyboardMarkup = new(new[] {
                            new KeyboardButton[] { commands.Message["TeachersWorkScheduleBack"] }
                        }) { ResizeKeyboard = true };

        private readonly ReplyKeyboardMarkup CorpsKeyboardMarkup = GetCorpsKeyboardMarkup();
        #endregion

        public async Task GroupErrorAdmin(ScheduleDbContext dbContext, ChatId chatId, TelegramUser user) {
            user.Mode = Mode.GroupСhange;
            await dbContext.SaveChangesAsync();

            await botClient.SendTextMessageAsync(chatId: chatId, text: $"Для того, чтобы узнать расписание, необходимо указать номер группы.", replyMarkup: CancelKeyboardMarkup);
        }
        public async Task GroupErrorUser(ChatId chatId) => await botClient.SendTextMessageAsync(chatId: chatId, text: $"Попросите владельца профиля указать номер группы в настройках профиля ({commands.Message["Other"]} -> {commands.Message["Profile"]}).", replyMarkup: MainKeyboardMarkup);

        public async Task StudentIdErrorAdmin(ScheduleDbContext dbContext, ChatId chatId, TelegramUser user) {
            user.Mode = Mode.StudentIDСhange;
            await dbContext.SaveChangesAsync();

            await botClient.SendTextMessageAsync(chatId: chatId, text: $"Для того, чтобы узнать успеваемость, необходимо указать номер зачетной книжки.", replyMarkup: CancelKeyboardMarkup);
        }
        public async Task StudentIdErrorUser(ChatId chatId) => await botClient.SendTextMessageAsync(chatId: chatId, text: $"Попросите владельца профиля указать номер зачетной книжки в настройках профиля ({commands.Message["Other"]} -> {commands.Message["Profile"]}).", replyMarkup: MainKeyboardMarkup);

        private async Task ScheduleRelevance(ScheduleDbContext dbContext, ITelegramBotClient botClient, ChatId chatId, string group, IReplyMarkup? replyMarkup) {
            DateTime? groupLastUpdate = dbContext.GroupLastUpdate.FirstOrDefault(i => i.Group == group)?.Update.ToLocalTime();

            if(groupLastUpdate is null || (DateTime.Now - groupLastUpdate)?.TotalMinutes > commands.Config.DisciplineUpdateTime) {
                int messageId = (await botClient.SendTextMessageAsync(chatId: chatId, text: "Нужно подождать...")).MessageId;
                if(!await parser.UpdatingDisciplines(dbContext, group, commands.Config.UpdateAttemptTime))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Сайт ТулГУ не отвечает!");

                await botClient.DeleteMessageAsync(chatId: chatId, messageId: messageId);
                groupLastUpdate = dbContext.GroupLastUpdate.FirstOrDefault(i => i.Group == group)?.Update.ToLocalTime();
            }

            if(groupLastUpdate is not null)
                await botClient.SendTextMessageAsync(chatId: chatId, text: $"Расписание актуально на {groupLastUpdate:dd.MM HH:mm}", replyMarkup: replyMarkup);
        }

        private async Task TeacherWorkScheduleRelevance(ScheduleDbContext dbContext, ITelegramBotClient botClient, ChatId chatId, string teacher, IReplyMarkup? replyMarkup) {
            DateTime? teacherLastUpdate = dbContext.TeacherLastUpdate.FirstOrDefault(i => i.Teacher == teacher)?.Update.ToLocalTime();
            if(teacherLastUpdate is null || (DateTime.Now - teacherLastUpdate)?.TotalMinutes > commands.Config.TeacherWorkScheduleUpdateTime) {
                int messageId = (await botClient.SendTextMessageAsync(chatId: chatId, text: "Нужно подождать...")).MessageId;
                if(!await parser.UpdatingTeacherWorkSchedule(dbContext, teacher, commands.Config.UpdateAttemptTime))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Сайт ТулГУ не отвечает!");

                await botClient.DeleteMessageAsync(chatId: chatId, messageId: messageId);
                teacherLastUpdate = dbContext.TeacherLastUpdate.FirstOrDefault(i => i.Teacher == teacher)?.Update.ToLocalTime();
            }

            if(teacherLastUpdate is not null)
                await botClient.SendTextMessageAsync(chatId: chatId, text: $"Расписание актуально на {teacherLastUpdate:dd.MM HH:mm}", replyMarkup: replyMarkup);
        }

        private async Task ProgressRelevance(ScheduleDbContext dbContext, ITelegramBotClient botClient, ChatId chatId, string studentID, IReplyMarkup? replyMarkup, bool send = true) {
            DateTime? studentIDlastUpdate = dbContext.StudentIDLastUpdate.FirstOrDefault(i => i.StudentID == studentID)?.Update.ToLocalTime();
            if(studentIDlastUpdate is null || (DateTime.Now - studentIDlastUpdate)?.TotalMinutes > commands.Config.StudentIDUpdateTime) {
                int messageId = (await botClient.SendTextMessageAsync(chatId: chatId, text: "Нужно подождать...")).MessageId;
                if(!await parser.UpdatingProgress(dbContext, studentID, commands.Config.UpdateAttemptTime))
                    await botClient.SendTextMessageAsync(chatId: chatId, text: "Сайт ТулГУ не отвечает!");

                await botClient.DeleteMessageAsync(chatId: chatId, messageId: messageId);
                studentIDlastUpdate = dbContext.StudentIDLastUpdate.FirstOrDefault(i => i.StudentID == studentID)?.Update.ToLocalTime();
            }

            if(send && studentIDlastUpdate is not null)
                await botClient.SendTextMessageAsync(chatId: chatId, text: $"Успеваемость актуально на {studentIDlastUpdate:dd.MM HH:mm}", replyMarkup: replyMarkup);
        }
    }
}
