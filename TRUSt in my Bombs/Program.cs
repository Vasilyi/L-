#region
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
#endregion

namespace TRUStInMyBombs
{
    public class Program
    {

        public static Menu Config;
        private static Obj_AI_Hero Player;
        private static List<JumpSpot> Jump;

        public static Spell Q;
        public static Spell Q2;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static SpellSlot IgniteSlot;


        public class Spells
        {
            public static float qRange = 850f;
            public static float qBounceRange = 650f;
            public static float wRange = 1000f;
            public static float eRange = 900f;
            public static float rRange = 5300f;
        }

        public static void DrawJumpSpots()
        {
            foreach (JumpSpot Jumppos in Jump)
            {
                Vector3 convertcoords = new Vector3(Jumppos.Jumppos.X, Jumppos.Jumppos.Z, Jumppos.Jumppos.Y);
                var drawdistance = Config.Item("satchelDrawdistance").GetValue<Slider>().Value;
                var colordraw = Config.Item("satchelDraw").GetValue<Circle>();
                if (drawdistance > ObjectManager.Player.Distance(convertcoords))
                {
                    Utility.DrawCircle(convertcoords, 80, colordraw.Color);
                    
                }
            }
        }
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != "Ziggs")
            {
                return;
            }
            Console.WriteLine("TRUStInMyBombs LOADED");
            InitializeJumpSpots();
            Game.OnGameUpdate += Game_OnGameUpdate;
            Interrupter.OnPosibleToInterrupt += ZOnPosibleToInterrupt;


            Player = ObjectManager.Player;
            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
            Q = new Spell(SpellSlot.Q, Spells.qRange);
            Q2 = new Spell(SpellSlot.Q, Spells.qRange + Spells.qBounceRange);
            W = new Spell(SpellSlot.W, Spells.wRange);
            E = new Spell(SpellSlot.E, Spells.eRange);
            R = new Spell(SpellSlot.R, Spells.rRange);


            Q.SetSkillshot(0.25f, 60f, 1750, false, SkillshotType.SkillshotCircle);
            Q2.SetSkillshot(0.7f, 60f, 1200, true, SkillshotType.SkillshotLine);
            //W.SetSkillshot(0.7f, 125f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.5f, 0f, 1750f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(1f, 550f, 1750f, false, SkillshotType.SkillshotCircle);
            try
            {
                //Create the menu
                Config = new Menu("TRUStInMyBombs", "mainmenu", true);

                Config.AddSubMenu(new Menu("HotKeys:", "hotkeys"));
                Config.SubMenu("hotkeys").AddItem(new MenuItem("ComboKey", "Combo!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                Config.SubMenu("hotkeys").AddItem(new MenuItem("HarassKey", "Harrass").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                Config.SubMenu("hotkeys").AddItem(new MenuItem("FarmKeyFreeze", "Farm Freeze").SetValue(new KeyBind("G".ToCharArray()[0], KeyBindType.Press)));
                Config.SubMenu("hotkeys").AddItem(new MenuItem("FarmKeyClear", "Farm Clear").SetValue(new KeyBind("H".ToCharArray()[0], KeyBindType.Press)));

                Config.AddSubMenu(new Menu("Auto Ultimate Logic:", "ultlogic"));
                Config.SubMenu("ultlogic").AddItem(new MenuItem("useRauto", "Use - Mega Inferno Bomb (AUTO)").SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                Config.SubMenu("ultlogic").AddItem(new MenuItem("enemysforR", "Minimal Enemys Amount")).SetValue(new Slider(3, 5, 1));

                Config.AddSubMenu(new Menu("Misc:", "misc"));
                Config.SubMenu("misc").AddItem(new MenuItem("interrupt", "Interrupt Spells").SetValue(true));

                Config.AddSubMenu(new Menu("KillSteal:", "killsteal"));
                Config.SubMenu("killsteal").AddItem(new MenuItem("ksR", "KS - Mega Inferno Bomb").SetValue(true));
                Config.SubMenu("killsteal").AddItem(new MenuItem("ksRRange", "KS - Inferno Bomb Range")).SetValue(new Slider(1000, 5300, 1));
                Config.SubMenu("killsteal").AddItem(new MenuItem("ksAll", "KS - Everything").SetValue(true));

                Config.AddSubMenu(new Menu("Auto Farm:", "autofarm"));
                Config.SubMenu("autofarm").AddItem(new MenuItem("farmQ", "Use - Bouncing Bomb").SetValue(true));
                Config.SubMenu("autofarm").AddItem(new MenuItem("farmE", "Use - Hexplosive Minefield").SetValue(true));

                Config.AddSubMenu(new Menu("Satchel Jumping Options:", "satchel"));
                Config.SubMenu("satchel").AddItem(new MenuItem("satchelDraw", "Draw satchel places").SetValue(new Circle(true, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
                Config.SubMenu("satchel").AddItem(new MenuItem("satchelDrawdistance", "Don't draw circles if the distance >")).SetValue(new Slider(2000, 10000, 1));
                Config.SubMenu("satchel").AddItem(new MenuItem("satchelJump", "Jump Key").SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));

                Config.AddSubMenu(new Menu("Combo Options:", "combospells"));
                Config.SubMenu("combospells").AddItem(new MenuItem("UseI", "Use Ignite if enemy is killable").SetValue(true));
                //Config.SubMenu("combospells").AddItem(new MenuItem("dfg", "Use DFG in full combo").SetValue(true));
                Config.SubMenu("combospells").AddItem(new MenuItem("useB", "Use - Bouncing Bomb").SetValue(true));
                Config.SubMenu("combospells").AddItem(new MenuItem("useE", "Use - Hexplosive Minefield").SetValue(true));
                Config.SubMenu("combospells").AddItem(new MenuItem("useR", "Use - Mega Inferno Bomb").SetValue(true));

                Config.AddSubMenu(new Menu("Harass Options:", "harassspells"));
                Config.SubMenu("harassspells").AddItem(new MenuItem("useBHarass", "Use - Bouncing Bomb").SetValue(true));
                Config.SubMenu("harassspells").AddItem(new MenuItem("useEHarass", "Use - Hexplosive Minefield").SetValue(true));

                Config.AddSubMenu(new Menu("Draw Options:", "drawing"));
                Config.SubMenu("drawing").AddItem(new MenuItem("noDraw", "Disable - Drawing").SetValue(true));
                //Config.SubMenu("drawing").AddItem(new MenuItem("drawDmg", "Draw - Damage Marks").SetValue(true));
                Config.SubMenu("drawing").AddItem(new MenuItem("drawF", "Draw - Furthest Spell Available").SetValue(true));
                Config.SubMenu("drawing").AddItem(new MenuItem("drawB", "Draw - Bouncing Bomb").SetValue(new Circle(true, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
                Config.SubMenu("drawing").AddItem(new MenuItem("drawQ", "Draw - Bomb (without bounce)").SetValue(new Circle(true, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
                Config.SubMenu("drawing").AddItem(new MenuItem("drawW", "Draw - Satchel Charge").SetValue(new Circle(true, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
                Config.SubMenu("drawing").AddItem(new MenuItem("drawE", "Draw - Hexplosive Minefield").SetValue(new Circle(true, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
                Config.SubMenu("drawing").AddItem(new MenuItem("drawR", "Draw - Mega Inferno Bomb").SetValue(new Circle(true, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
                Config.AddToMainMenu();
                Drawing.OnDraw += OnDraw;
            }
            catch (Exception)
            {
                Game.PrintChat("Error found in bombs. Refused to load.");
            }
        }

        public static void Game_OnGameUpdate(EventArgs args)
        {
            if (Config.Item("ComboKey").GetValue<KeyBind>().Active)
            {
               
                Combo();
            }
            if (Config.Item("HarassKey").GetValue<KeyBind>().Active)
            {
                //Console.WriteLine("DOING HARASS");
                Harass();
            }
            if (Config.Item("ksR").GetValue<bool>())
            {
                Ks();
            }
            if (Config.Item("useRauto").GetValue<KeyBind>().Active)
            {
                AutoUlt();
            }

            if (Config.Item("satchelJump").GetValue<KeyBind>().Active)
            {
                Console.WriteLine("STARTING JUMP");
                JumpProx();
            }

            var lc = Config.Item("FarmKeyClear").GetValue<KeyBind>().Active;
            if (lc || Config.Item("FarmKeyFreeze").GetValue<KeyBind>().Active)
                Farm(lc);
        }

        private static void OnDraw(EventArgs args)
        {
            if (Config.Item("satchelDraw").GetValue<Circle>().Active)
            {
                DrawJumpSpots();
            }

            if (Config.Item("noDraw").GetValue<bool>())
            {
                return;
            }
            var qValue = Config.Item("drawB").GetValue<Circle>();
            var qValue2 = Config.Item("drawQ").GetValue<Circle>();
            var wValue = Config.Item("drawW").GetValue<Circle>();
            var eValue = Config.Item("drawE").GetValue<Circle>();
            var rValue = Config.Item("drawR").GetValue<Circle>();
            if (Config.Item("drawF").GetValue<bool>())
            {
                if (Q.IsReady())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, Q2.Range, qValue.Color);
                    Utility.DrawCircle(ObjectManager.Player.Position, R.Range, rValue.Color, 5, 30, true);
            }
                else if (W.IsReady())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, W.Range, qValue.Color);
                }
                else if (E.IsReady())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, E.Range, qValue.Color);
                }
                else if (R.IsReady())
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, R.Range, qValue.Color);
                    Utility.DrawCircle(ObjectManager.Player.Position, R.Range, rValue.Color, 5, 30, true);
                }
                return;
            }
            if (qValue.Active)
            {
                Utility.DrawCircle(ObjectManager.Player.Position, Q2.Range, qValue.Color);
            }

            if (qValue2.Active)
            {
                Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, qValue.Color);
            }

            if (wValue.Active)
            {
                Utility.DrawCircle(ObjectManager.Player.Position, W.Range, wValue.Color);
            }


            if (eValue.Active)
            {
                Utility.DrawCircle(ObjectManager.Player.Position, E.Range, eValue.Color);
            }


            if (rValue.Active)
            {
                Utility.DrawCircle(ObjectManager.Player.Position, R.Range, rValue.Color);
                Utility.DrawCircle(ObjectManager.Player.Position, R.Range, rValue.Color,5,30,true);
            }


        }

        private static void AutoUlt()
        {
            var qTarget = SimpleTs.GetTarget(Q2.Range, SimpleTs.DamageType.Magical);
            var rTarget = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
            var rTargetsamount = Config.Item("enemysforR").GetValue<Slider>().Value;
            if (R.IsReady() && (qTarget != null || rTarget != null))
            {
                R.CastIfWillHit(qTarget, rTargetsamount);
                R.CastIfWillHit(qTarget, rTargetsamount);
            }
        }
        private static void Farm(bool laneClear)
        {

            var rangedMinions = MinionManager.GetMinions(
                ObjectManager.Player.ServerPosition, Q2.Range, MinionTypes.Ranged);
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, Q2.Range);

            var useQ = Config.Item("farmQ").GetValue<bool>();
            var useE = Config.Item("farmE").GetValue<bool>();

            if (laneClear)
            {
                if (Q.IsReady() && useQ)
                {
                    var rangedLocation = Q2.GetCircularFarmLocation(rangedMinions);
                    var location = Q2.GetCircularFarmLocation(allMinions);

                    var bLocation = (location.MinionsHit > rangedLocation.MinionsHit + 1) ? location : rangedLocation;

                    if (bLocation.MinionsHit > 0)
                    {
                        Q2.Cast(bLocation.Position.To3D());
                    }
                }

                if (E.IsReady() && useE)
                {
                    var rangedLocation = E.GetCircularFarmLocation(rangedMinions, E.Width * 2);
                    var location = E.GetCircularFarmLocation(allMinions, E.Width * 2);

                    var bLocation = (location.MinionsHit > rangedLocation.MinionsHit + 1) ? location : rangedLocation;

                    if (bLocation.MinionsHit > 2)
                    {
                        E.Cast(bLocation.Position.To3D());
                    }
                }
            }
            else
            {
                if (useQ && Q.IsReady())
                {
                    foreach (var minion in allMinions)
                    {
                        if (!Orbwalking.InAutoAttackRange(minion))
                        {
                            var Qdamage = DamageLib.getDmg(minion, DamageLib.SpellType.Q) * 0.75;

                            if (Qdamage > Q.GetHealthPrediction(minion))
                            {
                                Q2.Cast(minion);
                            }
                        }
                    }
                }

                if (E.IsReady() && useE)
                {
                    var rangedLocation = E.GetCircularFarmLocation(rangedMinions, E.Width * 2);
                    var location = E.GetCircularFarmLocation(allMinions, E.Width * 2);

                    var bLocation = (location.MinionsHit > rangedLocation.MinionsHit + 1) ? location : rangedLocation;

                    if (bLocation.MinionsHit > 2)
                    {
                        E.Cast(bLocation.Position.To3D());
                    }
                }
            }
        }

        private static void ZOnPosibleToInterrupt(Obj_AI_Base unit, InterruptableSpell spell)
        {
            if (!Config.Item("interrupt").GetValue<bool>()) return;

            if (Player.Distance(unit) < W.Range)
            {
                W.Cast(unit);
            }
        }

        private static void Ks()
        {
            var rTarget = SimpleTs.GetTarget(Config.Item("ksRRange").GetValue<Slider>().Value, SimpleTs.DamageType.Magical);
            var qTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var qTarget2 = SimpleTs.GetTarget(Q2.Range, SimpleTs.DamageType.Magical);
            var eTarget = SimpleTs.GetTarget(E.Range + W.Width * 0.5f, SimpleTs.DamageType.Magical);
            var ksAll = Config.Item("ksAll").GetValue<bool>();
            if (Q.IsReady() && qTarget2 != null && qTarget2.Health < DamageLib.getDmg(qTarget2, DamageLib.SpellType.Q) && ksAll)
            {
                Q2.Cast(qTarget2);
            }
            else if (R.IsReady() && rTarget != null && rTarget.Health < DamageLib.getDmg(rTarget, DamageLib.SpellType.R))
            {
                R.Cast(rTarget);
            }
            else if (Q.IsReady() && qTarget != null && qTarget.Health < (DamageLib.getDmg(qTarget, DamageLib.SpellType.Q) + DamageLib.getDmg(rTarget, DamageLib.SpellType.R)) && ksAll)
            {
                UseSpells(true, false, true);
            }
        }
        private static void JumpProx()
        {
            if (FindNearestJumpSpot() == null)
            {
                return;
            }
            JumpSpot nearestspot = FindNearestJumpSpot();
            Vector3 nearestJump = new Vector3(nearestspot.Jumppos.X, nearestspot.Jumppos.Z, nearestspot.Jumppos.Y);
            Vector3 moveposition = new Vector3(nearestspot.MovePosition.X, nearestspot.MovePosition.Z, nearestspot.MovePosition.Y);
            Console.WriteLine(nearestJump.ToString());
            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "ZiggsW")
            {
                ObjectManager.Player.Spellbook.CastSpell(SpellSlot.W, nearestJump);
            }
            else if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name != "ZiggsW" && ObjectManager.Player.Distance(moveposition) > 60)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, moveposition);
            }
            else if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name != "ZiggsW" && ObjectManager.Player.Distance(moveposition) < 60)
            {
                ObjectManager.Player.Spellbook.CastSpell(SpellSlot.W);
            }
        }



        public static JumpSpot FindNearestJumpSpot()
        {
            foreach (JumpSpot Jumppos in Jump)
            {
                Vector3 convertcoords = new Vector3(Jumppos.Jumppos.X, Jumppos.Jumppos.Z, Jumppos.Jumppos.Y);
                Vector3 moveposition = new Vector3(Jumppos.MovePosition.X, Jumppos.MovePosition.Z, Jumppos.MovePosition.Y);
                double cursorDistToWard = Math.Sqrt(Math.Pow(convertcoords.X - Game.CursorPos.X, 2) +
                             Math.Pow(convertcoords.Y - Game.CursorPos.Y, 2) +
                             Math.Pow(convertcoords.Z - Game.CursorPos.Z, 2));

                double playerDistToWard = Math.Sqrt(Math.Pow(convertcoords.X - ObjectManager.Player.Position.X, 2) +
                                                    Math.Pow(convertcoords.Y - ObjectManager.Player.Position.Y, 2) +
                                                    Math.Pow(convertcoords.Z - ObjectManager.Player.Position.Z, 2));

                if (cursorDistToWard <= 250.0 && playerDistToWard <= 900)
                {
                    return Jumppos;
                }
            }

            return null;
        }

        private static void Combo()
        {
            UseSpells(Config.Item("useB").GetValue<bool>(), Config.Item("useE").GetValue<bool>(),
                Config.Item("useR").GetValue<bool>());
            var qTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            if (IgniteSlot != SpellSlot.Unknown &&
                ObjectManager.Player.SummonerSpellbook.CanUseSpell(IgniteSlot) == SpellState.Ready &&
                ObjectManager.Player.Distance(qTarget) < 600 &&
                DamageLib.getDmg(qTarget, DamageLib.SpellType.IGNITE) > qTarget.Health)
            {
                ObjectManager.Player.SummonerSpellbook.CastSpell(IgniteSlot, qTarget);
            }
        }

        private static void Harass()
        {
            UseSpells(Config.Item("useBHarass").GetValue<bool>(), Config.Item("useEHarass").GetValue<bool>(), false);
        }


        private static void UseSpells(bool useQ, bool useE, bool useR)
        {
            var qTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var qTarget2 = SimpleTs.GetTarget(Q2.Range, SimpleTs.DamageType.Magical);
            var eTarget = SimpleTs.GetTarget(E.Range + W.Width * 0.5f, SimpleTs.DamageType.Magical);
            if (useQ && Q.IsReady() && qTarget != null)
            {
                Q.Cast(qTarget);
            }
            if (useQ && Q.IsReady() && qTarget2 != null)
            {
                Vector3 frompos = ObjectManager.Player.ServerPosition.To2D().Extend(qTarget2.ServerPosition.To2D(), Q.Range).To3D();
                Q2.UpdateSourcePosition(frompos);
                Q2.Cast(qTarget2);
                Q2.UpdateSourcePosition();
            }

            if (eTarget != null && useE && E.IsReady())
            {
                E.Cast(eTarget);
            }

            if (eTarget != null && useR && R.IsReady())
            {
                R.Cast(eTarget, false, true);
            }

        }


        private static void InitializeJumpSpots()
        {
            Jump = new List<JumpSpot>();
            Jump.Add(new JumpSpot(new Vector3(5182.2998046875f, 54.800712585449f, 7440.966796875f), new Vector3(5304.58984375f, 54.800712585449f, 7565.5278320313f)));
            Jump.Add(new JumpSpot(new Vector3(5476.32421875f, -58.546016693115f, 8302.73046875f), new Vector3(5416.7905273438f, -57.956558227539f, 8127.6274414063f)));
            Jump.Add(new JumpSpot(new Vector3(4340.427734375f, 52.527732849121f, 7379.603515625f), new Vector3(4190.7915039063f, 52.527732849121f, 7456.7749023438f)));
            Jump.Add(new JumpSpot(new Vector3(4861.8642578125f, -62.949485778809f, 10363.97265625f), new Vector3(5000.3354492188f, -62.949485778809f, 10470.788085938f)));
            Jump.Add(new JumpSpot(new Vector3(6525.5727539063f, 55.67924118042f, 9764.90234375f), new Vector3(6543.2924804688f, 55.67924118042f, 9906.08984375f)));
            Jump.Add(new JumpSpot(new Vector3(8502.6015625f, 56.100059509277f, 4030.9699707031f), new Vector3(8629.169921875f, 56.100059509277f, 4037.0673828125f)));
            Jump.Add(new JumpSpot(new Vector3(9579.015625f, -63.261302947998f, 3906.39453125f), new Vector3(9639.9208984375f, -63.261302947998f, 3721.6237792969f)));
            Jump.Add(new JumpSpot(new Vector3(9122.095703125f, -63.247100830078f, 4092.3359375f), new Vector3(9004.9052734375f, -63.247100830078f, 3957.8447265625f)));
            Jump.Add(new JumpSpot(new Vector3(7258.9287109375f, 57.113296508789f, 3830.3498535156f), new Vector3(7247.9721679688f, 57.113296508789f, 4008.0791015625f)));
            Jump.Add(new JumpSpot(new Vector3(6717.4697265625f, 60.789710998535f, 5199.8627929688f), new Vector3(6707.7407226563f, 60.789710998535f, 5358.189453125f)));
            Jump.Add(new JumpSpot(new Vector3(5947.009765625f, 54.906421661377f, 5799.0791015625f), new Vector3(6038.1240234375f, 54.906421661377f, 5719.5639648438f)));
            Jump.Add(new JumpSpot(new Vector3(5815.9038085938f, 52.852485656738f, 3194.2316894531f), new Vector3(5907.134765625f, 52.852485656738f, 3253.7680664063f)));
            Jump.Add(new JumpSpot(new Vector3(5089.8999023438f, 54.250331878662f, 6005.5874023438f), new Vector3(4989.0200195313f, 54.250331878662f, 6054.4975585938f)));
            Jump.Add(new JumpSpot(new Vector3(3048.2485351563f, 55.628494262695f, 6029.0205078125f), new Vector3(2962.1896972656f, 55.628494262695f, 5933.5864257813f)));
            Jump.Add(new JumpSpot(new Vector3(2134.3784179688f, 60.152767181396f, 6418.15234375f), new Vector3(2048.9326171875f, 60.152767181396f, 6406.2651367188f)));
            Jump.Add(new JumpSpot(new Vector3(1651.7109375f, 53.561576843262f, 7525.001953125f), new Vector3(1699.1987304688f, 53.561576843262f, 7631.7885742188f)));
            Jump.Add(new JumpSpot(new Vector3(1136.0306396484f, 50.775238037109f, 8481.19921875f), new Vector3(1247.5817871094f, 50.775238037109f, 8500.8662109375f)));
            Jump.Add(new JumpSpot(new Vector3(2433.314453125f, 53.364398956299f, 9980.5634765625f), new Vector3(2457.7978515625f, 53.364398956299f, 10102.342773438f)));
            Jump.Add(new JumpSpot(new Vector3(4946.9228515625f, 41.375110626221f, 12027.184570313f), new Vector3(4949.6733398438f, 41.375110626221f, 11907.3515625f)));
            Jump.Add(new JumpSpot(new Vector3(5993.0654296875f, 54.33109664917f, 11314.407226563f), new Vector3(5992.3364257813f, 54.33109664917f, 11414.142578125f)));
            Jump.Add(new JumpSpot(new Vector3(4985.8022460938f, 46.194820404053f, 11392.716796875f), new Vector3(4980.814453125f, 46.194820404053f, 11494.674804688f)));
            Jump.Add(new JumpSpot(new Vector3(6996.1748046875f, 53.763172149658f, 12262.01171875f), new Vector3(7003.19140625f, 53.763172149658f, 12405.4140625f)));
            Jump.Add(new JumpSpot(new Vector3(8423.283203125f, 47.13533782959f, 12247.524414063f), new Vector3(8546.052734375f, 47.13533782959f, 12225.833007813f)));
            Jump.Add(new JumpSpot(new Vector3(9263.97265625f, 52.484786987305f, 11869.091796875f), new Vector3(9389.8525390625f, 52.484786987305f, 11863.307617188f)));
            Jump.Add(new JumpSpot(new Vector3(9115.283203125f, 52.227199554443f, 12283.983398438f), new Vector3(9000.3017578125f, 52.227199554443f, 12261.0078125f)));
            Jump.Add(new JumpSpot(new Vector3(9994.4912109375f, 106.22331237793f, 11870.6875f), new Vector3(9894.1484375f, 106.22331237793f, 11853.583984375f)));
            Jump.Add(new JumpSpot(new Vector3(8409.6875f, 53.670509338379f, 10373.21875f), new Vector3(8534.4619140625f, 53.670509338379f, 10302.107421875f)));
            Jump.Add(new JumpSpot(new Vector3(4118.3876953125f, 108.71948242188f, 2121.7075195313f), new Vector3(4247f, 108.71948242188f, 2115f)));
            Jump.Add(new JumpSpot(new Vector3(4725.486328125f, 54.231761932373f, 2683.4543457031f), new Vector3(4607.6743164063f, 54.231761932373f, 2659.8942871094f)));
            Jump.Add(new JumpSpot(new Vector3(4897.533203125f, 54.2516746521f, 2052.708984375f), new Vector3(4996.212890625f, 54.2516746521f, 2028.4288330078f)));
            Jump.Add(new JumpSpot(new Vector3(5648.9956054688f, 55.286037445068f, 2016.7189941406f), new Vector3(5523.005859375f, 55.286037445068f, 2001.3936767578f)));
            Jump.Add(new JumpSpot(new Vector3(7022.462890625f, 52.594055175781f, 1376.6743164063f), new Vector3(7044.1245117188f, 52.594055175781f, 1469.1911621094f)));
            Jump.Add(new JumpSpot(new Vector3(7134.9140625f, 54.548675537109f, 2140.7275390625f), new Vector3(7133f, 54.548675537109f, 1977f)));
            Jump.Add(new JumpSpot(new Vector3(7945.6557617188f, 54.276401519775f, 2450.0712890625f), new Vector3(7942.3046875f, 54.276401519775f, 2660.9660644531f)));
            Jump.Add(new JumpSpot(new Vector3(9100.7197265625f, 60.792221069336f, 3073.9343261719f), new Vector3(9093.5087890625f, 60.792221069336f, 2917.2602539063f)));
            Jump.Add(new JumpSpot(new Vector3(9058.2998046875f, 68.232513427734f, 2428.3017578125f), new Vector3(9042.6005859375f, 68.232513427734f, 2530.5822753906f)));
            Jump.Add(new JumpSpot(new Vector3(9769.3544921875f, 68.960105895996f, 2188.2644042969f), new Vector3(9786.37890625f, 68.960105895996f, 2020.0227050781f)));
            Jump.Add(new JumpSpot(new Vector3(9871.234375f, 52.962394714355f, 1379.2374267578f), new Vector3(9860.5283203125f, 52.962394714355f, 1517.3566894531f)));
            Jump.Add(new JumpSpot(new Vector3(10133.500976563f, 49.336658477783f, 3153.0109863281f), new Vector3(10045.125f, 49.336658477783f, 3253.6359863281f)));
            Jump.Add(new JumpSpot(new Vector3(11265.125976563f, -62.610431671143f, 4252.2177734375f), new Vector3(11390.166992188f, -62.610431671143f, 4313.8203125f)));
            Jump.Add(new JumpSpot(new Vector3(11746.482421875f, 51.986545562744f, 4655.6328125f), new Vector3(11666.5859375f, 51.986545562744f, 4487.162109375f)));
            Jump.Add(new JumpSpot(new Vector3(12036.173828125f, 59.147567749023f, 5541.5102539063f), new Vector3(11895.735351563f, 59.147567749023f, 5513.7592773438f)));
            Jump.Add(new JumpSpot(new Vector3(11422.623046875f, 54.825256347656f, 5447.9135742188f), new Vector3(11555f, 54.825256347656f, 5475f)));
            Jump.Add(new JumpSpot(new Vector3(10471.787109375f, 54.86909866333f, 6708.9833984375f), new Vector3(10302.061523438f, 54.86909866333f, 6713.6376953125f)));
            Jump.Add(new JumpSpot(new Vector3(10763.604492188f, 54.87166595459f, 6806.6303710938f), new Vector3(10779.88671875f, 54.87166595459f, 6933.4072265625f)));
            Jump.Add(new JumpSpot(new Vector3(12053.392578125f, 54.827217102051f, 6344.705078125f), new Vector3(12133.737304688f, 54.827217102051f, 6425.1333007813f)));
            Jump.Add(new JumpSpot(new Vector3(12201.602539063f, 55.32479095459f, 5832.1831054688f), new Vector3(12343.471679688f, 55.32479095459f, 5817.8686523438f)));
            Jump.Add(new JumpSpot(new Vector3(11833.345703125f, 50.354991912842f, 9526.79296875f), new Vector3(11853.022460938f, 50.354991912842f, 9625.810546875f)));
            Jump.Add(new JumpSpot(new Vector3(11947.16015625f, 106.82741546631f, 10174.01171875f), new Vector3(11909f, 106.82741546631f, 10015f)));
            Jump.Add(new JumpSpot(new Vector3(11501.220703125f, 53.453559875488f, 8731.6298828125f), new Vector3(11384.24609375f, 53.453559875488f, 8615.3408203125f)));
            Jump.Add(new JumpSpot(new Vector3(10552.061523438f, 65.851661682129f, 8096.9360351563f), new Vector3(10495f, 65.851661682129f, 7963f)));
            Jump.Add(new JumpSpot(new Vector3(10462.163085938f, 55.272270202637f, 7388.1103515625f), new Vector3(10495.671875f, 55.272270202637f, 7499.1953125f)));
            Jump.Add(new JumpSpot(new Vector3(9902.44921875f, 55.129611968994f, 6451.4887695313f), new Vector3(10029.932617188f, 55.129611968994f, 6473.9155273438f)));
            Jump.Add(new JumpSpot(new Vector3(8837.2626953125f, -64.537475585938f, 5314.8232421875f), new Vector3(8753.697265625f, -64.537475585938f, 5169.1889648438f)));
            Jump.Add(new JumpSpot(new Vector3(8584.904296875f, -64.85913848877f, 6212.0083007813f), new Vector3(8648.8125f, -64.85913848877f, 6310.46484375f)));
            Jump.Add(new JumpSpot(new Vector3(8980.521484375f, 55.912460327148f, 6930.9438476563f), new Vector3(8895f, 55.912460327148f, 6815f)));
            Jump.Add(new JumpSpot(new Vector3(2241.9340820313f, 109.32015228271f, 4209.6376953125f), new Vector3(2258.6662597656f, 109.32015228271f, 4336.8940429688f)));
            Jump.Add(new JumpSpot(new Vector3(2362.7917480469f, 56.317901611328f, 4921.134765625f), new Vector3(2331f, 56.317901611328f, 4767f)));
            Jump.Add(new JumpSpot(new Vector3(2592.4208984375f, 60.191635131836f, 5629.8041992188f), new Vector3(2701f, 60.191635131836f, 5699f)));
            Jump.Add(new JumpSpot(new Vector3(3527.6638183594f, 55.608444213867f, 6323.6030273438f), new Vector3(3537.6516113281f, 55.608444213867f, 6451.9873046875f)));
            Jump.Add(new JumpSpot(new Vector3(3340.2841796875f, 53.313583374023f, 6995.8481445313f), new Vector3(3346.0974121094f, 53.313583374023f, 6864.9716796875f)));
            Jump.Add(new JumpSpot(new Vector3(3526.2731933594f, 54.509613037109f, 7071.9296875f), new Vector3(3522.7084960938f, 54.509613037109f, 7185.6630859375f)));
            Jump.Add(new JumpSpot(new Vector3(3516.3874511719f, 53.838798522949f, 7711.3291015625f), new Vector3(3686.1735839844f, 53.838798522949f, 7713.0024414063f)));
            Jump.Add(new JumpSpot(new Vector3(6438.3310546875f, -64.068969726563f, 8201.734375f), new Vector3(6466.015625f, -64.068969726563f, 8358.650390625f)));
            Jump.Add(new JumpSpot(new Vector3(6550.9438476563f, 56.018665313721f, 8759.0458984375f), new Vector3(6547f, 56.018665313721f, 8613f)));
            Jump.Add(new JumpSpot(new Vector3(7040.94140625f, 56.018997192383f, 8698.333984375f), new Vector3(7134.6791992188f, 56.018997192383f, 8759.9619140625f)));
            Jump.Add(new JumpSpot(new Vector3(8070.28125f, 55.055992126465f, 8696.9052734375f), new Vector3(7977f, 55.055992126465f, 8781f)));
            Jump.Add(new JumpSpot(new Vector3(7396.7861328125f, 55.606025695801f, 9239.8271484375f), new Vector3(7394.984375f, 55.606025695801f, 9063.814453125f)));
            Jump.Add(new JumpSpot(new Vector3(5525.7817382813f, 55.085205078125f, 9987.673828125f), new Vector3(5384.943359375f, 55.085205078125f, 10007.40234375f)));
            Jump.Add(new JumpSpot(new Vector3(4422.02734375f, -62.942153930664f, 10580.529296875f), new Vector3(4388.0249023438f, -62.942153930664f, 10663.412109375f)));
            Jump.Add(new JumpSpot(new Vector3(6607.0966796875f, 54.634994506836f, 10630.306640625f), new Vector3(6628.1147460938f, 54.634994506836f, 10463.506835938f)));
            Jump.Add(new JumpSpot(new Vector3(7374.482421875f, 53.263687133789f, 4671.4790039063f), new Vector3(7364.9262695313f, 53.263687133789f, 4513.6796875f)));
            Jump.Add(new JumpSpot(new Vector3(6405.2646484375f, 52.171257019043f, 3655.4738769531f), new Vector3(6313.0185546875f, 52.171257019043f, 3562.0717773438f)));
            Jump.Add(new JumpSpot(new Vector3(5576.25390625f, 51.753463745117f, 4176.833984375f), new Vector3(5500.7392578125f, 51.753463745117f, 4270.5112304688f)));
            Jump.Add(new JumpSpot(new Vector3(10226.828125f, 66.05110168457f, 8866.791015625f), new Vector3(10175.86328125f, 66.05110168457f, 8956.7744140625f)));
            Jump.Add(new JumpSpot(new Vector3(9750.0341796875f, 52.114059448242f, 9440.05078125f), new Vector3(9845f, 52.114059448242f, 9313f)));
            Jump.Add(new JumpSpot(new Vector3(9029.5546875f, 54.20191192627f, 9765.8134765625f), new Vector3(8947.9169921875f, 54.20191192627f, 9851.1064453125f)));
            Jump.Add(new JumpSpot(new Vector3(7642.74609375f, 53.922214508057f, 10822.842773438f), new Vector3(7745f, 53.922214508057f, 10897f)));
            Jump.Add(new JumpSpot(new Vector3(8236.056640625f, 49.935394287109f, 11255.161132813f), new Vector3(8112.7490234375f, 49.935394287109f, 11145.7734375f)));
            Jump.Add(new JumpSpot(new Vector3(1752.2419433594f, 54.923698425293f, 8448.9619140625f), new Vector3(1621.3428955078f, 54.923698425293f, 8437.0380859375f)));
        }

    }
}