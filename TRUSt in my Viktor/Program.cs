using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LeagueSharp;
using LeagueSharp.Common;

using SharpDX;

using Color = System.Drawing.Color;

namespace Viktor
{
    public class Program
    {
        public const string CHAMP_NAME = "Viktor";
        private static readonly Obj_AI_Hero player = ObjectManager.Player;

        // Spells
        private static Spell Q, W, E, R;
        private static readonly int maxRangeE = 1275;
        private static readonly int lengthE = 750;
        private static readonly int speedE = 1200;
        private static readonly int rangeE = 525;
        private static int lasttick = 0;
        private static bool AttacksEnabled
        {
            get
            {
                if ((keyLinks["comboActive"].Value.Active) || (keyLinks["harassActive"].Value.Active))
                    return (!Q.IsReady() && !E.IsReady() || ((player.Mana / player.MaxMana) * 100 < 10));

                return true;
            }
        }
        // Menu
        public static MenuWrapper menu;

        // Menu links
        public static Dictionary<string, MenuWrapper.BoolLink> boolLinks = new Dictionary<string, MenuWrapper.BoolLink>();
        public static Dictionary<string, MenuWrapper.CircleLink> circleLinks = new Dictionary<string, MenuWrapper.CircleLink>();
        public static Dictionary<string, MenuWrapper.KeyBindLink> keyLinks = new Dictionary<string, MenuWrapper.KeyBindLink>();
        public static Dictionary<string, MenuWrapper.SliderLink> sliderLinks = new Dictionary<string, MenuWrapper.SliderLink>();

        public static void Main(string[] args)
        {
            // Register events
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
          
        }

        private static void OrbwalkingOnBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            args.Process = AttacksEnabled;
        }
        private static void Game_OnGameLoad(EventArgs args)
        {
            // Champ validation
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
            E.SetSkillshot(0, 90, speedE, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.25f, 450f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            // Create menu
            SetupMenu();

            // Register events
            Game.OnUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Orbwalking.OnNonKillableMinion += Orbwalking_OnNonKillableMinion;
            Orbwalking.BeforeAttack += OrbwalkingOnBeforeAttack;
        }
        private static void Orbwalking_OnNonKillableMinion(AttackableUnit minion)
        {
            if (keyLinks["waveUseQLH"].Value.Active && Q.IsReady() && Orbwalking.CanMove(0))
            {
                var target = minion as Obj_AI_Base;
                if (target != null && Q.IsKillable(target))
                {
                    // Cast since it's killable with E
                    Q.Cast(target);
                }
            }
        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            // Combo
            if (keyLinks["comboActive"].Value.Active)
                OnCombo();
            // Harass
            if (keyLinks["harassActive"].Value.Active)
                OnHarass();
            // WaveClear
            if (keyLinks["waveActive"].Value.Active)
                OnWaveClear();
            if (R.Instance.Name != "ViktorChaosStorm" && boolLinks["AutoFollowR"].Value && Environment.TickCount - lasttick > 0)
            {
                var stormT = TargetSelector.GetTarget(player, 1100, TargetSelector.DamageType.Magical);
                if (stormT != null)
                {
                    R.Cast(stormT.ServerPosition);
                    lasttick = Environment.TickCount + 500;
                }
            }
        }

        private static void OnCombo()
        {
            bool useQ = boolLinks["comboUseQ"].Value && Q.IsReady();
            bool useW = boolLinks["comboUseW"].Value && W.IsReady();
            bool useE = boolLinks["comboUseE"].Value && E.IsReady();
            bool useR = boolLinks["comboUseR"].Value && R.IsReady();

            if (useQ)
            {
                var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                if (target != null)
                    Q.Cast(target);
            }

            if (useE)
            {
                var target = TargetSelector.GetTarget(maxRangeE, TargetSelector.DamageType.Magical);
                if (target != null)
                    PredictCastE(target);
            }
            if (useW)
            {
                var t = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);

                if (t != null)
                {
                    if (t.Path.Count() < 2)
                    {
                        if (t.HasBuffOfType(BuffType.Slow))
                        {
                            if (W.GetPrediction(t).Hitchance >= HitChance.VeryHigh)
                                if (W.Cast(t) == Spell.CastStates.SuccessfullyCasted)
                                    return;
                        }
                        if (t.CountEnemiesInRange(250) > 2)
                        {
                            if (W.GetPrediction(t).Hitchance >= HitChance.VeryHigh)
                                if (W.Cast(t) == Spell.CastStates.SuccessfullyCasted)
                                    return;
                        }
                        if (player.Position.Distance(t.ServerPosition) < player.Position.Distance(t.Position))
                        {
                            if (W.Cast(t) == Spell.CastStates.SuccessfullyCasted)
                                return;
                        }
                    }
                }
            }
            if (useR && R.Instance.Name == "ViktorChaosStorm")
            {
                var t = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);

                if (t != null)
                {
                    if (t.Health < Damage.GetSpellDamage(player, t, SpellSlot.R) && t.HealthPercent > 5 && boolLinks["rLastHit"].Value && !Q.IsReady() && !E.IsReady())
                        R.Cast(t);
                }
                foreach (var unit in HeroManager.Enemies.Where(h => h.IsValidTarget(R.Range)))
                {
                    R.CastIfWillHit(unit, sliderLinks["HitR"].Value.Value);
                }
            }
           
        }

        private static void OnHarass()
        {
                // Mana check
                if ((player.Mana / player.MaxMana) * 100 < sliderLinks["harassMana"].Value.Value)
                    return;
                bool useE = boolLinks["harassUseE"].Value && E.IsReady();
                bool useQ = boolLinks["harassUseQ"].Value && Q.IsReady();
                if (useQ)
                {
                    var qtarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
                    if (qtarget != null)
                        Q.Cast(qtarget);
                }
                if (useE)
                {
                    var target = TargetSelector.GetTarget(maxRangeE, TargetSelector.DamageType.Magical);

                    if (target != null)
                        PredictCastE(target);
                }
        }

        private static void OnWaveClear()
        {
            // Mana check
            if ((player.Mana / player.MaxMana) * 100 < sliderLinks["waveMana"].Value.Value)
                return;

            bool useQ = boolLinks["waveUseQ"].Value && Q.IsReady();
            bool useE = boolLinks["waveUseE"].Value && E.IsReady();

            if (useQ)
            {
                foreach (var minion in MinionManager.GetMinions(player.Position, player.AttackRange))
                {
                    if (Q.IsKillable(minion) && !Orbwalking.CanAttack() && Orbwalking.CanMove(0))
                    {
                        Q.Cast(minion);
                        break;
                    }
                }
            }

            if (useE)
                PredictCastMinionE(sliderLinks["waveNumE"].Value.Value + 1);
        }

        private static bool PredictCastMinionE(int requiredHitNumber = -1)
        {
            int hitNum = 0;
            Vector2 startPos = new Vector2(0, 0);
            Vector2 endPos = new Vector2(0, 0);
            foreach (var minion in MinionManager.GetMinions(player.Position, rangeE))
            {
                var farmLocation = GetBestLaserFarmLocation(minion.Position.To2D(), (from mnion in MinionManager.GetMinions(minion.Position, lengthE) select mnion.Position.To2D()).ToList<Vector2>(), E.Width, lengthE);
                if (farmLocation.MinionsHit > hitNum)
                {
                    hitNum = farmLocation.MinionsHit;
                    startPos = minion.Position.To2D();
                    endPos = farmLocation.Position;
                }
            }

            if (startPos.X != 0 && startPos.Y != 0)
                return PredictCastMinionE(startPos, requiredHitNumber);
            return false;
        }
        public static MinionManager.FarmLocation GetBestLaserFarmLocation(Vector2 sourcepos, List<Vector2> minionPositions, float width, float range)
        {
            var result = new Vector2();
            var minionCount = 0;
            var startPos = sourcepos;

            var max = minionPositions.Count;
            for (var i = 0; i < max; i++)
            {
                for (var j = 0; j < max; j++)
                {
                    if (minionPositions[j] != minionPositions[i])
                    {
                        minionPositions.Add((minionPositions[j] + minionPositions[i]) / 2);
                    }
                }
            }

            foreach (var pos in minionPositions)
            {
                if (pos.Distance(startPos, true) <= range * range)
                {
                    var endPos = startPos + range * (pos - startPos).Normalized();

                    var count =
                        minionPositions.Count(pos2 => pos2.Distance(startPos, endPos, true, true) <= width * width);

                    if (count >= minionCount)
                    {
                        result = endPos;
                        minionCount = count;
                    }
                }
            }

            return new MinionManager.FarmLocation(result, minionCount);
        }

        private static bool PredictCastMinionE(Vector2 fromPosition, int requiredHitNumber = 1)
        {
            var farmLocation = GetBestLaserFarmLocation(fromPosition, MinionManager.GetMinionsPredictedPositions(MinionManager.GetMinions(fromPosition.To3D(), lengthE), E.Delay, E.Width, speedE, fromPosition.To3D(), lengthE, false, SkillshotType.SkillshotLine), E.Width, lengthE);

            if (farmLocation.MinionsHit >= requiredHitNumber)
            {
                CastE(fromPosition, farmLocation.Position);
                return true;
            }

            return false;
        }

        private static void PredictCastE(Obj_AI_Hero target)
        {
            // Helpers
            bool inRange = Vector2.DistanceSquared(target.ServerPosition.To2D(), player.Position.To2D()) < E.Range * E.Range;
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
                if (Vector2.DistanceSquared(champ.ServerPosition.To2D(), player.Position.To2D()) < E.Range * E.Range)
                    innerChamps.Add(champ);
                else
                    outerChamps.Add(champ);
            }

            // Minions
            var nearMinions = MinionManager.GetMinions(player.Position, maxRangeE);
            var innerMinions = new List<Obj_AI_Base>();
            var outerMinions = new List<Obj_AI_Base>();
            foreach (var minion in nearMinions)
            {
                if (Vector2.DistanceSquared(minion.ServerPosition.To2D(), player.Position.To2D()) < E.Range * E.Range)
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
                        if (prediction.Hitchance == HitChance.High && Vector2.DistanceSquared(pos1.To2D(), prediction.CastPosition.To2D()) < (E.Range * E.Range) * 0.8)
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
                    // Try casting on minion
                    if (!PredictCastMinionE(pos1.To2D()))
                        // Cast it directly
                        CastE(pos1, E.GetPrediction(target).CastPosition);

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
                var targets = (from champ in nearChamps where Vector2.DistanceSquared(champ.ServerPosition.To2D(), startPoint.To2D()) < startPointRadius * startPointRadius && Vector2.DistanceSquared(player.Position.To2D(), champ.ServerPosition.To2D()) < rangeE * rangeE select champ).ToList();
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
                    var minionTargets = (from minion in nearMinions where Vector2.DistanceSquared(minion.ServerPosition.To2D(), startPoint.To2D()) < startPointRadius * startPointRadius && Vector2.DistanceSquared(player.Position.To2D(), minion.ServerPosition.To2D()) < rangeE * rangeE select minion).ToList();
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
                if (prediction.Hitchance == HitChance.High)
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

        private static void OnInterruptableTarget(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (args.DangerLevel >= Interrupter2.DangerLevel.High)
            {
                var useW = boolLinks["wInterrupt"].Value;
                var useR = boolLinks["rInterrupt"].Value;

                if (useW && W.IsReady() && unit.IsValidTarget(W.Range) &&
                    (Game.Time + 1.5 + W.Delay) >= args.EndTime)
                {
                    if (W.Cast(unit) == Spell.CastStates.SuccessfullyCasted)
                        return;
                }
                else if (useR && unit.IsValidTarget(R.Range) && R.Instance.Name == "ViktorChaosStorm")
                {
                    R.Cast(unit);
                }
            }
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (boolLinks["miscGapcloser"].Value && W.IsInRange(gapcloser.End))
                W.Cast(gapcloser.End.To2D(), true);
        }
        private static void AutoW()
        {
            if (!W.IsReady() || !boolLinks["autoW"].Value)
                return;

            var tPanth = HeroManager.Enemies.Find(h => h.IsValidTarget(W.Range) && h.HasBuff("Pantheon_GrandSkyfall_Jump", true));
            if (tPanth != null)
            {
                if (W.Cast(tPanth) == Spell.CastStates.SuccessfullyCasted)
                    return;
            }

            foreach (var enemy in HeroManager.Enemies.Where(h => h.IsValidTarget(W.Range)))
            {
                if (enemy.HasBuff("rocketgrab2"))
                {
                    var t = HeroManager.Allies.Find(h => h.BaseSkinName.ToLower() == "blitzcrank" && h.Distance(player) < W.Range);
                    if (t != null)
                    {
                        if (W.Cast(t) == Spell.CastStates.SuccessfullyCasted)
                            return;
                    }
                }
                if (enemy.HasBuffOfType(BuffType.Stun) || enemy.HasBuffOfType(BuffType.Snare) ||
                         enemy.HasBuffOfType(BuffType.Charm) || enemy.HasBuffOfType(BuffType.Fear) ||
                         enemy.HasBuffOfType(BuffType.Taunt) || enemy.HasBuffOfType(BuffType.Suppression) ||
                         enemy.IsStunned || enemy.IsRecalling())
                {
                    if (W.Cast(enemy) == Spell.CastStates.SuccessfullyCasted)
                        return;
                }
                if (W.GetPrediction(enemy).Hitchance == HitChance.Immobile)
                {
                    if (W.Cast(enemy) == Spell.CastStates.SuccessfullyCasted)
                        return;
                }
            }
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            // All circles
            foreach (var circle in circleLinks.Values.Select(link => link.Value))
            {
                if (circle.Active)
                    Render.Circle.DrawCircle(player.Position, circle.Radius, circle.Color);
            }
        }

        private static void ProcessLink(string key, object value)
        {
            if (value is MenuWrapper.BoolLink)
                boolLinks.Add(key, value as MenuWrapper.BoolLink);
            else if (value is MenuWrapper.CircleLink)
                circleLinks.Add(key, value as MenuWrapper.CircleLink);
            else if (value is MenuWrapper.KeyBindLink)
                keyLinks.Add(key, value as MenuWrapper.KeyBindLink);
            else if (value is MenuWrapper.SliderLink)
                sliderLinks.Add(key, value as MenuWrapper.SliderLink);
        }

        private static void SetupMenu()
        {
            menu = new MenuWrapper("TRUSt in my " + CHAMP_NAME);

            // Combo
            var subMenu = menu.MainMenu.AddSubMenu("Combo");
            ProcessLink("comboUseQ", subMenu.AddLinkedBool("Use Q"));
            ProcessLink("comboUseW", subMenu.AddLinkedBool("Use W"));
            ProcessLink("comboUseE", subMenu.AddLinkedBool("Use E"));
            ProcessLink("comboUseR", subMenu.AddLinkedBool("Use R"));
            ProcessLink("HitR", subMenu.AddLinkedSlider("Ultimate to hit", 3, 1, 5));
            ProcessLink("rLastHit", subMenu.AddLinkedBool("1 target ulti"));
            ProcessLink("AutoFollowR", subMenu.AddLinkedBool("Auto Follow R"));
            ProcessLink("comboActive", subMenu.AddLinkedKeyBind("Combo active", 32, KeyBindType.Press));

            // Harass
            subMenu = menu.MainMenu.AddSubMenu("Harass");
            ProcessLink("harassUseQ", subMenu.AddLinkedBool("Use Q"));
            ProcessLink("harassUseE", subMenu.AddLinkedBool("Use E"));
            ProcessLink("harassMana", subMenu.AddLinkedSlider("Mana usage in percent (%)", 30));
            ProcessLink("harassActive", subMenu.AddLinkedKeyBind("Harass active", 'C', KeyBindType.Press));

            // WaveClear
            subMenu = menu.MainMenu.AddSubMenu("WaveClear");
            ProcessLink("waveUseQ", subMenu.AddLinkedBool("Use Q"));
            ProcessLink("waveUseE", subMenu.AddLinkedBool("Use E"));
            ProcessLink("waveNumE", subMenu.AddLinkedSlider("Minions to hit with E", 2, 1, 10));
            ProcessLink("waveMana", subMenu.AddLinkedSlider("Mana usage in percent (%)", 30));
            ProcessLink("waveActive", subMenu.AddLinkedKeyBind("WaveClear active", 'V', KeyBindType.Press));

            subMenu = menu.MainMenu.AddSubMenu("LastHit");
            ProcessLink("waveUseQLH", subMenu.AddLinkedKeyBind("Use Q", 'A', KeyBindType.Press));   

            // Misc
            subMenu = menu.MainMenu.AddSubMenu("Misc");
            ProcessLink("rInterrupt", subMenu.AddLinkedBool("Use R to interrupt dangerous spells"));
            ProcessLink("wInterrupt", subMenu.AddLinkedBool("Use W to interrupt dangerous spells"));
            ProcessLink("autoW", subMenu.AddLinkedBool("Use W to continue CC"));
            ProcessLink("miscGapcloser", subMenu.AddLinkedBool("Use W against gapclosers"));

            // Drawings
            subMenu = menu.MainMenu.AddSubMenu("Drawings");
            ProcessLink("drawRangeQ", subMenu.AddLinkedCircle("Q range", true, Color.FromArgb(150, Color.IndianRed), Q.Range));
            ProcessLink("drawRangeW", subMenu.AddLinkedCircle("W range", true, Color.FromArgb(150, Color.IndianRed), W.Range));
            ProcessLink("drawRangeE", subMenu.AddLinkedCircle("E range", false, Color.FromArgb(150, Color.DarkRed), E.Range));
            ProcessLink("drawRangeEMax", subMenu.AddLinkedCircle("E max range", true, Color.FromArgb(150, Color.OrangeRed), maxRangeE));
            ProcessLink("drawRangeR", subMenu.AddLinkedCircle("R range", false, Color.FromArgb(150, Color.Red), R.Range));
        
        }
    }
}