using System.Collections.Concurrent;

using Core.DB;

namespace Core.Parser {

    /// <summary>
    /// Класс для поиска с использованием n-грамм. 
    /// Реализует поиск преподавателей и аудиторий по введенному запросу, используя n-граммы и алгоритм Левенштейна.
    /// </summary>
    public class NGramSearch {

        /// <summary>
        /// Экземпляр <c>NGramSearch</c> (реализован паттерн Singleton).
        /// </summary>
        private static NGramSearch? instance;

        /// <summary>
        /// Словарь n-грамм для преподавателей.
        /// </summary>
        private readonly ConcurrentDictionary<string, HashSet<string>> TeachersNgramsDict;

        /// <summary>
        /// Словарь n-грамм для аудиторий.
        /// </summary>
        private readonly ConcurrentDictionary<string, HashSet<string>> ClassroomNgramsDict;

        /// <summary>
        /// Объект блокировки для потокобезопасного создания экземпляра Singleton.
        /// </summary>
        private static readonly object _lock = new();

        /// <summary>
        /// Приватный конструктор (Singleton).
        /// Инициализирует словари n-грамм.
        /// </summary>
        private NGramSearch() {
            TeachersNgramsDict = [];
            ClassroomNgramsDict = [];
        }

        /// <summary>
        /// Свойство для получения единственного экземпляра <c>NGramSearch</c>.
        /// Потокобезопасная инициализация с использованием блокировки.
        /// </summary>
        public static NGramSearch Instance {
            get {
                if(instance is null) {
                    lock(_lock) {
                        instance ??= new NGramSearch();
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// Очищает текущий экземпляр <c>NGramSearch</c> и запускает сборщик мусора.
        /// </summary>
        public static void Clear() {
            instance = null;
            GC.Collect();
        }

        /// <summary>
        /// Генерация n-грамм для строки.
        /// </summary>
        /// <param name="input">Входная строка, для которой генерируются n-граммы.</param>
        /// <param name="n">Размер n-граммы.</param>
        /// <returns>Коллекция n-грамм.</returns>
        private static IEnumerable<string> GetNGrams(string input, int n) {
            if(n > input.Length)
                n = input.Length;

            for(int i = 0; i <= input.Length - n; i++) {
                yield return input.Substring(i, n);
            }
        }

        /// <summary>
        /// Предварительная генерация n-грамм для списка имен и их сохранение в словарь.
        /// </summary>
        /// <param name="names">Список имен для обработки.</param>
        /// <param name="ngramsDict">Словарь для сохранения n-грамм.</param>
        /// <param name="n">Размер n-грамм.</param>
        public static void PrecomputeNGrams(List<string> names, ConcurrentDictionary<string, HashSet<string>> ngramsDict, int n) {
            foreach(string name in names) {
                var ngrams = new HashSet<string>(GetNGrams(name.ToLower(), n));
                ngramsDict[name] = ngrams;
            }
        }

        /// <summary>
        /// Метод для вычисления сходства между двумя наборами n-грамм с использованием коэффициента Жаккара.
        /// </summary>
        /// <param name="ngrams1">Первый набор n-грамм.</param>
        /// <param name="ngrams2">Второй набор n-грамм.</param>
        /// <returns>Сходство между двумя наборами n-грамм.</returns>
        private static double Similarity(HashSet<string> ngrams1, HashSet<string> ngrams2) {
            int intersection = ngrams1.Intersect(ngrams2).Count();
            int union = ngrams1.Count + ngrams2.Count - intersection;
            return (double)intersection / union;
        }

        /// <summary>
        /// Поиск соответствия для имени преподавателя по запросу.
        /// </summary>
        /// <param name="query">Строка запроса.</param>
        /// <param name="n">Размер n-грамм (по умолчанию 3).</param>
        /// <param name="count">Максимальное количество результатов (по умолчанию 5).</param>
        /// <returns>Перечисление строк с именами преподавателей, которые наиболее соответствуют запросу.</returns>
        public IEnumerable<string> TeacherFindMatch(string query, int n = 3, int count = 5) {
            if(TeachersNgramsDict.IsEmpty) {
                using(ScheduleDbContext dbContext = new()) {
                    PrecomputeNGrams([.. dbContext.TeacherLastUpdate.Select(i => i.Teacher)], TeachersNgramsDict, n);
                }
            }

            query = query.ToLower().Trim();
            var queryNgrams = new HashSet<string>(GetNGrams(query, n));

            IEnumerable<string> found = TeachersNgramsDict
                .Select(i => new Tuple<string, double>(i.Key, Similarity(queryNgrams, i.Value)))
                .Where(i => i.Item2 != 0)
                .OrderByDescending(i => i.Item2)
                .Take(count)
                .Select(i => i.Item1);

            IEnumerable<string> contains = found.Where(i => i.Contains(query, StringComparison.CurrentCultureIgnoreCase));
            return contains.Any() ? contains : found;
        }

        /// <summary>
        /// Вычисляет расстояние Левенштейна между двумя строками.
        /// </summary>
        /// <param name="s">Первая строка.</param>
        /// <param name="t">Вторая строка.</param>
        /// <returns>Расстояние Левенштейна между строками.</returns>
        private static int LevenshteinDistance(string s, string t) {
            int[,] d = new int[s.Length + 1, t.Length + 1];

            for(int i = 0; i <= s.Length; i++)
                d[i, 0] = i;

            for(int j = 0; j <= t.Length; j++)
                d[0, j] = j;

            for(int i = 1; i <= s.Length; i++) {
                for(int j = 1; j <= t.Length; j++) {
                    int cost = s[i - 1] == t[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[s.Length, t.Length];
        }

        /// <summary>
        /// Поиск соответствия для аудитории по запросу.
        /// </summary>
        /// <param name="query">Строка запроса.</param>
        /// <param name="n">Размер n-грамм (по умолчанию 2).</param>
        /// <param name="count">Максимальное количество результатов (по умолчанию 5).</param>
        /// <returns>Перечисление строк с названиями аудиторий, которые наиболее соответствуют запросу.</returns>
        public IEnumerable<string> ClassroomFindMatch(string query, int n = 2, int count = 5) {
            if(ClassroomNgramsDict.IsEmpty) {
                using(ScheduleDbContext dbContext = new()) {
                    PrecomputeNGrams([.. dbContext.ClassroomLastUpdate.Select(i => i.Classroom)], ClassroomNgramsDict, n);
                }
            }

            query = query.ToLower().Trim();
            bool isNumericQuery = query.All(c => char.IsDigit(c) || c == '-' || c == ' ' || c == '.');

            IEnumerable<string> found;
            if(isNumericQuery) {
                // Поиск с использованием расстояния Левенштейна
                found = ClassroomNgramsDict.Keys
                    .Select(room => new Tuple<string, int>(room, LevenshteinDistance(query, room)))
                    .OrderBy(t => t.Item2)
                    .Take(count)
                    .Select(t => t.Item1);
            } else {
                // Поиск с использованием n-грамм
                var queryNgrams = new HashSet<string>(GetNGrams(query, n));
                found = ClassroomNgramsDict
                    .Select(i => new Tuple<string, double>(i.Key, Similarity(queryNgrams, i.Value)))
                    .Where(i => i.Item2 != 0)
                    .OrderByDescending(i => i.Item2)
                    .Take(count)
                    .Select(i => i.Item1);
            }

            IEnumerable<string> contains = found.Where(i => i.Contains(query, StringComparison.CurrentCultureIgnoreCase));
            return contains.Any() ? contains : found;
        }
    }
}
