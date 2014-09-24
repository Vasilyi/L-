using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace UltimateCarry
{
	class Gangplank : Champion
	{

		public static Spell Q;
		public static Spell W;
		public static Spell E;
		public static Spell R;
		public Gangplank()
		{
			LoadMenu();
			LoadSpells();

			Drawing.OnDraw += Drawing_OnDraw;
			Game.OnGameUpdate += Game_OnGameUpdate;
			PluginLoaded();
		}

		private void LoadSpells()
		{
			Q = new Spell(SpellSlot.Q, 625);

			W = new Spell(SpellSlot.W);

			E = new Spell(SpellSlot.E,1150);

			R = new Spell(SpellSlot.R);
			R.SetSkillshot(0.7f,200,float.MaxValue,false,SkillshotType.SkillshotCircle);
		}

		private void LoadMenu()
		{
			Program.Menu.AddSubMenu(new Menu("TeamFight", "TeamFight"));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useQ_TeamFight", "Use Q").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useE_TeamFight", "Use E").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useR_TeamFight", "Use R").SetValue(new Slider(2,0,5)));
		
			Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useQ_Harass", "Use Q").SetValue(true));
			AddManaManager("Harass", 40);

			Program.Menu.AddSubMenu(new Menu("LaneClear", "LaneClear"));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useQ_LaneClear_minion", "Use Q Minion").SetValue(true));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useQ_LaneClear_enemy", "Use Q Enemy").SetValue(true));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useE_LaneClear", "Use E").SetValue(true));
			AddManaManager("LaneClear", 20);

			Program.Menu.AddSubMenu(new Menu("LastHit", "LastHit"));
			Program.Menu.SubMenu("LastHit").AddItem(new MenuItem("useQ_LastHit", "Use Q").SetValue(true));
			AddManaManager("LastHit", 60);

			Program.Menu.AddSubMenu(new Menu("Passive", "Passive"));
			Program.Menu.SubMenu("Passive").AddItem(new MenuItem("useW_onStun", "Use W on Stun").SetValue(true));
			Program.Menu.SubMenu("Passive").AddItem(new MenuItem("useW_onLowlife", "Use W if Health below").SetValue(new Slider(40)));
			Program.Menu.SubMenu("Passive").AddItem(new MenuItem("useR_KS", "Use R for KS").SetValue(true));

			Program.Menu.AddSubMenu(new Menu("Drawing", "Drawing"));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Q", "Draw Q").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_E", "Draw E").SetValue(true));
		}

		private void Game_OnGameUpdate(EventArgs args)
		{

			if (Program.Menu.Item("useW_onStun").GetValue<bool>())
				CheckWStun();
			if (Program.Menu.Item("useW_onLowlife").GetValue<Slider>().Value >=
			    ObjectManager.Player.Health/ObjectManager.Player.MaxHealth*100 && W.IsReady())
				W.Cast();

			CastRKS();

			switch (Program.Orbwalker.ActiveMode)
			{
				case Orbwalking.OrbwalkingMode.Combo:
					if (Program.Menu.Item("useQ_TeamFight").GetValue<bool>())
						QEnemy();
					if (Program.Menu.Item("useE_TeamFight").GetValue<bool>())
						CastE();
					if (Program.Menu.Item("useR_TeamFight").GetValue<Slider>().Value >= 1)
						CastR();
					break;
				case Orbwalking.OrbwalkingMode.Mixed:
					if (Program.Menu.Item("useQ_Harass").GetValue<bool>() && ManaManagerAllowCast(Q))
						QEnemy();
					break;
				case Orbwalking.OrbwalkingMode.LaneClear:
					if (Program.Menu.Item("useQ_LaneClear_minion").GetValue<bool>() && ManaManagerAllowCast(Q))
						QLasthitMinion();
					if (Program.Menu.Item("useQ_LaneClear_enemy").GetValue<bool>() && ManaManagerAllowCast(Q))
						QEnemy();
					if (Program.Menu.Item("useE_LaneClear").GetValue<bool>() && ManaManagerAllowCast(Q))
						CastE();
					break;
				case Orbwalking.OrbwalkingMode.LastHit:
					if (Program.Menu.Item("useQ_LastHit").GetValue<bool>() && ManaManagerAllowCast(Q))
						QLasthitMinion();
					break;
			}
		}

		private void CastRKS()
		{

			if (!R.IsReady( ))
				return;
			foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget() && hero.Health <= (DamageLib.getDmg( hero,DamageLib.SpellType.R) / 2)))
			{
				R.Cast(enemy, Packets());
				return;
			}
		
		}

		private void CastR()
		{
			if (!R.IsReady( ))
				return;
			foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget()))
			{
				R.CastIfWillHit(enemy, Program.Menu.Item("useR_TeamFight").GetValue<Slider>().Value - 1, Packets());
			}
		}

		private void CastE()
		{
			if(!E.IsReady())
				return;
			if(Orbwalking.OrbwalkingMode.LaneClear == Program.Orbwalker.ActiveMode)
			{
				var allMinions = MinionManager.GetMinions(ObjectManager.Player.Position, 400, MinionTypes.All, MinionTeam.NotAlly);
				if(allMinions.Count >= 3)
				{
					E.Cast();
					return;
				}
				string[] minionNames = { "Worm", "Dragon", "LizardElder", "AncientGolem", "TT_Spiderboss", "TTNGolem", "TTNWolf", "TTNWraith" };
				if(
					!allMinions.Where(minion => minionNames.Contains(minion.Name))
						.Any(minion => ObjectManager.Get<Obj_AI_Hero>().Count(hero => hero.IsAlly && minion.Distance(hero) <= 400) >= 3))
					return;
				E.Cast();
				return;
			}
			if(Orbwalking.OrbwalkingMode.Combo != Program.Orbwalker.ActiveMode)
				return;
			var count = ObjectManager.Get<Obj_AI_Hero>().Count(hero =>  hero.IsAlly && hero.Distance(ObjectManager.Player) <= E.Range);
			if(count >= 3)
				E.Cast();
		}

		private void CheckWStun()
		{
			if(!W.IsReady())
				return;
			if(ObjectManager.Player.HasBuffOfType(BuffType.Stun) ||
				ObjectManager.Player.HasBuffOfType(BuffType.Snare) ||
				ObjectManager.Player.HasBuffOfType(BuffType.Slow))
				W.Cast();
		}

		private void QEnemy()
		{
			if(!Q.IsReady())
				return;
			var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
			if (target.IsValidTarget(Q.Range))
				Q.Cast(target, Packets());
		}

		private void QLasthitMinion()
		{
			if(!Q.IsReady())
				return;
			var allminions = MinionManager.GetMinions(ObjectManager.Player.Position, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
			if(allminions.Count == 0)
				return;
			foreach(var minion in allminions.Where(minion => minion.Health <= DamageLib.getDmg(minion, DamageLib.SpellType.Q)))
			{
				Q.CastOnUnit(minion, Packets());
				return;
			}
			if(Orbwalking.OrbwalkingMode.LaneClear != Program.Orbwalker.ActiveMode)
				return;
			foreach(var minion in allminions)
			{
				Q.CastOnUnit(minion, Packets());
				return;
			}
		}

		private void Drawing_OnDraw(EventArgs args)
		{
			if(Program.Menu.Item("Draw_Disabled").GetValue<bool>())
				return;

			if(Program.Menu.Item("Draw_Q").GetValue<bool>())
				if(Q.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);

			if(Program.Menu.Item("Draw_E").GetValue<bool>())
				if(E.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);
		}

	}
}
