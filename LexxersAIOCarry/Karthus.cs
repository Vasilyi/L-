using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace UltimateCarry
{
	/*
	 * LaneClear:
	 * - allow AA on Tower, Ward (don't Q wards)
	 * - improve in early game (possible?)
	 * 
	 * AutoIgnite:
	 * - add GetComboDamage and ignite earlier if combodamage would suffice
	 * 
	 * Ult KS:
	 * - don't KS anymore if enemy is recalling and would arrive base before ult went through (have to include BaseUlt functionality)
	* */

	class Karthus : Champion
	{
		readonly Menu _menu;

		private readonly Spell _spellQ;
		private readonly Spell _spellW;
		private readonly Spell _spellE;
		private readonly Spell _spellR;

		private const float SpellQWidth = 160f;
		private const float SpellWWidth = 160f;

		private bool _comboE;

		public Karthus()
		{
			_menu = Program.Menu;

			var comboMenu = _menu.AddSubMenu(new Menu("TeamFight", "TeamFight"));
			comboMenu.AddItem(new MenuItem("comboQ", "Use Q").SetValue(true));
			comboMenu.AddItem(new MenuItem("comboW", "Use W").SetValue(true));
			comboMenu.AddItem(new MenuItem("comboE", "Use E").SetValue(true));
			comboMenu.AddItem(new MenuItem("comboAA", "Use AA").SetValue(false));
			comboMenu.AddItem(new MenuItem("comboWPercent", "Use W until Mana %").SetValue(new Slider(10)));
			comboMenu.AddItem(new MenuItem("comboEPercent", "Use E until Mana %").SetValue(new Slider(15)));

			var harassMenu = _menu.AddSubMenu(new Menu("Harass", "Harass"));
		    harassMenu.AddItem(new MenuItem("harassQ", "Use Q").SetValue(true));
			harassMenu.AddItem(new MenuItem("harassQPercent", "Use Q until Mana %").SetValue(new Slider(15)));

			var farmMenu = _menu.AddSubMenu(new Menu("Farming", "Farming"));
			farmMenu.AddItem(new MenuItem("farmQ", "Use Q").SetValue(new StringList(new[] { "Last Hit", "Lane Clear", "Both", "No" }, 1)));
			farmMenu.AddItem(new MenuItem("farmE", "Use E in Lane Clear").SetValue(true));
			farmMenu.AddItem(new MenuItem("farmAA", "Use AA in Lane Clear").SetValue(false));
			farmMenu.AddItem(new MenuItem("farmQPercent", "Use Q until Mana %").SetValue(new Slider(10)));
			farmMenu.AddItem(new MenuItem("farmEPercent", "Use E until Mana %").SetValue(new Slider(20)));

			var notifyMenu = _menu.AddSubMenu(new Menu("Notify on R killable enemies", "Notify"));
			notifyMenu.AddItem(new MenuItem("notifyR", "Text Notify").SetValue(true));
			notifyMenu.AddItem(new MenuItem("notifyPing", "Ping Notify").SetValue(false));

			var drawMenu = _menu.AddSubMenu(new Menu("Drawing", "Drawing"));
			drawMenu.AddItem(new MenuItem("drawQ", "Draw Q range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(125, 0, 255, 0))));

			var miscMenu = _menu.AddSubMenu(new Menu("Misc", "Misc"));
			miscMenu.AddItem(new MenuItem("ultKS", "Ultimate KS").SetValue(true));
            miscMenu.AddItem(new MenuItem("autoCast", "Auto Combo/LaneClear if dead").SetValue(false));

			_spellQ = new Spell(SpellSlot.Q, 875);
			_spellW = new Spell(SpellSlot.W, 1000);
			_spellE = new Spell(SpellSlot.E, 505);
			_spellR = new Spell(SpellSlot.R, 20000f);

			_spellQ.SetSkillshot(1f, 160, float.MaxValue, false, SkillshotType.SkillshotCircle);
			_spellW.SetSkillshot(.5f, 70, float.MaxValue, false, SkillshotType.SkillshotCircle);
			_spellE.SetSkillshot(1f, 505, float.MaxValue, false, SkillshotType.SkillshotCircle);
			_spellR.SetSkillshot(3f, float.MaxValue, float.MaxValue, false, SkillshotType.SkillshotCircle);

			Drawing.OnDraw += Drawing_OnDraw;
			Game.OnGameUpdate += Game_OnGameUpdate;
			PluginLoaded();
		}

		void Game_OnGameUpdate(EventArgs args)
		{
			if(_menu.Item("ultKS").GetValue<bool>())
				UltKs();
			switch(Program.Orbwalker.ActiveMode)
			{
				case Orbwalking.OrbwalkingMode.Combo:
					Program.Orbwalker.SetAttacks(_menu.Item("comboAA").GetValue<bool>() || ObjectManager.Player.Mana < 100); //if no mana, allow auto attacks!
					Combo();
					break;
				case Orbwalking.OrbwalkingMode.Mixed:
					Program.Orbwalker.SetAttacks(true);
					Harass();
					break;
				case Orbwalking.OrbwalkingMode.LaneClear:
					Program.Orbwalker.SetAttacks(_menu.Item("farmAA").GetValue<bool>() || ObjectManager.Player.Mana < 100);
					LaneClear();
					break;
				case Orbwalking.OrbwalkingMode.LastHit:
					Program.Orbwalker.SetAttacks(true);
					LastHit();
					break;
				default:
                    Program.Orbwalker.SetAttacks(true);
					RegulateEState();

                    if (_menu.Item("autoCast").GetValue<bool>())
                        if(IsInPassiveForm())
                            if(!Combo())
                                LaneClear(true);

					break;
			}

		}

		bool Combo()
		{
            bool anyQTarget = false;

			if(_menu.Item("comboW").GetValue<bool>())
				CastW(SimpleTs.GetTarget(_spellW.Range, SimpleTs.DamageType.Magical), _menu.Item("comboWPercent").GetValue<Slider>().Value);

			if(_menu.Item("comboE").GetValue<bool>() && _spellE.IsReady() && !IsInPassiveForm())
			{
				var target = SimpleTs.GetTarget(_spellE.Range, SimpleTs.DamageType.Magical);

				if(target != null)
				{
					var enoughMana = GetManaPercent() >= _menu.Item("comboEPercent").GetValue<Slider>().Value;

					if(ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).ToggleState == 1)
					{
						if(ObjectManager.Player.Distance(target.ServerPosition) <= _spellE.Range && enoughMana)
						{
							_comboE = true;
							_spellE.Cast(ObjectManager.Player.Position, Packets());
						}
					}
					else if(!enoughMana)
						RegulateEState(true);
				}
				else
					RegulateEState();
			}

            if (_menu.Item("comboQ").GetValue<bool>() && _spellQ.IsReady())
            {
                var target = SimpleTs.GetTarget(_spellQ.Range, SimpleTs.DamageType.Magical);

                if(target != null)
                {
                    anyQTarget = true;
                    CastQ(target);
                }
            }

            return anyQTarget;
		}

        void Harass()
        {
            if (_menu.Item("harassQ").GetValue<bool>())
                CastQ(SimpleTs.GetTarget(_spellQ.Range, SimpleTs.DamageType.Magical), _menu.Item("harassQPercent").GetValue<Slider>().Value);
        }

        void LaneClear(bool ignoreConfig = false)
        {
            var farmQ = ignoreConfig || _menu.Item("farmQ").GetValue<StringList>().SelectedIndex == 1 || _menu.Item("farmQ").GetValue<StringList>().SelectedIndex == 2;
            var farmE = ignoreConfig || _menu.Item("farmE").GetValue<bool>();

            List<Obj_AI_Base> minions;

            bool jungleMobs;
            if (farmQ && _spellQ.IsReady())
            {
                minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _spellQ.Range, MinionTypes.All, MinionTeam.NotAlly);
                jungleMobs = minions.Any(x => x.Team == GameObjectTeam.Neutral);

                _spellQ.Width = SpellQWidth;
                var farmInfo = _spellQ.GetCircularFarmLocation(minions, _spellQ.Width);

                if (farmInfo.MinionsHit >= 1)
                    CastQ(farmInfo.Position, jungleMobs ? 0 : _menu.Item("farmQPercent").GetValue<Slider>().Value);
            }

            if (!farmE || !_spellE.IsReady() || IsInPassiveForm())
                return;
            _comboE = false;

            minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _spellE.Range, MinionTypes.All, MinionTeam.NotAlly);

            jungleMobs = minions.Any(x => x.Team == GameObjectTeam.Neutral);

            var enoughMana = GetManaPercent() > _menu.Item("farmEPercent").GetValue<Slider>().Value;

            if (enoughMana && ((minions.Count >= 3 || jungleMobs) && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).ToggleState == 1))
                _spellE.Cast(ObjectManager.Player.Position, Packets());
            else if (!enoughMana || ((minions.Count <= 2 && !jungleMobs) && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).ToggleState == 2))
                RegulateEState(!enoughMana);
        }

        void LastHit()
        {
            var farmQ = _menu.Item("farmQ").GetValue<StringList>().SelectedIndex == 0 || _menu.Item("farmQ").GetValue<StringList>().SelectedIndex == 2;

            if (!farmQ || !_spellQ.IsReady())
                return;
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _spellQ.Range, MinionTypes.All, MinionTeam.NotAlly);

            foreach (var minion in minions.Where(x => DamageLib.getDmg(x, DamageLib.SpellType.Q, DamageLib.StageType.FirstDamage) >= //FirstDamage = multitarget hit, differentiate! (check radius around mob predicted pos)
                                                      HealthPrediction.GetHealthPrediction(x, (int)(_spellQ.Delay * 1000))))
            {
                CastQ(minion, _menu.Item("farmQPercent").GetValue<Slider>().Value);
            }
        }

        void UltKs()
        {
            if (!_spellR.IsReady())
                return;
            var time = Environment.TickCount;

            foreach (var target in Program.Helper.EnemyInfo.Where(x => //need to check if recently recalled (for cases when no mana for baseult)
                x.Player.IsValid &&
                !x.Player.IsDead &&
                x.Player.IsEnemy &&
                !(x.RecallInfo.Recall.Status == Packet.S2C.Recall.RecallStatus.RecallStarted && x.RecallInfo.GetRecallCountdown() < 3100) && //let BaseUlt handle this one
                ((!x.Player.IsVisible && time - x.LastSeen < 10000) || (x.Player.IsVisible && x.Player.IsValidTarget())) &&
                DamageLib.getDmg(x.Player, DamageLib.SpellType.R) >= Program.Helper.GetTargetHealth(x, (int)(_spellR.Delay * 1000f))))
            {
                if (target.Player.IsVisible || (!target.Player.IsVisible && time - target.LastSeen < 2750)) //allies still attacking target? prevent overkill
                    if (Program.Helper.OwnTeam.Any(x => !x.IsMe && x.Distance(target.Player) < 1600))
                        continue;

                if (IsInPassiveForm() || !Program.Helper.EnemyTeam.Any(x => x.IsValid && !x.IsDead && (x.IsVisible || (!x.IsVisible && time - Program.Helper.GetPlayerInfo(x).LastSeen < 2750)) && ObjectManager.Player.Distance(x) < 1600)) //any other enemies around? dont ult unless in passive form
                    _spellR.Cast(ObjectManager.Player.Position, Packets());
            }
        }

        void RegulateEState(bool ignoreTargetChecks = false)
        {
            if (!_spellE.IsReady() || IsInPassiveForm() ||
                ObjectManager.Player.Spellbook.GetSpell(SpellSlot.E).ToggleState != 2)
                return;
            var target = SimpleTs.GetTarget(_spellE.Range, SimpleTs.DamageType.Magical);
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _spellE.Range, MinionTypes.All, MinionTeam.NotAlly);

            if (!ignoreTargetChecks && (target != null || (!_comboE && minions.Count != 0)))
                return;
            _spellE.Cast(ObjectManager.Player.Position, Packets());
            _comboE = false;
        }

        void CastQ(Obj_AI_Base target, int minManaPercent = 0)
        {
            if (!_spellQ.IsReady() || !(GetManaPercent() >= minManaPercent))
                return;
            if (target == null)
                return;
            _spellQ.Width = GetDynamicQWidth(target);
            _spellQ.CastIfHitchanceEquals(target, HitChance.High, Packets());
        }

		void CastQ(Vector2 pos, int minManaPercent = 0)
		{
			if(!_spellQ.IsReady())
				return;
			if(GetManaPercent() >= minManaPercent)
				_spellQ.Cast(pos, Packets());
		}

        void CastW(Obj_AI_Base target, int minManaPercent = 0)
        {
            if (!_spellW.IsReady() || !(GetManaPercent() >= minManaPercent))
                return;
            if (target == null)
                return;
            _spellW.Width = GetDynamicWWidth(target);
            _spellW.Cast(target, Packets());
        }

		float GetDynamicWWidth(Obj_AI_Base target)
		{
			return Math.Max(70, (1f - (ObjectManager.Player.Distance(target) / _spellW.Range)) * SpellWWidth);
		}

		float GetDynamicQWidth(Obj_AI_Base target)
		{
			return Math.Max(30, (1f - (ObjectManager.Player.Distance(target) / _spellQ.Range)) * SpellQWidth);
		}

		static bool IsInPassiveForm()
		{
            return ObjectManager.Player.IsZombie; //!ObjectManager.Player.IsHPBarRendered;
		}

		void Drawing_OnDraw(EventArgs args)
		{
			if(!ObjectManager.Player.IsDead)
			{
				var drawQ = _menu.Item("drawQ").GetValue<Circle>();

				if(drawQ.Active)
					Utility.DrawCircle(ObjectManager.Player.Position, _spellQ.Range, drawQ.Color);
			}

			var victims = "";

			var time = Environment.TickCount;

			foreach(EnemyInfo target in Program.Helper.EnemyInfo.Where(x =>
				x.Player.IsValid &&
				!x.Player.IsDead &&
				x.Player.IsEnemy &&
				((!x.Player.IsVisible && time - x.LastSeen < 10000) || (x.Player.IsVisible && Utility.IsValidTarget(x.Player))) &&
				DamageLib.getDmg(x.Player, DamageLib.SpellType.R) >= Program.Helper.GetTargetHealth(x, (int)(_spellR.Delay * 1000f))))
			{
				victims += target.Player.ChampionName + " ";

				if(!_menu.Item("notifyPing").GetValue<bool>() ||
					(target.LastPinged != 0 && Environment.TickCount - target.LastPinged <= 11000))
					continue;
				if(!(ObjectManager.Player.Distance(target.Player) > 1800) ||
					(!target.Player.IsVisible && time - target.LastSeen <= 2750))
					continue;
				Program.Helper.Ping(target.Player.Position);
				target.LastPinged = Environment.TickCount;
			}

			if(victims != "" && _menu.Item("notifyR").GetValue<bool>())
			{
				Drawing.DrawText(Drawing.Width * 0.44f, Drawing.Height * 0.7f, System.Drawing.Color.GreenYellow, "Ult can kill: " + victims);

				//use when pos works
				//new Render.Text((int)(Drawing.Width * 0.44f), (int)(Drawing.Height * 0.7f), "Ult can kill: " + victims, 30, SharpDX.Color.Red); //.Add()
			}
		}
	}
}
