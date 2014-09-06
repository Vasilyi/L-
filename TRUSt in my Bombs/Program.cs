#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using BuffLib;
#endregion

namespace TRUStInMyBombs
{
    public class Program
    {

        public static Menu Config;
        private static Obj_AI_Hero Player;


        //Spells
        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell Q2;
        public static Spell W;
        public static Spell E;
        public static Spell R;



        public class Spells
        {
            public static float qRange = 850f;
            public static float qBounceRange = 650f;
            public static float wRange = 1000f;
            public static float eRange = 900f;
            public static float rRange = 5300f;
        }


        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
            Console.WriteLine("TRUStInMyBombs LOADED");
            BuffMngr.BuffMngrInit();
            BuffMngr.OnGainBuff += BuffGained;
        }
        private static void BuffGained(Obj_AI_Base target, Obj_AI_Base source,  BuffMngr.OnGainBuffArgs args)
        {

        }
      
        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;


            Q = new Spell(SpellSlot.Q, Spells.qRange);
            Q2 = new Spell(SpellSlot.Q, Spells.qRange+Spells.qBounceRange);
            W = new Spell(SpellSlot.W, Spells.wRange);
            E = new Spell(SpellSlot.E, Spells.eRange);
            R = new Spell(SpellSlot.R, Spells.rRange);


            Q.SetSkillshot(0.25f, 60f, 1750, true, SkillshotType.SkillshotCircle);
            Q2.SetSkillshot(0.7f, 60f, 1200, true, SkillshotType.SkillshotLine);
            //W.SetSkillshot(0.7f, 125f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.5f, 0f, 1750f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(1f, 550f, 1750f, false, SkillshotType.SkillshotCircle);

            //Create the menu
            Config = new Menu("TRUStInMyBombs", "mainmenu", true);
            
            Config.AddSubMenu(new Menu("HotKeys:", "hotkeys"));
            Config.SubMenu("hotkeys").AddItem(new MenuItem("ComboKey", "Combo!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("hotkeys").AddItem(new MenuItem("HarassKey", "Harrass").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("hotkeys").AddItem(new MenuItem("FarmKeyFreeze", "Farm Freeze").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("hotkeys").AddItem(new MenuItem("FarmKeyClear", "Farm Clear").SetValue(new KeyBind("H".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Auto Ultimate Logic:", "ultlogic"));
            Config.SubMenu("ultlogic").AddItem(new MenuItem("useR", "Use - Mega Inferno Bomb (AUTO)").SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
            Config.SubMenu("ultlogic").AddItem(new MenuItem("enemysforR", "Minimal Enemys Amount")).SetValue(new Slider(3, 5, 1));

            Config.AddSubMenu(new Menu("Misc:", "misc"));
            Config.SubMenu("misc").AddItem(new MenuItem("interrupt", "Interrupt Spells").SetValue(true));

            Config.AddSubMenu(new Menu("KillSteal:", "killsteal"));
            Config.SubMenu("killsteal").AddItem(new MenuItem("ksR", "KS - Mega Inferno Bomb").SetValue(true));
            Config.SubMenu("killsteal").AddItem(new MenuItem("ksRamount", "Minimal Enemys Amount")).SetValue(new Slider(1000, 5300, 1));
            Config.SubMenu("killsteal").AddItem(new MenuItem("ksAll", "KS - Everything").SetValue(true));

            Config.AddSubMenu(new Menu("Auto Farm:", "autofarm"));
            Config.SubMenu("autofarm").AddItem(new MenuItem("ksAll", "Use - Bouncing Bomb").SetValue(true));
            Config.SubMenu("autofarm").AddItem(new MenuItem("ksAll", "Use - Hexplosive Minefield").SetValue(true));

            Config.AddSubMenu(new Menu("Satchel Jumping Options:", "satchel"));
            Config.SubMenu("satchel").AddItem(new MenuItem("satchelDraw", "Draw satchel places").SetValue(true));
            Config.SubMenu("satchel").AddItem(new MenuItem("satchelDrawdistance", "Don't draw circles if the distance >")).SetValue(new Slider(2000, 10000, 1));
            Config.SubMenu("satchel").AddItem(new MenuItem("satchelJump", "Jump Key").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Combo Options:", "combospells"));
            Config.SubMenu("combospells").AddItem(new MenuItem("UseI", "Use Ignite if enemy is killable").SetValue(true));
            Config.SubMenu("combospells").AddItem(new MenuItem("dfg", "Use DFG in full combo").SetValue(true));
            Config.SubMenu("combospells").AddItem(new MenuItem("useB", "Use - Bouncing Bomb").SetValue(true));
            Config.SubMenu("combospells").AddItem(new MenuItem("useE", "Use - Hexplosive Minefield").SetValue(true));
            Config.SubMenu("combospells").AddItem(new MenuItem("useR", "Use - Mega Inferno Bomb").SetValue(true));
            Config.SubMenu("combospells").AddItem(new MenuItem("useRcombo", "Use - Bouncing Bomb").SetValue(true));

            Config.AddSubMenu(new Menu("Harass Options:", "harassspells"));
            Config.SubMenu("harassspells").AddItem(new MenuItem("useBHarass", "Use - Bouncing Bomb").SetValue(true));
            Config.SubMenu("harassspells").AddItem(new MenuItem("useEHarass", "Use - Hexplosive Minefield").SetValue(true));

            Config.AddSubMenu(new Menu("Draw Options:", "drawing"));
            Config.SubMenu("drawing").AddItem(new MenuItem("noDraw", "Disable - Drawing").SetValue(true));
            Config.SubMenu("drawing").AddItem(new MenuItem("drawDmg", "Draw - Damage Marks").SetValue(true));
            Config.SubMenu("drawing").AddItem(new MenuItem("drawF", "Draw - Furthest Spell Available").SetValue(true));
            Config.SubMenu("drawing").AddItem(new MenuItem("drawB", "Draw - Bouncing Bomb").SetValue(true));
            Config.SubMenu("drawing").AddItem(new MenuItem("drawQ", "Draw - Bomb (without bounce)").SetValue(true));
            Config.SubMenu("drawing").AddItem(new MenuItem("drawW", "Draw - Satchel Charge").SetValue(true));
            Config.SubMenu("drawing").AddItem(new MenuItem("drawE", "Draw - Hexplosive Minefield").SetValue(true));
            Config.SubMenu("drawing").AddItem(new MenuItem("drawR", "Draw - Mega Inferno Bomb").SetValue(true));
        }

        public static void Game_OnGameUpdate(EventArgs args)
        {

        }
    }
}