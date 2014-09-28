#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
#endregion

namespace ProJumper
{
    public class Program
    {

        public static Obj_AI_Hero Player;
        public static Menu Config;
        public static SpellSlot JumpSlot = SpellSlot.Unknown;
        public static bool casted;
        public static Spell JumpSpell;
        public static Vector3 posforward;
        public static Spell Jslot;
        public static InventorySlot lastward;
        public static int oldstacks;
        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }


        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            Console.WriteLine(Player.BaseSkinName);
            if (Player.ChampionName == "LeeSin")
            {
                JumpSlot = SpellSlot.W;
            }
            else if (Player.ChampionName == "Jax")
            {
                JumpSlot = SpellSlot.Q;
            }
            else if (Player.ChampionName == "Katarina")
            {
                JumpSlot = SpellSlot.E;
            }
            else if (Player.ChampionName == "JarvanIV")
            {
                JumpSlot = SpellSlot.Q;
                Jslot = new Spell(SpellSlot.E, 625);
            }
            if (JumpSlot == SpellSlot.Unknown)
            {
                return;
            }
            else
            {
                JumpSpell = new Spell(JumpSlot, 625);
            }
            Game.OnGameUpdate += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
            Console.WriteLine("ProJumper LOADED");
            
            Config = new Menu("ProJumper", "ProJumper", true);
            Config.AddSubMenu(new Menu("Config", "Config"));
            Config.SubMenu("Config").AddItem(new MenuItem("wardjump", "Jump key").SetValue(new KeyBind(32, KeyBindType.Press)));
            Config.SubMenu("Config").AddItem(new MenuItem("drawrange", "Draw jump range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(100, 255, 0, 255))));
            Config.AddToMainMenu();
        }

        private static void OnDraw(EventArgs args)
        {
            var menuItem = Config.Item("drawrange").GetValue<Circle>();
            if (Config.Item("drawrange").GetValue<Circle>().Active)
            {
                if (Player.ChampionName == "JarvanIV")
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, 800, menuItem.Color);
                }
                else
                {
                    Utility.DrawCircle(ObjectManager.Player.Position, 625, menuItem.Color);
                }
            }
        }
        private static void OnGameUpdate(EventArgs args)
        {
            if (casted)
            {
                if (lastward.Id == 0 || lastward.Stacks != oldstacks)
                {
                    casted = false;
                }
            }
            if (!Config.Item("wardjump").GetValue<KeyBind>().Active)
            {
                return;
            }

            if (JumpSpell.IsReady() && Player.ChampionName != "JarvanIV" && (Player.ChampionName != "LeeSin" || ObjectManager.Player.Spellbook.GetSpell(JumpSlot).Name == "BlindMonkWOne"))
            {
                var mouse = Game.CursorPos;
                var newpos = mouse - Player.Position;
                newpos.Normalize();
                Vector3 mousepos = Player.Position + (newpos * 600);
                if (ObjectManager.Player.Distance(Game.CursorPos) < 600)
                {
                    posforward = Game.CursorPos;
                }
                else
                {
                    posforward = mousepos;
                }
                if (ObjectManager.Player.Distance(posforward) < 600)
                {
                    Obj_AI_Minion[] nearstobj = { null };
                    foreach (var obj in ObjectManager.Get<Obj_AI_Minion>().Where(obj => (Player.ChampionName == "Jax" || Player.ChampionName == "Katarina" || obj.IsAlly) && obj.Position.Distance(posforward) <= 200).Where(obj => nearstobj[0] == null || nearstobj[0].Position.Distance(posforward) > obj.Position.Distance(posforward)))
                    {
                        nearstobj[0] = obj;
                    }
                    if (nearstobj[0] != null)
                    {
                        JumpSpell.Cast(nearstobj[0]);
                    }
                    else if (Items.GetWardSlot() != null)
                    {
                        Items.GetWardSlot().UseItem(posforward);
                        casted = true;
                        lastward = Items.GetWardSlot();
                        oldstacks = lastward.Stacks;
                    }
                }
                    
            }
            else if (JumpSpell.IsReady() && Jslot.IsReady() && (Player.Spellbook.GetManaCost(SpellSlot.Q)+Player.Spellbook.GetManaCost(SpellSlot.E))<Player.Mana)
                {
                var mouse = Game.CursorPos;
                var newpos = mouse - Player.Position;
                newpos.Normalize();
                Vector3 mousepos = Player.Position + (newpos * 770);
                    if (ObjectManager.Player.Distance(Game.CursorPos)<770)
                    {
                        Jslot.Cast(Game.CursorPos);
                        JumpSpell.Cast(Game.CursorPos);
                    }
                    else
                    {
                        Jslot.Cast(mousepos);
                        JumpSpell.Cast(mousepos);
                    }
                }
                
            }
        }

}
