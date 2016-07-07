using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfApplication1
{
    public class DataItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }

        public static IEnumerable<DataItem> CreateData(int numfakes = 5)
        {
            for (int i = 0; i < numfakes; i++)
            {
                yield return new DataItem()
                {
                    Id = "id" + i,
                    Name = "name" + i,
                    Comment= "comment" + i,
                };
            }
        }
    }
}
