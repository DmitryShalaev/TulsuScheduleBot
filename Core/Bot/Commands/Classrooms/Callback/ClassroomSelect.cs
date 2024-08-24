﻿using Core.Bot.Interfaces;

using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;

#pragma warning disable CA1862 // Используйте перегрузки метода "StringComparison" для сравнения строк без учета регистра

namespace Core.Bot.Commands.Classrooms.Callback {
    public class ClassroomSelect : ICallbackCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public string Command => "Select";

        public Mode Mode => Mode.ClassroomSchedule;

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string message, string args) {
            user.TelegramUserTmp.Mode = Mode.ClassroomSelected;
            user.TelegramUserTmp.RequestingMessageID = null;

            ClassroomLastUpdate classroom = dbContext.ClassroomLastUpdate.First(i => i.Classroom.ToLower().StartsWith(args));

            string _classroom = user.TelegramUserTmp.TmpData = classroom.Classroom;
            await dbContext.SaveChangesAsync();

            await BotClient.DeleteMessageAsync(chatId: chatId, messageId: messageId);

            await BotClient.SendTextMessageAsync(chatId: chatId, text: $"{UserCommands.Instance.Message["CurrentClassroom"]}: {_classroom}", replyMarkup: DefaultMessage.GetClassroomWorkScheduleSelectedKeyboardMarkup(_classroom), disableWebPagePreview: true);
        }
    }
}