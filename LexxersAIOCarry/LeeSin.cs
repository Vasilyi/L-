using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace UltimateCarry
{
	class LeeSin : Champion
	{
		public Spell Q1;
		public Spell Q2;
		public Spell W1;
		public Spell W2;
		public Spell E1;
		public Spell E2;
		public Spell R;
		public Spell RPred;
		public Spell WardPred;
		public int Delay = 100;
		public int Delaytick = 0;
		public Vector3 LastBackPos = default(Vector3);

		public LeeSin()
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

			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useModus_Q", "Use Q").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useModus_W", "Use W").SetValue(new StringList(new[] { "Not", "for Passive", "Smart" }, 2)));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useModus_E", "Use E").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useModus_Passive_Q", "Passive Use Q").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useModus_Passive_W", "Passive Use W").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useModus_Passive_E", "Passive Use E").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useModus_Passive_E1_mode", "E 1 Priority").SetValue(new StringList(new[] { "Directly", "for Passive" })));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useModus_Passive_E2_mode", "E 2 Priority").SetValue(new StringList(new[] { "Slow", "Passive", "Smart" }, 2)));

			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useModus_Mode_key", "Switch mode key").SetValue((new KeyBind("T".ToCharArray()[0], KeyBindType.Toggle))));
			Program.Menu.Item("useModus_Mode_key").ValueChanged += SwitchMode;
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useModus_Mode", "Mode").SetValue(new StringList(new[] { "Teamfight", "Insec" })));

			Program.Menu.AddSubMenu(new Menu("LaneClear", "LaneClear"));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("LaneClear_Q", "Use Q").SetValue(true));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("LaneClear_W", "Use W at Health").SetValue(new Slider(75)));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("LaneClear_E", "Use E").SetValue(true));

			Program.Menu.AddSubMenu(new Menu("KillSteal", "KillSteal"));
			Program.Menu.SubMenu("KillSteal").AddItem(new MenuItem("use_ComboKS", "Use KS").SetValue(true));
			Program.Menu.SubMenu("KillSteal").AddItem(new MenuItem("useQ_1_Kill", "Use first Q").SetValue(true));
			Program.Menu.SubMenu("KillSteal").AddItem(new MenuItem("useR_Kill", "Use R").SetValue(true));
			Program.Menu.SubMenu("KillSteal").AddItem(new MenuItem("use_MinionKill", "Fail Q minion kill").SetValue(true));

			//Program.Menu.AddSubMenu(new Menu("Run like Hell", "RunlikeHell"));
			//Program.Menu.SubMenu("RunlikeHell").AddItem(new MenuItem("RunlikeHell_key", "Run Key").SetValue((new KeyBind("A".ToCharArray()[0], KeyBindType.Press))));
			//Program.Menu.SubMenu("RunlikeHell").AddItem(new MenuItem("RunlikeHell_range", "Mousecirle").SetValue(new Slider(500,100,1000)));
			//Program.Menu.SubMenu("RunlikeHell").AddItem(new MenuItem("RunlikeHell_useQ", "Use Q").SetValue(true));
			//Program.Menu.SubMenu("RunlikeHell").AddItem(new MenuItem("RunlikeHell_useW", "Use W").SetValue(true));
			//Program.Menu.SubMenu("RunlikeHell").AddItem(new MenuItem("RunlikeHell_useWard", "Use Wards").SetValue(true));

			Program.Menu.AddSubMenu(new Menu("Drawing", "Drawing"));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Q", "Draw Q").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_W", "Draw W").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_E", "Draw E").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_R", "Draw R").SetValue(true));
		}

		private void SwitchMode(object sender, OnValueChangeEventArgs e)
		{
			Program.Menu.Item("useModus_Mode")
				.SetValue(Program.Menu.Item("useModus_Mode").GetValue<StringList>().SelectedIndex == 0
					? new StringList(new[] { "Teamfight", "Insec" }, 1)
					: new StringList(new[] { "Teamfight", "Insec" }));
		}

		private void LoadSpells()
		{
			Q1 = new Spell(SpellSlot.Q, 1000);
			Q1.SetSkillshot(0.25f, 65, 1800, true, SkillshotType.SkillshotLine);

			Q2 = new Spell(SpellSlot.Q, 1200);

			W1 = new Spell(SpellSlot.W, 700);

			W2 = new Spell(SpellSlot.W);

			E1 = new Spell(SpellSlot.E, 440);

			E2 = new Spell(SpellSlot.E, 600);

			R = new Spell(SpellSlot.R, 375);

			RPred = new Spell(SpellSlot.R, 1000);
			RPred.SetSkillshot(0.25f, 90, 1, true, SkillshotType.SkillshotLine);

			WardPred = new Spell(SpellSlot.Trinket, 500);
			WardPred.SetSkillshot(0.25f, 10, float.MaxValue, false, SkillshotType.SkillshotCircle);
		}

		private void Game_OnGameUpdate(EventArgs args)
		{

			if(Environment.TickCount - Delaytick <= Delay)
				return;
			Delay = Environment.TickCount;

			if(KillStealCombo())
				return;

			//RunLikeHell();

			switch(Program.Orbwalker.ActiveMode)
			{
				case Orbwalking.OrbwalkingMode.Combo:
					Combo();
					break;
				case Orbwalking.OrbwalkingMode.Mixed:
					Harass();
					break;
				case Orbwalking.OrbwalkingMode.LaneClear:
					Laneclear();
					break;
			}
		}

		//private void RunLikeHell()
		//{
		//	if (Program.Menu.Item("RunlikeHell_key").GetValue<KeyBind>().Active)
		//	{
		//		if (Q1Ready() && Program.Menu.Item("RunlikeHell_useQ").GetValue<bool>())
		//		{
					
					//var allminionsenemy = MinionManager.GetMinions(Game.CursorPos, Program.Menu.Item("RunlikeHell_range").GetValue<Slider>().Value, MinionTypes.All, MinionTeam.NotAlly);
					//foreach (
					//	var minion in
					//		allminionsenemy.Where(
					//			minion =>
					//				minion.Distance(ObjectManager.Player) <= Q1.Range && minion.Distance(ObjectManager.Player) >= 550 &&
					//				minion.Health > DamageLib.getDmg(minion, DamageLib.SpellType.Q)))
					//{
					//	var collision = Q1.GetPrediction(minion).CollisionObjects.Count;
					//	Chat.Print(collision.ToString());
					//	if (collision == 0)
					//	{
					//		Q1.Cast(minion.Position, Packets());
					//		return;
					//	}
					//	if(collision == 1 && Q1.GetPrediction(minion).CollisionObjects.First().Health < SmiteDamage() && SmiteReady() && Q1.GetPrediction(minion).CollisionObjects.First().IsMinion )
					//	{
					//		ObjectManager.Player.SummonerSpellbook.CastSpell(Activator.GetSummonerSpellSlot("summonersmite"),Q1.GetPrediction(minion).CollisionObjects.First());
					//		return;
					//	}
					//}
		//		}
		//		if(W1Ready() && Program.Menu.Item("RunlikeHell_useW").GetValue<bool>())
		//		{
		//			Obj_AI_Base bestminion = null;
		//			foreach(var friend in ObjectManager.Get<Obj_AI_Minion>().Where(obj => obj.IsAlly && obj.Distance(ObjectManager.Player) >= 350 && obj.Distance(ObjectManager.Player) <= W1.Range))
		//			{
		//				if(bestminion == null || bestminion.Distance(ObjectManager.Player) < friend.Distance(ObjectManager.Player))
		//					bestminion = friend;
		//			}
		//			foreach(var friend in ObjectManager.Get<Obj_AI_Hero>().Where(obj => obj.IsAlly && obj.Distance(ObjectManager.Player) >= 350 && obj.Distance(ObjectManager.Player) <= W1.Range ))
		//			{
		//				if(bestminion == null || bestminion.Distance(ObjectManager.Player) < friend.Distance(ObjectManager.Player))
		//					bestminion = friend;
		//			}
		//			if (bestminion == null)
		//			{
						
		//			}
		//		}
		//	}
		//}

		//public Vector3 runwardposition()
		//{
		//	var me = ObjectManager.Player.Position;
		//	var mouse = Game.CursorPos;

		//	var newpos = mouse -me ;
		//	newpos.Normalize();
		//	return me + (newpos * WardPred.Range);
		//}
		//private double SmiteDamage()
		//{
		//	var level = ObjectManager.Player.Level;
		//	int[] stages = { 20 * level + 370, 30 * level + 330, 40 * level + 240, 50 * level + 100 };
		//	return stages.Max();
		//}

		private void Harass()
		{
			Program.Orbwalker.SetMovement(true);
			Program.Orbwalker.SetAttacks(true);
			PassiveE();
			PassiveW();
			PassiveQ();
			AutoQ();
			if(Q2Ready() && Q2.IsReady())
			{
				var qtarget = getQ2Target();
				if(Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
					if(qtarget != null && CanJump(qtarget))
						Q2.Cast();
				if(Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
					if(qtarget != null)
						Q2.Cast();
			}
			AutoE();
			if(Q1.IsReady() || E1.IsReady())
				return;
			AutoW();

		}

		private void JumpBack()
		{
			var target = SimpleTs.GetTarget(500, SimpleTs.DamageType.Physical);
			if(target == null)
				return;
			Obj_AI_Base backtarget = ObjectManager.Get<Obj_AI_Hero>()
				.Where(
					ally =>
						ally.IsValid && ally.IsAlly && !ally.IsMe && ally.Distance(ObjectManager.Player.ServerPosition) <= W1.Range &&
						ally.Distance(LastBackPos) <= 1000)
				.OrderByDescending(x => x.Distance(target.Position)).FirstOrDefault() ??
									  (Obj_AI_Base)ObjectManager.Get<Obj_AI_Minion>()
					.Where(
						minion =>
							minion.IsValid && minion.IsAlly && minion.Distance(ObjectManager.Player.ServerPosition) <= W1.Range &&
							minion.Distance(LastBackPos) <= 1000)
					.OrderByDescending(x => x.Distance(target.Position)).FirstOrDefault();

			W1.CastOnUnit(backtarget, Packets());
		}

		private bool CanJump(Obj_AI_Base target)
		{
			const int backrange = 600;
			if(!W1Ready() || !Q2Ready())
				return false;

			var backPos = ObjectManager.Player.Position;
			var range = W1.Range;

			Obj_AI_Base[] nearJumptarget = { null };
			foreach(var friendminion in ObjectManager.Get<Obj_AI_Minion>().Where(minion => minion.IsAlly && minion.Distance(backPos) <= backrange && minion.Distance(target) <= W1.Range))
				nearJumptarget[0] = friendminion;

			foreach(var friend in ObjectManager.Get<Obj_AI_Hero>().Where(friend => friend.IsAlly && !friend.IsMe && friend.Distance(backPos) <= backrange && friend.Distance(target) <= W1.Range))
				nearJumptarget[0] = friend;

			if(nearJumptarget[0] != null)
				LastBackPos = ObjectManager.Player.Position;
			return nearJumptarget[0] != null;
		}

		private void Combo()
		{
			switch(Program.Menu.Item("useModus_Mode").GetValue<StringList>().SelectedIndex)
			{
				case 0:
					Harass();
					break;
				case 1:
					InsecCombo();
					break;
			}
		}

		private void InsecCombo()
		{
			PassiveE();

			if(ObjectManager.Player.Level < 6)
			{
				Harass();
				return;
			}

			var rtarget = SimpleTs.GetTarget(Q1.Range, SimpleTs.DamageType.Physical);

			var wardSlot = AutoBushRevealer.GetAnyWardSlot();

			var wardpos = Getwardposition(rtarget);

			if(wardpos == default(Vector3))
				Harass();

			if(rtarget == null)
				return;
			if(Environment.TickCount - R.LastCastAttemptT > 0 && Environment.TickCount - R.LastCastAttemptT < 500 &&
				FlashReady())
			{
				if(Getwardposition(rtarget) != default(Vector3) && !Onwardposition())
				{
					ObjectManager.Player.SummonerSpellbook.CastSpell(Activator.GetSummonerSpellSlot("summonerflash"), Getwardposition(rtarget));
					return;
				}
			}
			if(Environment.TickCount - R.LastCastAttemptT < 1000)
			{
				if(Q2Ready() && rtarget == getQ2Target())
					Q2.Cast();
				else if(Q1Ready())
					Q1.Cast(rtarget, Packets());
			}

			if(!R.IsReady() && Environment.TickCount - R.LastCastAttemptT > 1000)
				Harass();

			if(ObjectManager.Player.Position.Distance(wardpos) < 300 && R.IsReady())
			{
				Program.Orbwalker.SetMovement(false);
				Program.Orbwalker.SetAttacks(false);
				Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(wardpos.X, wardpos.Y)).Send();
			}
			if(ObjectManager.Player.Position.Distance(wardpos) > 300)
			{
				Program.Orbwalker.SetMovement(true);
				Program.Orbwalker.SetAttacks(true);
			}


			if(Nearwardposition() && rtarget.IsValidTarget(R.Range))
			{
				R.CastOnUnit(rtarget, Packets());
				return;
			}

			if(Q2Ready() && getQ2Target() == rtarget && wardSlot != null && W1Ready() && 80 <= ObjectManager.Player.Mana && ObjectManager.Player.Position.Distance(wardpos) > 600 ||
				FlashReady() && getQ2Target().Distance(wardpos) <= 400 && ObjectManager.Player.Position.Distance(wardpos) > 400)
			{
				Q2.Cast();
				return;
			}

			if(Q1Ready() && (W1Ready() && ObjectManager.Player.Mana >= 130 && ObjectManager.Player.Position.Distance(wardpos) > 600 || 80 <= ObjectManager.Player.Mana && FlashReady()))
			{
				if(Q1.GetPrediction(rtarget).Hitchance >= HitChance.High)
				{
					Q1.Cast(rtarget, Packets());
					return;
				}
			}

			if(W1Ready() && ObjectManager.Player.Distance(wardpos) <= 600)
			{
				var jumpobj = GetJumpObject(wardpos);
				if(jumpobj != null)
				{
					W1.CastOnUnit(jumpobj, Packets());
					return;
				}
				if(AutoBushRevealer.GetAnyWardSlot() != null)
				{
                    AutoBushRevealer.GetAnyWardSlot().UseItem(wardpos);
				}
			}


		}

		private void Laneclear()
		{
			var arange = Orbwalking.GetRealAutoAttackRange(ObjectManager.Player);
			var allMinions = MinionManager.GetMinions(ObjectManager.Player.Position, Q1.Range, MinionTypes.All,
				MinionTeam.NotAlly, MinionOrderTypes.MaxHealth);
			if(allMinions.Count == 0)
				return;
			Obj_AI_Minion nearstMinion = null;
			foreach(var obj in ObjectManager.Get<Obj_AI_Minion>())
			{
				if(nearstMinion == null || nearstMinion.Position.Distance(ObjectManager.Player.Position) > obj.Position.Distance(ObjectManager.Player.Position))
					nearstMinion = obj;
			}

			if(Program.Menu.Item("LaneClear_W").GetValue<Slider>().Value >
				ObjectManager.Player.Health / ObjectManager.Player.MaxHealth * 100)
			{
				if(W1.IsReady())
				{
					if(W1Ready() && PassiveDown())
						if(nearstMinion.Distance(ObjectManager.Player.Position) <= arange)
							W1.CastOnUnit(ObjectManager.Player, Packets());

					if(W2Ready() && (PassiveDown() || Environment.TickCount - E1.LastCastAttemptT > 2800))
						if(nearstMinion.Distance(ObjectManager.Player.Position) <= arange)
							W2.Cast();
				}
			}
			if(Q1.IsReady() && Program.Menu.Item("LaneClear_Q").GetValue<bool>())
			{
				if(Q1Ready() && PassiveDown())
					if(nearstMinion.Distance(ObjectManager.Player.Position) <= Q1.Range)
						Cast_Basic_Farm(Q1, true);

				if(Q2Ready() && (PassiveDown() || Environment.TickCount - Q1.LastCastAttemptT > 2800 || getQ2Target().Distance(ObjectManager.Player) >= 400))
					if(nearstMinion.Distance(ObjectManager.Player.Position) <= Q2.Range)
						Q2.Cast();
			}
			if(E1.IsReady() && Program.Menu.Item("LaneClear_E").GetValue<bool>())
			{
				if(E1Ready() && PassiveDown())
					if(nearstMinion.Distance(ObjectManager.Player.Position) <= E1.Range)
						E1.Cast();

				if(E2Ready() && (PassiveDown() || Environment.TickCount - E1.LastCastAttemptT > 2800))
					if(nearstMinion.Distance(ObjectManager.Player.Position) <= arange)
						E2.Cast();
			}
		}

		private Obj_AI_Base GetJumpObject(Vector3 pos)
		{
			Obj_AI_Minion[] nearstobj = { null };
			foreach(var obj in ObjectManager.Get<Obj_AI_Minion>().Where(
				obj => obj.IsAlly && obj.Position.Distance(pos) <= 200).Where(obj => nearstobj[0] == null || nearstobj[0].Position.Distance(pos) > obj.Position.Distance(pos)))
				nearstobj[0] = obj;
			return nearstobj[0];
		}


		private bool Onwardposition()
		{
			var target = SimpleTs.GetTarget(500, SimpleTs.DamageType.Physical);
			for(var i = 100; i < R.Range; i = i + 50)
			{
				if(ObjectManager.Player.Distance(Getwardposition(target, i)) <= 75)
					return true;
			}
			return false;
		}

		private bool Nearwardposition()
		{
			var target = SimpleTs.GetTarget(500, SimpleTs.DamageType.Physical);
			for(var i = 100; i < R.Range; i = i + 100)
			{
				if(ObjectManager.Player.Distance(Getwardposition(target, i)) <= 150)
					return true;
			}
			return false;
		}

		public Vector3 Getwardposition(Obj_AI_Hero target, int range = 250)
		{
			var pos = WardPred.GetPrediction(target).UnitPosition;
			var throwposition = default(Vector3);
			foreach(var friend in ObjectManager.Get<Obj_AI_Hero>())
			{
				if(!friend.IsMe && friend.Health / friend.MaxHealth * 100 > 15 && friend.IsAlly && friend.Distance(target) <= RPred.Range + friend.AttackRange - 25 + 400)
				{
					if(throwposition == default(Vector3) || (friend.Position.Distance(target.Position) <= throwposition.Distance(target.Position) && friend.Position.Distance(target.Position) >= 200))
						throwposition = friend.Position;
				}
			}

			foreach(
				var tower in
					ObjectManager.Get<Obj_AI_Turret>()
						.Where(tower => tower.IsAlly && tower.Health >= 100 && tower.Distance(target) <= RPred.Range  + 775 - 25)
				)
				throwposition = tower.Position;

			if(throwposition == default(Vector3))
				return default(Vector3);
			var newpos = pos - throwposition;
			newpos.Normalize();
			return pos + (newpos * range);
		}

		private void AutoW()
		{
			if(!W1.IsReady() || !W1Ready() || W2Ready())
				return;
			var target = SimpleTs.GetTarget(Orbwalking.GetRealAutoAttackRange(ObjectManager.Player) + 100, SimpleTs.DamageType.Physical);
			if(target == null)
				return;
			if(Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed && PassiveDown())
			{
				JumpBack();
				return;
			}
			switch(Program.Menu.Item("useModus_W").GetValue<StringList>().SelectedIndex)
			{
				case 0:
					return;
				case 1:
					if((PassiveDown()))
						W1.Cast(ObjectManager.Player, Packets());
					break;
				case 2:
					if(PassiveDown() && ObjectManager.Player.Health / ObjectManager.Player.MaxHealth * 100 < 80)
						W1.Cast(ObjectManager.Player, Packets());
					break;
			}
		}

		private void AutoQ()
		{
			if(!Q1.IsReady() || !Q1Ready())
				return;
			if(!Program.Menu.Item("useModus_Q").GetValue<bool>())
				return;
			var target = SimpleTs.GetTarget(Q1.Range, SimpleTs.DamageType.Physical);
			if(target == null)
				return;
			if(Q1.GetPrediction(target).Hitchance >= HitChance.High)
				Q1.Cast(target, Packets());
		}

		private void AutoE()
		{
			if(!E1Ready() || !E1.IsReady())
				return;
			if(!Program.Menu.Item("useModus_E").GetValue<bool>())
				return;
			var target = SimpleTs.GetTarget(E1.Range, SimpleTs.DamageType.Physical);
			if(target == null)
				return;
			switch(Program.Menu.Item("useModus_Passive_E1_mode").GetValue<StringList>().SelectedIndex)
			{
				case 0:
					E1.Cast();
					break;
				case 1:
					if((Orbwalking.CanAttack()) || (Orbwalking.CanAttack() && PassiveDown()))
						E1.Cast();
					break;
			}
		}

		private void PassiveQ()
		{
			if(!(Q1.IsReady() && Program.Menu.Item("useModus_Passive_Q").GetValue<bool>()))
				return;
			var qtarget = getQ2Target();
			if(qtarget == null)
				return;
			if(Q2Ready() && CanPassiveQ(qtarget))
				Q2.Cast();
		}

		private bool CanPassiveQ(Obj_AI_Base target)
		{
			var temptarget = SimpleTs.GetTarget(Q2.Range, SimpleTs.DamageType.Physical);
			if(target == null)
				return false;
			if(target == temptarget)
				return (PassiveDown() || Environment.TickCount - Q1.LastCastAttemptT > 2800);
			return ObjectManager.Player.Distance(temptarget) > ObjectManager.Player.Distance(target) && ObjectManager.Player.Distance(target) < 500;
		}

		private void PassiveW()
		{
			if(!(W1.IsReady() && Program.Menu.Item("useModus_Passive_W").GetValue<bool>()))
				return;
			if(!W2Ready())
				return;
			if(PassiveDown() || Environment.TickCount - W1.LastCastAttemptT > 2500)
				W2.Cast(ObjectManager.Player, Packets());
		}

		private void PassiveE()
		{
			if(!(E1.IsReady() && Program.Menu.Item("useModus_Passive_E").GetValue<bool>()))
				return;
			if(!E2Ready())
				return;
			switch(Program.Menu.Item("useModus_Passive_E2_mode").GetValue<StringList>().SelectedIndex)
			{
				case 0:
					E2.Cast();
					break;
				case 1:
					if(PassiveDown() || Environment.TickCount - E1.LastCastAttemptT > 2800)
						E2.Cast();
					break;
				case 2:
					if(ObjectManager.Get<Obj_AI_Hero>().Count(hero => hero.IsEnemy && hero.HasBuff("BlindMonkEOne") && hero.Distance(ObjectManager.Player) < E2.Range) >= 2)
						E2.Cast();
					if(ObjectManager.Get<Obj_AI_Hero>().FirstOrDefault(hero => hero.HasBuff("BlindMonkEOne") && !(hero.HasBuffOfType(BuffType.Slow) || hero.HasBuffOfType(BuffType.Stun) || hero.HasBuffOfType(BuffType.Snare))) != null)
						E2.Cast();
					if(PassiveDown() || Environment.TickCount - E1.LastCastAttemptT > 2800)
						E2.Cast();
					break;
			}
		}

		private bool PassiveDown()
		{
			return ObjectManager.Player.Buffs.All(buff => buff.Name != "blindmonkpassive_cosmetic");
		}

		private bool KillStealCombo()
		{
			if(!Program.Menu.Item("use_ComboKS").GetValue<bool>())
				return false;
			if(ComboQonEnemy())
				return true;
			if(ComboNoQonEnemy())
				return true;
			if(!Program.Menu.Item("useR_Kill").GetValue<bool>())
				return false;
			foreach(var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget() && hero.IsEnemy))
				AutoR(enemy);
			return false;
		}

		private void AutoR(Obj_AI_Base enemy)
		{
			if(!R.IsReady() || enemy.Distance(ObjectManager.Player) > R.Range)
				return;
			if(enemy.Health < DamageLib.getDmg(enemy, DamageLib.SpellType.R))
				R.CastOnUnit(enemy, Packets());
		}

		private bool ComboNoQonEnemy()
		{
			foreach(var target in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy && hero.IsValidTarget()))
			{
				if(!target.IsValidTarget(Q1.Range + 150))
					continue;
				var targetdis = ObjectManager.Player.Distance(target);
				var qDamage = DamageLib.getDmg(target, DamageLib.SpellType.Q, DamageLib.StageType.FirstDamage);
				var rDamage = DamageLib.getDmg(target, DamageLib.SpellType.R);
				var aDamage = DamageLib.getDmg(target, DamageLib.SpellType.AD);
				var eDamage = DamageLib.getDmg(target, DamageLib.SpellType.E);
				var igniteDamage = GetIgniteDamage(target);
				var health = target.Health + (target.HPRegenRate / 5 * 3) + 50;
				if(health < 0)
					health = float.MaxValue;

				if(E1Ready() && targetdis <= E1.Range)
				{
					if(eDamage + igniteDamage >= health)
					{
						E1.Cast();
						return true;
					}
					if(W1Ready() && targetdis <= W1.Range + E1.Range - 100 && eDamage + igniteDamage >= health)
					{
						var jumptarget = ComboJump(target, E1.Range - 100);
						if(jumptarget != null)
						{
							W1.CastOnUnit(jumptarget, Packets());
							return true;
						}
					}
				}

				if(Program.Menu.Item("use_MinionKill").GetValue<bool>() && R.IsReady())
				{
					if(targetdis < R.Range)
					{
						if(rDamage + igniteDamage >= health)
						{
							R.CastOnUnit(target, Packets());
							return true;
						}
					}
					if(W1Ready() && targetdis <= W1.Range + R.Range - 100 && (rDamage + igniteDamage >= health))
					{
						var jumptarget = ComboJump(target, R.Range - 100);
						if(jumptarget != null)
						{
							W1.CastOnUnit(jumptarget, Packets());
							return true;
						}
					}
				}

				if(E1Ready() && Program.Menu.Item("use_MinionKill").GetValue<bool>() && R.IsReady())
				{
					if(targetdis < E1.Range)
					{
						if(targetdis < E1.Range)
						{
							if(health < eDamage + rDamage + igniteDamage)
							{
								E1.Cast();
								R.CastOnUnit(target, Packets());
								return true;
							}

						}
						if(W1Ready() && targetdis <= W1.Range + E1.Range - 100 && (rDamage + eDamage + igniteDamage >= health))
						{
							var jumptarget = ComboJump(target, R.Range - 100);
							if(jumptarget != null)
							{
								W1.CastOnUnit(jumptarget, Packets());
								return true;
							}
						}
					}
				}

				if(Q1Ready() && Program.Menu.Item("useQ_1_Kill").GetValue<bool>() && targetdis < Q1.Range)
				{
					if(health <= qDamage + igniteDamage)
					{
						Q1.CastIfHitchanceEquals(target, HitChance.High, Packets());
						return true;
					}
					if(health <= qDamage + SecondQDamage(target, (float)qDamage) + igniteDamage &&
						80 <= ObjectManager.Player.MaxMana)
					{
						Q1.CastIfHitchanceEquals(target, HitChance.High, Packets());
						return true;
					}
					if(E1Ready())
					{
						if(health <= qDamage + eDamage + igniteDamage && 100 <= ObjectManager.Player.Mana && targetdis < E1.Range)
						{
							E1.Cast();
							Q1.CastIfHitchanceEquals(target, HitChance.High, Packets());
							return true;
						}
						if(health <= qDamage + eDamage + igniteDamage + SecondQDamage(target, (float)qDamage + (float)eDamage) && 130 <= ObjectManager.Player.Mana && targetdis < E1.Range)
						{
							E1.Cast();
							Q1.CastIfHitchanceEquals(target, HitChance.High, Packets());
							return true;
						}
						if(health <= qDamage + eDamage + igniteDamage + SecondQDamage(target, (float)qDamage) && 130 <= ObjectManager.Player.Mana && targetdis < E1.Range)
						{
							Q1.CastIfHitchanceEquals(target, HitChance.High, Packets());
							return true;
						}
					}
					if(Program.Menu.Item("useR_Kill").GetValue<bool>() && R.IsReady())
					{
						if(health <= qDamage + rDamage + igniteDamage && targetdis < R.Range)
						{
							Q1.CastIfHitchanceEquals(target, HitChance.High, Packets());
							return true;
						}
						if(health <= qDamage + rDamage + igniteDamage + SecondQDamage(target, (float)qDamage + (float)rDamage) && 80 <= ObjectManager.Player.Mana && targetdis < R.Range)
						{
							Q1.CastIfHitchanceEquals(target, HitChance.High, Packets());
							return true;
						}
						if(health <= qDamage + rDamage + SecondQDamage(target, (float)qDamage) + igniteDamage &&
							80 <= ObjectManager.Player.Mana && targetdis < R.Range)
						{
							Q1.CastIfHitchanceEquals(target, HitChance.High, Packets());
							return true;
						}

					}
					if(Program.Menu.Item("useR_Kill").GetValue<bool>() && R.IsReady() && E1Ready())
					{
						if(health <=
							qDamage + eDamage + rDamage + SecondQDamage(target, (float)qDamage + (float)eDamage) + igniteDamage)
						{
							Q1.CastIfHitchanceEquals(target, HitChance.High, Packets());
							return true;
						}
						if(health <=
						   qDamage + eDamage + rDamage + SecondQDamage(target, (float)qDamage + (float)rDamage) + igniteDamage)
						{
							Q1.CastIfHitchanceEquals(target, HitChance.High, Packets());
							return true;
						}
						if(!(health <=
							  qDamage + eDamage + rDamage + SecondQDamage(target, (float)qDamage + (float)rDamage + (float)eDamage) +
							  igniteDamage))
							continue;
						Q1.CastIfHitchanceEquals(target, HitChance.High, Packets());
						return true;
					}
				}
			}
			return false;
		}

		private bool ComboQonEnemy()
		{
			var qtarget = getQ2Target();
			if(!Q2Ready() || qtarget == null)
				return false;
			var qtargetdis = qtarget.Distance(ObjectManager.Player);
			var qTargetDistance = ObjectManager.Player.Distance(qtarget);
			var qDamage = DamageLib.getDmg(qtarget, DamageLib.SpellType.Q, DamageLib.StageType.FirstDamage);
			var rDamage = DamageLib.getDmg(qtarget, DamageLib.SpellType.R);
			var aDamage = DamageLib.getDmg(qtarget, DamageLib.SpellType.AD);
			var eDamage = DamageLib.getDmg(qtarget, DamageLib.SpellType.E);
			var igniteDamage = GetIgniteDamage(qtarget);

			var health = qtarget.Health + (qtarget.HPRegenRate / 5 * 3) + 50;
			if(health < 0)
				health = float.MaxValue;
			if(qtarget.IsMinion)
			{
				if(Program.Menu.Item("use_MinionKill").GetValue<bool>() && R.IsReady())
				{
					if(qTargetDistance <= Q2.Range && health <= qDamage)
					{
						Q2.Cast();
						return true;
					}
					if(E1Ready() && 80 <= ObjectManager.Player.Mana && health <= eDamage + qDamage && qTargetDistance <= E1.Range)
					{
						E1.Cast();
						Q2.Cast();
						return true;
					}
				}

			}
			if(qtarget.Distance(ObjectManager.Player) <= Q2.Range)
			{
				if(health <= qDamage || health <= SecondQDamage(qtarget) + igniteDamage)
				{
					Q2.Cast();
					return true;
				}
			}
			if(E1Ready() && 80 <= ObjectManager.Player.Mana)
			{
				if(health <= eDamage + SecondQDamage(qtarget, (float)eDamage) + igniteDamage && qtargetdis <= E1.Range)
				{
					E1.Cast();
					Q2.Cast();
					return true;
				}
				if(health <= eDamage + SecondQDamage(qtarget) + igniteDamage && qtargetdis > E1.Range)
				{
					Q2.Cast();
					return true;
				}
			}
			if(Program.Menu.Item("useR_Kill").GetValue<bool>() && R.IsReady())
			{
				if(health <= rDamage + SecondQDamage(qtarget, (float)rDamage) + igniteDamage && qtargetdis <= R.Range)
				{
					R.CastOnUnit(qtarget, Packets());
					Q2.Cast();
					return true;
				}
				if(health <= rDamage + SecondQDamage(qtarget) + igniteDamage && qtargetdis > R.Range)
				{
					Q2.Cast();
					return true;
				}
			}
			if(!Program.Menu.Item("useR_Kill").GetValue<bool>() || !R.IsReady() || !E1Ready())
				return false;
			if(health <= eDamage + rDamage + SecondQDamage(qtarget, (float)rDamage + (float)eDamage) + igniteDamage && qtargetdis <= R.Range)
			{
				E1.Cast();
				R.CastOnUnit(qtarget, Packets());
				Q2.Cast();
				return true;
			}
			if(health <= eDamage + SecondQDamage(qtarget, (float)rDamage) + rDamage + igniteDamage && qtargetdis <= R.Range)
			{
				R.CastOnUnit(qtarget, Packets());
				Q2.Cast();
				return true;
			}
			if(health <= eDamage + SecondQDamage(qtarget) + rDamage + igniteDamage && qtargetdis > R.Range)
			{
				Q2.Cast();
				return true;
			}
			return false;
		}

		private Obj_AI_Base ComboJump(Obj_AI_Base target, float range)
		{
			var qtarget = getQ2Target();
			if(qtarget != null && ((qtarget == target) || range > qtarget.Distance(target)) ||
				range > ObjectManager.Player.Distance(target))
				return null;

			Obj_AI_Base[] nearJumptarget = { null };
			foreach(var friendminion in ObjectManager.Get<Obj_AI_Minion>().Where(friendminion => friendminion.IsAlly && (friendminion.Distance(target) <= range || friendminion.Distance(target) <= W1.Range) && friendminion.Health >= 10).Where(friendminion => nearJumptarget[0] == null || nearJumptarget[0].Distance(target) > friendminion.Distance(target)))
				nearJumptarget[0] = friendminion;

			foreach(var friend in ObjectManager.Get<Obj_AI_Hero>().Where(friend => friend.IsAlly && !friend.IsMe && (friend.Distance(target) <= range || friend.Distance(target) <= W1.Range) && friend.IsValid).Where(friend => nearJumptarget[0] == null || nearJumptarget[0].Distance(target) > friend.Distance(target)))
				nearJumptarget[0] = friend;

			return nearJumptarget[0];
		}

		private bool Q1Ready()
		{
			return ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Name == "BlindMonkQOne" && Q1.IsReady();
		}

		private bool Q2Ready()
		{
			return ObjectManager.Player.Spellbook.GetSpell(SpellSlot.Q).Name == "blindmonkqtwo" && Q2.IsReady();
		}

		private bool W1Ready()
		{
			return ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "BlindMonkWOne" && W1.IsReady();
		}

		private bool W2Ready()
		{
			return ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "blindmonkwtwo" && W2.IsReady();
		}

		private bool E1Ready()
		{
			return ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Name == "BlindMonkEOne" && E1.IsReady();
		}

		private bool E2Ready()
		{
			return ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).Name == "blindmonketwo" && E2.IsReady();
		}

		private Obj_AI_Base getQ2Target()
		{
			return ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsEnemy).FirstOrDefault(enemy => enemy.Buffs.Any(buff => buff.Name == "BlindMonkQOne" || buff.Name == "blindmonkqonechaos"));
		}

		private bool FlashReady()
		{
			var spells = ObjectManager.Player.SummonerSpellbook.Spells;
			var flash = SpellSlot.Unknown;
			foreach(var spell in spells.Where(spell => spell.Name.ToLower() == "summonerflash"))
				flash = spell.Slot;
			if(flash == SpellSlot.Unknown)
				return false;
			return ObjectManager.Player.SummonerSpellbook.CanUseSpell(flash) == SpellState.Ready;
		}

		//private bool SmiteReady()
		//{
		//	var spells = ObjectManager.Player.SummonerSpellbook.Spells;
		//	var flash = SpellSlot.Unknown;
		//	foreach(var spell in spells.Where(spell => spell.Name.ToLower() == "summonersmite"))
		//		flash = spell.Slot;
		//	if(flash == SpellSlot.Unknown)
		//		return false;
		//	return ObjectManager.Player.SummonerSpellbook.CanUseSpell(flash) == SpellState.Ready;
		//}

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

		private double SecondQDamage(Obj_AI_Base target, float extradamage = 0)
		{
			var damage = Q1.Level * 30 + 20 + ObjectManager.Player.BaseAttackDamage * 0.9 + 0.08 * (target.MaxHealth - target.Health - extradamage);
			var realdamage = DamageLib.CalcPhysicalDmg(damage, target);
			return realdamage;
		}

		private void Drawing_OnDraw(EventArgs args)
		{

			if (Program.Menu.Item("Draw_Disabled").GetValue<bool>())
				return;

			var target = SimpleTs.GetTarget(Q1.Range, SimpleTs.DamageType.Physical);
			if (target != null && Getwardposition(target) != default(Vector3))
				Utility.DrawCircle(Getwardposition(target), 50, Color.Blue, 15);

			//if (Program.Menu.Item("RunlikeHell_key").GetValue<KeyBind>().Active)
			//{
			//	Utility.DrawCircle(Game.CursorPos, Program.Menu.Item("RunlikeHell_range").GetValue<Slider>().Value, Color.Yellow);
			//	//Utility.DrawCircle(runwardposition(), 50, Color.Yellow);
			//}

			switch (Program.Menu.Item("useModus_Mode").GetValue<StringList>().SelectedIndex)
			{
				case 0:
					Drawing.DrawLine(Drawing.Width*0.5f - 61, Drawing.Height*0.78f - 3, Drawing.Width*0.5f + 121,
						Drawing.Height*0.78f - 3, 27, Color.Red);
					Drawing.DrawLine(Drawing.Width*0.5f - 60, Drawing.Height*0.78f - 2, Drawing.Width*0.5f + 120,
						Drawing.Height*0.78f - 2, 25, Color.Black);
					Drawing.DrawText(Drawing.Width*0.5f - 35, Drawing.Height*0.78f, Color.GreenYellow, "Modus: Teamfight");

					break;
				case 1:
					Drawing.DrawLine(Drawing.Width*0.5f - 51, Drawing.Height*0.78f - 3, Drawing.Width*0.5f + 101,
						Drawing.Height*0.78f - 3, 27, Color.Red);
					Drawing.DrawLine(Drawing.Width*0.5f - 50, Drawing.Height*0.78f - 2, Drawing.Width*0.5f + 100,
						Drawing.Height*0.78f - 2, 25, Color.Black);
					Drawing.DrawText(Drawing.Width*0.5f - 25, Drawing.Height*0.78f, Color.GreenYellow, "Modus: Insec");
					break;
			}
			if (Program.Menu.Item("Draw_Q").GetValue<bool>())
				if (Q1.Level > 0)
				{
					if (Q2Ready())
						Utility.DrawCircle(ObjectManager.Player.Position, Q2.Range, Q2Ready() ? Color.Green : Color.Red);
					else
						Utility.DrawCircle(ObjectManager.Player.Position, Q1.Range, Q1Ready() ? Color.Green : Color.Red);
				}

			if(Program.Menu.Item("Draw_W").GetValue<bool>())
				if(W1.Level > 0)
						Utility.DrawCircle(ObjectManager.Player.Position, W1.Range, W1Ready() ? Color.Green : Color.Red);


			if(Program.Menu.Item("Draw_E").GetValue<bool>())
				if(E1.Level > 0)
				{
					if(E2Ready())
						Utility.DrawCircle(ObjectManager.Player.Position, E2.Range, E2Ready() ? Color.Green : Color.Red);
					else 
						Utility.DrawCircle(ObjectManager.Player.Position, E1.Range, E1Ready() ? Color.Green : Color.Red);
				}

			if(Program.Menu.Item("Draw_R").GetValue<bool>())
				if(R.Level > 0)
					Utility.DrawCircle(ObjectManager.Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);

		}
	}
}
