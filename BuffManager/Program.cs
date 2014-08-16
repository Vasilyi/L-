#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
#endregion

namespace BuffMngr
{
    public class Program
    {

        public enum BuffTypes
        {
            BUFF_NONE = 0,
            BUFF_GLOBAL = 1,
            BUFF_BASIC = 2,
            BUFF_DEBUFF = 3,
            BUFF_STUN = 5,
            BUFF_STEALTH = 6,
            BUFF_SILENCE = 7,
            BUFF_TAUNT = 8,
            BUFF_SLOW = 10,
            BUFF_ROOT = 11,
            BUFF_DOT = 12,
            BUFF_REGENERATION = 13,
            BUFF_SPEED = 14,
            BUFF_MAGIC_IMMUNE = 15,
            BUFF_PHYSICAL_IMMUNE = 16,
            BUFF_IMMUNE = 17,
            BUFF_Vision_Reduce = 19,
            BUFF_FEAR = 21,
            BUFF_CHARM = 22,
            BUFF_POISON = 23,
            BUFF_SUPPRESS = 24,
            BUFF_BLIND = 25,
            BUFF_STATS_INCREASE = 26,
            BUFF_STATS_DECREASE = 27,
            BUFF_FLEE = 28,
            BUFF_KNOCKUP = 29,
            BUFF_KNOCKBACK = 30,
            BUFF_DISARM = 31,
        }


        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            LeagueSharp.Game.OnGameProcessPacket += PacketHandler;
            OnGainBuff += BuffGained;
            OnLoseBuff += BuffLost;
        }

        public static void BuffGained(Obj_AI_Base target, Obj_AI_Base source, Program.OnGainBuffArgs args)
        {
            Game.PrintChat(target.BaseSkinName + "Gain Buff" + args.BuffID);
        }

        public static void BuffLost(Obj_AI_Base target, Obj_AI_Base source, Program.OnGainBuffArgs args)
        {
            Game.PrintChat(target.BaseSkinName + "Lost Buff" + args.BuffID);
        }

        public delegate void OnGainBuffp(Obj_AI_Base target, Obj_AI_Base source, OnGainBuffArgs args);
        public delegate void OnLoseBuffp(Obj_AI_Base target, Obj_AI_Base source, OnGainBuffArgs args);
        public delegate void OnUpdateBuffp(Obj_AI_Base target, Obj_AI_Base source, OnGainBuffArgs args);
        public static event OnGainBuffp OnGainBuff;
        public static event OnLoseBuffp OnLoseBuff;
        public static event OnUpdateBuffp OnUpdateBuff;
        private static void PacketHandler(GamePacketEventArgs args)
        {
            if (OnGainBuff != null)
            {
                if (args.PacketData[0] == 0xB7)
                {

                    var targetbuff =
                        ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(BitConverter.ToInt32(args.PacketData, 1));
                    int buffSlot = args.PacketData[5];
                    int bufftype = args.PacketData[6];
                    int stackCount = args.PacketData[7];
                    int visible = args.PacketData[8];
                    int buffID = (BitConverter.ToInt32(args.PacketData, 9));
                    int targetID = (BitConverter.ToInt32(args.PacketData, 13));
                    int time = (BitConverter.ToInt32(args.PacketData, 21));
                    var sourceNetworkId = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(BitConverter.ToInt32(args.PacketData, 24));
                    OnGainBuff(targetbuff, sourceNetworkId,
                        new OnGainBuffArgs { Slot = buffSlot+1, Type = bufftype, Count = stackCount, Visible = visible, BuffID = buffID, TargetID = targetID, Timer = time, });
                }
            }
            if (OnLoseBuff != null)
            {
                if (args.PacketData[0] == 0x7B)
                {

                    int buffSlot = args.PacketData[5];
                    int buffID = (BitConverter.ToInt32(args.PacketData, 6));
                    var targetbuff = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(BitConverter.ToInt32(args.PacketData, 24));
                    if (targetbuff == null) 
                        return;
                    OnLoseBuff(targetbuff, targetbuff,
                        new OnGainBuffArgs { Slot = buffSlot + 1, Count = 0, BuffID = buffID});
                }
            }
            if (OnUpdateBuff != null)
            {
                if (args.PacketData[0] == 0x30)
                {

                    int buffSlot = args.PacketData[5];
                    int timeBuffAlreadyOnTarget = (BitConverter.ToInt32(args.PacketData, 6));
                    int duration = (BitConverter.ToInt32(args.PacketData, 10));
                    var source = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(BitConverter.ToInt32(args.PacketData, 14));
                    var targetbuff = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(BitConverter.ToInt32(args.PacketData, 1));
                    if (targetbuff == null)
                        return;
                    OnUpdateBuff(source, targetbuff,
                        new OnGainBuffArgs { Slot = buffSlot + 1, Count = 1 });
                }

                if (args.PacketData[0] == 0x1D)
                {

                    int buffSlot = args.PacketData[5];
                    int stackCount = args.PacketData[7];
                    int time = (BitConverter.ToInt32(args.PacketData, 8));
                    int timeBuffAlreadyOnTarget = (BitConverter.ToInt32(args.PacketData, 12));
                    int duration = (BitConverter.ToInt32(args.PacketData, 10));
                    var source = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(BitConverter.ToInt32(args.PacketData, 14));
                    var targetbuff = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(BitConverter.ToInt32(args.PacketData, 1));
                    if (targetbuff == null)
                        return;
                    OnUpdateBuff(source, targetbuff,
                        new OnGainBuffArgs { Slot = buffSlot + 1, Count = stackCount, Timer = duration });
                }
            }
        }

        public class OnGainBuffArgs : EventArgs
        {
            public int Slot;
            public int Type;
            public int Count;
            public int Visible;
            public int BuffID;
            public int TargetID;
            public int Timer;


        }

    }
}