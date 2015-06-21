using LeagueSharp;
using LeagueSharp.Common;

namespace TRUStinMarksman.Utils.Items
{
    internal class _3153 : Item
    {
        internal override int Id
        {
            get { return 3153; }
        }

        internal override string Name
        {
            get { return "Blade of the Ruined King"; }
        }

        internal override float Range
        {
            get { return 550; }
        }

        public override void Use()
        {
            var target = TRUStinMarksman.Orbwalker.GetTarget();

            if (!target.IsValid<Obj_AI_Hero>())
            {
                return;
            }

            var targetHero = (Obj_AI_Hero) target;

            if (targetHero.IsValidTarget(Range) &&
                TRUStinMarksman.Player.Health + TRUStinMarksman.Player.GetItemDamage(targetHero, Damage.DamageItems.Botrk) <
                TRUStinMarksman.Player.MaxHealth)
            {
                LeagueSharp.Common.Items.UseItem(Id, targetHero);
            }
        }
    }
}