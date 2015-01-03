using System;
using System.Collections.Generic;
using SharpDX;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;

namespace PerfectWard
{
    public class Ward
    {
        private const float WARD_INDICATOR_RADIUS = 80.0f;

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

        public static InventorySlot GetPinkSlot()
        {
            var wardIds = new[] { 2043, 3362 };
            return (from wardId in wardIds
                    where Items.CanUseItem(wardId)
                    select ObjectManager.Player.InventoryItems.FirstOrDefault(slot => slot.Id == (ItemId)wardId))
                .FirstOrDefault();
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

            _WardSpots.Add(new Vector3(3199.723f, 7846.65f, 52.0f));    // Blue Golem
            _WardSpots.Add(new Vector3(7831.46f, 3501.13f, 60.0f));      // Blue Lizard
            _WardSpots.Add(new Vector3(10586.62f, 3067.93f, 60.0f));     // Blue Tri Bush
            _WardSpots.Add(new Vector3(6483.73f, 4606.57f, 60.0f));      // Blue Pass Bush
            _WardSpots.Add(new Vector3(7610.46f, 5000.0f, 60.0f));    // Blue River Entrance
            _WardSpots.Add(new Vector3(4649.459f, 7124.964f, 50.83f));    // Blue Round Bush
            _WardSpots.Add(new Vector3(4799.897f, 8381.28f, 27.27f));      // Blue River Round Bush
            _WardSpots.Add(new Vector3(6951.01f, 3040.55f, 52.26f));    // Blue Split Push Bush
            _WardSpots.Add(new Vector3(5583.74f, 3573.83f, 51.43f));    // Blue Riveer Center Close

            _WardSpots.Add(new Vector3(11600.35f, 7090.37f, 51.73f));   // Purple Golem
            _WardSpots.Add(new Vector3(11573.9f, 6457.76f, 51.71f));   // Purple Golem2
            _WardSpots.Add(new Vector3(12629.72f, 4908.16f, 48.62f));     // Purple Tri Bush2
            _WardSpots.Add(new Vector3(7018.75f, 11362.12f, 54.76f));   // Purple Lizard
            _WardSpots.Add(new Vector3(4232.69f, 11869.25f, 47.56f));     // Purple Tri Bush
            _WardSpots.Add(new Vector3(8198.22f, 10267.89f, 49.38f));    // Purple Pass Bush
            _WardSpots.Add(new Vector3(7202.43f, 9881.83f, 53.18f));    // Purple River Entrance
            _WardSpots.Add(new Vector3(10074.63f, 7761.62f, 51.74f));    // Purple Round Bush
            _WardSpots.Add(new Vector3(9795.85f, 6355.15f, -12.21f));   // Purple River Round Bush
            _WardSpots.Add(new Vector3(7836.85f, 11906.34f, 56.48f)); // Purple Split Push Bush



            _WardSpots.Add(new Vector3(6250.84f, 10065.06f, 56.48f)); // Bush over baron pit

            _WardSpots.Add(new Vector3(10546.35f, 5019.06f, -60.0f));  // Dragon
            _WardSpots.Add(new Vector3(9344.95f, 5703.43f, -64.07f));   // Dragon Bush
            _WardSpots.Add(new Vector3(4334.98f, 9714.54f, -60.42f));   // Baron
            _WardSpots.Add(new Vector3(5363.31f, 9157.05f, -71.70f));   // Baron Bush

            _WardSpots.Add(new Vector3(12731.25f, 9132.66f, 50.32f));   // Purple Bot T2
            _WardSpots.Add(new Vector3(8036.52f, 12882.94f, 45.19f));   // Purple Bot T2
            _WardSpots.Add(new Vector3(9757.9f, 8768.25f, 50.73f));    // Purple Mid T1

            _WardSpots.Add(new Vector3(4749.79f, 5890.76f, 53.59f));   // Blue Mid T1
            _WardSpots.Add(new Vector3(5983.58f, 1547.98f, 52.99f));    // Blue Bot T2
            _WardSpots.Add(new Vector3(1213.70f, 5324.73f, 58.77f));    // Blue Top T2

            _WardSpots.Add(new Vector3(6523.58f, 6743.31f, 60.0f));   // Blue MidLane
            _WardSpots.Add(new Vector3(8223.67f, 8110.15f, 60.0f));   // Purple Nidlane
            _WardSpots.Add(new Vector3(9736.8f, 6916.26f, 51.98f));   // Purple Mid Path
            _WardSpots.Add(new Vector3(2222.31f, 9964.1f, 53.2f));   // Blue Tri Top
        }
        private static void InitializeSafeWardSpots()
        {
            _SafeWardSpots = new List<WardSpot>();

            // Dragon -> Tri Bush
            _SafeWardSpots.Add(new WardSpot(new Vector3(10072.0f, 3908.0f, -71.24f),
                                            new Vector3(10297.93f, 3358.59f, 49.03f),
                                            new Vector3(10273.9f, 3257.76f, 49.03f),
                                            new Vector3(10072.0f, 3908.0f, -71.24f)));
            // Nashor -> Tri Bush
            _SafeWardSpots.Add(new WardSpot(new Vector3(4724.0f, 10856.0f, -71.24f),
                                            new Vector3(4627.26f, 11311.69f, -71.24f),
                                            new Vector3(4473.9f, 11457.76f, 51.4f),
                                            new Vector3(4724.0f, 10856.0f, -71.24f)));

            // Blue Top -> Solo Bush
            _SafeWardSpots.Add(new WardSpot(new Vector3(2824.0f, 10356.0f, 54.33f),
                                            new Vector3(3078.62f, 10868.39f, 54.33f),
                                            new Vector3(3078.62f, 10868.39f, -67.95f),
                                            new Vector3(2824.0f, 10356.0f, 54.33f)));

            // Blue Mid -> round Bush // Inconsistent Placement
            _SafeWardSpots.Add(new WardSpot(new Vector3(5474.0f, 7906.0f, 51.67f),
                                            new Vector3(5132.65f, 8373.2f, 51.67f),
                                            new Vector3(5123.9f, 8457.76f, -21.23f),
                                            new Vector3(5474.0f, 7906.0f, 51.67f)));

            // Blue Mid -> River Lane Bush
            _SafeWardSpots.Add(new WardSpot(new Vector3(5874.0f, 7656.0f, 51.65f),
                                            new Vector3(6202.24f, 8132.12f, 51.65f),
                                            new Vector3(6202.24f, 8132.12f, -67.39f),
                                            new Vector3(5874.0f, 7656.0f, 51.65f)));

            // Blue Lizard -> Dragon Pass Bush
            _SafeWardSpots.Add(new WardSpot(new Vector3(8022.0f, 4258.0f, 53.72f),
                                            new Vector3(8400.68f, 4657.41f, 53.72f),
                                            new Vector3(8523.9f, 4707.76f, 51.24f),
                                            new Vector3(8022.0f, 4258.0f, 53.72f)));

            // Purple Mid -> Round Bush // Inconsistent Placement
            _SafeWardSpots.Add(new WardSpot(new Vector3(9372.0f, 7008.0f, 52.63f),
                                            new Vector3(9703.5f, 6589.9f, 52.63f),
                                            new Vector3(9823.9f, 6507.76f, 23.47f),
                                            new Vector3(9372.0f, 7008.0f, 52.63f)));

            // Purple Mid -> River Round Bush
            _SafeWardSpots.Add(new WardSpot(new Vector3(9072.0f, 7158.0f, 53.04f),
                                            new Vector3(8705.95f, 6819.1f, 53.04f),
                                            new Vector3(8718.88f, 6764.86f, 95.75f),
                                            new Vector3(9072.0f, 7158.0f, 53.04f)));

            // Purple Mid -> River Lane Bush
            _SafeWardSpots.Add(new WardSpot(new Vector3(8530.27f, 6637.38f, 46.98f),
                                            new Vector3(8539.27f, 6637.38f, 46.98f),
                                            new Vector3(8396.10f, 6464.81f, 46.98f),
                                            new Vector3(8779.17f, 6804.70f, 46.98f)));

            // Purple Bottom -> Solo Bush
            _SafeWardSpots.Add(new WardSpot(new Vector3(12422.0f, 4508.0f, 51.73f),
                                            new Vector3(12353.94f, 4031.58f, 51.73f),
                                            new Vector3(12023.9f, 3757.76f, -66.25f),
                                            new Vector3(12422.0f, 4508.0f, 51.73f)));

            // Purple Lizard -> Nashor Pass Bush // Inconsistent Placement
            _SafeWardSpots.Add(new WardSpot(new Vector3(6824.0f, 10656.0f, 56.0f),
                                            new Vector3(6370.69f, 10359.92f, 56.0f),
                                            new Vector3(6273.9f, 10307.76f, 53.67f),
                                            new Vector3(6824.0f, 10656.0f, 56.0f)));

            // Blue Golem -> Blue Lizard
            _SafeWardSpots.Add(new WardSpot(new Vector3(8272.0f, 2908.0f, 51.13f),
                                            new Vector3(8163.7056f, 3436.0476f, 51.13f),
                                            new Vector3(8163.71f, 3436.05f, 51.6628f),
                                            new Vector3(8272.0f, 2908.0f, 51.13f)));

            // Red Golem -> Red Lizard
            _SafeWardSpots.Add(new WardSpot(new Vector3(6574.0f, 12006.0f, 56.48f),
                                            new Vector3(6678.08f, 11477.83f, 56.48f),
                                            new Vector3(6678.08f, 11477.83f, 53.85f),
                                            new Vector3(6574.0f, 12006.0f, 56.48f)));

            // Blue Top Side Brush
            _SafeWardSpots.Add(new WardSpot(new Vector3(1774.0f, 10756.0f, 52.84f),
                                            new Vector3(2302.36f, 10874.22f, 52.84f),
                                            new Vector3(2773.9f, 11307.76f, -71.24f),
                                            new Vector3(1774.0f, 10756.0f, 52.84f)));

            // Mid Lane Death Brush
            _SafeWardSpots.Add(new WardSpot(new Vector3(5874.0f, 8306.0f, -70.12f),
                                            new Vector3(5332.9f, 8275.21f, -70.12f),
                                            new Vector3(5123.9f, 8457.76f, -21.23f),
                                            new Vector3(5874.0f, 8306.0f, -70.12f)));

            // Mid Lane Death Brush Right Side
            _SafeWardSpots.Add(new WardSpot(new Vector3(9022.0f, 6558.0f, 71.24f),
                                            new Vector3(9540.43f, 6657.68f, 71.24f),
                                            new Vector3(9773.9f, 6457.76f, 9.56f),
                                            new Vector3(9022.0f, 6558.0f, 71.24f)));

            // Blue Inner Turret Jungle
            _SafeWardSpots.Add(new WardSpot(new Vector3(6874.0f, 1708.0f, 50.52f),
                                            new Vector3(6849.11f, 2252.01f, 50.52f),
                                            new Vector3(6723.9f, 2507.76f, 52.17f),
                                            new Vector3(6874.0f, 1708.0f, 50.52f)));

            // Purple Inner Turret Jungle
            _SafeWardSpots.Add(new WardSpot(new Vector3(8122.0f, 13206.0f, 52.84f),
                                            new Vector3(8128.53f, 12658.41f, 52.84f),
                                            new Vector3(8323.9f, 12457.76f, 56.48f),
                                            new Vector3(8122.0f, 13206.0f, 52.84f)));
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

                if (IsOnScreen(screenPos[0], screenPos[1]) && ObjectManager.Player.Distance(wardPos) < PerfectWard.PerfectWardTracker.Config.Item("drawDistance").GetValue<Slider>().Value)
                {
                    Drawing.DrawCircle(wardPos, WARD_INDICATOR_RADIUS, wardColor);
                    //Console.WriteLine(wardPos.X);
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
                if (IsOnScreen(screenPos[0], screenPos[1]) && ObjectManager.Player.Distance(safeWardSpot.MagneticPosition) < PerfectWard.PerfectWardTracker.Config.Item("drawDistance").GetValue<Slider>().Value)
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
