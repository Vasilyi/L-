using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace UltimateCarry
{
	class Riven : Champion
	{
		public static Spell Q;
		public static Spell QPred;

		public static Spell W;
		public static Spell E;
		public static Spell R;
		public Spell Rstart;
		public Obj_AI_Hero Player = ObjectManager.Player;

		public static int StackPassive = 0;
		public int QStage = 0;
		public int QDelay = 300;
		public int QTick = 0;
		public int RDelay = 16000;
		public int RTick = 0;

		public Riven()
		{
			LoadMenu();
			LoadSpells();

			Drawing.OnDraw += Drawing_OnDraw;
			Game.OnGameUpdate += Game_OnGameUpdate;
			Obj_AI_Base.OnPlayAnimation += OnAnimation;
			Game.OnGameProcessPacket += OnGameProcessPacket;
			PluginLoaded();
		}

		private void LoadMenu()
		{
			Program.Menu.AddSubMenu(new Menu("TeamFight", "TeamFight"));

			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("use_UC", "Use UC Combo").SetValue(true));
			Program.Menu.Item("use_UC").ValueChanged += SwitchUc;
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("use_Detuks", "Use Detuks Combo").SetValue(false));
			Program.Menu.Item("use_Detuks").ValueChanged += SwitchDetuks;
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useQ_TeamFight", "Use Q").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useW_TeamFight", "Use W").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useE_TeamFight", "Use E").SetValue(true));
			Program.Menu.SubMenu("TeamFight").AddItem(new MenuItem("useR_TeamFight", "Use R").SetValue(true));

			Program.Menu.AddSubMenu(new Menu("Harass", "Harass"));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useQ_Harass", "Use Q").SetValue(true));
			Program.Menu.SubMenu("Harass").AddItem(new MenuItem("useW_Harass", "Use W").SetValue(true));

			Program.Menu.AddSubMenu(new Menu("LaneClear", "LaneClear"));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useQ_LaneClear", "Use Q").SetValue(true));
			Program.Menu.SubMenu("LaneClear").AddItem(new MenuItem("useW_LaneClear", "Use W").SetValue(true));

			Program.Menu.AddSubMenu(new Menu("Passive", "Passive"));
			Program.Menu.SubMenu("Passive").AddItem(new MenuItem("CancleQAnimation", "Cancle Q Move").SetValue(true));
			Program.Menu.Item("CancleQAnimation").ValueChanged += SwitchMove;
			Program.Menu.SubMenu("Passive").AddItem(new MenuItem("QLaugh", "Cancle Q Laugh").SetValue(false));
			Program.Menu.Item("QLaugh").ValueChanged += SwitchLaugh;
			Program.Menu.AddSubMenu(new Menu("Drawing", "Drawing"));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Disabled", "Disable All").SetValue(false));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_Q", "Draw Q").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_W", "Draw W").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_E", "Draw E").SetValue(true));
			Program.Menu.SubMenu("Drawing").AddItem(new MenuItem("Draw_R", "Draw R").SetValue(true));
		}

		private static void SwitchUc(object sender, OnValueChangeEventArgs e)
		{
			if(e.GetNewValue<bool>())
				Program.Menu.Item("use_Detuks").SetValue(false);
		}

		private static void SwitchDetuks(object sender, OnValueChangeEventArgs e)
		{
			if(e.GetNewValue<bool>())
				Program.Menu.Item("use_UC").SetValue(false);
		}

		private static void SwitchLaugh(object sender, OnValueChangeEventArgs e)
		{
			if(e.GetNewValue<bool>())
				Program.Menu.Item("CancleQAnimation").SetValue(false);
		}

		private static void SwitchMove(object sender, OnValueChangeEventArgs e)
		{
			if(e.GetNewValue<bool>())
				Program.Menu.Item("QLaugh").SetValue(false);
		}

		private void LoadSpells()
		{

			Q = new Spell(SpellSlot.Q, 280);

			QPred = new Spell(SpellSlot.Q, 280);
			QPred.SetSkillshot(0, 112.5f, float.MaxValue, false, SkillshotType.SkillshotCircle);

			W = new Spell(SpellSlot.W, 260);

			E = new Spell(SpellSlot.E, 390);
			E.SetSkillshot(0, Orbwalking.GetRealAutoAttackRange(Player), float.MaxValue, false, SkillshotType.SkillshotCircle);

			Rstart = new Spell(SpellSlot.R, 900);

			R = new Spell(SpellSlot.R, 900);
			R.SetSkillshot(0.25f, 300f, 1200, false, SkillshotType.SkillshotCone);
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

		private void Game_OnGameUpdate(EventArgs args)
		{
			var firstOrDefault = Player.Buffs.FirstOrDefault(buff => buff.Name == "rivenpassiveaaboost");
			StackPassive = firstOrDefault != null ? firstOrDefault.Count : 0;
			if(Environment.TickCount - QTick >= QDelay)
			{
				firstOrDefault =
					Player.Buffs.FirstOrDefault(
						buff =>
							buff.Name == "riventricleavesoundone" || buff.Name == "riventricleavesoundtwo" ||
							buff.Name == "riventricleavesoundthree");
				if(firstOrDefault == null)
					QStage = 0;
			}

			QPred.Width = GetQRadius();


			switch(Program.Orbwalker.ActiveMode)
			{

				case Orbwalking.OrbwalkingMode.Combo:
					if(Program.Menu.Item("use_Detuks").GetValue<bool>())
					{
						UseDetuksCombo();
						return;
					}
					if(Program.Menu.Item("useQ_TeamFight").GetValue<bool>() &&
								(StackPassive <= 0 || QStage == 2))
					{

						Cast_BasicCircleSkillshot_Enemy(QPred);
					}
					if(Program.Menu.Item("useR_TeamFight").GetValue<bool>())
						CastR();
					if(Program.Menu.Item("useW_TeamFight").GetValue<bool>() && StackPassive == 0)
						Cast_IfEnemys_inRange(W);
					if(Program.Menu.Item("useE_TeamFight").GetValue<bool>() && StackPassive == 0)
						CastE();
					break;
				case Orbwalking.OrbwalkingMode.Mixed:
					if(Program.Menu.Item("useQ_Harass").GetValue<bool>() &&
					   StackPassive == 0)
						Cast_BasicCircleSkillshot_Enemy(QPred);
					if(Program.Menu.Item("useW_Harass").GetValue<bool>() && StackPassive == 0)
						Cast_IfEnemys_inRange(W);
					break;
				case Orbwalking.OrbwalkingMode.LaneClear:
					if(Program.Menu.Item("useQ_LaneClear").GetValue<bool>() &&
					  StackPassive == 0)
						Cast_BasicCircleSkillshot_AOE_Farm(Q);
					if(Program.Menu.Item("useW_LaneClear").GetValue<bool>() && StackPassive == 0)
						Cast_W_Farm();
					break;
			}
		}

		private void UseDetuksCombo()
		{
			var target = SimpleTs.GetTarget(500, SimpleTs.DamageType.Physical);
			UseESmart(target);
			UseWSmart(target, true);
			UseRSmart(target);
		}
		private static void UseWSmart(Obj_AI_Base target, bool aaRange = false)
		{
			if(!Program.Menu.Item("useW_TeamFight").GetValue<bool>())
				return;
			float range;
			if(aaRange)
				range = ObjectManager.Player.AttackRange + target.BoundingRadius;
			else
				range = W.Range + target.BoundingRadius - 20;
			if(W.IsReady() && target.Distance(ObjectManager.Player.ServerPosition) < range)
				W.Cast();
		}

		private static void UseESmart(Obj_AI_Base target)
		{
			if(!Program.Menu.Item("useE_TeamFight").GetValue<bool>())
				return;
			var trueAaRange = ObjectManager.Player.AttackRange + target.BoundingRadius;
			var trueERange = target.BoundingRadius + E.Range;

			var dist = ObjectManager.Player.Distance(target);
			if(dist > trueAaRange && dist < trueERange)
				E.Cast(target.ServerPosition);
		}
		private static void UseRSmart(Obj_AI_Base target)
		{
			if(!Program.Menu.Item("useR_TeamFight").GetValue<bool>())
				return;
			if(!ObjectManager.Player.HasBuff("RivenFengShuiEngine") && !E.IsReady())
				R.Cast();
			else if(target is Obj_AI_Hero)
			{
				var targ = target as Obj_AI_Hero;
				var po = R.GetPrediction(targ, true);
				if(po.Hitchance != HitChance.High)
					return;
				if(DamageLib.getDmg(target, DamageLib.SpellType.R) > ((targ.Health + target.ScriptHealthBonus) - 5 * targ.Level) && R.IsReady())
					R.Cast(po.CastPosition);
			}

		}

		private void Cast_W_Farm()
		{
			if(!W.IsReady())
				return;
			var allminions = MinionManager.GetMinions(Player.Position, W.Range, MinionTypes.All, MinionTeam.NotAlly);
			if(allminions.Count >= 1)
				W.Cast();
		}

		public static void OnGameProcessPacket(GamePacketEventArgs args)
		{
			if(Orbwalking.OrbwalkingMode.Combo != Program.Orbwalker.ActiveMode ||
				!Program.Menu.Item("use_Detuks").GetValue<bool>())
				return;
			if(!Program.Menu.Item("useQ_TeamFight").GetValue<bool>())
				return;
			if(args.PacketData[0] != 101 || !Q.IsReady())
				return;
			var gp = new GamePacket(args.PacketData)
			{
				Position = 5
			};
			var dType = (int)gp.ReadByte();
			var targetId = gp.ReadInteger();
			var source = gp.ReadInteger();
			if(ObjectManager.Player.NetworkId != source)
				return;
			var targ = ObjectManager.GetUnitByNetworkId<Obj_AI_Hero>(targetId);
			if(dType == 12 || dType == 3)
				Q.Cast(targ.Position);
		}

		public static void OnGameSendPacket(GamePacketEventArgs args)
		{
			if(args.PacketData[0] != 154 || Orbwalking.OrbwalkingMode.Combo != Program.Orbwalker.ActiveMode)
				return;
			Packet.C2S.Cast.Struct cast = Packet.C2S.Cast.Decoded(args.PacketData);
			if((int)cast.Slot > -1 && (int)cast.Slot < 5)
				Utility.DelayAction.Add(Game.Ping, CancelAnim);

			if(cast.Slot == SpellSlot.E && R.IsReady())
				Utility.DelayAction.Add(Game.Ping + 100, () => UseRSmart(Program.Orbwalker.GetTarget()));
		}

		public static void CancelAnim()
		{
			Orbwalking.ResetAutoAttackTimer();

			if(W.IsReady())
				UseWSmart(Program.Orbwalker.GetTarget());
			else
			{
				if(Program.Menu.Item("QLaugh").GetValue<bool>())
					Game.Say("/l");
				else if(Program.Menu.Item("CancleQAnimation").GetValue<bool>())
					Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(Game.CursorPos.X, Game.CursorPos.Y)).Send();
			}
		}

		private void OnAnimation(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
		{
			if(Orbwalking.OrbwalkingMode.Combo != Program.Orbwalker.ActiveMode ||
				!Program.Menu.Item("use_UC").GetValue<bool>())
				return;
			if(!sender.IsMe)
				return;
			if(args.Animation == "Spell1a")
			{
				QStage = 1;
				if(Program.Menu.Item("QLaugh").GetValue<bool>())
					Game.Say("/l");
				else if(Program.Menu.Item("CancleQAnimation").GetValue<bool>())
					Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(Game.CursorPos.X, Game.CursorPos.Y)).Send();
			}

			if(args.Animation == "Spell1b")
			{
				QStage = 2;
				if(Program.Menu.Item("QLaugh").GetValue<bool>())
					Game.Say("/l");
				else if(Program.Menu.Item("CancleQAnimation").GetValue<bool>())
					Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(Game.CursorPos.X, Game.CursorPos.Y)).Send();
			}

			if(args.Animation != "Spell1c")
				return;
			QStage = 0;
			if(Program.Menu.Item("QLaugh").GetValue<bool>())
				Game.Say("/l");
			else if(Program.Menu.Item("CancleQAnimation").GetValue<bool>())
				Packet.C2S.Move.Encoded(new Packet.C2S.Move.Struct(Game.CursorPos.X, Game.CursorPos.Y)).Send();
		}

		private float GetQRadius()
		{
			var firstOrDefault = Player.Buffs.FirstOrDefault(buff => buff.Name == "RivenFengShuiEngine");
			if(firstOrDefault == null)
			{
				if(QStage == 0 || QStage == 1)
					return 162.5f;
				return 200;
			}
			if(QStage == 0 || QStage == 1)
				return 112.5f;
			return 150;
		}

		private void CastR()
		{
			if(!R.IsReady())
				return;
			var firstOrDefault = Player.Buffs.FirstOrDefault(buff => buff.Name == "RivenFengShuiEngine");
			if(firstOrDefault == null)
			{
				if(Cast_IfEnemys_inRange(R, 1, -900 + Orbwalking.GetRealAutoAttackRange(Player) + 75))
					RTick = Environment.TickCount;
			}
			else
			{
				if(Environment.TickCount - RTick > RDelay || Environment.TickCount - RTick >= RDelay)
					return;
				var target = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Physical);
				if(target == null)
					return;
				if(!target.IsValidTarget(R.Range) || R.GetPrediction(target).Hitchance < HitChance.High)
					return;
				if(DamageLib.getDmg(target, DamageLib.SpellType.R) >= target.Health || Environment.TickCount - RTick >= 13000 || !target.IsValidTarget(R.Range - 400))
					R.Cast(target, Packets());
			}
		}

		private void CastE()
		{
			if(!E.IsReady())
				return;
			var target = SimpleTs.GetTarget(E.Range + Orbwalking.GetRealAutoAttackRange(Player), SimpleTs.DamageType.Physical);
			if(!target.IsValidTarget(E.Range + Orbwalking.GetRealAutoAttackRange(Player)) && (target.Distance(ObjectManager.Player) >= Orbwalking.GetRealAutoAttackRange(Player)))
				return;
			E.Cast(target.Position, Packets());
		}
	}
}
