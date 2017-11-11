using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using System.Data;
using WpfAppGraph.ViewModel;
using OxyPlot.Series;
using OxyPlot.Wpf;
using WpfAppGraph.Logics;

namespace WpfAppGraph
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BackgroundWorker conductor = new BackgroundWorker();
        private readonly OpenFileDialog openFile = new OpenFileDialog()
        {
            Filter = "csv files (*.csv;*.da0)|*.csv;*.da0|All files (*.*)|*.*",
            FilterIndex = 1,
        };
        private bool _isMouseDown = false;
        private MainViewModel.ChartType _chartType = MainViewModel.ChartType.PolarScatter;
        private static readonly TraceSource trace = new TraceSource("MainWindow");  // INFO: level 設定することでFiltering可能 by 2nd argument

        // KM : https://spin.atomicobject.com/2013/12/11/wpf-data-binding-debug/ でXAML-Debug
        public MainWindow()
        {
            InitializeComponent();
            conductor.DoWork += conductor_DoWork;
            conductor.RunWorkerCompleted += conductor_RunWorkerCompleted;
            conductor.ProgressChanged += conductor_ProgressChanged;
            conductor.WorkerReportsProgress = true;

            ListBoxColumns.Items.Add("hoge");
            ListBoxChartType.Items.Add("Candle");
            ListBoxChartType.Items.Add("Candle");
        }

        
        private void buttonReadCsv_Click(object sender, RoutedEventArgs e)
        {
            //if (!File.Exists(textBoxInputFilePath.Text))
            //{
                if (openFile.ShowDialog() == true)
                {
                    textBoxInputFilePath.Text = openFile.FileName;
                }
                else
                {
                    return;
                }
            //}
            trace.TraceEvent(TraceEventType.Information, 1, "read as CSV: " + textBoxInputFilePath.Text);

            buttonReadCsv.IsEnabled = false;

            // Progress.Value = 0;
            // Results.Items.Clear();

            // TODO : 非同期読み込み用にWorkerの試行。ListBoxでMulti選択での読み込みかな。で、数に応じてWorker起動と
            // TODO : 調停用Workerがあったほうがいいけど、とりあえず、pass
            var filePathes = new List<String>();
            filePathes.Add(textBoxInputFilePath.Text);
            filePathes.Add(@"C:\Users\takan\Downloads\SLOT1_SAM3 - コピー.Da0");
            
            conductor.RunWorkerAsync(filePathes);

            //            var csvFile = new FileInfo(textBoxInputFilePath.Text);
            //          progressBar.Maximum = (int)csvFile.Length;  // 2GB 超えたら問題あるけど現状無視   TODO : ProgressBarは結局今使ってない＞CircleProgressBar入れるかな
            //bgWorker.RunWorkerAsync(textBoxInputFilePath.Text);
        }

        private void conductor_DoWork(object sender, DoWorkEventArgs e)
        {
            var filePathes = e.Argument as List<String>;
            var conductor = sender as BackgroundWorker;

            foreach (var filePath in filePathes) {
                var worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                worker.DoWork += reader_DoWork;
                worker.ProgressChanged += reader_ProgressChanged;
                worker.RunWorkerCompleted += reader_RunWorkerCompleted;
                worker.RunWorkerAsync(filePath);
            }
            // TODO:　HowToWait？
        }

        private void conductor_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void conductor_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // TODO : 待ち合わせして終了／／だよね、本来

            mainGrid.IsEnabled = true;
            buttonReadCsv.IsEnabled = true;
        }

        private void reader_DoWork(object sender, DoWorkEventArgs e)
        {
            var csvFile = new FileInfo((string)e.Argument);
            var worker = sender as BackgroundWorker;

            var csvInputter = new CsvFileLogic();
            csvInputter.UpdateProgress += worker.ReportProgress;

            e.Result = csvInputter.ReadFileAsync(csvFile).Result;
        }

        private void reader_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void reader_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var table = e.Result as DataTable;
            var tableDataSource = (table as IListSource).GetList();
            var tap = Tuple.Create(TextBox1st.Content.ToString(), TextBox2nd.Content.ToString(), TextBox3rd.Content.ToString(), SliderPercentage.Value);
            mainViewModel.AddDataTable(table, tap, _chartType);    // ((MainViewModel)this.DataContext).UpdateData(table)

            if (ListBoxColumns.Items.Count > 0) { return; } // 初期から追加されていたら更新せず
            ListBoxColumns.Items.Clear();
            foreach (var column in table.Columns)
            {
                ListBoxColumns.Items.Add(column.ToString());
            }
            (sender as BackgroundWorker).ReportProgress(100);   // TODO : とりあえず100w
        }

        private void label_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
                e.Effects = DragDropEffects.Copy;
        }

        private void label_Drop(object sender, DragEventArgs e)
        {
            (sender as Label).Content = e.Data.GetData(DataFormats.Text);
        }

        private void ListBoxColumns_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = true;
        }

        private void ListBoxColumns_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseDown)
            {
                var item = (sender as ListBox)?.SelectedItem;
                if (item != null)
                {
                    DragDrop.DoDragDrop(sender as ListBox, item.ToString(), DragDropEffects.Copy);
                }
            }
            _isMouseDown = false;
        }

        private void ListBoxColumns_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = false;
        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            UpdateModel();
        }

        private void TabItem_GotFocus(object sender, RoutedEventArgs e)
        {
            _chartType = MainViewModel.ChartType.ZoneComparison;
            UpdateModel();
        }

        private void TabItem_GotFocus_1(object sender, RoutedEventArgs e)
        {
            _chartType = MainViewModel.ChartType.PolarScatter;
            UpdateModel();
        }


        private void UpdateModel()
        {
            var tap = Tuple.Create(TextBox1st.Content.ToString(), TextBox2nd.Content.ToString(), TextBox3rd.Content.ToString(), SliderPercentage.Value );
            mainViewModel.UpdateModel(tap, _chartType);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void ZoneIsVisible_Checked(object sender, RoutedEventArgs e)
        {
            mainViewModel.UpdateZone(true);
        }

        private void ZoneIsVisible_Unchecked(object sender, RoutedEventArgs e)
        {
            mainViewModel.UpdateZone(false);
        }

        private void ButtonUpdateZone_Click(object sender, RoutedEventArgs e)
        {
            mainViewModel.UpdateZoneInformation();
        }
    }
}
// TODO:    TabItem で非表示のやつのデータ更新をしていないか？してたら対応したほうが速くなるはず
