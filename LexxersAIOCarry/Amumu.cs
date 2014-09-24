using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace UltimateCarry
{
    class Amumu : Champion
    {
        readonly Menu _menu;

        private readonly Spell _spellQ;
        private readonly Spell _spellW;
        private readonly Spell _spellE;
        private readonly Spell _spellR;

        private bool _comboW;

        public Amumu() //add Q near mouse (range), 
        {
            _menu = Program.Menu;

            var comboMenu = _menu.AddSubMenu(new Menu("TeamFight", "TeamFight"));
            comboMenu.AddItem(new MenuItem("comboQ" + ObjectManager.Player.ChampionName, "Use Q").SetValue(new StringList(new[] { "No", "Always", "If out of range"}, 1)));
            comboMenu.AddItem(new MenuItem("comboW" + ObjectManager.Player.ChampionName, "Use W").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboE" + ObjectManager.Player.ChampionName, "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboR" + ObjectManager.Player.ChampionName, "Auto Use R").SetValue(new Slider(3, 0, 5)));
            comboMenu.AddItem(new MenuItem("comboWPercent" + ObjectManager.Player.ChampionName, "Use W until Mana %").SetValue(new Slider(10)));

            var farmMenu = _menu.AddSubMenu(new Menu("Farming", "Farming"));
            farmMenu.AddItem(new MenuItem("farmQ" + ObjectManager.Player.ChampionName, "Use Q").SetValue(new StringList(new[] { "No", "Always", "If out of range"}, 2)));
            farmMenu.AddItem(new MenuItem("farmW" + ObjectManager.Player.ChampionName, "Use W").SetValue(true));
            farmMenu.AddItem(new MenuItem("farmE" + ObjectManager.Player.ChampionName, "Use E").SetValue(true));
            farmMenu.AddItem(new MenuItem("farmWPercent" + ObjectManager.Player.ChampionName, "Use W until Mana %").SetValue(new Slider(20)));

            var drawMenu = _menu.AddSubMenu(new Menu("Drawing", "Drawing"));
            drawMenu.AddItem(new MenuItem("drawQ" + ObjectManager.Player.ChampionName, "Draw Q range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(125, 0, 255, 0))));
            drawMenu.AddItem(new MenuItem("drawW" + ObjectManager.Player.ChampionName, "Draw W range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(125, 0, 255, 0))));
            drawMenu.AddItem(new MenuItem("drawE" + ObjectManager.Player.ChampionName, "Draw E range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(125, 0, 255, 0))));
            drawMenu.AddItem(new MenuItem("drawR" + ObjectManager.Player.ChampionName, "Draw R range").SetValue(new Circle(false, System.Drawing.Color.FromArgb(125, 0, 255, 0))));

            var miscMenu = _menu.AddSubMenu(new Menu("Misc", "Misc"));
            miscMenu.AddItem(new MenuItem("aimQ" + ObjectManager.Player.ChampionName, "Q near mouse").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));

            _spellQ = new Spell(SpellSlot.Q, 1080);
            _spellW = new Spell(SpellSlot.W, 300);
            _spellE = new Spell(SpellSlot.E, 350);
            _spellR = new Spell(SpellSlot.R, 550);

            _spellQ.SetSkillshot(.25f, 90, 2000, true, SkillshotType.SkillshotLine);  //check delay
            _spellW.SetSkillshot(0f, _spellW.Range, float.MaxValue, false, SkillshotType.SkillshotCircle); //correct
            _spellE.SetSkillshot(.5f, _spellE.Range, float.MaxValue, false, SkillshotType.SkillshotCircle); //check delay
            _spellR.SetSkillshot(.25f, _spellR.Range, float.MaxValue, false, SkillshotType.SkillshotCircle); //check delay

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnGameUpdate += Game_OnGameUpdate;
            PluginLoaded();
        }

        void Game_OnGameUpdate(EventArgs args)
        {
            AutoUlt();

            if(_menu.Item("aimQ" + ObjectManager.Player.ChampionName).GetValue<KeyBind>().Active)
                CastQ(Program.Helper.EnemyTeam.Where(x => x.IsValidTarget(_spellQ.Range) && x.Distance(Game.CursorPos) < 400).OrderBy(x => x.Distance(Game.CursorPos)).FirstOrDefault());

            switch (Program.Orbwalker.ActiveMode)
            {
                case Orbwalking.OrbwalkingMode.Combo:
                    Combo();
                    break;
                case Orbwalking.OrbwalkingMode.LaneClear:
                    LaneClear();
                    break;
                default:
                    RegulateWState();
                    break;
            }
        }

        void AutoUlt()
        {
            var comboR = _menu.Item("comboR" + ObjectManager.Player.ChampionName).GetValue<Slider>().Value;

            if (comboR > 0 && _spellR.IsReady())
            {
                int enemiesHit = 0;
                int killableHits = 0;

                foreach (Obj_AI_Hero enemy in Program.Helper.EnemyTeam.Where(x => x.IsValidTarget(_spellR.Range)))
                {
                    var prediction = Prediction.GetPrediction(enemy, _spellR.Delay);

                    if (prediction != null && prediction.UnitPosition.Distance(ObjectManager.Player.ServerPosition) <= _spellR.Range)
                    {
                        enemiesHit++;

                        if (DamageLib.getDmg(enemy, DamageLib.SpellType.R) >= enemy.Health)
                            killableHits++;
                    }
                }

                if (enemiesHit >= comboR || (killableHits >= 1 && ObjectManager.Player.Health / ObjectManager.Player.MaxHealth <= 0.1))
                    CastR();
            }
        }

        void CastE(Obj_AI_Base target)
        {
            if (!_spellE.IsReady() || target == null || !target.IsValidTarget())
                return;

            if (_spellE.GetPrediction(target).UnitPosition.Distance(ObjectManager.Player.ServerPosition) <= _spellE.Range)
                _spellE.Cast(ObjectManager.Player.Position, Packets());
        }

        void Combo()
        {
            var comboQ = _menu.Item("comboQ" + ObjectManager.Player.ChampionName).GetValue<StringList>().SelectedIndex;
            var comboW = _menu.Item("comboW" + ObjectManager.Player.ChampionName).GetValue<bool>();
            var comboE = _menu.Item("comboE" + ObjectManager.Player.ChampionName).GetValue<bool>();
            var comboR = _menu.Item("comboR" + ObjectManager.Player.ChampionName).GetValue<Slider>().Value;

            if (comboQ > 0 && _spellQ.IsReady())
            {
                if (_spellR.IsReady() && comboR > 0) //search unit that provides most targets hit by ult. prioritize hero target unit
                {
                    int maxTargetsHit = 0;
                    Obj_AI_Base unitMostTargetsHit = null;

                    foreach (Obj_AI_Base unit in ObjectManager.Get<Obj_AI_Base>().Where(x => x.IsValidTarget(_spellQ.Range) && _spellQ.GetPrediction(x).Hitchance >= HitChance.High)) //causes troubles?
                    {
                        int targetsHit = Utility.CountEnemysInRange((int)_spellR.Range, unit); //unitposition might not reflect where you land with Q

                        if (targetsHit > maxTargetsHit || (unitMostTargetsHit != null && targetsHit >= maxTargetsHit && unit.Type == GameObjectType.obj_AI_Hero))
                        {
                            maxTargetsHit = targetsHit;
                            unitMostTargetsHit = unit;
                        }
                    }

                    if (maxTargetsHit >= comboR)
                        CastQ(unitMostTargetsHit);
                }

                Obj_AI_Base target = SimpleTs.GetTarget(_spellQ.Range, SimpleTs.DamageType.Magical);

                if (target != null)
                    if (comboQ == 1 || (comboQ == 2 && !Orbwalking.InAutoAttackRange(target)))
                        CastQ(target);
            }

            if (comboW && _spellW.IsReady())
            {
                var target = SimpleTs.GetTarget(_spellW.Range, SimpleTs.DamageType.Magical);

                if (target != null)
                {
                    var enoughMana = GetManaPercent() >= _menu.Item("comboWPercent" + ObjectManager.Player.ChampionName).GetValue<Slider>().Value;

                    if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1)
                    {
                        if (ObjectManager.Player.Distance(target.ServerPosition) <= _spellW.Range && enoughMana)
                        {
                            _comboW = true;
                            _spellW.Cast(ObjectManager.Player.Position, Packets());
                        }
                    }
                    else if (!enoughMana)
                        RegulateWState(true);
                }
                else
                    RegulateWState();
            }

            if (comboE && _spellE.IsReady())
                CastE(Program.Helper.EnemyTeam.OrderBy(x => x.Distance(ObjectManager.Player)).FirstOrDefault());
        }

        void LaneClear()
        {
            var farmQ = _menu.Item("farmQ" + ObjectManager.Player.ChampionName).GetValue<StringList>().SelectedIndex;
            var farmW = _menu.Item("farmW" + ObjectManager.Player.ChampionName).GetValue<bool>();
            var farmE = _menu.Item("farmE" + ObjectManager.Player.ChampionName).GetValue<bool>();

            List<Obj_AI_Base> minions;

            if (farmQ > 0 && _spellQ.IsReady())
            {
                Obj_AI_Base minion = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _spellQ.Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth).FirstOrDefault(x => _spellQ.GetPrediction(x).Hitchance >= HitChance.Medium);

                if (minion != null)
                    if (farmQ == 1 || (farmQ == 2 && !Orbwalking.InAutoAttackRange(minion)))
                        CastQ(minion, HitChance.Medium);
            }

            if (farmE && _spellE.IsReady())
            {
                minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _spellE.Range, MinionTypes.All, MinionTeam.NotAlly);
                CastE(minions.OrderBy(x => x.Distance(ObjectManager.Player)).FirstOrDefault());
            }

            if (!farmW || !_spellW.IsReady())
                return;
            _comboW = false;

            minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _spellW.Range, MinionTypes.All, MinionTeam.NotAlly);

            bool anyJungleMobs = minions.Any(x => x.Team == GameObjectTeam.Neutral);

            var enoughMana = GetManaPercent() > _menu.Item("farmWPercent" + ObjectManager.Player.ChampionName).GetValue<Slider>().Value;

            if (enoughMana && ((minions.Count >= 3 || anyJungleMobs) && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 1))
                _spellW.Cast(ObjectManager.Player.Position, Packets());
            else if (!enoughMana || ((minions.Count <= 2 && !anyJungleMobs) && ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ToggleState == 2))
                RegulateWState(!enoughMana);
        }

        void RegulateWState(bool ignoreTargetChecks = false)
        {
            if (!_spellW.IsReady() || ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).ToggleState != 2)
                return;

            var target = SimpleTs.GetTarget(_spellW.Range, SimpleTs.DamageType.Magical);
            var minions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, _spellW.Range, MinionTypes.All, MinionTeam.NotAlly);

            if (!ignoreTargetChecks && (target != null || (!_comboW && minions.Count != 0)))
                return;

            _spellW.Cast(ObjectManager.Player.Position, Packets());
            _comboW = false;
        }

        void CastQ(Obj_AI_Base target, HitChance hitChance = HitChance.High)
        {
            if (!_spellQ.IsReady())
                return;
            if (target == null || !target.IsValidTarget())
                return;

            _spellQ.CastIfHitchanceEquals(target, hitChance, Packets());
        }

        void CastR()
        {
            if (!_spellR.IsReady())
                return;
            _spellR.Cast(ObjectManager.Player.Position, Packets());
        }

        void Drawing_OnDraw(EventArgs args)
        {
            if (!ObjectManager.Player.IsDead)
            {
                var drawQ = _menu.Item("drawQ" + ObjectManager.Player.ChampionName).GetValue<Circle>();
                var drawW = _menu.Item("drawW" + ObjectManager.Player.ChampionName).GetValue<Circle>();
                var drawE = _menu.Item("drawE" + ObjectManager.Player.ChampionName).GetValue<Circle>();
                var drawR = _menu.Item("drawR" + ObjectManager.Player.ChampionName).GetValue<Circle>();

                if (drawQ.Active)
                    Utility.DrawCircle(ObjectManager.Player.Position, _spellQ.Range, drawQ.Color);

                if (drawW.Active)
                    Utility.DrawCircle(ObjectManager.Player.Position, _spellW.Range, drawW.Color);

                if (drawE.Active)
                    Utility.DrawCircle(ObjectManager.Player.Position, _spellE.Range, drawE.Color);

                if (drawR.Active)
                    Utility.DrawCircle(ObjectManager.Player.Position, _spellR.Range, drawR.Color);
            }
        }
    }
}
