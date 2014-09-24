using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace UltimateCarry
{
	class Tristana : Champion
	{
		//public static UcTargetSelector TS;
		public static Spell Q;
		public static Spell W;
		public static Spell E;
		public static Spell R;
		public static int SpellRangeTick;
		public Tristana()
        {
			LoadMenu();
			LoadSpells();

			Drawing.OnDraw += Drawing_OnDraw;
			Game.OnGameUpdate += Game_OnGameUpdate;
			Interrupter.OnPosibleToInterrupt += Interrupter_OnPosibleToInterrupt;
			AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGabcloser;
			PluginLoaded();
		}

		private void LoadSpells()
		{
			Q = new Spell(SpellSlot.Q, Orbwalking.GetRealAutoAttackRange(ObjectManager.Player));

			W = new Spell(SpellSlot.W, 900);
			W.SetSkillshot(0.25f, 150, 1200, false, SkillshotType.SkillshotCircle);

			E = new Spell(SpellSlot.E, 550);

			R = new Spell(SpellSlot.R, 550);
		}

		private void LoadMenu()
		{
			//TS = new UcTargetSelector(Orbwalking.GetRealAutoAttackRange(ObjectManager.Player), UcTargetSelector.Mode.AutoPriority);
			
			//Program.Menu.AddSubMenu(new Menu("UC-TargetSelector", "UCTS"));
			//UcTargetSelector.AddtoMenu(Program.Menu.SubMenu("UCTS"));

			Program.Menu.AddSubMenu(new Menu("TeamFight", "TeamFight"));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useQ_TeamFight", "Use Q").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useE_TeamFight", "Use E").SetValue(true));

			Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useQ_Harass", "Use Q").SetValue(true));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useE_Harass", "Use E").SetValue(true));
			AddManaManager("Harass", 40);

			Program.Menu.AddSubMenu(new Menu("LaneClear", "LaneClear"));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useQ_LaneClear", "Use Q").SetValue(true));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useW_LaneClear", "Use W").SetValue(true));
			AddManaManager("LaneClear", 20);

			Program.Menu.AddSubMenu(new Menu("Passive", "Passive"));
			Program.Menu.SubMenu("Passive").AddItem(new MenuItem("useR_Interrupt", "Use R Interrupt").SetValue(true));
			Program.Menu.SubMenu("Passive").AddItem(new MenuItem("useR_Antigapclose", "Use R AntigapcloseS").SetValue(true));

			Program.Menu.AddSubMenu(new Menu("KillSteal", "KillSteal"));
			Program.Menu.SubMenu("KillSteal").AddItem(new MenuItem("use_ComboKS", "Use KS").SetValue(true));
			Program.Menu.SubMenu("KillSteal").AddItem(new MenuItem("use_ComboKS_W", "Use W in KS").SetValue(true));

			Program.Menu.AddSubMenu(new Menu("Drawing", "Drawing"));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Q", "Draw Q").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_W", "Draw W").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_E", "Draw E").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_R", "Draw R").SetValue(true));
		}

		private void Game_OnGameUpdate(EventArgs args)
		{
			UpdateSpellranges();
				

			switch(Program.Orbwalker.ActiveMode)
			{
				case Orbwalking.OrbwalkingMode.Combo:
					if(ComboKs())
						return;
					if (Program.Menu.Item("useQ_TeamFight").GetValue<bool>())
						CastQ();
					if (Program.Menu.Item("useE_TeamFight").GetValue<bool>())
						CastE();
					break;
				case Orbwalking.OrbwalkingMode.Mixed:
					if(Program.Menu.Item("useQ_Harass").GetValue<bool>())
						CastQ();
					if(Program.Menu.Item("useE_Harass").GetValue<bool>())
						CastE();
					break;
				case Orbwalking.OrbwalkingMode.LaneClear:
					if(Program.Menu.Item("useQ_LaneClear").GetValue<bool>())
						CastQ_farm();
					if(Program.Menu.Item("useW_LaneClear").GetValue<bool>())
					Cast_BasicCircleSkillshot_AOE_Farm(W,300);				
					break;
			}
			

		}

		private bool ComboKs()
		{
			if (!Program.Menu.Item("use_ComboKS").GetValue<bool>())
				return false;
			foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget() && !hero.IsAlly))
			{
				if(!enemy.IsValidTarget(W.Range + E.Range))
					continue;
				var targetdis = ObjectManager.Player.Distance(enemy);
				var rDamage = DamageLib.getDmg(enemy, DamageLib.SpellType.R);
				var wDamage = DamageLib.getDmg(enemy, DamageLib.SpellType.W);
				var eDamage = DamageLib.getDmg(enemy, DamageLib.SpellType.E);
				var igniteDamage = GetIgniteDamage(enemy);
				var health = enemy.Health + (enemy.HPRegenRate / 5 * 3) + 50;

				if (E.IsReady())
				{
					if(health <= eDamage + igniteDamage && targetdis < E.Range)
					{
						E.CastOnUnit(enemy, Packets());
						return true;
					}

					if(health <= rDamage + eDamage + igniteDamage && targetdis < E.Range && R.IsReady())
					{
						E.CastOnUnit(enemy, Packets());
						return true;
					}
				}

				if(W.IsReady() && Program.Menu.Item("use_ComboKS_W").GetValue<bool>())
				{
					if(health <= wDamage + igniteDamage && targetdis < W.Range)
					{
						W.CastIfHitchanceEquals(enemy, HitChance.High, Packets());
						return true;
					}
					if(health <=  eDamage + rDamage + wDamage + igniteDamage && targetdis < W.Range)
					{
						Program.Orbwalker.ForceTarget(enemy);
						W.CastIfHitchanceEquals(enemy,HitChance.High, Packets());
						return true;
					}

					if(health <= igniteDamage && targetdis < W.Range + Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) - 200 )
					{
						Program.Orbwalker.ForceTarget(enemy);
						W.Cast( GetJumpposition(enemy) ,Packets());
						return true;
					}
					if(health <= eDamage   + igniteDamage && targetdis < W.Range + E.Range - 200 && E.IsReady())
					{
						Program.Orbwalker.ForceTarget(enemy);
						W.Cast(GetJumpposition(enemy), Packets());
						return true;
					}
					if(health <= rDamage  + igniteDamage && targetdis < W.Range + R.Range - 200 &&  R.IsReady())
					{
						Program.Orbwalker.ForceTarget(enemy);
						W.Cast(GetJumpposition(enemy), Packets());
						return true;
					}
				}
				if (R.IsReady())
				{
					if(health <= rDamage + igniteDamage && targetdis < R.Range)
					{
						R.CastOnUnit(enemy, Packets());
						return true;
					}
				}
			}
			return false;
		}

		private Vector3 GetJumpposition(Obj_AI_Hero enemy)
		{
			if (ObjectManager.Player.Position.Distance(enemy.Position) <= W.Range)
				return enemy.Position;
			var newpos = enemy.Position - ObjectManager.Player.Position;
			newpos.Normalize();
			return ObjectManager.Player.Position + (newpos * W.Range);
		}
		private float GetIgniteDamage(Obj_AI_Base target)
		{
			var spells = ObjectManager.Player.SummonerSpellbook.Spells;
			var smite = SpellSlot.Unknown;
			foreach(var spell in spells.Where(spell => spell.Name.ToLower() == "summonersmite"))
				smite = spell.Slot;
			if(smite == SpellSlot.Unknown)
				return 0;
			if(ObjectManager.Player.SummonerSpellbook.CanUseSpell(smite) == SpellState.Ready)
				return (float)DamageLib.getDmg(target, DamageLib.SpellType.IGNITE);
			return 0;
		}

		private void CastQ_farm()
		{
			if(!Q.IsReady() )
				return;
			var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Orbwalking.GetRealAutoAttackRange(ObjectManager.Player), MinionTypes.All, MinionTeam.NotAlly);
			if (allMinions.Where(minion => minion != null).Any(minion => minion.IsValidTarget(Orbwalking.GetRealAutoAttackRange(ObjectManager.Player)) && Q.IsReady()))
				Q.Cast();		
		}

		private void CastE()
		{
			var target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);
			if(!E.IsReady() || !target.IsValidTarget(E.Range) || !ManaManagerAllowCast( E))
				return;
			E.CastOnUnit(target,Packets());
		}

		private void CastQ()
		{
			var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
			if(!Q.IsReady() || !target.IsValidTarget(Q.Range) )
				return;
			Q.Cast();
		}

		private void AntiGapcloser_OnEnemyGabcloser(ActiveGapcloser gapcloser)
		{
			if(!R.IsReady() || !gapcloser.Sender.IsValidTarget(R.Range) ||
				!Program.Menu.Item("useR_Antigapclose").GetValue<bool>())
				return;
			R.CastOnUnit(gapcloser.Sender, Packets());
		}

		private void Interrupter_OnPosibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
		{
			if(!R.IsReady() || !unit.IsValidTarget(R.Range) || unit.IsAlly ||
				!Program.Menu.Item("useR_Interrupt").GetValue<bool>())
				return;
			R.CastOnUnit(unit, Packets());
		}

		private void UpdateSpellranges()
		{
			if (Environment.TickCount - SpellRangeTick < 100) 
				return;
			SpellRangeTick = Environment.TickCount;

			Q.Range = Orbwalking.GetRealAutoAttackRange(ObjectManager.Player);
			E.Range = 550 + (9 * (ObjectManager.Player.Level - 1));
			R.Range = 550 + (9 * (ObjectManager.Player.Level - 1));
		}

		private void Drawing_OnDraw(EventArgs args)
		{
			if(Program.Menu.Item("Draw_Disabled").GetValue<bool>())
				return;

			foreach (var tar in ObjectManager.Get<Obj_AI_Hero>().Where(enemy => enemy.IsValidTarget(2000)))
			{
				Utility.DrawCircle(GetJumpposition(tar), 150,Color.Blue );
			}
			if(Program.Menu.Item("Draw_Q").GetValue<bool>())
				if(Q.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);

			if(Program.Menu.Item("Draw_W").GetValue<bool>())
				if(W.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);

			if(Program.Menu.Item("Draw_E").GetValue<bool>())
				if(E.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, E.Range - 1, E.IsReady() ? Color.Green : Color.Red);

			if(Program.Menu.Item("Draw_R").GetValue<bool>())
				if(R.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, R.Range - 2, R.IsReady() ? Color.Green : Color.Red);

		}
	}
}
