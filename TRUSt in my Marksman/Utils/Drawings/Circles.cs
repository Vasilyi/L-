using System;
using System.Collections.Generic;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;

namespace TRUStinMarksman.Utils.Drawings
{
    public static class Circles
    {
        private static readonly Dictionary<string, object> RangeCircles = new Dictionary<string, object>();

        static Circles()
        {
            Drawing.OnDraw += DrawingOnOnDraw;
        }

        private static void DrawingOnOnDraw(EventArgs args)
        {
            foreach (var circle in RangeCircles)
            {
                var c = TRUStinMarksman.Config.SubMenu("Drawings").Item(circle.Key, true).GetValue<Circle>();
                var range = 0f;

                if (circle.Value is Spell)
                {
                    range = ((Spell) circle.Value).Range;
                }

                if (c.Active)
                {
                    Render.Circle.DrawCircle(TRUStinMarksman.Player.Position, range, c.Color);
                }
            }
        }

        internal static void Add(string name, object spellOrCallBack)
        {
            TRUStinMarksman.Config.SubMenu("Drawings")
                .AddItem(new MenuItem(name, name, true).SetValue(new Circle(true, Color.White)));

            RangeCircles.Add(name, spellOrCallBack);
        }
    }
}