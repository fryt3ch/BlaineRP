using RAGE;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.Data
{
    public class ShootingRange : Events.Script
    {
        public enum Types
        {
            Shop = 0,
            Army,
        }

        private static Dictionary<Types, Data> Ranges { get; set; } = new Dictionary<Types, Data>();

        public class Data
        {
            public Types Type { get; private set; }

            public Vector3[] TargetPositions { get; private set; }

            public Vector3 TargetRotation { get; private set; }

            public Data(Types Type, Vector3 TargetRotation, Vector3[] TargetPositions)
            {
                Ranges.Add(Type, this);
            }
        }

        public ShootingRange()
        {
            new Data(Types.Shop, new Vector3(-90f, 0f, 160f), new Vector3[]
            {
                new Vector3(10.96f, -1088.14f, 31.55f),
                new Vector3(11.88f, -1088.48f, 31.55f),
                new Vector3(12.82f, -1088.86f, 31.55f),
                new Vector3(13.77f, -1089.18f, 31.55f),
                new Vector3(14.69f, -1089.52f, 31.55f),
                new Vector3(15.64f, -1089.85f, 31.55f),
                new Vector3(16.58f, -1090.2f, 31.55f),
                new Vector3(17.51f, -1090.54f, 31.55f),
                new Vector3(18.48f, -1090.86f, 31.55f),
                new Vector3(19.42f, -1091.24f, 31.55f),
                new Vector3(20.33f, -1091.58f, 31.55f),
                new Vector3(21.33f, -1091.94f, 31.55f),
                new Vector3(14.1f, -1079.58f, 31.55f),
                new Vector3(15.02f, -1079.95f, 31.55f),
                new Vector3(15.96f, -1080.3f, 31.55f),
                new Vector3(16.9f, -1080.59f, 31.55f),
                new Vector3(17.83f, -1080.96f, 31.55f),
                new Vector3(18.76f, -1081.31f, 31.55f),
                new Vector3(19.71f, -1081.67f, 31.55f),
                new Vector3(20.63f, -1082.05f, 31.55f),
                new Vector3(21.59f, -1082.33f, 31.55f),
                new Vector3(22.52f, -1082.71f, 31.55f),
                new Vector3(23.47f, -1083.04f, 31.55f),
                new Vector3(24.42f, -1083.38f, 31.55f),
                new Vector3(17.84f, -1069.23f, 31.55f),
                new Vector3(18.79f, -1069.56f, 31.55f),
                new Vector3(19.73f, -1069.9f, 31.55f),
                new Vector3(20.67f, -1070.25f, 31.55f),
                new Vector3(21.61f, -1070.61f, 31.55f),
                new Vector3(22.54f, -1070.95f, 31.55f),
                new Vector3(23.48f, -1071.29f, 31.55f),
                new Vector3(24.42f, -1071.64f, 31.55f),
                new Vector3(25.37f, -1071.98f, 31.55f),
                new Vector3(26.3f, -1072.33f, 31.55f),
                new Vector3(27.24f, -1072.66f, 31.55f),
                new Vector3(28.17f, -1073.03f, 31.55f),
            });
        }
    }
}
