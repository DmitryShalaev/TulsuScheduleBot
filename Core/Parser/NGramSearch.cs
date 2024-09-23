using System.Collections.Concurrent;

using Core.DB;

namespace Core.Parser {
    public class NGramSearch {
        private static NGramSearch? instance;
        private readonly ConcurrentDictionary<string, HashSet<string>> TeachersNgramsDict;
        private readonly ConcurrentDictionary<string, HashSet<string>> ClassroomNgramsDict;

        private static readonly object _lock = new();

        private NGramSearch() {
            TeachersNgramsDict = [];
            ClassroomNgramsDict = [];
        }

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

        public static void Clear() {
            instance = null;

            GC.Collect();
        }

        private static IEnumerable<string> GetNGrams(string input, int n) {
            if(n > input.Length)
                n = input.Length;

            for(int i = 0; i <= input.Length - n; i++) {
                yield return input.Substring(i, n);
            }
        }

        public static void PrecomputeNGrams(List<string> names, ConcurrentDictionary<string, HashSet<string>> ngramsDict, int n) {
            foreach(string name in names) {
                var ngrams = new HashSet<string>(GetNGrams(name.ToLower(), n));
                ngramsDict[name] = ngrams;
            }
        }

        private static double Similarity(HashSet<string> ngrams1, HashSet<string> ngrams2) {
            int intersection = ngrams1.Intersect(ngrams2).Count();
            int union = ngrams1.Count + ngrams2.Count - intersection;
            return (double)intersection / union;
        }

        public IEnumerable<string> TeacherFindMatch(string query, int n = 3, int count = 5) {
            if(TeachersNgramsDict.IsEmpty) {
                using(ScheduleDbContext dbContext = new()) {
                    PrecomputeNGrams([.. dbContext.TeacherLastUpdate.Select(i => i.Teacher)], TeachersNgramsDict, n);
                }
            }

            query = query.ToLower().Trim();

            var queryNgrams = new HashSet<string>(GetNGrams(query, n));

            IEnumerable<string> found = TeachersNgramsDict.Select(i => new Tuple<string, double>(i.Key, Similarity(queryNgrams, i.Value)))
                .Where(i => i.Item2 != 0)
                .OrderByDescending(i => i.Item2)
                .Take(count)
                .Select(i => i.Item1);

            IEnumerable<string> contains = found.Where(i => i.Contains(query, StringComparison.CurrentCultureIgnoreCase));
            return contains.Any() ? contains : found;
        }

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
                found = ClassroomNgramsDict.Keys
                    .Select(room => new Tuple<string, int>(room, LevenshteinDistance(query, room)))
                    .OrderBy(t => t.Item2)
                    .Take(count)
                    .Select(t => t.Item1);

            } else {
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
