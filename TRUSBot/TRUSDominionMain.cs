using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using System.IO;
using SharpDX;
//specially broke this assembly, so noobs will not ask what it doing
namespace TRUSDominion
{
    internal class TRUSDominionMain
    {
        public TRUSDominionMain()
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        class PointData
        {
            public string Name { get; set; }
            public Vector3 Position { get; set; }
            public string State { get; set; }
            public GameObject Unit { get; set; }


            public PointData(string name, Vector3 position)
            {
                this.Name = name;
                this.Position = position;
                this.State = "Unknown";
                this.Unit = Unit;
            }
        }


        static List<PointData> PointData2 = new List<PointData>()
        {
            new PointData("BotRight", new Vector3(9533f, 2519f, -142f)),
            new PointData("BotLeft", new Vector3(4342f, 2515f, -145f)),
            new PointData("Top", new Vector3(6941f, 10981f, -147f)),
            new PointData("MiddleLeft", new Vector3(2519f, 7781f, -141f)),
            new PointData("MiddleRight", new Vector3(11401f, 7775f, -145f)),
        };


        public class Enemies
        {
            public static int BotRight;
            public static int BotLeft;
            public static int Top;
            public static int MiddleLeft;
            public static int MiddleRight;
        }

        public class Team
        {
            public static string BotRight;
            public static string BotLeft;
            public static string Top;
            public static string MiddleLeft;
            public static string MiddleRight;
        }

        public class previouscoords
        {
            public static float X;
            public static float X2;
            public static float Y2;
            public static float Y;
        }

        public class Allies
        {
            public static int BotRight;
            public static int BotLeft;
            public static int Top;
            public static int MiddleLeft;
            public static int MiddleRight;

        }
        public class Distance
        {
            public static float BotRight;
            public static float BotLeft;
            public static float Top;
            public static float MiddleLeft;
            public static float MiddleRight;
            public static float Start;
            public static float End;

        }
        private static Obj_AI_Hero Player;
        private static double lasthelp;
        private static double waittime10;
        private static bool champ = false;
        private static double waittime1;
        private static GameObject attackedhero;
        private static float buyitemdelay = 0;
        public static GameObject cappedpoint = null;
        public static GameObject shop;
        public static string currentstatus = "";
        public static double retreattimer = 0;


        private static int CountHeroes(bool myteam, Vector3 position, int distance)
        {
            int counter = 0;
            foreach (Obj_AI_Hero obj in ObjectManager.Get<Obj_AI_Hero>())
            {
                if (((obj.Team != Player.Team) || myteam) && (Vector3.Distance(obj.ServerPosition, position) <= distance))
                {
                    counter++;
                }
            }
            return counter;
        }
        public static bool HasSlot()
        {
            var itemscount = 0;
          foreach (var slot in ObjectManager.Player.InventoryItems)
          {
              if (slot.Id != 0)
              {
                  itemscount++;
              }
          }
          if (itemscount < 6)
          {
              Console.WriteLine("Have free slot");
              return true;
              
          }
          else
          {
              Console.WriteLine("No free slot");
              return false;
          }
        }

        public static void InventoryCheck()
        {
            foreach (ItemsList Item in ChampionsItems.ItemsToBuy)
            {
                if (Items.HasItem(Item.ItemID))
                {

                    foreach (BuildsList ItemsToBuyz in ChampionsItems.Builds)
                    {
                        if (ItemsToBuyz.ItemName == Item.ItemName)
                        {
                            ChampionsItems.Builds.Remove(ItemsToBuyz);
                            //Console.WriteLine("Full Item bought : " + Item.ItemName);
                        }
                    }
                }

            }
        }

        public static void HowToBuy(ItemsList Item)
        {
            //Console.WriteLine("Buyting logic started");
            if (!Items.HasItem(Item.ItemID) && Item.Part1 != 0  && !Items.HasItem(Item.Part1) && HasSlot())
            {
                BuyItem(Item.Part1);
                Console.WriteLine("Buyting  part1");
                return;
            }
            else if (!Items.HasItem(Item.ItemID) && Items.HasItem(Item.Part1) && Item.Part2 != 0 && !Items.HasItem(Item.Part2) && HasSlot())
            {
                BuyItem(Item.Part2);
                Console.WriteLine("Buyting  part2");
                    return;
            }
            else if (!Items.HasItem(Item.ItemID) && Item.ItemCost < Player.Gold)
            {
                BuyItem(Item.ItemID);
                //Console.WriteLine("Buyting  full item");
                return;
            }
        }

        public static void BuyItemsTick()
        {
            
            InventoryCheck();
            if ((buyitemdelay < Environment.TickCount) && (ObjectManager.Player.Distance(shop.Position) <= 1250f || ObjectManager.Player.IsDead))
            {
                
                buyitemdelay = Environment.TickCount + 500;
                
                var count = 0;
                foreach (BuildsList ItemsToBuyz in ChampionsItems.Builds)
                {
                    //Console.WriteLine(ItemsToBuyz.ItemName);
                    foreach (ItemsList Item in ChampionsItems.ItemsToBuy)
                    {
                        if (count == 0 && ItemsToBuyz.ItemName == Item.ItemName)
                        {
                            HowToBuy(Item);
                           // Console.WriteLine("TRYING TO BUY");
                        }
                       
                    }

                    count++;
                }
            }

            
        }

        public static bool RuneCheck()
        {

            if (Player.Health / Player.MaxHealth < 0.5 && CountHeroes(false, Player.Position, 2000) == 0)
            {
                foreach (GameObject obj in ObjectManager.Get<GameObject>())
                {
                    if (obj.Name == "odin_heal_rune.troy" && ObjectManager.Player.Distance(obj.Position) < 2000)
                    {
                        ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, new Vector3(obj.Position.X, obj.Position.Y, obj.Position.Z));
                        StatusUpdate("NO enemies around, eating rune");
                        return true;
                    }
                }
            }
            return false;
        }
        public static void StatusUpdate(string status)
        {
            currentstatus = status;
        }
        public static void MoveLogicOnTick()
        {
          
            assignpoints();
            BuyItemsTick();
            var Selector2000 = SimpleTs.GetTarget(2000f, SimpleTs.DamageType.True);
            //// On game start move to mid-side point
            if (Game.Time < 60)
            {
                if (ObjectManager.Player.Team == GameObjectTeam.Order)
                    ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, new Vector3(647f, 4427f, 0));
                else
                    ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, new Vector3(13192f, 4388f, 0));
                return;
            }
            
            if (Player.IsDead)
            {
                Console.WriteLine("im dead");
                return;
            }
            ///check for rune
            if (RuneCheck())
            {
                return;
            }
            

            #region points variables
            foreach (PointData pointname in PointData2)
            {
                if (pointname.Name == "Top")
                {
                    Enemies.Top = CountHeroes(false, pointname.Position, 2000);
                    Allies.Top = CountHeroes(true, pointname.Position, 2000);
                    Distance.Top = ObjectManager.Player.Distance(pointname.Position);
                    if (Player.Team == GameObjectTeam.Chaos && pointname.State == "Red")
                    {
                        Team.Top = "Green";
                    }
                    else if (Player.Team == GameObjectTeam.Chaos && pointname.State == "Green")
                    {
                        Team.Top = "Red";
                    }
                    else
                    {
                        Team.Top = pointname.State;
                    }

                }
                else if (pointname.Name == "MiddleLeft")
                {
                    Enemies.MiddleLeft = CountHeroes(false, pointname.Position, 2000);
                    Allies.MiddleLeft = CountHeroes(true, pointname.Position, 2000);
                    Distance.MiddleLeft = ObjectManager.Player.Distance(pointname.Position);
                    
                    if (Player.Team == GameObjectTeam.Chaos && pointname.State == "Red")
                    {
                        Team.MiddleLeft = "Green";
                    }
                    else if (Player.Team == GameObjectTeam.Chaos && pointname.State == "Green")
                    {
                        Team.MiddleLeft = "Red";
                    }
                    else
                    {
                        Team.MiddleLeft = pointname.State;
                    }
                }
                else if (pointname.Name == "MiddleRight")
                {
                    Enemies.MiddleRight = CountHeroes(false, pointname.Position, 2000);
                    Allies.MiddleRight = CountHeroes(true, pointname.Position, 2000);
                    Distance.MiddleRight = ObjectManager.Player.Distance(pointname.Position);
                    if (Player.Team == GameObjectTeam.Chaos && pointname.State == "Red")
                    {
                        Team.MiddleRight = "Green";
                    }
                    else if (Player.Team == GameObjectTeam.Chaos && pointname.State == "Green")
                    {
                        Team.MiddleRight = "Red";
                    }
                    else
                    {
                        Team.MiddleRight = pointname.State;
                    }
                }
                else if (pointname.Name == "BotRight")
                {
                    Enemies.BotRight = CountHeroes(false, pointname.Position, 2000);
                    Allies.BotRight = CountHeroes(true, pointname.Position, 2000);
                    Distance.BotRight = ObjectManager.Player.Distance(pointname.Position);

                    if (Player.Team == GameObjectTeam.Chaos && pointname.State == "Red")
                    {
                        Team.BotRight = "Green";
                    }
                    else if (Player.Team == GameObjectTeam.Chaos && pointname.State == "Green")
                    {
                        Team.BotRight = "Red";
                    }
                    else
                    {
                        Team.BotRight = pointname.State;
                    }

                }
                else if (pointname.Name == "BotLeft")
                {
                    Enemies.BotLeft = CountHeroes(false, pointname.Position, 2000);
                    Allies.BotLeft = CountHeroes(true, pointname.Position, 2000);
                    Distance.BotLeft = ObjectManager.Player.Distance(pointname.Position);
                    if (Player.Team == GameObjectTeam.Chaos && pointname.State == "Red")
                    {
                        Team.BotLeft = "Green";
                    }
                    else if (Player.Team == GameObjectTeam.Chaos && pointname.State == "Green")
                    {
                        Team.BotLeft = "Red";
                    }
                    else
                    {
                        Team.BotLeft = pointname.State;
                    }

                }
                else if (pointname.Name == "Start")
                {
                    Distance.Start = ObjectManager.Player.Distance(pointname.Position);
                }
                else if (pointname.Name == "End")
                {
                    Distance.End = ObjectManager.Player.Distance(pointname.Position);
                }
            }
#endregion
            
            if (Selector2000 != null && (ObjectManager.Player.Distance(Selector2000) > 1000) && (Distance.Start > 1500) ||
                (Selector2000 != null && Distance.End < 3000) && retreattimer < Environment.TickCount)
            {
                StatusUpdate("Bad idea to continue chasing, returning");
                retreattimer = Environment.TickCount+5000;
                return;
            }

            if (Enemies.Top > Allies.Top + 1 && (Selector2000 == null || ObjectManager.Player.Distance(Selector2000.Position)>1000))
            {
                StatusUpdate("Too much enemys near, changing position");
                //return?
            }

            if (Selector2000 == null || ObjectManager.Player.Distance(Selector2000.Position)>1000)
            {
               
                // checks for recall
                if (Player.Health / Player.MaxHealth < 0.5 && Selector2000 == null && !MinionAround())
                {
                    if ((Distance.Top < 1000 && Team.Top == "Green") || 
                        (Distance.BotLeft < 1000 && Team.BotLeft == "Green") || 
                        (Distance.BotRight < 1000 && Team.BotRight == "Green") ||
                        (Distance.MiddleLeft < 1000 && Team.MiddleLeft == "Green") ||
                        (Distance.MiddleRight < 1000 && Team.MiddleRight == "Green"))
                    {
                        StatusUpdate("Recall cause all fine and low hp");
                        Player.Spellbook.CastSpell(SpellSlot.Recall);
                        return;
                    }
                }

                //---capture midleft if it near and enemy/uncaped
                if (Team.MiddleLeft != "Green" && Player.Team == GameObjectTeam.Order)
                {
                    foreach (PointData point in PointData2.Where(point => point.Name == "MiddleLeft"))
                    {
                        StatusUpdate("Capture midleft : " + Team.MiddleLeft.ToString());
                    Capture(point.Unit);
                        return;
                    }
                }
                else if (Team.MiddleRight != "Green" && Player.Team == GameObjectTeam.Chaos)
                {
                    foreach (PointData point in PointData2.Where(point => point.Name == "MiddleRight"))
                    {
                    Capture(point.Unit);
                    StatusUpdate("Capture midright : " + Team.MiddleRight.ToString());
                        return;
                    }
                }
                else if (Team.MiddleRight != "Green" && Player.Team == GameObjectTeam.Order && Distance.MiddleRight < 1000)
                {
                    foreach (PointData point in PointData2.Where(point => point.Name == "MiddleRight"))
                    {
                    Capture(point.Unit);
                    StatusUpdate("Capture midright : " + Team.MiddleRight.ToString());
                        return;
                    }
                }
                else if (Team.MiddleLeft != "Green" && Player.Team == GameObjectTeam.Chaos && Distance.MiddleLeft < 1000)
                {
                    foreach (PointData point in PointData2.Where(point => point.Name == "MiddleLeft"))
                    {
                    Capture(point.Unit);
                    StatusUpdate("Capture midright : " + Team.MiddleRight.ToString());
                        return;
                    }
                }
                else if (Team.Top != "Green" && ((Player.Team == GameObjectTeam.Chaos && Team.MiddleRight == "Green") || Player.Team == GameObjectTeam.Order && Team.MiddleLeft == "Green"))
                {
                    foreach (PointData point in PointData2.Where(point => point.Name == "Top"))
                    {
                    Capture(point.Unit);
                    StatusUpdate("Capturing top");
                        return;
                    }
                }
                else if (Team.Top == "Green" && ((Player.Team == GameObjectTeam.Chaos && Team.MiddleRight == "Green" && Team.BotRight != "Green") || Player.Team == GameObjectTeam.Order && Team.MiddleLeft == "Green" && Team.BotLeft != "Green"))
                {
                    if (Player.Team == GameObjectTeam.Chaos)
                    {
                    foreach (PointData point in PointData2.Where(point => point.Name == "BotRight"))
                    {
                    Capture(point.Unit);
                    StatusUpdate("Capturing BotRight");
                        return;
                    }
                    }
                    else if (Player.Team == GameObjectTeam.Order)
                        {
                    foreach (PointData point in PointData2.Where(point => point.Name == "BotLeft"))
                    {
                    Capture(point.Unit);
                    StatusUpdate("Capturing BotLeft");
                        return;
                    }
                    }
                }
                else if (Team.BotRight != "Green" && Player.Team == GameObjectTeam.Order && Distance.BotRight < 1000)
                {
                    foreach (PointData point in PointData2.Where(point => point.Name == "BotRight"))
                    {
                    Capture(point.Unit);
                    StatusUpdate("Capturing BotRight");
                        return;
                    }
                }
                else if (Team.BotLeft != "Green" && Player.Team == GameObjectTeam.Order && Distance.BotLeft < 1000)
                {
                    foreach (PointData point in PointData2.Where(point => point.Name == "BotLeft"))
                    {
                    Capture(point.Unit);
                    StatusUpdate("Capturing BotLeft");
                        return;
                    }
                }

                if (Enemies.Top == 0 && cappedpoint != null)
                {
                    Player.IssueOrder(GameObjectOrder.MoveTo, cappedpoint.Position);
                    StatusUpdate("MOVE TO POINT WHICH CAPTURED");
                    cappedpoint = null;
                    return;
                }

                if (attackedhero != null && (Environment.TickCount > lasthelp + 500))
                {
                    Player.IssueOrder(GameObjectOrder.MoveTo, attackedhero.Position);
                    StatusUpdate("MOVE TO ALLIED WHICH ATTACKED");
                    lasthelp = Environment.TickCount;
                    attackedhero = null;
                    return;
                }
                if (Environment.TickCount > waittime10 + 10000 && previouscoords.X == Player.Position.X && previouscoords.Y == Player.Position.Y)
                {
                    waittime10 = Environment.TickCount;
                    StatusUpdate("10s recall");
                    Player.Spellbook.CastSpell(SpellSlot.Recall);
                    previouscoords.X = Player.Position.X;
                    previouscoords.Y = Player.Position.Y;
                }
                if (Environment.TickCount > waittime1 + 1000) 
                {
 
                    Console.WriteLine("1s tick");
                    waittime1 = Environment.TickCount;
                    if (previouscoords.X2 == Player.Position.X && previouscoords.Y2 == Player.Position.Y && Player.Health/Player.MaxHealth > 0.4)
                    {
                        StatusUpdate("Nothing to do, moving top");
                        foreach (PointData point in PointData2.Where(point => point.Name == "Top"))
                        {
                        Player.IssueOrder(GameObjectOrder.MoveTo, point.Position);
                        }
                    }
                    else
                    {
                        previouscoords.X2 = Player.Position.X;
                        previouscoords.Y2 = Player.Position.Y;
                    }
                }
                if (Enemies.Top > 1 && Allies.Top == 0 && Distance.Top <3000)
                {
                    if (Player.Team == GameObjectTeam.Order)
                    {
                        foreach (PointData point in PointData2.Where(point => point.Name == "MiddleLeft"))
                        {
                    Player.IssueOrder(GameObjectOrder.MoveTo, point.Position);
                        }

                    }
                    else
                    {
                        foreach (PointData point in PointData2.Where(point => point.Name == "MiddleRIght"))
                        {
                    Player.IssueOrder(GameObjectOrder.MoveTo, point.Position);
                        }
                    }
                    StatusUpdate("Retiring :^)");
                }
            }

        }

        public static void Game_OnGameUpdate(EventArgs args)
        {
            MoveLogicOnTick();
        }
        private static void GetShop()
        {
            foreach (GameObject obj in ObjectManager.Get<Obj_Shop>())
            {
                if (obj.IsAlly)
                {
                    shop = obj;
                    new PointData("Start", new Vector3(obj.Position.X,obj.Position.Y,obj.Position.Z));
                }
                else
                {
                    new PointData("End", new Vector3(obj.Position.X, obj.Position.Y, obj.Position.Z));
                }
            }
        }
        private static void OnProcessSpell(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs Spell)
        {
            if ((Spell.SData.Name == "OdinCaptureChannel") && unit.Team != ObjectManager.Player.Team)
            {
                foreach (PointData curpoint in PointData2)
                if (Vector3.Distance(unit.Position, curpoint.Position) <= 1000f)
                {
                    cappedpoint = curpoint.Unit;
                    Console.WriteLine("SOMEONE CAPTURING : " + curpoint.Name);
                }
            }
            if (unit.Team != Player.Team && unit.Type == GameObjectType.obj_AI_Hero && Spell.Target != null && Spell.Target.Type == GameObjectType.obj_AI_Hero && Spell.Target.Team == Player.Team)
            {
                attackedhero = Spell.Target;
            }
        }
        private static bool MinionAround()
        {
            var allMinions = MinionManager.GetMinions(ObjectManager.Player.ServerPosition, 600,MinionTypes.All,MinionTeam.Enemy);
            if (allMinions.Count > 0)
            {
                return true;
            }
            else 
                return false;
        }
        private static void assignpoints()
        {
            GetShop();
            foreach (Obj_AI_Minion m in ObjectManager.Get<Obj_AI_Minion>())
            {
                foreach (PointData curpoint in PointData2)
                {
                    if ((Vector3.Distance(m.ServerPosition, curpoint.Position) <= 500f) && (m.Name == "OdinNeutralGuardian"))
                    {
                        curpoint.Unit = m;
                        if (curpoint.Unit.Team == GameObjectTeam.Chaos)
                        {
                            curpoint.State = "Red";
                        }
                        else if (curpoint.Unit.Team == GameObjectTeam.Order)
                        {
                            curpoint.State = "Green";
                        }
                        else if (curpoint.Unit.Team == GameObjectTeam.Neutral)
                        {
                            curpoint.State = "Neutral";
                        }
                    }
                }
            }
        }
        private static void GetChampion(EventArgs args)
        {
            switch (ObjectManager.Player.ChampionName)
            {
                case "Annie":
                    Annie.Game_OnGameLoad(args);
                    champ = true;
                    break;
                case "Darius":
                    Darius.Game_OnGameLoad(args);
                    champ = true;
                    break;
                case "Katarina":
                    Katarina.Game_OnGameLoad(args);
                    champ = true;
                    break;
                case "Ryze":
                    Ryze.Game_OnGameLoad(args);
                    champ = true;
                    break;
                case "XinZhao":
                    XinZhao.Game_OnGameLoad(args);
                    champ = true;
                    break;
            }
        }
        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Utility.Map.GetMap() == Utility.Map.MapType.CrystalScar)
            {
                
                Summoners.summonersinit();
                Console.WriteLine("TRUSBot");
                assignpoints();
                Leveling.LevelingUpdate();
                Game.OnGameUpdate += Game_OnGameUpdate;
                Game.OnGameUpdate += Summoners.SummonersTick;
                Game.OnGameUpdate += Leveling.LevelingTick;
                Game.OnGameEnd += Game_OnGameEnd;
                Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
                Player = ObjectManager.Player;
                Drawing.OnDraw += OnDraw;
                GetChampion(args);
                if (champ == false)
                {
                    Unknown.Game_OnGameLoad(args);
                }
            }
            else
            {
                Game.PrintChat("This map is not supported");
            }
        }

        private static void OnDraw(EventArgs args)
        {
            Vector2 playerpos = Drawing.WorldToScreen(Player.Position);
            Drawing.DrawText(playerpos.X, playerpos.Y, System.Drawing.Color.White, currentstatus);
        }
        static void Game_OnGameEnd(GameEndEventArgs args)
        {

        }
        private static void Capture(GameObject point)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write((byte)0x3A);
            bw.Write(ObjectManager.Player.NetworkId);
            bw.Write(point.NetworkId);

            Game.SendPacket(ms.ToArray(), PacketChannel.C2S, PacketProtocolFlags.NoFlags);
        }

        private static void BuyItem(int itemid)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write(Packet.C2S.BuyItem.Header);
            bw.Write(ObjectManager.Player.NetworkId);
            bw.Write(itemid);

            Game.SendPacket(ms.ToArray(), PacketChannel.C2S, PacketProtocolFlags.NoFlags);
        }
    }
}