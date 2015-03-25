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
        public static bool casted = false;
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
            Console.WriteLine("TRUStInJumps LOADED");
            Config = new Menu("TRUStInJumps", "TRUStInJumps", true);
            Config.AddSubMenu(new Menu("Config", "Config"));
            Config.SubMenu("Config").AddItem(new MenuItem("wardjump", "Jump key").SetValue(new KeyBind(32, KeyBindType.Press)));
            Config.SubMenu("Config").AddItem(new MenuItem("drawrange", "Draw jump range").SetValue(new Circle(true, System.Drawing.Color.White)));
            Config.AddToMainMenu();
            Game.OnUpdate += OnGameUpdate;
            Drawing.OnDraw += JumperDraw;
        }

        private static void JumperDraw(EventArgs args)
        {

            try
            {

                var menuItem = Config.SubMenu("Config").Item("drawrange").GetValue<Circle>();
                Console.WriteLine(menuItem.Color);
                if (menuItem.Active)
                {
                    
                    if (Player.ChampionName == "JarvanIV")
                    {
                        Render.Circle.DrawCircle(ObjectManager.Player.Position, 625, menuItem.Color);
                    }
                    else
                    {
                        Render.Circle.DrawCircle(ObjectManager.Player.Position, 625, menuItem.Color);

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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
                    Obj_AI_Hero[] nearsthero = { null };
                    foreach (var obj in ObjectManager.Get<Obj_AI_Minion>().Where(obj => (Player.ChampionName == "Jax" || Player.ChampionName == "Katarina") && obj.Position.Distance(posforward) <= 200).Where(obj => nearstobj[0] == null || nearstobj[0].Position.Distance(posforward) > obj.Position.Distance(posforward)))
                    {
                        nearstobj[0] = obj;
                    }
                    foreach (var obj in ObjectManager.Get<Obj_AI_Hero>().Where(obj => (Player.ChampionName == "Jax" || Player.ChampionName == "Katarina") && obj.Position.Distance(posforward) <= 200).Where(obj => nearsthero[0] == null || nearsthero[0].Position.Distance(posforward) > obj.Position.Distance(posforward)))
                    {
                        nearsthero[0] = obj;
                    }
                    if (nearstobj[0] != null)
                    {
                        JumpSpell.Cast(nearstobj[0]);
                        return;
                    }
                    if (nearsthero[0] != null)
                    {
                        JumpSpell.Cast(nearsthero[0]);
                        return;
                    }
                    else if (Items.GetWardSlot() != null)
                    {
                        Player.Spellbook.CastSpell(Items.GetWardSlot().SpellSlot, posforward);
                        casted = true;
                        lastward = Items.GetWardSlot();
                        oldstacks = lastward.Stacks;
                    }
                }
                    
            }
            else if (JumpSpell.IsReady() && Jslot.IsReady() && (ObjectManager.Player.Spellbook.Spells.First(s => s.Slot == SpellSlot.Q).ManaCost + ObjectManager.Player.Spellbook.Spells.First(s => s.Slot == SpellSlot.E).ManaCost) < Player.Mana)
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
