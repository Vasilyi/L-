#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
#endregion

namespace BuffLib
{
    public class BuffMngr
    {

        public static void BuffMngrInit()
        {
            Console.WriteLine("BUFF MNGR LOADED");
            LeagueSharp.Game.OnGameProcessPacket += PacketHandler;
        }

        public delegate void OnGainBuffp(Obj_AI_Base target, Obj_AI_Base source, OnGainBuffArgs args);
        public delegate void OnLoseBuffp(Obj_AI_Base target, Obj_AI_Base source, OnGainBuffArgs args);
        public delegate void OnUpdateBuffp(Obj_AI_Base target, Obj_AI_Base source, OnGainBuffArgs args);
        public static event OnGainBuffp OnGainBuff;
        public static event OnLoseBuffp OnLoseBuff;
        public static event OnUpdateBuffp OnUpdateBuff;
        private static void PacketHandler(GamePacketEventArgs args)
        {
            var packet = new GamePacket(args.PacketData);
            if (OnGainBuff != null)
            {
                
                if (args.PacketData[0] == 0xB7)
                {
                    packet.Position = 1;
                    var targetbuff =
                        ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(packet.ReadInteger());
                    int buffSlot = packet.ReadByte();
                    int bufftype = packet.ReadByte();
                    int stackCount = packet.ReadByte();
                    int visible = packet.ReadByte();
                    int buffID = packet.ReadInteger();
                    int targetID = packet.ReadInteger();
                    int unknown = packet.ReadInteger();
                    float duration = packet.ReadFloat();
                    float starttime = Game.Time;
                    float endtime = Game.Time + duration;
                    var sourceNetworkId = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(BitConverter.ToInt32(args.PacketData, 24));
                    OnGainBuff(targetbuff, sourceNetworkId,
                        new OnGainBuffArgs { Slot = buffSlot + 1, Type = bufftype, Count = stackCount, Visible = visible, BuffID = buffID, TargetID = targetID, Duration = duration, StartTime = starttime, EndTime = endtime });
                    
                };
            };
            if (OnLoseBuff != null)
            {
                
                if (args.PacketData[0] == 0x7B)
                {
                    packet.Position = 1;
                    int buffSlot = packet.ReadByte();
                    int buffID = packet.ReadInteger();
                    var targetbuff = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(packet.ReadInteger());
                    if (targetbuff == null)
                        return;
                    OnLoseBuff(targetbuff, targetbuff,
                        new OnGainBuffArgs { Slot = buffSlot + 1, Count = 0, BuffID = buffID });
                };
            };
            if (OnUpdateBuff != null)
            {
                
                if (args.PacketData[0] == 0x2F)
                {
                    Console.WriteLine("Update BUFF PACKET");
                    var targetbuff = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(packet.ReadInteger());
                    int buffSlot = packet.ReadByte();
                    float timeBuffAlreadyOnTarget = packet.ReadFloat();
                    float duration = packet.ReadFloat();
                    float starttime = Game.Time;
                    float endtime = Game.Time + duration;
                    var source = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(packet.ReadInteger());
                    if (targetbuff == null)
                        return;
                    OnUpdateBuff(source, targetbuff,
                        new OnGainBuffArgs { Slot = buffSlot + 1, Count = 1, Duration = duration, EndTime = endtime });
                };

                if (args.PacketData[0] == 0x1C)
                {
                    Console.WriteLine("Update BUFF PACKET2");
                    var targetbuff = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(packet.ReadInteger());
                    int buffSlot = packet.ReadByte();
                    int stackCount = packet.ReadByte();
                    float duration = packet.ReadFloat();
                    float timeBuffAlreadyOnTarget = packet.ReadFloat();
                    float starttime = Game.Time;
                    float endtime = Game.Time + duration;
                    var source = ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(packet.ReadInteger());
                    if (targetbuff == null)
                        return;
                    OnUpdateBuff(source, targetbuff,
                        new OnGainBuffArgs { Slot = buffSlot + 1, Count = stackCount, Duration = duration, EndTime = endtime });
                };
            };
        }

        public class OnGainBuffArgs : EventArgs
        {
            public int Slot;
            public int Type;
            public int Count;
            public int Visible;
            public int BuffID;
            public int TargetID;
            public float Duration;
            public float StartTime;
            public float EndTime;

        }

    }
}