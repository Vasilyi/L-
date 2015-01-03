using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using System;
using System.Reflection;
using System.Linq;
namespace PerfectWard
{
    internal class PerfectWardTracker
    {
        private const int VK_LBUTTON = 1;
        private const int WM_KEYDOWN = 0x0100, WM_KEYUP = 0x0101, WM_CHAR = 0x0102, WM_SYSKEYDOWN = 0x0104, WM_SYSKEYUP = 0x0105, WM_MOUSEDOWN = 0x201;
        public static LeagueSharp.Common.Menu Config;
        public static float lastuseward = 0;
        public class Wardspoting
        {
            public static WardSpot _PutSafeWard;
        }

        public PerfectWardTracker()
        {
            Game.OnGameStart += OnGameStart;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += OnDraw;


            //Create the menu
            Config = new LeagueSharp.Common.Menu("PerfectWard", "PerfectWard", true);

            Config.AddSubMenu(new LeagueSharp.Common.Menu("Drawing:", "Drawing"));
            Config.SubMenu("Drawing").AddItem(new LeagueSharp.Common.MenuItem("drawplaces", "Draw ward places").SetValue(new Circle(true, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
            Config.SubMenu("Drawing").AddItem(new LeagueSharp.Common.MenuItem("drawDistance", "Don't draw if the distance >")).SetValue(new Slider(2000, 10000, 1));
            Config.SubMenu("Drawing").AddItem(new LeagueSharp.Common.MenuItem("placekey", "NormalWard Key").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Drawing").AddItem(new LeagueSharp.Common.MenuItem("placekeypink", "PinkWard Key").SetValue(new KeyBind("Z".ToCharArray()[0], KeyBindType.Press)));
            Config.AddToMainMenu();
        }


        void Game_OnGameUpdate(EventArgs args)
        {

            InventorySlot wardSpellSlot = null;
            if (Config.Item("placekey").GetValue<KeyBind>().Active)
            {
                wardSpellSlot = Items.GetWardSlot();
            }

          
            else if (Config.Item("placekeypink").GetValue<KeyBind>().Active)
            {
                wardSpellSlot = Ward.GetPinkSlot();
            }
            {
                if (wardSpellSlot == null || lastuseward + 1000 > Environment.TickCount)
                {
                    return;
                }
                Vector3? nearestWard = Ward.FindNearestWardSpot(Drawing.ScreenToWorld(Game.CursorPos.X, Game.CursorPos.Y));

                if (nearestWard != null)
                {
                    if (wardSpellSlot != null)
                    {
                        Console.WriteLine("putting ward");
                        ObjectManager.Player.Spellbook.CastSpell(wardSpellSlot.SpellSlot, (Vector3)nearestWard);
                        lastuseward = Environment.TickCount;
                    }
                }

                WardSpot nearestSafeWard = Ward.FindNearestSafeWardSpot(Drawing.ScreenToWorld(Game.CursorPos.X, Game.CursorPos.Y));

                if (nearestSafeWard != null)
                {
                    if (wardSpellSlot != null)
                    {
                        ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, nearestSafeWard.MovePosition);
                        Wardspoting._PutSafeWard = nearestSafeWard;
                    }
                }
            }


            if (Wardspoting._PutSafeWard != null && lastuseward + 1000 < Environment.TickCount)
            {
                wardSpellSlot = Items.GetWardSlot();
                if (Math.Sqrt(Math.Pow(Wardspoting._PutSafeWard.ClickPosition.X - ObjectManager.Player.Position.X, 2) + Math.Pow(Wardspoting._PutSafeWard.ClickPosition.Y - ObjectManager.Player.Position.Y, 2)) <= 640.0)
                {
                    if (Config.Item("placekey").GetValue<KeyBind>().Active)
                    {
                        
                        wardSpellSlot = Items.GetWardSlot();
                    }
                    else if (Config.Item("placekeypink").GetValue<KeyBind>().Active)
                    {
                        wardSpellSlot = Ward.GetPinkSlot();
                    }
                        if (wardSpellSlot != null)
                        {
                            Console.WriteLine("putting ward2");
                            ObjectManager.Player.Spellbook.CastSpell(wardSpellSlot.SpellSlot, (Vector3)Wardspoting._PutSafeWard.ClickPosition);
                            lastuseward = Environment.TickCount;

                        }
                        Wardspoting._PutSafeWard = null;
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
