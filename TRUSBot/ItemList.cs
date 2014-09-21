
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;

namespace TRUSDominion
{
    public class ChampionsItems
    {
        public static List<ItemsList> _ItemsToBuy;
        public static List<ItemsList> ItemsToBuy
        {
            get
            {
                if (_ItemsToBuy == null)
                {
                    InitializeItems();
                }

                return _ItemsToBuy;
            }
        }

        private static void InitializeItems()
        {
            _ItemsToBuy = new List<ItemsList>();
            Console.WriteLine("initialize items");
            if (ObjectManager.Player.BaseSkinName == "Ryze")
            {
                _ItemsToBuy.Add(new ItemsList(3073,"Tear of the Goddess", 120, 1027, 1004));
            }
        }
    }
}


