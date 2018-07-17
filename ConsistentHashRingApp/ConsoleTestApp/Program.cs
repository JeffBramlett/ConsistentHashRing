using ConsistentHashRing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTestApp
{
    /// <summary>
    /// Console app to test with many items
    /// </summary>
    class Program
    {
        /// <summary>
        /// Entry point.  use args:  /locations:<# of locations> and /items:<# of items>
        /// Default is: locations = 2000, items = 1,000,000
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            int maxLocations = 2000;
            int maxItems = 1000000;

            foreach(string arg in args)
            {
                if(arg.StartsWith("/locations:"))
                {
                    string intStr = arg.Substring(11);
                    int locNum;
                    if(int.TryParse(intStr.Trim(), out locNum))
                    {
                        maxLocations = locNum;
                    }
                }

                if(arg.StartsWith("/items:"))
                {
                    string intStr = arg.Substring(7);
                    int locNum;
                    if (int.TryParse(intStr.Trim(), out locNum))
                    {
                        maxItems = locNum;
                    }
                }
            }

            HashRing<Guid> hashRing1 = new HashRing<Guid>();

            Console.WriteLine("Test:  Locations before Items");

            for(var i = 0; i < maxLocations; i++)
            {
                hashRing1.AddLocation(Guid.NewGuid());
            }

            for(var i = 0; i < maxItems; i++)
            {
                hashRing1.AddItem(Guid.NewGuid());
            }

            int sumOfItems = 0;
            for( var l = 0; l < hashRing1.LocationCount; l++)
            {
                Console.WriteLine("Location");
                var node = hashRing1.LocationAt(l);

                if (node.Next != null)
                    sumOfItems += 1;

                sumOfItems += node.ItemCount;
                Console.WriteLine(node.ItemCount);
            }
            Console.WriteLine("Sum of items: " + sumOfItems);
            Console.ReadKey();
        }
    }
}
