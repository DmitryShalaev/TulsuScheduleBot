using Core.Bot.Commands.Interfaces;
using Core.DB;
using Core.DB.Entity;

using SkiaSharp;

using Telegram.Bot.Types;

namespace Core.Bot.Commands.Admin.Statistics.Message {
    internal class Heatmap : IMessageCommand {

        public List<string> Commands => ["Heatmap"];

        public List<Mode> Modes => [Mode.Admin];

        public Manager.Check Check => Manager.Check.admin;

        public Task Execute(ScheduleDbContext dbContext, ChatId chatId, int messageId, TelegramUser user, string args) {
            DateTime months = DateTime.UtcNow.Date.AddMonths(-1);

            var activityData = dbContext.MessageLog.Where(m => m.Date > months)
                                                   .GroupBy(o => o.Date.Date)
                                                   .OrderBy(d => d.Key)
                                                   .Select(g => new ActivityData {
                                                       Date = g.Key,
                                                       Items = g.GroupBy(o => o.Date.ToLocalTime().Hour)
                                                           .Select(c => new HourlyActivity {
                                                               Hour = c.Key,
                                                               Count = c.Count()
                                                           })
                                                           .OrderByDescending(h => h.Hour).ToList()
                                                   }).ToList();

            MessagesQueue.Message.SendPhoto(chatId: chatId, path: DrawHeatmap(activityData), replyMarkup: Statics.AdminPanelKeyboardMarkup, deleteFile: true);

            return Task.CompletedTask;
        }

        public static string DrawHeatmap(List<ActivityData> activityData) {
            // Размеры для клеток и отступов
            int cellSize = 50; // Размер одной ячейки
            int paddingLeft = 25; // Отступ для подписей дней
            int paddingTop = 25;  // Отступ сверху для подписей часов
            int daysCount = activityData.Count; // Количество дней (по горизонтали)
            int imageWidth = daysCount * cellSize + paddingLeft; // Ширина изображения
            int imageHeight = 24 * cellSize + paddingTop + 50; // Высота изображения (24 часа по вертикали)

            // Создаем пустое изображение с использованием SkiaSharp
            using(var surface = SKSurface.Create(new SKImageInfo(imageWidth, imageHeight))) {
                SKCanvas canvas = surface.Canvas;
                canvas.Clear(SKColors.Black);

                // Получаем максимальное значение активности для нормализации
                int maxCount = activityData.SelectMany(a => a.Items).Max(i => i.Count);

                // Отрисовываем тепловую карту
                for(int dayIndex = 0; dayIndex < activityData.Count; dayIndex++) {
                    ActivityData dayData = activityData[dayIndex];

                    foreach(HourlyActivity hourData in dayData.Items) {
                        int hour = hourData.Hour;
                        int count = hourData.Count;

                        // Рассчитываем интенсивность цвета
                        float intensity = (float)count / maxCount;
                        byte greenValue = (byte)(255 * intensity); // Чем больше значение, тем ярче зеленый цвет

                        var paint = new SKPaint {
                            Color = new SKColor(0, greenValue, 0), // Оттенок зеленого
                            Style = SKPaintStyle.Fill
                        };

                        int x = paddingLeft + dayIndex * cellSize; // X по дням
                        int y = paddingTop + hour * cellSize;      // Y по часам

                        // Рисуем ячейку
                        canvas.DrawRect(x, y, cellSize, cellSize, paint);
                    }
                }

                // Горизонтальная ось
                using(var dayTextPaint = new SKPaint {
                    Color = SKColors.White,
                    TextSize = 12
                }) {
                    for(int i = 0; i < daysCount; i++) {
                        DateTime date = activityData[i].Date;
                        string dayLabel = date.ToString("dd.MM");

                        // Позиционируем текст для каждого дня
                        int dayX = paddingLeft + i * cellSize + 10;
                        canvas.DrawText(dayLabel, dayX, imageHeight - 10, dayTextPaint);
                    }
                }

                // Вертикальная ось
                using(var hourTextPaint = new SKPaint {
                    Color = SKColors.White,
                    TextSize = 12
                }) {
                    for(int i = 23; i >= 0; i--) {
                        string hourLabel = $"{i}";

                        // Позиционируем текст для каждого часа
                        int hourY = paddingTop + i * cellSize + cellSize / 2 + 6;
                        canvas.DrawText(hourLabel, 5, hourY, hourTextPaint);
                    }
                }

                // Сохранение изображения во временный файл
                using(SKImage image = surface.Snapshot())
                using(SKData data = image.Encode(SKEncodedImageFormat.Png, 100)) {
                    string tempFileName = Path.GetTempFileName();
                    string tempFilePath = Path.ChangeExtension(tempFileName, ".png");

                    using(FileStream stream = System.IO.File.OpenWrite(tempFilePath)) {
                        data.SaveTo(stream);
                    }

                    return tempFilePath;
                }
            }
        }
    }

    public class HourlyActivity {
        public int Hour { get; set; }
        public int Count { get; set; }
    }

    public class ActivityData {
        public DateTime Date { get; set; }
        public required List<HourlyActivity> Items { get; set; }
    }
}