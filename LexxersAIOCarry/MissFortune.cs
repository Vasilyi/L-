using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace UltimateCarry
{
	internal class MissFortune : Champion
	{
		public Spell Q;
		public Spell W;
		public Spell E;
		public Spell R;
		public static int UltTick;

        public MissFortune()
        {
			LoadMenu();
			LoadSpells();

			Drawing.OnDraw += Drawing_OnDraw;
			Game.OnGameUpdate += Game_OnGameUpdate;
			Obj_AI_Base.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
			Game.OnGameSendPacket += GameOnOnGameSendPacket;
			PluginLoaded();
		}

		private void LoadMenu()
		{
			Program.Menu.AddSubMenu(new Menu("TeamFight", "TeamFight"));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useQ_TeamFight", "Use Q").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useW_TeamFight", "Use W").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useE_TeamFight_bind", "Use E if stunned").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useE_TeamFight_willhit", "Use E if hit").SetValue(new Slider(1, 5, 0)));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useR_TeamFight_willhit", "Use R if Hit").SetValue(new Slider(3, 5, 0)));

			Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useQ_Harass", "Use Q").SetValue(true));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useW_Harass", "Use W").SetValue(true));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useE_Harass_bind", "Use E if stunned").SetValue(true));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useE_Harass_willhit", "Use E if hit").SetValue(new Slider(2, 5, 0)));
			AddManaManager("Harass", 40);

			Program.Menu.AddSubMenu(new Menu("LaneClear", "LaneClear"));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useQ_LaneClear", "Use Q").SetValue(true));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useW_LaneClear", "Use W").SetValue(true));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useE_LaneClear", "Use E").SetValue(true));
			AddManaManager("LaneClear", 20);

			Program.Menu.AddSubMenu(new Menu("Drawing", "Drawing"));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Q", "Draw Q").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_E", "Draw E").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_R", "Draw R").SetValue(true));

		}

		private void LoadSpells()
		{
			Q = new Spell(SpellSlot.Q, 650);
			Q.SetTargetted(0.29f, 1400f);

			W = new Spell(SpellSlot.W);

			E = new Spell(SpellSlot.E,800);
			E.SetSkillshot(0.5f,100f,float.MaxValue,false,SkillshotType.SkillshotCircle);

			R = new Spell(SpellSlot.R,1400);
			R.SetSkillshot(0.333f, 200, float.MaxValue, false, SkillshotType.SkillshotLine);
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

			if(Program.Menu.Item("Draw_R").GetValue<bool>())
				if(R.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
		
		}

		private void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
		{
			if(sender.IsMe)
			{
				if (args.SData.Name == "MissFortuneBulletTime")
				{
					Program.Orbwalker.SetAttacks(false);
					Program.Orbwalker.SetMovement(false);
					UltTick  = Environment.TickCount;
				}
			}
		}
		private void Game_OnGameUpdate(EventArgs args)
		{
			if (IsShooting())
				return;
			Program.Orbwalker.SetAttacks(true);
			Program.Orbwalker.SetMovement(true);

			switch(Program.Orbwalker.ActiveMode)
			{
				case Orbwalking.OrbwalkingMode.Combo:
					if(Program.Menu.Item("useQ_TeamFight").GetValue<bool>())
						CastQEnemy();
					if(Program.Menu.Item("useW_TeamFight").GetValue<bool>())
						CastW();
					if(Program.Menu.Item("useE_TeamFight_bind").GetValue<bool>())
						CastEEnemyBind();
					if(Program.Menu.Item("useE_TeamFight_willhit").GetValue<Slider>().Value >= 1)
						CastEEnemyAmount();
					if(Program.Menu.Item("useR_TeamFight_willhit").GetValue<Slider>().Value >= 1)
						CastREnemyAmount();
					break;
				case Orbwalking.OrbwalkingMode.Mixed:
					if(Program.Menu.Item("useQ_Harass").GetValue<bool>() && ManaManagerAllowCast(Q))
						CastQEnemy();
					if(Program.Menu.Item("useW_Harass").GetValue<bool>() && ManaManagerAllowCast(W))
						CastW();
					if(Program.Menu.Item("useE_Harass_bind").GetValue<bool>() && ManaManagerAllowCast(E))
						CastEEnemyBind();
					if(Program.Menu.Item("useE_Harass_willhit").GetValue<Slider>().Value >= 1 && ManaManagerAllowCast(E))
						CastEEnemyAmount();
					break;
				case Orbwalking.OrbwalkingMode.LaneClear:
					if(Program.Menu.Item("useQ_LaneClear").GetValue<bool>() && ManaManagerAllowCast(Q))
						CastQMinion();
					if(Program.Menu.Item("useW_LaneClear").GetValue<bool>() && ManaManagerAllowCast(W))
						CastW();
					if(Program.Menu.Item("useE_LaneClear").GetValue<bool>() && ManaManagerAllowCast(E))
						CastEMinion();
					break;
			}
		}

		private static void GameOnOnGameSendPacket(GamePacketEventArgs args)
		{
			if(args.PacketData[0] == Packet.C2S.Move.Header && IsShooting())
			{
				args.Process = false;
			}
		}

		private static bool IsShooting()
		{
			return Environment.TickCount - UltTick < 250 || ObjectManager.Player.HasBuff("missfortunebulletsound");
		}

		private void CastREnemyAmount()
		{
			if(!R.IsReady())
				return;
			if(
				!Program.Helper.EnemyTeam.Where(hero => hero.IsValidTarget(R.Range))
					.Any(
						enemy =>
							R.CastIfWillHit(enemy, Program.Menu.Item("useR_TeamFight_willhit").GetValue<Slider>().Value - 1, Packets())))
				return;
			Program.Orbwalker.SetAttacks(false);
			Program.Orbwalker.SetMovement(false);
			UltTick = Environment.TickCount;
		}


		private void CastEEnemyBind()
		{
			if(!E.IsReady())
				return;
            foreach (var enemy in Program.Helper.EnemyTeam.Where(hero => (hero.HasBuffOfType(BuffType.Snare) || hero.HasBuffOfType(BuffType.Stun) && hero.IsValidTarget(E.Range + (E.Width / 2)))))
			{
				E.Cast(enemy.Position, Packets());
				return;
			}
		}

		private void CastEEnemyAmount()
		{
			if(!E.IsReady())
				return;
            foreach (var enemy in Program.Helper.EnemyTeam.Where(hero => hero.IsValidTarget(E.Range + (E.Width / 2))))
			{
				E.CastIfWillHit(enemy, Program.Menu.Item("useE_TeamFight_willhit").GetValue<Slider>().Value - 1, Packets());
				return;
			}
		}

		private void CastW()
		{
			if(!W.IsReady())
				return;

			var target = SimpleTs.GetTarget(Orbwalking.GetRealAutoAttackRange(ObjectManager.Player), SimpleTs.DamageType.Physical);
			if(target != null)
				W.Cast();

			if(Program.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LaneClear)
				return;
			var allMinion = MinionManager.GetMinions(ObjectManager.Player.Position,
				Orbwalking.GetRealAutoAttackRange(ObjectManager.Player), MinionTypes.All, MinionTeam.NotAlly);
			if(!allMinion.Any(minion => minion.IsValidTarget(Orbwalking.GetRealAutoAttackRange(ObjectManager.Player))))
				return;
			W.Cast();
		}

		private void CastQEnemy()
		{
			if(!Q.IsReady())
				return;
			var target = SimpleTs.GetTarget(Q.Range + 500, SimpleTs.DamageType.Physical);
			var allMinion = MinionManager.GetMinions(target.Position, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
			if (target.IsValidTarget(Q.Range))
			{
				Q.CastOnUnit(target,Packets());
				return;
			}
			Obj_AI_Base[]  nearstMinion = {null};
			foreach (var minion in allMinion.Where(minion => minion.Distance(ObjectManager.Player) <= target.Distance(ObjectManager.Player) && target.Distance(minion) < 450 ).Where(minion => nearstMinion[0] == null || minion.Distance(ObjectManager.Player) < nearstMinion[0].Distance(ObjectManager.Player)))
				nearstMinion[0] = minion;
			if (nearstMinion[0] != null && nearstMinion[0].IsValidTarget(Q.Range))
				Q.CastOnUnit(nearstMinion[0], Packets());
		}

		private void CastQMinion()
		{
			if(!Q.IsReady())
				return;
			var allMinion = MinionManager.GetMinions(ObjectManager.Player.Position, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
			Obj_AI_Base[] nearstMinion = {null};
			foreach (var minion in allMinion.Where(minion => nearstMinion[0] == null || ObjectManager.Player.Distance(nearstMinion[0]) > ObjectManager.Player.Distance(minion)))
				nearstMinion[0] = minion;
			if (nearstMinion[0] != null)
				Q.CastOnUnit(nearstMinion[0],Packets());
		}

		private void CastEMinion()
		{
			if(!E.IsReady())
				return;
			var minions = MinionManager.GetMinions(ObjectManager.Player.Position, E.Range + (E.Width +200 / 2), MinionTypes.All, MinionTeam.NotAlly);
			if(minions.Count == 0)
				return;
			var castPostion = MinionManager.GetBestCircularFarmLocation(minions.Select(minion => minion.ServerPosition.To2D()).ToList(),E.Width + 200,E.Range);
			E.Cast(castPostion.Position, Packets());
		}

	}
}
