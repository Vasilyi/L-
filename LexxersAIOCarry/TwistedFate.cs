using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace UltimateCarry
{
	class TwistedFate : Champion
	{
		public Spell Q;
		public Spell W;
		public Spell R;

		public int CardPickTick;
		public TwistedFate()
		{
			LoadMenu();
			LoadSpells();

			Drawing.OnDraw += Drawing_OnDraw;
			Game.OnGameUpdate += Game_OnGameUpdate;
			PluginLoaded();
		}

		private void LoadMenu()
		{
			Program.Menu.AddSubMenu(new Menu("TeamFight", "TeamFight"));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useQ_TeamFight", "Use Q").SetValue(new StringList(new[] { "Not", "OnStun", "Always" }, 2)));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useW_TeamFight", "Use W").SetValue(true));

			Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useQ_Harass", "Use Q").SetValue(new StringList(new[] { "Not", "OnStun", "Always" }, 1)));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useW_Harass", "Use W").SetValue(true));

			Program.Menu.AddSubMenu(new Menu("LaneClear", "LaneClear"));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useQ_LaneClear", "Use Q").SetValue(true));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useW_LaneClear", "Use W").SetValue(true));

			Program.Menu.AddSubMenu(new Menu("LastHit", "LastHit"));
			Program.Menu.SubMenu("LastHit").AddItem(new MenuItem("useW_LastHit", "Use W").SetValue(true));

			Program.Menu.AddSubMenu(new Menu("Passive", "Passive"));
			Program.Menu.SubMenu("Passive").AddItem(new MenuItem("use_CardPick", "Cardpick").SetValue(new StringList(new[] { "Smart", "Gold", "Blue", "Red" })));

			Program.Menu.AddSubMenu(new Menu("Drawing", "Drawing"));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Q", "Draw Q").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_R", "Draw R").SetValue(true));

		}

		private void LoadSpells()
		{
			Q = new Spell(SpellSlot.Q, 1450);
			Q.SetSkillshot(0.25f, 40, 1000, false, SkillshotType.SkillshotLine);

			W = new Spell(SpellSlot.W, 1000);
			W.SetSkillshot(0.3f, 80f, 1600, true, SkillshotType.SkillshotLine);
			
			R = new Spell(SpellSlot.R, 5500);
		}

		private void Game_OnGameUpdate(EventArgs args)
		{
			if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Name == "gate")
				CastUltGoldCard();
			
			switch(Program.Orbwalker.ActiveMode)
			{
				case Orbwalking.OrbwalkingMode.Combo:
					UseQTeamfight();
					if(Program.Menu.Item("useW_TeamFight").GetValue<bool>())
						CastW();
					break;
				case Orbwalking.OrbwalkingMode.Mixed:
					UseQHarass();
					if(Program.Menu.Item("useW_Harass").GetValue<bool>())
						CastW();
					break;
				case Orbwalking.OrbwalkingMode.LaneClear:
					if (Program.Menu.Item("useQ_LaneClear").GetValue<bool>())
						Cast_BasicLineSkillshot_AOE_Farm(Q);
					if(Program.Menu.Item("useW_LaneClear").GetValue<bool>())
						CastW();
					break;
				case Orbwalking.OrbwalkingMode.LastHit:
					if(Program.Menu.Item("useW_LastHit").GetValue<bool>())
						CastW();
					break;
			}
		}

		private void CastUltGoldCard()
		{
			if(!W.IsReady())
				return;
			if(Environment.TickCount - CardPickTick < 250)
				return;
			CardPickTick = Environment.TickCount;
			if(ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "PickACard")
				W.Cast();
			PickGold() ;
		}

		private void CastW()
		{
			if (!W.IsReady())
				return;
			if (Environment.TickCount - CardPickTick < 250)
				return;
			CardPickTick = Environment.TickCount;
			if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "PickACard")
				W.Cast();
			PickaCard();
		}

		private void PickaCard()
		{
			switch(Program.Menu.Item("use_CardPick").GetValue<StringList>().SelectedIndex)
			{
				case 0:
					SmartPick();
					return;
				case 1:
					PickGold();
					break;
				case 2:
					PickBlue();
					break;
				case 3:
					PickRed();
					break;
			}
		}

		private void SmartPick()
		{
			switch (Program.Orbwalker.ActiveMode)
			{
				case Orbwalking.OrbwalkingMode.Mixed:
					if(ObjectManager.Player.Mana / ObjectManager.Player.MaxMana * 100 <= 70)
						PickBlue();
					else
					{
						PickRed();
						PickGold();
					}
					break;
				case Orbwalking.OrbwalkingMode.Combo:
					if(ObjectManager.Player.Mana / ObjectManager.Player.MaxMana * 100 <= 20)
						PickBlue();
					else
						PickGold();
					break;
				case Orbwalking.OrbwalkingMode.LaneClear:
					if(ObjectManager.Player.Mana / ObjectManager.Player.MaxMana * 100 <= 60)
						PickBlue();
					else
						PickRed();
					break;
				case Orbwalking.OrbwalkingMode.LastHit:
					PickBlue();
					break;
			}
		}

		private void PickGold()
		{
			if(ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "goldcardlock")
				W.Cast();
		}

		private void PickRed()
		{
			if(ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "redcardlock")
				W.Cast();
		}

		private void PickBlue()
		{
			if(ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "bluecardlock")
				W.Cast();
		}

		private void UseQHarass()
		{
			if(!Q.IsReady())
				return;
			switch(Program.Menu.Item("useQ_Harass").GetValue<StringList>().SelectedIndex)
			{
				case 0:
					return;
				case 1:
					foreach(var enemy in Program.Helper.EnemyTeam.Where(hero => (hero.HasBuffOfType(BuffType.Snare) || hero.HasBuffOfType(BuffType.Stun) && hero.IsValidTarget(Q.Range))))
					{
						Q.Cast(enemy, Packets());
						return;
					}
					break;
				case 2:
					Cast_BasicLineSkillshot_Enemy(Q, SimpleTs.DamageType.Magical);
					break;
			}
		}

		private void UseQTeamfight()
		{
			if(!Q.IsReady())
				return;
			switch(Program.Menu.Item("useQ_TeamFight").GetValue<StringList>().SelectedIndex)
			{
				case 0:
					return;
				case 1:
					foreach(var enemy in Program.Helper.EnemyTeam.Where(hero => (hero.HasBuffOfType(BuffType.Snare) || hero.HasBuffOfType(BuffType.Stun) && hero.IsValidTarget(Q.Range))))
					{
						Q.Cast(enemy, Packets());
						return;
					}
					break;
				case 2:
					Cast_BasicLineSkillshot_Enemy(Q, SimpleTs.DamageType.Magical);
					break;
			}
		}

		private void Drawing_OnDraw(EventArgs args)
		{
			if(Program.Menu.Item("Draw_Disabled").GetValue<bool>())
				return;

			if(Program.Menu.Item("Draw_Q").GetValue<bool>())
				if(Q.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);

			if(Program.Menu.Item("Draw_R").GetValue<bool>())
				if (R.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
				
		}


	}
}
