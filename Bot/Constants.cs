namespace ScheduleBot.Bot {
    static class Constants {
        public const string RK_Today = "Сегодня";
        public const string RK_Tomorrow = "Завтра";

        public const string RK_ByDays = "По дням";
        public const string RK_ForAWeek = "На неделю";

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

        public const string RK_Semester = "семестр";

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
}
