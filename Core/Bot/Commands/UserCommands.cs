using Newtonsoft.Json.Linq;

namespace Core.Bot.Commands {
    public class UserCommands {
        public struct CallbackStruct {
            public string text;
            public string callback;
        }
        public struct CorpsStruct {
            public string text;
            public float latitude;
            public float longitude;
            public string title;
            public string address;
            public string? map;
        }
        public struct CollegeStruct {
            public string text;
            public string title;
            public CorpsStruct[] corps;
        }
        public struct ConfigStruct {
            public int DisciplineUpdateTime;
            public int StudentIDUpdateTime;
            public int TeacherWorkScheduleUpdateTime;
            public int DisciplineUpdateDays;
            public int UpdateAttemptTime;
        }

        public Dictionary<string, string> Message { get; private set; }
        public Dictionary<string, CallbackStruct> Callback { get; private set; }
        public CorpsStruct[] Corps { get; private set; }
        public string[] StagesOfAdding { get; private set; }
        public CollegeStruct College { get; private set; }
        public ConfigStruct Config { get; private set; }

        private static UserCommands? instance;
        public static UserCommands Instance => instance ??= new UserCommands();

        public UserCommands() {
            using(StreamReader sr = new(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"/Bot/TulsuScheduleBotSettings.json")) {
                var commands = JObject.Parse(sr.ReadToEnd());

                Message = commands[nameof(Message)]?.ToObject<Dictionary<string, string>>() ?? throw new NullReferenceException("Message");
                Callback = commands[nameof(Callback)]?.ToObject<Dictionary<string, CallbackStruct>>() ?? throw new NullReferenceException("Callback");
                Corps = commands[nameof(Corps)]?.ToObject<CorpsStruct[]>() ?? throw new NullReferenceException("Corps");
                StagesOfAdding = commands[nameof(StagesOfAdding)]?.ToObject<string[]>() ?? throw new NullReferenceException("StagesOfAdding");
                College = commands[nameof(College)]?.ToObject<CollegeStruct>() ?? throw new NullReferenceException("College");
                Config = commands[nameof(Config)]?.ToObject<ConfigStruct>() ?? throw new NullReferenceException("Config");

                Message.TrimExcess();
                Callback.TrimExcess();
            }
        }
    }
}
