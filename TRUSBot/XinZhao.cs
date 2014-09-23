using System;

using LeagueSharp;
using LeagueSharp.Common;
namespace TRUSDominion
{
    class XinZhao
    {
        public static string ChampName = "XinZhao";
        public static Obj_AI_Base Player = ObjectManager.Player; // Instead of typing ObjectManager.Player you can just type Player
        public static Spell Q, W, E, R;
        public static Items.Item hydra = new Items.Item(3074, 400);
        public static Items.Item tiamat = new Items.Item(3077, 400);
        public static Items.Item BoRK = new Items.Item(3153, 400);

        public static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.BaseSkinName != ChampName) return;

            Q = new Spell(SpellSlot.Q, 0);
            W = new Spell(SpellSlot.W, 0);
            E = new Spell(SpellSlot.E, 600);
            R = new Spell(SpellSlot.R, 180);
            Game.OnGameUpdate += Game_OnGameUpdate; // adds OnGameUpdate (Same as onTick in bol)

            Game.PrintChat(ChampName + " loaded! By Animated ;)");
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
                Combo();
                KillSteal();
        }



        public static void Combo()
        {
            var target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);
            if (target == null) return;

            if (target.IsValidTarget(hydra.Range) && hydra.IsReady())
                hydra.Cast();

            if (target.IsValidTarget(tiamat.Range) && tiamat.IsReady())
                tiamat.Cast();

            if (target.IsValidTarget(BoRK.Range) && BoRK.IsReady())
                BoRK.Cast(target);

            if (target.IsValidTarget(E.Range) && Q.IsReady())
            {
                Q.Cast();

            }
            if (target.IsValidTarget(E.Range) && W.IsReady())
            {
                W.Cast();
            }
            if (target.IsValidTarget(E.Range) && E.IsReady())
            {
                E.Cast(target);
            }
            //if (target.IsValidTarget(R.Range) && R.IsReady() && Player.Distance(target)>= R.Range)
            //  if (target.Health < Rdmg)
            // {
            //  R.Cast();
        }

        public static void KillSteal()
        {
            var target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Physical);
            var igniteDmg = DamageLib.getDmg(target, DamageLib.SpellType.IGNITE);
            var RDmg = DamageLib.getDmg(target, DamageLib.SpellType.R);

            {
                if (target != null && R.IsReady() && target.IsValidTarget(180))
                {
                    if (target.Health < RDmg)
                    {
                        R.Cast();
                    }
                }
            }




        }

    }
}
