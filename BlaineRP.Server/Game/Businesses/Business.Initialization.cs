using GTANetworkAPI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Businesses
{
    public abstract partial class Business
    {
        public static void LoadPrices()
        {
            var ns = typeof(Business).Namespace;

            foreach (var x in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == ns && t.IsClass && !t.IsAbstract)) // && typeof(Shop).IsAssignableFrom(t)
            {
                var matData = (MaterialsData)x.GetProperty("InitMaterialsData")?.GetValue(null);

                if (matData == null)
                    continue;

                var shopType = (BusinessType?)x.GetProperty("DefaultType")?.GetValue(null);

                if (shopType is BusinessType shopTypeN)
                {
                    Shop.AllPrices.Add(shopTypeN, matData);
                }
            }

            var lines = new List<string>();

            foreach (var x in Shop.AllPrices)
            {
                lines.Add($"Prices.Add(Game.Businesses.BusinessType.{x.Key}, new Dictionary<string, uint>() {{{string.Join(", ", x.Value.Prices.Select(y => $"{{\"{y.Key}\", {y.Value * x.Value.RealPrice}}}"))}}});");
            }

            Utils.FillFileToReplaceRegion(Directory.GetCurrentDirectory() + Properties.Settings.Static.ClientScriptsTargetPath + @"\Game\UI\CEF\Shop.cs", "TO_REPLACE", lines);
        }

        public static int LoadAll()
        {
            new ClothesShop1(1, new Vector3(1201.013f, 2701.155f, 38.15381f), new Vector4(1201.885f, 2710.143f, 38.2226f, 105f));
            new ClothesShop1(3, new Vector3(-1096.97f, 2701.689f, 18.93724f), new Vector4(-1097.523f, 2714.026f, 19.108f, 150f));
            new ClothesShop1(4, new Vector3(1683.12f, 4822.236f, 42.03899f), new Vector4(1694.727f, 4817.582f, 42.06f, 20f));
            new ClothesShop1(5, new Vector3(-4.590637f, 6516.01f, 31.48112f), new Vector4(1f, 6508.753f, 31.87f, 325f));
            new ClothesShop1(11, new Vector3(-814.2472f, -1078.953f, 11.13255f), new Vector4(-817.8808f, -1070.944f, 11.32811f, 135f));
            new ClothesShop1(12, new Vector3(84.3979f, -1388.727f, 29.41426f), new Vector4(75.42346f, -1387.689f, 29.37614f, 195.8f));
            new ClothesShop1(13, new Vector3(416.1883f, -810.5462f, 29.33503f), new Vector4(425.6321f, -811.4822f, 29.49114f, 11f));

            new ClothesShop2(2, new Vector3(623.3489f, 2744.308f, 42.01938f), new Vector4(613.035f, 2761.843f, 42.088f, 265f));
            new ClothesShop2(14, new Vector3(-3168.294f, 1062.636f, 20.84479f), new Vector4(-3169.008f, 1044.211f, 20.86322f, 48.6f));
            new ClothesShop2(9, new Vector3(126.1404f, -203.9787f, 54.53281f), new Vector4(127.3073f, -223.18f, 54.55783f, 66f));
            new ClothesShop2(10, new Vector3(-1203.813f, -784.1447f, 17.05554f), new Vector4(-1194.725f, -767.6141f, 17.31629f, 208f));

            new ClothesShop3(6, new Vector3(-1459.713f, -233.0381f, 49.45752f), new Vector4(-1448.824f, -237.893f, 49.81332f, 45f));
            new ClothesShop3(7, new Vector3(-717.7559f, -159.8642f, 36.99546f), new Vector4(-708.95f, -151.6612f, 37.415f, 114f));
            new ClothesShop3(8, new Vector3(-152.0491f, -303.7605f, 38.95464f), new Vector4(-165f, -303.2f, 39.73328f, 251f));

            new Market(15, new Vector3(546.4489f, 2675.079f, 42.16602f), new Vector4(549.1185f, 2671.407f, 42.1565f, 91.55f));
            new Market(49, new Vector3(26.42344f, -1350.762f, 29.33121f), new Vector4(24.50421f, -1347.159f, 29.49702f, 268.0024f));
            new Market(50, new Vector3(1732.894f, 6409.292f, 35.00063f), new Vector4(1727.906f, 6415.247f, 35.03723f, 240.8585f));
            new Market(51, new Vector3(2685.241f, 3284.355f, 55.24056f), new Vector4(2678.03f, 3279.434f, 55.24113f, 329.2751f));
            new Market(52, new Vector3(1967.934f, 3740.245f, 32.35117f), new Vector4(1960.05f, 3740.059f, 32.34375f, 296.5273f));
            new Market(53, new Vector3(378.6611f, 321.5639f, 103.3893f), new Vector4(372.5879f, 326.392f, 103.5664f, 250.3989f));
            new Market(54, new Vector3(-3038.023f, 592.4523f, 7.817484f), new Vector4(-3038.89f, 584.5623f, 7.908929f, 14.47209f));
            new Market(55, new Vector3(-3237.194f, 1006.861f, 12.40062f), new Vector4(-3242.218f, 1000.011f, 12.83071f, 353.0931f));
            new Market(56, new Vector3(2561.556f, 382.956f, 108.6208f), new Vector4(2557.244f, 380.8391f, 108.623f, 354.6382f));
            new Market(58, new Vector3(-55.66323f, -1755.771f, 29.43963f), new Vector4(-46.66877f, -1757.868f, 29.421f, 43.06324f));
            new Market(59, new Vector3(-709.1494f, -917.5579f, 19.21441f), new Vector4(-706.1553f, -913.4304f, 19.21559f, 85.40551f));
            new Market(60, new Vector3(1699.463f, 4932.005f, 42.07813f), new Vector4(1698.205f, 4922.841f, 42.06368f, 321.726f));
            new Market(61, new Vector3(1162.182f, -327.2775f, 69.21461f), new Vector4(1164.647f, -322.5854f, 69.20515f, 92.57941f));
            new Market(62, new Vector3(-1819.677f, 789.2336f, 138.1425f), new Vector4(-1820.276f, 794.3466f, 138.0898f, 125.2652f));

            new Market(63, new Vector3(-1229.439f, -902.624f, 12.19648f), new Vector4(-1221.97f, -908.2956f, 12.32635f, 29.60223f));
            new Market(64, new Vector3(1142.673f, -979.2043f, 46.28062f), new Vector4(1134.253f, -982.5103f, 46.4158f, 275.2812f));
            new Market(65, new Vector3(-1490.807f, -386.128f, 39.67329f), new Vector4(-1486.285f, -378.0215f, 40.16339f, 132.633f));
            new Market(66, new Vector3(1168.038f, 2702.137f, 38.17805f), new Vector4(1165.925f, 2710.803f, 38.15771f, 175.9661f));
            new Market(67, new Vector3(-2975.207f, 389.1467f, 15.02665f), new Vector4(-2966.424f, 390.8955f, 15.04331f, 84.64081f));

            new GasStation(16, new Vector3(270.1317f, 2601.239f, 44.64737f), null, new Vector4(263.9698f, 2607.402f, 44.98298f, 7.5f));
            new GasStation(68, new Vector3(1698.498f, 6425.668f, 32.76406f), null, new Vector4(1701.724f, 6416.483f, 32.76001f, 10f));
            new GasStation(69, new Vector3(1676.205f, 4923.622f, 42.05463f), null, new Vector4(1686.895f, 4929.302f, 42.0781f, 7.5f));
            new GasStation(70, new Vector3(2671.806f, 3259.376f, 55.24052f), null, new Vector4(2679.951f, 3264.098f, 55.24052f, 7.5f));
            new GasStation(71, new Vector3(2591.919f, 351.4034f, 108.4573f), null, new Vector4(2581.307f, 361.7208f, 108.4688f, 13f));
            new GasStation(72, new Vector3(45.4535f, 2788.245f, 57.87816f), null, new Vector4(49.50636f, 2778.856f, 58.04395f, 7.5f));
            new GasStation(73, new Vector3(1203.327f, 2656.537f, 37.8521f), null, new Vector4(1208.081f, 2660.416f, 37.90003f, 7.5f));
            new GasStation(74, new Vector3(-2567.018f, 2344.798f, 33.07035f), null, new Vector4(-2555.233f, 2334.28f, 33.07805f, 13f));
            new GasStation(75, new Vector3(-1788.858f, 812.7391f, 138.4958f), null, new Vector4(-1800.013f, 802.8375f, 138.6524f, 13f));
            new GasStation(76, new Vector3(626.5507f, 249.8059f, 103.0407f), null, new Vector4(620.7009f, 268.8442f, 103.0894f, 13f));
            new GasStation(77, new Vector3(1187.193f, -313.6362f, 69.18339f), null, new Vector4(1180.661f, -330.5511f, 69.31657f, 13f));
            new GasStation(78, new Vector3(-2106.312f, -334.4171f, 13.01274f), null, new Vector4(-2096.814f, -320.1179f, 13.16092f, 13f));
            new GasStation(79, new Vector3(-1423.172f, -287.5374f, 46.2537f), null, new Vector4(-1436.963f, -276.8523f, 46.20766f, 12.5f));
            new GasStation(80, new Vector3(168.2355f, 6594.022f, 31.84759f), null, new Vector4(179.9487f, 6602.907f, 31.86819f, 13f));
            new GasStation(81, new Vector3(-226.9775f, 6184.625f, 31.4902f), null, new Vector4(-217.702f, 6200.958f, 31.48996f, 7.5f));
            new GasStation(82, new Vector3(-734.1768f, -946.4086f, 18.53576f), null, new Vector4(-723.9554f, -936.2401f, 19.89023f - 0.5f, 12.5f));
            new GasStation(83, new Vector3(-305.5214f, -1486.814f, 30.38564f), null, new Vector4(-319.5534f, -1471.608f, 30.5486f, 12.5f));
            new GasStation(84, new Vector3(809.7831f, -1043.675f, 26.57383f), null, new Vector4(818.9824f, -1027.963f, 26.40432f, 12.5f));
            new GasStation(85, new Vector3(273.7592f, -1276.649f, 29.23184f), null, new Vector4(265.0627f, -1261.298f, 29.29272f, 12.5f));
            new GasStation(86, new Vector3(-64.82754f, -1775.309f, 28.89548f), null, new Vector4(-70.46722f, -1761.065f, 29.53409f, 12.5f));
            new GasStation(87, new Vector3(1219.133f, -1391.201f, 35.1943f), null, new Vector4(1208.479f, -1402.744f, 35.22414f, 10f));

            new CarShop1(17, new Vector3(-62.48621f, -1089.3f, 26.69341f), new Vector4(-55.08611f, -1111.217f, 26.05543f, 36.2f), new Vector4[] { new Vector4(-41.65706f, -1116.344f, 26.05584f, 3f), new Vector4(-45.15728f, -1116.411f, 26.05584f, 3f), new Vector4(-47.71569f, -1116.379f, 26.05584f, 3f), new Vector4(-50.56787f, -1116.191f, 26.05584f, 3f), new Vector4(-53.62245f, -1116.565f, 26.05584f, 3f), new Vector4(-56.34209f, -1116.566f, 26.05584f, 3f), new Vector4(-59.11841f, -1116.814f, 26.05584f, 3f), new Vector4(-62.03639f, -1117.178f, 26.05584f, 3f) }, new Vector4(-54.68786f, -1088.418f, 26.42234f, 155.8f));

            new BoatShop(18, new Vector3(-813.3688f, -1336.428f, 5.150263f), new Vector4(-852.8972f, -1335.998f, 0.1195435f, 108.4271f), new Vector4[] { }, new Vector4(-813.8713f, -1343.797f, 5.150264f, 49.62344f));

            new AeroShop(19, new Vector3(1757.495f, 3239.969f, 41.94524f), new Vector4(1770.4f, 3239.908f, 42.02776f, 352.3067f), new Vector4[] { }, new Vector4(1760.724f, 3234.819f, 42.13989f, 314.5554f));

            new TuningShop(20, new Vector3(1178.526f, 2647.779f, 37.79328f), new Vector4(1175.327f, 2639.85f, 37.3765f, 325f), new Vector4[] { new Vector4(1188.023f, 2650.051f, 37.46183f, 50f), new Vector4(1188.778f, 2658.188f, 37.44228f, 315f) }, new Vector4(1175.327f, 2639.85f, 37.3765f));
            new TuningShop(21, new Vector3(-356.59f, -129.5743f, 39.43067f), new Vector4(-338.4644f, -136.1622f, 38.62881f, 115f), new Vector4[] { new Vector4(-356.0863f, -115.5013f, 38.31553f, 72f), new Vector4(-377.5053f, -146.7845f, 38.30297f, 300f), new Vector4(-378.845f, -143.6312f, 38.30399f, 300f), new Vector4(-380.2281f, -140.2031f, 38.30404f, 300f), new Vector4(-382.0588f, -137.2818f, 38.30477f, 300f), new Vector4(-384.2779f, -134.4641f, 38.30402f, 300f), new Vector4(-385.8221f, -131.2085f, 38.30363f, 300f), new Vector4(-387.4566f, -128.2165f, 38.30125f, 300f), new Vector4(-388.955f, -124.9111f, 38.30383f, 300f), new Vector4(-390.6784f, -121.6463f, 38.29424f, 300f), new Vector4(-392.3392f, -118.4143f, 38.21078f, 300f) }, new Vector4(-338.4644f, -136.1622f, 38.62881f));
            new TuningShop(22, new Vector3(720.2494f, -1082.472f, 22.25672f), new Vector4(732.6448f, -1088.88f, 21.78887f, 140f), new Vector4[] { new Vector4(719.222f, -1079.078f, 21.85986f, 95f), new Vector4(705.7102f, -1071.86f, 22.03793f, 270f), new Vector4(705.0659f, -1061.33f, 22.0562f, 270f), new Vector4(706.761f, -1053.564f, 21.96827f, 250f) }, new Vector4(732.6448f, -1088.88f, 21.78887f));
            new TuningShop(23, new Vector3(-1139.816f, -1992.655f, 13.16545f), new Vector4(-1154.301f, -2006.171f, 12.79945f, 0f), new Vector4[] { new Vector4(-1151.391f, -1982.481f, 12.77934f, 280f), new Vector4(-1136.405f, -1975.773f, 12.78101f, 180f), new Vector4(-1132.026f, -1975.115f, 12.78097f, 180f), new Vector4(-1127.017f, -1976.082f, 12.79133f, 180f) }, new Vector4(-1154.301f, -2006.171f, 12.79945f));

            new WeaponShop(24, new Vector3(-1109.188f, 2690.507f, 18.6103f), new Vector4(-1118.7f, 2699.981f, 18.55415f, 218.2682f), new Vector4(-1122.019f, 2696.923f, 18.55415f, 305.8211f));
            new WeaponShop(40, new Vector3(-321.92f, 6077.066f, 31.23439f), new Vector4(-331.4013f, 6085.207f, 31.45479f, 219.7941f), new Vector4(-334.3136f, 6082.133f, 31.45479f));
            new WeaponShop(41, new Vector3(1698.355f, 3749.416f, 34.38319f), new Vector4(1692.568f, 3761.267f, 34.70531f, 220.4062f), new Vector4(1689.685f, 3757.973f, 34.70531f, 313.7246f));
            new WeaponShop(42, new Vector3(-3161.872f, 1084.455f, 20.84778f), new Vector4(-3173.364f, 1088.678f, 20.83875f, 245.0422f), new Vector4(-3174.961f, 1084.755f, 20.83875f, 335.8448f));
            new WeaponShop(43, new Vector3(243.5876f, -42.53535f, 69.89652f), new Vector4(253.699f, -50.85014f, 69.94106f, 63.67846f), new Vector4(255.0537f, -46.73441f, 69.94106f, 156.473f));
            new WeaponShop(44, new Vector3(846.4146f, -1020.743f, 27.53313f), new Vector4(842.0754f, -1035.25f, 28.19486f, 353.1079f), new Vector4(846.3333f, -1035.166f, 28.29122f, 85.9926f));
            new WeaponShop(45, new Vector3(-666.7805f, -946.7999f, 21.53879f), new Vector4(-661.8839f, -933.6004f, 21.82923f, 175.6398f), new Vector4(-666.3051f, -933.7073f, 21.82923f, 263.6278f));
            new WeaponShop(57, new Vector3(2566.838f, 305.1818f, 109.6083f), new Vector4(2567.377f, 292.6247f, 109.7349f, 355.6926f), new Vector4(2572.017f, 292.7495f, 109.7349f, 84.82509f));

            new BarberShop(25, new Vector3(-285.7959f, 6232.987f, 31.49523f), new Vector4(-277.8783f, 6230.489f, 31.69553f, 42.97519f), new Vector4(-278.4798f, 6227.705f, 31.69553f, 354.1428f));
            new BarberShop(26, new Vector3(1933.23f, 3721.24f, 32.86353f), new Vector4(1930.866f, 3728.215f, 32.84443f, 212.5517f), new Vector4(-278.4798f, 6227.705f, 31.69553f, 354.1428f));
            new BarberShop(27, new Vector3(-827.2922f, -186.6121f, 37.63704f), new Vector4(-822.0836f, -183.4575f, 37.56893f, 202.6368f), new Vector4(-815.0012f, -183.6297f, 37.56892f, 166.9111f));
            new BarberShop(28, new Vector3(-31.65811f, -144.1567f, 57.07059f), new Vector4(-30.80062f, -151.6473f, 57.0765f, 337.2878f), new Vector4(-278.4798f, 6227.705f, 31.69553f, 354.1428f));
            new BarberShop(29, new Vector3(1203.78f, -472.2391f, 66.20185f), new Vector4(1211.432f, -470.6421f, 66.20801f, 71.84773f), new Vector4(-278.4798f, 6227.705f, 31.69553f, 354.1428f));
            new BarberShop(30, new Vector3(-1291.942f, -1119.263f, 6.557382f), new Vector4(-1284.172f, -1115.377f, 6.990112f, 85.004921f), new Vector4(-278.4798f, 6227.705f, 31.69553f, 354.1428f));
            new BarberShop(31, new Vector3(129.2226f, -1712.456f, 29.25804f), new Vector4(134.6306f, -1707.976f, 29.29162f, 139.164f), new Vector4(-278.4798f, 6227.705f, 31.69553f, 354.1428f));

            new BagShop(32, null, new Vector4(1929.689f, 4635.728f, 40.44981f, 344.2735f), new Vector4(1928.622f, 4642.992f, 40.41078f, 197.1759f)); // new Vector3(1925.026f, 4637.98f, 39.16909f)

            new MaskShop(33, null, new Vector4(-1334.991f, -1277.063f, 4.96355f, 114.1392f), new Vector4(-1342.1f, -1277.109f, 4.89769f, 245.6555f)); // new Vector3(-1334.951f, -1283.553f, 3.835981f)

            new JewelleryShop(34, new Vector3(-630.8311f, -241.3816f, 38.16668f), new Vector4(-623.4676f, -230.2429f, 38.05705f, 123.827f), new Vector4(-619.3085f, -228.7722f, 38.05704f, 127.965f));

            new TattooShop(35, new Vector3(-286.4024f, 6199.159f, 31.45244f), new Vector4(-292.0363f, 6199.72f, 31.48712f, 222.5584f), new Vector4(-293.9031f, 6200.241f, 31.48711f, 186.6973f));
            new TattooShop(36, new Vector3(-3165.543f, 1075.181f, 20.84793f), new Vector4(-3170.584f, 1073.112f, 20.82918f, 335.5721f), new Vector4(-3169.231f, 1077.034f, 20.82918f, 194.2503f));
            new TattooShop(39, new Vector3(1857.347f, 3750.072f, 33.07676f), new Vector4(1862.596f, 3748.521f, 33.03189f, 25.12906f), new Vector4(1864.448f, 3747.398f, 33.03189f, 11.68702f));
            new TattooShop(46, new Vector3(322.6399f, 176.0838f, 103.576f), new Vector4(319.8488f, 180.9351f, 103.5865f, 248.9856f), new Vector4(323.457f, 179.829f, 103.5865f, 46.61205f));
            new TattooShop(47, new Vector3(-1157.496f, -1423.657f, 4.730599f), new Vector4(-1152.126f, -1423.978f, 4.954461f, 120.8584f), new Vector4(-1154.998f, -1426.173f, 4.954462f, 289.2736f));
            new TattooShop(48, new Vector3(1322.572f, -1647.34f, 52.14555f), new Vector4(1324.569f, -1650.291f, 52.2751f, 125.5188f), new Vector4(1321.871f, -1652.73f, 52.27516f, 291.6809f));

            new FurnitureShop(37, null, new Vector4(2738.052f, 3461.774f, 55.69563f, 336.6192f));

            new Farm(38, new Vector3(2036.85f, 4984.79f, 40.43526f), new Vector4(2020.814f, 4970.569f, 41.30896f, 266.771f))
            {
                CropFields = new List<Farm.CropField>()
                {
                    new Farm.CropField(Farm.CropField.Types.Pumpkin, 40.09598f, new Vector2(2.16f, -2.16f),
                        new (Vector2, byte)[]
                        {
                            (new Vector2(2019.139f, 4939.039f), 11),
                            (new Vector2(2020.566f, 4940.467f), 11),
                            (new Vector2(2022.273f, 4942.173f), 11),
                            (new Vector2(2023.703f, 4943.604f), 11),
                            (new Vector2(2025.108f, 4945.008f), 11),
                            (new Vector2(2026.458f, 4946.359f), 11),
                            (new Vector2(2027.92f, 4947.82f), 11),
                            (new Vector2(2029.353f, 4949.253f), 11),
                            (new Vector2(2030.774f, 4950.675f), 11),
                            (new Vector2(2032.135f, 4952.036f), 11),
                            (new Vector2(2033.581f, 4953.481f), 11),
                            (new Vector2(2035.001f, 4954.901f), 11),
                            (new Vector2(2036.404f, 4956.305f), 11),
                            (new Vector2(2037.807f, 4957.707f), 11),
                            (new Vector2(2039.229f, 4959.129f), 11),
                            (new Vector2(2040.655f, 4960.555f), 11),
                            (new Vector2(2042.088f, 4961.988f), 11),
                            (new Vector2(2043.435f, 4963.335f), 11),
                            (new Vector2(2044.891f, 4964.791f), 11),
                            (new Vector2(2046.293f, 4966.193f), 11),
                        })
                    {
                        IrrigationPoints = new List<Vector3>()
                        {
                            new Vector3(2060.191f, 4950.752f, 125f),
                            new Vector3(2051.832f, 4942.393f, 125f),
                            new Vector3(2043.281f, 4933.842f, 125f),
                            new Vector3(2034.596f, 4925.157f, 125f),
                        }
                    },

                    new Farm.CropField(Farm.CropField.Types.Cabbage, 40.09598f, new Vector2(2.16f, -2.16f),
                        new (Vector2, byte)[]
                        {
                            (new Vector2(2046.209f, 4912.653f), 11),
                            (new Vector2(2047.677f, 4914.121f), 11),
                            (new Vector2(2049.343f, 4915.786f), 11),
                            (new Vector2(2050.742f, 4917.186f), 11),
                            (new Vector2(2052.181f, 4918.625f), 11),
                            (new Vector2(2053.558f, 4920.002f), 11),
                            (new Vector2(2054.976f, 4921.42f), 11),
                            (new Vector2(2056.419f, 4922.864f), 11),
                            (new Vector2(2057.854f, 4924.298f), 11),
                            (new Vector2(2059.196f, 4925.641f), 11),
                            (new Vector2(2060.621f, 4927.065f), 11),
                            (new Vector2(2062.071f, 4928.516f), 11),
                            (new Vector2(2063.497f, 4929.941f), 11),
                            (new Vector2(2064.86f, 4931.304f), 11),
                            (new Vector2(2066.296f, 4932.741f), 11),
                            (new Vector2(2067.712f, 4934.156f), 11),
                            (new Vector2(2069.134f, 4935.578f), 11),
                            (new Vector2(2070.515f, 4936.958f), 11),
                            (new Vector2(2071.931f, 4938.375f), 11),
                            (new Vector2(2073.379f, 4939.823f), 11),
                        })
                    {
                        IrrigationPoints = new List<Vector3>()
                        {
                            new Vector3(2083.521f-3.666f, 4928.101f+3.666f, 125f),
                            new Vector3(2075.103f-3.666f, 4919.683f+3.666f, 125f),
                            new Vector3(2066.564f-3.666f, 4911.144f+3.666f, 125f),
                            new Vector3(2057.878f-3.666f, 4902.458f+3.666f, 125f),
                        }
                    },

                    new Farm.CropField(Farm.CropField.Types.Cabbage, 41.9025f, new Vector2(2.16f, -2.16f),
                        new (Vector2, byte)[]
                        {
                            (new Vector2(2019.139f-36.571f, 4939.039f-36.579f), 11),
                            (new Vector2(2020.566f - 36.571f, 4940.467f - 36.579f), 11),
                            (new Vector2(2022.273f - 36.571f, 4942.173f - 36.579f), 11),
                            (new Vector2(2023.703f - 36.571f, 4943.604f - 36.579f), 11),
                            (new Vector2(2025.108f - 36.571f, 4945.008f - 36.579f), 11),
                            (new Vector2(2026.458f - 36.571f, 4946.359f - 36.579f), 11),
                            (new Vector2(2027.92f - 36.571f, 4947.82f - 36.579f), 11),
                            (new Vector2(2029.353f - 36.571f, 4949.253f - 36.579f), 11),
                            (new Vector2(2030.774f - 36.571f, 4950.675f - 36.579f), 11),
                            (new Vector2(2032.135f - 36.571f, 4952.036f - 36.579f), 11),
                            (new Vector2(2033.581f - 36.571f, 4953.481f - 36.579f), 11),
                            (new Vector2(2035.001f - 36.571f, 4954.901f - 36.579f), 11),
                            (new Vector2(2036.404f - 36.571f, 4956.305f - 36.579f), 11),
                            (new Vector2(2037.807f - 36.571f, 4957.707f - 36.579f), 11),
                            (new Vector2(2039.229f - 36.571f, 4959.129f - 36.579f), 11),
                            (new Vector2(2040.655f - 36.571f, 4960.555f - 36.579f), 11),
                            (new Vector2(2042.088f - 36.571f, 4961.988f - 36.579f), 11),
                            (new Vector2(2043.435f - 36.571f, 4963.335f - 36.579f), 11),
                        })
                    {
                        IrrigationPoints = new List<Vector3>()
                        {
                            new Vector3(2044.11f, 4888.689f, 110f),
                            new Vector3(2035.581f, 4880.161f, 110f),
                            new Vector3(2027.037f, 4871.617f, 110f),
                            new Vector3(2017.256f, 4861.835f, 110f),
                        }
                    },

                    new Farm.CropField(Farm.CropField.Types.Pumpkin, 41.9025f, new Vector2(2.16f, -2.16f),
                        new (Vector2, byte)[]
                        {
                            (new Vector2(2046.209f - 36.625f, 4912.653f - 36.625f), 11),
                            (new Vector2(2047.677f - 36.625f, 4914.121f - 36.625f), 11),
                            (new Vector2(2049.343f - 36.625f, 4915.786f - 36.625f), 11),
                            (new Vector2(2050.742f - 36.625f, 4917.186f - 36.625f), 11),
                            (new Vector2(2052.181f - 36.625f, 4918.625f - 36.625f), 11),
                            (new Vector2(2053.558f - 36.625f, 4920.002f - 36.625f), 11),
                            (new Vector2(2054.976f - 36.625f, 4921.42f - 36.625f), 11),
                            (new Vector2(2056.419f - 36.625f, 4922.864f - 36.625f), 11),
                            (new Vector2(2057.854f - 36.625f, 4924.298f - 36.625f), 11),
                            (new Vector2(2059.196f - 36.625f, 4925.641f - 36.625f), 11),
                            (new Vector2(2060.621f - 36.625f, 4927.065f - 36.625f), 11),
                            (new Vector2(2062.071f - 36.625f, 4928.516f - 36.625f), 11),
                            (new Vector2(2063.497f - 36.625f, 4929.941f - 36.625f), 11),
                            (new Vector2(2064.86f - 36.625f, 4931.304f - 36.625f), 11),
                            (new Vector2(2066.296f - 36.625f, 4932.741f - 36.625f), 11),
                            (new Vector2(2067.712f - 36.625f, 4934.156f - 36.625f), 11),
                            (new Vector2(2069.134f - 36.625f, 4935.578f - 36.625f), 11),
                            (new Vector2(2070.515f - 36.625f, 4936.958f - 36.625f), 11),
                        })
                    {
                        IrrigationPoints = new List<Vector3>()
                        {
                            new Vector3(2017.169, 4915.062f, 110f),
                            new Vector3(2008.635, 4906.527f, 110f),
                            new Vector3(2000.186, 4898.079f, 110f),
                            new Vector3(1990.599, 4888.492f, 110f),
                        }
                    },

                    new Farm.CropField(Farm.CropField.Types.Pumpkin, 44.47724f, new Vector2(2.16f, -2.16f),
                        new (Vector2, byte)[]
                        {
                            (new Vector2(1930.234f, 4846.968f), 8),
                            (new Vector2(1931.695f, 4848.429f), 8),
                            (new Vector2(1933.369f, 4850.103f), 8),
                            (new Vector2(1934.769f, 4851.503f), 8),
                            (new Vector2(1936.193f, 4852.927f), 8),
                            (new Vector2(1937.61f, 4854.344f), 8),
                            (new Vector2(1939.023f, 4855.757f), 8),
                            (new Vector2(1940.397f, 4857.132f), 8),
                            (new Vector2(1941.833f, 4858.568f), 8),
                            (new Vector2(1943.219f, 4859.954f), 8),
                            (new Vector2(1944.574f, 4861.309f), 8),
                            (new Vector2(1945.925f, 4862.66f), 8),
                            (new Vector2(1947.305f, 4864.04f), 8),
                            (new Vector2(1948.747f, 4865.482f), 8),
                            (new Vector2(1950.167f, 4866.902f), 8),
                            (new Vector2(1951.496f, 4868.231f), 8),
                            (new Vector2(1952.952f, 4869.687f), 8),
                            (new Vector2(1954.416f, 4871.151f), 8),
                            (new Vector2(1955.934f, 4872.669f), 8),
                            (new Vector2(1957.304f, 4874.039f), 8),
                            (new Vector2(1958.715f, 4875.45f), 8),
                            (new Vector2(1960.047f, 4876.782f), 8),
                            (new Vector2(1961.454f, 4878.189f), 8),
                            (new Vector2(1962.898f, 4879.632f), 8),
                        })
                    {
                        IrrigationPoints = new List<Vector3>()
                        {
                            new Vector3(1969.373f-3.666f, 4871.934f+3.666f, 135f),
                            new Vector3(1958.054f-3.666f, 4860.614f+3.666f, 135f),
                            new Vector3(1948.13f-3.666f, 4850.69f+3.666f, 135f),
                            new Vector3(1938.017-3.666f, 4840.576f+3.666f, 135f),
                        }
                    },

                    new Farm.CropField(Farm.CropField.Types.Wheat, 42.92637f, new Vector2(3.7f, 3.7f),
                        new (Vector2, byte)[]
                        {
                            (new Vector2(1935.894f, 4791.247f), 8),
                            (new Vector2(1942.579f, 4784.562f), 8),
                            (new Vector2(1949.346f, 4777.795f), 8),
                        })
                    {
                        IrrigationPoints = new List<Vector3>()
                        {
                            new Vector3(1971.222f-5.821f, 4810.679f+5.821f, 115f),
                            new Vector3(1955.938f-5.821f, 4795.396f+5.821f, 115f),
                            new Vector3(1941.686f-5.821f, 4781.144f+5.821f, 115f),
                        }
                    },

                    new Farm.CropField(Farm.CropField.Types.Wheat, 41.51846f, new Vector2(3.7f, 3.7f),
                        new (Vector2, byte)[]
                        {
                            (new Vector2(1911.692f, 4739.881f), 9),
                            (new Vector2(1902.925f, 4744.109f), 10),
                            (new Vector2(1896.589f, 4749.528f), 10),
                            (new Vector2(1891.051f, 4755.066f), 10),
                        })
                    {
                        IrrigationPoints = new List<Vector3>()
                        {
                            new Vector3(1936.407f+3.666f, 4779.13f-3.666f, 135f),
                            new Vector3(1917.305f+3.666f, 4760.029f-3.666f, 135f),
                            new Vector3(1898.928f+3.666f, 4741.652f-3.666f, 135f),
                        }
                    },

                    new Farm.CropField(Farm.CropField.Types.Wheat, 43.17502f, new Vector2(3.7f, 3.7f),
                        new (Vector2, byte)[]
                        {
                            (new Vector2(1874.996f, 4772.179f), 10),
                            (new Vector2(1869.431f, 4777.744f), 10),
                            (new Vector2(1863.818f, 4783.357f), 10),
                        })
                    {
                        IrrigationPoints = new List<Vector3>()
                        {
                            new Vector3(1906.347f, 4808.907f, 125f),
                            new Vector3(1887.021f, 4789.582f, 125f),
                            new Vector3(1869.118f, 4771.678f, 125f),
                        }
                    },

                    new Farm.CropField(Farm.CropField.Types.Wheat, 43.17502f, new Vector2(3.7f, 3.7f),
                        new (Vector2, byte)[]
                        {
                            (new Vector2(1851.668f, 4795.507f), 10),
                            (new Vector2(1845.889f, 4801.287f), 10),
                            (new Vector2(1840.328f, 4806.847f), 10),
                        })
                    {
                        IrrigationPoints = new List<Vector3>()
                        {
                            new Vector3(1880.168f, 4835.369f, 125f),
                            new Vector3(1861.13f, 4816.312f, 125f),
                            new Vector3(1842.804f, 4797.992f, 125f),
                        }
                    },
                },

                OrangeTrees = new List<Farm.OrangeTreeData>()
                {
                    new Farm.OrangeTreeData(new Vector3(1981.833f, 4771.882f, 40.53275f)),
                    new Farm.OrangeTreeData(new Vector3(2003.66f, 4786.886f, 40.72567f)),
                    new Farm.OrangeTreeData(new Vector3(2015.966f, 4800.509f, 40.89126f)),
                    new Farm.OrangeTreeData(new Vector3(2031.14f, 4802.009f, 40.62146f)),
                    new Farm.OrangeTreeData(new Vector3(2064.053f, 4819.512f, 40.77041f)),
                    new Farm.OrangeTreeData(new Vector3(2060.294f, 4842.658f, 40.7404f)),
                    new Farm.OrangeTreeData(new Vector3(2083.302, 4853.354, 40.82642)),
                    new Farm.OrangeTreeData(new Vector3(2086.233f, 4825.683f, 40.42909f)),
                    new Farm.OrangeTreeData(new Vector3(2098.362f, 4841.382f, 40.63255f)),
                    new Farm.OrangeTreeData(new Vector3(2117.429f, 4842.077f, 40.49171f)),
                    new Farm.OrangeTreeData(new Vector3(2102.03f, 4877.732f, 39.98615f)),
                    new Farm.OrangeTreeData(new Vector3(2122.08f, 4861.78f, 40.02671f)),
                    new Farm.OrangeTreeData(new Vector3(2122.971f, 4883.903f, 39.78011f)),
                    new Farm.OrangeTreeData(new Vector3(2146.314f, 4867.564f, 39.64517f)),

                    new Farm.OrangeTreeData(new Vector3(2098.273f, 4860.119f, 40.30014f)),
                    new Farm.OrangeTreeData(new Vector3(2021.173f, 4783.085f, 40.64199f)),
                    new Farm.OrangeTreeData(new Vector3(1963.213f, 4753.013f, 40.73709f)),
                },

                OrangeTreeBoxPositions = new List<Vector3>()
                {
                    new Vector3(2077.927f, 4871.625f, 41.53327f),
                    new Vector3(1994.271f, 4788.977f, 41.94209f),
                },

                Cows = new List<Farm.CowData>()
                {
                    new Farm.CowData(new Vector4(1971.106f, 4947.68f, 43.20691f, 278.614f)),
                    new Farm.CowData(new Vector4(1971.017f, 4957.933f, 43.03f, 224.2186f)),
                    new Farm.CowData(new Vector4(1982.485f, 4954.036f, 42.74105f, 161.3991f)),
                    new Farm.CowData(new Vector4(1970.186f, 4934.179f, 43.37288f, 323.106f)),
                    new Farm.CowData(new Vector4(1962.676f, 4938.37f, 43.94593f, 269.1295f)),
                },

                CowBucketPositions = new List<Vector3>()
                {
                    new Vector3(1980.839f, 4937.755f, 42.74192f),
                },
            };

            WeaponShop.ShootingRangePrice = 150;

            return All.Count;
        }

        public static void ReplaceClientsideLines()
        {
            var lines = new List<string>();

            foreach (var x in All.Values)
            {
                lines.Add($"new {x.GetType().Name}({x.ClientData});");
            }

            Utils.FillFileToReplaceRegion(System.IO.Directory.GetCurrentDirectory() + Properties.Settings.Static.ClientScriptsTargetPath + @"\Game\Businesses\Business.Initialization.cs", "TO_REPLACE", lines);
        }
    }
}
