using Core.Bot.Commands.Admin.Statistics;
using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using Telegram.Bot.Types;

namespace Core.Bot.Commands.Admin.Message {
    internal class AdminDefault : IMessageCommand {

        public List<string>? Commands => null;

        public List<Mode> Modes => [Mode.Admin];

        public Manager.Check Check => Manager.Check.admin;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            if(args == "Statistics") {
                string globalStats = await StatisticsForTheYear.GetGlobalStats(dbContext);

                string message = await StatisticsForTheYear.SendStatisticsMessageAsync(dbContext, chatId, globalStats);
                MessagesQueue.Message.SendTextMessage(chatId: chatId, text: message, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown, replyMarkup: Statics.AdminPanelKeyboardMarkup);
            }

            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["AdminPanel"], replyMarkup: Statics.AdminPanelKeyboardMarkup);
        }
    }
}
