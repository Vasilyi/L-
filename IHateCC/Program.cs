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
            //Console.WriteLine("Buff gained packet");
            Checks(target,source,args);
        }
        private static void Checks(Obj_AI_Base target, Obj_AI_Base source,  BuffMngr.OnGainBuffArgs args)
        {
            //Console.WriteLine("Doing checks PACKETS");
            if (Config.Item("ccactive").GetValue<KeyBind>().Active && Config.Item("ccactiveT").GetValue<KeyBind>().Active)
            {
                //hard CC
                if (!target.IsMe || (!Config.Item("ccactive").GetValue<KeyBind>().Active && !Config.Item("ccactiveT").GetValue<KeyBind>().Active))
                {
                    return;
                };
                if ((args.Type == (int)BuffMngr.BuffTypes.BUFF_STUN || args.Type == (int)BuffMngr.BuffTypes.BUFF_TAUNT || args.Type == (int)BuffMngr.BuffTypes.BUFF_FEAR || args.Type == (int)BuffMngr.BuffTypes.BUFF_CHARM) && Config.Item("HardCC").GetValue<KeyBind>().Active)
                {
                    CleanseChecks(false, args.Duration);
                };
                if (args.Type == (int)BuffMngr.BuffTypes.BUFF_SILENCE && Config.Item("silence").GetValue<KeyBind>().Active)
                {
                    CleanseChecks(false, args.Duration);
                };
                if (args.Type == (int)BuffMngr.BuffTypes.BUFF_SUPPRESS && Config.Item("supress").GetValue<KeyBind>().Active)
                {
                    CleanseChecks(true, args.Duration);
                };
                if (args.Type == (int)BuffMngr.BuffTypes.BUFF_DISARM && Config.Item("disarm").GetValue<KeyBind>().Active)
                {
                    CleanseChecks(false, args.Duration);
                };
                if (args.Type == (int)BuffMngr.BuffTypes.BUFF_BLIND && Config.Item("blind").GetValue<KeyBind>().Active)
                {
                    CleanseChecks(false, args.Duration);
                };
                if (args.Type == (int)BuffMngr.BuffTypes.BUFF_ROOT && Config.Item("root").GetValue<KeyBind>().Active)
                {
                    CleanseChecks(false, args.Duration);
                };
                //if (args.BuffID == "???" && Config.Item("exhaust").GetValue<KeyBind>().Active)
                //{
                //    CleanseChecks(false, args.Duration);
                //};
            }
        }


        private static void Checks(Obj_AI_Base target, Obj_AI_Base source,  int Type)
        {
            if (Config.Item("ccactive").GetValue<KeyBind>().Active && Config.Item("ccactiveT").GetValue<KeyBind>().Active)
            {
                Console.WriteLine("Doing checks offpacket");
                //hard CC
                if (!target.IsMe || (!Config.Item("ccactive").GetValue<KeyBind>().Active && !Config.Item("ccactiveT").GetValue<KeyBind>().Active))
                {
                    return;
                };
                if ((Type == (int)BuffMngr.BuffTypes.BUFF_STUN || Type == (int)BuffMngr.BuffTypes.BUFF_TAUNT || Type == (int)BuffMngr.BuffTypes.BUFF_FEAR || Type == (int)BuffMngr.BuffTypes.BUFF_CHARM) && Config.Item("HardCC").GetValue<KeyBind>().Active)
                {
                    CleanseChecks(true, 0);
                };
                if (Type == (int)BuffMngr.BuffTypes.BUFF_SILENCE && Config.Item("silence").GetValue<KeyBind>().Active)
                {
                    CleanseChecks(false, 0);
                };
                if (Type == (int)BuffMngr.BuffTypes.BUFF_SUPPRESS && Config.Item("supress").GetValue<KeyBind>().Active)
                {
                    CleanseChecks(false, 0);
                };
                if (Type == (int)BuffMngr.BuffTypes.BUFF_DISARM && Config.Item("disarm").GetValue<KeyBind>().Active)
                {
                    CleanseChecks(false, 0);
                };
                if (Type == (int)BuffMngr.BuffTypes.BUFF_BLIND && Config.Item("blind").GetValue<KeyBind>().Active)
                {
                    CleanseChecks(false, 0);
                };
                if (Type == (int)BuffMngr.BuffTypes.BUFF_ROOT && Config.Item("root").GetValue<KeyBind>().Active)
                {
                    CleanseChecks(false, 0);
                };
                //if (args.BuffID == "???" && Config.Item("exhaust").GetValue<KeyBind>().Active)
                //{
                //    CleanseChecks(false, args.Duration);
                //};
            }
        }
        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            //Create the menu
            Config = new Menu("I Hate CC", "IHateCC", true);
            
            Config.AddSubMenu(new Menu("Types", "Types"));
            Config.SubMenu("Types").AddItem(new MenuItem("minimaltime", "Min CC time (10 = 1second)")).SetValue(new Slider(1, 20, 10));
            Config.SubMenu("Types").AddItem(new MenuItem("HardCC", "Clenase HARD CC").SetValue(true));
            Config.SubMenu("Types").AddItem(new MenuItem("silence", "Clenase Silence").SetValue(true));
            Config.SubMenu("Types").AddItem(new MenuItem("supress", "Clenase supress").SetValue(true));
            Config.SubMenu("Types").AddItem(new MenuItem("disarm", "Clenase disarm").SetValue(true));
            Config.SubMenu("Types").AddItem(new MenuItem("blind", "Clenase blind").SetValue(true));
            Config.SubMenu("Types").AddItem(new MenuItem("root", "Clenase root").SetValue(true));
            Config.SubMenu("Types").AddItem(new MenuItem("exhaust", "Clenase exhaust").SetValue(true));

            Config.AddSubMenu(new Menu("HotKey", "HotKey"));
            Config.SubMenu("HotKey")
                .AddItem(new MenuItem("ccactive", "Auto Cleanse").SetValue(new KeyBind(32, KeyBindType.Press)));
            Config.SubMenu("HotKey")
                .AddItem(
                    new MenuItem("ccactiveT", "Auto Cleanse (toggle)").SetValue(new KeyBind("C".ToCharArray()[0],
                        KeyBindType.Toggle)));
            Config.AddToMainMenu();
            Game.OnGameUpdate += Game_OnGameUpdate;
            itemslots.CleanseSlot = Utility.GetSpellSlot(Player, "SummonerBoost");
            if (Player.BaseSkinName == "Gankplank")
            {
                itemslots.spellslot = SpellSlot.W;
            };
            if (Player.BaseSkinName == "Olaf")
            {
                itemslots.spellslot = SpellSlot.R;
            };
        }

        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("ccactive").GetValue<KeyBind>().Active && Config.Item("ccactiveT").GetValue<KeyBind>().Active)
            {

                foreach (var buff in
                    ObjectManager.Player.Buffs.Where(
                        buff => Game.Time - buff.EndTime > Config.Item("minimaltime").GetValue<Slider>().Value/10))
                        {         Checks(Player,Player, (int)buff.Type);
                };
            };
        }

        private static void CleanseChecks(bool supress, float duration)
        {
            if (duration > Config.Item("minimaltime").GetValue<Slider>().Value/10 || duration == 0)
            {
                CleanseSLot();
                CastCleanse(false);
            }
        }
        private static void CastCleanse(bool supress)
        {
            if (supress && itemslots.QSSslot != 0)
            {
                Items.UseItem(itemslots.QSSslot);
            };
            if (!supress)
            {
                if (itemslots.QSSslot != 0 && Items.CanUseItem(itemslots.QSSslot))
                {
                    Items.UseItem(itemslots.QSSslot);
                    return;
                };
                if (itemslots.CleanseSlot != 0 && ObjectManager.Player.Spellbook.CanUseSpell(itemslots.CleanseSlot) != SpellState.Ready)
                {
                    ObjectManager.Player.Spellbook.CastSpell(itemslots.CleanseSlot);
                    return;
                };
                if (itemslots.spellslot != 0 && ObjectManager.Player.Spellbook.CanUseSpell(itemslots.spellslot) != SpellState.Ready)
                {
                    ObjectManager.Player.Spellbook.CastSpell(itemslots.spellslot);
                    return;
                };
            }
        }
        public class itemslots
        {
            public static int QSSslot;
            public static bool mikaelslot;
            public static SpellSlot CleanseSlot = 0;
            public static SpellSlot spellslot = 0;

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