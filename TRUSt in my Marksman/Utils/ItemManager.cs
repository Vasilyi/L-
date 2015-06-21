using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LeagueSharp;
using LeagueSharp.Common;
using TRUStinMarksman.Utils;
using TRUStinMarksman.Utils.Items;

namespace TRUStinMarksman.Utils
{
    internal static class ItemManager
    {
        private static readonly List<Item> Items = new List<Item>();

        static ItemManager()
        {
            Game.OnUpdate += Game_OnUpdate;
        }

        private static Menu ItemsSubMenu
        {
            get { return TRUStinMarksman.Config.SubMenu("Items"); }
        }

        internal static void Load()
        {
            const string @namespace = "TRUStinMarksman.Utils.Items";

            var q = from t in Assembly.GetExecutingAssembly().GetTypes()
                where t.IsClass && t.Name != "Item" && t.Namespace == @namespace
                select t;

            q.ToList().ForEach(t => LoadItem((Item) DynamicInitializer.NewInstance(t))); 
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            Items.Where(item => item.IsActive).ToList().ForEach(item => item.OnUpdate());

            if (TRUStinMarksman.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
            {
                return;
            }

            Items.Where(item => item.IsActive).ToList().ForEach(item => item.Use());
        }

        private static void LoadItem(Item item)
        {
            Items.Add(item.CreateMenuItem(ItemsSubMenu));
        }
    }
}
