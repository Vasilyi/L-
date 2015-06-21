using System;
using System.Linq;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using TRUStinMarksman.Utils;
using TRUStinMarksman.Utils;
using TRUStinMarksman.Utils.Drawings;

namespace TRUStinMarksman
{
    internal static class TRUStinMarksman
    {
        internal static Menu Config;
        internal static Orbwalking.Orbwalker Orbwalker;
        internal static Obj_AI_Hero Player = ObjectManager.Player;

        internal static void Load()
        {
            try
            {
                //Print the welcome message
                Game.PrintChat("TRUStinMarksman Loaded!");

                // Load the menu.
                Config = new Menu("TRUStinMarksman", "TRUStinMarksman", true);

                // Add the target selector.
                TargetSelector.AddToMenu(Config.SubMenu("Selector"));

                // Add the orbwalking.
                Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalker"));

                // Load the crosshair
                Crosshair.Load();

                // Check if the champion is supported
                var type = Type.GetType("TRUStinMarksman.Champions." + Player.ChampionName);
                if (type != null)
                {
                    DynamicInitializer.NewInstance(type);
                }

                // Load whitelist harass menu
                var wList = new Menu("Harass Whitelist", "hwl");
                foreach (var enemy in HeroManager.Enemies)
                {
                    wList.AddItem(new MenuItem("hwl" + enemy.ChampionName, enemy.ChampionName))
                        .SetValue(TargetSelector.GetPriority(enemy) >= 2);
                }

                Config.SubMenu("harass").AddSubMenu(wList);

                // Add ADC items usage.
                ItemManager.Load();

                // Add the menu as main menu.
                Config.AddToMainMenu();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        internal static bool CanCombo()
        {
            // "usecombo" keybind required
            // "combomana" slider required
            return Config.Item("usecombo").GetValue<KeyBind>().Active &&
                   Player.Mana / Player.MaxMana * 100 > Config.Item("combomana").GetValue<Slider>().Value;
        }

        internal static bool CanHarass()

        {   // "harasscombo" keybind required
            // "harassmana" slider required
            return Config.Item("useharass").GetValue<KeyBind>().Active &&
                    Player.Mana/Player.MaxMana*100 > Config.Item("harassmana").GetValue<Slider>().Value;
           
        }

        internal static bool CanClear()
        {            
            // "clearcombo" keybind required
            // "clearmana" slider required
            return Config.Item("useclear").GetValue<KeyBind>().Active &&
                  Player.Mana / Player.MaxMana * 100 > Config.Item("clearmana").GetValue<Slider>().Value;               
        }

        internal static bool IsWhiteListed(Obj_AI_Hero unit)
        {
            // "harass" submenu required
            return Config.SubMenu("harass").Item("hwl" + unit.ChampionName).GetValue<bool>();
        }

        internal static IEnumerable<Obj_AI_Minion> JungleMobsInRange(float range)
        {
            var names = new[]
            {
                // summoners rift
                "SRU_Razorbeak", "SRU_Krug", "Sru_Crab",
                "SRU_Baron", "SRU_Dragon", "SRU_Blue", "SRU_Red", "SRU_Murkwolf", "SRU_Gromp",

                // twisted treeline
                "TT_NGolem5", "TT_NGolem2", "TT_NWolf6", "TT_NWolf3",
                "TT_NWraith1", "TT_Spider"
            };

            var minions = from minion in ObjectManager.Get<Obj_AI_Minion>()
                where minion.IsValidTarget(range) && !minion.Name.Contains("Mini")
                where names.Any(name => minion.Name.StartsWith(name))
                select minion;

            return minions;
        }
    }
}