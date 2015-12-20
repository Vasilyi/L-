#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
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
        public static Orbwalking.Orbwalker Orbwalker;
        //Menu
        public static Menu Config;
        private static Obj_AI_Hero Player;
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Obj_AI_Hero.OnIssueOrder += ObjAiHeroOnOnIssueOrder;

        }

        private static void ObjAiHeroOnOnIssueOrder(Obj_AI_Base sender, GameObjectIssueOrderEventArgs args)
        {
            if (sender.IsMe && Utils.TickCount < tSpells.rStartTick + 300)
            {
                //Console.WriteLine("BLOCKED");
                args.Process = false;
            }
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.BaseSkinName != ChampionName) return;
            Console.WriteLine("ResetsAllTheWay loaded");

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
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);
            //Orbwalker submenu
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));

            //Load the orbwalker and add it to the submenu.
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));


            Config.AddSubMenu(new Menu("HotKeys:", "hotkeys"));
            Config.SubMenu("hotkeys").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
            Config.SubMenu("hotkeys").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Combo Options:", "combooptions"));
            Config.SubMenu("combooptions").AddItem(new MenuItem("useR", "Use - Death Lotus (R)").SetValue(true));
            Config.SubMenu("combooptions").AddItem(new MenuItem("Epriority", "Use - (E) before (Q)").SetValue(true));

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
            Game.OnUpdate += Game_OnGameUpdate;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            //Draw the ranges of the spells.
            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                {
                    Render.Circle.DrawCircle(Player.Position, spell.Range, menuItem.Color);
                }
            }
        }
        public static class tSpells
        {
            public static float rEndTick;
            public static float rStartTick = 0;
            public static bool ulting = false;
            public static float wLastUse;
            public static float qlastuse;
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

        }

        private static void Combo()
        {
            var qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var wtarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            var etarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            var rtarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            var curtarget = TargetSelector.GetTarget(400, TargetSelector.DamageType.Magical);;
            if (E.IsReady())
                curtarget = etarget;
            else if (Q.IsReady())
                curtarget = qtarget;
            else if (W.IsReady())
                curtarget = wtarget;
            else if (R.IsReady())
                curtarget = rtarget;
            //Console.WriteLine(CalculateDamage(curtarget).ToString());

            if (((CalculateDamage(curtarget)>curtarget.Health || rtarget == null) && tSpells.ulting == true) || tSpells.ulting == false)
            {
                DoCombo(curtarget);
            }
        }

        private static void PlayAnimation(GameObject sender, GameObjectPlayAnimationEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.Animation == "Spell4")
                {
                    tSpells.ulting = true;
                }
                else
                {
                    tSpells.ulting = false;
                }
            }
        }

        private static void DoCombo(Obj_AI_Base target)
        {
            if (Q.IsReady() && (Config.Item("Epriority").GetValue<bool>() == false || !E.IsReady() || ObjectManager.Player.Distance(target.ServerPosition) > E.Range) && ObjectManager.Player.Distance(target.ServerPosition) < Q.Range)
            {
                Q.Cast(target, false);
                tSpells.qlastuse = Environment.TickCount;
            }
            if (E.IsReady() && ObjectManager.Player.Distance(target.ServerPosition) < E.Range)
            {
                E.Cast(target);
            }
            if (W.IsReady() && !Q.IsReady() && ObjectManager.Player.Distance(target.ServerPosition) < W.Range && Environment.TickCount > tSpells.wLastUse + 250 && (!Config.Item("wDelay").GetValue<bool>() || checkformark(target) || Environment.TickCount > tSpells.qlastuse + 100 || R.IsReady()))
            {
                W.Cast();
                tSpells.wLastUse = Environment.TickCount;
                //Console.WriteLine("CAST W");
            }
            if (R.IsReady() && Config.Item("useR").GetValue<bool>() && !W.IsReady() && ObjectManager.Player.Distance(target.ServerPosition) < R.Range && !tSpells.ulting && Environment.TickCount > tSpells.rStartTick + 300)
            {
                ObjectManager.Player.IssueOrder(GameObjectOrder.HoldPosition, new Vector3(Player.ServerPosition.X, Player.ServerPosition.Y, Player.ServerPosition.Z));
                R.Cast();
                tSpells.rStartTick = Utils.TickCount;
                Console.WriteLine("CAST ULT");
            }
            if (Config.Item("ignite").GetValue<bool>() && tSpells.useignite)
            {
                ObjectManager.Player.Spellbook.CastSpell(IgniteSlot, target);
            }
        }

        private static bool checkformark(Obj_AI_Base target)
        {
            if (target.Buffs.Any(buff => buff.Name.ToLower().Contains("katarinaqmark")))
                   {
                       
                       return true;
                       
                   }
            //Console.WriteLine(target.BaseSkinName + " : not marked");
            return false;
        }

        public static double CalculateDamage(Obj_AI_Base target)
        {
            double totaldamage = 0;
            bool marked = checkformark(target);
            tSpells.useignite = false;
            if ((ObjectManager.Player.Distance(target.ServerPosition) < Q.Range || ObjectManager.Player.Distance(target.ServerPosition) < E.Range && E.IsReady()) && Q.IsReady() && (W.IsReady() || E.IsReady() || R.IsReady()))
            {
                totaldamage += Player.GetSpellDamage(target, SpellSlot.Q);
            }
            if ((ObjectManager.Player.Distance(target.ServerPosition) < W.Range || ObjectManager.Player.Distance(target.ServerPosition) < E.Range && E.IsReady()) && W.IsReady())
            {
                totaldamage += Player.GetSpellDamage(target,SpellSlot.W);
            }
            if (ObjectManager.Player.Distance(target.ServerPosition) < E.Range && E.IsReady())
            {
                totaldamage += Player.GetSpellDamage(target, SpellSlot.E);
            }
            if ((ObjectManager.Player.Distance(target.ServerPosition) < R.Range || ObjectManager.Player.Distance(target.ServerPosition) < E.Range && E.IsReady()) && R.IsReady())
            {
                totaldamage += Player.GetSpellDamage(target, SpellSlot.R)*3;
            }
            if (!Q.IsReady() && marked)
            {
                totaldamage += Player.GetSpellDamage(target, SpellSlot.Q, 1);
            }

            if (totaldamage > target.Health)
            {
               return totaldamage;
            }
            
            if (Config.Item("ignite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown && ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready && ObjectManager.Player.Distance(target.ServerPosition) < 600)
            {
                
                if (totaldamage + Player.GetSummonerSpellDamage(target,Damage.SummonerSpell.Ignite) > target.Health)
                {
                    tSpells.useignite = true;
                    totaldamage += Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
                }
            }

            
            return totaldamage;
        }


        public static double CalculateDamageDrawing(Obj_AI_Base target)
        {
            double totaldamage = 0;
            bool marked = checkformark(target);
            if (Q.IsReady())
            {
                totaldamage += Player.GetSpellDamage(target, SpellSlot.Q);
                totaldamage += Player.GetSpellDamage(target, SpellSlot.Q, 1);
            }
            if (E.IsReady() && W.IsReady())
            {
                totaldamage += Player.GetSpellDamage(target, SpellSlot.W);
            }
            if (E.IsReady())
            {
                totaldamage += Player.GetSpellDamage(target, SpellSlot.E);
            }
            if (R.IsReady())
            {
                totaldamage += Player.GetSpellDamage(target, SpellSlot.R) * 3;
            }


            if (Config.Item("ignite").GetValue<bool>() && IgniteSlot != SpellSlot.Unknown && ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready)
            {
                totaldamage += Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite);
            }
            return totaldamage;
        }
        private static void Harass(bool useQ, bool useW, bool useE)
        {
            var qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var wtarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            var etarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
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
