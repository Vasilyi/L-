using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.UI;
using LeagueSharp.SDK.Enumerations;
using Color = System.Drawing.Color;
using LeagueSharp.SDK.Utils;

namespace Skillshots
{
    class Program
    {
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        static void Main(string[] args)
        {
            Bootstrap.Init(args);
            Events.OnLoad += Game_OnGameLoad;
        }
        private const string MenuName = "IHateSkillshots";
        public static Menu MainMenu { get; set; } = new Menu(MenuName, MenuName, true);
        public static MenuList<string> HitChanceList;
        public static Menu DrawMenu;



        private static void Game_OnGameLoad(object sender, EventArgs e)
        {
            try
            {
                MainMenu = new Menu("IHateSkillshots", "IHateSkillshots", true).Attach();
                HitChanceList = MainMenu.Add(new MenuList<string>("HitChance", "Hitchance", new[] { "Low", "Medium", "High", "VeryHigh" }));
                DrawMenu = MainMenu.Add(new Menu("Drawings", "Drawings Settings"));

                foreach (var spell in SpellDatabase.Spells)
                    if (spell.ChampionName == ObjectManager.Player.ChampionName)
                    {
                        Game.PrintChat(spell.Slot + " LOADED");
                        if (spell.Slot == SpellSlot.Q)
                        {
                            Q = new Spell(spell.Slot, spell.Range);
                            Q.SetSkillshot(spell.Delay / 1000, spell.Radius, spell.MissileSpeed, spell.CanBeRemoved, spell.Type);

                            MainMenu.Add(new MenuKeyBind("Spell1", "Use Q", System.Windows.Forms.Keys.Z, KeyBindType.Press));
                            DrawMenu.Add(new MenuBool("QRange", "Q range"));
                            DrawMenu.Add(new MenuColor("QRangeC", "Q range", SharpDX.Color.Aqua));
                            SpellList.Add(Q);
                        }
                        if (spell.Slot == SpellSlot.W)
                        {
                            W = new Spell(spell.Slot, spell.Range);
                            W.SetSkillshot(spell.Delay / 1000, spell.Radius, spell.MissileSpeed, spell.CanBeRemoved, spell.Type);
                            MainMenu.Add(new MenuKeyBind("Spell2", "Use W", System.Windows.Forms.Keys.Z, KeyBindType.Press));
                            DrawMenu.Add(new MenuBool("WRange", "W range"));
                            DrawMenu.Add(new MenuColor("WRangeC", "W range", SharpDX.Color.Black));
                            SpellList.Add(W);
                        }
                        if (spell.Slot == SpellSlot.E)
                        {
                            E = new Spell(spell.Slot, spell.Range);
                            E.SetSkillshot(spell.Delay / 1000, spell.Radius, spell.MissileSpeed, spell.CanBeRemoved, spell.Type);
                            MainMenu.Add(new MenuKeyBind("Spell3", "Use E", System.Windows.Forms.Keys.Z, KeyBindType.Press));
                            DrawMenu.Add(new MenuBool("ERange", "E range"));
                            DrawMenu.Add(new MenuColor("ERangeC", "E range", SharpDX.Color.Coral));
                            SpellList.Add(E);
                        }
                        if (spell.Slot == SpellSlot.R)
                        {
                            R = new Spell(spell.Slot, spell.Range);
                            R.SetSkillshot(spell.Delay / 1000, spell.Radius, spell.MissileSpeed, spell.CanBeRemoved, spell.Type);
                            MainMenu.Add(new MenuKeyBind("Spell4", "Use R", System.Windows.Forms.Keys.Z, KeyBindType.Press));
                            DrawMenu.Add(new MenuBool("RRange", "R range"));
                            DrawMenu.Add(new MenuColor("RRangeC", "R range", SharpDX.ColorBGRA.FromRgba(0)));
                            SpellList.Add(R);
                        }

                    }
            }
            catch (Exception)
            {
                Game.PrintChat("Error found in skillshots. Refused to load.");
            }
             Drawing.OnDraw += Drawing_OnDraw;
             Game.OnUpdate += Game_OnGameUpdate;
        }



        private static void Drawing_OnDraw(EventArgs args)
        {
            if (SpellList == null) return;

            foreach (Spell spell in SpellList)
            {

                var menuItem = DrawMenu[spell.Slot + "Range"].GetValue<MenuBool>();

                if (menuItem == true)
                {
                    var SpellColor = DrawMenu[spell.Slot + "RangeC"].GetValue<MenuColor>();
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spell.Range, SpellColor.Color.ToSystemColor());
                }
            }

        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            
            if (MainMenu["Spell1"] != null && MainMenu["Spell1"].GetValue<MenuKeyBind>().Active)
                ExecuteQ();
            if (MainMenu["Spell2"] != null && MainMenu["Spell2"].GetValue<MenuKeyBind>().Active)
                ExecuteW();
            if (MainMenu["Spell3"] != null && MainMenu["Spell3"].GetValue<MenuKeyBind>().Active)
                ExecuteE();
            if (MainMenu["Spell4"] != null && MainMenu["Spell4"].GetValue<MenuKeyBind>().Active)
                ExecuteR();
        }


        private static void ExecuteQ()
        {
            
            var target = Q.GetTarget();
            if (target == null) return;
            var rMode = MainMenu["HitChance"].GetValue<MenuList>().Index;
            Console.WriteLine(rMode);
            if (Q.IsReady())
            {
                switch (rMode)
                {
                    case 0://Low
                        Q.Cast();
                        break;
                    case 1://Medium
                        Q.CastIfHitchanceMinimum(target, HitChance.Medium);
                        break;
                    case 2://High
                      
                        Q.CastIfHitchanceMinimum(target, HitChance.High);
                        break;
                    case 3://Very High
                        Q.CastIfHitchanceMinimum(target, HitChance.VeryHigh);
                        break;
                }
            }


        }
        private static void ExecuteW()
        {
            var target = W.GetTarget();
            if (target == null) return;
            var rMode = MainMenu["HitChance"].GetValue<MenuList>().Index;
            Console.WriteLine(rMode);
            if (W.IsReady() && ObjectManager.Player.Distance(target.ServerPosition) <= W.Range)
            {
                switch (rMode)
                {
                    case 0://Low
                        W.Cast(target);
                        break;
                    case 1://Medium
                        W.CastIfHitchanceMinimum(target, HitChance.Medium);
                        break;
                    case 2://High
                        W.CastIfHitchanceMinimum(target, HitChance.High);
                        break;
                    case 3://Very High
                        W.CastIfHitchanceMinimum(target, HitChance.VeryHigh);
                        break;
                }
            }
        }
        private static void ExecuteE()
        {
            var target = E.GetTarget();
            if (target == null) return;
            var rMode = MainMenu["HitChance"].GetValue<MenuList>().Index;
            Console.WriteLine(rMode);
            if (E.IsReady() && ObjectManager.Player.Distance(target.ServerPosition) <= E.Range)
            {
                switch (rMode)
                {
                    case 0://Low
                        E.Cast(target);
                        break;
                    case 1://Medium
                        E.CastIfHitchanceMinimum(target, HitChance.Medium);
                        break;
                    case 2://High
                        E.CastIfHitchanceMinimum(target, HitChance.High);
                        break;
                    case 3://Very High
                        E.CastIfHitchanceMinimum(target, HitChance.VeryHigh);
                        break;
                }
            }
        }
        private static void ExecuteR()
        {
            var target = R.GetTarget();
            if (target == null) return;
            var rMode = MainMenu["HitChance"].GetValue<MenuList>().Index;
            Console.WriteLine(rMode);
            if (R.IsReady() && ObjectManager.Player.Distance(target.ServerPosition) <= R.Range)
            {
                switch (rMode)
                {
                    case 0://Low hitchance
                        R.Cast(target);
                        break;
                    case 1://Medium
                        R.CastIfHitchanceMinimum(target, HitChance.Medium);
                        break;
                    case 2://High
                        R.CastIfHitchanceMinimum(target, HitChance.High);
                        break;
                    case 3://Very High
                        R.CastIfHitchanceMinimum(target, HitChance.VeryHigh);
                        break;
                }
            }
        }
    }
}