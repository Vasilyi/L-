#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace TRUSDominion
{
    class Ryze
    {
        public const string ChampionName = "Ryze";

        //Spells
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        //Menu

        private static Obj_AI_Hero Player;

        public static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.BaseSkinName != ChampionName) return;
            //Create the spells
            Q = new Spell(SpellSlot.Q, 625);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 600);
            R = new Spell(SpellSlot.R, 0);
            Game.OnGameUpdate += Game_OnGameUpdate;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
                Combo();
        }

        private static void Combo()
        {
            
            var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var qCd = Player.Spellbook.GetSpell(SpellSlot.Q).CooldownExpires - Game.Time;

            if (target != null)
            {
                if (Player.Distance(target) <= 600)
                {
                    if (Player.Distance(target) <= 550)
                    {
                        Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                    }
                    else
                    {
                        Player.IssueOrder(GameObjectOrder.MoveTo, target.Position);
                    }

                    if (Player.Distance(target) >= 575 && W.IsReady() && target.Path.Count() > 0 &&
                        target.Path[0].Distance(Player.ServerPosition) >
                        Player.Distance(target))
                    {
                        W.CastOnUnit(target);
                    }
                    else if (Q.IsReady())
                    {
                        Q.CastOnUnit(target);
                    }
                    else
                    {
                        if (qCd > 1.25f)
                        {
                            if (R.IsReady())
                            {
                                R.Cast();
                            }
                            else if (W.IsReady())
                            {
                                W.CastOnUnit(target);
                            }
                            else if (E.IsReady())
                            {
                                E.CastOnUnit(target);
                            }
                        }
                    }
                }
                else if (DamageLib.getDmg(target, DamageLib.SpellType.Q) > target.Health)
                {
                    Q.CastOnUnit(target);
                }
            }
        }
    }
}