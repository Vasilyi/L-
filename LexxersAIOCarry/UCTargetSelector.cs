using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace UltimateCarry
{
	internal class UcTargetSelector
	{
		public enum Mode
		{
			LowHP,
			MostAD,
			MostAP,
			Closest,
			NearMouse,
			LessAttack,
			LessCast,
			AutoPriority,
		}

		private static readonly string[] Modelist = { Mode.AutoPriority.ToString(), Mode.LowHP.ToString(), Mode.MostAD.ToString(), Mode.MostAP.ToString(), Mode.Closest.ToString(), Mode.NearMouse.ToString(), Mode.LessAttack.ToString(), Mode.LessCast.ToString() };
 
		private static double _lasttick;

		private static readonly string[] AP = { "Ahri", "Akali", "Anivia", "Annie", "Brand", "Cassiopeia", "Diana", "FiddleSticks", "Fizz", "Gragas", "Heimerdinger", "Karthus", "Kassadin", "Katarina", "Kayle", "Kennen", "Leblanc", "Lissandra", "Lux", "Malzahar", "Mordekaiser", "Morgana", "Nidalee", "Orianna", "Ryze", "Sion", "Swain", "Syndra", "Teemo", "TwistedFate", "Veigar", "Viktor", "Vladimir", "Xerath", "Ziggs", "Zyra", "Velkoz" };
		private static readonly string[] AD = { "Ashe", "Caitlyn", "Corki", "Draven", "Ezreal", "Graves", "KogMaw", "MissFortune", "Quinn", "Sivir", "Talon", "Tristana", "Twitch", "Urgot", "Varus", "Vayne", "Zed", "Jinx", "Yasuo", "Lucian" };	
		private static readonly string[] Sup = { "Blitzcrank", "Janna", "Karma", "Leona", "Lulu", "Nami", "Sona", "Soraka", "Thresh", "Zilean" };
		private static readonly string[] Bruiser = { "Darius", "Elise", "Evelynn", "Fiora", "Gangplank", "Gnar", "Jayce", "Pantheon", "Irelia", "JarvanIV", "Jax", "Khazix", "LeeSin", "Nocturne", "Olaf", "Poppy", "Renekton", "Rengar", "Riven", "Shyvana", "Trundle", "Tryndamere", "Udyr", "Vi", "MonkeyKing", "XinZhao", "Aatrox", "Rumble", "Shaco", "MasterYi" };

		public Obj_AI_Hero Target;
		private Obj_AI_Hero _maintarget;
		private static Mode _mode;
		private float _range;
		private bool _update = true;

		public UcTargetSelector(float range, Mode mode)
		{
			_range = range;
			_mode = mode;
			Game.OnGameUpdate += Game_OnGameUpdate;
			Game.OnWndProc += Game_OnWndProc;
		}
		private void Game_OnWndProc(WndEventArgs args)
		{
			if(MenuGUI.IsChatOpen || ObjectManager.Player.Spellbook.SelectedSpellSlot != SpellSlot.Unknown || args.WParam != 1)
				return;

			switch(args.Msg)
			{
				case 257:
					foreach(var hero in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.IsValidTarget() &&
										SharpDX.Vector2.Distance(Game.CursorPos.To2D(), hero.ServerPosition.To2D()) < 300))
					{
						Target = hero;
						_maintarget = hero;
						Game.PrintChat("UC-TargetSelector: New main target: " + _maintarget.ChampionName);
					}
					break;
			}
		}

		private void Game_OnGameUpdate(EventArgs args)
		{
			if (!(Environment.TickCount > _lasttick + 100)) 
				return;
			_lasttick = Environment.TickCount;

			if(!_update)
				return;
				
			if(_maintarget == null)
				GetNormalTarget();			
			else
				if(Geometry.Distance(_maintarget) > _range)
					GetNormalTarget();					
				else
					if(_maintarget.IsValidTarget())
						Target = _maintarget;	
					else
						GetNormalTarget();		
		}

		private void GetNormalTarget()
		{
			Obj_AI_Hero newtarget = null;
			if(_mode != Mode.AutoPriority)
			{
				foreach(var target in ObjectManager.Get<Obj_AI_Hero>())
					if(target.IsValidTarget() && Geometry.Distance(target) <= _range)
					{
						if(newtarget == null)
							newtarget = target;
						else
							switch(_mode)
							{
								case Mode.LowHP:
									if(target.Health < newtarget.Health)
										newtarget = target;
									break;
								case Mode.MostAD:
									if(target.BaseAttackDamage + target.FlatPhysicalDamageMod <
										newtarget.BaseAttackDamage + newtarget.FlatPhysicalDamageMod)
										newtarget = target;
									break;
								case Mode.MostAP:
									if(target.FlatMagicDamageMod < newtarget.FlatMagicDamageMod)
										newtarget = target;
									break;
								case Mode.Closest:
									if(Geometry.Distance(target) < Geometry.Distance(newtarget))
										newtarget = target;
									break;
								case Mode.NearMouse:
									if(SharpDX.Vector2.Distance(Game.CursorPos.To2D(), target.Position.To2D()) <
										SharpDX.Vector2.Distance(Game.CursorPos.To2D(), newtarget.Position.To2D()))
										newtarget = target;
									break;
								case Mode.LessAttack:
									if((target.Health - DamageLib.CalcPhysicalDmg(target.Health, target)) <
										(newtarget.Health - DamageLib.CalcPhysicalDmg(newtarget.Health, newtarget)))
										newtarget = target;
									break;
								case Mode.LessCast:
									if((target.Health - DamageLib.CalcMagicDmg(target.Health, target)) <
										(target.Health - DamageLib.CalcMagicDmg(newtarget.Health, newtarget)))
										newtarget = target;
									break;
							}
					}
			}
			else
				newtarget = AutoPriority();

			Target = newtarget;
		}

		private Obj_AI_Hero AutoPriority()
		{
			Obj_AI_Hero autopriority = null;
			var prio = 5;
			foreach(var target in ObjectManager.Get<Obj_AI_Hero>().Where(target => target != null && target.IsValidTarget() && Geometry.Distance(target) <= _range))
				if(autopriority == null)
				{
					autopriority = target;
					prio = FindPrioForTarget(target.ChampionName);
					Program.Menu.Item("UC_TS" + target.ChampionName + "Priority")
						.SetValue(new Slider(prio, 5, 1));
				}
				else
					if(FindPrioForTarget(target.ChampionName) < prio)
					{
						autopriority = target;
						prio = FindPrioForTarget(target.ChampionName);
						Program.Menu.Item("UC_TS" + target.ChampionName + "Priority")
						.SetValue(new Slider(prio, 5, 1));
					}
					else if(FindPrioForTarget(target.ChampionName) == prio)
					{
						if(!(target.Health < autopriority.Health))
							continue;
						autopriority = target;
						prio = FindPrioForTarget(target.ChampionName);
						Program.Menu.Item("UC_TS" + target.ChampionName + "Priority")
						.SetValue(new Slider(prio, 5, 1));
					}
			return autopriority;
		}

		private static int FindPrioForTarget(string championName)
		{
			if(AD.Contains(championName))
				return 1;
			
			if(AP.Contains(championName))
				return 2;
			
			if(Sup.Contains(championName))
				return 3;
			
			return Bruiser.Contains(championName) ? 4 : 5;
		}
		
		public void OverrideTarget(Obj_AI_Hero newtarget)
		{
			Target = newtarget;
			_update = false;
		}
		
		public void DisableTargetOverride()
		{
			_update = true;
		}
		
		public float GetRange()
		{
			return _range;
		}
		
		public void SetRange(float range)
		{
			_range = range;
		}
		
		public Mode GetTargetingMode()
		{
			return _mode;
		}
		
		public static void SetTargetingMode(Mode mode)
		{
			_mode = mode;
		}
		
		public override string ToString()
		{
			return "UC-Target: " + Target.ChampionName + "Range: " + _range + "Mode: " + _mode;
		}

		public static void AddtoMenu(Menu config)
		{
			//config.AddItem(new MenuItem("AutoPriority", "Auto priorities").SetValue(true));
			//config.AddItem(new MenuItem("Sep", ""));
			config.AddItem(new MenuItem("empty1", "if Auto priorities is ON"));
			config.AddItem(new MenuItem("empty2", "Changes have no effect"));
			config.AddItem(new MenuItem("hint", "Low Number = High Prio"));
			config.AddItem(new MenuItem("Sep1", ""));
			foreach(var enemy in ObjectManager.Get<Obj_AI_Hero>().Where(hero => hero.Team != ObjectManager.Player.Team)
)
			{
				config.AddItem(
				new MenuItem("UC_TS" + enemy.ChampionName + "Priority", enemy.ChampionName)
				.SetValue(
				new Slider(FindPrioForTarget(enemy.ChampionName), 5, 1)));
			}
			config.AddItem(new MenuItem("Sep2", ""));
			foreach (var mod in Modelist)
			{
				config.AddItem(mod == "AutoPriority"
					? new MenuItem("UC_TSPriority_" + mod, mod).SetValue(true).DontSave()
					: new MenuItem("UC_TSPriority_" + mod, mod).SetValue(false).DontSave());
				Program.Menu.Item("UC_TSPriority_" + mod).ValueChanged += Modswitch;
			}
		}

		private static void Modswitch(object sender, OnValueChangeEventArgs e)
		{
			if(!e.GetNewValue<bool>())
				return;
			var item = (MenuItem)sender;
			foreach (var mod in Modelist)
			{
				if (mod != item.DisplayName)
					Program.Menu.Item("UC_TSPriority_" + mod).SetValue(false);
			}
			if(item.DisplayName == Mode.AutoPriority.ToString())
				SetTargetingMode(Mode.AutoPriority);
			if(item.DisplayName == Mode.Closest.ToString())
				SetTargetingMode(Mode.Closest);
			if(item.DisplayName == Mode.LessAttack.ToString())
				SetTargetingMode(Mode.LessAttack);
			if(item.DisplayName == Mode.LessCast.ToString())
				SetTargetingMode(Mode.LessCast);
			if(item.DisplayName == Mode.LowHP.ToString())
				SetTargetingMode(Mode.LowHP);
			if(item.DisplayName == Mode.MostAD.ToString())
				SetTargetingMode(Mode.MostAD);
			if(item.DisplayName == Mode.MostAP.ToString())
				SetTargetingMode(Mode.MostAP);
			if(item.DisplayName == Mode.NearMouse.ToString())
				SetTargetingMode(Mode.NearMouse);

			Chat.Print("UC-Targetselector: Switch Targetingmode to " + item.DisplayName);
		}
	}
}
