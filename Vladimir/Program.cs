#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
//using BuffLib;
#endregion

namespace Vladimir
{
    public class Program
    {
        public const string ChampionName = "Vladimir";

        //Spells
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell E;
        public static Spell R;
        public float lastE = 0f;
        public static Orbwalking.Orbwalker Orbwalker;
        //Menu
        public static Menu Config;
        private static Obj_AI_Hero Player;
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Console.WriteLine("MAIN LOADED");
            //BuffMngr.BuffMngrInit();
            //BuffMngr.OnGainBuff += BuffGained;
            //BuffMngr.OnLoseBuff += BuffLost;
            //BuffMngr.OnUpdateBuff += BuffUpdated;
        }


        private static void checkforzed()
        {
           foreach (var hero in ObjectManager.Get<Obj_AI_Hero>())
           {
               if (hero.Name == "Zed")
               {
                   Config.SubMenu("Misc").AddItem(new MenuItem("antized", "Put your shit on Zed").SetValue(true));
               }
           }
        }
        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            if (Player.ChampionName != ChampionName) return;

            //Create the spells
            Q = new Spell(SpellSlot.Q, 600);
            E = new Spell(SpellSlot.E, 610);
            R = new Spell(SpellSlot.R, 700);

            SpellList.Add(Q);
            SpellList.Add(E);
            SpellList.Add(R);

            R.SetSkillshot(0.25f, 175, 700, false, SkillshotType.SkillshotCircle);

            //Create the menu
            Config = new Menu(ChampionName, ChampionName, true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);
            //Orbwalker submenu
            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));

            //Load the orbwalker and add it to the submenu.
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));


            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo")
                .AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("QRange", "Q range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("ERange", "E range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings")
    .AddItem(
        new MenuItem("RRange", "R range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc")
                .AddItem(
                    new MenuItem("StackE", "StackE (toggle)!").SetValue(new KeyBind("C".ToCharArray()[0],
                        KeyBindType.Toggle)));
            //Config.SubMenu("Misc").AddItem(new MenuItem("StackE", "Auto stack E").SetValue(new KeyBind(Config.Item("StackE").GetValue<KeyBind>().Key, KeyBindType.Toggle)));
            checkforzed();
            Config.AddToMainMenu();

            //Add the events we are going to use:
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
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
        public static class ECharges
        {
            public static int lastE;
            public static int timeleft;
        }
        public static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs attack)
        {
            if (Config.Item("antized") != null && Config.Item("antized").GetValue<bool>() && attack.Target.IsMe)
            {
                if (unit.Name == "Zed" && attack.SData.Name == "zedult")
                    ObjectManager.Player.Spellbook.CastSpell(SpellSlot.W);
                    Console.WriteLine("Zed feck u:)");
            };
            if (unit.IsMe && attack.SData.Name == "VladimirTidesofBlood")
            {
                ECharges.lastE = Environment.TickCount - 250;
            }
        }
        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            };
            if (Config.Item("HarassActive").GetValue<KeyBind>().Active)
            {
                    Harass();
            };
            if (Config.Item("StackE").GetValue<KeyBind>().Active)
            {
                if (ObjectManager.Player.Spellbook.CanUseSpell(SpellSlot.E) == SpellState.Ready)
                {
                    int eTimeLeft = Environment.TickCount - ECharges.lastE;
                   // Console.WriteLine(ECharges.lastE.ToString() + " CURRENT TICK : " + Environment.TickCount.ToString() + " TIMELEFT : " + eTimeLeft.ToString());
                    if ((eTimeLeft >= 9900) && E.IsReady())
                    {
                        
                        E.Cast();
                    }
                }
            }

        }
        private static float TotalDmg(Obj_AI_Base enemy, bool useQ, bool useE, bool useR)
        {
            var damage = 0d;
            var estacks = Player.GetBuffCount("vladimirtidesofbloodcost");
            //Base Q damage
            if (useQ && Q.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);
            }

            //E damage
            if (useE && E.IsReady())
            {
                var edmg =  new double[] { 60, 85, 110, 135, 160 }[Player.Spellbook.GetSpell(SpellSlot.E).Level];
                edmg = edmg * (1 + 0.25 * estacks);
                edmg = edmg + 0.45 * Player.FlatMagicDamageMod;
                damage += edmg;
            }

            //R damage
            if (useR && R.IsReady())
            {
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);
                damage += TotalDmg(enemy, true, true, false) * 0.12;
            }

            // Ludens Echo damage
            if (Items.HasItem(3285))
                damage += Player.CalcDamage(enemy, Damage.DamageType.Magical, 100 + Player.FlatMagicDamageMod * 0.1);
            return (float)damage;
        }
        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (target != null)
            {
                if (Player.Distance(target.ServerPosition) <= R.Range + R.Width && R.IsReady() && TotalDmg(target, true, true, false) < target.Health)
                    R.Cast(target, true, true);

                if (Player.Distance(target.ServerPosition) <= Q.Range && Q.IsReady())
                    Q.Cast(target);
                if (Player.Distance(target.ServerPosition) <= E.Range && E.IsReady() && !Player.Spellbook.IsCastingSpell)
                    E.Cast();

            }
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (target != null)
            {
                if (Player.Distance(target.ServerPosition) <= Q.Range && Q.IsReady())
                    Q.Cast(target, false);
                if (Player.Distance(target.ServerPosition) <= E.Range && E.IsReady() && !Player.Spellbook.IsCastingSpell)
                    E.Cast();
            }
        }


    }
}