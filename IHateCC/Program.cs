#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
#endregion

namespace IHateCC
{
    public class Program
    {

        public class itemslots
        {
            public static int QSSslot;
            public static bool mikaelslot;
            public static SpellSlot CleanseSlot;
            public static SpellSlot spellslot = 0;
            public static float lastcleanse;

        }
        public static Menu Config;
        private static Obj_AI_Hero Player;
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Console.WriteLine("I HATE CC LOADED");
        }


        private static void Checks(string Type)
        {
          //  Console.WriteLine(Type);
            try
            {
                if ((Config.Item("ccactive").GetValue<KeyBind>().Active || Config.Item("ccactiveT").GetValue<bool>()))
                {
                    if ((Type == "Stun" || Type == "Taunt" || Type == "Fear" || Type == "Charm") && Config.Item("HardCC").GetValue<bool>())
                    {
                        Console.WriteLine("CC ACTIVET : " + Config.Item("ccactiveT").GetValue<KeyBind>().Active + " CC ACTIVE : " + Config.Item("ccactive").GetValue<KeyBind>().Active + " TARGET ME : ");
                        CleanseChecks(false);
                    }
                    if (Type == "Silence" && Config.Item("silence").GetValue<bool>())
                    {
                        CleanseChecks(false);
                    }
                    if (Type == "Suppression" && Config.Item("supress").GetValue<bool>())
                    {
                        CleanseChecks(true);
                    }
                    if (Type == "Disarm" && Config.Item("disarm").GetValue<bool>())
                    {
                        CleanseChecks(false);
                    }
                    if (Type == "Blind" && Config.Item("blind").GetValue<bool>())
                    {
                        CleanseChecks(false);
                    }
                    if ((Type == "Snare" || Type == "Root") && Config.Item("root").GetValue<bool>())
                    {
                        CleanseChecks(false);
                    }

                    //if (args.BuffID == "???" && Config.Item("exhaust").GetValue<KeyBind>().Active)
                    //{
                    //    CleanseChecks(false, args.Duration);
                    //};
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("s" + e);
            }
        }

        private static bool ChampIngame(string champname)
        {
            return ObjectManager.Get<Obj_AI_Hero>().Any(h => h.IsEnemy && !h.IsMe && h.ChampionName == champname);
        }


        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            //Create the menu
            Config = new Menu("I Hate CC", "IHateCC", true);
            
            Config.AddSubMenu(new Menu("Types", "Types"));
            Config.SubMenu("Types").AddItem(new MenuItem("minimaltime", "Min CC time (10 = 1second)")).SetValue(new Slider(0, 20, 0));
            Config.SubMenu("Types").AddItem(new MenuItem("HardCC", "Cleanse HARD CC").SetValue(true));
            Config.SubMenu("Types").AddItem(new MenuItem("silence", "Cleanse Silence").SetValue(true));
            Config.SubMenu("Types").AddItem(new MenuItem("supress", "Cleanse supress").SetValue(true));
            Config.SubMenu("Types").AddItem(new MenuItem("disarm", "Cleanse disarm").SetValue(true));
            Config.SubMenu("Types").AddItem(new MenuItem("blind", "Cleanse blind").SetValue(true));
            Config.SubMenu("Types").AddItem(new MenuItem("root", "Cleanse root").SetValue(true));
            Config.SubMenu("Types").AddItem(new MenuItem("exhaust", "Cleanse exhaust").SetValue(true));

            if (ChampIngame("Zed"))
            {
                Config.SubMenu("Types").AddItem(new MenuItem("zedR", "Zed Mark").SetValue(true));
            }
            if (ChampIngame("Vladimir"))
            {
                Config.SubMenu("Types").AddItem(new MenuItem("vladR", "Vladimir ulti").SetValue(true));
            }
        Config.AddSubMenu(new Menu("HotKey", "HotKey"));
            Config.SubMenu("HotKey")
                .AddItem(new MenuItem("ccactive", "Auto Cleanse").SetValue(new KeyBind(32, KeyBindType.Press)));
            Config.SubMenu("HotKey")
                .AddItem(
                    new MenuItem("ccactiveT", "Auto Cleanse (toggle)").SetValue(new KeyBind("C".ToCharArray()[0],
                        KeyBindType.Toggle)));
            Config.AddToMainMenu();
            Game.OnUpdate += Game_OnGameUpdate;
            itemslots.CleanseSlot = Utility.GetSpellSlot(Player, "SummonerBoost");
            Console.WriteLine(Utility.GetSpellSlot(Player, "SummonerBoost") + " found cleanse");
            if (Player.BaseSkinName == "Gangplank")
            {
                itemslots.spellslot = SpellSlot.W;
                Console.WriteLine("Found Gankplank");
            };
        }

        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("ccactive").GetValue<KeyBind>().Active || Config.Item("ccactiveT").GetValue<KeyBind>().Active)
            {
                foreach (var buff in ObjectManager.Player.Buffs)
                {
                    var buffend = buff.EndTime - Game.Time;
                    if (Config.Item("zedR") != null && Config.Item("zedR").GetValue<bool>() && buff.Name == "zedulttargetmark")
                    {
                        Utility.DelayAction.Add(2900, () => CleanseChecks(true));
                    }
                    if (Config.Item("vladR") != null && Config.Item("vladR").GetValue<bool>() && buff.Name == "VladimirHemoplague")
                    {
                        CleanseChecks(true);
                    }
                    if (!buff.Caster.IsMe && buffend > Config.Item("minimaltime").GetValue<Slider>().Value / 10)
                    {
                        Console.WriteLine(buff.Name + " : " + buffend + " : " + Config.Item("minimaltime").GetValue<Slider>().Value / 10);
                        Checks(buff.Type.ToString());
                    }
                };
            };
        }

        private static void CleanseChecks(bool supress)
        {
            CleanseSLot();
            try
            {
                if (itemslots.lastcleanse + 100 > Environment.TickCount)
                {
                    return;
                };
                Console.WriteLine(ObjectManager.Player.Spellbook.CanUseSpell(itemslots.spellslot));
                if (supress && itemslots.QSSslot != 0 && Items.CanUseItem(itemslots.QSSslot))
                {
                    Console.WriteLine("Supress " + itemslots.QSSslot + " : " + itemslots.CleanseSlot + " : " + itemslots.spellslot);
                    Items.UseItem(itemslots.QSSslot);
                    itemslots.lastcleanse = Environment.TickCount;
                };
                if (!supress)
                {
                    Console.WriteLine("Not supress " + itemslots.QSSslot + " : " + itemslots.CleanseSlot + " : " + itemslots.spellslot);
                    if (itemslots.QSSslot != 0 && Items.CanUseItem(itemslots.QSSslot))
                    {
                        Console.WriteLine("Found QSS");
                        Items.UseItem(itemslots.QSSslot);
                        itemslots.lastcleanse = Environment.TickCount;
                    }
                    else if (ObjectManager.Player.Spellbook.CanUseSpell(itemslots.CleanseSlot) == SpellState.Ready)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(itemslots.CleanseSlot);
                        Console.WriteLine("Found Summoner");
                        itemslots.lastcleanse = Environment.TickCount;
                    }
                    else if (itemslots.spellslot != 0 && ObjectManager.Player.Spellbook.CanUseSpell(itemslots.spellslot) == SpellState.Ready)
                    {
                        Console.WriteLine("Found Spell");
                        ObjectManager.Player.Spellbook.CastSpell(itemslots.spellslot);
                        itemslots.lastcleanse = Environment.TickCount;
                    };
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("b" +  e);
            }
        }



        private static void CleanseSLot()
        {
            if (Items.HasItem(3140,Player)) // mercurial and QSS
            {
                itemslots.QSSslot = 3140;
            };
            if (Items.HasItem(3139,Player)) // mercurial and QSS
            {
                itemslots.QSSslot = 3139;
            };
            if (Items.HasItem(3222,Player)) // mikael
            {
                itemslots.mikaelslot = true;
            };
        }
    }
}
