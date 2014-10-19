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


            //new
            _WardSpots.Add(new Vector3(9641.6591796875f, 6368.748046875f, 53.01416015625f));
            _WardSpots.Add(new Vector3(8081.4360351563f, 4683.443359375f, 55.9482421875f));
            _WardSpots.Add(new Vector3(5943.51953125f, 9792.4091796875f, 53.189331054688f));
            _WardSpots.Add(new Vector3(4379.513671875f, 8093.740234375f, 42.734619140625f));
            _WardSpots.Add(new Vector3(4222.724609375f, 7038.5805664063f, 53.612548828125f));
            _WardSpots.Add(new Vector3(9068.0224609375f, 11186.685546875f, 53.22705078125f));
            _WardSpots.Add(new Vector3(7970.822265625f, 10005.072265625f, 53.527709960938f));
            _WardSpots.Add(new Vector3(4978.1943359375f, 3042.6975097656f, 54.343017578125f));
            _WardSpots.Add(new Vector3(7907.6357421875f, 11629.322265625f, 49.947143554688f));
            _WardSpots.Add(new Vector3(7556.0654296875f, 11739.625f, 50.61547851625f));
            _WardSpots.Add(new Vector3(5973.4853515625f, 11115.6875f, 54.348999023438f));
            _WardSpots.Add(new Vector3(5732.8198242188f, 10289.76953125f, 53.397827148438f));
            //_WardSpots.Add(new Vector3(7969.15625f, 3307.5673828125f, 56.940795898438f));
            _WardSpots.Add(new Vector3(12073.184570313f, 4795.50390625f, 52.322265625f));
            _WardSpots.Add(new Vector3(4044.1313476563f, 11600.502929688f, 48.591918945313f));
            _WardSpots.Add(new Vector3(5597.6669921875f, 12491.047851563f, 39.739379882813f));
            _WardSpots.Add(new Vector3(10070.202148438f, 4132.4536132813f, -60.332153320313f));
            _WardSpots.Add(new Vector3(8320.2890625f, 4292.8090820313f, 56.473876953125f));
            _WardSpots.Add(new Vector3(9603.5205078125f, 7872.2368164063f, 54.713745117188f));

            //new2
            _WardSpots.Add(new Vector3(7812f, 5177f, 56f));
            //_WardSpots.Add(new Vector3(9623f, 6358f, 2f));
            _WardSpots.Add(new Vector3(11653f, 9408f, 50f));
            _WardSpots.Add(new Vector3(8748f, 2038f, 54f));
            _WardSpots.Add(new Vector3(7909f, 3282f, 56f));
            _WardSpots.Add(new Vector3(6222f, 2852f, 53f));
            _WardSpots.Add(new Vector3(6033f, 4484f, 51f));
            _WardSpots.Add(new Vector3(8274f, 4259f, 56f));
            //_WardSpots.Add(new Vector3(9127f, 5416f, -64f));
            _WardSpots.Add(new Vector3(5035f, 3233f, 54f));
            _WardSpots.Add(new Vector3(5314f, 3360f, 54f));
            _WardSpots.Add(new Vector3(2364f, 7234f, 54f));
            _WardSpots.Add(new Vector3(1917f, 9663f, 53f));
            _WardSpots.Add(new Vector3(2720f, 10591f, -63f));
            _WardSpots.Add(new Vector3(2519f, 11176f, -64f));
            _WardSpots.Add(new Vector3(5325f, 12470f, 39f));
            _WardSpots.Add(new Vector3(7989f, 6222f, -64f));
            _WardSpots.Add(new Vector3(7721f, 6026f, -64f));
            _WardSpots.Add(new Vector3(11748f, 1119f, 48f));
            _WardSpots.Add(new Vector3(6382f, 8300f, -63f));
            _WardSpots.Add(new Vector3(6105f, 8049f, -57f));
            _WardSpots.Add(new Vector3(4747f, 8954f, -63f));
            _WardSpots.Add(new Vector3(6372f, 11253f, 54f));
            _WardSpots.Add(new Vector3(6063f, 11148f, 54f));
            _WardSpots.Add(new Vector3(7531f, 11706f, 50f));
            _WardSpots.Add(new Vector3(7822f, 11537f, 49f));
            _WardSpots.Add(new Vector3(8634f, 11139f, 52f));
            _WardSpots.Add(new Vector3(9008f, 11194f, 55f));
            _WardSpots.Add(new Vector3(11631f, 7265f, 55f));
           // _WardSpots.Add(new Vector3(12197f, 4912f, 55f));
            _WardSpots.Add(new Vector3(11393f, 3835f, -54f));
            _WardSpots.Add(new Vector3(11483f, 3458f, -55f));
            _WardSpots.Add(new Vector3(5839f, 9843f, 53f));
            _WardSpots.Add(new Vector3(5733f, 10202f, 53f));
            _WardSpots.Add(new Vector3(966f, 12358f, 40f));
            _WardSpots.Add(new Vector3(1533f, 12934f, 34f));
            _WardSpots.Add(new Vector3(2296f, 13323f, 29f));
            _WardSpots.Add(new Vector3(2388f, 5078f, 55f));
            _WardSpots.Add(new Vector3(7965f, 10001f, 53f));
            _WardSpots.Add(new Vector3(12327f, 1565f, 48f));
            _WardSpots.Add(new Vector3(12775f, 2076f, 48f));
            _WardSpots.Add(new Vector3(13217f, 2793f, 48f));
            _WardSpots.Add(new Vector3(7022f, 7065f, 54f));
            _WardSpots.Add(new Vector3(6308f, 6591f, 55f));
            _WardSpots.Add(new Vector3(3982f, 4424f, 53f));
            _WardSpots.Add(new Vector3(5085f, 5437f, 54f));
            _WardSpots.Add(new Vector3(7714f, 7764f, 53f));
            _WardSpots.Add(new Vector3(6119f, 9442f, 55f));
            _WardSpots.Add(new Vector3(426f, 7946f, 47f));
            _WardSpots.Add(new Vector3(7417f, 659f, 52f));
            _WardSpots.Add(new Vector3(8978f, 9013f, 54f));
            _WardSpots.Add(new Vector3(10002f, 10141f, 51f));
            _WardSpots.Add(new Vector3(9237f, 13256f, 80f));
            _WardSpots.Add(new Vector3(13012f, 9449f, 50f));
            _WardSpots.Add(new Vector3(1109f, 5111f, 57f));
            _WardSpots.Add(new Vector3(4917f, 1240f, 53f));
            _WardSpots.Add(new Vector3(5550f, 1317f, 53f));
            _WardSpots.Add(new Vector3(8428f, 13214f, 46f));
            _WardSpots.Add(new Vector3(9909f, 11533f, 106f));
            _WardSpots.Add(new Vector3(9833f, 12533f, 106f));
            _WardSpots.Add(new Vector3(11448f, 10277f, 106f));
            _WardSpots.Add(new Vector3(12307f, 10041f, 106f));
            _WardSpots.Add(new Vector3(11418f, 11703f, 106f));
            _WardSpots.Add(new Vector3(1775f, 4411f, 108f));
            _WardSpots.Add(new Vector3(2643f, 4229f, 105f));
            _WardSpots.Add(new Vector3(4102f, 2895f, 110f));
            _WardSpots.Add(new Vector3(4206f, 1948f, 108f));
            _WardSpots.Add(new Vector3(2563f, 2717f, 130f));
            _WardSpots.Add(new Vector3(12039f, 1336f, 48f));
            _WardSpots.Add(new Vector3(13018f, 2433f, 48f));
            //_WardSpots.Add(new Vector3(8147f, 4666f, 55f));
            _WardSpots.Add(new Vector3(6495f, 2789f, 55f));
            _WardSpots.Add(new Vector3(7672f, 3211f, 54f));
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
