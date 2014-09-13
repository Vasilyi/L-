using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace PerfectWard
{
    internal class PerfectWardTracker
    {
        private const int VK_LBUTTON = 1;
        private const int WM_KEYDOWN = 0x0100, WM_KEYUP = 0x0101, WM_CHAR = 0x0102, WM_SYSKEYDOWN = 0x0104, WM_SYSKEYUP = 0x0105, WM_MOUSEDOWN = 0x201;
        public static LeagueSharp.Common.Menu Config;
        public class Wardspoting
        {
            public static WardSpot _PutSafeWard;
        }

        public PerfectWardTracker()
        {
            Game.OnGameStart += OnGameStart;
            Game.OnWndProc += OnWndProc;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += OnDraw;


            //Create the menu
            Config = new LeagueSharp.Common.Menu("PerfectWard", "PerfectWard", true);

            Config.AddSubMenu(new LeagueSharp.Common.Menu("Drawing:", "Drawing"));
            Config.SubMenu("Drawing").AddItem(new LeagueSharp.Common.MenuItem("drawplaces", "Draw ward places").SetValue(new Circle(true, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
            Config.SubMenu("Drawing").AddItem(new LeagueSharp.Common.MenuItem("drawDistance", "Don't draw if the distance >")).SetValue(new Slider(2000, 10000, 1));
            Config.SubMenu("Drawing").AddItem(new LeagueSharp.Common.MenuItem("placekey", "Place Key").SetValue(new KeyBind(16, KeyBindType.Press)));
            Config.AddToMainMenu();
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            if (Wardspoting._PutSafeWard != null)
            {

                if (Math.Sqrt(Math.Pow(Wardspoting._PutSafeWard.ClickPosition.X - ObjectManager.Player.Position.X, 2) + Math.Pow(Wardspoting._PutSafeWard.ClickPosition.Y - ObjectManager.Player.Position.Y, 2)) <= 640.0)
                {
                    InventorySlot wardSpellSlot = Items.GetWardSlot();

                    if (wardSpellSlot != null)
                    {
                        wardSpellSlot.UseItem((Vector3)Wardspoting._PutSafeWard.ClickPosition);
                        
                    }
                    Wardspoting._PutSafeWard = null;
                }
            }
        }

        public void OnWndProc(WndEventArgs args)
        {
            if (args.Msg == WM_MOUSEDOWN)
            {
                if (args.WParam == VK_LBUTTON && Config.Item("placekey").GetValue<KeyBind>().Active)
                {
                    Vector3? nearestWard = Ward.FindNearestWardSpot(Drawing.ScreenToWorld(Game.CursorPos.X, Game.CursorPos.Y));

                    if (nearestWard != null)
                    {
                        InventorySlot wardSpellSlot = Items.GetWardSlot();
                         
                         if (wardSpellSlot != null)
                         {
                             wardSpellSlot.UseItem((Vector3)nearestWard);
                         } 
                    }

                    WardSpot nearestSafeWard = Ward.FindNearestSafeWardSpot(Drawing.ScreenToWorld(Game.CursorPos.X, Game.CursorPos.Y));

                    if (nearestSafeWard != null)
                    {
                        InventorySlot wardSpellSlot = Items.GetWardSlot();

                        if (wardSpellSlot != null)
                        {
                            ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, nearestSafeWard.MovePosition);
                            Wardspoting._PutSafeWard = nearestSafeWard;
                        }
                    }
                }
            }
        }

        private void OnGameStart(EventArgs args)
        {
            Game.PrintChat(
                string.Format(
                    "{0} v{1} loaded.",
                    Assembly.GetExecutingAssembly().GetName().Name,
                    Assembly.GetExecutingAssembly().GetName().Version
                    )
                );
        }

        private void OnDraw(EventArgs args)
        {
            if (Config.Item("drawplaces").GetValue<Circle>().Active)
            {
                Ward.DrawWardSpots();
                Ward.DrawSafeWardSpots();
            }
        }
    }
}
