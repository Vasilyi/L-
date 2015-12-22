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

    public class Program
    {
        public static Menu Config;
        public static readonly Obj_AI_Hero player = ObjectManager.Player;
        public static List<Vector3> barrelpoints = new List<Vector3>();
        // Spells
        private static Spell Q, W, E, R;
        private const int BarrelExplosionRange = 350;
        private const int BarrelConnectionRange = BarrelExplosionRange * 2 - 20;
        public static Orbwalking.Orbwalker Orbwalker;
        public static List<Obj_AI_Minion> savedbarrels = new List<Obj_AI_Minion>();
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
            Q = new Spell(SpellSlot.Q, 600f); //2600f
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 1000f);
            E.SetSkillshot(0.8f, 50, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R = new Spell(SpellSlot.R);
            R.SetSkillshot(1f, 100, float.MaxValue, false, SkillshotType.SkillshotCircle);
            SetupMenu();
            // Register events
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += GameObjectOnOnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
        }


        public static void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            savedbarrels.RemoveAll(b => b.NetworkId == sender.NetworkId || !b.IsValidTarget());
        }

        public static void GameObjectOnOnCreate(GameObject sender, EventArgs args)
        {
            if (sender.Name == "Barrel")
            {
                savedbarrels.Add(sender as Obj_AI_Minion);
            }
        }



        private static void Game_OnGameUpdate(EventArgs args)
        {
            Orbwalker.SetAttack(true);
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();

            };
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit)
            {
                Farm();
            };

            if (Config.Item("coorda").GetValue<KeyBind>().Active)
            {
                acoords = Game.CursorPos;
            };

            if (Config.Item("coordb").GetValue<KeyBind>().Active)
            {
                bcoords = Game.CursorPos;
            };


            if (Config.Item("AutoDetonate", true).GetValue<bool>() && Q.IsReady())
            {
                AutoExplode();

            };
            if (Config.Item("explodenear").GetValue<KeyBind>().Active)
            {
                ExplodeNearBarrel();
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
                Config.SubMenu("Barrel").AddItem(new MenuItem("AutoDetonate", "Auto Detonate", true)).SetValue(true);
                Config.SubMenu("Barrel")
                    .AddItem(new MenuItem("detoneateTargets", "Blow up enemies with E"))
                    .SetValue(new Slider(2, 1, 5));
                Config.SubMenu("Barrel").AddItem(new MenuItem("ScanIntense", "Increase if have lags")).SetValue(new Slider(2, 1, 10));

                Config.AddSubMenu(new Menu("Combo", "Combo"));
                Config.SubMenu("Combo")
                    .AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));

                Config.AddSubMenu(new Menu("Farm", "Farm"));
                Config.SubMenu("Farm").AddItem(new MenuItem("FarmActive", "Q farm", true)).SetValue(true);
                Config.SubMenu("Farm").AddItem(new MenuItem("explodenear", "Explode near barrel").SetValue(new KeyBind(32, KeyBindType.Press)));

                Config.AddSubMenu(new Menu("Draw", "Draw"));
                Config.SubMenu("Draw").AddItem(new MenuItem("DrawBarrels", "Barrels range", true)).SetValue(true);
                Config.SubMenu("Draw").AddItem(new MenuItem("DrawBarrelsExplode", "Barrels range connect explode", true)).SetValue(true);
                Config.SubMenu("Draw").AddItem(new MenuItem("DrawBarrelsTime", "Barrels ready time", true)).SetValue(true);

                Config.AddSubMenu(new Menu("Debug", "Debug"));
                Config.SubMenu("Debug")
                                    .AddItem(new MenuItem("debugf", "Debug", true)).SetValue(true);
                Config.SubMenu("Debug")
                                    .AddItem(new MenuItem("trieplb", "Triple Barrel test", true)).SetValue(true);
                Config.SubMenu("Debug").AddItem(new MenuItem("coorda", "A!").SetValue(new KeyBind(32, KeyBindType.Press)));
                Config.SubMenu("Debug").AddItem(new MenuItem("coordb", "B!").SetValue(new KeyBind(32, KeyBindType.Press)));

                Config.AddToMainMenu();
                Console.WriteLine("menu initialize");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public static void Farm()
        {
            if (Q.IsReady() && Config.Item("FarmActive", true).GetValue<bool>())
            {

                var minion =
                    MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.NotAlly)
                        .Where(m => m.Health < Q.GetDamage(m) && m.Name != "Barrel" && HealthPrediction.GetHealthPrediction(m, (int)GetQTime(m)) > 0)
                        .OrderByDescending(m => m.MaxHealth)
                        .ThenByDescending(m => m.Distance(player))
                        .FirstOrDefault();

                if (minion != null)
                {
                    Q.CastOnUnit(minion);
                }
            }
        }
        public static void Drawing_OnDraw(EventArgs args)
        {
            if (acoords != null)
            {
                Render.Circle.DrawCircle(acoords, 60, System.Drawing.Color.Aqua);
            }
            if (bcoords != null)
            {
                Render.Circle.DrawCircle(bcoords, 60, System.Drawing.Color.Peru);
            }
            if (savedbarrels == null)
            {
                return;
            }
            foreach (var barrels in savedbarrels.Where(b => b.IsValidTarget(E.Range)))
            {
                if (Config.Item("DrawBarrels", true).GetValue<bool>())
                {
                    Render.Circle.DrawCircle(barrels.ServerPosition, BarrelExplosionRange, System.Drawing.Color.Aqua);
                }
                if (Config.Item("DrawBarrelsExplode", true).GetValue<bool>())
                {
                    Render.Circle.DrawCircle(barrels.ServerPosition, BarrelExplosionRange + BarrelConnectionRange, System.Drawing.Color.Aqua);
                }
                if (Config.Item("DrawBarrelsTime", true).GetValue<bool>())
                {
                    var pos = Drawing.WorldToScreen(new Vector3(barrels.ServerPosition.X, barrels.ServerPosition.Y, barrels.ServerPosition.Z));
                    var timeleft = (barrels.GetBuff("gangplankebarrellife").StartTime - Game.Time + getEActivationDelay() * 2);
                    if (timeleft > 0 && timeleft.ToString().Length > 2)
                    {
                        Drawing.DrawText(pos.X, pos.Y, Color.Aqua, timeleft.ToString().Substring(0, 3));
                    }
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

        public static void AutoExplode()
        {
            foreach (var barrel in savedbarrels)
            {
                if (KillableBarrel(barrel) && Q.IsReady() && barrel.CountEnemiesInRange(BarrelExplosionRange) >= Config.Item("detoneateTargets").GetValue<Slider>().Value)
                {
                    Q.Cast(barrel);
                }
            }
        }


        public static void ExplodeNearBarrel()
        {

            var tempbarrel = savedbarrels.Where(b => b.IsValidTarget(Q.Range)).MinOrDefault(b => player.Distance(b));
            if (Q.IsReady() && tempbarrel.IsValidTarget(Q.Range) && KillableBarrel(tempbarrel))
            {
                Q.Cast(tempbarrel);
            }

        }
        public static bool KillableBarrel(Obj_AI_Base targetB, bool ecastinclude = false)
        {
            float adddelay = 0;
            if (ecastinclude)
            {
                adddelay = 0.25f;
            }
            if (targetB.Health == 1)
            {
                return true;
            }
            var barrel = savedbarrels.FirstOrDefault(b => b.NetworkId == targetB.NetworkId);
            if (barrel != null)
            {

                var time = getEActivationDelay() * 2;
                // DebugWrite(barrel.GetBuff("gangplankebarrellife").StartTime + " : " + Game.Time + " : " + GetQTime(targetB) + " : " + time);
                if (Game.Time - barrel.GetBuff("gangplankebarrellife").StartTime > time - GetQTime(targetB) - adddelay)
                {

                    return true;
                }
            }

            return false;
        }

        public static List<Vector3> PointsAroundTheTargetOuterRing(Vector3 pos, float dist, float width = 15)
        {
            List<Vector3> list = new List<Vector3>();


            if (!pos.IsValid())
            {
                return new List<Vector3>();
            }

            var max = 2 * dist / 2 * Math.PI / width / 2;
            var angle = 360f / max * Math.PI / 180.0f;
            for (int i = 0; i < max; i += 2)
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
            return savedbarrels.Count(b => b.Distance(position) < range);
        }

        public static Obj_AI_Minion FindChainBarrels(Vector3 position)
        {
            foreach (var barrel in savedbarrels)
            {
                if (barrel.IsValidTarget() && barrel.Distance(position) < BarrelConnectionRange)
                {
                    if (barrel.CountEnemiesInRange(BarrelExplosionRange) >= Config.Item("detoneateTargets").GetValue<Slider>().Value)
                    {

                        return barrel;
                    }
                }


            }

            return null;
        }



        public static void Combo()
        {
            try
            {
                barrelpoints.Clear();
                var targetfore = TargetSelector.GetTarget(E.Range + BarrelExplosionRange, TargetSelector.DamageType.Physical);
                var targetforq = TargetSelector.GetTarget(Q.Range + BarrelExplosionRange, TargetSelector.DamageType.Physical);
                bool secondrequired = true;
                Obj_AI_Minion FindChainBarrelObject = null;
                Obj_AI_Minion meleeRangeBarrel = null;
                Obj_AI_Minion rangedbarrel = null;
                bool blockQ = false;
                if (!targetfore.IsValidTarget())
                {
                    return;
                }
                foreach (var barrel in savedbarrels.Where(b => b.IsValidTarget(Q.Range)))
                {
                    if (KillableBarrel(barrel, true))
                    {

                        if (targetfore.Distance(barrel) < BarrelExplosionRange)
                        {
                            secondrequired = false;
                        }
                        var newP = GetBarrelPoints(barrel.Position).Where(p => !p.IsWall() && player.Distance(p) < E.Range && barrel.Distance(p) < BarrelConnectionRange);
                        if (newP.Any())
                        {
                            barrelpoints.AddRange(newP);
                        }
                        foreach (var enemy1 in HeroManager.Enemies.Where(b => b.IsValidTarget(E.Range + BarrelConnectionRange)))
                        {
                                barrelpoints.AddRange(GetBarrelPoints(enemy1.ServerPosition).Where(p => !p.IsWall() && player.Distance(p) < E.Range && barrel.Distance(p) < BarrelConnectionRange));
                        }
                        if (barrel.Distance(targetforq) < BarrelConnectionRange && E.Instance.Ammo > 0)
                        {
                            blockQ = true;
                        }

                        if (KillableBarrel(barrel))
                        {
                            if (Orbwalking.InAutoAttackRange(barrel) && HeroManager.Enemies.Count(o =>
                                         o.IsValidTarget(E.Range + BarrelConnectionRange) && o.Distance(barrel) < BarrelExplosionRange) > 0)
                            {
                                meleeRangeBarrel = barrel;
                            }



                            if (barrel.Distance(targetforq) < BarrelExplosionRange)
                            {
                                rangedbarrel = barrel;
                            }
                        }
                    }
                }

                var pos = Prediction.GetPrediction(targetfore, 0.5f);
                var closest = barrelpoints.MinOrDefault(point => point.Distance(pos.UnitPosition));
                if (E.IsReady() && Q.IsReady() && secondrequired)
                {

                    if (closest != null && pos.Hitchance > HitChance.High && closest.CountEnemiesInRange(BarrelExplosionRange) >= Config.Item("detoneateTargets").GetValue<Slider>().Value)
                    {
                        if (closest != EDelay.position)
                        {
                            EDelay.position = closest;
                            EDelay.time = Environment.TickCount;
                            var qtarget = savedbarrels.MinOrDefault(b => b.IsValidTarget(Q.Range) && KillableBarrel(b, true) && b.Distance(closest) < BarrelConnectionRange);

                            E.Cast(closest);
                            Utility.DelayAction.Add(100, () => Q.Cast(qtarget));
                            if (Config.Item("trieplb", true).GetValue<bool>())
                            {
                                foreach (var barrel in savedbarrels)
                                {
                                    foreach (var enemy3 in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(E.Range + BarrelExplosionRange) && hero.Distance(barrel) > BarrelExplosionRange && hero.Distance(barrel) < BarrelConnectionRange))
                                    {
                                        Utility.DelayAction.Add(200, () => E.Cast(enemy3.ServerPosition));
                                    }
                                }
                            }
                        }
                    }


                }
                if (rangedbarrel.IsValidTarget(Q.Range) && Q.IsReady())
                {
                    Q.Cast(rangedbarrel);
                }


                if (meleeRangeBarrel.IsValidTarget() && !Q.IsReady())
                {
                    Orbwalker.ForceTarget(meleeRangeBarrel);
                }

                var closestbarrel = savedbarrels.Where(barrel => KillableBarrel(barrel) && barrel.IsValidTarget(Q.Range)).MinOrDefault(point => point.Distance(targetfore.ServerPosition));

                if (closestbarrel != null)
                {

                    FindChainBarrelObject = FindChainBarrels(closestbarrel.ServerPosition);

                    if (FindChainBarrelObject.IsValidTarget())
                    {

                        if (Q.IsReady())
                        {
                            Q.Cast(closestbarrel);
                        }
                        else if (Orbwalking.InAutoAttackRange(closestbarrel))
                        {
                            Orbwalker.ForceTarget(closestbarrel);
                        } 
                    }

                }
                if (targetforq.IsValidTarget(Q.Range) && !E.IsReady(2) && (!blockQ  || player.GetSpellDamage(targetforq, SpellSlot.Q) > targetforq.Health))
                {
                    Q.Cast(targetforq);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }




    }
}