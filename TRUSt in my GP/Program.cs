using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;

using SharpDX;

using Color = System.Drawing.Color;

namespace Gangplank
{
    public class Barrel
    {
        public Obj_AI_Minion barrel;
        public float time;

        public Barrel(Obj_AI_Minion objAiBase, int tickCount)
        {
            barrel = objAiBase;
            time = tickCount;
        }
    }
    public class Program
    {
        public static Menu Config;
        public static readonly Obj_AI_Hero player = ObjectManager.Player;

        // Spells
        private static Spell Q, W, E, R;
        private const int BarrelExplosionRange = 375;
        private const int BarrelConnectionRange = 660;
        public static List<Barrel> savedBarrels = new List<Barrel>();
        public static Orbwalking.Orbwalker Orbwalker;

        public static Vector3 acoords;
        public static Vector3 bcoords;


        public static void Main(string[] args)
        {
            // Register events
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            GameObject.OnCreate += GameObjectOnOnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
        }



        public static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            for (int i = 0; i < savedBarrels.Count; i++)
            {
                if (savedBarrels[i].barrel.NetworkId == sender.NetworkId)
                {
                    savedBarrels.RemoveAt(i);
                    return;
                }
            }
        }

        public static void GameObjectOnOnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name == "Barrel")
            {
                savedBarrels.Add(new Barrel(sender as Obj_AI_Minion, System.Environment.TickCount));
            }
        }

        public static IEnumerable<Obj_AI_Minion> GetBarrels()
        {
            return savedBarrels.Select(b => b.barrel).Where(b => b.IsValid);
        }
        private static void Game_OnGameLoad(EventArgs args)
        {
            // Champ validation
            if (player.ChampionName != "Gangplank")
                return;
            // Define spells
            Q = new Spell(SpellSlot.Q, 590f); //2600f
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 950);
            E.SetSkillshot(0.8f, 50, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R = new Spell(SpellSlot.R);
            R.SetSkillshot(1f, 100, float.MaxValue, false, SkillshotType.SkillshotCircle);

            // Register events
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            SetupMenu();
        }


        public static void SetupMenu()
        {
            try
            {
                
                Config = new Menu("GangPlank", "GangPlank", true);

                var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
                TargetSelector.AddToMenu(targetSelectorMenu);
                Config.AddSubMenu(targetSelectorMenu);
                //Orbwalker submenu
                Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));

                //Load the orbwalker and add it to the submenu.
                Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
                Config.AddSubMenu(new Menu("Barrel", "Barrel"));
                Config.SubMenu("Barrel")
                    .AddItem(new MenuItem("useq", "Use Q", true)).SetValue(true);
                Config.SubMenu("Barrel")
                    .AddItem(new MenuItem("detoneateTargets", "   Blow up enemies with E"))
                    .SetValue(new Slider(2, 1, 5));

                Config.AddSubMenu(new Menu("Combo", "Combo"));
                Config.SubMenu("Combo")
                    .AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

                Config.AddSubMenu(new Menu("Harass", "Harass"));
                Config.SubMenu("Harass")
                    .AddItem(
                        new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));

                Config.AddToMainMenu();
                Console.WriteLine("menu initialize");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        public static List<Vector3> PointsAroundTheTargetOuterRing(Vector3 pos, float dist, float width = 15)
        {
            if (!pos.IsValid())
            {
                return new List<Vector3>();
            }
            List<Vector3> list = new List<Vector3>();
            var max = 2 * dist / 2 * Math.PI / width / 2;
            var angle = 360f / max * Math.PI / 180.0f;
            for (int i = 0; i < max; i++)
            {
                list.Add(
                    new Vector3(
                        pos.X + (float)(Math.Cos(angle * i) * dist), pos.Y + (float)(Math.Sin(angle * i) * dist),
                        pos.Z));
            }

            return list;
        }
        public static List<Vector3> GetBarrelPoints(Vector3 point)
        {
            return PointsAroundTheTargetOuterRing(point, BarrelConnectionRange, 20f);
        }

        public static float getEActivationDelay()
        {
            if (player.Level >= 13)
            {
                return 0.5f;
            }
            if (player.Level >= 7)
            {
                return 1f;
            }
            return 2f;
        }
        public static float GetQTime(Obj_AI_Base targetB)
        {
            return player.Distance(targetB) / 2800f + 0.25f;
        }
        public static bool KillableBarrel(Obj_AI_Base targetB)
        {
            if (targetB.Health < 2)
            {
                return true;
            }
            var barrel = savedBarrels.FirstOrDefault(b => b.barrel.NetworkId == targetB.NetworkId);
            if (barrel != null)
            {
                var time = targetB.Health * getEActivationDelay();
                if (System.Environment.TickCount - barrel.time - time - GetQTime(targetB) < 0)
                {
                    return true;
                }
            }
            return false;
        }
        public static void CastE(List<Obj_AI_Minion> barrels)
        {
            var enemies =
                HeroManager.Enemies.Where(e => e.IsValidTarget() && e.Distance(player) < E.Range)
                    .Select(e => Prediction.GetPrediction(e, 0.35f));
            List<Vector3> points = new List<Vector3>();
            foreach (var barrel in
                barrels.Where(b => b.Distance(player) < Q.Range && KillableBarrel(b)))
            {
                if (barrel != null)
                {
                    var newP = GetBarrelPoints(barrel.Position).Where(p => !p.IsWall());
                    if (newP.Any())
                    {
                        points.AddRange(newP.Where(p => p.Distance(player.Position) < E.Range));
                    }
                }
            }
            var bestPoint =
                points.Where(b => enemies.Count(e => e.UnitPosition.Distance(b) < BarrelExplosionRange) > 0)
                    .OrderByDescending(b => enemies.Count(e => e.UnitPosition.Distance(b) < BarrelExplosionRange))
                    .FirstOrDefault();
            if (bestPoint.IsValid() &&
                !savedBarrels.Any(b => b.barrel.Position.Distance(bestPoint) < BarrelConnectionRange))
            {
                E.Cast(bestPoint);
            }
            else
            {
                var targetfore = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                Vector3 doublebarrelsource = new Vector3(0,0,0);
                foreach (var barrel in barrels.Where(b => b.Distance(player) < Q.Range && KillableBarrel(b)))
                {
                    if (barrel != null && barrel.Distance(targetfore) < BarrelConnectionRange * E.Instance.Ammo)
                    {
                        doublebarrelsource = barrel.Position;
                        acoords = barrel.Position;
                        continue;
                    }
                }
                if (doublebarrelsource != new Vector3(0,0,0))
                {
                    Vector3 positionforfirst = Geometry.Extend(targetfore.Position, doublebarrelsource, BarrelConnectionRange);
                    E.Cast(positionforfirst);
                    if (positionforfirst.Distance(targetfore.Position) > BarrelExplosionRange)
                    {
                        Vector3 positionforsecond = Geometry.Extend(targetfore.Position, doublebarrelsource, targetfore.Distance(doublebarrelsource) - BarrelConnectionRange);
                        bcoords = positionforsecond;
                        Utility.DelayAction.Add(10, () =>
                        {
                            CastEDouble(positionforsecond);
                        });
                    }
                    Utility.DelayAction.Add(20, () =>
                    {
                        Q.Cast(doublebarrelsource);
                    });
                }
            }
        }
        public static void CastEDouble(Vector3 positionforsecond)
        {

        }

        private static void MeleeBarrel(List<Obj_AI_Minion> barrels)
        {
            var meleeRangeBarrel = barrels.FirstOrDefault(b =>b.Health < 2 && Orbwalking.InAutoAttackRange(b) &&b.CountEnemiesInRange(BarrelExplosionRange) > 0);
            if (meleeRangeBarrel != null)
            {
                Orbwalker.ForceTarget(meleeRangeBarrel);
            }
        }


        private static void QLogic(List<Obj_AI_Minion> barrels)
        {
            if (Q.IsReady())
            {
                Obj_AI_Hero target = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                if (barrels.Any())
                {
                    var detoneateTargetBarrels = barrels.Where(b => b.Distance(player) < Q.Range && KillableBarrel(b));
                    var enemies =
                        HeroManager.Enemies.Where(e => e.IsValidTarget() && e.Distance(player) < 600)
                            .Select(e => Prediction.GetPrediction(e, 0.25f));
                    var enemies2 = HeroManager.Enemies.Where(e => e.IsValidTarget() && e.Distance(player) < E.Range).Select(e => Prediction.GetPrediction(e, 0.4f));
                    if (detoneateTargetBarrels.Any())
                    {
                        foreach (var detoneateTargetBarrel in detoneateTargetBarrels)
                        {
                            var enemyCount =
                                enemies.Count(
                                    e =>
                                        e.UnitPosition.Distance(detoneateTargetBarrel.Position) <
                                        BarrelExplosionRange);
                            if (enemyCount >= Config.Item("detoneateTargets", true).GetValue<Slider>().Value &&
                                detoneateTargetBarrel.CountEnemiesInRange(BarrelExplosionRange) >=
                                Config.Item("detoneateTargets", true).GetValue<Slider>().Value)
                            {
                                Q.CastOnUnit(detoneateTargetBarrel);
                                return;
                            }
                            var detoneateTargetBarrelSeconds =
                                barrels.Where(b => b.Distance(detoneateTargetBarrel) < BarrelConnectionRange);
                            if (detoneateTargetBarrelSeconds.Any())
                            {
                                foreach (var detoneateTargetBarrelSecond in detoneateTargetBarrelSeconds)
                                {
                                    if (enemyCount +
                                        enemies2.Count(
                                            e =>
                                                e.UnitPosition.Distance(detoneateTargetBarrelSecond.Position) <
                                                BarrelExplosionRange) >=
                                        Config.Item("detoneateTargets", true).GetValue<Slider>().Value &&
                                        detoneateTargetBarrelSecond.CountEnemiesInRange(BarrelExplosionRange) >=
                                        Config.Item("detoneateTargets", true).GetValue<Slider>().Value)
                                    {
                                        Q.CastOnUnit(detoneateTargetBarrel);
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                if (E.Instance.Ammo > 0)
                                {
                                    foreach (var enemy in HeroManager.Enemies.Where(e => e.IsValidTarget() && e.Distance(player) < Q.Range))
                                    {
                                        var detoneateTargetBarrelThird = barrels.Where(b => b.Distance(enemy) < BarrelConnectionRange * E.Instance.Ammo + BarrelExplosionRange);
                                        if (detoneateTargetBarrelThird.Any())
                                        {
                                            CastE(barrels);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            var targetforQ = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (Config.Item("useq", true).GetValue<bool>() && Q.CanCast(targetforQ) && Orbwalking.CanMove(100))
            {
                Q.CastOnUnit(targetforQ);
            }
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            };
            if (Config.Item("HarassActive").GetValue<KeyBind>().Active)
            {
                Harass();
            };
        }

        public static void Combo()
        {
            var barrels = GetBarrels().Where(o =>
                o.IsValid && !o.IsDead && o.Distance(player) < 1600 && o.SkinName == "GangplankBarrel" &&
                o.GetBuff("gangplankebarrellife").Caster.IsMe).ToList();
            if (barrels.Any())
            {
                CastE(barrels);
            }
        }
        public static void Harass()
        {

        }
        public static void Drawing_OnDraw(EventArgs args)
        {
            Render.Circle.DrawCircle(acoords, 60, System.Drawing.Color.Aqua);
            Render.Circle.DrawCircle(bcoords, 60, System.Drawing.Color.Peru);
        }

    }
}