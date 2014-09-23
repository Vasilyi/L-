using System;

using LeagueSharp;
using LeagueSharp.Common;
namespace TRUSDominion
{
    class Unknown
    {

        public static Obj_AI_Base Player = ObjectManager.Player; // Instead of typing ObjectManager.Player you can just type Player
        public static Spell Q, W, E, R;
        public static Items.Item hydra = new Items.Item(3074, 400);
        public static Items.Item tiamat = new Items.Item(3077, 400);
        public static Items.Item BoRK = new Items.Item(3153, 400);

        public static void Game_OnGameLoad(EventArgs args)
        {

            Q = new Spell(SpellSlot.Q, 500);
            W = new Spell(SpellSlot.W, 500);
            E = new Spell(SpellSlot.E, 500);
            R = new Spell(SpellSlot.R, 400);
            Game.OnGameUpdate += Game_OnGameUpdate; // adds OnGameUpdate (Same as onTick in bol)
        }

        static void Game_OnGameUpdate(EventArgs args)
        {
            Combo();
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
                Q.Cast(target);
                Q.Cast();

            }
            if (target.IsValidTarget(E.Range) && W.IsReady())
            {
                W.Cast(target);
                W.Cast();
            }
            if (target.IsValidTarget(E.Range) && E.IsReady())
            {
                E.Cast(target);
                E.Cast();
            }
            if (target.IsValidTarget(R.Range) && R.IsReady() && Player.Distance(target) >= R.Range)
            {
                R.Cast(target);
                R.Cast();
            }
        }

    }
}
