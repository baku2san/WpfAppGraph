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

namespace WpfAppGraph.ViewModel
{

    public class MainViewModel :  INotifyPropertyChanged
    {
        public MainViewModel()
        {
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
            this.PlotModel = new PlotModel();
            SetUpModel();
        }

        public string Title { get; private set; }
        public IList<DataPoint> Points { get; private set; }

        private PlotModel _PlotModel;
        public PlotModel PlotModel
        {
            get { return _PlotModel; }
            set { _PlotModel = value; OnPropertyChanged("PlotModel"); }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void SetUpModel()
        {
            PlotModel.LegendTitle = "Legend";
            PlotModel.LegendOrientation = LegendOrientation.Horizontal;
            PlotModel.LegendPlacement = LegendPlacement.Outside;
            PlotModel.LegendPosition = LegendPosition.TopRight;
            PlotModel.LegendBackground = OxyColor.FromAColor(200, OxyColors.White);
            PlotModel.LegendBorder = OxyColors.Black;

            var dateAxis = new LinearAxis() { MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Automatic, IntervalLength = 80, Title = "base"};
            PlotModel.Axes.Add(dateAxis);
            var valueAxis = new LinearAxis() { MajorGridlineStyle = LineStyle.Solid, MinorGridlineStyle = LineStyle.Automatic, Title = "value" };
            PlotModel.Axes.Add(valueAxis);
        }


        public void UpdateData(System.Data.DataTable table)
        {

            var lineSerie = new LineSeries
            {
                StrokeThickness = 2,
                MarkerSize = 3,
                //MarkerStroke = ,
                //MarkerType = markerTypes[data.Key],
                CanTrackerInterpolatePoints = false,
                Title = "cvs data",
                Smooth = false,
            };
            foreach (DataRow data in table.Rows)
            {
                lineSerie.Points.Add(new DataPoint(DateTimeAxis.ToDouble(data.Field<int>(0)), data.Field<int>(1)));
            }
            PlotModel.Series.Add(lineSerie);
        }
        public void UpdateModel()
        {
            //var measurements = Data.GetUpdateData(lastUpdate);
            //var dataPerDetector = measurements.GroupBy(m => m.DetectorId).OrderBy(m => m.Key).ToList();

            //foreach (var data in dataPerDetector)
            //{
            //    var lineSerie = PlotModel.Series[data.Key] as LineSeries;
            //    if (lineSerie != null)
            //    {
            //        data.ToList()
            //            .ForEach(d => lineSerie.Points.Add(new DataPoint(DateTimeAxis.ToDouble(d.DateTime), d.Value)));
            //    }
            //}
            //lastUpdate = DateTime.Now;
        }
    }
}
