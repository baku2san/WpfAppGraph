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
        public ZoneAngleDivisionProvider() : base(3, 4)
        {
            Console.WriteLine(Items);
        }
    }
    public class ZoneRadiusProvider : ListProviderInt
    {
        public ZoneRadiusProvider() : base(20,25)
        {
            Console.WriteLine(Items);
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
