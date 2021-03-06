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
            public static float lastcleanse = 0;

        }
        public static Menu Config;
        private static Obj_AI_Hero Player;
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Console.WriteLine("I HATE CC LOADED");
        }


        private static void Checks(BuffType Type)
        {
            
          //  Console.WriteLine(Type);
                if ((Config.Item("ccactive").GetValue<KeyBind>().Active || Config.Item("ccactiveT").GetValue<bool>()))
                {
                    if ((Type == BuffType.Stun || Type == BuffType.Taunt || Type == BuffType.Fear || Type == BuffType.Charm) && Config.Item("HardCC").GetValue<bool>())
                    {
                        //Console.WriteLine("CC ACTIVET : " + Config.Item("ccactiveT").GetValue<KeyBind>().Active + " CC ACTIVE : " + Config.Item("ccactive").GetValue<KeyBind>().Active + " TARGET ME : ");
                        CleanseChecks(false);
                    }
                    if (Type == BuffType.Silence && Config.Item("silence").GetValue<bool>())
                    {
                        CleanseChecks(false);
                    }
                    if (Type == BuffType.Suppression && Config.Item("supress").GetValue<bool>())
                    {
                        CleanseChecks(true);
                    }
                    if (Type == BuffType.Disarm && Config.Item("disarm").GetValue<bool>())
                    {
                        CleanseChecks(false);
                    }
                    if (Type == BuffType.Blind && Config.Item("blind").GetValue<bool>())
                    {
                        CleanseChecks(false);
                    }
                    if ((Type == BuffType.Snare || Type == BuffType.Snare) && Config.Item("root").GetValue<bool>())
                    {
                        CleanseChecks(false);
                    }

                    //if (args.BuffID == "???" && Config.Item("exhaust").GetValue<KeyBind>().Active)
                    //{
                    //    CleanseChecks(false, args.Duration);
                    //};
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
            Config.SubMenu("Types").AddItem(new MenuItem("delaycleanse", "Cleanse delay (10 = 1second)")).SetValue(new Slider(0, 20, 0));
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
            if (Player.ChampionName == "Gangplank")
            {
                itemslots.spellslot = SpellSlot.W;
                Console.WriteLine("Found Gankplank");
            };
        }

        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("ccactive").GetValue<KeyBind>().Active || Config.Item("ccactiveT").GetValue<KeyBind>().Active)
            {
        
                foreach (var buff in ObjectManager.Player.Buffs.Where(b => b.IsValidBuff() && b.Caster.IsValid))
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

                        if (buff.Type == BuffType.Stun
                    || buff.Type == BuffType.Taunt ||
                    buff.Type == BuffType.Fear ||
                    buff.Type == BuffType.Charm ||
                    buff.Type == BuffType.Silence ||
                    buff.Type == BuffType.Suppression ||
                    buff.Type == BuffType.Disarm)
                        {
                            //Console.WriteLine(buff.Name + " : " + buffend + " : " + Config.Item("minimaltime").GetValue<Slider>().Value / 10);
                            Checks(buff.Type);
                        }
                    }
                };
            };
        }


        private static void CleanseChecks(bool supress)
        {
            CleanseSLot();
            try
            {
                var delaycleanse = Config.Item("minimaltime").GetValue<Slider>().Value / 10;
                if (itemslots.lastcleanse + 100 + delaycleanse > Environment.TickCount)
                {
                    return;
                }
             
                if (supress && itemslots.QSSslot != 0 && Items.CanUseItem(itemslots.QSSslot))
                {
                    Console.WriteLine("Supress " + itemslots.QSSslot + " : " + itemslots.CleanseSlot + " : " + itemslots.spellslot);
                    //Items.UseItem(itemslots.QSSslot);
                    Utility.DelayAction.Add(delaycleanse, delegate {Items.UseItem(itemslots.QSSslot);});
                }
                if (!supress)
                {
                    Console.WriteLine("Not supress " + itemslots.QSSslot + " : " + itemslots.CleanseSlot + " : " + itemslots.spellslot);
                    if (itemslots.QSSslot != 0 && Items.CanUseItem(itemslots.QSSslot))
                    {
                        Console.WriteLine("Found QSS");
                        //Items.UseItem(itemslots.QSSslot);
                        Utility.DelayAction.Add(delaycleanse, delegate { Items.UseItem(itemslots.QSSslot); });
                        itemslots.lastcleanse = Environment.TickCount;
                    }
                    else if (ObjectManager.Player.Spellbook.CanUseSpell(itemslots.CleanseSlot) == SpellState.Ready)
                    {
                        //ObjectManager.Player.Spellbook.CastSpell(itemslots.CleanseSlot);
                        Utility.DelayAction.Add(delaycleanse, delegate { ObjectManager.Player.Spellbook.CastSpell(itemslots.CleanseSlot); });
                        Console.WriteLine("Found Summoner");
                        itemslots.lastcleanse = Environment.TickCount;
                    }
                    else if (itemslots.spellslot != 0 && ObjectManager.Player.Spellbook.CanUseSpell(itemslots.spellslot) == SpellState.Ready)
                    {
                        Console.WriteLine("Found Spell");
                        //ObjectManager.Player.Spellbook.CastSpell(itemslots.spellslot);
                        Utility.DelayAction.Add(delaycleanse, delegate { ObjectManager.Player.Spellbook.CastSpell(itemslots.spellslot); });
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
