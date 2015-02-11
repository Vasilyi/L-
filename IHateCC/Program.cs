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
            public static SpellSlot spellslot;
            public static float lastcleanse;

        }
        public static Menu Config;
        private static Obj_AI_Hero Player;
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Console.WriteLine("I HATE CC LOADED");
        }


        private static void Checks(Obj_AI_Base target, Obj_AI_Base source,  BuffType Type)
        {
            if ((Config.Item("ccactive").GetValue<KeyBind>().Active || Config.Item("ccactiveT").GetValue<KeyBind>().Active) && target.IsMe)
            {
                if ((Type == BuffType.Stun || Type == BuffType.Taunt || Type == BuffType.Fear || Type == BuffType.Charm))
                {
                    Console.WriteLine("CC ACTIVET : " + Config.Item("ccactiveT").GetValue<KeyBind>().Active + " CC ACTIVE : " + Config.Item("ccactive").GetValue<KeyBind>().Active + " TARGET ME : " + target.IsMe);
                    CleanseChecks(false, 0);
                };
                if (Type == BuffType.Silence && Config.Item("silence").GetValue<KeyBind>().Active)
                {
                    CleanseChecks(false, 0);
                };
                if (Type == BuffType.Suppression && Config.Item("supress").GetValue<KeyBind>().Active)
                {
                    CleanseChecks(true, 0);
                };
                if (Type == BuffType.Disarm && Config.Item("disarm").GetValue<KeyBind>().Active)
                {
                    CleanseChecks(false, 0);
                };
                if (Type == BuffType.Blind && Config.Item("blind").GetValue<KeyBind>().Active)
                {
                    CleanseChecks(false, 0);
                };
                if (Type == BuffType.Snare && Config.Item("root").GetValue<KeyBind>().Active)
                {
                    CleanseChecks(false,0);
                };
                //if (args.BuffID == "???" && Config.Item("exhaust").GetValue<KeyBind>().Active)
                //{
                //    CleanseChecks(false, args.Duration);
                //};
            };
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

            Config.AddSubMenu(new Menu("HotKey", "HotKey"));
            Config.SubMenu("HotKey").AddItem(new MenuItem("nonpackets", "Non-packet mode").SetValue(true));
            Config.SubMenu("HotKey")
                .AddItem(new MenuItem("ccactive", "Auto Cleanse").SetValue(new KeyBind(32, KeyBindType.Press)));
            Config.SubMenu("HotKey")
                .AddItem(
                    new MenuItem("ccactiveT", "Auto Cleanse (toggle)").SetValue(new KeyBind("C".ToCharArray()[0],
                        KeyBindType.Toggle)));
            Config.AddToMainMenu();
            Game.OnGameUpdate += Game_OnGameUpdate;
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
                    if (Game.Time > Config.Item("minimaltime").GetValue<Slider>().Value / 10)
                    {
                        Checks(Player, Player, buff.Type);
                    }
                };
            };
        }

        private static void CleanseChecks(bool supress, float duration)
        {
            Console.WriteLine("Checking duration " + (duration > Config.Item("minimaltime").GetValue<Slider>().Value / 10));
            if (duration > Config.Item("minimaltime").GetValue<Slider>().Value/10 || duration == 0)
            {
                CleanseSLot();
                CastCleanse(false);
            }
        }
        private static void CastCleanse(bool supress)
        {
            if (itemslots.lastcleanse == Environment.TickCount)
            {
                return;
            };
            Console.WriteLine(ObjectManager.Player.Spellbook.CanUseSpell(itemslots.spellslot));
            if (supress && itemslots.QSSslot != 0)
            {
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
                else if (itemslots.spellslot != SpellSlot.Q && ObjectManager.Player.Spellbook.CanUseSpell(itemslots.CleanseSlot) == SpellState.Ready)
                {
                    ObjectManager.Player.Spellbook.CastSpell(itemslots.CleanseSlot);
                    Console.WriteLine("Found Summoner");
                    itemslots.lastcleanse = Environment.TickCount;
                }
                else if (itemslots.spellslot != SpellSlot.Q && ObjectManager.Player.Spellbook.CanUseSpell(itemslots.spellslot) == SpellState.Ready)
                {
                    Console.WriteLine("Found Spell");
                    ObjectManager.Player.Spellbook.CastSpell(itemslots.spellslot);
                    itemslots.lastcleanse = Environment.TickCount;
                };
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
