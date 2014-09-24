using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace UltimateCarry
{
	class Xerath : Champion 
	{
		public static Spell Q;
		public static Spell W;
		public static Spell E;
		public static Spell R;
		public static int UltTick;

		public Xerath()
        {
			LoadMenu();
			LoadSpells();

			Drawing.OnDraw += Drawing_OnDraw;
			Game.OnGameUpdate += Game_OnGameUpdate;
			Game.OnGameSendPacket += Game_OnGameSendPacket2;
			Interrupter.OnPosibleToInterrupt += Interrupter_OnPosibleToInterrupt;
			PluginLoaded();
		}

		private void LoadMenu()
		{
			Program.Menu.AddSubMenu(new Menu("TeamFight", "TeamFight"));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useQ_TeamFight", "Use Q").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useW_TeamFight", "Use W").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useE_TeamFight", "Use E").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useR_TeamFight", "Use R").SetValue(true));

			Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useQ_Harass", "Use Q").SetValue(true));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useW_Harass", "Use W").SetValue(true));
			AddManaManager("Harass", 40);

			Program.Menu.AddSubMenu(new Menu("LaneClear", "LaneClear"));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useQ_LaneClear", "Use Q").SetValue(true));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useW_LaneClear", "Use W").SetValue(true));
			AddManaManager("LaneClear", 20);

			Program.Menu.AddSubMenu(new Menu("Passive", "Passive"));
			Program.Menu.SubMenu("Passive").AddItem(new MenuItem("useE_Interupt", "Use E Interrupt").SetValue(true));
			Program.Menu.SubMenu("Passive").AddItem(new MenuItem("useR_KS", "Use R for KS").SetValue((new KeyBind("T".ToCharArray()[0], KeyBindType.Press))));
			Program.Menu.SubMenu("Passive").AddItem(new MenuItem("useR_safe", "Saferange").SetValue(new Slider(700, 2000, 0)));
			Program.Menu.SubMenu("Passive").AddItem(new MenuItem("useR_Killabletext", "Write if is killable").SetValue(true));
			Program.Menu.SubMenu("Passive").AddItem(new MenuItem("useR_Killableping", "Ping if is killable").SetValue(true));
			Program.Menu.SubMenu("Passive").AddItem(new MenuItem("useR_warning", "warn if active").SetValue(false));
			
			Program.Menu.AddSubMenu(new Menu("Drawing", "Drawing"));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Q", "Draw Q").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_W", "Draw W").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_E", "Draw E").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_R", "Draw R").SetValue(true));
		}

		private void LoadSpells()
		{
			Q = new Spell(SpellSlot.Q, 1550);
			Q.SetSkillshot(0.6f, 100, float.MaxValue, false, SkillshotType.SkillshotLine);
			Q.SetCharged("XerathArcanopulseChargeUp", "XerathArcanopulseChargeUp", 750, 1550, 1.5f);

			W = new Spell(SpellSlot.W, 1000);
			W.SetSkillshot(0.7f, 125, float.MaxValue, false, SkillshotType.SkillshotCircle);
		
			E = new Spell(SpellSlot.E, 1150);
			E.SetSkillshot(0.2f, 60, 1400, true, SkillshotType.SkillshotLine);
			
			R = new Spell(SpellSlot.R, 675);
			R.SetSkillshot(0.7f, 120, float.MaxValue, false, SkillshotType.SkillshotCircle);
		}

		private void Game_OnGameUpdate(EventArgs args)
		{

			R_Check();
			
			switch(Program.Orbwalker.ActiveMode)
			{
				case Orbwalking.OrbwalkingMode.Combo:
					if(Program.Menu.Item("useQ_TeamFight").GetValue<bool>())
						QEnemy();
					if (Program.Menu.Item("useW_TeamFight").GetValue<bool>())
						Cast_BasicCircleSkillshot_Enemy(W, SimpleTs.DamageType.Magical);
					if(Program.Menu.Item("useE_TeamFight").GetValue<bool>())
						Cast_BasicLineSkillshot_Enemy(E, SimpleTs.DamageType.Magical);
					break;
				case Orbwalking.OrbwalkingMode.Mixed:
					if(Program.Menu.Item("useQ_Harass").GetValue<bool>() && (ManaManagerAllowCast(Q) || Q.IsCharging) )
						QEnemy();
					if(Program.Menu.Item("useW_Harass").GetValue<bool>() && ManaManagerAllowCast(W))
						Cast_BasicCircleSkillshot_Enemy(W, SimpleTs.DamageType.Magical);
					break;
				case Orbwalking.OrbwalkingMode.LaneClear:
					if(Program.Menu.Item("useQ_LaneClear").GetValue<bool>() && (ManaManagerAllowCast(Q) || Q.IsCharging))
						QFarm();
					if(Program.Menu.Item("useW_LaneClear").GetValue<bool>() && ManaManagerAllowCast(Q))
						Cast_BasicCircleSkillshot_AOE_Farm(W);
					break;
			}
		}

		private void R_Check()
		{
			if (!Program.Menu.Item("useR_KS").GetValue<KeyBind>().Active)
				return;
			if (!R.IsReady() && !IsShooting() )
				return;
			if (Utility.CountEnemysInRange(Program.Menu.Item("useR_safe").GetValue<Slider>().Value) >= 1 && !IsShooting())
				return;
			R.Range = GetRRange();
			Obj_AI_Hero[] lowesttarget = {null};
			foreach (var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget(R.Range)).Where(enemy => lowesttarget[0] == null || lowesttarget[0].Health > enemy.Health))
			{
				lowesttarget[0] = enemy;
			}
			if(lowesttarget[0] != null && lowesttarget[0].Health < (DamageLib.getDmg(lowesttarget[0], DamageLib.SpellType.R) * 0.9) && Environment.TickCount -UltTick  >= 700) 
			{
				R.Cast(lowesttarget[0], Packets());
				UltTick = Environment.TickCount;
				return;
			}
			if (IsShooting())
			{
				var target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
				R.Cast(target, Packets());
			}

		}

		private float GetRRange()
		{
			return 2000 + (1100 * R.Level);
		}

		private void QEnemy()
		{
			if (!Q.IsReady())
				return;
			var target = SimpleTs.GetTarget(Q.ChargedMaxRange, SimpleTs.DamageType.Physical);
			if (!target.IsValidTarget(Q.ChargedMaxRange))
				return;
			if (Q.IsCharging)
			{
				if (Q.GetPrediction(target).Hitchance >= HitChance.High)
					Q.Cast(target, Packets());
				return;
			}
			Q.StartCharging();

		}

		public void QFarm()
		{
			if(!Q.IsReady())
				return;
			var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q.ChargedMaxRange,MinionTypes.All,MinionTeam.NotAlly );
			if(minions.Count <= 0)
				return;
			if(Q.IsCharging)
			{
				var locQ = Q.GetLineFarmLocation(minions);
				if(minions.Count == minions.Count(m => ObjectManager.Player.Distance(m) < Q.Range) && locQ.MinionsHit > 0 && locQ.Position.IsValid())
					Q.Cast(locQ.Position);
			}
			else if(minions.Count > 0)
				Q.StartCharging();
		}

		private void Interrupter_OnPosibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
		{
			if(!Program.Menu.Item("useE_Interupt").GetValue<bool>())
				return;
			if(ObjectManager.Player.Distance(unit) < E.Range && E.IsReady() && unit.IsEnemy)
				E.Cast(unit, Packets());
		}

		private void Game_OnGameSendPacket2(GamePacketEventArgs args)
		{
			if(args.PacketData[0] == Packet.C2S.Move.Header && IsShooting())
				args.Process = false;
		}

		private bool IsShooting()
		{
			return Environment.TickCount - UltTick < 250 || ObjectManager.Player.HasBuff("XerathLocusOfPower2");
		}

		private void Drawing_OnDraw(EventArgs args)
		{
			if (Program.Menu.Item("Draw_Disabled").GetValue<bool>())
				return;

			if (Program.Menu.Item("Draw_Q").GetValue<bool>())
				if (Q.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);

			if (Program.Menu.Item("Draw_W").GetValue<bool>())
				if (W.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);

			if (Program.Menu.Item("Draw_E").GetValue<bool>())
				if (E.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, E.Range, E.IsReady() ? Color.Green : Color.Red);

			if (Program.Menu.Item("Draw_R").GetValue<bool>())
				if (R.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);
			
			var victims = "";
			
			foreach(var target in Program.Helper.EnemyInfo.Where(x =>
			 x.Player.IsVisible && x.Player.IsValidTarget(GetRRange()) && DamageLib.getDmg(x.Player, DamageLib.SpellType.R)* 0.9 >= x.Player.Health ))
			{
				victims += target.Player.ChampionName + " ";

				if(!Program.Menu.Item("useR_Killableping").GetValue<bool>() ||
					(target.LastPinged != 0 && Environment.TickCount - target.LastPinged <= 11000))
					continue;
				if(!(ObjectManager.Player.Distance(target.Player) > 1800) ||
					(!target.Player.IsVisible))
					continue;
				Program.Helper.Ping(target.Player.Position);
				target.LastPinged = Environment.TickCount;
			}
			if(victims != "" && Program.Menu.Item("useR_Killabletext").GetValue<bool>())
				if (!Program.Menu.Item("useR_KS").GetValue<KeyBind>().Active && R.IsReady())
				{
					Drawing.DrawLine(Drawing.Width * 0.44f - 50, Drawing.Height * 0.7f - 2, Drawing.Width * 0.44f + 350, Drawing.Height * 0.7f - 2, 25, Color.Black);		
					Drawing.DrawText(Drawing.Width * 0.44f, Drawing.Height * 0.7f, Color.GreenYellow, "[Press " + Convert.ToChar( Program.Menu.Item("useR_KS").GetValue<KeyBind>().Key)  + "] Ult can kill: " + victims);
				}
			if (victims == "")
				if (Program.Menu.Item("useR_warning").GetValue<bool>() && Program.Menu.Item("useR_KS").GetValue<KeyBind>().Active)
				{
					Drawing.DrawLine(Drawing.Width * 0.44f - 50, Drawing.Height * 0.7f - 2, Drawing.Width * 0.44f + 350, Drawing.Height * 0.7f - 2, 25, Color.Black);		
					Drawing.DrawText(Drawing.Width * 0.44f, Drawing.Height * 0.7f, Color.OrangeRed, "Warning: [Press " + Convert.ToChar(Program.Menu.Item("useR_KS").GetValue<KeyBind>().Key) + "] to disable KS ");
				}
		}
	}
}
