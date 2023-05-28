using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace ScheduleBot.Bot {
    static class Constants {
        public const string RK_Today = "Сегодня";
        public const string RK_Tomorrow = "Завтра";

        public const string RK_ByDays = "По дням";
        public const string RK_ForAWeek = "На неделю";

        public const string RK_Profile = "Профиль";

        public const string RK_Exam = "Экзамены";
        public const string RK_NextExam = "Ближайший экзамен";
        public const string RK_AllExams = "Все экзамены";

        public const string RK_AcademicPerformance = "Успеваемость";

        public const string RK_Monday = "Понедельник";
        public const string RK_Tuesday = "Вторник";
        public const string RK_Wednesday = "Среда";
        public const string RK_Thursday = "Четверг";
        public const string RK_Friday = "Пятница";
        public const string RK_Saturday = "Суббота";

        public const string RK_ThisWeek = "Эта неделя";
        public const string RK_NextWeek = "Следующая неделя";

        public const string RK_Back = "Назад";
        public const string RK_Cancel = "Отмена";
        public const string RK_Reset = "Восстановить";

        public const string RK_Semester = "семестр";

        public const string RK_GetProfileLink = "Поделиться профилем";
        public const string RK_ResetProfileLink = "Восстановить свой профиль";

        public const string RK_Corps = "Корпуса";
        public const string RK_Other = "Другое";

        #region Corps
        public abstract class Corps {
            protected Corps(string text, double latitude, double longitude, string title, string address) {
                Text = text; Latitude = latitude; Longitude = longitude; Title = title; Address = address;
            }

            public string Text { get; }
            public double Latitude { get; }
            public double Longitude { get; }
            public string Title { get; }
            public string Address { get; }
        }

        public class RK_MainCorps : Corps {
            public const string text = "Главный корпус";
            public const double latitude = 54.166896f;
            public const double longitude = 37.586329f;
            public const string title = "Главный корпус";
            public const string address = "проспект Ленина, 92";

            public RK_MainCorps() : base(text, latitude, longitude, title, address) { }
        }
        public class RK_1Corps : Corps {
            public const string text = "1";
            public const double latitude = 54.172720f;
            public const double longitude = 37.596040f;
            public const string title = "Корпус № 1";
            public const string address = "проспект Ленина, 95";

            public RK_1Corps() : base(text, latitude, longitude, title, address) { }
        }
        public class RK_2Corps : Corps {
            public const string text = "2";
            public const double latitude = 54.172583f;
            public const double longitude = 37.594019f;
            public const string title = "Корпус № 2";
            public const string address = "проспект Ленина, 84";

            public RK_2Corps() : base(text, latitude, longitude, title, address) { }
        }
        public class RK_3Corps : Corps {
            public const string text = "3";
            public const double latitude = 54.171561f;
            public const double longitude = 37.589347f;
            public const string title = "Корпус № 3";
            public const string address = "проспект Ленина, 84к8";

            public RK_3Corps() : base(text, latitude, longitude, title, address) { }
        }
        public class RK_4Corps : Corps {
            public const string text = "4";
            public const double latitude = 54.172415f;
            public const double longitude = 37.589222f;
            public const string title = "Корпус № 4";
            public const string address = "проспект Ленина, 84к7";

            public RK_4Corps() : base(text, latitude, longitude, title, address) { }
        }
        public class RK_5Corps : Corps {
            public const string text = "5";
            public const double latitude = 54.173321f;
            public const double longitude = 37.592114f;
            public const string title = "Корпус № 5";
            public const string address = "улица Фридриха Энгельса, 155";

            public RK_5Corps() : base(text, latitude, longitude, title, address) { }
        }
        public class RK_6Corps : Corps {
            public const string text = "6";
            public const double latitude = 54.168045f;
            public const double longitude = 37.588925f;
            public const string title = "Корпус № 6";
            public const string address = "проспект Ленина, 90к1";

            public RK_6Corps() : base(text, latitude, longitude, title, address) { }
        }
        public class RK_7Corps : Corps {
            public const string text = "7";
            public const double latitude = 54.172167f;
            public const double longitude = 37.597513f;
            public const string title = "Корпус № 7";
            public const string address = "проспект Ленина, 93А";

            public RK_7Corps() : base(text, latitude, longitude, title, address) { }
        }
        public class RK_8Corps : Corps {
            public const string text = "8";
            public const double latitude = 54.167339f;
            public const double longitude = 37.587919f;
            public const string title = "Корпус № 8";
            public const string address = "улица Болдина, 153";

            public RK_8Corps() : base(text, latitude, longitude, title, address) { }
        }
        public class RK_9Corps : Corps {
            public const string text = "9";
            public const double latitude = 54.166896f;
            public const double longitude = 37.586329f;
            public const string title = "Корпус № 9";
            public const string address = "проспект Ленина, 92";

            public RK_9Corps() : base(text, latitude, longitude, title, address) { }
        }
        public class RK_10Corps : Corps {
            public const string text = "10";
            public const double latitude = 54.167866f;
            public const double longitude = 37.585143f;
            public const string title = "Корпус № 10";
            public const string address = "улица Болдина, 128";

            public RK_10Corps() : base(text, latitude, longitude, title, address) { }
        }
        public class RK_11Corps : Corps {
            public const string text = "11";
            public const double latitude = 54.167708f;
            public const double longitude = 37.586976f;
            public const string title = "Корпус № 11";
            public const string address = "улица Болдина, 151";

            public RK_11Corps() : base(text, latitude, longitude, title, address) { }
        }
        public class RK_12Corps : Corps {
            public const string text = "12";
            public const double latitude = 54.174185f;
            public const double longitude = 37.593686f;
            public const string title = "Корпус № 12";
            public const string address = "улица Агеева, 1Б";

            public RK_12Corps() : base(text, latitude, longitude, title, address) { }
        }
        public class RK_13Corps : Corps {
            public const string text = "13";
            public const double latitude = 54.172185f;
            public const double longitude = 37.596664f;
            public const string title = "Корпус № 13";
            public const string address = "";

            public RK_13Corps() : base(text, latitude, longitude, title, address) { }
        }
        public class LaboratoryCorps : Corps {
            public const string text = "ЛК 6";
            public const double latitude = 54.168537f;
            public const double longitude = 37.587775f;
            public const string title = "Лабораторный корпус №6";
            public const string address = "улица Смидович, 3А";

            public LaboratoryCorps() : base(text, latitude, longitude, title, address) { }
        }
        public class RK_FOC : Corps {
            public const string text = "ФОЦ";
            public const double latitude = 54.171898f;
            public const double longitude = 37.592267f;
            public const string title = "Физкультурно-оздоровительный центр";
            public const string address = "проспект Ленина, 84к1";

            public RK_FOC() : base(text, latitude, longitude, title, address) { }
        }
        public class RK_SanatoriumDispensary : Corps {
            public const string text = "Санаторий-профилакторий";
            public const double latitude = 54.167634f;
            public const double longitude = 37.581891f;
            public const string title = "Санаторий-профилакторий";
            public const string address = "улица Оружейная, 15к2";

            public RK_SanatoriumDispensary() : base(text, latitude, longitude, title, address) { }
        }
        public class RK_Stadium : Corps {
            public const string text = "Стадион ТулГУ";
            public const double latitude = 54.167057f;
            public const double longitude = 37.583075f;
            public const string title = "Стадион ТулГУ";
            public const string address = "улица Смидович, 12Б";
            public RK_Stadium() : base(text, latitude, longitude, title, address) { }
        }
        public class RK_PoolOnBoldin : Corps {
            public const string text = "Бассейн на ул.Болдина";
            public const double latitude = 54.168599f;
            public const double longitude = 37.582817f;
            public const string title = "Бассейн на ул.Болдина";
            public const string address = "улица Болдина, 120";

            public RK_PoolOnBoldin() : base(text, latitude, longitude, title, address) { }
        }
        public class RK_SportsComplexOnBoldin : Corps {
            public const string text = "Спортивный комплекс на ул.Болдина";
            public const double latitude = 54.168145f;
            public const double longitude = 37.583957f;
            public const string title = "Спортивный комплекс на ул.Болдина";
            public const string address = "улица Болдина, 126";

            public RK_SportsComplexOnBoldin() : base(text, latitude, longitude, title, address) { }
        }

        public const string RK_TechnicalCollege = "Технический колледж имени С.И. Мосина";
        #endregion

        public struct IK_ViewAll {
            public const string text = "Посмотреть все";
            public const string callback = "All";
        }
        public struct IK_Edit {
            public const string text = "Редактировать";
            public const string callback = "Edit";
        }
        public struct IK_Back {
            public const string text = "Назад";
            public const string callback = "Back";
        }
        public struct IK_Add {
            public const string text = "Добавить";
            public const string callback = "Add";
        }
        public struct IK_SetEndTime {
            public const string text = "";
            public const string callback = "SetEndTime";
        }

        public static readonly string[] StagesOfAdding = {"Введите название", "Введите тип", "Введите лектора", "Введите аудиторию", "Введите время начала", "Введите время конца", "Дисциплина добавлена", "Ошибка"};
    }

    public partial class TelegramBot {
        private async Task GroupErrorAdmin(ITelegramBotClient botClient, ChatId chatId) => await botClient.SendTextMessageAsync(chatId: chatId, text: $"Для того, чтобы узнать расписание, необходимо указать номер группы в настройках профиля ({Constants.RK_Other} -> {Constants.RK_Profile}).", replyMarkup: MainKeyboardMarkup);
        private async Task GroupErrorUser(ITelegramBotClient botClient, ChatId chatId) => await botClient.SendTextMessageAsync(chatId: chatId, text: $"Попросите владельца профиля указать номер группы в настройках профиля ({Constants.RK_Other} -> {Constants.RK_Profile}).", replyMarkup: MainKeyboardMarkup);
        private async Task StudentIdErrorAdmin(ITelegramBotClient botClient, ChatId chatId) => await botClient.SendTextMessageAsync(chatId: chatId, text: $"Для того, чтобы узнать успеваемость, необходимо указать номер зачетной книжки в настройках профиля ({Constants.RK_Other} -> {Constants.RK_Profile}).", replyMarkup: MainKeyboardMarkup);
        private async Task StudentIdErrorUser(ITelegramBotClient botClient, ChatId chatId) => await botClient.SendTextMessageAsync(chatId: chatId, text: $"Попросите владельца профиля указать номер зачетной книжки в настройках профиля ({Constants.RK_Other} -> {Constants.RK_Profile}).", replyMarkup: MainKeyboardMarkup);
        private async Task ScheduleRelevance(ITelegramBotClient botClient, ChatId chatId, IReplyMarkup? replyMarkup) => await botClient.SendTextMessageAsync(chatId: chatId, text: $"Расписание актуально на {Parser.scheduleLastUpdate.ToString("dd.MM HH:mm")}", replyMarkup: replyMarkup);
        private async Task ProgressRelevance(ITelegramBotClient botClient, ChatId chatId, IReplyMarkup? replyMarkup) => await botClient.SendTextMessageAsync(chatId: chatId, text: $"Успеваемость актуально на {Parser.scheduleLastUpdate.ToString("dd.MM HH:mm")}", replyMarkup: replyMarkup);
    }
}
