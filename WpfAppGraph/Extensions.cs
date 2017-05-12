using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppGraph
{
    public static class Extensions
    {
        public static int LengthB(this string input)
        {
            return System.Text.Encoding.GetEncoding("Shift_JIS").GetByteCount(input);
        }
        public static Single ParseExNothingIsMinValue(this string input)
        {
            if (input == "")
            {
                return 0;// Single.MinValue;
            }
            return Single.Parse(input);
        }
    }
}
