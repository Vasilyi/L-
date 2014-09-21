using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRUSDominion
{
    public class ItemsList
    {
        public string ItemName { get; private set; }
        public int ItemCost { get; private set; }
        public int ItemID { get; private set; }
        public int Part1 { get; private set; }
        public int Part2 { get; private set; }
        public ItemsList(int id, string itemname, int cost, int part1, int part2)
        {
            ItemName = itemname;
            ItemID = id;
            Part1 = part1;
            Part2 = part2;
            ItemCost = cost;
        }
    }
}