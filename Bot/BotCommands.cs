using Newtonsoft.Json.Linq;

namespace ScheduleBot.Bot {
    public class BotCommands {
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
        }
        public struct CollegeStruct {
            public string text;
            public string title;
            public CorpsStruct[] corps;
        }

        public Dictionary<string, string> Message { get; private set; }
        public Dictionary<string, CallbackStruct> Callback { get; private set; }
        public CorpsStruct[] Corps { get; private set; }
        public string[] StagesOfAdding { get; private set; }
        public CollegeStruct College { get; private set; }

        public BotCommands() {
            JObject commands;

            try {
                using(var client = new HttpClient()) {
                    using(HttpResponseMessage response = client.GetAsync(Environment.GetEnvironmentVariable("TelegramBotSettings")).Result) {
                        if(response.IsSuccessStatusCode) {
                            commands = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                        } else {
                            throw new Exception();
                        }
                    }
                }
            } catch(Exception) {
                using(StreamReader sr = new("Bot/TulsuScheduleBotSettings.json"))
                    commands = JObject.Parse(sr.ReadToEnd());
            }

            Message = commands["Message"]?.ToObject<Dictionary<string, string>>() ?? throw new NullReferenceException("Message");
            Callback = commands["Callback"]?.ToObject<Dictionary<string, CallbackStruct>>() ?? throw new NullReferenceException("Callback");
            Corps = commands["Corps"]?.ToObject<CorpsStruct[]>() ?? throw new NullReferenceException("Corps");
            StagesOfAdding = commands["StagesOfAdding"]?.ToObject<string[]>() ?? throw new NullReferenceException("StagesOfAdding");
            College = commands["College"]?.ToObject<CollegeStruct>() ?? throw new NullReferenceException("College");

            Message.TrimExcess();
            Callback.TrimExcess();
        }
    }
}
