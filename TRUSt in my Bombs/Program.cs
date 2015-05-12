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
        public static int LastbombTime;

        public static GameObject wBomb;

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
                    Render.Circle.DrawCircle(convertcoords, 80, colordraw.Color);
                    
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
            Game.OnUpdate += Game_OnGameUpdate;
            GameObject.OnCreate += OnCreateObj;
            GameObject.OnDelete += OnDeleteObj;
            Interrupter2.OnInterruptableTarget += ZOnPosibleToInterrupt;


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
                Config.SubMenu("satchel").AddItem(new MenuItem("satchelJumpMouseOver", "Jump To mouse Key").SetValue(new KeyBind("Y".ToCharArray()[0], KeyBindType.Press)));

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

            if (Config.Item("satchelJumpMouseOver").GetValue<KeyBind>().Active)
            {
                Console.WriteLine("STARTING JUMP TO MOUSE");
                JumpProxMouse();
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
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, Q2.Range, qValue.Color);
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, rValue.Color, 5, true);
                }
                else if (W.IsReady())
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, qValue.Color);
                }
                else if (E.IsReady())
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, qValue.Color);
                }
                else if (R.IsReady())
                {
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, qValue.Color);
                    Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, rValue.Color, 5, true);
                }
                return;
            }
            if (qValue.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q2.Range, qValue.Color);
            }

            if (qValue2.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, qValue.Color);
            }

            if (wValue.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, wValue.Color);
            }


            if (eValue.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, E.Range, eValue.Color);
            }


            if (rValue.Active)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, rValue.Color);
                Render.Circle.DrawCircle(ObjectManager.Player.Position, R.Range, rValue.Color,5,true);
            }


        }

        private static void AutoUlt()
        {
            var qTarget = TargetSelector.GetTarget(Q2.Range, TargetSelector.DamageType.Magical);
            var rTarget = TargetSelector.GetTarget(R.Range, TargetSelector.DamageType.Magical);
            var rTargetsamount = Config.Item("enemysforR").GetValue<Slider>().Value;
            if (R.IsReady() && (qTarget != null || rTarget != null))
            {
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
                            var Qdamage = Player.GetSpellDamage(minion, SpellSlot.Q) * 0.75;

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

        private static void ZOnPosibleToInterrupt(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!Config.Item("interrupt").GetValue<bool>()) return;

            if (Player.Distance(unit.ServerPosition) < W.Range)
            {
                W.Cast(unit);
            }
        }

        private static void Ks()
        {
            var rTarget = TargetSelector.GetTarget(Config.Item("ksRRange").GetValue<Slider>().Value, TargetSelector.DamageType.Magical);
            var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var qTarget2 = TargetSelector.GetTarget(Q2.Range, TargetSelector.DamageType.Magical);
            var eTarget = TargetSelector.GetTarget(E.Range + W.Width * 0.5f, TargetSelector.DamageType.Magical);
            var ksAll = Config.Item("ksAll").GetValue<bool>();
            if (Q.IsReady() && qTarget2 != null && qTarget2.Health < Player.GetSpellDamage(qTarget2, SpellSlot.Q) && ksAll)
            {
                Q2.Cast(qTarget2);
            }
            else if (R.IsReady() && rTarget != null && rTarget.Health < Player.GetSpellDamage(rTarget, SpellSlot.R))
            {
                R.Cast(rTarget);
            }
            else if (Q.IsReady() && qTarget != null && qTarget.Health < (Player.GetSpellDamage(qTarget, SpellSlot.Q) + Player.GetSpellDamage(rTarget, SpellSlot.R)) && ksAll)
            {
                UseSpells(true, false, true);
            }
        }

        private static void JumpProxMouse()
        {
            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "ZiggsW" && Environment.TickCount > LastbombTime)
            {
                ObjectManager.Player.Spellbook.CastSpell(SpellSlot.W, Player.Position);
                LastbombTime = Environment.TickCount + 500;
            }
            else if (wBomb != null && ObjectManager.Player.Distance(wBomb.Position) > 100 && ObjectManager.Player.Distance(wBomb.Position) < 300)
            {
                ObjectManager.Player.Spellbook.CastSpell(SpellSlot.W);
                Console.WriteLine("Jump on bomb");
            }

        }

        public static void OnCreateObj(GameObject obj, EventArgs args)
        {
            if (obj.Name == "ZiggsW_mis_ground.troy")
            {
                wBomb = obj;
                Console.WriteLine("Bomb created");
            }
        }

        public static void OnDeleteObj(GameObject obj, EventArgs args)
        {
            if (obj.Name == "ZiggsW_mis_ground.troy")
            {
                wBomb = null;
                Console.WriteLine("Bomb deleted");
            }
        }


        private static void JumpProx()
        {
            if (FindNearestJumpSpot() == null)
            {
                return;
            }
            JumpSpot nearestspot = FindNearestJumpSpot();
            Console.WriteLine(nearestspot);
            Vector3 nearestJump = new Vector3(nearestspot.Jumppos.X, nearestspot.Jumppos.Z, nearestspot.Jumppos.Y);
            Vector3 moveposition = new Vector3(nearestspot.MovePosition.X, nearestspot.MovePosition.Z, nearestspot.MovePosition.Y);
            Console.WriteLine(nearestJump.ToString());
            if (ObjectManager.Player.Spellbook.GetSpell(SpellSlot.W).Name == "ZiggsW" && Environment.TickCount > LastbombTime)
            {
                ObjectManager.Player.Spellbook.CastSpell(SpellSlot.W, nearestJump);
                LastbombTime = Environment.TickCount + 500;
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
            var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            if (IgniteSlot != SpellSlot.Unknown &&
                ObjectManager.Player.Spellbook.CanUseSpell(IgniteSlot) == SpellState.Ready &&
                ObjectManager.Player.Distance(qTarget.ServerPosition) < 600 &&
                Player.GetSummonerSpellDamage(qTarget, Damage.SummonerSpell.Ignite) > qTarget.Health)
            {
                ObjectManager.Player.Spellbook.CastSpell(IgniteSlot, qTarget);
            }
        }

        private static void Harass()
        {
            UseSpells(Config.Item("useBHarass").GetValue<bool>(), Config.Item("useEHarass").GetValue<bool>(), false);
        }


        private static void UseSpells(bool useQ, bool useE, bool useR)
        {
            var qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Magical);
            var qTarget2 = TargetSelector.GetTarget(Q2.Range, TargetSelector.DamageType.Magical);
            var eTarget = TargetSelector.GetTarget(E.Range + W.Width * 0.5f, TargetSelector.DamageType.Magical);
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
            Jump.Add(new JumpSpot(new Vector3(5512.908f, 6039.813f, 51.31262f), new Vector3(5345.714f, 6148.591f, 50.80725f)));
            Jump.Add(new JumpSpot(new Vector3(3831.914f, 52.46252f, 6549.028f), new Vector3(3830.042f, 52.46143f, 6652.219f)));
            Jump.Add(new JumpSpot(new Vector3(3308.438f, 52.0874f, 6435.311f), new Vector3(3209.204f, 51.93689f, 6392.481f)));
            Jump.Add(new JumpSpot(new Vector3(3732.878f, 50.80249f, 7201.029f), new Vector3(3731.923f, 51.41162f, 7312.149f)));
            Jump.Add(new JumpSpot(new Vector3(4759.604f, 52.5658f, 7822.059f), new Vector3(4640.189f, 52.51257f, 7804.859f)));
            Jump.Add(new JumpSpot(new Vector3(3885.163f, 51.8927f, 7825.341f), new Vector3(4015.652f, 51.33838f, 7829.216f)));
            Jump.Add(new JumpSpot(new Vector3(2216.856f, 51.77698f, 8393.946f), new Vector3(2123.749f, 51.7771f, 8377.014f)));
            Jump.Add(new JumpSpot(new Vector3(2950.097f, 57.04395f, 5879.647f), new Vector3(3074.498f, 57.04492f, 5903.677f)));
            Jump.Add(new JumpSpot(new Vector3(5234.063f, -71.24072f, 10726.57f), new Vector3(5320.236f, -71.24048f, 10757.75f)));
            Jump.Add(new JumpSpot(new Vector3(4881.556f, -71.24072f, 10782.05f), new Vector3(4869.955f, -71.24084f, 10860.36f)));
            Jump.Add(new JumpSpot(new Vector3(6110.383f, 55.34912f, 10585.62f), new Vector3(6216.023f, 54.39734f, 10602.18f)));
            Jump.Add(new JumpSpot(new Vector3(6552.886f, 54.21692f, 11547.54f), new Vector3(6535.944f, 53.92749f, 11637.58f)));
            Jump.Add(new JumpSpot(new Vector3(6458.442f, 56.47693f, 12112.99f), new Vector3(6467.904f, 56.47681f, 12005.04f)));
            Jump.Add(new JumpSpot(new Vector3(7918.915f, 53.72021f, 4151.359f), new Vector3(7921.666f, 53.71948f, 4239.165f)));
            Jump.Add(new JumpSpot(new Vector3(8402.849f, 51.10583f, 2753.006f), new Vector3(8388.755f, 51.13f, 2842.286f)));
            Jump.Add(new JumpSpot(new Vector3(8982.477f, 53.33093f, 4001.319f), new Vector3(9085.941f, 53.7821f, 4030.224f)));
            Jump.Add(new JumpSpot(new Vector3(8944.968f, 52.15833f, 4721.304f), new Vector3(9033.252f, 52.0719f, 4675.623f)));
            Jump.Add(new JumpSpot(new Vector3(9727.997f, -71.24084f, 4040.509f), new Vector3(9685.505f, -70.97998f, 3954.977f)));
            Jump.Add(new JumpSpot(new Vector3(9468.955f, -71.24072f, 4501.78f), new Vector3(9367.121f, -71.2406f, 4542.082f)));
            Jump.Add(new JumpSpot(new Vector3(9042.721f, -71.24048f, 6411.511f), new Vector3(9093.207f, -71.24072f, 6477.897f)));
            Jump.Add(new JumpSpot(new Vector3(9477.367f, 52.48083f, 7107.661f), new Vector3(9431.294f, 52.53564f, 7017.614f)));
            Jump.Add(new JumpSpot(new Vector3(7741.808f, 52.40625f, 5898.093f), new Vector3(7815.08f, 51.64905f, 5953.273f)));
            Jump.Add(new JumpSpot(new Vector3(8193.631f, -71.24072f, 6280.748f), new Vector3(8126.122f, -71.24072f, 6197.067f)));
            Jump.Add(new JumpSpot(new Vector3(5657.813f, 51.65442f, 7702.506f), new Vector3(5719.472f, 51.65442f, 7758.776f)));
            Jump.Add(new JumpSpot(new Vector3(6061.12f, -68.72888f, 8316.865f), new Vector3(6007.151f, -68.7594f, 8257.824f)));
            Jump.Add(new JumpSpot(new Vector3(6740.511f, -71.24084f, 8535.017f), new Vector3(6803.211f, -71.24072f, 8591.198f)));
            Jump.Add(new JumpSpot(new Vector3(7786.991f, 52.44653f, 9320.914f), new Vector3(7787.272f, 52.50171f, 9227.487f)));
            Jump.Add(new JumpSpot(new Vector3(7554.679f, 52.87244f, 8830.464f), new Vector3(7598.848f, 52.87256f, 8890.213f)));
            Jump.Add(new JumpSpot(new Vector3(7144.915f, 52.84851f, 10033.38f), new Vector3(7143.229f, 52.46057f, 10126.55f)));
            Jump.Add(new JumpSpot(new Vector3(8056.637f, 50.70618f, 11011.76f), new Vector3(8119.013f, 50.7179f, 11056.78f)));
            Jump.Add(new JumpSpot(new Vector3(10693.3f, 51.87354f, 7608.199f), new Vector3(10744.99f, 52.01514f, 7506.439f)));
            Jump.Add(new JumpSpot(new Vector3(10693.3f, 51.87354f, 7608.199f), new Vector3(10744.99f, 52.01514f, 7506.439f)));
            Jump.Add(new JumpSpot(new Vector3(10909.05f, 62.68127f, 8249.537f), new Vector3(10916.33f, 62.87183f, 8143.787f)));
            Jump.Add(new JumpSpot(new Vector3(11499.76f, 59.40002f, 8549.204f), new Vector3(11576.31f, 60.37341f, 8582.757f)));
            Jump.Add(new JumpSpot(new Vector3(11951.33f, 50.3103f, 8876.381f), new Vector3(11879.36f, 50.43408f, 8810.297f)));
            Jump.Add(new JumpSpot(new Vector3(11011.02f, 51.72339f, 7057.407f), new Vector3(10997.64f, 51.72363f, 7140.384f)));
            Jump.Add(new JumpSpot(new Vector3(10868.45f, 51.72302f, 7031.942f), new Vector3(10781.43f, 51.72278f, 7060.89f)));
            Jump.Add(new JumpSpot(new Vector3(10217.83f, 51.97852f, 6823.584f), new Vector3(10308.14f, 51.97656f, 6835.764f)));
            Jump.Add(new JumpSpot(new Vector3(12686.56f, 51.70142f, 6388.303f), new Vector3(12772.3f, 51.677f, 6391.575f)));
            Jump.Add(new JumpSpot(new Vector3(11528.67f, -71.2406f, 4623.761f), new Vector3(11603.53f, -71.2406f, 4699.122f)));
            Jump.Add(new JumpSpot(new Vector3(12003.49f, 52.35034f, 4974.583f), new Vector3(11934.33f, 51.9657f, 4872.047f)));
            Jump.Add(new JumpSpot(new Vector3(6762.071f, 48.5238f, 3944.274f), new Vector3(6664.041f, 48.52441f, 3997.398f)));
        }
         
    }
}