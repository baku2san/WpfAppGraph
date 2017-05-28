using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppGraph.BindingSources
{
    public class PercentageValueProvider: ListProvider<int>
    {
        // コンストラクタ
        public PercentageValueProvider():base(Enumerable.Range(0, 100))
        {
            Console.WriteLine(Items);
        }
    }
    public class ZoneAngleDivisionProvider : ListProviderInt
    {
        public ZoneAngleDivisionProvider() : base(2, 3, 4, 5)
        {
            //Console.WriteLine(Items);
        }
    }
    public class ZoneRadiusProvider : ListProviderInt
    {
        public ZoneRadiusProvider() : base(20, 25, 50)  // TODO: DetectZone　で少数の場合の対応を検討必要かもね。余ったところを最外周に含める、とか。現状はRadiusの公約数のみ
        {
            //Console.WriteLine(Items);
        }
    }
    public class RadiusProvider : ListProviderInt
    {
        public RadiusProvider() : base(100, 200)
        {
            //Console.WriteLine(Items);
        }
    }
    /// <summary>
    /// params は、Generic未対応なので・・
    /// </summary>
    public class ListProviderInt
    {
        public List<int> Items { get; protected set; }
        public ListProviderInt(params int[] items)
        {
            Items = items.ToList();
        }
    }
    public class ListProvider<T>
    {
        public List<T> Items { get; protected set; }
        public ListProvider(IEnumerable<T> items = null)
        {
            Items = new List<T>(items);
        }
    }
}
