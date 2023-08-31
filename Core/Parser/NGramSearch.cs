using ScheduleBot.DB;

namespace ScheduleBot {
    public class NGramSearch {
        private static NGramSearch? instance;
        private readonly Dictionary<string, HashSet<string>> ngramsDict;

        private NGramSearch() => ngramsDict = new Dictionary<string, HashSet<string>>();

        public static NGramSearch Instance => instance ??= new NGramSearch();

        private static IEnumerable<string> GetNGrams(string input, int n) {
            if(n > input.Length)
                n = input.Length;

            for(int i = 0; i <= input.Length - n; i++) {
                yield return input.Substring(i, n);
            }
        }

        public void PrecomputeNGrams(List<string> names, int n) {
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

        public IEnumerable<string> FindMatch(string query, int n = 3, int count = 5) {
            if(ngramsDict.Count == 0) {
                using(ScheduleDbContext dbContext = new()) {
                    PrecomputeNGrams(dbContext.TeacherLastUpdate.Select(i => i.Teacher).ToList(), n);
                }
            }

            query = query.ToLower().Trim();

            var queryNgrams = new HashSet<string>(GetNGrams(query, n));

            IEnumerable<string> found = ngramsDict.Select(i => new Tuple<string, double>(i.Key, Similarity(queryNgrams, i.Value))).Where(i => i.Item2 != 0).OrderByDescending(i => i.Item2).Take(count).Select(i => i.Item1);

            IEnumerable<string> contains = found.Where(i => i.ToLower().Contains(query));
            return contains.Any() ? contains : found;
        }
    }
}
