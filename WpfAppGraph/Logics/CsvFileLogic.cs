using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppGraph.Logics
{
    /// <summary>
    /// Input部分の分離
    /// </summary>
    public class CsvFileLogic
    {
        public CsvFileLogic()
        {
        }
        public delegate void ProgressChanged(int percentage);
        public ProgressChanged UpdateProgress;

        public async Task<DataTable> ReadFileAsync(FileInfo inputFile)
        {
            try
            {
                if (inputFile.Extension.ToLower() == ".da0")
                {
                    var table = await ReadDa0FileAsync(inputFile);
                    table.TableName = inputFile.Name;
                    return table;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
            }
            return null;
        }

        /// <summary>
        /// Progressは非対応とした。
        /// ・非同期で動かす
        /// ・100%を何にするか未定
        /// </summary>
        /// <param name="inputFile"></param>
        /// <returns></returns>
        private async Task<DataTable> ReadDa0FileAsync(FileInfo inputFile)
        {
            DataTable dataTable = null;

            using (var stream = File.OpenRead(inputFile.FullName))
            using (var reader = new StreamReader(stream))
            {
                var result = await CsvParser.ReadHeadDataTailAsync(reader);
                dataTable = result.Item2;
            }
            return dataTable;
        }

        private async Task<DataTable> ReadSimpleCsvFileAsync(FileInfo inputFile)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("No.", typeof(int));
            dataTable.Columns.Add("IntRandom", typeof(int));

            // CSVが、'"'　で文字列判断をしなければいけない場合には、以下のParserに切り替えることで簡単に動作可能。その場合、10-20%程度の速度低下有。 
            //using (TextFieldParser tTextParser = new TextFieldParser(tInputFilePath, Encoding.GetEncoding("Shift_JIS"))            ) {
            using (var tTextStream = new StreamReader(new FileStream(inputFile.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan), Encoding.Default))
            {
                int LineReadCount = 0;
                int GrossByteSize = 0;
                string[] row;
                string tLine;

                while (!tTextStream.EndOfStream)
                {
                    // 1行読み込み
                    tLine = await tTextStream.ReadLineAsync();
                    row = tLine.Split(',');
                    LineReadCount++;
                    GrossByteSize += tLine.LengthB() + 2; // 改行分 0a0d で+2      in tInputFile.Length

                    int.TryParse(row[0], out int column1);
                    int.TryParse(row[1], out int column2);
                    dataTable.Rows.Add(column1, column2);

                    if (LineReadCount % 100 == 0)
                    {
                        this.UpdateProgress(GrossByteSize);
                    }
                }
                this.UpdateProgress(GrossByteSize);
            }
            return dataTable;
        }
    }
}
