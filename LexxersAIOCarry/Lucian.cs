using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace UltimateCarry
{
	class Lucian : Champion
	{
		public Spell Q;
		public Spell Q2;
		public Spell W;
		public Spell E;
		public Spell R;

		public bool RActive;
		public int SpellCastetTick;
		public bool CanUseSpells = true;
		public bool WaitingForBuff = false;
		public bool GainBuff = false;

        public Lucian()
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
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useQ_TeamFight", "Use Q").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useW_TeamFight", "Use W").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useE_TeamFight", "Use E").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useE_TeamFight_Range", "E Target in Range").SetValue(new Slider(1100, 2000, 500)));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useR_TeamFight", "Use R").SetValue(false));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useR_TeamFight_2", "Use R if rest on CD").SetValue(true));

			Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useQ_Harass", "Use Q").SetValue(true));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useW_Harass", "Use W").SetValue(true));
			AddManaManager("Harass", 50);

			Program.Menu.AddSubMenu(new Menu("LaneClear", "LaneClear"));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useQ_LaneClear", "Use Q").SetValue(true));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useW_LaneClear", "Use W").SetValue(true));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useE_LaneClear", "Use E").SetValue(true));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useE_LaneClear_Range", "E Minion in Range").SetValue(new Slider(1100, 2000, 500)));
			AddManaManager("LaneClear", 25);

			Program.Menu.AddSubMenu(new Menu("LastHit", "LastHit"));
			Program.Menu.SubMenu("LastHit").AddItem(new MenuItem("useQ_LastHit", "Use Q").SetValue(true));
			AddManaManager("LastHit", 70);

			Program.Menu.AddSubMenu(new Menu("Drawing", "Drawing"));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Q", "Draw Q").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_W", "Draw W").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_E", "Draw E").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_R", "Draw R").SetValue(true));

		}

		private void LoadSpells()
		{
			Q = new Spell(SpellSlot.Q, 675);
			Q.SetTargetted(0.5f, float.MaxValue);

			Q2 = new Spell(SpellSlot.Q, 1100);
			Q2.SetSkillshot(0.5f, 5f, float.MaxValue, true, SkillshotType.SkillshotLine);

			W = new Spell(SpellSlot.W, 1000);
			W.SetSkillshot(0.3f, 80f, 1600, true, SkillshotType.SkillshotLine);

			E = new Spell(SpellSlot.E, 475);
			E.SetSkillshot(0.25f, 0.01f, float.MaxValue, false, SkillshotType.SkillshotLine);

			R = new Spell(SpellSlot.R, 1400);
			R.SetSkillshot(0.01f, 110, 2800f, true, SkillshotType.SkillshotLine);
		}

		private void Game_OnGameUpdate(EventArgs args)
		{
			BuffCheck();
			UltCheck();

			switch (Program.Orbwalker.ActiveMode)
			{
				case Orbwalking.OrbwalkingMode.Combo:
					if (Program.Menu.Item("useE_TeamFight").GetValue<bool>())
						CastE();
					if (Program.Menu.Item("useQ_TeamFight").GetValue<bool>())
						CastQEnemy();
					if (Program.Menu.Item("useW_TeamFight").GetValue<bool>())
						CastWEnemy();
					if(Program.Menu.Item("useR_TeamFight").GetValue<bool>() || Program.Menu.Item("useR_TeamFight_2").GetValue<bool>())
						CastREnemy();
					break;
				case Orbwalking.OrbwalkingMode.Mixed:
					if(Program.Menu.Item("useQ_Harass").GetValue<bool>() && ManaManagerAllowCast(Q))
						CastQEnemy();
					if(Program.Menu.Item("useW_Harass").GetValue<bool>() && ManaManagerAllowCast(W))
						CastWEnemy();
					break;
				case Orbwalking.OrbwalkingMode.LaneClear:
					if(Program.Menu.Item("useE_LaneClear").GetValue<bool>() && ManaManagerAllowCast(E))
						CastE();
					if(Program.Menu.Item("useQ_LaneClear").GetValue<bool>() && ManaManagerAllowCast(Q))
					{
						CastQEnemy();
						CastQMinion();
					}
					if(Program.Menu.Item("useW_LaneClear").GetValue<bool>() && ManaManagerAllowCast(W))
					{
						CastWEnemy();
						CastWMinion();
					}
					break;
				case Orbwalking.OrbwalkingMode.LastHit:
					if(Program.Menu.Item("useQ_LastHit").GetValue<bool>() && ManaManagerAllowCast(Q))
						CastQMinion();
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

			if(Program.Menu.Item("Draw_W").GetValue<bool>())
				if(W.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);

			if(Program.Menu.Item("Draw_E").GetValue<bool>())
				if(E.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

			if(Program.Menu.Item("Draw_R").GetValue<bool>())
				if(R.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
		}

		private void CastQEnemy()
		{
			if(!Q.IsReady() || !CanUseSpells)
				return;
			var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);

			if(target != null)
			{
				if((target.IsValidTarget(Q.Range)))
				{
					Q.Cast(target, Packets());
					UsedSkill();
				}
			}
			target = SimpleTs.GetTarget(Q2.Range, SimpleTs.DamageType.Physical);
			if(target == null)
				return;
			if((!target.IsValidTarget(Q2.Range)) || !CanUseSpells || !Q.IsReady())
				return;
			var qCollision = Q2.GetPrediction(target).CollisionObjects;
			foreach(var qCollisionChar in qCollision.Where(qCollisionChar => qCollisionChar.IsValidTarget(Q.Range)))
			{
				Q.Cast(qCollisionChar, Packets());
				UsedSkill();
			}
		}

		private void CastQMinion()
		{
			if(!Q.IsReady() || !CanUseSpells)
				return;
			var lastHit = Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit;
			var laneClear = Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear;
			var allMinions = MinionManager.GetMinions(ObjectManager.Player.Position, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
			if(lastHit)
			{
				var minion =
					allMinions.FirstOrDefault(minionn => minionn.Distance(ObjectManager.Player) <= Q.Range && minionn.Health <= DamageLib.getDmg(minionn, DamageLib.SpellType.Q));
				if(minion == null)
					return;
				Q.CastOnUnit(minion, Packets());
				UsedSkill();
			}
			else if(laneClear)
			{
				var minion =
                allMinions.FirstOrDefault(
					minionn => minionn.Distance(ObjectManager.Player) <= Q.Range);
				if(minion == null)
					return;
				Q.CastOnUnit(minion, Packets());
				UsedSkill();
			}
		}

		private void CastWEnemy()
		{
			if(!W.IsReady() || !CanUseSpells)
				return;
			var target = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Magical);
			if(target == null)
				return;
			if(target.IsValidTarget(W.Range) && W.GetPrediction(target).Hitchance >= HitChance.High)
			{
				W.Cast(target, Packets());
				UsedSkill();
			}
			else if(W.GetPrediction(target).Hitchance == HitChance.Collision)
			{
				var wCollision = W.GetPrediction(target).CollisionObjects;
				foreach(var wCollisionChar in wCollision.Where(wCollisionChar => wCollisionChar.Distance(target) <= 100))
				{
					W.Cast(wCollisionChar.Position, Packets());
					UsedSkill();
				}
			}
		}

		private void CastWMinion()
		{
			if (!W.IsReady() || !CanUseSpells)
				return;
			var allMinions = MinionManager.GetMinions(ObjectManager.Player.Position, W.Range + 100, MinionTypes.All,
				MinionTeam.NotAlly);
            var minion = allMinions.FirstOrDefault(minionn => minionn.IsValidTarget(W.Range));
			if (minion != null)
			{
				W.Cast(minion.Position, Packets());
				UsedSkill();
			}
		}

		private void CastREnemy()
		{
			if((Program.Menu.Item("useR_TeamFight_2").GetValue<bool>() && (Q.IsReady() || W.IsReady() || E.IsReady())) || (!R.IsReady() || !CanUseSpells))
				return;
			var target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Physical);
			if (target.IsValidTarget(R.Range))
			{
				R.Cast(target, Packets());
				UsedSkill();
				RActive = true;
			}
		}

		private void CastE()
		{
			if(!E.IsReady() || !CanUseSpells)
				return;
			var combo = Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo;
			var comboRange = Program.Menu.Item("useE_TeamFight_Range").GetValue<Slider>().Value;

			var laneClear = Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear;
			var laneClearRange = Program.Menu.Item("useE_LaneClear_Range").GetValue<Slider>().Value;


			if(combo)
			{
				var target = SimpleTs.GetTarget(comboRange, SimpleTs.DamageType.Physical);
				if (!target.IsValidTarget(comboRange)) 
					return;
				E.Cast(Game.CursorPos, Packets());
				UsedSkill();
			}
			else if(laneClear)
			{
				var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, laneClearRange, MinionTypes.All, MinionTeam.NotAlly);
				foreach (var minion in allMinions.Where(minion => minion != null).Where(minion => minion.IsValidTarget(laneClearRange) && E.IsReady()))
				{
					E.Cast(Game.CursorPos, Packets());
					UsedSkill();
				}
			}
		}

		private void UsedSkill()
		{
			if(!CanUseSpells)
				return;
			CanUseSpells = false;
			SpellCastetTick = Environment.TickCount;
		}

		private void UltCheck()
		{
			var tempultactive = false;
			foreach(var buff in ObjectManager.Player.Buffs.Where(buff => buff.Name == "LucianR"))
				tempultactive = true;

			if(tempultactive)
			{
				Program.Orbwalker.SetAttacks(false);
				RActive = true;
			}
			if(!tempultactive)
			{
				Program.Orbwalker.SetAttacks(true);
				RActive = false;
			}
		}

		private void BuffCheck()
		{
			if(CanUseSpells == false && WaitingForBuff == false && GainBuff == false)
				WaitingForBuff = true;

			if(WaitingForBuff)
				foreach(var buff in ObjectManager.Player.Buffs.Where(buff => buff.Name == "lucianpassivebuff"))
					GainBuff = true;

			if(GainBuff)
			{
				WaitingForBuff = false;
				var tempgotBuff = false;
				foreach(var buff in ObjectManager.Player.Buffs.Where(buff => buff.Name == "lucianpassivebuff"))
					tempgotBuff = true;
				if(tempgotBuff == false)
				{
					GainBuff = false;
					CanUseSpells = true;
				}
			}

			if(SpellCastetTick >= Environment.TickCount - 1000 || WaitingForBuff != true)
				return;
			WaitingForBuff = false;
			GainBuff = false;
			CanUseSpells = true;
		}

	}
}
