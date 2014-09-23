#region

using System;
using System.Collections.Generic;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using BuffLib;
#endregion

namespace TRUSDominion
{

    public class QMark
    {
        public string unit { get; private set; }

        public float endtime { get; private set; }

        public QMark(string Unit, float EndTime)
        {
            unit = Unit;
            endtime = EndTime;
        }
    }

    public class Katarina
    {
        public const string ChampionName = "Katarina";

        //Spells


        public static List<Spell> SpellList = new List<Spell>();
        public static List<QMark> MarkList = new List<QMark>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;
        public static SpellSlot IgniteSlot;
        
        //Menu
        public static Menu Config;
        private static Obj_AI_Hero Player;

        private static void BuffGained(Obj_AI_Base target, Obj_AI_Base source, BuffMngr.OnGainBuffArgs args)
        {
            if (target.IsMe && args.BuffID == 3334932)
            {
                tSpells.rEndTick = args.EndTime;
                tSpells.rStartTick = args.StartTime;
                tSpells.ulting = true;
            }
            else if (args.BuffID == 84848667) //mark
            {
                MarkList.Add(new QMark(target.BaseSkinName, args.EndTime));
            }
        }

        private static void BuffLost(Obj_AI_Base target, Obj_AI_Base source, BuffMngr.OnGainBuffArgs args)
        {
            if (target.IsMe && args.BuffID == 3334932)
            {
                tSpells.ulting = false;
            }
            else if (args.BuffID == 84848667) // mark
            {
                foreach (var mark in MarkList)
                {
                    if (mark.unit == target.BaseSkinName)
                    {
                        MarkList.Remove(mark);
                    }
                }
            }
        }
        public static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;
            if (Player.BaseSkinName != ChampionName) return;
            Console.WriteLine("ResetsAllTheWay loaded");
            BuffMngr.BuffMngrInit();
            BuffMngr.OnGainBuff += BuffGained;
            BuffMngr.OnLoseBuff += BuffLost;

            //Create the spells
            Q = new Spell(SpellSlot.Q, 675);
            W = new Spell(SpellSlot.W, 400);
            E = new Spell(SpellSlot.E, 700);
            R = new Spell(SpellSlot.R, 550);
            IgniteSlot = ObjectManager.Player.GetSpellSlot("SummonerDot");
            SpellList.Add(Q);
            SpellList.Add(W);
            SpellList.Add(E);
            SpellList.Add(R);

            Game.OnGameUpdate += Game_OnGameUpdate;
            Game.OnGameSendPacket += GameOnOnGameSendPacket;
        }

 
        public static class tSpells
        {
            public static float rEndTick;
            public static float rStartTick;
            public static bool ulting;
            public static float wLastUse;
            public static float qlastuse;
            public static bool usedfg;
            public static bool useignite;

        }

        public static void Game_OnGameUpdate(EventArgs args)
        {
                Combo();
        }

        private static void Combo()
        {
            var qtarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);

            DoCombo(qtarget);
        }


        private static void GameOnOnGameSendPacket(GamePacketEventArgs args)
        {
            if (args.PacketData[0] == Packet.C2S.Move.Header && Environment.TickCount < tSpells.rStartTick + 300)
            {
                args.Process = false;
                Console.WriteLine("BLOCK PACKET");
            }
        }

        private static void DoCombo(Obj_AI_Base target)
        {
            Items.UseItem(3128, target);
            Q.Cast(target, false);
            E.Cast(target);
            W.Cast();
            if (R.IsReady() && !W.IsReady() && ObjectManager.Player.Distance(target) < R.Range && !tSpells.ulting && Environment.TickCount > tSpells.rStartTick + 300)
            {
                ObjectManager.Player.IssueOrder(GameObjectOrder.HoldPosition, new Vector3(Player.ServerPosition.X, Player.ServerPosition.Y, Player.ServerPosition.Z));
                R.Cast();
                ObjectManager.Player.SummonerSpellbook.CastSpell(IgniteSlot, target);
            }
        }



    }
}
