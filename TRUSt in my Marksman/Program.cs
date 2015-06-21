using System;
using LeagueSharp.Common;

namespace TRUStinMarksman
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += GameOnOnGameLoad;
        }

        private static void GameOnOnGameLoad(EventArgs args)
        {
            TRUStinMarksman.Load();
        }
    }
}