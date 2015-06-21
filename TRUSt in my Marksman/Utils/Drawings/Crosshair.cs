using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using TRUStinMarksman.Properties;
using SharpDX;

namespace TRUStinMarksman.Utils.Drawings
{
    internal static class Crosshair
    {
        private static readonly Dictionary<string, SpellSlot> SupportedHeros = new Dictionary<string, SpellSlot>
        {
            { "Caitlyn", SpellSlot.R },
            { "Ezreal", SpellSlot.R },
            { "Graves", SpellSlot.R },
            { "Jinx", SpellSlot.R },
            { "Varus", SpellSlot.Q },
            { "Ashe", SpellSlot.R }
        };

        private static Vector2 DrawPosition
        {
            get
            {
                return KillableEnemy != null ? new Vector2(
                    Drawing.WorldToScreen(KillableEnemy.Position).X - KillableEnemy.BoundingRadius / 2f,
                    Drawing.WorldToScreen(KillableEnemy.Position).Y - KillableEnemy.BoundingRadius / 0.5f) : Vector2.Zero;
            }
        }

        private static bool DrawSprite
        {
            get
            {
                return KillableEnemy != null && KillableEnemy.Position.IsOnScreen() &&
                       SupportedHeros[TRUStinMarksman.Player.ChampionName].IsReady() &&
                       TRUStinMarksman.Config.SubMenu("Drawings").Item("Crosshair", true).GetValue<bool>();
            }
        }

        private static Obj_AI_Hero KillableEnemy
        {
            get
            {
                return
                    ObjectManager.Get<Obj_AI_Hero>()
                        .OrderBy(hero => hero.Health)
                        .FirstOrDefault(
                            hero =>
                                hero.IsValidTarget(3000f) &&
                                hero.Health <=
                                TRUStinMarksman.Player.GetSpellDamage(hero, SupportedHeros[TRUStinMarksman.Player.ChampionName]));
            }
        }

        internal static void Load()
        {
            if (SupportedHeros.All(hero => hero.Key != TRUStinMarksman.Player.ChampionName))
            {
                return;
            }

            new Render.Sprite(Resources.Crosshair, new Vector2())
            {
                PositionUpdate = () => DrawPosition,
                Scale = new Vector2(1f, 1f),
                VisibleCondition = sender => DrawSprite
            }.Add();

            TRUStinMarksman.Config.SubMenu("Drawings").AddItem(new MenuItem("Crosshair", "Crosshair", true).SetValue(true));
        }
    }
}