#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using BuffLib;
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
            BuffMngr.BuffMngrInit();
            BuffMngr.OnGainBuff += BuffGained;
        }
        private static void BuffGained(Obj_AI_Base target, Obj_AI_Base source,  BuffMngr.OnGainBuffArgs args)
        {
            
            Checks(target,source,args);
        }
        private static void Checks(Obj_AI_Base target, Obj_AI_Base source,  BuffMngr.OnGainBuffArgs args)
        {
            if ((Config.Item("ccactive").GetValue<KeyBind>().Active || Config.Item("ccactiveT").GetValue<KeyBind>().Active) && target.IsMe && args.Duration > 0)
            {
                if ((args.Type == (int)BuffMngr.BuffTypes.BUFF_STUN || args.Type == (int)BuffMngr.BuffTypes.BUFF_TAUNT || args.Type == (int)BuffMngr.BuffTypes.BUFF_FEAR || args.Type == (int)BuffMngr.BuffTypes.BUFF_CHARM))
                {
                    Console.WriteLine(args.BuffID + " CC ACTIVET : " + Config.Item("ccactiveT").GetValue<KeyBind>().Active + " CC ACTIVE : " + Config.Item("ccactive").GetValue<KeyBind>().Active + " TARGET ME : " + target.IsMe);
                    CleanseChecks(false, args.Duration, args.EndTime);
                };
                if (args.Type == (int)BuffMngr.BuffTypes.BUFF_SILENCE && Config.Item("silence").GetValue<KeyBind>().Active)
                {
                    CleanseChecks(false, args.Duration, args.EndTime);
                };
                if (args.Type == (int)BuffMngr.BuffTypes.BUFF_SUPPRESS && Config.Item("supress").GetValue<KeyBind>().Active)
                {
                    CleanseChecks(true, args.Duration, args.EndTime);
                };
                if (args.Type == (int)BuffMngr.BuffTypes.BUFF_DISARM && Config.Item("disarm").GetValue<KeyBind>().Active)
                {
                    CleanseChecks(false, args.Duration, args.EndTime);
                };
                if (args.Type == (int)BuffMngr.BuffTypes.BUFF_BLIND && Config.Item("blind").GetValue<KeyBind>().Active)
                {
                    CleanseChecks(false, args.Duration, args.EndTime);
                };
                if (args.Type == (int)BuffMngr.BuffTypes.BUFF_ROOT && Config.Item("root").GetValue<KeyBind>().Active)
                {
                    CleanseChecks(false, args.Duration, args.EndTime);
                };
                //if (args.BuffID == "???" && Config.Item("exhaust").GetValue<KeyBind>().Active)
                //{
                //    CleanseChecks(false, args.Duration);
                //};
            };
        }


        private static void Checks(Obj_AI_Base target, Obj_AI_Base source,  int Type, float endtimer)
        {
            if (Config.Item("ccactive").GetValue<KeyBind>().Active || Config.Item("ccactiveT").GetValue<KeyBind>().Active)
            {  
                if ((Type == (int)BuffMngr.BuffTypes.BUFF_STUN 
                    || Type == (int)BuffMngr.BuffTypes.BUFF_TAUNT 
                    || Type == (int)BuffMngr.BuffTypes.BUFF_FEAR 
                    || Type == (int)BuffMngr.BuffTypes.BUFF_CHARM) && Config.Item("HardCC").GetValue<KeyBind>().Active)
                {
                    CleanseChecks(true, 0, endtimer);
                };
                if (Type == (int)BuffMngr.BuffTypes.BUFF_SILENCE && Config.Item("silence").GetValue<KeyBind>().Active)
                {
                    CleanseChecks(false, 0, endtimer);
                };
                if (Type == (int)BuffMngr.BuffTypes.BUFF_SUPPRESS && Config.Item("supress").GetValue<KeyBind>().Active)
                {
                    CleanseChecks(false, 0, endtimer);
                };
                if (Type == (int)BuffMngr.BuffTypes.BUFF_DISARM && Config.Item("disarm").GetValue<KeyBind>().Active)
                {
                    CleanseChecks(false, 0, endtimer);
                };
                if (Type == (int)BuffMngr.BuffTypes.BUFF_BLIND && Config.Item("blind").GetValue<KeyBind>().Active)
                {
                    CleanseChecks(false, 0, endtimer);
                };
                if (Type == (int)BuffMngr.BuffTypes.BUFF_ROOT && Config.Item("root").GetValue<KeyBind>().Active)
                {
                    CleanseChecks(false, 0, endtimer);
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
            Config.SubMenu("HotKey")
                .AddItem(new MenuItem("ccactive", "Auto Cleanse").SetValue(new KeyBind(32, KeyBindType.Press)));
            Config.SubMenu("HotKey")
                .AddItem(
                    new MenuItem("ccactiveT", "Auto Cleanse (toggle)").SetValue(new KeyBind("C".ToCharArray()[0],
                        KeyBindType.Toggle)));
            Config.AddToMainMenu();
            Game.OnGameUpdate += Game_OnGameUpdate;
            
            itemslots.CleanseSlot = Utility.GetSpellSlot(Player, "SummonerBoost", true);
            Console.WriteLine(Utility.GetSpellSlot(Player, "SummonerBoost", true) + " found cleanse");
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

                foreach (var buff in
                    ObjectManager.Player.Buffs.Where(
                        buff => Game.Time - buff.EndTime > Config.Item("minimaltime").GetValue<Slider>().Value/10))
                        {         
                    Checks(Player,Player, (int)buff.Type, buff.EndTime);
                };
            };
        }

        private static void CleanseChecks(bool supress, float duration, float endtime)
        {
            Console.WriteLine("Checking duration " + (duration > Config.Item("minimaltime").GetValue<Slider>().Value / 10));
            if (duration > Config.Item("minimaltime").GetValue<Slider>().Value/10 || duration == 0)
            {
                CleanseSLot();
                CastCleanse(false, endtime);
            }
        }
        private static void CastCleanse(bool supress, float endtimer)
        {
            if (itemslots.lastcleanse == endtimer)
            {
                return;
            };
            Console.WriteLine(ObjectManager.Player.Spellbook.CanUseSpell(itemslots.spellslot));
            if (supress && itemslots.QSSslot != 0)
            {
                Items.UseItem(itemslots.QSSslot);
            };
            if (!supress)
            {
                Console.WriteLine("Not supress " + itemslots.QSSslot + " : " + itemslots.CleanseSlot + " : " + itemslots.spellslot);
                if (itemslots.QSSslot != 0 && Items.CanUseItem(itemslots.QSSslot))
                {
                    Console.WriteLine("Found QSS");
                    Items.UseItem(itemslots.QSSslot);
                    itemslots.lastcleanse = endtimer;
                }
                else if (itemslots.spellslot != SpellSlot.Q && ObjectManager.Player.SummonerSpellbook.CanUseSpell(itemslots.CleanseSlot) == SpellState.Ready)
                {
                    ObjectManager.Player.SummonerSpellbook.CastSpell(itemslots.CleanseSlot);
                    Console.WriteLine("Found Summoner");
                    itemslots.lastcleanse = endtimer;
                }
                else if (itemslots.spellslot != SpellSlot.Q && ObjectManager.Player.Spellbook.CanUseSpell(itemslots.spellslot) == SpellState.Ready)
                {
                    Console.WriteLine("Found Spell");
                    ObjectManager.Player.Spellbook.CastSpell(itemslots.spellslot);
                    itemslots.lastcleanse = endtimer;
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