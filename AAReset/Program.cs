#region
using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

#endregion

namespace AAReseter
{
    internal class Program
    {

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Obj_AI_Base.OnPlayAnimation += OnAnimation;
        }


        private static void OnAnimation(GameObject sender, GameObjectPlayAnimationEventArgs args)
        {
            if (sender.IsMe && (args.Animation == "Run" || args.Animation == "Idle") && Orbwalking.CanMove(0) == false)
            {
                Orbwalking.ResetAutoAttackTimer();
            }
        }
    }
}
