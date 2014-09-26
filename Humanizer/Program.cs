#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
#endregion

namespace Humanizer
{
    public class Program
    {

        public static Menu Config;
        public static float lastmovement;

        public class LatestCast
        {
            public static float Tick = 0;
            public static float Timepass;
            public static double X;
            public static double Y;
            public static double Distance;
            public static double Delay;

        }

        private static Obj_AI_Hero Player;
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Console.WriteLine("Humanizer LOADED");
            LeagueSharp.Game.OnGameProcessPacket += PacketHandler;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            //Create the menu
            Config = new Menu("Humanizer", "Humanizer", true);

            Config.AddSubMenu(new Menu("Config", "Config"));
            Config.SubMenu("Config").AddItem(new MenuItem("delaytime", "Delay time for casts")).SetValue(new Slider(0, 10, 0));
            Config.SubMenu("Config").AddItem(new MenuItem("delaytimem", "Delay time for movements")).SetValue(new Slider(0, 10, 0));
            Config.AddToMainMenu();
        }
        private static void PacketHandler(GamePacketEventArgs args)
        {
            var Packetc = new GamePacket(args.PacketData);
            if (Packetc.Header == Packet.C2S.Cast.Header)
            {
                var decodedpacket = Packet.C2S.Cast.Decoded(args.PacketData);
                if (LatestCast.Tick + 500 < Environment.TickCount)
                {
                    LatestCast.Timepass = Environment.TickCount - LatestCast.Tick;
                    LatestCast.Distance = Math.Sqrt(Math.Pow(decodedpacket.ToX - LatestCast.X, 2) + Math.Pow(decodedpacket.ToY - LatestCast.Y, 2));
                    LatestCast.Delay = (LatestCast.Distance * 0.1 * Config.Item("delaytime").GetValue<Slider>().Value);
                    if (Environment.TickCount > LatestCast.Tick + LatestCast.Delay)
                    {
                        args.Process = false;
                    }
                    if (args.Process == true)
                    {
                        LatestCast.Tick = Environment.TickCount;
                        LatestCast.X = decodedpacket.ToX;
                        LatestCast.Y = decodedpacket.ToY;
                    }
                }
            }


            else if (Packetc.Header == Packet.C2S.Move.Header)
            {
                var decodedpacket = Packet.C2S.Cast.Decoded(args.PacketData);
                if (LatestCast.Tick + Config.Item("delaytime").GetValue<Slider>().Value*50 < Environment.TickCount)
                {
                    args.Process = false;
                }
                else
                {
                    args.Process = true;
                    lastmovement = Environment.TickCount;

                }
            }
        }
    }
}