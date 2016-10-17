using LeagueSharp;
using LeagueSharp.SDK;
using LeagueSharp.SDK.UI;
using LeagueSharp.SDK.Enumerations;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp.SDK.Utils;

using Color = System.Drawing.Color;

namespace Viktor
{
    public class Program
    {
        public const string CHAMP_NAME = "Viktor";
        public static Obj_AI_Hero player;
        public static List<Spell> SpellList = new List<Spell>();
        // Spells
        private static Spell Q, W, E, R;
        private static readonly int maxRangeE = 1225;
        private static readonly int lengthE = 700;
        private static readonly int speedE = 1050;
        private static readonly int rangeE = 525;
        private static int lasttick = 0;
        private static Vector3 GapCloserPos;

        private static bool AttacksEnabled
        {
            get
            {
                if (ComboMenu["comboActive"].GetValue<MenuKeyBind>().Active)
                {
                    return ((!Q.IsReady() || player.Mana < Q.Instance.ManaCost) && (!E.IsReady() || player.Mana < E.Instance.ManaCost) && (!ComboMenu["qAuto"].GetValue<MenuBool>() || player.HasBuff("viktorpowertransferreturn")));
                }
                else if (HarassMenu["harassActive"].GetValue<MenuKeyBind>().Active)
                {
                    return ((!Q.IsReady() || player.Mana < Q.Instance.ManaCost) && (!E.IsReady() || player.Mana < E.Instance.ManaCost));
                }
                return true;
            }
        }

        // Menu
        private const string MenuName = "TRUSt in my Viktor";
        public static Menu MainMenu, DrawMenu, ComboMenu, RMenu, RSolo, TestFeatures, HarassMenu, LastHit, WaveClear, FleeMenu, MiscMenu;
        public static MenuList<string> HitChanceList;
        public static MenuList<string> RTargetsAmount;

        public static Orbwalker Orbwalker => Variables.Orbwalker;


        public static void Main(string[] args)
        {
            Bootstrap.Init(args);
            Events.OnLoad += Game_OnGameLoad;

        }

        private static readonly TRUStInMyViktor.HpBarIndicator Indicator = new TRUStInMyViktor.HpBarIndicator();


        private static void Game_OnGameLoad(object sender, EventArgs e)
        {
            // Champ validation
            player = ObjectManager.Player;
            if (player.ChampionName != CHAMP_NAME)
                return;

            // Define spells
            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 700);
            E = new Spell(SpellSlot.E, rangeE);
            R = new Spell(SpellSlot.R, 700);

            // Finetune spells
            Q.SetTargetted(0.25f, 2000);
            W.SetSkillshot(0.5f, 300, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0, 80, speedE, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.25f, 300f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            // Create menu
            SetupMenu();

            // Register events
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;

            Events.OnGapCloser += AntiGapcloser_OnEnemyGapcloser;
            Variables.Orbwalker.OnAction += Orbwalker_OnAction;
            Events.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Drawing.OnEndScene += DrawDmg;
        }

        public static void DrawDmg(EventArgs args)
        {
            foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(ene => ene.IsValidTarget(1350)))
            {
                if (DrawMenu["dmgdraw"].GetValue<MenuBool>()) continue;

                Indicator.Unit = enemy;
                Indicator.DrawDmg(TotalDmg(enemy, true, true, true, true), SharpDX.Color.LawnGreen);
            }
        }


        private static void Orbwalker_OnAction(object sender, OrbwalkingActionArgs args)
        {
            if (args.Type == OrbwalkingType.BeforeAttack)
            {
                OrbwalkingOnBeforeAttack(args);
            }
            else if (args.Type == OrbwalkingType.NonKillableMinion)
            {
                Orbwalking_OnNonKillableMinion(args);
            }

        }

        private static void OrbwalkingOnBeforeAttack(OrbwalkingActionArgs args)
        {
            if (args.Target.Type == GameObjectType.obj_AI_Hero)
            {
                args.Process = AttacksEnabled;
            }
            else
                args.Process = true;

        }

        private static void Orbwalking_OnNonKillableMinion(OrbwalkingActionArgs args)
        {
            QLastHit((Obj_AI_Base)args.Target);
        }
        private static void QLastHit(Obj_AI_Base minion)
        {

            bool castQ = ((WaveClear["waveUseQLH"].GetValue<MenuBool>()) || WaveClear["waveUseQ"].GetValue<MenuBool>() && WaveClear["waveActive"].GetValue<MenuBool>());
            if (castQ)
            {

                var distance = Extensions.Distance(player, minion);
                var t = 250 + (int)distance / 2;
                var predHealth = Health.GetPrediction(minion, t, 0);

                // Console.WriteLine(" Distance: " + distance + " timer : " + t + " health: " + predHealth);
                if (predHealth > 0 && Q.CanKill(minion))
                {
                    Q.Cast(minion);
                }
            }
        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            // Combo
            if (ComboMenu["comboActive"].GetValue<MenuKeyBind>().Active)
                OnCombo();
            // Harass�
            if (HarassMenu["harassActive"].GetValue<MenuKeyBind>().Active)
                OnHarass();
            // WaveClear
            if (WaveClear["waveActive"].GetValue<MenuKeyBind>().Active)
                OnWaveClear();

            if (WaveClear["jungleActive"].GetValue<MenuKeyBind>().Active)
                OnJungleClear();

            if (FleeMenu["FleeActive"].GetValue<MenuKeyBind>().Active)
                Flee();

            if (RSolo["forceR"].GetValue<MenuKeyBind>().Active)
            {
                if (R.IsReady())
                {
                    List<Obj_AI_Hero> ignoredchamps = new List<Obj_AI_Hero>();

                    foreach (var hero in GameObjects.EnemyHeroes)
                    {
                        if (!RSolo["RU" + hero.ChampionName].GetValue<MenuBool>())
                        {
                            ignoredchamps.Add(hero);
                        }
                    }
                    var RTarget = R.GetTarget(0, false, ignoredchamps);
                    if (RTarget.IsValidTarget())
                    {
                        R.Cast(RTarget);
                    }
                }

            }
            // Ultimate follow
            if (R.Instance.Name != "ViktorChaosStorm" && RMenu["AutoFollowR"].GetValue<MenuBool>() && Environment.TickCount - lasttick > 0)
            {
                var stormT = Variables.TargetSelector.GetTarget(1100, DamageType.Magical);
                if (stormT != null)
                {
                    R.Cast(stormT.ServerPosition);
                    lasttick = Environment.TickCount + 500;
                }
            }
        }

        private static void OnCombo()
        {

            try
            {



                bool useQ = ComboMenu["comboUseQ"].GetValue<MenuBool>() && Q.IsReady();
                bool useW = ComboMenu["comboUseW"].GetValue<MenuBool>() && W.IsReady();
                bool useE = ComboMenu["comboUseE"].GetValue<MenuBool>() && E.IsReady();
                bool useR = ComboMenu["comboUseR"].GetValue<MenuBool>() && R.IsReady();

                bool killpriority = TestFeatures["spPriority"].GetValue<MenuBool>() && R.IsReady();
                bool rKillSteal = RSolo["rLastHit"].GetValue<MenuBool>();
                Obj_AI_Hero Etarget = Variables.TargetSelector.GetTarget(maxRangeE, DamageType.Magical);
                Obj_AI_Hero Qtarget = Variables.TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                Obj_AI_Hero RTarget = Variables.TargetSelector.GetTarget(R.Range, DamageType.Magical);
                if (killpriority && Qtarget != null & Etarget != null && Etarget != Qtarget && ((Etarget.Health > TotalDmg(Etarget, false, true, false, false)) || (Etarget.Health > TotalDmg(Etarget, false, true, true, false) && Etarget == RTarget)) && Qtarget.Health < TotalDmg(Qtarget, true, true, false, false))
                {
                    Etarget = Qtarget;
                }

                if (RTarget != null && rKillSteal && useR && RSolo["RU" + RTarget.ChampionName].GetValue<MenuBool>())
                {
                    if (TotalDmg(RTarget, true, true, false, false) < RTarget.Health && TotalDmg(RTarget, true, true, true, true) > RTarget.Health)
                    {
                        R.Cast(RTarget.ServerPosition);
                    }
                }


                if (useE)
                {
                    if (Etarget != null)
                        PredictCastE(Etarget);
                }
                if (useQ)
                {

                    if (Qtarget != null)
                        Q.Cast(Qtarget);
                }
                if (useW)
                {
                    var t = Variables.TargetSelector.GetTarget(W.Range, DamageType.Magical);

                    if (t != null)
                    {
                        if (t.Path.Count() < 2)
                        {
                            if (t.HasBuffOfType(BuffType.Slow))
                            {
                                if (W.GetPrediction(t).Hitchance >= HitChance.VeryHigh)
                                    if (W.Cast(t) == CastStates.SuccessfullyCasted)
                                        return;
                            }
                            if (Extensions.CountEnemyHeroesInRange(t, 250) > 2)
                            {
                                if (W.GetPrediction(t).Hitchance >= HitChance.VeryHigh)
                                    if (W.Cast(t) == CastStates.SuccessfullyCasted)
                                        return;
                            }
                        }
                    }
                }
                if (useR && R.Instance.Name == "ViktorChaosStorm" && player.CanCast && !player.Spellbook.IsCastingSpell)
                {

                    foreach (var unit in GameObjects.EnemyHeroes.Where(h => h.IsValidTarget(R.Range)))
                    {
                        R.CastIfWillHit(unit, RMenu["HitR"].GetValue<MenuList>().Index + 1);

                    }
                }
            }
            catch (Exception error)
            {
                Console.WriteLine(error.ToString());
            }
        }

        private static void Flee()
        {
            Orbwalker.Move(Game.CursorPos);

            if (!Q.IsReady() || !(player.HasBuff("viktorqaug") || player.HasBuff("viktorqeaug") || player.HasBuff("viktorqwaug") || player.HasBuff("viktorqweaug")))
            {
                return;
            }
            var closestenemy = GameObjects.Enemy.MinOrDefault(m => m.IsValidTarget(Q.Range));

            if (Q.CanCast(closestenemy))
            {
                Q.Cast(closestenemy);
            }

        }


        private static void OnHarass()
        {
            // Mana check
            if ((player.Mana / player.MaxMana) * 100 < HarassMenu["harassMana"].GetValue<MenuSlider>())
                return;
            bool useE = HarassMenu["harassUseE"].GetValue<MenuBool>() && E.IsReady();
            bool useQ = HarassMenu["harassUseQ"].GetValue<MenuBool>() && Q.IsReady();
            if (useQ)
            {
                var qtarget = Q.GetTarget();
                if (qtarget != null && Q.CanCast(qtarget))
                    Q.Cast(qtarget);
            }
            if (useE)
            {

                var harassrange = HarassMenu["eDistance"].GetValue<MenuSlider>();
                Obj_AI_Hero target = Variables.TargetSelector.GetTarget(harassrange, DamageType.Magical);
                if (target != null)
                    PredictCastE(target);
            }
        }

        private static void OnWaveClear()
        {
            // Mana check
            if ((player.Mana / player.MaxMana) * 100 < WaveClear["waveMana"].GetValue<MenuSlider>())
                return;

            bool useE = WaveClear["waveUseE"].GetValue<MenuBool>() && E.IsReady();
            bool useQ = WaveClear["waveUseQ"].GetValue<MenuBool>() && Q.IsReady();

            if (useQ)
            {
                foreach (var minion in GameObjects.EnemyMinions.Where(m => m.IsValidTarget(player.AttackRange) && Q.CanKill(m) && m.CharData.BaseSkinName.Contains("Siege")))
                {
                    if (minion.IsValidTarget())
                    {
                        QLastHit(minion);
                        break;
                    }
                }
            }

            if (useE)
                PredictCastMinionE();
        }

        private static void OnJungleClear()
        {
            // Mana check
            if ((player.Mana / player.MaxMana) * 100 < WaveClear["waveMana"].GetValue<MenuSlider>())
                return;

            bool useE = WaveClear["waveUseE"].GetValue<MenuBool>() && E.IsReady();
            bool useQ = WaveClear["waveUseQ"].GetValue<MenuBool>() && Q.IsReady();

            if (useQ)
            {
                var minion = GameObjects.EnemyMinions.Where(m => m.Team == GameObjectTeam.Neutral && m.IsValidTarget(player.AttackRange)).MaxOrDefault(min => min.MaxHealth);
                {
                    Q.Cast(minion);
                }
            }

            if (useE)
                PredictCastMinionEJungle();
        }


        public static FarmLocation GetBestLaserFarmLocation(bool jungle)
        {
            var bestendpos = new Vector2();
            var beststartpos = new Vector2();
            var minionCount = 0;
            List<Obj_AI_Minion> allminions;
            var minimalhit = WaveClear["waveNumE"].GetValue<MenuSlider>();
            if (!jungle)
            {
                allminions = GameObjects.EnemyMinions.Where(m => m.IsValidTarget(maxRangeE) && m.Team != GameObjectTeam.Neutral).ToList();
            }
            else
            {
                allminions = GameObjects.EnemyMinions.Where(m => m.IsValidTarget(maxRangeE) && m.Team == GameObjectTeam.Neutral).ToList();
            }

            var minionslist = (from mnion in allminions select mnion.Position.ToVector2()).ToList();
            var posiblePositions = new List<Vector2>();
            posiblePositions.AddRange(minionslist);
            var max = posiblePositions.Count;
            for (var i = 0; i < max; i++)
            {
                for (var j = 0; j < max; j++)
                {
                    if (posiblePositions[j] != posiblePositions[i])
                    {
                        posiblePositions.Add((posiblePositions[j] + posiblePositions[i]) / 2);
                    }
                }
            }

            foreach (var startposminion in allminions.Where(m => player.Distance(m) < rangeE))
            {
                var startPos = startposminion.Position.ToVector2();

                foreach (var pos in posiblePositions)
                {
                    if (Extensions.DistanceSquared(pos, startPos) <= lengthE * lengthE)
                    {
                        var endPos = startPos + lengthE * (pos - startPos).Normalized();

                        var count =
                            minionslist.Count(pos2 => Extensions.DistanceSquared(pos2, startPos, endPos) <= 140 * 140);

                        if (count >= minionCount)
                        {
                            bestendpos = endPos;
                            minionCount = count;
                            beststartpos = startPos;
                        }

                    }
                }
            }
            if ((!jungle && minimalhit < minionCount) || (jungle && minionCount > 0))
            {
                //Console.WriteLine("MinimalHits: " + minimalhit + "\n Startpos: " + beststartpos + "\n Count : " + minionCount);
                return new FarmLocation(beststartpos, bestendpos, minionCount);
            }
            else
            {
                return new FarmLocation(beststartpos, bestendpos, 0);
            }
        }



        private static bool PredictCastMinionEJungle()
        {
            var farmLocation = GetBestLaserFarmLocation(true);

            if (farmLocation.MinionsHit > 0)
            {
                CastE(farmLocation.Position1, farmLocation.Position2);
                return true;
            }

            return false;
        }

        public struct FarmLocation
        {
            /// <summary>
            /// The minions hit
            /// </summary>
            public int MinionsHit;

            /// <summary>
            /// The start position
            /// </summary>
            public Vector2 Position1;


            /// <summary>
            /// The end position
            /// </summary>
            public Vector2 Position2;

            /// <summary>
            /// Initializes a new instance of the <see cref="FarmLocation"/> struct.
            /// </summary>
            /// <param name="position">The position.</param>
            /// <param name="minionsHit">The minions hit.</param>
            public FarmLocation(Vector2 startpos, Vector2 endpos, int minionsHit)
            {
                Position1 = startpos;
                Position2 = endpos;
                MinionsHit = minionsHit;
            }
        }
        private static bool PredictCastMinionE()
        {
            var farmLoc = GetBestLaserFarmLocation(false);
            if (farmLoc.MinionsHit > 0)
            {
                Console.WriteLine("Minion amount: " + farmLoc.MinionsHit + "\n Startpos: " + farmLoc.Position1 + "\n EndPos: " + farmLoc.Position2);

                CastE(farmLoc.Position1, farmLoc.Position2);
                return true;
            }

            return false;
        }


        private static void PredictCastE(Obj_AI_Hero target)
        {
            // Helpers
            bool inRange = Vector2.DistanceSquared(target.ServerPosition.ToVector2(), player.Position.ToVector2()) < E.Range * E.Range;
            PredictionOutput prediction;
            bool spellCasted = false;

            // Positions
            Vector3 pos1, pos2;

            // Champs
            var nearChamps = (from champ in ObjectManager.Get<Obj_AI_Hero>() where champ.IsValidTarget(maxRangeE) && target != champ select champ).ToList();
            var innerChamps = new List<Obj_AI_Hero>();
            var outerChamps = new List<Obj_AI_Hero>();
            foreach (var champ in nearChamps)
            {
                if (Vector2.DistanceSquared(champ.ServerPosition.ToVector2(), player.Position.ToVector2()) < E.Range * E.Range)
                    innerChamps.Add(champ);
                else
                    outerChamps.Add(champ);
            }

            // Minions
            var nearMinions = GameObjects.EnemyMinions.Where(m => m.IsValidTarget(maxRangeE));

            var innerMinions = new List<Obj_AI_Base>();
            var outerMinions = new List<Obj_AI_Base>();
            foreach (var minion in nearMinions)
            {
                if (Vector3.DistanceSquared(minion.ServerPosition, player.Position) < E.Range * E.Range)
                    innerMinions.Add(minion);
                else
                    outerMinions.Add(minion);
            }

            // Main target in close range
            if (inRange)
            {
                // Get prediction reduced speed, adjusted sourcePosition
                E.Speed = speedE * 0.9f;
                E.From = target.ServerPosition + (Vector3.Normalize(player.Position - target.ServerPosition) * (lengthE * 0.1f));
                prediction = E.GetPrediction(target);
                E.From = player.Position;

                // Prediction in range, go on
                if (prediction.CastPosition.Distance(player.Position) < E.Range)
                    pos1 = prediction.CastPosition;
                // Prediction not in range, use exact position
                else
                {
                    pos1 = target.ServerPosition;
                    E.Speed = speedE;
                }

                // Set new sourcePosition
                E.From = pos1;
                E.RangeCheckFrom = pos1;

                // Set new range
                E.Range = lengthE;

                // Get next target
                if (nearChamps.Count > 0)
                {
                    // Get best champion around
                    var closeToPrediction = new List<Obj_AI_Hero>();
                    foreach (var enemy in nearChamps)
                    {
                        // Get prediction
                        prediction = E.GetPrediction(enemy);
                        // Validate target
                        if (prediction.Hitchance >= HitChance.High && Vector2.DistanceSquared(pos1.ToVector2(), prediction.CastPosition.ToVector2()) < (E.Range * E.Range) * 0.8)
                            closeToPrediction.Add(enemy);
                    }

                    // Champ found
                    if (closeToPrediction.Count > 0)
                    {
                        // Sort table by health DEC
                        if (closeToPrediction.Count > 1)
                            closeToPrediction.Sort((enemy1, enemy2) => enemy2.Health.CompareTo(enemy1.Health));

                        // Set destination
                        prediction = E.GetPrediction(closeToPrediction[0]);
                        pos2 = prediction.CastPosition;

                        // Cast spell
                        CastE(pos1, pos2);
                        spellCasted = true;
                    }
                }

                // Spell not casted
                if (!spellCasted)
                {
                    CastE(pos1, E.GetPrediction(target).CastPosition);
                }

                // Reset spell
                E.Speed = speedE;
                E.Range = rangeE;
                E.From = player.Position;
                E.RangeCheckFrom = player.Position;
            }

            // Main target in extended range
            else
            {
                // Radius of the start point to search enemies in
                float startPointRadius = 150;

                // Get initial start point at the border of cast radius
                Vector3 startPoint = player.Position + Vector3.Normalize(target.ServerPosition - player.Position) * rangeE;

                // Potential start from postitions
                var targets = (from champ in nearChamps where Extensions.DistanceSquared(champ.ServerPosition, startPoint) < startPointRadius * startPointRadius && Extensions.DistanceSquared(player.Position, champ.ServerPosition) < rangeE * rangeE select champ).ToList();
                if (targets.Count > 0)
                {
                    // Sort table by health DEC
                    if (targets.Count > 1)
                        targets.Sort((enemy1, enemy2) => enemy2.Health.CompareTo(enemy1.Health));

                    // Set target
                    pos1 = targets[0].ServerPosition;
                }
                else
                {
                    var minionTargets = (from minion in nearMinions where Extensions.DistanceSquared(minion.ServerPosition, startPoint) < startPointRadius * startPointRadius && Extensions.DistanceSquared(player.Position, minion.ServerPosition) < rangeE * rangeE select minion).ToList();
                    if (minionTargets.Count > 0)
                    {
                        // Sort table by health DEC
                        if (minionTargets.Count > 1)
                            minionTargets.Sort((enemy1, enemy2) => enemy2.Health.CompareTo(enemy1.Health));

                        // Set target
                        pos1 = minionTargets[0].ServerPosition;
                    }
                    else
                        // Just the regular, calculated start pos
                        pos1 = startPoint;
                }

                // Predict target position
                E.From = pos1;
                E.Range = lengthE;
                E.RangeCheckFrom = pos1;
                prediction = E.GetPrediction(target);

                // Cast the E
                if (prediction.Hitchance >= HitChance.High)
                    CastE(pos1, prediction.CastPosition);

                // Reset spell
                E.Range = rangeE;
                E.From = player.Position;
                E.RangeCheckFrom = player.Position;
            }

        }



        private static void CastE(Vector3 source, Vector3 destination)
        {
            E.Cast(source, destination);
        }

        private static void CastE(Vector2 source, Vector2 destination)
        {
            E.Cast(source, destination);
        }

        private static void Interrupter2_OnInterruptableTarget(object sender, Events.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel >= LeagueSharp.Data.Enumerations.DangerLevel.High)
            {
                var unit = args.Sender;
                var useW = MiscMenu["wInterrupt"].GetValue<MenuBool>();
                var useR = MiscMenu["rInterrupt"].GetValue<MenuBool>();


                if (useW && W.IsReady() && unit.IsValidTarget(W.Range) &&
                    (Game.Time + 1.5 + W.Delay) >= args.EndTime)
                {
                    if (W.Cast(unit) == CastStates.SuccessfullyCasted)
                        return;
                }
                else if (useR && unit.IsValidTarget(R.Range) && R.Instance.Name == "ViktorChaosStorm")
                {
                    R.Cast(unit);
                }
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(object sender, Events.GapCloserEventArgs gapcloser)
        {

            if (MiscMenu["miscGapcloser"].GetValue<MenuBool>() && W.IsInRange(gapcloser.End))
            {
                GapCloserPos = gapcloser.End;
                if (Extensions.Distance(gapcloser.Start, gapcloser.End) > gapcloser.Sender.Spellbook.GetSpell(gapcloser.Slot).SData.CastRangeDisplayOverride && gapcloser.Sender.Spellbook.GetSpell(gapcloser.Slot).SData.CastRangeDisplayOverride > 100)
                {
                    GapCloserPos = Extensions.Extend(gapcloser.Start, gapcloser.End, gapcloser.Sender.Spellbook.GetSpell(gapcloser.Slot).SData.CastRangeDisplayOverride);
                }
                W.Cast(GapCloserPos);
            }
        }
        private static void AutoW()
        {
            if (!W.IsReady() || !MiscMenu["autoW"].GetValue<MenuBool>())
                return;

            var tPanth = GameObjects.EnemyHeroes.Find(h => h.IsValidTarget(W.Range) && h.HasBuff("Pantheon_GrandSkyfall_Jump"));
            if (tPanth != null)
            {
                if (W.Cast(tPanth) == CastStates.SuccessfullyCasted)
                    return;
            }

            foreach (var enemy in GameObjects.EnemyHeroes.Where(h => h.IsValidTarget(W.Range)))
            {
                if (enemy.HasBuff("rocketgrab2"))
                {
                    var t = GameObjects.AllyHeroes.Find(h => h.ChampionName.ToLower() == "blitzcrank" && h.Distance((AttackableUnit)player) < W.Range);
                    if (t != null)
                    {
                        if (W.Cast(t) == CastStates.SuccessfullyCasted)
                            return;
                    }
                }
                if (enemy.HasBuffOfType(BuffType.Stun) || enemy.HasBuffOfType(BuffType.Snare) ||
                         enemy.HasBuffOfType(BuffType.Charm) || enemy.HasBuffOfType(BuffType.Fear) ||
                         enemy.HasBuffOfType(BuffType.Taunt) || enemy.HasBuffOfType(BuffType.Suppression) ||
                         enemy.IsStunned || enemy.IsRecalling())
                {
                    if (W.Cast(enemy) == CastStates.SuccessfullyCasted)
                        return;
                }
                if (W.GetPrediction(enemy).Hitchance == HitChance.Immobile)
                {
                    if (W.Cast(enemy) == CastStates.SuccessfullyCasted)
                        return;
                }
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            // All circles
            var menuItem2 = DrawMenu["drawRangeEMax"].GetValue<MenuBool>();
            if (menuItem2 == true)
            {

                var SpellColor2 = DrawMenu["ERangeC2"].GetValue<MenuColor>();
                Render.Circle.DrawCircle(ObjectManager.Player.Position, maxRangeE, SpellColor2.Color.ToSystemColor());
            }
            foreach (Spell spell in SpellList)
            {

                var menuItem = DrawMenu["drawRange" + spell.Slot].GetValue<MenuBool>();
             
                if (menuItem == true)
                {
                    
                    var SpellColor = DrawMenu[spell.Slot + "RangeC"].GetValue<MenuColor>();
                   
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, spell.Range, SpellColor.Color.ToSystemColor());
                }
            }
        }

        private static float TotalDmg(Obj_AI_Base enemy, bool useQ, bool useE, bool useR, bool qRange)
        {
            var qaaDmg = new Double[] { 20, 40, 60, 80, 100 };
            var damage = 0d;
            var rTicks = RMenu["rTicks"].GetValue<MenuSlider>();
            bool inQRange = ((qRange && AutoAttack.InAutoAttackRange(enemy)) || qRange == false);
            //Base Q damage
            if (useQ && Q.IsReady() && inQRange)
            {
                damage += player.GetSpellDamage(enemy, SpellSlot.Q);
                damage += Damage.CalculateDamage(player, enemy, DamageType.Magical, qaaDmg[Q.Level - 1] + 0.5 * player.TotalMagicalDamage + player.TotalAttackDamage);
            }

            // Q damage on AA
            if (useQ && !Q.IsReady() && player.HasBuff("viktorpowertransferreturn") && inQRange)
            {
                damage += Damage.CalculateDamage(player, enemy, DamageType.Magical, qaaDmg[Q.Level - 1] + 0.5 * player.TotalMagicalDamage + player.TotalAttackDamage);
            }

            //E damage
            if (useE && E.IsReady())
            {
                if (player.HasBuff("viktoreaug") || player.HasBuff("viktorqeaug") || player.HasBuff("viktorqweaug"))
                    damage += player.GetSpellDamage(enemy, SpellSlot.E, LeagueSharp.Data.Enumerations.DamageStage.Detonation);
                else
                    damage += player.GetSpellDamage(enemy, SpellSlot.E);
            }

            //R damage + 2 ticks
            if (useR && R.Level > 0 && R.IsReady() && R.Instance.Name == "ViktorChaosStorm")
            {
                damage += Damage.GetSpellDamage(player, enemy, SpellSlot.R, LeagueSharp.Data.Enumerations.DamageStage.DamagePerSecond) * rTicks;
                damage += Damage.GetSpellDamage(player, enemy, SpellSlot.R);
            }

            // Ludens Echo damage
            if (Items.HasItem(3285))
                damage += Damage.CalculateDamage(player, enemy, DamageType.Magical, 100 + player.FlatMagicDamageMod * 0.1);

            //sheen damage
            if (Items.HasItem(3057))
                damage += Damage.CalculateDamage(player, enemy, DamageType.Physical, 0.5 * player.BaseAttackDamage);

            //lich bane dmg
            if (Items.HasItem(3100))
                damage += Damage.CalculateDamage(player, enemy, DamageType.Magical, 0.5 * player.FlatMagicDamageMod + 0.75 * player.BaseAttackDamage);

            return (float)damage;
        }
        private static float GetComboDamage(Obj_AI_Base enemy)
        {

            return TotalDmg(enemy, true, true, true, false);
        }
        private static void SetupMenu()
        {

            MainMenu = new Menu("TRUStInMyViktor", "TRUStInMyViktor", true).Attach();

            // Combo
            ComboMenu = MainMenu.Add(new Menu("Combo", "Combo"));
            ComboMenu.Add(new MenuBool("comboUseQ", "Use Q", true));
            ComboMenu.Add(new MenuBool("comboUseW", "Use W", true));
            ComboMenu.Add(new MenuBool("comboUseE", "Use E", true));
            ComboMenu.Add(new MenuBool("comboUseR", "Use R", true));
            ComboMenu.Add(new MenuBool("qAuto", "Dont autoattack without passive"));
            ComboMenu.Add(new MenuKeyBind("comboActive", "Combo key", System.Windows.Forms.Keys.Space, KeyBindType.Press));

            RMenu = MainMenu.Add(new Menu("RMenu", "R config"));
            RMenu.Add(new MenuList<string>("HitR", "Auto R if: ", new[] { "1 target", "2 targets", "3 targets", "4 targets", "5 targets" }));
            RMenu.Add(new MenuBool("AutoFollowR", "Auto Follow R"));
            RMenu.Add(new MenuSlider("rTicks", "Ultimate ticks to count", 2, 1, 14));


            RSolo = RMenu.Add(new Menu("RSolo", "R one target"));
            RSolo.Add(new MenuKeyBind("forceR", "Force R on target", System.Windows.Forms.Keys.T, KeyBindType.Press));
            RSolo.Add(new MenuBool("rLastHit", "1 target ulti"));

            foreach (var hero in GameObjects.EnemyHeroes)
            {
                RSolo.Add(new MenuBool("RU" + hero.ChampionName, "Use R on: " + hero.ChampionName));
            }

            TestFeatures = MainMenu.Add(new Menu("TestF", "Test features"));
            TestFeatures.Add(new MenuBool("spPriority", "Prioritize kill over dmg"));



            // Harass
            HarassMenu = MainMenu.Add(new Menu("Harass", "Harass"));
            HarassMenu.Add(new MenuBool("harassUseQ", "Use Q", true));
            HarassMenu.Add(new MenuBool("harassUseE", "Use E", true));
            HarassMenu.Add(new MenuSlider("harassMana", "Mana usage in percent (%)", 30));
            HarassMenu.Add(new MenuSlider("eDistance", "Harass range with E", maxRangeE, rangeE, maxRangeE));
            HarassMenu.Add(new MenuKeyBind("harassActive", "Harass active", System.Windows.Forms.Keys.C, KeyBindType.Press));


            // WaveClear
            WaveClear = MainMenu.Add(new Menu("WaveClear", "WaveClear"));
            WaveClear.Add(new MenuBool("waveUseQ", "Use Q", true));
            WaveClear.Add(new MenuBool("waveUseE", "Use E", true));
            WaveClear.Add(new MenuSlider("waveNumE", "Minions to hit with E", 2, 1, 10));
            WaveClear.Add(new MenuSlider("waveMana", "Mana usage in percent (%)", 30));

            WaveClear.Add(new MenuKeyBind("waveActive", "WaveClear active", System.Windows.Forms.Keys.G, KeyBindType.Press));
            WaveClear.Add(new MenuKeyBind("jungleActive", "JungleClear active", System.Windows.Forms.Keys.G, KeyBindType.Press));


            // LastHit
            LastHit = MainMenu.Add(new Menu("LastHit", "LastHit"));
            LastHit.Add(new MenuKeyBind("waveUseQLH", "Use Q", System.Windows.Forms.Keys.A, KeyBindType.Press));

            // Flee
            FleeMenu = MainMenu.Add(new Menu("Flee", "Flee"));
            FleeMenu.Add(new MenuKeyBind("FleeActive", "Flee mode", System.Windows.Forms.Keys.Z, KeyBindType.Press));

            // Misc
            MiscMenu = MainMenu.Add(new Menu("Misc", "Misc"));
            MiscMenu.Add(new MenuBool("rInterrupt", "Use R to interrupt dangerous spells", true));
            MiscMenu.Add(new MenuBool("wInterrupt", "Use W to interrupt dangerous spells", true));
            MiscMenu.Add(new MenuBool("autoW", "Use W to continue CC", true));
            MiscMenu.Add(new MenuBool("miscGapcloser", "Use W against gapclosers", true));


            // Drawings
            DrawMenu = MainMenu.Add(new Menu("Drawings", "Drawings"));

            DrawMenu.Add(new MenuBool("drawRangeQ", "Q range", true));
            DrawMenu.Add(new MenuColor("QRangeC", "Q range", SharpDX.ColorBGRA.FromRgba(0xBF3F3FFF)));
            DrawMenu.Add(new MenuBool("drawRangeW", "W range", true));
            DrawMenu.Add(new MenuColor("WRangeC", "W range", SharpDX.ColorBGRA.FromRgba(0xBFBF3FFF)));
            DrawMenu.Add(new MenuBool("drawRangeE", "E range", true));
            DrawMenu.Add(new MenuColor("ERangeC", "E range", SharpDX.ColorBGRA.FromRgba(0x3FBFBFFF)));
            DrawMenu.Add(new MenuBool("drawRangeEMax", "E max range", true));
            DrawMenu.Add(new MenuColor("ERangeC2", "E max range", SharpDX.ColorBGRA.FromRgba(0xBF7F3FFF)));
            DrawMenu.Add(new MenuBool("drawRangeR", "R range", true));
            DrawMenu.Add(new MenuColor("RRangeC", "R range", SharpDX.ColorBGRA.FromRgba(0xBF3FBFFF)));
            DrawMenu.Add(new MenuBool("dmgdraw", "Draw dmg on healthbar"));
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);
        }
    }
}