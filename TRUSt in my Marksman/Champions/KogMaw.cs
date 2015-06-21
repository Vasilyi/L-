using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using TRUStinMarksman.Utils.Drawings;

namespace TRUStinMarksman.Champions
{
    public class KogMaw
    {
        internal static Spell Q;
        internal static Spell W;
        internal static Spell E;
        internal static Spell R;

        internal static float WRange;

        public KogMaw()
        {
            Q = new Spell(SpellSlot.Q, 1200f);
            Q.SetSkillshot(0.25f, 70f, 1650, true, SkillshotType.SkillshotLine);

            W = new Spell(SpellSlot.W, 500f);

            E = new Spell(SpellSlot.E, 1360f);
            E.SetSkillshot(0.25f, 120f, 1400, false, SkillshotType.SkillshotLine);

            R = new Spell(SpellSlot.R, 1200f);
            R.SetSkillshot(1200f, 150, float.MaxValue, false, SkillshotType.SkillshotCircle);

            // Drawings
            Drawing.OnDraw += args =>
            {
                if (TRUStinMarksman.Config.Item("draww").GetValue<Circle>().Active)
                {
                    Render.Circle.DrawCircle(TRUStinMarksman.Player.Position, WRange,
                        TRUStinMarksman.Config.Item("draww").GetValue<Circle>().Color);
                }
            };

            var dMenu = new Menu("Drawingzs", "drawings");
            dMenu.AddItem(new MenuItem("draww", "W Range")).SetValue(new Circle(true, System.Drawing.Color.White));
            TRUStinMarksman.Config.AddSubMenu(dMenu);

            // Spell usage
            var cMenu = new Menu("Combo", "combo");
            cMenu.AddItem(new MenuItem("combomana", "Minimum mana %")).SetValue(new Slider(5));
            cMenu.AddItem(new MenuItem("usecomboq", "Use Caustic Spittle", true).SetValue(true));
            cMenu.AddItem(new MenuItem("usecombow", "Use Bio-Arcane Barrage", true).SetValue(true));
            cMenu.AddItem(new MenuItem("usecombor", "Use Living Artillery", true).SetValue(true)); 
            cMenu.AddItem(new MenuItem("usecombo", "Combo (active)")).SetValue(new KeyBind(32, KeyBindType.Press));
            TRUStinMarksman.Config.AddSubMenu(cMenu);

            var hMenu = new Menu("Harass", "harass");
            hMenu.AddItem(new MenuItem("harassmana", "Minimum mana %")).SetValue(new Slider(55));
            hMenu.AddItem(new MenuItem("useharassq", "Caustic Spittle", true).SetValue(true));
            hMenu.AddItem(new MenuItem("useharassw", "Bio-Arcane Barrage", true).SetValue(true));
            hMenu.AddItem(new MenuItem("useharassr", "Living Artillery", true).SetValue(true));
            hMenu.AddItem(new MenuItem("useharass", "Harass (active)")).SetValue(new KeyBind(67, KeyBindType.Press));
            TRUStinMarksman.Config.AddSubMenu(hMenu);

            var fMenu = new Menu("Farming", "farming");
            fMenu.AddItem(new MenuItem("clearmana", "Minimum mana %")).SetValue(new Slider(35));
            fMenu.AddItem(new MenuItem("useclear", "Wave/Jungle (active)")).SetValue(new KeyBind(86, KeyBindType.Press));
            TRUStinMarksman.Config.AddSubMenu(fMenu);

            var mMenu = new Menu("Misc", "misc");
            mMenu.AddItem(new MenuItem("useeimm", "Use E on Immobile", true)).SetValue(true);
            mMenu.AddItem(new MenuItem("useegap", "Use E on Gapcloser", true)).SetValue(false);
            TRUStinMarksman.Config.AddSubMenu(mMenu);

            Game.OnUpdate += Game_OnUpdate;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
        }

        internal void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (TRUStinMarksman.Config.Item("useegap", true).GetValue<bool>())
            {
                if (gapcloser.Sender.IsValidTarget(E.Range))
                    E.Cast(gapcloser.Sender);
            }
        }

        private bool ShouldCast()
        {
            if (!Orbwalking.CanAttack(-250) && !Orbwalking.CanAttack() && Orbwalking.CanMove(0))
            {
                return true;
            }


            return false;
        }

        private void Game_OnUpdate(EventArgs args)
        {
            WRange = TRUStinMarksman.Player.HasBuff("KogMawBioArcaneBarrage")
                ? 500 + new[] {130, 150, 170, 190, 210}[W.Level - 1]
                : 500;

            if (E.IsReady())
            {
                foreach (var target in ObjectManager.Get<Obj_AI_Hero>().Where(h => h.IsValidTarget(E.Range)))
                {
                    if (TRUStinMarksman.Config.Item("useeimm", true).GetValue<bool>() && (ShouldCast() || !Orbwalking.InAutoAttackRange(target)))
                        E.CastIfHitchanceEquals(target, HitChance.Immobile);
                }
            }

            if (TRUStinMarksman.CanCombo())
            {
                var qtarget = TargetSelector.GetTargetNoCollision(Q);
                if (qtarget.IsValidTarget() && Q.IsReady())
                {
                    if (TRUStinMarksman.Config.Item("usecomboq", true).GetValue<bool>() && (ShouldCast() || !Orbwalking.InAutoAttackRange(qtarget)))
                        Q.Cast(qtarget);
                }

                var wtarget = TargetSelector.GetTarget(W.Range + new[] { 130, 150, 170, 190, 210 }[W.Level - 1], TargetSelector.DamageType.Physical);
                if (wtarget.IsValidTarget() && W.IsReady())
                {
                    if (TRUStinMarksman.Config.Item("usecombow", true).GetValue<bool>())
                        W.Cast();
                }

                var rtarget = TargetSelector.GetTarget(new[] { 1200f, 1500f, 1800f }[R.Level - 1], TargetSelector.DamageType.Physical);
                if (rtarget.IsValidTarget() && R.IsReady() && (ShouldCast() || !Orbwalking.InAutoAttackRange(rtarget)))
                {
                    var rbuffstacks = TRUStinMarksman.Player.GetBuffCount("kogmawlivingartillery");
                    if (rbuffstacks < 6)
                    {
                        if (TRUStinMarksman.Config.Item("usecombor", true).GetValue<bool>())
                            R.Cast(rtarget);
                    }

                    if (R.GetDamage(rtarget)*2 >= rtarget.Health)
                    {
                        if (TRUStinMarksman.Config.Item("usecombor", true).GetValue<bool>())
                            R.Cast(rtarget);
                    }
                }
            }

            if (TRUStinMarksman.CanHarass())
            {
                var qtarget = TargetSelector.GetTargetNoCollision(Q);
                if (qtarget.IsValidTarget() && Q.IsReady() && TRUStinMarksman.IsWhiteListed(qtarget) && (ShouldCast() || !Orbwalking.InAutoAttackRange(qtarget)))
                {
                    if (TRUStinMarksman.Config.Item("useharassq", true).GetValue<bool>())
                        Q.Cast(qtarget);
                }

                var wtarget = TargetSelector.GetTarget(W.Range + new[] {130,150,170,190,210}[W.Level-1], TargetSelector.DamageType.Physical);
                if (wtarget.IsValidTarget() && W.IsReady() && TRUStinMarksman.IsWhiteListed(wtarget))
                {
                    if (TRUStinMarksman.Config.Item("useharassw", true).GetValue<bool>())
                        W.Cast();
                }

                var rtarget = TargetSelector.GetTarget(new[] {1200f, 1500f, 1800f}[R.Level - 1], TargetSelector.DamageType.Physical);
                if (rtarget.IsValidTarget() && R.IsReady() && TRUStinMarksman.IsWhiteListed(rtarget) && (ShouldCast() || !Orbwalking.InAutoAttackRange(rtarget)))
                {
                    var rbuffstacks = TRUStinMarksman.Player.GetBuffCount("kogmawlivingartillery");
                    if (rbuffstacks < 3)
                    {
                        if (TRUStinMarksman.Config.Item("useharassr", true).GetValue<bool>())
                            R.Cast(rtarget);
                    }
                }
            }
        }
    }
}
