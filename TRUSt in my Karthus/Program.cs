#region
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
#endregion

namespace TRUStinmyKarthus
{
    public class Program
    {

        public static Menu Config;
        private static Obj_AI_Hero Player;

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static SpellSlot IgniteSlot;
        public class Spells
        {
            public static float qRange = 875f;
            public static float wRange = 1000f;
            public static float eRange = 425f;
        }

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != "Karthus")
            {
                return;
            }
            Console.WriteLine("TRUStInMyKarthus LOADED");
            Game.OnUpdate += Game_OnGameUpdate;
var targetSelectorMenu = new Menu("Target Selector", "TargetSelector");
            TargetSelector.AddToMenu(targetSelectorMenu);
           
            Player = ObjectManager.Player;
            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
            Q = new Spell(SpellSlot.Q, Spells.qRange);
            W = new Spell(SpellSlot.W, Spells.wRange);
            E = new Spell(SpellSlot.E, Spells.eRange);
            R = new Spell(SpellSlot.R, 99999);

            Q.SetSkillshot(1f, 150f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            W.SetSkillshot(0.25f, 0f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            try
            {
                //Create the menu
                Config = new Menu("TRUStInMyKarthus", "mainmenu", true);
 Config.AddSubMenu(targetSelectorMenu);
                Config.AddSubMenu(new Menu("HotKeys:", "hotkeys"));
                Config.SubMenu("hotkeys").AddItem(new MenuItem("ComboKey", "Combo!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                Config.SubMenu("hotkeys").AddItem(new MenuItem("HarassKey", "Harrass").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                Config.SubMenu("hotkeys").AddItem(new MenuItem("FarmKeyFreeze", "LastHit with Q").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Press)));
                Config.SubMenu("hotkeys").AddItem(new MenuItem("LaneClear", "Lane Clear with Q").SetValue(new KeyBind("H".ToCharArray()[0], KeyBindType.Press)));

                Config.AddSubMenu(new Menu("Ultimate notify Logic:", "ultlogic"));
                Config.SubMenu("ultlogic").AddItem(new MenuItem("useRauto", "Ping on killable").SetValue(true));

                Config.AddSubMenu(new Menu("Combo Options:", "combospells"));
                Config.SubMenu("combospells").AddItem(new MenuItem("UseI", "Use Ignite if enemy is killable").SetValue(true));
                Config.SubMenu("combospells").AddItem(new MenuItem("dfg", "Use DFG in full combo").SetValue(true));
                Config.SubMenu("combospells").AddItem(new MenuItem("useQ", "Use - Q").SetValue(true));
                Config.SubMenu("combospells").AddItem(new MenuItem("useW", "Use - W").SetValue(true));
                Config.SubMenu("combospells").AddItem(new MenuItem("useE", "Use - E").SetValue(true));

                Config.AddSubMenu(new Menu("Harass Options:", "harassspells"));
                Config.SubMenu("harassspells").AddItem(new MenuItem("useQHarass", "Use - Q").SetValue(true));
                Config.SubMenu("harassspells").AddItem(new MenuItem("useEHarass", "Use - E").SetValue(true));

                Config.AddSubMenu(new Menu("Draw Options:", "drawing"));
                Config.SubMenu("drawing").AddItem(new MenuItem("noDraw", "Disable - Drawing").SetValue(true));
                //Config.SubMenu("drawing").AddItem(new MenuItem("drawDmg", "Draw - Damage Marks").SetValue(true));
                Config.SubMenu("drawing").AddItem(new MenuItem("drawQ", "Draw - Q range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
                Config.SubMenu("drawing").AddItem(new MenuItem("drawW", "Draw - W range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
                Config.SubMenu("drawing").AddItem(new MenuItem("drawE", "Draw - E range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
                Config.AddToMainMenu();
                Drawing.OnDraw += OnDraw;
            }
            catch (Exception)
            {
                Game.PrintChat("Error found in bombs. Refused to load.");
            }
        }

        public static void Game_OnGameUpdate(EventArgs args)
        {
            try
            {

                if (Config.Item("ComboKey").GetValue<KeyBind>().Active)
                {
                    Combo();
                }
                if (Config.Item("HarassKey").GetValue<KeyBind>().Active)
                {
                    Harass();
                }

                if (Config.Item("useRauto").GetValue<bool>())
                {
                    AutoUlt();
                }

                if (Config.Item("FarmKeyFreeze").GetValue<KeyBind>().Active)
                {
                    Farm();
                }
                if (Config.Item("LaneClear").GetValue<KeyBind>().Active)
                {
                    LaneClear();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            
        }

        private static void OnDraw(EventArgs args)
        {
            if (Config.Item("noDraw").GetValue<bool>())
            {
                return;
            }
            var qValue2 = Config.Item("drawQ").GetValue<Circle>();
            var wValue = Config.Item("drawW").GetValue<Circle>();
            var eValue = Config.Item("drawE").GetValue<Circle>();

            if (qValue2.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, qValue2.Color);
            }

            if (wValue.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, wValue.Color);
            }


            if (eValue.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, eValue.Color);
            }



        }

        private static void AutoUlt()
        {
           foreach (var hero in ObjectManager.Get<Obj_AI_Hero>().Where(x => ObjectManager.Player.GetSpellDamage(x, SpellSlot.R) >= x.Health && x.IsValidTarget()))
           {

               Drawing.DrawText(Drawing.Width * 0.44f, Drawing.Height * 0.7f, System.Drawing.Color.GreenYellow, "Ult can kill: " + hero.BaseSkinName);
           }
        }

        private static void Farm()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
                if (Q.IsReady())
                {
                    foreach (var minion in allMinions.Where(x => Player.GetSpellDamage(x, SpellSlot.Q) >= HealthPrediction.GetHealthPrediction(x, (int)(800))))
                    {
                            FarmCast(minion);
                    }
                }
        }


        private static void LaneClear()
        {
            var rangedMinions = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.Ranged);
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            if (Q.IsReady())
            {
                var rangedLocation = Q.GetCircularFarmLocation(rangedMinions);
                var location = Q.GetCircularFarmLocation(allMinions);

                var bLocation = (location.MinionsHit > rangedLocation.MinionsHit + 1) ? location : rangedLocation;

                if (bLocation.MinionsHit > 0)
                {
                    Q.Cast(bLocation.Position.To3D());
                }
            }
        }

        static bool IsInPassiveForm()
        {
            return ObjectManager.Player.IsZombie; //!ObjectManager.Player.IsHPBarRendered;
        }

        public static Vector3 FindHitPosition(PredictionOutput minion)
        {
            Console.WriteLine("Searching hit position");
            int multihit = 0;
            for (int i = -100; i < 100; i = i + 10)
            {
                for (int a = -100; a < 100; a = a + 10)
                {
                    Vector3 tempposition = new Vector3(minion.UnitPosition.X + i, minion.UnitPosition.Y + a, minion.UnitPosition.Z);
                    multihit = CheckMultiHit(tempposition);
                    if (multihit == 1)
                    {
                        return tempposition;
                    }
                }
            }
                return new Vector3(0,0,0);
        }

        static int CheckMultiHit(Vector3 minion)
        {
            var count = 0;
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range);
            foreach (Obj_AI_Base minionvar in allMinions.Where(x => Vector3.Distance(minion, Prediction.GetPrediction(x, 250f).UnitPosition) < 200))
            {
                    count++;               
            }
            return count;
        }
        private static void FarmCast(Obj_AI_Base minion)
        {
            Console.WriteLine("Starting farm check");
            var position = FindHitPosition(Prediction.GetPrediction(minion, 250f));
            if (!(position.X == 0 && position.Y == 0 && position.Z == 0))
            {
                Console.WriteLine("Cast Q: " + position.X + " : " + position.Y + " : " + position.Z);
                Q.Cast(position);
            }
        }
        private static void Combo()
        {
            UseSpells(Config.Item("useQ").GetValue<bool>(), Config.Item("useE").GetValue<bool>(),
                Config.Item("useW").GetValue<bool>());
            var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (IgniteSlot != SpellSlot.Unknown &&
                ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready &&
                ObjectManager.Player.Distance(qTarget.ServerPosition) < 600 &&
                Player.GetSummonerSpellDamage(qTarget, Damage.SummonerSpell.Ignite) > qTarget.Health)
            {
                ObjectManager.Player.Spellbook.CastSpell(IgniteSlot, qTarget);
            }
        }

        private static void Harass()
        {
            UseSpells(Config.Item("useQHarass").GetValue<bool>(), Config.Item("useEHarass").GetValue<bool>(), false);
        }


        private static void UseSpells(bool useQ, bool useE, bool useW)
        {
            
            var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var wTarget = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            var eTarget = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Magical);
            if (useQ && Q.IsReady() && qTarget != null)
            {
                Q.Cast(qTarget);
            }
            if (eTarget != null && useE && E.IsReady() && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).ToggleState == 1)
            {
                E.Cast();
            }
            else if (eTarget == null && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).ToggleState != 1)
            {
                E.Cast();
            }

            if (wTarget != null && useW && W.IsReady())
            {
                W.Cast(wTarget, false, true);
            }

        }


       
    }
}
