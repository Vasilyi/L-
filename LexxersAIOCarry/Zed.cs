using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace UltimateCarry
{
	class Zed : Champion 
	{
		public Spell Q;
		public Spell W;
		public Spell E;
		public Spell R;

		public Obj_AI_Minion CloneW = null;
		public bool CloneWCreated = false;
		public bool CloneWFound = false;
		public int CloneWTick = 0;
		public int WCastTick = 0;

		public static Obj_AI_Minion CloneR = null;
		public bool CloneRCreated = false;
		public bool CloneRFound = false;
		public int CloneRTick = 0;
		public int RCastTick = 0;
		public Vector3 CloneRNearPosition;

		public int Delay2 = 300;
		public int DelayTick2 = 0;

        public Zed()
        {
			LoadMenu();
			LoadSpells();

			Drawing.OnDraw += Drawing_OnDraw;
			Game.OnGameUpdate += Game_OnGameUpdate;
			GameObject.OnCreate += OnSpellCast;
			PluginLoaded();
		}

		private void LoadMenu()
		{
			Program.Menu.AddSubMenu(new Menu("TeamFight", "TeamFight"));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useQ_TeamFight", "Use Q").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useW_TeamFight", "Use W").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useE_TeamFight", "Use E").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useR_TeamFight", "Use R").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("followW_TeamFight", "Follow W in Range").SetValue(true));

			Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useQ_Harass", "Use Q").SetValue(true));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useW_Harass", "Use W").SetValue(true));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("followW_Harass", "Follow W").SetValue(false));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useE_Harass", "Use E").SetValue(true));

			Program.Menu.AddSubMenu(new Menu("LaneClear", "LaneClear"));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useQ_LaneClear", "Use Q").SetValue(true));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useE_LaneClear", "Use E").SetValue(true));

			Program.Menu.AddSubMenu(new Menu("LastHit", "LastHit"));
			Program.Menu.SubMenu("LastHit").AddItem(new MenuItem("useQ_LastHit", "Use Q").SetValue(true));
			
			Program.Menu.AddSubMenu(new Menu("Passive", "Passive"));
			Program.Menu.SubMenu("Passive").AddItem(new MenuItem("useE_Passive", "Auto E").SetValue((new KeyBind("H".ToCharArray()[0], KeyBindType.Toggle, true))));

			Program.Menu.AddSubMenu(new Menu("Drawing", "Drawing"));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Q", "Draw Q").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_W", "Draw W").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_E", "Draw E").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_R", "Draw R").SetValue(true));

		}

		private void LoadSpells()
		{
			Q = new Spell(SpellSlot.Q, 900);
			Q.SetSkillshot(0.235f, 50f, 1700, false, SkillshotType.SkillshotLine);

			W = new Spell(SpellSlot.W, 550);
			
			E = new Spell(SpellSlot.E, 290);

			R = new Spell(SpellSlot.R, 600);
		}

		private void Game_OnGameUpdate(EventArgs args)
		{

			if(CloneWCreated && !CloneWFound)
				SearchForClone("W");
			if(CloneRCreated && !CloneRFound)
				SearchForClone("R");

			if(CloneW != null && (CloneWTick < Environment.TickCount - 4000))
			{
				CloneW = null;
				CloneWCreated = false;
				CloneWFound = false;
			}

			if(CloneR != null && (CloneRTick < Environment.TickCount - 6000))
			{
				CloneR = null;
				CloneRCreated = false;
				CloneRFound = false;
			}

			if(Program.Menu.Item("useE_Passive").GetValue<KeyBind>().Active )
				CastE();

			switch(Program.Orbwalker.ActiveMode)
			{
				case Orbwalking.OrbwalkingMode.Combo:
					if(Program.Menu.Item("useQ_TeamFight").GetValue<bool>())
						CastQEnemy();
					if(Program.Menu.Item("useE_TeamFight").GetValue<bool>())
						CastE();
					if(Program.Menu.Item("useW_TeamFight").GetValue<bool>())
						CastWEnemy();
					if(Program.Menu.Item("useR_TeamFight").GetValue<bool>())
						CastR();
					break;
				case Orbwalking.OrbwalkingMode.Mixed:
					if(Program.Menu.Item("useQ_Harass").GetValue<bool>())
						CastQEnemy();
					if(Program.Menu.Item("useE_Harass").GetValue<bool>())
						CastE();
					if(Program.Menu.Item("useW_Harass").GetValue<bool>())
						CastWEnemy();
					break;
				case Orbwalking.OrbwalkingMode.LaneClear:
					if(Program.Menu.Item("useQ_LaneClear").GetValue<bool>())
					{
						CastQEnemy();
						CastQMinion();
					}
					if(Program.Menu.Item("useE_LaneClear").GetValue<bool>())
						CastE();
					break;
				case Orbwalking.OrbwalkingMode.LastHit:
					if(Program.Menu.Item("useQ_LastHit").GetValue<bool>())
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
			if (!Q.IsReady())
				return;
			
			if(Program.Menu.Item("useR_TeamFight").GetValue<bool>())
				if(Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
					if(R.IsReady() && CloneR == null && Q.IsReady() && E.IsReady() && IsEnoughEnergy(GetCost(SpellSlot.Q) + GetCost(SpellSlot.W) + GetCost(SpellSlot.E) + GetCost(SpellSlot.R)))
						return;

			if((Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && Program.Menu.Item("useW_Harass").GetValue<bool>() ||
				(Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo  && Program.Menu.Item("useW_TeamFight").GetValue<bool>())))
				if (W.IsReady() && CloneW == null)
					return;

			var target = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
			if (target != null)
				if (target.IsValidTarget(Q.Range) && Q.GetPrediction(target).Hitchance >= HitChance.High)
				{
					Q.Cast(target, Packets());
					return;
				}

			if(CloneW != null)
                foreach (var hero in Program.Helper.EnemyTeam.Where(hero => (hero.Distance(CloneW.Position) < Q.Range) && hero.IsValidTarget() && hero.IsVisible))
				{
					Q.Cast(hero.Position, Packets());
					return;
				}

			if(CloneR == null )
				return;
            foreach (var hero in Program.Helper.EnemyTeam.Where(hero => (hero.Distance(CloneR.Position) < Q.Range) && hero.IsValidTarget() && hero.IsVisible))
				Q.Cast(hero.Position, Packets());
		}

		private void CastQMinion()
		{
			if(!Q.IsReady())
				return;
			var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.NotAlly);
			foreach(var minion in allMinions)
			{
				if(!minion.IsValidTarget())
					continue;
				var minionInRangeAa = Orbwalking.InAutoAttackRange(minion);
				var minionInRangeSpell = minion.Distance(ObjectManager.Player) <= Q.Range;
				var minionKillableAa = DamageLib.getDmg(minion, DamageLib.SpellType.AD) >= minion.Health;
				var minionKillableSpell = DamageLib.getDmg(minion, DamageLib.SpellType.Q) >= minion.Health;
				var lastHit = Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit;
				var laneClear = Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear;

				if((lastHit && minionInRangeSpell && minionKillableSpell) && ((minionInRangeAa && !minionKillableAa) || !minionInRangeAa))
					Q.Cast(minion.Position, Packets());
				else if((laneClear && minionInRangeSpell && !minionKillableSpell) && ((minionInRangeAa && !minionKillableAa) || !minionInRangeAa))
					Q.Cast(minion.Position, Packets());
			}
		}

		private void CastWEnemy()
		{

			if(Program.Menu.Item("useR_TeamFight").GetValue<bool>())
			if (Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
				if(R.IsReady() && CloneR == null && Q.IsReady() && E.IsReady() && IsEnoughEnergy(GetCost(SpellSlot.Q) + GetCost(SpellSlot.W) + GetCost(SpellSlot.E) + GetCost(SpellSlot.R)))
					return;
			if(Delay2 >= Environment.TickCount - DelayTick2)
				return;
			DelayTick2 = Environment.TickCount;
			var target = SimpleTs.GetTarget(W.Range + Q.Range, SimpleTs.DamageType.Physical);
			if (IsTeleportToClone("W"))
			{
				if (ObjectManager.Player.Health*100/ObjectManager.Player.MaxHealth > target.Health*100/target.MaxHealth &&
				    (Program.Menu.Item("followW_TeamFight").GetValue<bool>() &&
				     Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo) ||
				    (Program.Menu.Item("followW_Harass").GetValue<bool>() &&
				     Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed))
					if (CloneW.Position.Distance(target.Position) < ObjectManager.Player.Position.Distance(target.Position))
						W.Cast();
			}
			else
			{
				if (target == null)
					return;
				if ((W.IsReady() && Q.IsReady() && target.IsValidTarget(Q.Range + W.Range) &&
				     IsEnoughEnergy(GetCost(SpellSlot.Q) + GetCost(SpellSlot.W)))
				    ||
				    (W.IsReady() && E.IsReady() && target.IsValidTarget(W.Range + E.Range) &&
				     IsEnoughEnergy(GetCost(SpellSlot.W) + GetCost(SpellSlot.E)))
				    ||
				    (W.IsReady() && target.IsValidTarget(E.Range + Orbwalking.GetRealAutoAttackRange(target)) &&
				     Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo))
				{
					W.Cast(target.Position, Packets());
				}
			}
		}

		


		private void CastE()
		{
			if(!E.IsReady())
				return;

			if(Program.Menu.Item("useR_TeamFight").GetValue<bool>())
				if(Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
					if(R.IsReady() && CloneR == null && Q.IsReady() && E.IsReady() && IsEnoughEnergy(GetCost(SpellSlot.Q) + GetCost(SpellSlot.W) + GetCost(SpellSlot.E) + GetCost(SpellSlot.R)))
						return;

			var target = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);
			if(target != null)
			{
				E.Cast();
				return;
			}
			if(CloneW != null)
                if (Program.Helper.EnemyTeam.Any(hero => (hero.Distance(CloneW.Position) < E.Range) && hero.IsValidTarget() && hero.IsVisible))
				{
					E.Cast();
					return;
				}

			if(CloneR != null)
                if (Program.Helper.EnemyTeam.Any(hero => (hero.Distance(CloneR.Position) < E.Range) && hero.IsValidTarget() && hero.IsVisible))
				{
					E.Cast();
					return;
				}

			if(Program.Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.LaneClear)
				return;
			var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, E.Range, MinionTypes.All, MinionTeam.NotAlly);
			foreach(var minion in allMinions)
			{
				if (!minion.IsValidTarget(E.Range)) 
					continue;
				if((DamageLib.getDmg(minion, DamageLib.SpellType.E) > minion.Health) || (DamageLib.getDmg(minion, DamageLib.SpellType.E) + 100 < minion.Health))
					E.Cast();
			}
		}

		private void CastR()
		{
			if(!R.IsReady())
				return;

			var target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Physical);
			if (!IsTeleportToClone("R"))
			{

				//var dmg = DamageLib.getDmg(target, DamageLib.SpellType.Q);
				//dmg += DamageLib.getDmg(target, DamageLib.SpellType.E);
				//dmg += DamageLib.getDmg(target, DamageLib.SpellType.R);
				//dmg += DamageLib.getDmg(target, DamageLib.SpellType.AD)*2;

				//if (dmg >= target.Health)
				//{
					R.Cast(target);
					SearchForClone("R");
				//}
			}
			else
				if(ObjectManager.Player.Health * 100 / ObjectManager.Player.MaxHealth < target.Health * 100 / target.MaxHealth)
					if(CloneR.Position.Distance(target.Position) < ObjectManager.Player.Position.Distance(target.Position))
						R.Cast();
		}

		private bool IsTeleportToClone(string spell)
		{
			if (spell == "W")
				if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "zedw2")
					return true;
			if (spell != "R") 
				return false;
			return ObjectManager.Player.Spellbook.GetSpell(SpellSlot.R).Name == "ZedR2";
		}

		private bool IsEnoughEnergy(float energy)
		{
			return energy <= ObjectManager.Player.Mana;
		}

		private float GetCost(SpellSlot spell)
		{
			if(SpellSlot.Q == spell)
				return 50 + (5 * Q.Level);
			if(SpellSlot.W == spell)
				return 15 + (5 * W.Level);
			if(SpellSlot.E == spell)
				return 50;
			return 0;
		}

		private void SearchForClone(string p)
		{
			Obj_AI_Minion shadow;
			if(p != null && p == "W")
			{
				shadow = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(hero => (hero.Name == "Shadow" && hero.IsAlly && (hero != CloneR)));
				if(shadow != null)
				{
					CloneW = shadow;
					CloneWFound = true;
					CloneWTick = Environment.TickCount;
				}
			}
			if (p == null || p != "R") 
				return;
			shadow = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(hero => ((hero.ServerPosition.Distance(CloneRNearPosition)) < 50) && hero.Name == "Shadow" && hero.IsAlly && hero != CloneW);
			if (shadow == null) 
				return;
			CloneR = shadow;
			CloneRFound = true;
			CloneRTick = Environment.TickCount;
		}

		private void OnSpellCast(GameObject sender, EventArgs args)
		{
			var spell = (Obj_SpellMissile)sender;
			var unit = spell.SpellCaster.Name;
			var name = spell.SData.Name;

			if(unit == ObjectManager.Player.Name && name == "ZedShadowDashMissile")
				CloneWCreated = true;
			if(unit == ObjectManager.Player.Name && name == "ZedUltMissile")
			{
				CloneRCreated = true;
				CloneRNearPosition = ObjectManager.Player.ServerPosition;
			}
		}
	}
}
