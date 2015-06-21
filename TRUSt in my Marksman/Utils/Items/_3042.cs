using System;
using LeagueSharp;
using LeagueSharp.Common;

namespace TRUStinMarksman.Utils.Items
{
    internal class _3042 : Item
    {
        public _3042()
        {
            Obj_AI_Base.OnProcessSpellCast += OnSpellCast;
        }

        internal override string Name
        {
            get { return "Muramana"; }
        }

        internal override int Id
        {
            get { return 3042; }
        }

        internal static bool Canmuramana;
        public override void OnUpdate()
        {
            if (!Canmuramana)
            {
                var manamune = TRUStinMarksman.Player.GetSpellSlot("Muramana");
                if (manamune != SpellSlot.Unknown && TRUStinMarksman.Player.HasBuff("Muramana"))
                {
                    TRUStinMarksman.Player.Spellbook.CastSpell(manamune);
                }
            }
        }

        public override void Use()
        {
            if (Canmuramana)
            {
                var manamune = TRUStinMarksman.Player.GetSpellSlot("Muramana");
                if (manamune != SpellSlot.Unknown && !TRUStinMarksman.Player.HasBuff("Muramana"))
                {
                    TRUStinMarksman.Player.Spellbook.CastSpell(manamune);
                    Utility.DelayAction.Add(400, () => Canmuramana = false);
                }
            }
        }

        internal static void OnSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe)
            {
                return;
            }

            if (args.SData.HaveHitEffect)
                Canmuramana = true;

            if (args.SData.IsAutoAttack() &&
               (TRUStinMarksman.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo ||
                args.Target.Type == GameObjectType.obj_AI_Hero))
            {
                Canmuramana = true;
            }

            else
            {
                Utility.DelayAction.Add(400, () => Canmuramana = false);
            }

        }
    }
}
