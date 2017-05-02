using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppGraph.Logics
{
    /// <summary>
    /// https://www.codeproject.com/Tips/823670/Csharp-Light-and-Fast-CSV-Parser
    /// </summary>
    public static class CsvParser
    {
        private static readonly TraceSource trace = new TraceSource("Logics"); 

        private static Tuple<T, IEnumerable<T>> HeadAndTail<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            var en = source.GetEnumerator();
            en.MoveNext();
            return Tuple.Create(en.Current, EnumerateTail(en));
        }

        private static IEnumerable<T> EnumerateTail<T>(IEnumerator<T> en)
        {
            while (en.MoveNext()) yield return en.Current;
        }

        public static IEnumerable<IList<string>> Parse(string content, char delimiter, char qualifier)
        {
            using (var reader = new StringReader(content))
                return Parse(reader, delimiter, qualifier);
        }

        public static Tuple<IList<string>, IEnumerable<IList<string>>> ParseHeadAndTail(TextReader reader, char delimiter, char qualifier)
        {
            return HeadAndTail(Parse(reader, delimiter, qualifier));
        }

        private enum ReadStatus
        {
            Initial,
            Header,
            Data,
            Footer,
        };
        public static IEnumerable<IList<string>> Parse(TextReader reader, char delimiter, char qualifier)
        {
            var inQuote = false;
            var record = new List<string>();
            var sb = new StringBuilder();

            while (reader.Peek() != -1)
            {
                var readChar = (char)reader.Read();

                if (readChar == '\n' || (readChar == '\r' && (char)reader.Peek() == '\n'))
                {
                    // If it's a \r\n combo consume the \n part and throw it away.
                    if (readChar == '\r')
                        reader.Read();

                    if (inQuote)
                    {
                        if (readChar == '\r')
                            sb.Append('\r');
                        sb.Append('\n');
                    }
                    else
                    {
                        if (record.Count > 0 || sb.Length > 0)
                        {
                            record.Add(sb.ToString());
                            sb.Clear();
                        }

                        if (record.Count > 0)
                            yield return record;

                        record = new List<string>(record.Count);
                    }
                }
                else if (sb.Length == 0 && !inQuote)
                {
                    if (readChar == qualifier)
                        inQuote = true;
                    else if (readChar == delimiter)
                    {
                        record.Add(sb.ToString());
                        sb.Clear();
                    }
                    else if (char.IsWhiteSpace(readChar))
                    {
                        // Ignore leading whitespace
                    }
                    else
                        sb.Append(readChar);
                }
                else if (readChar == delimiter)
                {
                    if (inQuote)
                        sb.Append(delimiter);
                    else
                    {
                        record.Add(sb.ToString());
                        sb.Clear();
                    }
                }
                else if (readChar == qualifier)
                {
                    if (inQuote)
                    {
                        if ((char)reader.Peek() == qualifier)
                        {
                            reader.Read();
                            sb.Append(qualifier);
                        }
                        else
                            inQuote = false;
                    }
                    else
                        sb.Append(readChar);
                }
                else
                    sb.Append(readChar);
            }

            if (record.Count > 0 || sb.Length > 0)
                record.Add(sb.ToString());

            if (record.Count > 0)
                yield return record;
        }

        /// <summary>
        /// Da0用に自作。
        /// '"' で分離されている箇所はHeader部分だけと考えて、現状無視。
        /// また、Column Headerには、',' は'"'間に内包されないとして、これも無視
        /// Header/Data/Footerの判断は、以下として簡略化
        /// ・'"' が先頭に存在しているかどうか。
        /// ★データ内容が数字かどうか？を全行に対して行うと遅くなるため実施しない。問題が出たら速度犠牲にして行うこととする。
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="delimiter"></param>
        /// <param name="qualifier"></param>
        /// <returns>header, data, footer</returns>
        public static async Task<Tuple<List<string>, DataTable, List<string>>> ReadHeadDataTailAsync(TextReader reader)
        {
            var delimiter = ',';
            var qualifier = '"';
            var header = new List<string>();
            var data = new DataTable();
            var footer = new List<string>();
            var readStatus = ReadStatus.Header;
            var columnCount = 0;

            while (reader.Peek() != -1)
            {
                var line = await reader.ReadLineAsync();

                switch (readStatus)
                {
                    case ReadStatus.Header:
                        if (line.First() != qualifier)
                        {
                            header.Last().Split(delimiter).ToList().ForEach(f => data.Columns.Add(f.Trim(qualifier), typeof(Single)));
                            columnCount = data.Columns.Count;
                            readStatus = ReadStatus.Data;
                        }
                        break;
                    case ReadStatus.Data:
                        if (line.First() == qualifier)
                        {
                            readStatus = ReadStatus.Footer;
                        }
                        break;
                    default:
                        break;
                }

                switch (readStatus)
                {
                    case ReadStatus.Header:
                        header.Add(line);
                        break;
                    case ReadStatus.Data:
                        // TODO : 速度検証必要：重くなりそうなので
                        data.Rows.Add(line.Split(delimiter).Take(columnCount).Select(s=>s.ParseExNothingIsMinValue()).Cast<Object>().ToArray()); // Row[0]に設定されてしまうのでちと諦め中
                        //data.Rows.Add(new object[]{ 1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16});
                        //var dataRow = data.NewRow();
                        //var columns = line.Split(delimiter).Take(columnCount).ToList();
                        //for (var count = 0; count < columnCount; count++){
                        //        dataRow[count] = columns[count].ParseExNothingIsMinValue();// Single.Parse(columns[count]);
                        ////}
                        //data.Rows.Add(null);
                        break;
                    default:
                        footer.Add(line);
                        break;
                }
            }
            return Tuple.Create(header, data, footer);
        }
    }
}
