using System;
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
using WpfAppGraph.BindingSources;

namespace WpfAppGraph.ViewModel
{
    // TODO : 試作用のModelが沢山あるので、最終的に削除必要。same as XAML

    public class MainViewModel   // INotifyPropertyChanged の簡易版？
    {
        private static readonly TraceSource trace = new TraceSource("ViewModels");  // INFO: level 設定することでFiltering可能 by 2nd argument
        public MainViewModel()

        {
            this.ZoneInfo = new ZoneInformation();
            this.DataTables = new List<DataTable>();
            this.CurrentTableIndex = 0;
            this.PolarScatterModel = InitialPolarScatterModel();
            this.ZoneComparisonModel = InitialZoneComparisonModel();

            this.PieModel = InitialPieModel("pie1");
            this.PieModel2 = InitialPieModel(null,1300);
            this.CandleModel = InitialCandleModel("candle");

            this.ScatterModel = InitialScatterModel("Scatter");
            this.PolarTypeModel = InitialPolarTypeModel("polarType");
        }

        private ZoneInformation ZoneInfo { get; set; }

        public PlotModel ZoneComparisonModel { get; set; }
        public PlotModel PieModel { get; set; }
        public PlotModel PieModel2 { get; set; }
        public PlotModel CandleModel { get; set; }
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
                    Title ="Angle",
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
                Title = "Magnitude",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Solid,
                //Minimum = 0,
                //Maximum = 100,
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
            AddZoneSeries(model);
            return model;

        }
        private void AddZoneSeries(PlotModel model)
        {
            // TODO : Zone設定更新時の描画更新
            // Zone描画：TODO：初期から表示しておいて、最前面に常に置けるようであればそのほうが高速化にはなるはず。その場合、上の Clear()も考慮必要
            var circleDevision = this.ZoneInfo.Radius / this.ZoneInfo.ZoneRadius;
            for (var circle = 1; circle < circleDevision; circle++)
            {
                // 同心円描画
                model.Series.Add(new FunctionSeries(
                    r => this.ZoneInfo.ZoneRadius * circle,
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
                    this.ZoneInfo.ZoneRadius * circle,
                    this.ZoneInfo.ZoneRadius * (circle + 1),
                    this.ZoneInfo.ZoneRadius * Constants.DeltaOfZoneSeriesDeltaRatio
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
                    r => (Math.PI * 2 / ConvertToAngleDivision(circle)) * (f - Constants.ZoneDisplayAngleShift),
                    this.ZoneInfo.ZoneRadius * circle + (this.ZoneInfo.ZoneRadius * Constants.ZoneDisplayRadiusShift),
                    this.ZoneInfo.ZoneRadius * circle + (this.ZoneInfo.ZoneRadius * Constants.ZoneDisplayRadiusShift),
                    this.ZoneInfo.ZoneRadius * Constants.DeltaOfZoneSeriesDeltaRatio
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
            // Zone 01(中心) の描画
            model.Series.Add(new FunctionSeries(r => 1, 0, 1, 0.1)
            {
                Title = "01",
                RenderInLegend = false,
                LineLegendPosition = LineLegendPosition.End,
                TextColor = model.DefaultColors[0],
                FontWeight = OxyPlot.FontWeights.Bold,
            });
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
                    var targetRangeLower = 1 - targetRangeUpper;
                    currentModel = this.ZoneComparisonModel;
                    var boxPlotItemSource = table.AsEnumerable().GroupBy(g => g.Field<int>(Constants.ColumnName_Zone)).Select(s => new BoxPlotItem(
                        s.Key,
                        s.Min(a => a.Field<Single>(targetColumnName)),
                        s.OrderBy(a => a.Field<Single>(targetColumnName)).Skip((int)((Single)s.Count() * targetRangeLower)).First().Field<Single>(targetColumnName),
                        s.OrderBy(a => a.Field<Single>(targetColumnName)).Skip((int)((Single)s.Count() * 0.5)).First().Field<Single>(targetColumnName),
                        s.OrderBy(a => a.Field<Single>(targetColumnName)).Skip((int)((Single)s.Count() * targetRangeUpper)).First().Field<Single>(targetColumnName),
                        s.Max(a => a.Field<Single>(targetColumnName))
                        )
                        {
                            Mean = s.Average(a => a.Field<Single>(targetColumnName)),
                        }
                        ).OrderBy(o => o.X);
                    // TODO : 計算量がやばそうなので減らすべく考える必要ありかも
                    //      http://stackoverflow.com/questions/4140719/calculate-median-in-c-sharp

                    currentModel.Series.Clear();  // TODO 一本だけで良いはずだけど、差分等の対応時にはなにがしか必要かも
                    currentModel.Series.Add(new BoxPlotSeries()
                    {
                        ItemsSource = boxPlotItemSource,
                        BoxWidth = 0.3,
                        StrokeThickness = 1,
                        MedianThickness = 2,
                        MeanThickness = 2,
                        //OutlierSize = 2,                    // outlier : 外れ値。必要になったらこの辺りも表示かな
                        //OutlierType = MarkerType.Circle,
                        MedianPointSize = 2,
                        MeanPointSize = 2,
                        WhiskerWidth = 0.5,
                        LineStyle = LineStyle.Solid,
                        ShowBox = true,
                        ShowMedianAsDot = false,
                        ShowMeanAsDot = false,
                    //TrackerFormatString = "{0}\n{1}: {2}\nMax: {3:0.###}\n" + targetRangeUpperPercentage + "%: {6:0.###}\nAverage: {5:0.###}\nMin: {4:0.###}",
                    });
                    currentModel.GetAxisOrDefault("Y", null).Minimum = boxPlotItemSource.Min(m => m.LowerWhisker);
                    currentModel.GetAxisOrDefault("Y", null).Maximum = boxPlotItemSource.Max(m => m.UpperWhisker);
                    currentModel.GetAxisOrDefault("X", null).Minimum = 0;
                    currentModel.GetAxisOrDefault("X", null).Maximum = boxPlotItemSource.Count() + 1;
                    currentModel.InvalidatePlot(true);
                    return;

                default:
                    break;
            }
        }

        // KM : settings https://www.codeproject.com/articles/25829/user-settings-applied
        private class ZoneInformation
        {
            public ZoneInformation()
            {
                _radius = new RadiusProvider().GetItem((int)Properties.Settings.Default["Radius"]);
                _zoneRadius = new ZoneRadiusProvider().GetItem((int)Properties.Settings.Default["ZoneRadius"]);
                _zoneAngleDivision = new ZoneAngleDivisionProvider().GetItem((int)Properties.Settings.Default["ZoneAngleDivision"]);
            }

            private int _radius;
            public int Radius { get { return _radius; } }
            private int _zoneRadius;
            public int ZoneRadius { get { return _zoneRadius; } }
            private int _zoneAngleDivision;
            public int ZoneAngleDivision { get { return _zoneAngleDivision; } }
        }
        // TODO : 100を25で分割した際に、最大径がおかしい気がするので調査必要。20と表示上限が相違ある為。
        public void UpdateZoneInformation()
        {
            this.ZoneInfo = new ZoneInformation();
            var currentModel = this.PolarScatterModel;
            currentModel.Series.Where(w => w.TrackerKey != Constants.TrackerKeyPlotData).ToList().ForEach(f => currentModel.Series.Remove(f));
            AddZoneSeries(currentModel);
            currentModel.InvalidatePlot(true);
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
        // TODO : UpdateZoneColumn : wrap して、DataTableに追加したZoneColumnを更新する必要あり。
        public void AddZoneColumn()
        {
            var table = this.DataTables[this.CurrentTableIndex];
            var zoneColumnName = Constants.ColumnName_Zone;
            var column = table.Columns.Add(zoneColumnName, typeof(int));
            foreach (var row in table.AsEnumerable())
            {
                row[zoneColumnName] = DetectZone(row.Field<Single>("Radius (mm)"), row.Field<Single>("Angle (deg)"));
            }
            // ZoneのPoint数を確認用に。分割予定数を超えたとこが０かも確認。
            var circleDevision = this.ZoneInfo.Radius / this.ZoneInfo.ZoneRadius;
            if (table.AsEnumerable().Count(c=>c.Field<int>(Constants.ColumnName_Zone) == SumOfAngleDivision(circleDevision) + 1) > 0)
            {
                throw new Exception("zone exceeds.");
            }
            //for (var zoneNumber = 1; zoneNumber <= SumOfAngleDivision(circleDevision) + 1; zoneNumber++)
            //{
            //    //Console.WriteLine("zone " + zoneNumber + " : " + table.AsEnumerable().Count(c => c.Field<int>(Constants.ColumnName_Zone) == zoneNumber));
            //}
        }
        private int SumOfAngleDivision(int circleNumber)
        {
            return Enumerable.Range(0, circleNumber).Select(s => ConvertToAngleDivision(s)).Sum();
        }
        private int ConvertToAngleDivision(int circleNumber) {
            switch (circleNumber)
            {
                case 0:
                    return 1;
                default:
                    return (int)(Math.Pow(2, circleNumber - 1) * this.ZoneInfo.ZoneAngleDivision);
            }
        }
        private int DetectZone(Single radius, Single angle)
        {
            var circleNumber = (int)(radius / this.ZoneInfo.ZoneRadius);  // 0,1,2,3
                                                                        // 1,3,6,12,24
            int zone = 0;
            if (circleNumber == 0)
            {
                zone = 1;
            }
            else
            {
                var k = (int)(Math.Pow(2, circleNumber - 1) * this.ZoneInfo.ZoneAngleDivision);
                zone = (int)(angle / (360 / ((double)k))) + k + 2 - this.ZoneInfo.ZoneAngleDivision;
            }
            return zone;
        }
    }
}
