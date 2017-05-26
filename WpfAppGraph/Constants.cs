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
        internal static string TrackerKeyPlotData = "Raw";
        internal static double DeltaOfZoneSeries = 0.05;
        internal static double DeltaOfZoneSeriesStraight = 10.0;

        internal static int MaximumRadius = 100;            // 最大半径

        internal static double ZoneDisplayAngleShift = 0.5;     // Zone番号表示をする為に、Zone枠線からの角度ずらし量（前後のZoneの中間＝0.5)
        internal static double ZoneDisplayRadiusShift = 0.5;    // 同上の径版
        internal static int ZoneDivideRadius = 25;              // 半径の分割長
        internal static int ZoneFirstAngleDivision= 3;          // 径の最初の分割数
        internal static int ZoneFirstDivideAngle = 360 / ZoneFirstAngleDivision;     // 径の最初の分割角度
    }
}
