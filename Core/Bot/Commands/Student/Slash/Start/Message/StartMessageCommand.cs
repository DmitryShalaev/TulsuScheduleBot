﻿using Core.Bot.Commands;
using Core.Bot.Interfaces;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
namespace Core.Bot.New.Commands.Student.Slash.Start.Message {
    public class StartMessageCommand : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string> Commands => ["/start"];

        public List<Mode> Modes => Enum.GetValues(typeof(Mode)).Cast<Mode>().ToList();

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            await BotClient.SendTextMessageAsync(chatId: chatId, text: "👋", replyMarkup: Statics.MainKeyboardMarkup);

            if(user.TelegramUserTmp.Mode == Mode.AddingDiscipline)
                dbContext.CustomDiscipline.RemoveRange(dbContext.CustomDiscipline.Where(i => !i.IsAdded && i.ScheduleProfile == user.ScheduleProfile));

            await Statics.DeleteTempMessage(user);

            user.TelegramUserTmp.TmpData = null;
            user.TelegramUserTmp.Mode = Mode.Default;

            if(string.IsNullOrWhiteSpace(user.ScheduleProfile.Group)) {
                user.TelegramUserTmp.Mode = Mode.GroupСhange;

                user.TelegramUserTmp.RequestingMessageID = (await BotClient.SendTextMessageAsync(chatId: chatId, text: "Для начала работы с ботом укажите номер учебной группы.", replyMarkup: Statics.CancelKeyboardMarkup)).MessageId;
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
