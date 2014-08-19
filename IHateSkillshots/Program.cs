using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace Skillshots
{
    class Program
    {
        public static List<Spell> SpellList = new List<Spell>();
        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Menu Config;
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            try
            {
                Config = new Menu("Skillshots", "Skillshots", true);
                Config.AddSubMenu(new Menu("Combo", "Combo"));
                Config.SubMenu("Combo").AddItem(new MenuItem("Hitchance", "Only High HitChance").SetValue(true));
                Config.AddSubMenu(new Menu("Drawings", "Drawings"));
                var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
                SimpleTs.AddToMenu(targetSelectorMenu);
                Config.AddSubMenu(targetSelectorMenu);
                foreach (var spell in SpellDatabase.Spells)
                    if (spell.BaseSkinName == ObjectManager.Player.BaseSkinName)
                    {
                        Game.PrintChat(spell.Slot + " LOADED");
                        if (spell.Slot == SpellSlot.Q)
                        {
                        Q = new Spell(spell.Slot, spell.Range);
                        Q.SetSkillshot(spell.Delay/1000, spell.Radius, spell.MissileSpeed, spell.CanBeRemoved, spell.Type);
                        Config.SubMenu("Combo").AddItem(new MenuItem("Spell1", "Q").SetValue(new KeyBind(90, KeyBindType.Press)));
                        Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
                        SpellList.Add(Q);
                        }
                        if (spell.Slot == SpellSlot.W)
                        {
                            W = new Spell(spell.Slot, spell.Range);
                            W.SetSkillshot(spell.Delay / 1000, spell.Radius, spell.MissileSpeed, spell.CanBeRemoved, spell.Type);
                            Config.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W range").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
                            Config.SubMenu("Combo").AddItem(new MenuItem("Spell2", "W").SetValue(new KeyBind(88, KeyBindType.Press)));
                            SpellList.Add(W);
                        }
                        if (spell.Slot == SpellSlot.E)
                        {
                            E = new Spell(spell.Slot, spell.Range);
                            E.SetSkillshot(spell.Delay / 1000, spell.Radius, spell.MissileSpeed, spell.CanBeRemoved, spell.Type);
                            Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E range").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
                            Config.SubMenu("Combo").AddItem(new MenuItem("Spell3", "E").SetValue(new KeyBind(67, KeyBindType.Press)));
                            SpellList.Add(E);
                        }
                        if (spell.Slot == SpellSlot.R)
                        {
                            R = new Spell(spell.Slot, spell.Range);
                            R.SetSkillshot(spell.Delay / 1000, spell.Radius, spell.MissileSpeed, spell.CanBeRemoved, spell.Type);
                            Config.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R range").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
                            Config.SubMenu("Combo").AddItem(new MenuItem("Spell4", "R").SetValue(new KeyBind(86, KeyBindType.Press)));
                            SpellList.Add(R);
                        }

                    }
            }
            catch (Exception)
            {
                Game.PrintChat("Error found in skillshots. Refused to load.");
            }
            Config.AddToMainMenu();

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (SpellList == null) return;

            foreach (Spell spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();

                if (menuItem.Active)
                    Utility.DrawCircle(ObjectManager.Player.Position, spell.Range, menuItem.Color);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("Spell1").GetValue<KeyBind>().Active)
                ExecuteQ();
            if (Config.Item("Spell2").GetValue<KeyBind>().Active)
                ExecuteW();
            if (Config.Item("Spell3").GetValue<KeyBind>().Active)
                ExecuteE();
            if (Config.Item("Spell4").GetValue<KeyBind>().Active)
                ExecuteR();
        }


        private static void ExecuteQ()
        {
            Obj_AI_Hero target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            if (target == null) return;
            if (Q.IsReady() && ObjectManager.Player.Distance(target) <= Q.Range)
                if (!Config.Item("Hitchance").GetValue<bool>())
                    Q.Cast(target, false);
                if (Config.Item("Hitchance").GetValue<bool>())
                    Q.CastIfHitchanceEquals(target, Prediction.HitChance.HighHitchance, false);
                //Q.Cast(target, false);
 
        }
        private static void ExecuteW()
        {
            Obj_AI_Hero target = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Magical);
            if (target == null) return;
            if (W.IsReady() && ObjectManager.Player.Distance(target) <= W.Range)
                if (!Config.Item("Hitchance").GetValue<bool>())
                    W.Cast(target, false);
                if (Config.Item("Hitchance").GetValue<bool>())
                    W.CastIfHitchanceEquals(target, Prediction.HitChance.HighHitchance, false);
        }
        private static void ExecuteE()
        {
            Obj_AI_Hero target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            if (target == null) return;
            if (E.IsReady() && ObjectManager.Player.Distance(target) <= E.Range)
                if (!Config.Item("Hitchance").GetValue<bool>())
                    E.Cast(target, false);
                if (Config.Item("Hitchance").GetValue<bool>())
                    E.CastIfHitchanceEquals(target, Prediction.HitChance.HighHitchance, false);
        }
        private static void ExecuteR()
        {
            Obj_AI_Hero target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            if (target == null) return;
            if (R.IsReady() && ObjectManager.Player.Distance(target) <= R.Range)
                if (!Config.Item("Hitchance").GetValue<bool>())
                    R.Cast(target, false);
                if (Config.Item("Hitchance").GetValue<bool>())
                    R.CastIfHitchanceEquals(target, Prediction.HitChance.HighHitchance, false);
        }
    }
}
