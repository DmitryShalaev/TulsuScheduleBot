﻿using Core.Bot.Interfaces;

using ScheduleBot;
using ScheduleBot.DB;
using ScheduleBot.DB.Entity;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
namespace Core.Bot.Commands.Classrooms.Message {
    internal class ClassroomWorkScheduleDefault : IMessageCommand {
        public ITelegramBotClient BotClient => TelegramBot.Instance.botClient;

        public List<string>? Commands => null;

        public List<Mode> Modes => [Mode.ClassroomSchedule];

        public Manager.Check Check => Manager.Check.none;

        public async Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            IEnumerable<string> find = NGramSearch.Instance.ClassroomFindMatch(args);

            if(find.Any()) {
                if(find.Count() > 1) {
                    var buttons = new List<InlineKeyboardButton[]>();
                    foreach(string item in find) {
                        string callback = $"Select|{item}";

                        buttons.Add([InlineKeyboardButton.WithCallbackData(text: item, callbackData: callback[..Math.Min(callback.Length, 35)])]);
                    }

                    user.TelegramUserTmp.RequestingMessageID = (await BotClient.SendTextMessageAsync(chatId: chatId, text: "Выберите аудиторию.\nЕсли её нет уточните запрос.", replyMarkup: new InlineKeyboardMarkup(buttons))).MessageId;
                } else {
                    user.TelegramUserTmp.Mode = Mode.ClassroomSelected;
                    string _classroom = user.TelegramUserTmp.TmpData = find.First();

                    var classroom = dbContext.ClassroomLastUpdate.First(i => i.Classroom == _classroom);


                    await BotClient.SendTextMessageAsync(chatId: chatId, text: $"{UserCommands.Instance.Message["CurrentClassroom"]}: {_classroom}", replyMarkup: DefaultMessage.GetClassroomWorkScheduleSelectedKeyboardMarkup(_classroom), disableWebPagePreview: true);
                }

                await dbContext.SaveChangesAsync();
            } else {
                await BotClient.SendTextMessageAsync(chatId: chatId, text: "Аудитория не найдена!", replyMarkup: Statics.WorkScheduleBackKeyboardMarkup);
            }
        }
    }
}