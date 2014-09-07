using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using System.IO;
using System.Diagnostics;
using System.Windows;
using SharpDX;
specially broke this assembly, so noobs will not ask what it doing
namespace TRUSDominion
{
    class Program
    {
        static void Main(string[] args)
        {
            Game.OnGameStart += Game_OnGameStart;
            if (Game.Time > 20)
            {
                Game_OnGameStart(new EventArgs());
            }
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
            }
        }


        static readonly List<PointData> PointData2 = new List<PointData>()
        {
            new PointData("BotRight", new Vector3(9533f, 2519f, -142f)),
            new PointData("BotLeft", new Vector3(4342f, 2515f, -145f)),
            new PointData("Top", new Vector3(6941f, 10981f, -147f)),
            new PointData("Middleleft", new Vector3(2519f, 7781f, -141f)),
            new PointData("MiddleRight", new Vector3(11401f, 7775f, -145f)),
        };
        private static bool buyitemdelay = false;
        private static List<Obj_AI_Minion> capturePoints;
        private static GameObject shop;
        private static void Game_OnGameNotifyEvent(GameNotifyEventArgs args)
        {
            if (args.EventId == GameEventId.OnCapturePointCaptured_A || args.EventId == GameEventId.OnCapturePointCaptured_B || args.EventId == GameEventId.OnCapturePointCaptured_C || args.EventId == GameEventId.OnCapturePointCaptured_D || args.EventId == GameEventId.OnCapturePointCaptured_E || args.EventId == GameEventId.OnCapturePointFiveCap)
            if (args.EventId == GameEventId.OnItemPurchased)
            {
                buyitemdelay = false;
            }
        }
        private static int CountHeroes(string sTeam, Vector3 position, int distance)
        {
            int counter = 0;
            foreach (Obj_AI_Hero obj in ObjectManager.Get<Obj_AI_Hero>())
            {
                if ((obj.Team.ToString()  == sTeam) && (Vector3.Distance(obj.ServerPosition, position) <= distance))
                {
                    counter++;
                }
            }
            return counter;
        }

        private static bool HasItem(string itemid)
        {
            bool hasitem1 = false;
            foreach (InventorySlot inv in ObjectManager.Player.InventoryItems)
            {
                if (inv.Id.ToString() == itemid)
                    hasitem1 = true;
            }
            return hasitem1;
        }
        private static void BuyItemsTick()
        {
            if ((buyitemdelay == false) && ((Vector3.Distance(shop.Position, ObjectManager.Player.Position) <= 1250f) || ObjectManager.Player.IsDead))
            {
                BuyItem(2003);
            }
        }
        private static void Game_OnGameUpdate(EventArgs args)
        {
            BuyItemsTick();
            if (Game.Time < 30)
            {
                if (ObjectManager.Player.Team == GameObjectTeam.Order)
                    ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, new Vector3(647f, 4427f, 0));
                else
                    ObjectManager.Player.IssueOrder(GameObjectOrder.MoveTo, new Vector3(13192f, 4388f, 0));
            }
        }
        private static void GetShop()
        {
            foreach (GameObject obj in ObjectManager.Get<Obj_Shop>())
            {
                Game.PrintChat("DONT FORGET TO PUT SHOP TEAM " + obj.Team);
                GameObject shop = obj;
            }
        }
        private static void OnProcessSpell(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs Spell)
        {
            if ((Spell.SData.Name == "OdinCaptureChannel") && unit.Team != ObjectManager.Player.Team)
            {
                foreach (PointData curpoint in PointData2)
                if (Vector3.Distance(unit.Position, curpoint.Position) <= 1000f)
                {
                    GameObject cappedpoint = curpoint.Unit;
                    Game.PrintChat("SOMEONE CAPTURING : " + curpoint.Name);
                }
            }
        }
        private static void assignpoints()
        {
            GetShop();
            capturePoints = new List<Obj_AI_Minion>();
            foreach (Obj_AI_Minion m in ObjectManager.Get<Obj_AI_Minion>())
            {
                foreach (PointData curpoint in PointData2)
                {
                    if ((Vector3.Distance(m.ServerPosition, curpoint.Position) <= 500f) && (m.Name == "OdinNeutralGuardian"))
                    {
                        curpoint.Unit = m;
                        Game.PrintChat(curpoint.Unit.Team.ToString());
                    }

                    foreach (GameObject obj in ObjectManager.Get<GameObject>())
                    {
                        
                        if ((Vector3.Distance(obj.Position, curpoint.Position) <= 100f))
                        {
                           
                            if (obj.Name == "OdinNeutralGuardian_Stone.troy")
                            {
                                curpoint.State = "Neutral";
                                Game.PrintChat(curpoint.Name.ToString() + " " + curpoint.State.ToString());

                            }
                            if (obj.Name == "OdinNeutralGuardian_Green.troy")
                            {
                                curpoint.State = "Green";
                                Game.PrintChat(curpoint.Name.ToString() + " " + curpoint.State.ToString());
                            }
                            if (obj.Name == "OdinNeutralGuardian_Red.troy")
                            {
                                curpoint.State = "Red";
                                Game.PrintChat(curpoint.Name.ToString() + " " + curpoint.State.ToString());

                            }

                        }
                    }
                }
            }
        }
        private static void Game_OnGameStart(EventArgs args)
        {
            Game.PrintChat("TRUSBot");
            assignpoints();
            GameObject.OnCreate += OnCreate;
            Game.OnGameNotifyEvent += Game_OnGameNotifyEvent;
            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.OnGameEnd += Game_OnGameEnd;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpell;
        }

        static void Game_OnGameEnd(GameEndEventArgs args)
        {
            Process[] processes = Process.GetProcessesByName("League of Legends");
            foreach (Process p in processes)
            {
                p.CloseMainWindow();
            }
        }
        static void OnCreate(GameObject obj, EventArgs args)
        {
            foreach (PointData curpoint in PointData2)
                if ((Vector3.Distance(obj.Position, curpoint.Position) <= 100f))
                {
                    if (obj.Name == "OdinNeutralGuardian_Stone.troy")
                    {
                        curpoint.State = "Neutral";
                        Game.PrintChat(curpoint.Name.ToString() + " " + curpoint.State.ToString());

                    }
                    if (obj.Name == "OdinNeutralGuardian_Green.troy")
                    {
                        curpoint.State = "Green";
                        Game.PrintChat(curpoint.Name.ToString() + " " + curpoint.State.ToString());
                        Game.PrintChat(obj.Type.ToString());
                    }
                    if (obj.Name == "OdinNeutralGuardian_Red.troy")
                    {
                        curpoint.State = "Red";
                        Game.PrintChat(curpoint.Name.ToString() + " " + curpoint.State.ToString());

                    }

                }
        }
        private static void Capture(Obj_AI_Minion point)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write((byte)0x39);
            bw.Write(ObjectManager.Player.NetworkId);
            bw.Write(point.NetworkId);

            Game.SendPacket(ms.ToArray(), PacketChannel.C2S, PacketProtocolFlags.NoFlags);
        }

        private static void BuyItem(int itemid)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            bw.Write((byte)0x81);
            bw.Write(ObjectManager.Player.NetworkId);
            bw.Write(itemid);

            Game.SendPacket(ms.ToArray(), PacketChannel.C2S, PacketProtocolFlags.NoFlags);
            buyitemdelay = true;
        }
    }
}