using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppGraph
{
    static public class Constants
    {
        // TODO: Settingに移行必要
        internal static readonly string InvisiblePrefix = "_";
        internal static readonly string ColumnName_Zone = Constants.InvisiblePrefix + "Zone";
        internal static string TrackerKeyPlotData = "Raw";
        internal static double DeltaOfZoneSeries = 0.05;
        internal static double DeltaOfZoneSeriesStraight = 10.0;

        internal static int MaximumRadius = 100;            // 最大半径

        internal static double ZoneDisplayAngleShift = 0.5;     // Zone番号表示をする為に、Zone枠線からの角度ずらし量（前後のZoneの中間＝0.5)
        internal static double ZoneDisplayRadiusShift = 0.5;    // 同上の径版
    }
}
