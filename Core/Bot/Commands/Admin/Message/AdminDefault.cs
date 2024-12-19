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

                foreach(TelegramUser? item in dbContext.TelegramUsers.Where(i => !i.IsDeactivated).ToList()) {
                    string message = await StatisticsForTheYear.SendStatisticsMessageAsync(dbContext, item.ChatID, globalStats);

                    MessagesQueue.Message.SendSharedTextMessage(chatId: item.ChatID, text: message, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown, disableNotification: true);
                }

                MessagesQueue.Message.SendSharedTextMessage(chatId: chatId, text: "Все сообщения доставлены", parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
            }

            MessagesQueue.Message.SendTextMessage(chatId: chatId, text: UserCommands.Instance.Message["AdminPanel"], replyMarkup: Statics.AdminPanelKeyboardMarkup);
        }
    }
}
