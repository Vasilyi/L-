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
        public static IEnumerable<Obj_AI_Minion> savedbarrels;
        public static List<Vector3> barrelpoints = new List<Vector3>();
        // Spells
        private static Spell Q, W, E, R;
        private const int BarrelExplosionRange = 350;
        private const int BarrelConnectionRange = BarrelExplosionRange * 2 - 20;
        public static Orbwalking.Orbwalker Orbwalker;

        public static Vector3 acoords;
        public static Vector3 bcoords;
        public static void Main(string[] args)
        {


            // Register events
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        public class EDelay
        {
            public static Vector3 position;
            public static int time;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            // Champ validation
            if (player.ChampionName != "Gangplank")
                return;
            // Define spells
            Q = new Spell(SpellSlot.Q, 625f); //2600f
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1000f);
            E.SetSkillshot(0.8f, 50, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R = new Spell(SpellSlot.R);
            R.SetSkillshot(1f, 100, float.MaxValue, false, SkillshotType.SkillshotCircle);
            SetupMenu();
            // Register events
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;

        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            savedbarrels = GetBarrels();
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();

            };
            if (Config.Item("HarassActive").GetValue<KeyBind>().Active)
            {
                Harass();
            };
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

                Config.AddSubMenu(new Menu("Debug", "Debug"));
                Config.SubMenu("Debug")
                                    .AddItem(new MenuItem("debugf", "Debug", true)).SetValue(true);

                Config.AddToMainMenu();
                Console.WriteLine("menu initialize");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public static void Harass()
        {

        }
        public static void Drawing_OnDraw(EventArgs args)
        {
            Render.Circle.DrawCircle(acoords, 60, System.Drawing.Color.Aqua);
            Render.Circle.DrawCircle(bcoords, 60, System.Drawing.Color.Peru);


            if (Config.Item("debugf", true).GetValue<bool>())
            {
                foreach (var points in barrelpoints)
                {
                    Render.Circle.DrawCircle(points, 60, System.Drawing.Color.Aqua);
                }
            }

            foreach (var barrels in savedbarrels)
            {
                if (barrels.IsValid)
                {
                    Render.Circle.DrawCircle(barrels.ServerPosition, BarrelExplosionRange, System.Drawing.Color.Aqua);
                }
            }
        }

        public static IEnumerable<Obj_AI_Minion> GetBarrels()
        {
            var MinionList =
                      ObjectManager.Get<Obj_AI_Minion>()
                          .Where(
                              minion =>
                                  minion.IsValidTarget(1500) && minion.Name == "Barrel" && minion.GetBuff("gangplankebarrellife").Caster.IsMe);
            return MinionList;
        }
        public static void Combo()
        {
            CastE();
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

        public static void DebugWrite(string text)
        {
            if (!Config.Item("debugf", true).GetValue<bool>())
                return;
            Console.WriteLine(text);
        }
        public static bool KillableBarrel(Obj_AI_Base targetB)
        {
            if (targetB.Health == 1)
            {
                return true;
            }
            var barrel = savedbarrels.FirstOrDefault(b => b.NetworkId == targetB.NetworkId);
            if (barrel != null)
            {

                var time = targetB.Health * getEActivationDelay();
                DebugWrite(barrel.GetBuff("gangplankebarrellife").StartTime + " : " + Game.Time + " : " + GetQTime(targetB) + " : " + time);
                if (Game.Time - barrel.GetBuff("gangplankebarrellife").StartTime > time - GetQTime(targetB))
                {
                    DebugWrite("KILLABLE");
                    return true;
                }
            }
            DebugWrite("non killable");
            return false;
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

        public static int CheckRangeForBarrels(Vector3 position, int range)
        {
            Console.WriteLine(savedbarrels.Count(b => b.Distance(position) < range));
            return savedbarrels.Count(b => b.Distance(position) < range);
        }

        public static bool FindChainBarrels(Vector3 position)
        {
            Vector3 testposition = new Vector3(0,0,0);
             foreach (var barrel in savedbarrels)
             {
                 if (barrel.Distance(position) < BarrelConnectionRange)
                 {
                     if (barrel.CountEnemiesInRange(BarrelExplosionRange) >= Config.Item("detoneateTargets").GetValue<Slider>().Value)
                     {

                         return true;
                     }
                     else
                     {
                         testposition = barrel.ServerPosition;
                     }
                 }
             }
             foreach (var barrel in savedbarrels)
             {
                 if (barrel.Distance(testposition) < BarrelConnectionRange && barrel.CountEnemiesInRange(BarrelExplosionRange) >= Config.Item("detoneateTargets").GetValue<Slider>().Value)
                 {
                     return true;
                 }
             }
            return false;
        }
        public static void CastE()
        {
            try
            {
                var targetfore = TargetSelector.GetTarget(E.Range, TargetSelector.DamageType.Physical);
                var enemies =
                    HeroManager.Enemies.Where(e => e.IsValidTarget() && e.Distance(player) < E.Range)
                        .Select(e => Prediction.GetPrediction(e, 0.35f));
                foreach (var barrel in savedbarrels.Where(b => b.IsValidTarget(Q.Range) && KillableBarrel(b)))
                {
                    var newP = GetBarrelPoints(barrel.Position).Where(p => !p.IsWall());
                    if (newP.Any())
                    {
                        barrelpoints.AddRange(newP.Where(p => p.Distance(player.Position) < E.Range));
                    }
                }
                if (barrelpoints.Any() &&  E.IsReady())
                {
                    foreach (var secondbarrelpoint in barrelpoints)
                    {
                        if (secondbarrelpoint == null)
                        {
                            return;
                        }
                         DebugWrite("finding second " + secondbarrelpoint.CountEnemiesInRange(BarrelExplosionRange));
                        if (secondbarrelpoint.CountEnemiesInRange(BarrelExplosionRange) < Config.Item("detoneateTargets").GetValue<Slider>().Value)
                        {
                            if (Config.Item("detoneateTargets").GetValue<Slider>().Value == 1)
                            {
                                //Console.WriteLine(" 1 target");
                                
                                var closest = barrelpoints.MinOrDefault(point => point.Distance(targetfore.ServerPosition));
                                if (closest.CountEnemiesInRange(BarrelExplosionRange)>0 && CheckRangeForBarrels(closest, BarrelExplosionRange) == 0 && closest != EDelay.position)
                                {
                                    EDelay.position = closest;
                                    EDelay.time = Environment.TickCount;
                                    E.Cast(closest);
                                }
                            }
                            else if (secondbarrelpoint != EDelay.position)
                            {
                                Console.WriteLine("> 1 target");
                                EDelay.position = secondbarrelpoint;
                                EDelay.time = Environment.TickCount;
                                E.Cast(secondbarrelpoint);
                            }
                        }

                        foreach (var barrel in savedbarrels.Where(b => b.IsValidTarget(Q.Range) && KillableBarrel(b)))
                        {

                            if (targetfore.Distance(barrel) < BarrelConnectionRange * E.Instance.Ammo)
                            {
                                var closest = barrelpoints.MinOrDefault(point => point.Distance(targetfore.ServerPosition));
                                if (CheckRangeForBarrels(closest, BarrelExplosionRange) == 0 && closest != EDelay.position)
                                {
                                    EDelay.position = secondbarrelpoint;
                                    EDelay.time = Environment.TickCount;
                                    E.Cast(closest);
                                }
                            }

                            if (FindChainBarrels(barrel.ServerPosition))
                            {
                                Q.Cast(barrel);
                            }
                            
                        }


                    }
                }

                barrelpoints = null;
                barrelpoints = new List<Vector3>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

    }
}