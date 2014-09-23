#region

/*
 * Credits to:
 * Eskor
 * Roach_
 * Both for helping me alot doing this Assembly and start On L# 
 * lepqm for cleaning my shit up
 * iMeh Code breaker 101
 */
using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

#endregion

namespace TRUSDominion
{
    internal class Annie
    {
        public const string CharName = "Annie";
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static Spell R1;

        public static float DoingCombo = 0;

        public static SpellSlot IgniteSlot;
        public static SpellSlot FlashSlot;

        private static int StunCount
        {
            get
            {
                foreach (var buff in
                    ObjectManager.Player.Buffs.Where(
                        buff => buff.Name == "pyromania" || buff.Name == "pyromania_particle"))
                {
                    switch (buff.Name)
                    {
                        case "pyromania":
                            return buff.Count;
                        case "pyromania_particle":
                            return 4;
                    }
                }

                return 0;
            }
        }


        public static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.ChampionName != CharName)
            {
                return;
            }

            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
            FlashSlot = ObjectManager.Player.GetSpellSlot("SummonerFlash");

            Q = new Spell(SpellSlot.Q, 625f);
            W = new Spell(SpellSlot.W, 625f);
            E = new Spell(SpellSlot.E, float.MaxValue);
            R = new Spell(SpellSlot.R, 600f);
            R1 = new Spell(SpellSlot.R, 900f);

            W.SetSkillshot(0.60f, 625f, float.MaxValue, false, SkillshotType.SkillshotCone);
            R.SetSkillshot(0.20f, 200f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R1.SetSkillshot(0.25f, 200f, float.MaxValue, false, SkillshotType.SkillshotCircle);

            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(R);
            SpellList.Add(R1);

            Game.OnGameUpdate += OnGameUpdate;
            GameObject.OnCreate += OnCreateObject;
            Orbwalking.BeforeAttack += Orbwalking_BeforeAttack;
        }


        private static void OnCreateObject(GameObject sender, EventArgs args)
        {
            if (sender.IsAlly || !(sender is Obj_SpellMissile))
            {
                return;
            }

            var missile = (Obj_SpellMissile)sender;
            if (!(missile.SpellCaster is Obj_AI_Hero) || !(missile.Target.IsMe))
            {
                return;
            }

            if (E.IsReady())
            {
                E.Cast();
            }
            else if (!ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(missile.SpellCaster.NetworkId).IsMelee())
            {
                var ecd = (int)(ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).CooldownExpires - Game.Time) *
                          1000;
                if ((int)Vector3.Distance(missile.Position, ObjectManager.Player.ServerPosition) /
                    ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(missile.SpellCaster.NetworkId)
                        .BasicAttack.MissileSpeed * 1000 > ecd)
                {
                    Utility.DelayAction.Add(ecd, () => E.Cast());
                }
            }
        }

        private static void OnGameUpdate(EventArgs args)
        {
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var flashRtarget = SimpleTs.GetTarget(900, SimpleTs.DamageType.Magical);
                Combo(target, flashRtarget, true);
        }

        private static void Orbwalking_BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            args.Process = Environment.TickCount - DoingCombo > 500;
        }

       private static void Combo(Obj_AI_Base target, Obj_AI_Base flashRtarget, bool ulti)
        {
            Console.WriteLine("[" + Game.Time + "]Combo started");
            if ((target == null && flashRtarget == null) || Environment.TickCount - DoingCombo < 500 ||
                (!Q.IsReady() && !W.IsReady() && !R.IsReady()))
            {
                return;
            }

            Console.WriteLine("[" + Game.Time + "]Target acquired");
            if (target != null)
            {
                Items.UseItem(3128, target);
            }

            switch (StunCount)
            {
                case 3:
                    Console.WriteLine("[" + Game.Time + "]Case 3");
                    if (Q.IsReady())
                    {
                        DoingCombo = Environment.TickCount;
                        Q.CastOnUnit(target, false);
                        Utility.DelayAction.Add(
                            (int)(ObjectManager.Player.Distance(target) / Q.Speed * 1000 - 100 - Game.Ping / 2.0),
                            () =>
                            {
                                if (R.IsReady() &&
                                    !(DamageLib.getDmg(target, DamageLib.SpellType.R) * 0.6 > target.Health))
                                {
                                    R.Cast(target, false, true);
                                }
                            });
                    }
                    else if (W.IsReady())
                    {
                        DoingCombo = Environment.TickCount;
                    }

                    W.Cast(target, false, true); //stack only goes up after 650 secs

                    Utility.DelayAction.Add(
                        650 - 100 - Game.Ping / 2, () =>
                        {
                            if (R.IsReady() && !(DamageLib.getDmg(target, DamageLib.SpellType.R) * 0.6 > target.Health))
                            {
                                R.Cast(target, false, true);
                            }

                            DoingCombo = Environment.TickCount;
                        });

                    break;
                case 4:
                    Console.WriteLine("[" + Game.Time + "]Case 4");
                    if (R.IsReady() && !(DamageLib.getDmg(target, DamageLib.SpellType.R) * 0.6 > target.Health) && ulti)
                    {
                        R.Cast(target, false, true);
                    }

                    if (W.IsReady())
                    {
                        W.Cast(target, false, true);
                    }

                    if (Q.IsReady())
                    {
                        Q.Cast(target, false, true);
                    }

                    break;
                default:
                    Console.WriteLine("[" + Game.Time + "]Case default");
                    if (Q.IsReady())
                    {
                        Q.CastOnUnit(target, false);
                    }

                    if (W.IsReady())
                    {
                        W.Cast(target, false, true);
                    }

                    break;
            }

            if (IgniteSlot != SpellSlot.Unknown &&
                ObjectManager.Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready &&
                ObjectManager.Player.Distance(target) < 600 &&
                DamageLib.getDmg(target, DamageLib.SpellType.IGNITE) > target.Health)
            {
                ObjectManager.Player.SummonerSpellbook.CastSpell(IgniteSlot, target);
            }
        }

      
        private static int GetEnemiesInRange(Vector3 pos, float range)
        {
            //var Pos = pos;
            return
                ObjectManager.Get<Obj_AI_Hero>()
                    .Where(hero => hero.Team != ObjectManager.Player.Team)
                    .Count(hero => Vector3.Distance(pos, hero.ServerPosition) <= range);
        }
    }
}
