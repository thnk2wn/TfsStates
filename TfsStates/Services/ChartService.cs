using System.Collections.Generic;
using System.Linq;
using ChartJSCore.Models;
using ChartJSCore.Models.Bar;
using Humanizer;
using TfsStates.Models;

namespace TfsStates.Services
{
    public class ChartService : IChartService
    {
        public Chart CreateBarChart(TfsQueryResult queryResult)
        {
            Chart chart = new Chart
            {
                Type = "bar"
            };

            Data data = new Data
            {
                Labels = new List<string>()
            };

            var counts = queryResult
                .TfsItems
                .GroupBy(t => t.TransitionCount)
                .OrderBy(t => t.Key)
                .Select(c => c.Key)
                .ToList();

            var rawData = new List<double>();

            foreach (var count in counts)
            {
                data.Labels.Add($"{"cycle".ToQuantity(count)}");
                var itemCount = queryResult.TfsItems.Count(t => t.TransitionCount == count);
                rawData.Add(itemCount);
            }

            BarDataset dataset = new BarDataset()
            {
                Label = "# work items with cycle",
                Data = rawData,
                BackgroundColor = new List<string>()
                {
                "rgba(255, 99, 132, 0.2)",
                "rgba(54, 162, 235, 0.2)",
                "rgba(255, 206, 86, 0.2)",
                "rgba(75, 192, 192, 0.2)",
                "rgba(153, 102, 255, 0.2)",
                "rgba(255, 159, 64, 0.2)"
                },
                BorderColor = new List<string>()
                {
                "rgba(255,99,132,1)",
                "rgba(54, 162, 235, 1)",
                "rgba(255, 206, 86, 1)",
                "rgba(75, 192, 192, 1)",
                "rgba(153, 102, 255, 1)",
                "rgba(255, 159, 64, 1)"
                },
                BorderWidth = new List<int>() { 1 },
            };

            data.Datasets = new List<Dataset>
            {
                dataset
            };

            chart.Data = data;

            BarOptions options = new BarOptions()
            {
                Scales = new Scales(),
                BarPercentage = 0.7,
                Title = new Title
                {
                    Display = true,
                    Text = "Frequency of Cycles (transitions)"
                },
            };

            Scales scales = new Scales()
            {
                YAxes = new List<Scale>()
                {
                    new CartesianScale()
                    {
                        Ticks = new CartesianLinearTick()
                        {
                            BeginAtZero = true
                        }
                    }
                }
            };

            options.Scales = scales;
            chart.Options = options;

            chart.Options.Layout = new Layout()
            {
                Padding = new Padding()
                {
                    PaddingObject = new PaddingObject()
                    {
                        Left = 10,
                        Right = 12
                    }
                }
            };

            return chart;
        }
    }
}
