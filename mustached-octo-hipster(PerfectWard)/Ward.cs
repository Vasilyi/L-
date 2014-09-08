using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using LeagueSharp;

namespace PerfectWard
{
    public class Ward
    {
        private const float WARD_INDICATOR_RADIUS = 80.0f;

        public const string GREEN_WARD_NAME = "SightWard";
        public const float GREEN_WARD_LENGTH = 180.0f;
        public const string PINK_WARD_NAME = "VisionWard";
        public const float PINK_WARD_LENGTH = float.MaxValue;
        public const string TRINKET_WARD_NAME = "YellowTrinket";
        public const float TRINKET_WARD_LENGTH = 60.0f;

        private static List<Vector3> _WardSpots;
        private static List<WardSpot> _SafeWardSpots;

        public static List<Vector3> WardSpots
        {
            get
            {
                if(_WardSpots == null)
                {
                    InitializeWardSpots();
                }

                return _WardSpots;
            }
        }

        public static List<WardSpot> SafeWardSpots
        {
            get
            {
                if(_SafeWardSpots == null)
                {
                    InitializeSafeWardSpots();
                }

                return _SafeWardSpots;
            }
        }

        public Vector3 Position { get; private set; }

        public float[] TextPos { get; private set; }

        public float AliveTo { get; private set; }

        public int NetworkId { get; private set; }

        private System.Drawing.Color DrawColor { get; set; }

        public Ward(string skinName, Vector3 position, int networkId)
        {
            Position = position;


            if (skinName == GREEN_WARD_NAME)
            {
                AliveTo = Game.Time + GREEN_WARD_LENGTH;
                DrawColor = System.Drawing.Color.Green;
            }
            else if (skinName == PINK_WARD_NAME)
            {
                AliveTo = PINK_WARD_LENGTH; // Pinks may last forever.
                DrawColor = System.Drawing.Color.DeepPink;
            }
            else if (skinName == TRINKET_WARD_NAME)
            {
                AliveTo = Game.Time + TRINKET_WARD_LENGTH;
                DrawColor = System.Drawing.Color.Yellow;
            }

            NetworkId = networkId;
        }

        public void Draw()
        {
            //Drawing.DrawCircle(Position, WARD_INDICATOR_RADIUS, System.Drawing.Color.Red);

            Vector2 TextPos = Drawing.WorldToScreen(Position);
            Drawing.DrawCircle(Position, 80.0f, DrawColor);

            if (AliveTo != float.MaxValue)
            {
                Drawing.DrawText(TextPos[0] - 15.0f, TextPos[1] - 10.0f, System.Drawing.Color.White, String.Format("{0}", (int)(AliveTo - Game.Time)));
            }

        }

        public override bool Equals(object obj)
        {
            Ward wardItem = obj as Ward;

            return wardItem.NetworkId == this.NetworkId;
        }

        public bool CheckRemove()
        {

            if (AliveTo == float.MaxValue &&
                ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(NetworkId) != null &&
                ObjectManager.GetUnitByNetworkId<Obj_AI_Base>(NetworkId).IsDead)
            {
                AliveTo = Game.Time - 1.0f;
            }

            // Hold the object for a while after it's death
            // to ensure we don't re-add it while its getting reaped.
            return (Game.Time >= AliveTo + 5.0f);
        }

        public override int GetHashCode()
        {
            return NetworkId;
        }

        private static void InitializeWardSpots()
        {
            _WardSpots = new List<Vector3>();

            _WardSpots.Add(new Vector3(2823.37f, 7617.03f, 55.03f));    // Blue Golem
            _WardSpots.Add(new Vector3(7422.0f, 3282.0f, 46.53f));      // Blue Lizard
            _WardSpots.Add(new Vector3(10148.0f, 2839.0f, 44.41f));     // Blue Tri Bush
            _WardSpots.Add(new Vector3(6269.0f, 4445.0f, 42.51f));      // Blue Pass Bush
            _WardSpots.Add(new Vector3(7151.64f, 4719.66f, 51.67f));    // Blue River Entrance
            _WardSpots.Add(new Vector3(4354.54f, 7079.51f, 53.67f));    // Blue Round Bush
            _WardSpots.Add(new Vector3(4728.0f, 8336.0f, 51.29f));      // Blue River Round Bush
            _WardSpots.Add(new Vector3(6762.52f, 2918.75f, 55.68f));    // Blue Split Push Bush

            _WardSpots.Add(new Vector3(11217.39f, 6841.89f, 54.87f));   // Purple Golem
            _WardSpots.Add(new Vector3(6610.35f, 11064.61f, 54.45f));   // Purple Lizard
            _WardSpots.Add(new Vector3(3883.0f, 11577.0f, 39.87f));     // Purple Tri Bush
            _WardSpots.Add(new Vector3(7775.0f, 10046.49f, 43.14f));    // Purple Pass Bush
            _WardSpots.Add(new Vector3(6867.68f, 9567.63f, 57.01f));    // Purple River Entrance
            _WardSpots.Add(new Vector3(9720.86f, 7501.50f, 54.85f));    // Purple Round Bush
            _WardSpots.Add(new Vector3(9233.13f, 6094.48f, -44.63f));   // Purple River Round Bush
            _WardSpots.Add(new Vector3(7282.69f, 1148992.53f, 52.59f)); // Purple Split Push Bush

            _WardSpots.Add(new Vector3(10180.18f, 4969.32f, -62.32f));  // Dragon
            _WardSpots.Add(new Vector3(8875.13f, 5390.57f, -64.07f));   // Dragon Bush
            _WardSpots.Add(new Vector3(3920.88f, 9477.78f, -60.42f));   // Baron
            _WardSpots.Add(new Vector3(5017.27f, 8954.09f, -62.70f));   // Baron Bush

            _WardSpots.Add(new Vector3(12731.25f, 9132.66f, 50.32f));   // Purple Bot T2
            _WardSpots.Add(new Vector3(12731.25f, 9132.66f, 50.32f));   // Purple Bot T2
            _WardSpots.Add(new Vector3(9260.02f, 8582.67f, 54.62f));    // Purple Mid T1

            _WardSpots.Add(new Vector3(4749.79f, 5890.76f, 53.559f));   // Blue Mid T1
            _WardSpots.Add(new Vector3(5983.58f, 1547.98f, 52.99f));    // Blue Bot T2
            _WardSpots.Add(new Vector3(1213.70f, 5324.73f, 58.77f));    // Blue Top T2
        }

        private static void InitializeSafeWardSpots()
        {
            _SafeWardSpots = new List<WardSpot>();

            // Dragon -> Tri Bush
            _SafeWardSpots.Add(new WardSpot(new Vector3(9695.0f, 3465.0f, 43.02f),
                                            new Vector3(9843.38f, 3125.16f, 43.02f),
                                            new Vector3(9946.10f, 3064.81f, 43.02f),
                                            new Vector3(9595.0f, 3665.0f, 43.02f)));
            // Nashor -> Tri Bush
            _SafeWardSpots.Add(new WardSpot(new Vector3(4346.10f, 10964.81f, 36.62f),
                                            new Vector3(4214.93f, 11202.01f, 36.62f),
                                            new Vector3(4146.10f, 11314.81f, 36.62f),
                                            new Vector3(4384.36f, 10680.41f, 36.62f)));

            // Blue Top -> Solo Bush
            _SafeWardSpots.Add(new WardSpot(new Vector3(2349.0f, 10387.0f, 44.20f),
                                            new Vector3(2257.97f, 10783.37f, 44.20f),
                                            new Vector3(2446.10f, 10914.81f, 44.20f),
                                            new Vector3(2311.0f, 10185.0f, 44.20f)));

            // Blue Mid -> round Bush // Inconsistent Placement
            _SafeWardSpots.Add(new WardSpot(new Vector3(4946.52f, 6474.56f, 54.71f),
                                            new Vector3(4891.98f, 6639.05f, 53.62f),
                                            new Vector3(4546.10f, 6864.81f, 53.78f),
                                            new Vector3(5217.0f, 6263.0f, 54.95f)));

            // Blue Mid -> River Lane Bush
            _SafeWardSpots.Add(new WardSpot(new Vector3(5528.96f, 7615.20f, 45.64f),
                                            new Vector3(5688.96f, 7825.20f, 45.64f),
                                            new Vector3(5796.10f, 7914.81f, 45.64f),
                                            new Vector3(5460.13f, 7469.77f, 45.64f)));

            // Blue Lizard -> Dragon Pass Bush
            _SafeWardSpots.Add(new WardSpot(new Vector3(7745.0f, 4065.0f, 47.71f),
                                            new Vector3(7927.65f, 4239.77f, 47.71f),
                                            new Vector3(8146.10f, 4414.81f, 47.71f),
                                            new Vector3(7645.0f, 4015.0f, 47.71f)));

            // Purple Mid -> Round Bush // Inconsistent Placement
            _SafeWardSpots.Add(new WardSpot(new Vector3(9057.0f, 8245.0f, 45.73f),
                                            new Vector3(9230.7f, 7892.22f, 66.39f),
                                            new Vector3(9446.10f, 7814.81f, 54.66f),
                                            new Vector3(8895.0f, 8313.0f, 54.89f)));

            // Purple Mid -> River Round Bush
            _SafeWardSpots.Add(new WardSpot(new Vector3(9025.78f, 6591.64f, 46.27f),
                                            new Vector3(9200.08f, 6425.05f, 43.21f),
                                            new Vector3(9396.10f, 6264.81f, 23.72f),
                                            new Vector3(8795.0f, 6815.0f, 56.11f)));

            // Purple Mid -> River Lane Bush
            _SafeWardSpots.Add(new WardSpot(new Vector3(8530.27f, 6637.38f, 46.98f),
                                            new Vector3(8539.27f, 6637.38f, 46.98f),
                                            new Vector3(8396.10f, 6464.81f, 46.98f),
                                            new Vector3(8779.17f, 6804.70f, 46.98f)));

            // Purple Bottom -> Solo Bush
            _SafeWardSpots.Add(new WardSpot(new Vector3(11889.0f, 4205.0f, 42.84f),
                                            new Vector3(11974.23f, 3807.21f, 42.84f),
                                            new Vector3(11646.10f, 3464.81f, 42.84f),
                                            new Vector3(11939.0f, 4255.0f, 42.84f)));

            // Purple Lizard -> Nashor Pass Bush // Inconsistent Placement
            _SafeWardSpots.Add(new WardSpot(new Vector3(6299.0f, 10377.75f, 45.47f),
                                            new Vector3(6030.24f, 10292.37f, 54.29f),
                                            new Vector3(5846.10f, 10164.81f, 53.94f),
                                            new Vector3(6447.0f, 10463.0f, 54.63f)));
        }

        public static Vector3? FindNearestWardSpot(Vector3 cursorPosition)
        {
            foreach (Vector3 wardPosition in WardSpots)
            {

                double cursorDistToWard = Math.Sqrt(Math.Pow(wardPosition.X - Game.CursorPos.X, 2) +
                             Math.Pow(wardPosition.Y - Game.CursorPos.Y, 2) +
                             Math.Pow(wardPosition.Z - Game.CursorPos.Z, 2));

                double playerDistToWard = Math.Sqrt(Math.Pow(wardPosition.X - ObjectManager.Player.Position.X, 2) +
                                                    Math.Pow(wardPosition.Y - ObjectManager.Player.Position.Y, 2) +
                                                    Math.Pow(wardPosition.Z - ObjectManager.Player.Position.Z, 2));

                if (cursorDistToWard <= 250.0 && playerDistToWard <= 650.0) 
                {
                    return wardPosition;
                }
            }

            return null;
        }

        public static WardSpot FindNearestSafeWardSpot(Vector3 cursorPosition)
        {
            foreach (WardSpot wardSpot in SafeWardSpots)
            {
                double cursorDistToWard = Math.Sqrt(Math.Pow(wardSpot.MagneticPosition.X - Game.CursorPos.X, 2) +
                             Math.Pow(wardSpot.MagneticPosition.Y - Game.CursorPos.Y, 2) +
                             Math.Pow(wardSpot.MagneticPosition.Z - Game.CursorPos.Z, 2));

                if (cursorDistToWard <= 100.0)
                {
                    return wardSpot;
                }
            }

            return null;
        }

        public static void DrawWardSpots()
        {
            foreach (Vector3 wardPos in WardSpots)
            {
                System.Drawing.Color wardColor = (Math.Sqrt(Math.Pow(wardPos.X - Game.CursorPos.X, 2) +
                                     Math.Pow(wardPos.Y - Game.CursorPos.Y, 2) +
                                     Math.Pow(wardPos.Z - Game.CursorPos.Z, 2)) <= 250.0) ? System.Drawing.Color.Red : System.Drawing.Color.Blue;

                Vector2 screenPos = Drawing.WorldToScreen(wardPos);

                if (IsOnScreen(screenPos[0], screenPos[1]))
                {
                    Drawing.DrawCircle(wardPos, WARD_INDICATOR_RADIUS, wardColor);
                }
            }
        }

        public static void DrawSafeWardSpots()
        {
            foreach (WardSpot safeWardSpot in SafeWardSpots)
            {
                System.Drawing.Color wardColor = (Math.Sqrt(Math.Pow(safeWardSpot.MagneticPosition.X - Game.CursorPos.X, 2) +
                                     Math.Pow(safeWardSpot.MagneticPosition.Y - Game.CursorPos.Y, 2) +
                                     Math.Pow(safeWardSpot.MagneticPosition.Z - Game.CursorPos.Z, 2)) <= 100.0) ? System.Drawing.Color.Red : System.Drawing.Color.Blue;
                System.Drawing.Color arrowColor = (Math.Sqrt(Math.Pow(safeWardSpot.MagneticPosition.X - Game.CursorPos.X, 2) +
                                     Math.Pow(safeWardSpot.MagneticPosition.Y - Game.CursorPos.Y, 2) +
                                     Math.Pow(safeWardSpot.MagneticPosition.Z - Game.CursorPos.Z, 2)) <= 100.0) ? System.Drawing.Color.Green : System.Drawing.Color.FromArgb(0, 255, 255, 255);

                Vector2 screenPos = Drawing.WorldToScreen(safeWardSpot.MagneticPosition);
                if (IsOnScreen(screenPos[0], screenPos[1]))
                {
                    Drawing.DrawCircle(safeWardSpot.WardPosition, 31.0f, wardColor);
                    Drawing.DrawCircle(safeWardSpot.WardPosition, 32.0f, wardColor);

                    Drawing.DrawCircle(safeWardSpot.MagneticPosition, 99.0f, wardColor);
                    Drawing.DrawCircle(safeWardSpot.MagneticPosition, 100.0f, wardColor);


                    
                    Vector3 directionVector = (safeWardSpot.WardPosition - safeWardSpot.MagneticPosition);
                    directionVector.Normalize();
                    float len = (safeWardSpot.WardPosition - safeWardSpot.MagneticPosition).Length();

                    Vector3 newVector = new Vector3(safeWardSpot.MagneticPosition.X + (directionVector.X * len),
                                                    safeWardSpot.MagneticPosition.Y + (directionVector.Y * len),
                                                    safeWardSpot.MagneticPosition.Z + (directionVector.Z * len));


                    Vector2 screenMagneticPos = Drawing.WorldToScreen(safeWardSpot.MagneticPosition);
                    Vector2 screenDirectionVector = Drawing.WorldToScreen(newVector);

                    Drawing.DrawLine(screenMagneticPos[0], screenMagneticPos[1], screenDirectionVector[0], screenDirectionVector[1], 2.5f, arrowColor);
                  
                }
            }
        }

       

        public static bool IsOnScreen(float x, float y)
        {
            return (x >= 0 && x <= 1920 && y >= 0 && y <= 1020);
        }

    }
}
