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
            DateTime months = DateTime.Now.Date.AddMonths(-1);

            var activityData = dbContext.MessageLog.Where(m => m.Date.ToLocalTime() > months)
                                                   .GroupBy(o => o.Date.ToLocalTime().Date)
                                                   .OrderBy(d => d.Key)
                                                   .Select(g => new ActivityData {
                                                       Date = g.Key.ToLocalTime(),
                                                       Items = g.GroupBy(o => o.Date.ToLocalTime().Hour)
                                                           .Select(c => new HourlyActivity {
                                                               Hour = c.Key,
                                                               Count = c.Count()
                                                           }).ToList()
                                                   }).ToList();

            MessagesQueue.Message.SendDocument(chatId: chatId, path: DrawHeatmap(activityData), name: "Heatmap.png", replyMarkup: Statics.AdminPanelKeyboardMarkup, deleteFile: true); 

            return Task.CompletedTask;
        }

        public static SKPaint textPaint = new() {
            Color = SKColors.White,
            TextSize = 12,
        };

        public static string DrawHeatmap(List<ActivityData> activityData) {
            // Размеры для клеток и отступов
            int cellSize = 55; // Размер одной ячейки
            int paddingLeft = 50; // Отступ для подписей дней
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

                var textBounds = new SKRect();

                using(SKPaint textPaint = new() {
                    Color = SKColors.White,
                    TextSize = 10,
                }) {
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

                            textPaint.MeasureText($"{count}", ref textBounds);

                            float xText = x + cellSize / 2 - textBounds.Width / 2;
                            float yText = y + cellSize / 2 - textBounds.Height / 2 - textBounds.Top;

                            canvas.DrawText($"{count}", xText, yText, textPaint);
                        }
                    }
                }

                // Горизонтальная ось
                for(int i = 0; i < daysCount; i++) {
                    DateTime date = activityData[i].Date;
                    string dayLabel = date.ToString("dd.MM");

                    // Измеряем текст
                    textPaint.MeasureText(dayLabel, ref textBounds);

                    // Позиционируем текст по центру клетки для каждого дня
                    float dayX = paddingLeft + i * cellSize + cellSize / 2 - textBounds.Width / 2;
                    canvas.DrawText(dayLabel, dayX, imageHeight - 10, textPaint);
                }

                // Вертикальная ось
                for(int i = 23; i >= 0; i--) {
                    string hourLabel = $"{i}";

                    // Измеряем текст
                    textPaint.MeasureText(hourLabel, ref textBounds);

                    // Позиционируем текст по центру клетки для каждого часа
                    float hourY = paddingTop + i * cellSize + cellSize / 2 - textBounds.Height / 2 - textBounds.Top;
                    canvas.DrawText(hourLabel, 5, hourY, textPaint);
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