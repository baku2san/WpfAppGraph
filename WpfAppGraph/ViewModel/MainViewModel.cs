﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using System.ComponentModel;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Data;
using Prism.Mvvm;
using System.Diagnostics;
using System.Windows;

namespace WpfAppGraph.ViewModel
{

    public class MainViewModel   // INotifyPropertyChanged の簡易版？
    {
        private static readonly TraceSource trace = new TraceSource("ViewModels");  // INFO: level 設定することでFiltering可能 by 2nd argument
        public MainViewModel()

        {
            this.DataTables = new List<DataTable>();
            this.CurrentTableIndex = 0;
            this.PolarScatterModel = InitialPolarScatterModel();
            this.ZoneComparisonModel = InitialZoneComparisonModel();

            // TODO 以下はSample用
            this.Title = "OxyPlot Sample";
            this.Points = new List<DataPoint>
                              {
                                  new DataPoint(0, 4),
                                  new DataPoint(10, 13),
                                  new DataPoint(20, 15),
                                  new DataPoint(30, 16),
                                  new DataPoint(40, 12),
                                  new DataPoint(50, 12)
                              };

            this.PieModel = InitialPieModel("pie1");
            this.PieModel2 = InitialPieModel(null,1300);
            this.CandleModel = InitialCandleModel("candle");

            this.FunctionModel = InitialFunctionModel("func");
            this.ScatterModel = InitialScatterModel("Scatter");
            this.PolarTypeModel = InitialPolarTypeModel("polarType");
        }

        public string Title { get; private set; }
        public IList<DataPoint> Points { get; set; }
        public PlotModel ZoneComparisonModel { get; set; }
        public PlotModel PieModel { get; set; }
        public PlotModel PieModel2 { get; set; }
        public PlotModel CandleModel { get; set; }
        public PlotModel FunctionModel { get; set; }
        public PlotModel ScatterModel { get; set; }
        public PlotModel PolarTypeModel { get; set; }
        public PlotModel PolarScatterModel { get; set; }

        private int CurrentTableIndex { get; set; }
        public List<DataTable> DataTables { get; private set; }

        private PlotModel InitialPolarScatterModel(string title = null)
        {
            var model = new PlotModel
            {
                Title = title,
                PlotType = PlotType.Polar,
                PlotAreaBorderThickness = new OxyThickness(0),
                PlotMargins = new OxyThickness(60, 20, 4, 40)
            };
            model.Axes.Add(
                new AngleAxis
                {
                    MajorStep = Math.PI / 4,
                    MinorStep = Math.PI / 16,
                    MajorGridlineStyle = LineStyle.Solid,
                    MinorGridlineStyle = LineStyle.Solid,
                    FormatAsFractions = true,
                    FractionUnit = Math.PI,
                    FractionUnitSymbol = "π",
                    Minimum = 0,
                    Maximum = 2 * Math.PI
                });
            model.Axes.Add(new MagnitudeAxis
            {
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                Minimum = 0,
                Maximum = 100,
            });
            model.Axes.Add(new LinearColorAxis()      // TODO Slot毎に上下限が変わるので色の変化も変えたほうが良いと思う
            {
                Position = AxisPosition.Left,
                Palette = OxyPalettes.Rainbow(16),
                //Minimum = 2000,
                //Maximum = 4000,
                HighColor = OxyColors.Magenta,
                LowColor = OxyColors.Green
            });
            model.Series.Add(new ScatterSeries()    // Order(ZIndex) が存在しない為、SeriesIndex順に描画される。その為、Zoneの前に実データを入れておく為、ここで暫定定義。後からSortで対応してもいいんだけどね
            {
                TrackerKey = Constants.TrackerKeyPlotData,
            });
            // Zone描画：TODO：初期から表示しておいて、最前面に常に置けるようであればそのほうが高速化にはなるはず。その場合、上の Clear()も考慮必要
            var circleDevision = 4;
            for (var circle = 1; circle < circleDevision; circle++)
            {
                // 同心円描画
                model.Series.Add(new FunctionSeries(
                    r => 25 * circle,
                    r => r * Math.PI,
                    0,
                    2,
                    Constants.DeltaOfZoneSeries)
                {
                    Color = model.DefaultColors[0],
                });
                // 放射線描画
                Enumerable.Range(1, ConvertToAngleDivision(circle)).ToList().ForEach(f => model.Series.Add(new FunctionSeries(
                    r => (Math.PI * 2 / ConvertToAngleDivision(circle)) * f,
                    25 * circle,
                    25 * (circle + 1),
                    Constants.DeltaOfZoneSeriesStraight
                    )
                {
                    Color = model.DefaultColors[0],
                    //Title = (SumOfAngleDivision(circle) + f).ToString(),
                    //RenderInLegend = false,
                    //LineLegendPosition = LineLegendPosition.None,
                    //TextColor = model.GetDefaultColor(),
                }));
                // Zone番号描画：放射線に付属させると、LineLegendPosition.Start/Endの選択だった為に、位置合わせのため、ずらして表示
                Enumerable.Range(1, ConvertToAngleDivision(circle)).ToList().ForEach(f => model.Series.Add(new FunctionSeries(
                r => (Math.PI * 2 / ConvertToAngleDivision(circle)) * (f - 0.5),
                    25 * circle + (25 / 2),
                    25 * circle + (25 / 2),
                    Constants.DeltaOfZoneSeriesStraight
                    )
                {
                    //Color = model.DefaultColors[0],
                    Title = (SumOfAngleDivision(circle) + f).ToString("00"),
                    RenderInLegend = false,
                    LineLegendPosition = LineLegendPosition.End,
                    TextColor = model.DefaultColors[0],
                    FontWeight = OxyPlot.FontWeights.Bold,
                }));
            }
            model.Series.Add(new FunctionSeries(r => 1, 0, 1, 0.1)
            {

                Title = "01",
                RenderInLegend = false,
                LineLegendPosition = LineLegendPosition.End,
                TextColor = model.DefaultColors[0],
                FontWeight = OxyPlot.FontWeights.Bold,
            });
            return model;

        }
        private PlotModel InitialPolarTypeModel(string title = null)
        {
            var model = new PlotModel
            {
                Title = title,
                Subtitle = "Archimedean spiral with equation r(θ) = θ for 0 < θ < 6π",
                PlotType = PlotType.Polar,
                PlotAreaBorderThickness = new OxyThickness(0),
                PlotMargins = new OxyThickness(60, 20, 4, 40)
            };
            model.Axes.Add(
                new AngleAxis
                {
                    MajorStep = Math.PI / 4,
                    MinorStep = Math.PI / 16,
                    MajorGridlineStyle = LineStyle.Solid,
                    MinorGridlineStyle = LineStyle.Solid,
                    FormatAsFractions = true,
                    FractionUnit = Math.PI,
                    FractionUnitSymbol = "π",
                    Minimum = 0,
                    Maximum = 2 * Math.PI
                });
            model.Axes.Add(new MagnitudeAxis
            {
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid
            });
            var series = new LineSeries();
            series.Points.Add(new DataPoint(Math.PI / 4, 1.5));
            series.Points.Add(new DataPoint(Math.PI / 8, 1.0));
            series.Points.Add(new DataPoint(Math.PI / 12, 0.5));
            series.Points.Add(new DataPoint(Math.PI / 16, 1.2));
            model.Series.Add(series);
            return model;
        }
        private PlotModel InitialScatterModel(string title = null)
        {
            var model = new PlotModel() {
                Title = title,
                LegendPlacement = LegendPlacement.Outside,
            };
            var xAxis = new LinearAxis() { Minimum = -1, Maximum = 1, Position = AxisPosition.Bottom, MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Automatic, Title = "x" };
            model.Axes.Add(xAxis);
            var yAxis = new LinearAxis() { Minimum =-1, Maximum = 1, Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Automatic, Title = "y" };
            model.Axes.Add(yAxis);

            var s1 = new ScatterSeries()
            {
                Title = "Diamond",
                MarkerType = MarkerType.Diamond,
                MarkerStrokeThickness = 0,
                BinSize = 1,
            };
            var random = new Random(Environment.TickCount);
            for (int i = 0; i < 50; i++)
            {
                var x = random.NextDouble();
                var y = random.NextDouble();
                var z = random.NextDouble();
                s1.Points.Add(new ScatterPoint(x, y) { Value = z });
            }

            model.Series.Add(s1);

            model.Axes.Add(new LinearColorAxis {
                Position = AxisPosition.Right, Palette = OxyPalettes.Rainbow(16), Minimum = -1, Maximum = 1, HighColor = OxyColors.Magenta, LowColor = OxyColors.Green });

            var s2 = new ScatterSeries
            {
                Title = "star",
                MarkerType = MarkerType.Star,
                MarkerSize = 6,
            };
            for (int i = 0; i < 50; i++)
            {
                var x = random.NextDouble();
                int odd = (int)(x * 100) % 2;
                x = x * ((odd == 0) ? 1: -1);
                var y = random.NextDouble();
                odd = (int)(y * 100) % 2;
                y = y * ((odd == 0) ? 1 : -1);
                var z = random.NextDouble();
                odd = (int)(z * 100) % 2;
                z = z * ((odd == 0) ? 1 : -1);
                s2.Points.Add(new ScatterPoint(x, y) { Value = z });
            }

            model.Series.Add(s2);
            return model;
        }
        private PlotModel InitialFunctionModel(string title = null)
        {
            var model = new PlotModel { PlotType = PlotType.XY };
            var series = new FunctionSeries(
                t => 2 * Math.Cos(t) - Math.Cos(2 * t), // x座標の関数
                t => 2 * Math.Sin(t) - Math.Sin(2 * t), // y座標の関数
                0,                                      // tの最小値
                2 * Math.PI,                            // tの最大値
                Math.PI / 32,                           // tの刻み幅
                "カージオイド");                        // グラフタイトル
            model.Series.Add(series);

            var series2 = new OxyPlot.Series.FunctionSeries(
                StandardNormalDistribution,     // 引数double，戻り値doubleの関数
                -3,                             // x座標の最小値
                1,                              // x座標の最大値
                0.01,                           // x座標の刻み幅
                "標準正規分布");                // グラフタイトル
            model.Series.Add(series2);
            return model;
        }
        private static double StandardNormalDistribution(double x)
        {
            var tX = x + 1;
            return 3*(1 / Math.Sqrt(2 * Math.PI)) * Math.Exp(-tX * tX / 2);
        }

        private PlotModel InitialCandleModel(string title = null)
        {
            var model = new PlotModel();
            try
            {
                model.Title = title;
                var seriesCandle = new CandleStickSeries
                {
                    //Title = "candleSample",
                    //StrokeThickness = 2.0,
                    //DataFieldX = "x 側",
                    //DataFieldHigh = "H",
                    //DataFieldLow = "L",
                    //DataFieldClose = "C",
                    //DataFieldOpen = "O",
                    //TrackerFormatString = "Date: {2}\nOpen: {5:0.00000}\nHigh: {3:0.00000}\nLow: {4:0.00000}\nClose: {6:0.00000}"   // IMPORTANT : Display message when left click!
                };
                var startTimeValue = DateTimeAxis.ToDouble(new DateTime(2016, 1, 1));
                var random = new Random(Environment.TickCount);
                var openCloseWidth = 20;
                var lowHighWidth = 40;
                for (var count = 0; count < 100; count++) {
                    seriesCandle.Items.Add(new HighLowItem(
                                                startTimeValue + count,
                                                random.Next(count, count + lowHighWidth),
                                                random.Next(count - lowHighWidth, count),
                                                random.Next(count, count + openCloseWidth),
                                                random.Next(count - openCloseWidth, count)));
                }
                model.Series.Add(seriesCandle);


                var dateAxis = new DateTimeAxis()
                {
                    Title = "date?",
                    Position = AxisPosition.Bottom,
                    MinorIntervalType = DateTimeIntervalType.Auto,
                    MajorGridlineStyle = LineStyle.Dot,
                    MinorGridlineStyle = LineStyle.Dot,
                    MajorGridlineColor = OxyColor.FromRgb(44, 44, 44),
                    TicklineColor = OxyColor.FromRgb(82, 82, 82)
                };
                model.Axes.Add(dateAxis);
                var valueAxis = new LinearAxis() { Position = AxisPosition.Left, MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Automatic, Title = "value" };
                model.Axes.Add(valueAxis);
            }
            catch (Exception ex) when (ex.InnerException!=null)
            {
                Console.WriteLine(ex);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally { }
            return model;
        }

        private PlotModel InitialPieModel(string title = null, int offset = 0)
        {            
            var model = new PlotModel();
            model.Title = title;
            dynamic seriesP1 = new PieSeries { StrokeThickness = 2.0, InsideLabelPosition = 0.8, AngleSpan = 360, StartAngle = 0 };
            seriesP1.Slices.Add(new PieSlice("Africa", 1030 ) { IsExploded = false, Fill = OxyColors.PaleVioletRed });
            seriesP1.Slices.Add(new PieSlice("Americas", 929) { IsExploded = true });
            seriesP1.Slices.Add(new PieSlice("Asia", 4157 ) { IsExploded = true });
            seriesP1.Slices.Add(new PieSlice("Europe", 739) { IsExploded = true });
            seriesP1.Slices.Add(new PieSlice("Oceania", 35 + offset) { IsExploded = true });
            model.Series.Add(seriesP1);
            return model;
        }
        private PlotModel InitialZoneComparisonModel()
        {
            var model = new PlotModel()
            {
                LegendOrientation = LegendOrientation.Horizontal,
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.TopRight,
                LegendBackground = OxyColor.FromAColor(200, OxyColors.White),
                LegendBorder = OxyColors.Black,
            };
            var xAxis = new LinearAxis()
            {
                Key = "X",
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Automatic,
                //IntervalLength = 80,
                Title = "Zone"
            };
            model.Axes.Add(xAxis);
            var yAxis = new LinearAxis()
            {
                Key = "Y",
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Automatic,
                Title = "Value"
            };
            model.Axes.Add(yAxis);
            return model;
        }

        public enum ChartType
        {
            PolarScatter,   // シリコンウェハーの粗さをScatterで円形表示する為のもの
            ZoneComparison,
        }
        public void AddDataTable(DataTable table, Tuple<string, string, string, double> selectedItems, ChartType chartType)
        {
            if (this.DataTables.Any(a=>a.TableName == table.TableName))
            {
                return;
            }
            else
            {
                this.DataTables.Clear();    // TODO : とりあえず、一面更新で動作させる
                this.CurrentTableIndex = 0;
                this.DataTables.Add(table);
                AddZoneColumn();    // TODO : Zone解析をする際に対応が必要。更新する為に、Calss定義化とか
            }
            try
            {
                UpdateModel(selectedItems, chartType);
            }
            catch(Exception ex)
            {
                trace.TraceEvent(TraceEventType.Error, 1, ex.Message);
            }
            return;
        }
        // TODO: 結構重いので、Modelを分けて、Form上で一括で IsVisible の変更をしたいところ。その場合、描画領域の追従方法（円の中心とサイズ）を検討必要
        public void UpdateZone(bool isVisible)
        {
            if (this.DataTables.Count == 0) { return; }
            var table = this.DataTables[this.CurrentTableIndex];
            var currentModel = this.PolarScatterModel;
            currentModel.Series.Where(w => w.TrackerKey != Constants.TrackerKeyPlotData).ToList().ForEach(f => f.IsVisible = isVisible);
            currentModel.InvalidatePlot(true);
        }
        public void UpdateModel(Tuple<string, string, string, double> selectedItems, ChartType chartType)
        {
            // TODO : tuple（選択Column)内容が、対象のTable.Columnsに存在しているかの確認が必要
            var table = this.DataTables[this.CurrentTableIndex];
            PlotModel currentModel = null;
            switch (chartType)
            {
                case ChartType.PolarScatter:
                    currentModel = this.PolarScatterModel;
                    var polarScatterItemSource = table.AsEnumerable().Select(s => new ScatterPoint(
                        s.Field<Single>(selectedItems.Item1),
                        s.Field<Single>(selectedItems.Item2) / 180 * Math.PI, 3,
                        s.Field<Single>(selectedItems.Item3)));
                    var rawIndex = currentModel.Series.IndexOf(currentModel.Series.FirstOrDefault(w => w.TrackerKey == Constants.TrackerKeyPlotData));  // TODO 一本だけで良いはずだけど、差分等の対応時にはなにがしか必要かも
                    currentModel.Series[rawIndex] = new ScatterSeries()
                    {
                        TrackerKey = Constants.TrackerKeyPlotData,
                        ItemsSource = polarScatterItemSource,
                        TrackerFormatString = "{0}\n{1}: {2:0.0}\n{3}: {4:0.0}\n{5}: {6:0.000}",
                    };
                    currentModel.InvalidatePlot(true);
                    return;

                case ChartType.ZoneComparison:
                    var startTimeValue = DateTimeAxis.ToDouble(new DateTime(2016, 1, 1));
                    var targetColumnName = selectedItems.Item3;
                    var targetRangeUpperPercentage = selectedItems.Item4;   // 昇順にし、この％の位置にある値を取得する。100%=最大値（0,100は非選択としてあるが）
                    var targetRangeUpper = targetRangeUpperPercentage / 100;
                    currentModel = this.ZoneComparisonModel;
                    var candleItemSource = table.AsEnumerable().GroupBy(g => g.Field<int>("_Zone")).Select(s => new HighLowItem(
                        s.Key,
                        s.Max(a => a.Field<Single>(targetColumnName)),
                        s.Min(a => a.Field<Single>(targetColumnName)),
                        s.Average(a => a.Field<Single>(targetColumnName)),
                        s.OrderBy(a=> a.Field<Single>(targetColumnName)).Skip((int)((Single)s.Count() * targetRangeUpper)).First().Field<Single>(targetColumnName)
                        )).OrderBy(o => o.X);

                    currentModel.Series.Clear();  // TODO 一本だけで良いはずだけど、差分等の対応時にはなにがしか必要かも
                    currentModel.Series.Add(new CandleStickSeries()
                    {
                        ItemsSource = candleItemSource,
                        DataFieldX = "Zone",
                        DataFieldHigh = "Max",
                        DataFieldLow = "Min",
                        DataFieldOpen = "Average",
                        DataFieldClose = targetRangeUpperPercentage + "%",
                        TrackerFormatString = "{0}\n{1}: {2}\nMax: {3:0.###}\n" + targetRangeUpperPercentage + "%: {6:0.###}\nAverage: {5:0.###}\nMin: {4:0.###}",
                    });
                    // TODO : 自動で良いと思うが一応後で調査するかも
                    //currentModel.GetAxisOrDefault("Y", null).Minimum = candleItemSource.Min(m => m.Low);
                    //currentModel.GetAxisOrDefault("Y", null).Maximum = candleItemSource.Max(m => m.High);
                    //currentModel.GetAxisOrDefault("X", null).Minimum = 1;
                    //currentModel.GetAxisOrDefault("X", null).Maximum = candleItemSource.Count();
                    currentModel.InvalidatePlot(true);
                    return;

                default:
                    break;
            }
        }
        public void AddZoneColumn()
        {
            var table = this.DataTables[this.CurrentTableIndex];
            var zoneColumnName = Constants.InvisiblePrefix + "Zone";
            var column = table.Columns.Add(zoneColumnName, typeof(int));
            foreach (var row in table.AsEnumerable())
            {
                row[zoneColumnName] = DetectZone(row.Field<Single>("Radius (mm)"), row.Field<Single>("Angle (deg)"));
            }
            //for (var zoneNumber = 1; zoneNumber < 23; zoneNumber++)
            //{
            //    Console.WriteLine("zone " + zoneNumber + " : " + dataTable.AsEnumerable().Count(c => c.Field<int>("_Zone") == zoneNumber));
            //}
        }
        private int ConvertToAngleDivision(int circleNumber) {
            switch (circleNumber)
            {
                case 0:
                    return 1;
                default:
                    return (int)Math.Pow(2, circleNumber - 1) * 3;
            }
        }
        private int SumOfAngleDivision(int circleNumber)
        {
            return Enumerable.Range(0, circleNumber).Select(s => ConvertToAngleDivision(s)).Sum();
        }
        private int DetectZone(Single radius, Single angle)
        {
            if (radius < 25)
            {
                return 1;
            }
            else if (radius < 50)
            {
                if (angle < 120)
                {
                    return 2;
                }
                else if (angle < 240)
                {
                    return 3;
                }
                else
                {
                    return 4;
                }
            }
            else if (radius < 75)
            {
                return (int)angle / 60 + 5;
            }
            else
            {
                return (int)angle / 30 + 11;
            }
        }
    }
}
