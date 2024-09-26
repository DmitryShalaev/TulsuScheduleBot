using SkiaSharp;

namespace Core.Bot.Commands.Admin.Statistics {
    public static class HeatmapGenerator {
        public static string DrawHeatmap(List<ActivityData> activityData) {
            // Определение минимальной и максимальной даты
            var minDate = activityData.Min(a => a.Date);
            var maxDate = activityData.Max(a => a.Date);

            // Расчет количества недель между минимальной и максимальной датами
            int totalWeeks = GetWeekOfYear(maxDate) - GetWeekOfYear(minDate) + 1;

            int cellSize = 10; // Размер одной ячейки
            int paddingLeft = 50; // Отступ для подписей месяцев
            int imageWidth = totalWeeks * cellSize + paddingLeft; // Ширина изображения
            int imageHeight = 7 * cellSize + 40; // Высота изображения

            using(var surface = SKSurface.Create(new SKImageInfo(imageWidth, imageHeight))) {
                var canvas = surface.Canvas;
                canvas.Clear(SKColors.Black);

                // Получаем максимальное значение активности для нормализации
                var maxCount = activityData.Max(a => a.Count);

                // Отрисовываем тепловую карту
                foreach(var data in activityData) {
                    int week = GetWeekOfYear(data.Date) - GetWeekOfYear(minDate);
                    int day = (int)data.Date.DayOfWeek;

                    float intensity = (float)data.Count / maxCount;
                    byte greenValue = (byte)(255 * intensity);

                    var paint = new SKPaint {
                        Color = new SKColor(0, greenValue, 0),
                        Style = SKPaintStyle.Fill
                    };

                    int x = paddingLeft + week * cellSize;
                    int y = day * cellSize;

                    canvas.DrawRect(x, y, cellSize, cellSize, paint);
                }


                string[] months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
                using(var textPaint = new SKPaint {
                    Color = SKColors.White,
                    TextSize = 12
                }) {
                    for(int i = 0; i < 12; i++) {
                        var monthStart = new DateTime(minDate.Year, i + 1, 1);
                        if(monthStart > maxDate) break;

                        int monthWeek = GetWeekOfYear(monthStart) - GetWeekOfYear(minDate);
                        int monthPosition = paddingLeft + monthWeek * cellSize;
                        canvas.DrawText(months[i], monthPosition, imageHeight - 20, textPaint);
                    }
                }

   
                using(var image = surface.Snapshot())
                using(var data = image.Encode(SKEncodedImageFormat.Png, 100)) {
                    string tempFileName = Path.GetTempFileName();
                    string tempFilePath = Path.ChangeExtension(tempFileName, ".png");

                    using(var stream = File.OpenWrite(tempFilePath)) {
                        data.SaveTo(stream);
                    }
                    return tempFilePath;
                }
            }
        }

        public static int GetWeekOfYear(DateTime date) {
            var day = date.DayOfYear;
            return (day - 1) / 7 + 1;
        }
    }

    public class ActivityData {
        public DateTime Date { get; set; }
        public int Count { get; set; }

    }
}