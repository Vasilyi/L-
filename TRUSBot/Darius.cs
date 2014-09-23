#region
using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
#endregion

namespace TRUSDominion
{
    internal class Darius
    {
        private static readonly List<Spell> SpellList = new List<Spell>();
        private static Spell _q, _w, _e, _r;

        public static SpellSlot IgniteSlot;
        public static Items.Item Hydra;
        public static Items.Item Tiamat;


        public static void Game_OnGameLoad(EventArgs args)
        {
            _q = new Spell(SpellSlot.Q, 425);
            _w = new Spell(SpellSlot.W, 145);
            _e = new Spell(SpellSlot.E, 540);
            _r = new Spell(SpellSlot.R, 460);

            SpellList.Add(_q);
            SpellList.Add(_w);
            SpellList.Add(_e);
            SpellList.Add(_r);

            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
            Tiamat = new Items.Item(3077, 375);
            Hydra = new Items.Item(3074, 375);
           
            Game.OnGameUpdate += Game_OnGameUpdate;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
                ExecuteSkills();
        }

        private static void Orbwalking_AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
        {
                _w.Cast();
                Items.UseItem(Items.HasItem(3077) ? 3077 : 3074);
        }

        private static void CastR(Obj_AI_Base target)
        {
            if (!target.IsValidTarget(_r.Range) || !_r.IsReady()) return;

            if (!(DamageLib.getDmg(target, DamageLib.SpellType.R, DamageLib.StageType.FirstDamage) > target.Health))
            {
                foreach (var buff in target.Buffs)
                {
                    if (buff.Name == "dariushemo")
                    {
                        if (DamageLib.getDmg(target, DamageLib.SpellType.R, DamageLib.StageType.FirstDamage) *
                            (1 + buff.Count / 5) - 1 > target.Health)
                        {
                            _r.CastOnUnit(target, true);
                        }
                    }
                }
            }
            else if (DamageLib.getDmg(target, DamageLib.SpellType.R, DamageLib.StageType.FirstDamage) - 15 >
                     target.Health)
            {
                _r.CastOnUnit(target, true);
            }
        }

   

        private static void ExecuteSkills()
        {
            var target = SimpleTs.GetTarget(_e.Range, SimpleTs.DamageType.Physical);
            if (target == null) return;

            if (_e.IsReady() && ObjectManager.Player.Distance(target) <= _e.Range)
                _e.Cast(target.ServerPosition);

            if (_q.IsReady() && ObjectManager.Player.Distance(target) <= _q.Range)
                _q.Cast();

            if (_r.IsReady() && ObjectManager.Player.Distance(target) <= _r.Range)
                CastR(target);

            if (_r.IsReady()) return;
            if (IgniteSlot != SpellSlot.Unknown &&
                ObjectManager.Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready &&
                DamageLib.getDmg(target, DamageLib.SpellType.IGNITE) - 5 > target.Health)
                ObjectManager.Player.SummonerSpellbook.CastSpell(IgniteSlot, target);
        }
    }
}