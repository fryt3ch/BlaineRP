using GTANetworkAPI;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BCRPServer.Game.Businesses
{
    public abstract partial class Business
    {
        public static int LoadAll()
        {
            #region Clothes (Cheap)
            new ClothesShop1(1, new Vector3(1201.013f, 2701.155f, 37.15381f), new Utils.Vector4(1201.885f, 2710.143f, 38.2226f, 105f));
            new ClothesShop1(3, new Vector3(-1096.97f, 2701.689f, 17.93724f), new Utils.Vector4(-1097.523f, 2714.026f, 19.108f, 150f));
            new ClothesShop1(4, new Vector3(1683.12f, 4822.236f, 41.03899f), new Utils.Vector4(1694.727f, 4817.582f, 42.06f, 20f));
            new ClothesShop1(5, new Vector3(-4.590637f, 6516.01f, 30.48112f), new Utils.Vector4(1f, 6508.753f, 31.87f, 325f));

            new ClothesShop1(11, new Vector3(-814.2472f, -1078.953f, 10.13255f), new Utils.Vector4(-817.8808f, -1070.944f, 11.32811f, 135f));
            new ClothesShop1(12, new Vector3(84.3979f, -1388.727f, 28.41426f), new Utils.Vector4(75.42346f, -1387.689f, 29.37614f, 195.8f));
            new ClothesShop1(13, new Vector3(416.1883f, -810.5462f, 28.33503f), new Utils.Vector4(425.6321f, -811.4822f, 29.49114f, 11f));
            #endregion

            #region Clothes (Expensive)
            new ClothesShop2(2, new Vector3(623.3489f, 2744.308f, 41.01938f), new Utils.Vector4(613.035f, 2761.843f, 42.088f, 265f));
            new ClothesShop2(14, new Vector3(-3168.294f, 1062.636f, 19.84479f), new Utils.Vector4(-3169.008f, 1044.211f, 20.86322f, 48.6f));

            new ClothesShop2(9, new Vector3(126.1404f, -203.9787f, 53.53281f), new Utils.Vector4(127.3073f, -223.18f, 54.55783f, 66f));
            new ClothesShop2(10, new Vector3(-1203.813f, -784.1447f, 16.05554f), new Utils.Vector4(-1194.725f, -767.6141f, 17.31629f, 208f));

            #endregion

            #region Clothes (Brand)
            new ClothesShop3(6, new Vector3(-1459.713f, -233.0381f, 48.45752f), new Utils.Vector4(-1448.824f, -237.893f, 49.81332f, 45f));
            new ClothesShop3(7, new Vector3(-717.7559f, -159.8642f, 35.99546f), new Utils.Vector4(-708.95f, -151.6612f, 37.415f, 114f));
            new ClothesShop3(8, new Vector3(-152.0491f, -303.7605f, 37.95464f), new Utils.Vector4(-165f, -303.2f, 39.73328f, 251f));
            #endregion

            new Market(15, new Vector3(546.4489f, 2675.079f, 41.16602f), new Utils.Vector4(549.1185f, 2671.407f, 42.1565f, 91.55f));

            new GasStation(16, new Vector3(270.1317f, 2601.239f, 43.64737f), null, new Vector3(263.9698f, 2607.402f, 44.98298f));

            new CarShop1(17, new Vector3(-62.48621f, -1089.3f, 25.69341f), new Utils.Vector4(-55.08611f, -1111.217f, 26.05543f, 36.2f), new Utils.Vector4[] { new Utils.Vector4(-41.65706f, -1116.344f, 26.05584f, 3f), new Utils.Vector4(-45.15728f, -1116.411f, 26.05584f, 3f), new Utils.Vector4(-47.71569f, -1116.379f, 26.05584f, 3f), new Utils.Vector4(-50.56787f, -1116.191f, 26.05584f, 3f), new Utils.Vector4(-53.62245f, -1116.565f, 26.05584f, 3f), new Utils.Vector4(-56.34209f, -1116.566f, 26.05584f, 3f), new Utils.Vector4(-59.11841f, -1116.814f, 26.05584f, 3f), new Utils.Vector4(-62.03639f, -1117.178f, 26.05584f, 3f) }, new Utils.Vector4(-54.68786f, -1088.418f, 26.42234f, 155.8f));

            new BoatShop(18, new Vector3(-813.3688f, -1336.428f, 4.150263f), new Utils.Vector4(-852.8972f, -1335.998f, 0.1195435f, 108.4271f), new Utils.Vector4[] { }, new Utils.Vector4(-813.8713f, -1343.797f, 5.150264f, 49.62344f));

            new AeroShop(19, new Vector3(1757.495f, 3239.969f, 40.94524f), new Utils.Vector4(1770.4f, 3239.908f, 42.02776f, 352.3067f), new Utils.Vector4[] { }, new Utils.Vector4(1760.724f, 3234.819f, 42.13989f, 314.5554f));

            new TuningShop(20, new Vector3(1178.526f, 2647.779f, 36.79328f), new Utils.Vector4(1175.327f, 2639.85f, 37.3765f, 325f), new Utils.Vector4[] { new Utils.Vector4(1188.023f, 2650.051f, 37.46183f, 50f), new Utils.Vector4(1188.778f, 2658.188f, 37.44228f, 315f) }, new Utils.Vector4(1175.327f, 2639.85f, 37.3765f));
            new TuningShop(21, new Vector3(-356.59f, -129.5743f, 38.43067f), new Utils.Vector4(-338.4644f, -136.1622f, 38.62881f, 115f), new Utils.Vector4[] { new Utils.Vector4(-356.0863f, -115.5013f, 38.31553f, 72f), new Utils.Vector4(-377.5053f, -146.7845f, 38.30297f, 300f), new Utils.Vector4(-378.845f, -143.6312f, 38.30399f, 300f), new Utils.Vector4(-380.2281f, -140.2031f, 38.30404f, 300f), new Utils.Vector4(-382.0588f, -137.2818f, 38.30477f, 300f), new Utils.Vector4(-384.2779f, -134.4641f, 38.30402f, 300f), new Utils.Vector4(-385.8221f, -131.2085f, 38.30363f, 300f), new Utils.Vector4(-387.4566f, -128.2165f, 38.30125f, 300f), new Utils.Vector4(-388.955f, -124.9111f, 38.30383f, 300f), new Utils.Vector4(-390.6784f, -121.6463f, 38.29424f, 300f), new Utils.Vector4(-392.3392f, -118.4143f, 38.21078f, 300f) }, new Utils.Vector4(-338.4644f, -136.1622f, 38.62881f));
            new TuningShop(22, new Vector3(720.2494f, -1082.472f, 21.25672f), new Utils.Vector4(732.6448f, -1088.88f, 21.78887f, 140f), new Utils.Vector4[] { new Utils.Vector4(719.222f, -1079.078f, 21.85986f, 95f), new Utils.Vector4(705.7102f, -1071.86f, 22.03793f, 270f), new Utils.Vector4(705.0659f, -1061.33f, 22.0562f, 270f), new Utils.Vector4(706.761f, -1053.564f, 21.96827f, 250f) }, new Utils.Vector4(732.6448f, -1088.88f, 21.78887f));
            new TuningShop(23, new Vector3(-1139.816f, -1992.655f, 12.16545f), new Utils.Vector4(-1154.301f, -2006.171f, 12.79945f, 0f), new Utils.Vector4[] { new Utils.Vector4(-1151.391f, -1982.481f, 12.77934f, 280f), new Utils.Vector4(-1136.405f, -1975.773f, 12.78101f, 180f), new Utils.Vector4(-1132.026f, -1975.115f, 12.78097f, 180f), new Utils.Vector4(-1127.017f, -1976.082f, 12.79133f, 180f) }, new Utils.Vector4(-1154.301f, -2006.171f, 12.79945f));

            new WeaponShop(24, new Vector3(-1109.188f, 2690.507f, 17.6103f), new Utils.Vector4(-1118.7f, 2699.981f, 18.55415f, 218.2682f), new Utils.Vector4(-1122.019f, 2696.923f, 18.55415f, 305.8211f));

            new BarberShop(25, new Vector3(-285.7959f, 6232.987f, 30.49523f), new Utils.Vector4(-277.8783f, 6230.489f, 31.69553f, 42.97519f), new Utils.Vector4(-278.4798f, 6227.705f, 31.69553f, 354.1428f));
            new BarberShop(26, new Vector3(1933.23f, 3721.24f, 31.86353f), new Utils.Vector4(1930.866f, 3728.215f, 32.84443f, 212.5517f), new Utils.Vector4(-278.4798f, 6227.705f, 31.69553f, 354.1428f));
            new BarberShop(27, new Vector3(-827.2922f, -186.6121f, 36.63704f), new Utils.Vector4(-822.0836f, -183.4575f, 37.56893f, 202.6368f), new Utils.Vector4(-815.0012f, -183.6297f, 37.56892f, 166.9111f));
            new BarberShop(28, new Vector3(-31.65811f, -144.1567f, 56.07059f), new Utils.Vector4(-30.80062f, -151.6473f, 57.0765f, 337.2878f), new Utils.Vector4(-278.4798f, 6227.705f, 31.69553f, 354.1428f));
            new BarberShop(29, new Vector3(1203.78f, -472.2391f, 65.20185f), new Utils.Vector4(1211.432f, -470.6421f, 66.20801f, 71.84773f), new Utils.Vector4(-278.4798f, 6227.705f, 31.69553f, 354.1428f));
            new BarberShop(30, new Vector3(-1291.942f, -1119.263f, 5.557382f), new Utils.Vector4(-1284.172f, -1115.377f, 6.990112f, 85.004921f), new Utils.Vector4(-278.4798f, 6227.705f, 31.69553f, 354.1428f));
            new BarberShop(31, new Vector3(129.2226f, -1712.456f, 28.25804f), new Utils.Vector4(134.6306f, -1707.976f, 29.29162f, 139.164f), new Utils.Vector4(-278.4798f, 6227.705f, 31.69553f, 354.1428f));

            new BagShop(32, null, new Utils.Vector4(1929.689f, 4635.728f, 40.44981f, 344.2735f), new Utils.Vector4(1928.622f, 4642.992f, 40.41078f, 197.1759f)); // new Vector3(1925.026f, 4637.98f, 39.16909f)

            new MaskShop(33, null, new Utils.Vector4(-1334.991f, -1277.063f, 4.96355f, 114.1392f), new Utils.Vector4(-1342.1f, -1277.109f, 4.89769f, 245.6555f)); // new Vector3(-1334.951f, -1283.553f, 3.835981f)

            new JewelleryShop(34, new Vector3(-630.8311f, -241.3816f, 37.16668f), new Utils.Vector4(-623.4676f, -230.2429f, 38.05705f, 123.827f), new Utils.Vector4(-619.3085f, -228.7722f, 38.05704f, 127.965f));

            new TattooShop(35, new Vector3(-286.4024f, 6199.159f, 30.45244f), new Utils.Vector4(-292.0363f, 6199.72f, 31.48712f, 222.5584f), new Utils.Vector4(-293.9031f, 6200.241f, 31.48711f, 186.6973f));
            new TattooShop(36, new Vector3(-3165.543f, 1075.181f, 20.84793f), new Utils.Vector4(-3170.584f, 1073.112f, 20.82918f, 335.5721f), new Utils.Vector4(-3169.231f, 1077.034f, 20.82918f, 194.2503f));

            var lines = new List<string>();

            foreach (var x in All.Values)
            {
                MySQL.LoadBusiness(x);

                var daysDiff = CurrentStatisticsDayIdx - PreviousStatisticsDayIdx;

                for (int j = 0; j < daysDiff; j++)
                {
                    for (int i = 0; i < x.Statistics.Length; i++)
                    {
                        if (i == x.Statistics.Length - 1)
                        {
                            x.Statistics[i] = 0;
                        }
                        else
                        {
                            x.Statistics[i] = x.Statistics[i + 1];
                        }
                    }
                }

                lines.Add($"new {x.GetType().Name}({x.ClientData});");
            }

            Utils.FillFileToReplaceRegion(Settings.DIR_CLIENT_LOCATIONS_DATA_PATH, "BIZS_TO_REPLACE", lines);

            var ns = typeof(Business).Namespace;

            foreach (var x in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == ns && t.IsClass && !t.IsAbstract && typeof(Shop).IsAssignableFrom(t)))
            {
                var matData = (MaterialsData)x.GetProperty("InitMaterialsData")?.GetValue(null);

                if (matData == null)
                    continue;

                var shopType = (Types?)x.GetProperty("DefaultType")?.GetValue(null);

                if (shopType is Types shopTypeN)
                {
                    Shop.AllPrices.Add(shopTypeN, matData);
                }
            }

            lines = new List<string>();

            foreach (var x in Shop.AllPrices)
            {
                lines.Add($"Prices.Add(Types.{x.Key}, new Dictionary<string, int>() {{{string.Join(", ", x.Value.Prices.Select(y => $"{{\"{y.Key}\", {y.Value * x.Value.RealPrice}}}"))}}});");
            }

            Utils.FillFileToReplaceRegion(Settings.DIR_CLIENT_SHOP_DATA_PATH, "TO_REPLACE", lines);

            WeaponShop.ShootingRangePrice = 150;

            return All.Count;
        }
    }
}
