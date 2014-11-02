using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace Spin2Win
{

    internal class Spin2Win
    {
        public static Obj_AI_Hero Player;
        public static Menu Config;
        public static double direction = 0;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += OnGameLoad;
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {

            if (Config.Item("SpinningOn").GetValue<KeyBind>().Active)
            {
                double spinX = 100 * Math.Sin(Math.PI * direction / Config.Item("spinspeed").GetValue<Slider>().Value);
                double spinZ = 100 * Math.Cos(Math.PI * direction / Config.Item("spinspeed").GetValue<Slider>().Value);
                Vector3 moveposition = new Vector3(Player.ServerPosition.X + (float)spinX, Player.ServerPosition.Y +  (float)spinZ, Player.ServerPosition.Z);
                Player.IssueOrder(GameObjectOrder.MoveTo, moveposition);
                direction++;
                
            }
        }

        private static void OnGameLoad(EventArgs args)
        {

            Game.OnGameUpdate += Game_OnGameUpdate;
            Config = new Menu("Spin2Win", "Spin2Win", true);
            Config.AddToMainMenu();
            Config.AddSubMenu(new Menu("Spin Settings", "Spin"));
            Config.SubMenu("Spin").AddItem(new MenuItem("SpinningOn", "Spin!").SetValue(new KeyBind(32, KeyBindType.Press)));
            Config.SubMenu("Spin")
                .AddItem(new MenuItem("spinspeed", "Spin Speed"))
                .SetValue(new Slider(5, 1, 16));
            Player = ObjectManager.Player;
            Game.PrintChat("<font color='#F7A100'>Spin2Win</font>");
        }


    }
}