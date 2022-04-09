using CryptoAlertsBot.Models;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.SkiaSharp;

namespace CryptoAlertsBot.Charts
{
    public static class ChartGenerator
    {
        public static Stream GenerateChartImageFromPricesList(List<Prices> prices, string coinName)
        {
            try
            {
                if (prices == default || prices.Count == 0)
                {
                    return default;
                }

                List<DataPoint> dataPointsList = prices.Select(s => new DataPoint(DateTimeAxis.ToDouble(s.PriceDate.Value), s.PriceUsd)).ToList();

                Stream result = GenerateChartImage(dataPointsList, coinName);

                return result;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public static Stream GenerateChartImage(IList<DataPoint> series, string title)
        {
            try
            {
                var plotModel = new PlotModel
                {
                    Title = title,
                    Background = OxyColors.White,
                    
                };

                var minValue = series[0].X;
                var maxValue = series[series.Count - 1].X; 

                int numberOfDecimals = series[0].Y.ToString().Length - (((int)series[0].Y).ToString().Length + 1);
                string stringFormat = numberOfDecimals <= 0 ? "0 USD" : $"0.{new string('0', numberOfDecimals)} USD";

                plotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, StringFormat = stringFormat });
                plotModel.Axes.Add(new DateTimeAxis { Position = AxisPosition.Bottom, Minimum = minValue, Maximum = maxValue, StringFormat = "dd/MM" });

                var line1 = new LineSeries()
                {
                    Color = OxyColors.Black,
                    StrokeThickness = 2,
                    ItemsSource = series
                };
                plotModel.Series.Add(line1);

                var pngExporter = new PngExporter() {Width = 700, Height = 400 }; 

                var stream = new MemoryStream();

                pngExporter.Export(plotModel, stream);

                return stream;
                //pngExporter.ExportToFile(pm, "C:\\Users\\Sergio\\Desktop\\test.png");
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
