using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;
using Menu = LeagueSharp.Common.Menu;
using MenuItem = LeagueSharp.Common.MenuItem;

namespace UltimateCarry
{
	class Elise : Champion 
	{

		public Spell QHuman;
		public Spell WHuman;
		public Spell EHuman;
		public Spell QSpider;
		public Spell WSpider;
		public Spell ESpider;
		public Spell R;

		public Elise()
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
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("Sorry", "Sorry cant let you"));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("Sorry1", "disable spells :P"));
			
			Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useQ_Harass", "Use Q Human").SetValue(true));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useW_Harass", "Use W Human").SetValue(true));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useE_Harass", "Use E Human").SetValue(true));
			AddManaManager("Harass",40);

			Program.Menu.AddSubMenu(new Menu("LaneClear", "LaneClear"));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useQ_LaneClear", "Use Q Human").SetValue(true));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useW_LaneClear", "Use W Human").SetValue(true));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useE_LaneClear", "Use E Human").SetValue(true));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useQ_LaneClear2", "Use Q Spider").SetValue(true));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useW_LaneClear2", "Use W Spider").SetValue(true));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useE_LaneClear2", "Use E Spider").SetValue(true));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("LaneClear_forms", "Switch Forms").SetValue(true));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("LaneClear_mana", "Use Spider below Mana").SetValue(new Slider(40)));

			Program.Menu.AddSubMenu(new Menu("Drawing", "Drawing"));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Q_Human", "Draw Q Human").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_W_Human", "Draw W Human").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_E_Human", "Draw E Human").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Q_Spider", "Draw Q Spider").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_E_Spider", "Draw E Spider").SetValue(true));
		}

		private void LoadSpells()
		{
			QHuman = new Spell(SpellSlot.Q, 625);

			WHuman = new Spell(SpellSlot.W, 950);
			WHuman.SetSkillshot(0.3f, 10, float.MaxValue, true, SkillshotType.SkillshotLine);

			EHuman = new Spell(SpellSlot.E, 1075);
			EHuman.SetSkillshot(0.25f, 70, float.MaxValue, true, SkillshotType.SkillshotLine);

			QSpider = new Spell(SpellSlot.Q,475);

			WSpider = new Spell(SpellSlot.W);

			ESpider = new Spell(SpellSlot.E, 1000);
			
			R = new Spell(SpellSlot.R);
		}

		private void Game_OnGameUpdate(EventArgs args)
		{		
			switch(Program.Orbwalker.ActiveMode)
			{
				case Orbwalking.OrbwalkingMode.Combo:
					Combo();
					break;
				case Orbwalking.OrbwalkingMode.Mixed:
					Harass();
					break;
				case Orbwalking.OrbwalkingMode.LaneClear:
					LaneClear();
					break;
			}
		}

		private void Drawing_OnDraw(EventArgs args)
		{
		
			if(Program.Menu.Item("Draw_Disabled").GetValue<bool>())
				return;

			if(Humanform())
			{
				if(Program.Menu.Item("Draw_Q_Human").GetValue<bool>())
					if(QHuman.Level > 0)
						Utility.DrawCircle(ObjectManager.Player.Position, QHuman.Range, QHuman.IsReady() ? Color.Green : Color.Red);

				if(Program.Menu.Item("Draw_W_Human").GetValue<bool>())
					if(WHuman.Level > 0)
						Utility.DrawCircle(ObjectManager.Player.Position, WHuman.Range, WHuman.IsReady() ? Color.Green : Color.Red);

				if(Program.Menu.Item("Draw_E_Human").GetValue<bool>())
					if(EHuman.Level > 0)
						Utility.DrawCircle(ObjectManager.Player.Position, EHuman.Range, EHuman.IsReady() ? Color.Green : Color.Red);

			}
			else
			{
				if(Program.Menu.Item("Draw_Q_Spider").GetValue<bool>())
					if(QSpider.Level > 0)
						Utility.DrawCircle(ObjectManager.Player.Position, QSpider.Range, QSpider.IsReady() ? Color.Green : Color.Red);

				if(Program.Menu.Item("Draw_E_Spider").GetValue<bool>())
					if(ESpider.Level > 0)
						Utility.DrawCircle(ObjectManager.Player.Position, ESpider.Range, ESpider.IsReady() ? Color.Green : Color.Red);
			}
		}

		private void LaneClear()
		{
			var justSpider = Program.Menu.Item("LaneClear_mana").GetValue<Slider>().Value >=
			                 ObjectManager.Player.Mana/ObjectManager.Player.MaxMana*100;

			if (justSpider)
				Switchto("Spider");

			if(Humanform())
			{
				if (Program.Menu.Item("useQ_LaneClear").GetValue<bool>())
					Cast_Basic_Farm(QHuman);
				if(Program.Menu.Item("useW_LaneClear").GetValue<bool>())
					Cast_Basic_Farm(WHuman,true );
				if(Program.Menu.Item("useE_LaneClear").GetValue<bool>())
					Cast_Basic_Farm(EHuman, true);
				if(Program.Menu.Item("LaneClear_forms").GetValue<bool>())
					if ( (Program.Menu.Item("useQ_LaneClear").GetValue<bool>() && !QHuman.IsReady()) ||
						(Program.Menu.Item("useW_LaneClear").GetValue<bool>() && !WHuman.IsReady()) ||
						(Program.Menu.Item("useE_LaneClear").GetValue<bool>() && !EHuman.IsReady()) )
						Switchto("Spider");
			}
			else
			{
				if(Program.Menu.Item("useQ_LaneClear").GetValue<bool>())
					Cast_Basic_Farm(QSpider );
				if(Program.Menu.Item("useE_LaneClear").GetValue<bool>())
					Cast_Basic_Farm(ESpider);
				if(Program.Menu.Item("useW_LaneClear").GetValue<bool>())
					if (
						MinionManager.GetMinions(ObjectManager.Player.ServerPosition,
							Orbwalking.GetRealAutoAttackRange(ObjectManager.Player), MinionTypes.All, MinionTeam.NotAlly,
							MinionOrderTypes.MaxHealth).Count >= 1)
						WSpider.Cast();
				if(MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 400, MinionTypes.All, MinionTeam.Ally).All(spider => spider.Name != "Spiderling") && !justSpider)
					Switchto("Human");
			}

		}

		private void Harass()
		{
			if(Program.Menu.Item("useQ_Harass").GetValue<bool>())
			CastQHuman();
			if(Program.Menu.Item("useW_Harass").GetValue<bool>())
			CastEHuman();
			if(Program.Menu.Item("useE_Harass").GetValue<bool>())
			CastWHuman();
		}

		private void Combo()
		{

			if (Humanform())
			{
				CastQHuman();
				CastEHuman();
				CastWHuman();
				if (!(QHuman.IsReady() || WHuman.IsReady() || EHuman.IsReady()))
					Switchto("Spider");
			}
			else
			{
				CastESpider();
				CastQSpider();
				CastWSpider();
				CheckSwitchtoHuman();				
			}
		}

		private void CheckSwitchtoHuman()
		{
			if (WSpider.IsReady())
				return;
			var target = SimpleTs.GetTarget(QSpider.Range, SimpleTs.DamageType.Magical);	
			if (target == null)
				Switchto("Human");
		}

		private void CastWSpider()
		{
			var target = SimpleTs.GetTarget(Orbwalking.GetRealAutoAttackRange(ObjectManager.Player), SimpleTs.DamageType.Magical);
			if (target.IsValidTarget())
				WSpider.Cast();
		}

		private void CastQSpider()
		{
			if(!QSpider.IsReady())
				return;
			var target = SimpleTs.GetTarget(QSpider.Range, SimpleTs.DamageType.Magical);
			QSpider.Cast(target, Packets());
		}

		private void CastESpider()
		{
			if(!ESpider.IsReady())
				return;
			var target = SimpleTs.GetTarget(ESpider.Range, SimpleTs.DamageType.Magical);
			if (target.IsValidTarget(ESpider.Range) && !target.IsValidTarget(QSpider.Range))
				ESpider.Cast(target, Packets());
		}

		private void Switchto(string p)
		{
			if(!R.IsReady())
				return;
			if (p == "Spider" && Humanform())
				R.Cast();
			else if (p == "Human" && !Humanform())
				R.Cast();
		}
		

		private void CastEHuman()
		{
			if(!EHuman.IsReady())
				return;
			var target = SimpleTs.GetTarget(EHuman.Range, SimpleTs.DamageType.Magical);
			if(EHuman.GetPrediction(target).Hitchance >= HitChance.High)
				EHuman.Cast(target, Packets());
		}

		private void CastWHuman()
		{
			if(!WHuman.IsReady())
				return;
			var target = SimpleTs.GetTarget(WHuman.Range, SimpleTs.DamageType.Magical);
			if (WHuman.GetPrediction(target).Hitchance >= HitChance.High )
				WHuman.Cast(target, Packets());
		}

		private void CastQHuman()
		{
			if (!QHuman.IsReady())
				return;
			var target = SimpleTs.GetTarget(QHuman.Range, SimpleTs.DamageType.Magical );
			QHuman.Cast(target, Packets());
		}


		private bool Humanform()
		{
		

			return ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Name   == "EliseHumanQ";
		}
	}
}
