﻿using Core.Bot.Interfaces;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
namespace Core.Bot.Commands.Student.Other.Profile.Message {
    internal class Profile : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => [UserCommands.Instance.Message["Profile"]];

        public List<Mode> Modes => [Mode.Default];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            user.TelegramUserTmp.TmpData = UserCommands.Instance.Message["Profile"];
            await dbContext.SaveChangesAsync();

            await BotClient.SendTextMessageAsync(chatId: chatId, text: UserCommands.Instance.Message["Profile"], replyMarkup: DefaultMessage.GetProfileKeyboardMarkup(user));
        }
    }
}
