using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using System.IO;
using System.Diagnostics;
using System.Windows;
using SharpDX;


namespace TRUSDominion
{
    class Summoners
    {
        public static SpellSlot Barrier = SpellSlot.Unknown;
        public static SpellSlot Heal = SpellSlot.Unknown;
        public static SpellSlot Dot = SpellSlot.Unknown;
        public static SpellSlot Exhaust = SpellSlot.Unknown;

        public static SpellSlot GetSummonerSpellSlot(String name)
        {
            var spell = ObjectManager.Player.SummonerSpellbook.Spells.FirstOrDefault(x => x.Name.ToLower() == name);
            return spell != null ? spell.Slot : SpellSlot.Unknown;
        }

        public static void summonersinit()
        {
            Barrier = GetSummonerSpellSlot("summonerbarrier");
            Heal = GetSummonerSpellSlot("summonerheal");
            Dot = GetSummonerSpellSlot("summonerdot");
            Exhaust = GetSummonerSpellSlot("summonerexhaust");
        }

        public static void SummonersTick(EventArgs args)
        {
            Check_Barrier();
            Check_Heal();
            Check_Dot();
            Check_Exhaust();
        }
        private static void Check_Barrier()
        {
                var target = SimpleTs.GetTarget(1000, SimpleTs.DamageType.Physical);
                if (Barrier == SpellSlot.Unknown ||
                    ObjectManager.Player.SummonerSpellbook.CanUseSpell(Barrier) !=
                    SpellState.Ready || ObjectManager.Player.Health / ObjectManager.Player.MaxHealth * 100 >= 50 || target == null)
                {
                    return;
                }
                ObjectManager.Player.SummonerSpellbook.CastSpell(Barrier);
        }

        private static void Check_Heal()
        {
            var target = SimpleTs.GetTarget(1000, SimpleTs.DamageType.Physical);
            if (Heal == SpellSlot.Unknown ||
                ObjectManager.Player.SummonerSpellbook.CanUseSpell(Heal) !=
                SpellState.Ready || ObjectManager.Player.Health / ObjectManager.Player.MaxHealth * 100 >= 50 || target == null)
            {
                return;
            }
            ObjectManager.Player.SummonerSpellbook.CastSpell(Heal);
        }

        private static void Check_Dot()
        {
            if (Dot == SpellSlot.Unknown ||
                    ObjectManager.Player.SummonerSpellbook.CanUseSpell(Dot) !=
                    SpellState.Ready)
                    return;
            const int range = 600;
            foreach (Obj_AI_Hero hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy && hero.IsValidTarget(range)))
            {
                if (DamageLib.getDmg(hero, DamageLib.SpellType.IGNITE) >= hero.Health)
                    {
                        ObjectManager.Player.SummonerSpellbook.CastSpell(Dot, hero);
                        return;
                    }
            }
        }

        private static void Check_Exhaust()
        {
            var champions = ObjectManager.Get<Obj_AI_Hero>().ToList();
                if (Exhaust == SpellSlot.Unknown || ObjectManager.Player.SummonerSpellbook.CanUseSpell(Exhaust) !=
                    SpellState.Ready)
                    return;
                const int range = 550;

                Obj_AI_Hero maxDpsHero = champions.Where(hero => hero.IsEnemy && hero.IsValidTarget(range + 200)).OrderByDescending(x => x.BaseAttackDamage * x.AttackSpeedMod).FirstOrDefault();
                if (maxDpsHero == null)
                    return;
        }
    }
}