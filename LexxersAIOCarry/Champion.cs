using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace UltimateCarry
{
	class Champion
	{
		public static List<string> ManaManagerList = new List<string>();

        public Champion()
        {
            Chat.Print(ObjectManager.Player.ChampionName + " Plugin Loading ...");
            MenuBasics();
        }

        public void PluginLoaded()
        {
            Chat.Print(ObjectManager.Player.ChampionName + " Plugin Loaded!");
        }

		public bool Packets()
		{
			return Program.Menu.Item("usePackets").GetValue<bool>();
		}

		public void MenuBasics()
		{
			Program.Menu.AddSubMenu(new Menu("Packet Setting", "Packets"));
			Program.Menu.SubMenu("Packets").AddItem(new MenuItem("usePackets", "Enable Packets").SetValue(true));

			Program.Menu.Item("Orbwalk").DisplayName = "TeamFight";
			Program.Menu.Item("Farm").DisplayName = "Harass";
		}

		public void Game_OnGameSendPacket(GamePacketEventArgs args)
		{
			if(args.PacketData[0] != Packet.C2S.Move.Header)
				return;
			var decodedPacket = Packet.C2S.Move.Decoded(args.PacketData);
			if(decodedPacket.MoveType == 3 &&
				(Program.Orbwalker.GetTarget().IsMinion && !Program.Menu.Item("hitMinions").GetValue<bool>()))
				args.Process = false;
		}

		public void AddManaManager(string menuname, int basicmana)
		{
			Program.Menu.SubMenu(menuname).AddItem(new MenuItem("ManaManager_" + menuname, "Mana-Manager").SetValue(new Slider(basicmana)));
			ManaManagerList.Add("ManaManager_" + menuname);
		}

		public bool ManaManagerAllowCast(Spell spell)
		{
			bool ismixed;
			bool islasthit;
			bool islaneclear;
			if (ObjectManager.Player.ChampionName == "Azir")
			{
				ismixed = Program.Azirwalker.ActiveMode == Azir.Orbwalking.OrbwalkingMode.Mixed &&
						  ManaManagerList.Contains("ManaManager_Harass");
				islasthit = Program.Azirwalker.ActiveMode == Azir.Orbwalking.OrbwalkingMode.LastHit &&
							ManaManagerList.Contains("ManaManager_LastHit");
				islaneclear = Program.Azirwalker.ActiveMode == Azir.Orbwalking.OrbwalkingMode.LaneClear &&
							  ManaManagerList.Contains("ManaManager_LaneClear");
			}
			else
			{
				ismixed = Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed &&
				          ManaManagerList.Contains("ManaManager_Harass");
				islasthit = Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit &&
				            ManaManagerList.Contains("ManaManager_LastHit");
				islaneclear = Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear &&
				              ManaManagerList.Contains("ManaManager_LaneClear");
			}
			if(ismixed)
			{
				if((int)ObjectManager.Player.Spellbook.GetSpell(spell.Slot).ManaCost <= 1)
					return true;
				if(GetManaPercent() >= Program.Menu.Item("ManaManager_Harass").GetValue<Slider>().Value)
					return true;
				return false;
			}
			if(islasthit)
			{
				if((int)ObjectManager.Player.Spellbook.GetSpell(spell.Slot).ManaCost <= 1)
					return true;
                if (GetManaPercent() >=	Program.Menu.Item("ManaManager_LastHit").GetValue<Slider>().Value)
					return true;
				return false;
			}
			if(islaneclear)
			{
				if((int)ObjectManager.Player.Spellbook.GetSpell(spell.Slot).ManaCost <= 1)
					return true;
                if (GetManaPercent() >=	Program.Menu.Item("ManaManager_LaneClear").GetValue<Slider>().Value)
					return true;
				return false;
			}
			return true;
		}

		public Obj_AI_Hero Cast_BasicLineSkillshot_Enemy(Spell spell, SimpleTs.DamageType damageType = SimpleTs.DamageType.Physical)
		{
			if(!spell.IsReady() || !ManaManagerAllowCast(spell))
				return null;
			var target = SimpleTs.GetTarget(spell.Range, damageType);
			if(target == null)
				return null;
			if (!target.IsValidTarget(spell.Range) || spell.GetPrediction(target).Hitchance < HitChance.High)
				return null;
			spell.Cast(target, Packets());
			return target;
		}

		public void Cast_BasicLineSkillshot_Enemy(Spell spell, Vector3 sourcePosition, SimpleTs.DamageType damageType = SimpleTs.DamageType.Physical)
		{
			if(!spell.IsReady() || !ManaManagerAllowCast(spell))
				return;
			spell.UpdateSourcePosition(sourcePosition, sourcePosition);
            foreach (var hero in Program.Helper.EnemyTeam
				.Where(hero => (hero.Distance(sourcePosition) < spell.Range) && hero.IsValidTarget()).Where(hero => spell.GetPrediction(hero).Hitchance >= HitChance.High))
			{
				spell.Cast(hero, Packets());
				return;
			}
		}

        public void Cast_BasicCircleSkillshot_Enemy(Spell spell, SimpleTs.DamageType damageType = SimpleTs.DamageType.Physical, float extrarange = 0)
		{
			if(!spell.IsReady() || !ManaManagerAllowCast(spell))
				return;
            var target = SimpleTs.GetTarget(spell.Range + extrarange, damageType);
			if(target == null)
				return;
            if (target.IsValidTarget(spell.Range + extrarange) && spell.GetPrediction(target).Hitchance >= HitChance.High)
				spell.Cast(target, Packets());
		}

		public void Cast_BasicLineSkillshot_AOE_Farm(Spell spell)
		{
			if(!spell.IsReady() || !ManaManagerAllowCast(spell))
				return;
			var minions = MinionManager.GetMinions(ObjectManager.Player.Position, spell.Range, MinionTypes.All, MinionTeam.NotAlly);
			if(minions.Count == 0)
				return;
			var castPostion = MinionManager.GetBestLineFarmLocation(minions.Select(minion => minion.ServerPosition.To2D()).ToList(), spell.Width, spell.Range);
			spell.Cast(castPostion.Position, Packets());
		}

		public void Cast_BasicCircleSkillshot_AOE_Farm(Spell spell, int extrawidth = 0)
		{
			if(!spell.IsReady() || !ManaManagerAllowCast(spell))
				return;
			var minions = MinionManager.GetMinions(ObjectManager.Player.Position, spell.Range + ((spell.Width + extrawidth)/ 2 ), MinionTypes.All, MinionTeam.NotAlly);
			if(minions.Count == 0)
				return;
			var castPostion = MinionManager.GetBestCircularFarmLocation(minions.Select(minion => minion.ServerPosition.To2D()).ToList(), spell.Width + extrawidth, spell.Range);
			spell.Cast(castPostion.Position, Packets());
		}

		public void Cast_Basic_Farm(Spell spell , bool skillshot = false)
		{
			if(!spell.IsReady() || !ManaManagerAllowCast(spell))
				return;
			var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, spell.Range, MinionTypes.All, MinionTeam.NotAlly,MinionOrderTypes.MaxHealth );
			foreach(var minion in allMinions)
			{
				if(!minion.IsValidTarget())
					continue;
				var minionInRangeAa = Orbwalking.InAutoAttackRange(minion);
				var minionInRangeSpell = minion.Distance(ObjectManager.Player) <= spell.Range;
				var minionKillableAa = DamageLib.getDmg(minion, DamageLib.SpellType.AD) >= minion.Health;
				var minionKillableSpell = DamageLib.getDmg(minion, DamageLib.SpellType.Q) >= minion.Health;
				var lastHit = Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LastHit;
				var laneClear = Program.Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear;

				if((lastHit && minionInRangeSpell && minionKillableSpell) && ((minionInRangeAa && !minionKillableAa) || !minionInRangeAa))
					if(skillshot)
						spell.Cast(minion.Position, Packets());
					else
						spell.Cast(minion, Packets());
				else if((laneClear && minionInRangeSpell && !minionKillableSpell) && ((minionInRangeAa && !minionKillableAa) || !minionInRangeAa))
					if(skillshot)
						spell.Cast(minion.Position, Packets());
					else
						spell.Cast(minion, Packets());
			}
		}

		public void Cast_Speedboost_onFriend(Spell spell)
		{
			if(!spell.IsReady() || !ManaManagerAllowCast(spell))
				return;

			var friend = Program.Helper.OwnTeam.FirstOrDefault(x => x.Distance(ObjectManager.Player) <= spell.Range &&
                Program.Helper.EnemyTeam.Any(enemy => x.Distance(enemy) <= Orbwalking.GetRealAutoAttackRange(x) + 200 && x.BaseAttackDamage * x.AttackSpeedMod * 3 >= enemy.Health));

			if(friend == null)
				return;
			spell.CastOnUnit(friend, Packets());
		}

		public void Cast_Shield_onFriend(Spell spell, int percent,bool skillshot = false)
		{
			if(!spell.IsReady() || !ManaManagerAllowCast(spell))
				return;
			foreach(var friend in Program.Helper.OwnTeam.Where(hero => hero.Distance(ObjectManager.Player) <= spell.Range).Where(friend => friend.Health / friend.MaxHealth * 100 <= percent && Utility.CountEnemysInRange(1000) >= 1))
			{
				if (skillshot)
					spell.Cast(friend.Position, Packets());
				else
					spell.CastOnUnit(friend, Packets());
				return;
			}
		}

        public bool Cast_IfEnemys_inRange(Spell spell,int count = 1,float extrarange = 0)
        {
            if(!spell.IsReady() || !ManaManagerAllowCast(spell))
                return false;
            if(Utility.CountEnemysInRange((int)spell.Range + (int)extrarange) < count)
                return false;
            spell.Cast();
            return true;
        }

		public void Cast_onEnemy(Spell spell, SimpleTs.DamageType damageType = SimpleTs.DamageType.Physical)
		{
			if(!spell.IsReady() || !ManaManagerAllowCast(spell))
				return;
			var target = SimpleTs.GetTarget(spell.Range, damageType);
			if(target.IsValidTarget(spell.Range))
				spell.CastOnUnit(target, Packets());
		}

		public void Cast_onMinion_nearEnemy(Spell spell, float range, SimpleTs.DamageType damageType = SimpleTs.DamageType.Physical, MinionTypes minionTypes = MinionTypes.All, MinionTeam minionTeam = MinionTeam.All)
		{
			if(!spell.IsReady() || !ManaManagerAllowCast(spell))
				return;
			var target = SimpleTs.GetTarget(spell.Range + range, damageType);
			Obj_AI_Base[] nearstMinion = { null };
			var allminions = MinionManager.GetMinions(target.Position, range, minionTypes, minionTeam);
			foreach(var minion in allminions.Where(minion => minion.Distance(ObjectManager.Player) <= spell.Range && minion.Distance(target) <= range).Where(minion => nearstMinion[0] == null || nearstMinion[0].Distance(target) >= minion.Distance(target)))
				nearstMinion[0] = minion;

			if(nearstMinion[0] != null)
				spell.CastOnUnit(nearstMinion[0], Packets());
		}

		public bool EnoughManaFor(SpellSlot spell, SpellSlot spell2 = SpellSlot.Unknown, SpellSlot spell3 = SpellSlot.Unknown, SpellSlot spell4 = SpellSlot.Unknown)
		{
			var cost1 = ObjectManager.Player.Spellbook.GetSpell(spell).ManaCost;
			var cost2 = 0f;
			var cost3 = 0f;
			var cost4 = 0f;
			if(spell2 != SpellSlot.Unknown)
				cost2 = ObjectManager.Player.Spellbook.GetSpell(spell2).ManaCost;
			if(spell3 != SpellSlot.Unknown)
				cost3 = ObjectManager.Player.Spellbook.GetSpell(spell3).ManaCost;
			if(spell4 != SpellSlot.Unknown)
				cost4 = ObjectManager.Player.Spellbook.GetSpell(spell4).ManaCost;

			return cost1 + cost2 + cost3 + cost4 <= ObjectManager.Player.Mana;
		}

		public Vector3 GetReversePosition(Vector3 positionMe, Vector3 positionEnemy)
		{
			var x = positionMe.X - positionEnemy.X;
			var y = positionMe.Y - positionEnemy.Y;
			return new Vector3(positionMe.X + x, positionMe.Y + y, positionMe.Z);
		}

        public float GetManaPercent()
        {
            return (ObjectManager.Player.Mana / ObjectManager.Player.MaxMana) * 100f;
        }
	}
}

