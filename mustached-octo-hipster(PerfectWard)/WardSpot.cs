using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerfectWard
{
    public class WardSpot
    {
        public Vector3 MagneticPosition { get; private set; }

        public Vector3 ClickPosition { get; private set; }

        public Vector3 WardPosition { get; private set; }

        public Vector3 MovePosition { get; private set; }

        public WardSpot(Vector3 magneticPosition, Vector3 clickPosition,
                            Vector3 wardPosition, Vector3 movePosition)
        {
            MagneticPosition = magneticPosition;
            ClickPosition = clickPosition;
            WardPosition = wardPosition;
            MovePosition = movePosition;
        }
    }
}
