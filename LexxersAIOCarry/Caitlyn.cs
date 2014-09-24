using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace UltimateCarry
{
	class Caitlyn : Champion 
	{
		public Spell Q;
		public Spell W;
		public Spell E;
		public Spell R;

		public Caitlyn()
		{
			LoadMenu();
			LoadSpells();

			//Drawing.OnDraw += Drawing_OnDraw;
			//Game.OnGameUpdate += Game_OnGameUpdate;
			PluginLoaded();
		}

		private void LoadMenu()
		{
			Program.Menu.AddSubMenu(new Menu("TeamFight", "TeamFight"));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useQ_TeamFight", "Use Q").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useW_TeamFight", "Use W").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useR_TeamFight", "Use R").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("minimumRRange_Teamfight", "R Range min.").SetValue(new Slider(500, 900, 0)));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("minimumRHit_Teamfight", "R Will Hit min.").SetValue(new Slider(2, 5, 1)));

			Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useQ_Harass", "Use Q").SetValue(true));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useW_Harass", "Use W").SetValue(true));
			AddManaManager("Harass", 40);

			Program.Menu.AddSubMenu(new Menu("LaneClear", "LaneClear"));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useQ_LaneClear_minion", "Use Q Minion").SetValue(true));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useQ_LaneClear_enemy", "Use Q Enemy").SetValue(true));
			AddManaManager("LaneClear", 20);

			Program.Menu.AddSubMenu(new Menu("LastHit", "LastHit"));
			Program.Menu.SubMenu("LastHit").AddItem(new MenuItem("useQ_LastHit", "Use Q").SetValue(true));
			AddManaManager("LastHit", 60);

			Program.Menu.AddSubMenu(new Menu("Drawing", "Drawing"));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Q", "Draw Q").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_W", "Draw W").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_E", "Draw E").SetValue(true));

		}

		private void LoadSpells()
		{
			Q = new Spell(SpellSlot.Q, 1200);
			Q.SetSkillshot(0.25f, 60f, 2000f, true, SkillshotType.SkillshotLine);

			W = new Spell(SpellSlot.W, 1050);
			W.SetSkillshot(0.25f, 80f, 2000f, false, SkillshotType.SkillshotLine);

			E = new Spell(SpellSlot.E, 475);
			E.SetSkillshot(0.25f, 80f, 1600f, false, SkillshotType.SkillshotCircle);

			R = new Spell(SpellSlot.R, 3000);
			R.SetSkillshot(1f, 160f, 2000f, false, SkillshotType.SkillshotLine);

		}

	}


}
