using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace UltimateCarry
{
	class Jax : Champion
	{
		public Spell Q;
		public Spell W;
		public Spell E;
		public Spell R;

		public Jax()
        {
			LoadMenu();
			LoadSpells();

			Drawing.OnDraw += Drawing_OnDraw;
			Game.OnGameUpdate += Game_OnGameUpdate;
			Game.OnGameSendPacket += Game_OnGameSendPacket;
			Orbwalking.AfterAttack += Orbwalking_AfterAttack;
			PluginLoaded();
		}

		private void LoadMenu()
		{
			Program.Menu.AddSubMenu(new Menu("TeamFight", "TeamFight"));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useQ_TeamFight", "Use Q").SetValue(new StringList(new[] { "Not", "Out of Range", "Always" }, 1)));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useW_TeamFight", "Use W").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useE_TeamFight", "Use E").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useR_TeamFight", "Use R if Enemys").SetValue(new Slider(2, 5, 0)));
			//Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useW_TeamFight_bind", "Use W if stunned").SetValue(true));
			//Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useW_TeamFight_willhit", "Use W if hit").SetValue(new Slider(2, 5, 0)));
			//Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useR_TeamFight", "Use R if Hit").SetValue(new Slider(2, 5, 0)));


			Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useQ_Harass", "Use Q").SetValue(new StringList(new[] { "Not", "Out of Range", "Always" })));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useW_Harass", "Use W").SetValue(true));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useE_Harass", "Use E").SetValue(true));
			//Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useW_Harass_willhit", "Use W if hit").SetValue(true));

			Program.Menu.AddSubMenu(new Menu("LaneClear", "LaneClear"));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useQ_LaneClear", "Use Q").SetValue(true));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useW_LaneClear", "Use W").SetValue(true));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useE_Laneclear", "Use E").SetValue(true));
			//Program.Menu.AddSubMenu(new Menu("SupportMode", "SupportMode"));
			//Program.Menu.SubMenu("SupportMode").AddItem(new MenuItem("hitMinions", "Hit Minions").SetValue(false));

			Program.Menu.AddSubMenu(new Menu("Drawing", "Drawing"));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Q", "Draw Q").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_E", "Draw E").SetValue(true));
		}

		private void LoadSpells()
		{
			Q = new Spell(SpellSlot.Q, 700);

			W = new Spell(SpellSlot.W);
			
			E = new Spell(SpellSlot.E,185);

			R = new Spell(SpellSlot.R);
		}

		private void Orbwalking_AfterAttack(Obj_AI_Base unit, Obj_AI_Base target)
		{
			if (!unit.IsMe || Environment.TickCount - W.LastCastAttemptT < 100)
				return;
			switch(Program.Orbwalker.ActiveMode)
			{
				case Orbwalking.OrbwalkingMode.Combo:
					if (Program.Menu.Item("useW_TeamFight").GetValue<bool>() && W.IsReady())
					{
						Orbwalking.ResetAutoAttackTimer();
						W.Cast();
					}
					break;
				case Orbwalking.OrbwalkingMode.Mixed:
					if(Program.Menu.Item("useW_Harass").GetValue<bool>())
						if(!Program.Orbwalker.GetTarget().IsMinion && Program.Orbwalker.GetTarget().IsEnemy && W.IsReady())
						{
							Orbwalking.ResetAutoAttackTimer();
							W.Cast();
						}
					break;
				case Orbwalking.OrbwalkingMode.LaneClear:
					if(Program.Menu.Item("useW_LaneClear").GetValue<bool>())
						if(Program.Orbwalker.GetTarget().Health >= DamageLib.getDmg(Program.Orbwalker.GetTarget(),DamageLib.SpellType.AD) && W.IsReady())
						{
							Orbwalking.ResetAutoAttackTimer();
							W.Cast();
						}
					break;
			}
		}

		private void Game_OnGameUpdate(EventArgs args)
		{
			switch(Program.Orbwalker.ActiveMode)
			{
				case Orbwalking.OrbwalkingMode.Combo:
					if(Q.IsReady())
						CastQ();
					CastE();
					CastR();
					break;
				case Orbwalking.OrbwalkingMode.Mixed:
					if(Q.IsReady())
						CastQ();
					CastE();
					break;
				case Orbwalking.OrbwalkingMode.LaneClear:
					if(Q.IsReady())
						CastQ();
					CastE();
					break;
			}
		}

		private void CastR()
		{
			if(!R.IsReady() || Program.Menu.Item("useR_TeamFight").GetValue<Slider>().Value == 0)
				return;
			if(Utility.CountEnemysInRange(500) >= Program.Menu.Item("useR_TeamFight").GetValue<Slider>().Value)
				R.Cast();
		}

		private void CastE()
		{
			if(E.IsReady() && Environment.TickCount - E.LastCastAttemptT <= 5000)
				if(Utility.CountEnemysInRange((int)E.Range) >= 1)
					E.Cast();
			
			if (!E.IsReady()) 
				return;
		
			switch(Program.Orbwalker.ActiveMode)
			{
				case Orbwalking.OrbwalkingMode.Combo:
					if(Program.Menu.Item("useE_TeamFight").GetValue<bool>())
						if(Q.IsReady() && (Utility.CountEnemysInRange((int)Q.Range + +200) >= 1) ||
						   (Utility.CountEnemysInRange((int)E.Range + 100) >= 1))
							E.Cast();
					break;
				case Orbwalking.OrbwalkingMode.Mixed:
					if(Program.Menu.Item("useE_Harass").GetValue<bool>())
					if(Q.IsReady() && (Utility.CountEnemysInRange((int)Q.Range + +200) >= 1) ||
					   (Utility.CountEnemysInRange((int)E.Range + 100) >= 1))
						E.Cast();
					break;
				case Orbwalking.OrbwalkingMode.LaneClear:
					if(!Program.Menu.Item("useE_Laneclear").GetValue<bool>())
						return;
					var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range, MinionTypes.All,
						MinionTeam.NotAlly);
					if(allMinions.Count(minion => minion.IsValidTarget(Orbwalking.GetRealAutoAttackRange(ObjectManager.Player))) >= 1)
						E.Cast();
					break;
			}
		}

		private void CastQ()
		{
			var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
			switch(Program.Orbwalker.ActiveMode)
			{
				case Orbwalking.OrbwalkingMode.Combo:
					switch(Program.Menu.Item("useQ_TeamFight").GetValue<StringList>().SelectedIndex)
					{
						case 0:
							return;
						case 1:
							if(target.Distance(ObjectManager.Player) >= Orbwalking.GetRealAutoAttackRange(ObjectManager.Player))
								Q.CastOnUnit(target, Packets());
							break;
						case 2:
							Q.CastOnUnit(target, Packets());
							break;
					}
					break;
				case Orbwalking.OrbwalkingMode.Mixed:
					switch(Program.Menu.Item("useQ_Harass").GetValue<StringList>().SelectedIndex)
					{
						case 0:
							return;
						case 1:
							if(target.Distance(ObjectManager.Player) >= Orbwalking.GetRealAutoAttackRange(ObjectManager.Player))
								Q.CastOnUnit(target, Packets());
							break;
						case 2:
							Q.CastOnUnit(target, Packets());
							break;
					}
					break;
				case Orbwalking.OrbwalkingMode.LaneClear:
					if(Program.Menu.Item("useQ_LaneClear").GetValue<bool>())
						Cast_Basic_Farm(Q);
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

			if(Program.Menu.Item("Draw_E").GetValue<bool>())
				if(E.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);
		}
	}
}
