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

namespace ResetsAllTheWay
{

    public class QMark
    {
        public string unit { get; private set; }

        public float endtime { get; private set; }

        public QMark(string Unit, float EndTime)
        {
            unit = Unit;
            endtime = EndTime;
        }
    }

    public class Program
    {
        public const string ChampionName = "Katarina";

        //Spells


        public static List<Spell> SpellList = new List<Spell>();
        public static List<QMark> MarkList = new List<QMark>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static SpellSlot IgniteSlot;
        
        //Menu
        public static Menu Config;
        private static Obj_AI_Hero Player;
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;

        }
        private static void BuffGained(Obj_AI_Base target, Obj_AI_Base source, BuffMngr.OnGainBuffArgs args)
        {
            if (target.IsMe && args.BuffID == 3334932)
            {
                tSpells.rEndTick = args.EndTime;
                tSpells.rStartTick = args.StartTime;
                tSpells.ulting = true;
            }
            else if (args.BuffID == 84848667) //mark
            {
                MarkList.Add(new QMark(target.BaseSkinName, args.EndTime));
            }
        }

        private static void BuffLost(Obj_AI_Base target, Obj_AI_Base source, BuffMngr.OnGainBuffArgs args)
        {
            if (target.IsMe && args.BuffID == 3334932)
            {
                tSpells.ulting = false;
            }
            else if (args.BuffID == 84848667) // mark
            {
                foreach (var mark in MarkList)
                {
                    if (mark.unit == target.BaseSkinName)
                    {
                        MarkList.Remove(mark);
                    }
                }
            }
        }
        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.BaseSkinName != ChampionName) return;
            Console.WriteLine("ResetsAllTheWay loaded");
            BuffMngr.BuffMngrInit();
            BuffMngr.OnGainBuff += BuffGained;
            BuffMngr.OnLoseBuff += BuffLost;

            //Create the spells
            Q = new Spell(SpellSlot.Q, 675);
            W = new Spell(SpellSlot.W, 400);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 550);
            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            //Create the menu
            Config = new Menu(ChampionName, ChampionName, true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);



            Config.AddSubMenu(new Menu("HotKeys:", "hotkeys"));
            Config.SubMenu("hotkeys").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
            Config.SubMenu("hotkeys").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Harass Options:", "harassspells"));
            Config.SubMenu("harassspells").AddItem(new MenuItem("useQHarass", "Use - Bouncing Blades (Q)").SetValue(true));
            Config.SubMenu("harassspells").AddItem(new MenuItem("useWHarass", "Use - Sinister Steel (W)").SetValue(true));
            Config.SubMenu("harassspells").AddItem(new MenuItem("useEHarass", "Use - Shunpo (E)").SetValue(true));

            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Draw damage after Combo").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit += hero => (float)CalculateDamageDrawing(hero);

            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };
            Config.SubMenu("Drawings").AddItem(dmgAfterComboItem);
            Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));

            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("ignite", "Use Ignite")).SetValue(true);
            Config.SubMenu("Misc").AddItem(new MenuItem("dfg", "Use DFG")).SetValue(true);
            Config.SubMenu("Misc").AddItem(new MenuItem("wDelay", "Delay W to proc mark")).SetValue(false);

            Config.AddToMainMenu();

            //Add the events we are going to use:
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.OnGameSendPacket += GameOnOnGameSendPacket;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            //Draw the ranges of the spells.
            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                {
                    Utility.DrawCircle(Player.Position, spell.Range, menuItem.Color);
                }
            }
        }
        public static class tSpells
        {
            public static float rEndTick;
            public static float rStartTick;
            public static bool ulting;
            public static float wLastUse;
            public static float qlastuse;
            public static bool usedfg;
            public static bool useignite;

        }

        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            };
            if (Config.Item("HarassActive").GetValue<KeyBind>().Active)
            {
                Harass(Config.Item("useQHarass").GetValue<bool>(), Config.Item("useWHarass").GetValue<bool>(), Config.Item("useEHarass").GetValue<bool>());
            };
            if (Config.Item("StackE").GetValue<KeyBind>().Active)
            {
                if (ObjectManager.Player.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.Ready)
                {
                }
            }

        }

        private static void Combo()
        {
            var qtarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var wtarget = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Magical);
            var etarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            var rtarget = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
            var curtarget = SimpleTs.GetTarget(400, SimpleTs.DamageType.Magical);;
            if (E.IsReady())
                curtarget = etarget;
            else if (Q.IsReady())
                curtarget = qtarget;
            else if (W.IsReady())
                curtarget = wtarget;
            else if (R.IsReady())
                curtarget = rtarget;
            Console.WriteLine(CalculateDamage(curtarget).ToString());

            if (((CalculateDamage(curtarget)>curtarget.Health || rtarget == null) && tSpells.ulting == true) || tSpells.ulting == false)
            {
                DoCombo(curtarget);
            }
        }


        private static void GameOnOnGameSendPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] == Packet.C2S.Move.Header && Environment.TickCount < tSpells.rStartTick + 100)
            {
                args.Process = false;
            }
        }

        private static void DoCombo(Obj_AI_Base target)
        {
            if (Config.Item("dfg").GetValue<bool>() && tSpells.usedfg && Items.HasItem(3128) && Items.CanUseItem(3128))
            {
                Items.UseItem(3128, target);
            }
            if (Q.IsReady() && ObjectManager.Player.Distance(target) < Q.Range)
            {
                Q.Cast(target, false);
                tSpells.qlastuse = Environment.TickCount;
            }
            if (E.IsReady() && ObjectManager.Player.Distance(target) < E.Range)
            {
                E.Cast(target);
            }
            if (W.IsReady() && ObjectManager.Player.Distance(target) < W.Range && Environment.TickCount > tSpells.wLastUse + 50 && (!Config.Item("wDelay").GetValue<bool>() || checkformark(target) || Environment.TickCount > tSpells.qlastuse + 100 || R.IsReady()))
            {
                W.Cast();
                tSpells.wLastUse = Environment.TickCount;
            }
            if (R.IsReady() && ObjectManager.Player.Distance(target) < R.Range && !tSpells.ulting && Environment.TickCount > tSpells.rStartTick + 100)
            {
                ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, new Vector3(Player.ServerPosition.X, Player.ServerPosition.Y, Player.ServerPosition.Z));
                R.Cast();
                tSpells.rStartTick = Environment.TickCount;
            }
            if (Config.Item("ignite").GetValue<bool>() && tSpells.useignite)
            {
                ObjectManager.Player.SummonerSpellbook.CastSpell(IgniteSlot, target);
            }
        }

        private static bool checkformark(Obj_AI_Base target)
        {
            foreach (QMark mark in MarkList)
            {
                if (mark.unit == target.BaseSkinName)
                {
                    Console.WriteLine(mark.unit + " : marked");
                    return true;
                    
                }
            }
            //Console.WriteLine(target.BaseSkinName + " : not marked");
            return false;
        }

        public static double CalculateDamage(Obj_AI_Base target)
        {
            double totaldamage = 0;
            bool marked = checkformark(target);
            tSpells.useignite = false;
            tSpells.usedfg = false;
            if ((ObjectManager.Player.Distance(target) < Q.Range || ObjectManager.Player.Distance(target) < E.Range && E.IsReady()) && Q.IsReady() && (W.IsReady() || E.IsReady() || R.IsReady()))
            {
                totaldamage += DamageLib.getDmg(target, DamageLib.SpellType.Q);
            }
            if ((ObjectManager.Player.Distance(target) < W.Range || ObjectManager.Player.Distance(target) < E.Range && E.IsReady()) && W.IsReady())
            {
                totaldamage += DamageLib.getDmg(target, DamageLib.SpellType.W);
            }
            if (ObjectManager.Player.Distance(target) < E.Range && E.IsReady())
            {
                totaldamage += DamageLib.getDmg(target, DamageLib.SpellType.E);
            }
            if ((ObjectManager.Player.Distance(target) < R.Range || ObjectManager.Player.Distance(target) < E.Range && E.IsReady()) && R.IsReady())
            {
                totaldamage += DamageLib.getDmg(target, DamageLib.SpellType.R);
            }
            if (!Q.IsReady() && marked)
            {
                totaldamage += DamageLib.CalcMagicDmg(((ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level * 15)) + (0.15 * ObjectManager.Player.FlatMagicDamageMod), target);
            }

            if (totaldamage > target.Health)
            {
               return totaldamage;
            }

            if (Config.Item("dfg").GetValue<bool>() && Items.HasItem(3128) && Items.CanUseItem(3128))
            {
                totaldamage = (totaldamage * 1.2) + DamageLib.CalcMagicDmg(target.MaxHealth * 0.15,target);
            }

            if (totaldamage > target.Health)
            {
                tSpells.usedfg = true;
                return totaldamage;
            }
            
            if (Config.Item("ignite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown && ObjectManager.Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && ObjectManager.Player.Distance(target) < 600)
            {
                
                if (totaldamage + DamageLib.getDmg(target, DamageLib.SpellType.IGNITE) > target.Health)
                {
                    tSpells.useignite = true;
                    totaldamage += DamageLib.getDmg(target, DamageLib.SpellType.IGNITE);
                }
            }
            tSpells.usedfg = true;
            
            return totaldamage;
        }


        public static double CalculateDamageDrawing(Obj_AI_Base target)
        {
            double totaldamage = 0;
            bool marked = checkformark(target);
            if (Q.IsReady() && (W.IsReady() || E.IsReady() || R.IsReady()))
            {
                totaldamage += DamageLib.getDmg(target, DamageLib.SpellType.Q);
            }
            if (E.IsReady() && W.IsReady())
            {
                totaldamage += DamageLib.getDmg(target, DamageLib.SpellType.W);
            }
            if (E.IsReady())
            {
                totaldamage += DamageLib.getDmg(target, DamageLib.SpellType.E);
            }
            if (R.IsReady())
            {
                totaldamage += DamageLib.getDmg(target, DamageLib.SpellType.R);
            }
            if (!Q.IsReady() && marked)
            {
                totaldamage += DamageLib.CalcMagicDmg(((ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Level * 15)) + (0.15 * ObjectManager.Player.FlatMagicDamageMod), target);
            }

            if (Config.Item("dfg").GetValue<bool>() && Items.HasItem(3128) && Items.CanUseItem(3128))
            {
                totaldamage = (totaldamage * 1.2) + DamageLib.CalcMagicDmg(target.MaxHealth * 0.15, target);
            }

            if (Config.Item("ignite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown && ObjectManager.Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                totaldamage += DamageLib.getDmg(target, DamageLib.SpellType.IGNITE);
            }
            return totaldamage;
        }
        private static void Harass(bool useQ, bool useW, bool useE)
        {
            var qtarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var wtarget = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Magical);
            var etarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            if (qtarget != null && useQ && Q.IsReady())
            {
                Q.Cast(qtarget, false);
                tSpells.qlastuse = Environment.TickCount;
            }
            else if (wtarget != null && useW && W.IsReady() && (!Config.Item("wDelay").GetValue<bool>() || checkformark(wtarget)))
            {
                W.Cast();
                tSpells.wLastUse = Environment.TickCount;
            }
            else if (etarget != null && useE && E.IsReady())
                E.Cast(etarget, false);
        }
        
    }
}
