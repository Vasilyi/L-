#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace Ryze
{
    internal class Program
    {
        public const string ChampionName = "Ryze";

        //Orbwalker instance
        public static Orbwalking.Orbwalker Orbwalker;

        //Spells
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Items.Item TearoftheGoddess = new Items.Item(3070, 0);
        public static Items.Item ArchangelsStaff = new Items.Item(3003, 0);
        //Menu
        public static Menu Config;
        public static double myQCooldown;
        public static string lasttext;

        private static Obj_AI_Hero Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;

        }

        private static void Game_OnGameLoad(EventArgs args)
        {

            Player = ObjectManager.Player;
            if (Player.BaseSkinName != ChampionName) return;
            Obj_AI_Base.OnProcessSpellCast += ObjAiHeroOnOnProcessSpellCast;
            //Create the spells
           
            Q = new Spell(SpellSlot.Q, 900);
            Q.SetSkillshot(0.25f, Q.Instance.SData.LineWidth, Q.Instance.SData.MissileSpeed, true, SkillshotType.SkillshotLine);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 600);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);

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
            Config.SubMenu("Combo")
                .AddItem(new MenuItem("RootComboActive", "Root Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            Config.SubMenu("Harass")
                .AddItem(
                    new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Farm", "Farm"));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("UseQFarm", "Use Q").SetValue(
                        new StringList(new[] { "Freeze", "LaneClear", "Both", "No" }, 2)));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("UseWFarm", "Use W").SetValue(
                        new StringList(new[] { "Freeze", "LaneClear", "Both", "No" }, 3)));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("UseEFarm", "Use E").SetValue(
                        new StringList(new[] { "Freeze", "LaneClear", "Both", "No" }, 1)));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("FreezeActive", "Freeze!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Farm")
                .AddItem(
                    new MenuItem("LaneClearActive", "LaneClear!").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));

            Config.AddSubMenu(new Menu("JungleFarm", "JungleFarm"));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseQJFarm", "Use Q").SetValue(true));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseWJFarm", "Use W").SetValue(true));
            Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseEJFarm", "Use E").SetValue(true));
            Config.SubMenu("JungleFarm")
                .AddItem(
                    new MenuItem("JungleFarmActive", "JungleFarm!").SetValue(new KeyBind("V".ToCharArray()[0],
                        KeyBindType.Press)));

            //Damage after combo:
            var dmgAfterComboItem = new MenuItem("DamageAfterCombo", "Draw damage after a rotation").SetValue(true);
            Utility.HpBarDamageIndicator.DamageToUnit += hero => (float)(Player.GetSpellDamage(hero, SpellSlot.Q) * 2 + Player.GetSpellDamage(hero, SpellSlot.W) + Player.GetSpellDamage(hero, SpellSlot.E));
            Utility.HpBarDamageIndicator.Enabled = dmgAfterComboItem.GetValue<bool>();
            dmgAfterComboItem.ValueChanged += delegate(object sender, OnValueChangeEventArgs eventArgs)
            {
                Utility.HpBarDamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
            };

            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings")
                .AddItem(new MenuItem("QRange", "Q range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("WRange", "W range").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings")
                .AddItem(
                    new MenuItem("ERange", "E range").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(dmgAfterComboItem);
            Config.AddToMainMenu();

            //Add the events we are going to use:
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnGameUpdate;
            Orbwalking.BeforeAttack += OrbwalkingOnBeforeAttack;
        }


        private static void ObjAiHeroOnOnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender == null || !sender.IsMe)
            {
                return;
            }
            if ( PassiveCharged() && (args.SData.Name == "RyzeW" || args.SData.Name == "RyzeE" || args.SData.Name == "RyzeR" || args.SData.Name == "ryzerw" || args.SData.Name == "ryzere"))
            {
                myQCooldown = myQCooldown - (4*(1+Player.PercentCooldownMod));
            }
            else 
                if (args.SData.Name == "RyzeQ" || args.SData.Name == "ryzerq")
                {
                    myQCooldown = (4 * (1+Player.PercentCooldownMod));
                }

            Console.WriteLine(args.SData.Name + " : " + Game.Time + " : " + (myQCooldown));
        }

        private static void OrbwalkingOnBeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
                args.Process = !(Q.IsReady() || W.IsReady() || E.IsReady() || Player.Distance(args.Target) >= 600);
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

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else if (Config.Item("RootComboActive").GetValue<KeyBind>().Active)
            {
                RootCombo();
            }
            else
            {
                if (Config.Item("HarassActive").GetValue<KeyBind>().Active)
                    Harass();

                var lc = Config.Item("LaneClearActive").GetValue<KeyBind>().Active;
                if (lc || Config.Item("FreezeActive").GetValue<KeyBind>().Active)
                    Farm(lc);

                if (Config.Item("JungleFarmActive").GetValue<KeyBind>().Active)
                    JungleFarm();
            }
            if (Q.IsReady() && ObjectManager.Player.InFountain() && (TearoftheGoddess.IsOwned(Player) || ArchangelsStaff.IsOwned(Player)))
            {
                Q.Cast(ObjectManager.Player, true, true);
            }
        }
        private static void DebugWrite(string text)
        {
            if (lasttext == text)
                return;
            Console.WriteLine(text);
            lasttext = text;
        }
        private static void Combo()
        {
            var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (target.CountEnemiesInRange(200) > 1 && (UltActive() || (R.Instance.Level > 0 && R.IsReady())))
            {
                AoeCombo();
                return;
            }
            if (qTarget != null && !Player.Spellbook.IsCastingSpell)
            {
                var collided = Q.GetPrediction(qTarget, false, -1f, new[] { CollisionableObjects.Minions });
                if (Q.IsReady())
                {
                    if (collided.Hitchance == HitChance.VeryHigh)
                    {
                        DebugWrite("High hitchance Q");
                        ObjectManager.Player.Spellbook.CastSpell(Q.Slot, collided.CastPosition, false);
                        return;
                    }
                    else if (collided.Hitchance == HitChance.Immobile)
                    {
                        DebugWrite("Immobile Q");
                        ObjectManager.Player.Spellbook.CastSpell(Q.Slot, collided.CastPosition, false);
                        return;
                    }
                    else if (PassiveCharged() && Player.ManaPercent > 30)
                    {
                        DebugWrite("PassiveCharged Q");
                        ObjectManager.Player.Spellbook.CastSpell(Q.Slot, collided.CastPosition, false);
                        return;
                    }

                }
                if (target == null)
                    return;
                if (W.IsReady())
                {
                    var spellnametext = "W";
                       
                    if (myQCooldown > 0.25)
                    {
                        W.CastOnUnit(target);
                        DebugWrite("Reducing Q cooldown with " + spellnametext);
                        return;
                    }
                    else if (collided.Hitchance == HitChance.Collision)
                    {
                        W.CastOnUnit(target);
                        DebugWrite("Q is in collision with " + spellnametext);
                        return;
                    }
                }
                if (E.IsReady())
                {
                    var spellnametext = "E";

                    if (myQCooldown > 0.25)
                    {
                        E.CastOnUnit(target);
                        DebugWrite("Reducing Q cooldown with " + spellnametext);
                        return;
                    }
                    else if (collided.Hitchance == HitChance.Collision)
                    {
                        E.CastOnUnit(target);
                        DebugWrite("Q is in collision with " + spellnametext);
                        return;
                    }
                }
                if (Q.IsReady())
                {
                    if (collided.Hitchance > HitChance.Medium)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(Q.Slot, collided.CastPosition, false);
                        DebugWrite("Medium chance Q");
                    }
                    if (PassiveCharged() && Player.ManaPercent > 30)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(Q.Slot, collided.CastPosition, false);
                        DebugWrite("Q TO PROC PASSIVE");
                    }
                }
            }
        }


        private static void AoeCombo()
        {
            var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);

            if (qTarget != null && !Player.Spellbook.IsCastingSpell)
            {
                var collided = Q.GetPrediction(qTarget, false, -1f, new[] { CollisionableObjects.Minions });
                if (target == null)
                    return;
                if (R.IsReady())
                {
                    R.Cast();
                }
                if (E.IsReady())
                {
                    var spellnametext = "E";

                        E.CastOnUnit(target);
                        DebugWrite("Reducing Q cooldown with " + spellnametext);
                        return;
                }

                if (Q.IsReady())
                {
                    if (collided.Hitchance == HitChance.VeryHigh)
                    {
                        DebugWrite("High hitchance Q");
                        ObjectManager.Player.Spellbook.CastSpell(Q.Slot, collided.CastPosition, false);
                        return;
                    }
                    else if (collided.Hitchance == HitChance.Immobile)
                    {
                        DebugWrite("Immobile Q");
                        ObjectManager.Player.Spellbook.CastSpell(Q.Slot, collided.CastPosition, false);
                        return;
                    }
                    else if (PassiveCharged() && Player.ManaPercent > 30)
                    {
                        DebugWrite("PassiveCharged Q");
                        ObjectManager.Player.Spellbook.CastSpell(Q.Slot, collided.CastPosition, false);
                        return;
                    }

                }
                if (W.IsReady())
                {
                    var spellnametext = "W";
                        W.CastOnUnit(target);
                        DebugWrite("Reducing Q cooldown with " + spellnametext);
                        return;
                }
              
                if (Q.IsReady())
                {
                    if (collided.Hitchance > HitChance.Medium)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(Q.Slot, collided.CastPosition, false);
                        DebugWrite("Medium chance Q");
                    }
                    if (PassiveCharged() && Player.ManaPercent > 30)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(Q.Slot, collided.CastPosition, false);
                        DebugWrite("Q TO PROC PASSIVE");
                    }
                }
            }
        }

        private static void RootCombo()
        {
            var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Magical);
            var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);


            if (qTarget != null && !Player.Spellbook.IsCastingSpell)
            {
                var collided = Q.GetPrediction(qTarget, false, -1f, new[] { CollisionableObjects.Minions });
                if (target == null)
                    return;
                if (W.IsReady())
                {
                    var spellnametext = "W";
                        W.CastOnUnit(target);
                        DebugWrite("Reducing Q cooldown with " + spellnametext);
                        return;
                }
                if (Q.IsReady())
                {
                    if (collided.Hitchance == HitChance.VeryHigh)
                    {
                        DebugWrite("High hitchance Q");
                        ObjectManager.Player.Spellbook.CastSpell(Q.Slot, collided.CastPosition, false);
                        return;
                    }
                    else if (collided.Hitchance == HitChance.Immobile)
                    {
                        DebugWrite("Immobile Q");
                        ObjectManager.Player.Spellbook.CastSpell(Q.Slot, collided.CastPosition, false);
                        return;
                    }
                    else if (PassiveCharged() && Player.ManaPercent > 30)
                    {
                        DebugWrite("PassiveCharged Q");
                        ObjectManager.Player.Spellbook.CastSpell(Q.Slot, collided.CastPosition, false);
                        return;
                    }

                }
                if (E.IsReady())
                {
                        E.CastOnUnit(target);
                        DebugWrite("Reducing W cooldown with " + "E");
                        return;
                }
                if (Q.IsReady())
                {
                    if (collided.Hitchance > HitChance.Medium)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(Q.Slot, collided.CastPosition, false);
                        DebugWrite("Medium chance Q");
                    }
                    if (PassiveCharged() && Player.ManaPercent > 30)
                    {
                        ObjectManager.Player.Spellbook.CastSpell(Q.Slot, collided.CastPosition, false);
                        DebugWrite("Q TO PROC PASSIVE");
                    }
                }
            }
        }


        private static bool PassiveCharged()
        {
            return Player.HasBuff("ryzepassivecharged");
        }

        private static bool UltActive()
        {
            return Player.HasBuff("ryzepassivecharged");
        }
        private static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            foreach (var buff in Player.Buffs)
            {
                Console.WriteLine("PLAYER : " + buff.Name);
            }
            if (target != null && Config.Item("UseQHarass").GetValue<bool>())
            {
                var collided = Q.GetPrediction(target, false, -1f, new[] { CollisionableObjects.Minions });
                if (collided.Hitchance > HitChance.High)
                {
                    ObjectManager.Player.Spellbook.CastSpell(Q.Slot, collided.CastPosition, false);
                }
            }
        }

        private static void Farm(bool laneClear)
        {
            if (!Orbwalking.CanMove(40)) return;
            var allMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range);
            var useQi = Config.Item("UseQFarm").GetValue<StringList>().SelectedIndex;
            var useWi = Config.Item("UseWFarm").GetValue<StringList>().SelectedIndex;
            var useEi = Config.Item("UseEFarm").GetValue<StringList>().SelectedIndex;
            var useQ = (laneClear && (useQi == 1 || useQi == 2)) || (!laneClear && (useQi == 0 || useQi == 2));
            var useW = (laneClear && (useWi == 1 || useWi == 2)) || (!laneClear && (useWi == 0 || useWi == 2));
            var useE = (laneClear && (useEi == 1 || useEi == 2)) || (!laneClear && (useEi == 0 || useEi == 2));

            if (useQ && Q.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget() &&
                        HealthPrediction.GetHealthPrediction(minion,
                            (int)(Player.Distance((AttackableUnit)minion) * 1000 / 1700)) <
                         Player.GetSpellDamage(minion, SpellSlot.Q))
                    {
                        var collided = Q.GetPrediction(minion, false, -1f, new[] { CollisionableObjects.Minions });
                        if (collided.Hitchance > HitChance.Medium)
                        {
                            ObjectManager.Player.Spellbook.CastSpell(Q.Slot, collided.CastPosition, false);
                            return;
                        }
                    }
                }
            }
            else if (useW && W.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget(W.Range) &&
                        minion.Health < Player.GetSpellDamage(minion, SpellSlot.Q))
                    {
                        W.CastOnUnit(minion);
                        return;
                    }
                }
            }
            else if (useE && E.IsReady())
            {
                foreach (var minion in allMinions)
                {
                    if (minion.IsValidTarget(E.Range) &&
                        HealthPrediction.GetHealthPrediction(minion,
                            (int)(Player.Distance((AttackableUnit)minion) * 1000 / 1000)) <
                        Player.GetSpellDamage(minion, SpellSlot.Q) - 10)
                    {
                        E.CastOnUnit(minion);
                        return;
                    }
                }
            }

            if (laneClear)
            {
                foreach (var minion in allMinions)
                {
                    if (useQ)
                        Q.Cast(minion.Position);

                    if (useW)
                        W.CastOnUnit(minion);

                    if (useE)
                        E.CastOnUnit(minion);
                }
            }
        }

        private static void JungleFarm()
        {
            var mobs = MinionManager.GetMinions(Player.ServerPosition, W.Range,
                MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            var QMobs = MinionManager.GetMinions(Player.ServerPosition, Q.Range,
                MinionTypes.All,
                MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                if (mobs.Count > 0)
                {
                    if (mobs[0] == null || !mobs[0].IsValid)
                        return;
                    var temptarget = mobs[0];
                    if (!Player.Spellbook.IsCastingSpell)
                    {
                        var collided = Q.GetPrediction(temptarget, false, -1f, new[] { CollisionableObjects.Minions });
                        if (Q.IsReady())
                        {
                            if (collided.Hitchance == HitChance.VeryHigh)
                            {
                                DebugWrite("High hitchance Q");
                                ObjectManager.Player.Spellbook.CastSpell(Q.Slot, collided.CastPosition, false);
                                return;
                            }
                            else if (collided.Hitchance == HitChance.Immobile)
                            {
                                DebugWrite("Immobile Q");
                                ObjectManager.Player.Spellbook.CastSpell(Q.Slot, collided.CastPosition, false);
                                return;
                            }
                            else if (PassiveCharged() && Player.ManaPercent > 30)
                            {
                                DebugWrite("PassiveCharged Q");
                                ObjectManager.Player.Spellbook.CastSpell(Q.Slot, collided.CastPosition, false);
                                return;
                            }

                        }
                        if (W.IsReady())
                        {
                            var spellnametext = "W";

                            if (myQCooldown > 0.25)
                            {
                                W.CastOnUnit(temptarget);
                                DebugWrite("Reducing Q cooldown with " + spellnametext);
                                return;
                            }
                            else if (collided.Hitchance == HitChance.Collision)
                            {
                                W.CastOnUnit(temptarget);
                                DebugWrite("Q is in collision with " + spellnametext);
                                return;
                            }
                        }
                        if (E.IsReady())
                        {
                            var spellnametext = "E";

                            if (myQCooldown > 0.25)
                            {
                                E.CastOnUnit(temptarget);
                                DebugWrite("Reducing Q cooldown with " + spellnametext);
                                return;
                            }
                            else if (collided.Hitchance == HitChance.Collision)
                            {
                                E.CastOnUnit(temptarget);
                                DebugWrite("Q is in collision with " + spellnametext);
                                return;
                            }
                        }
                        if (Q.IsReady())
                        {
                            if (collided.Hitchance > HitChance.Medium)
                            {
                                ObjectManager.Player.Spellbook.CastSpell(Q.Slot, collided.CastPosition, false);
                                DebugWrite("Medium chance Q");
                            }
                            if (PassiveCharged() && Player.ManaPercent > 30)
                            {
                                ObjectManager.Player.Spellbook.CastSpell(Q.Slot, collided.CastPosition, false);
                                DebugWrite("Q TO PROC PASSIVE");
                            }
                        }
                    }
                }

        }


    }
}
