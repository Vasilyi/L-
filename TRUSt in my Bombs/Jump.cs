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
    public class JumpSpot
    {
        public Vector3 Jumppos { get; private set; }

        public Vector3 MovePosition { get; private set; }

        public JumpSpot(Vector3 JumpPoistion, Vector3 movePosition)
        {
            Jumppos = JumpPoistion;
            MovePosition = movePosition;
        }
    }
}