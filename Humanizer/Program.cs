#region

using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
#endregion

namespace Humanizer
{
    public class Program
    {

        public static Menu Config;
        public static float lastmovement;
        private static DateTime assemblyLoadTime = DateTime.Now;
        public class LatestCast
        {
            public static float Tick = 0;
            public static float Timepass;
            public static float X = 0;
            public static float Y = 0;
            public static double Distance;
            public static double Delay;
            public static int count = 0;
            public static double SavedTime = 0;

        }

        private static Obj_AI_Hero Player;
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Console.WriteLine("Humanizer LOADED");
            Spellbook.OnCastSpell += HumanizerCast;

            Drawing.OnDraw += onDrawArgs =>
            {
                if (Config.Item("DrawTesting").GetValue<bool>())
                {
                    Drawing.DrawText(Drawing.Width - 290, 100, System.Drawing.Color.Lime, "Blocked " + LatestCast.count + " clicks");
                    Drawing.DrawText(Drawing.Width - 290, 200, System.Drawing.Color.Lime, "Blocked " + LatestCast.SavedTime + " ms");
                }
            };
        }
        public static float CurrentTick
        {
            get
            {
                return (int)DateTime.Now.Subtract(assemblyLoadTime).TotalMilliseconds;
            }
        }

        public static void HumanizerCast(Spellbook sender, SpellbookCastSpellEventArgs eventArgs)
        {
            Vector2 tempvect = new Vector2(LatestCast.X, LatestCast.Y);
            LatestCast.Timepass = CurrentTick - LatestCast.Tick;

            LatestCast.Distance = tempvect.Distance(eventArgs.StartPosition);
            LatestCast.Delay = (LatestCast.Distance * 0.001 * Config.Item("delaytime").GetValue<Slider>().Value);
            if (CurrentTick < LatestCast.Tick + LatestCast.Delay)
            {
                eventArgs.Process = false;
                LatestCast.count += 1;
                LatestCast.SavedTime += LatestCast.Delay;

            }
            if (eventArgs.Process == true && LatestCast.Timepass > Config.Item("delaytimecasts").GetValue<Slider>().Value)
            {
                LatestCast.X = eventArgs.StartPosition.X;
                LatestCast.Y = eventArgs.StartPosition.Y;
                LatestCast.Tick = CurrentTick;
            }
        }

        private static void Game_OnGameLoad(EventArgs args)
        {

            Player = ObjectManager.Player;

            //Create the menu
            Config = new Menu("Humanizer", "Humanizer", true);

            Config.AddSubMenu(new Menu("Casts delay", "Castsdelay"));
            Config.AddSubMenu(new Menu("Movements delay", "Movementdelay"));
            Config.SubMenu("Castsdelay").AddItem(new MenuItem("delaytime", "Delay time for distance")).SetValue(new Slider(200, 1000, 0));
            Config.SubMenu("Castsdelay").AddItem(new MenuItem("delaytimecasts", "Delay time between casts")).SetValue(new Slider(0, 1000, 0));
            Config.SubMenu("Movementdelay").AddItem(new MenuItem("delaytimem", "Delay time")).SetValue(new Slider(0, 1000, 0));
            Config.AddItem(new MenuItem("DrawTesting", "DrawTesting").SetValue(true));
            Config.AddToMainMenu();
        }

    }
}