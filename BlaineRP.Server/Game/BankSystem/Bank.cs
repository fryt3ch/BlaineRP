﻿using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.BankSystem
{
    public partial class Bank
    {
        public const decimal ATM_TAX = 0.02m;

        public const decimal MOBILE_SEND_TAX = 0.05m;

        public static float MobileSendTaxClientVisual = (float)Math.Ceiling(MOBILE_SEND_TAX * 100);

        private static Dictionary<int, Vector4[]> Banks = new Dictionary<int, Vector4[]>()
        {
            { 0, new Vector4[] { new Vector4(-111.2246f, 6469.992f, 31.62671f, 133.712f) } },
            { 1, new Vector4[] { new Vector4(1175.001f, 2708.205f, 38.08792f, 176.3602f) } },

            { 2, new Vector4[] { new Vector4(243.6818f, 226.22f, 106.2876f, 154.0384f), new Vector4(248.7591f, 224.3721f, 106.2876f, 154.0384f), new Vector4(253.9547f, 222.5404f, 106.2876f, 154.0384f) } },

            { 3, new Vector4[] { new Vector4(-351.3526f, -51.27759f, 49.0365f, 338.2035f) } },
            { 4, new Vector4[] { new Vector4(313.8149f, -280.5039f, 54.16468f, 338.7193f) } },
            { 5, new Vector4[] { new Vector4(-1211.996f, -332.0042f, 37.78094f, 25.14937f) } },
            { 6, new Vector4[] { new Vector4(-2961.119f, 482.9693f, 15.697f, 86.52053f) } },
            { 7, new Vector4[] { new Vector4(149.432f, -1042.05f, 29.36801f, 337.0007f) } },
        };

        private static Dictionary<int, Vector4> ATMs = new Dictionary<int, Vector4>()
        {
            { 0, new Vector4(-301.65726f, -829.5886f, 32.419765f, 1f) },
            { 1, new Vector4(-303.2257f, -829.3121f, 32.419765f, 1f) },
            { 2, new Vector4(-204.0193f, -861.0091f, 30.271332f, 1f) },
            { 3, new Vector4(118.64156f, -883.56946f, 31.13945f, 1f) },
            { 4, new Vector4(24.5933f, -945.543f, 29.333046f, 1f) },
            { 5, new Vector4(5.686035f, -919.9551f, 29.48088f, 1f) },
            { 6, new Vector4(296.17563f, -896.2318f, 29.290146f, 1f) },
            { 7, new Vector4(296.8775f, -894.3196f, 29.261478f, 1f) },
            { 8, new Vector4(147.47305f, -1036.2175f, 29.367783f, 1f) },
            { 9, new Vector4(145.83922f, -1035.6254f, 29.367783f, 1f) },
            { 10, new Vector4(112.47614f, -819.80804f, 31.339552f, 1f) },
            { 11, new Vector4(111.38856f, -774.84015f, 31.437658f, 1f) },
            { 12, new Vector4(114.54742f, -775.9721f, 31.417364f, 1f) },
            { 13, new Vector4(-256.6386f, -715.88983f, 33.7883f, 1f) },
            { 14, new Vector4(-259.27673f, -723.2652f, 33.701546f, 1f) },
            { 15, new Vector4(-254.52185f, -692.8869f, 33.578255f, 1f) },
            { 16, new Vector4(-27.890343f, -724.10895f, 44.22287f, 1f) },
            { 17, new Vector4(-30.099571f, -723.2863f, 44.22287f, 1f) },
            { 18, new Vector4(228.03244f, 337.85013f, 105.50133f, 1f) },
            { 19, new Vector4(158.79654f, 234.74516f, 106.643265f, 1f) },
            { 20, new Vector4(527.77765f, -160.66086f, 57.136715f, 1f) },
            { 21, new Vector4(-57.170288f, -92.37918f, 57.750687f, 1f) },
            { 22, new Vector4(89.813385f, 2.880325f, 68.35214f, 1f) },
            { 23, new Vector4(285.3485f, 142.97507f, 104.16232f, 1f) },
            { 24, new Vector4(357.12845f, 174.08362f, 103.059654f, 1f) },
            { 25, new Vector4(1137.8113f, -468.86255f, 66.698654f, 1f) },
            { 26, new Vector4(1167.06f, -455.6541f, 66.818565f, 1f) },
            { 27, new Vector4(1077.7786f, -776.96643f, 58.256516f, 1f) },
            { 28, new Vector4(289.52997f, -1256.7876f, 29.440575f, 1f) },
            { 29, new Vector4(289.26785f, -1282.3204f, 29.65519f, 1f) },
            { 30, new Vector4(-165.58443f, 234.76587f, 94.92897f, 1f) },
            { 31, new Vector4(-165.58443f, 232.69547f, 94.92897f, 1f) },
            { 32, new Vector4(-1044.466f, -2739.6414f, 9.12406f, 1f) },
            { 33, new Vector4(-1205.3783f, -326.5286f, 37.85104f, 1f) },
            { 34, new Vector4(-1206.1417f, -325.03165f, 37.85104f, 1f) },
            { 35, new Vector4(-846.6537f, -341.50903f, 38.668503f, 1f) },
            { 36, new Vector4(-847.204f, -340.42908f, 38.6793f, 1f) },
            { 37, new Vector4(-720.6288f, -415.52432f, 34.97996f, 1f) },
            { 38, new Vector4(-867.013f, -187.99278f, 37.882175f, 1f) },
            { 39, new Vector4(-867.97455f, -186.34193f, 37.882175f, 1f) },
            { 40, new Vector4(-1415.4801f, -212.33244f, 46.49542f, 1f) },
            { 41, new Vector4(-1430.6633f, -211.35867f, 46.47162f, 1f) },
            { 42, new Vector4(-1410.7357f, -98.927895f, 52.39701f, 1f) },
            { 43, new Vector4(-1410.183f, -100.64539f, 52.396523f, 1f) },
            { 44, new Vector4(-1282.0983f, -210.55992f, 42.43031f, 1f) },
            { 45, new Vector4(-1286.7037f, -213.78275f, 42.43031f, 1f) },
            { 46, new Vector4(-1289.742f, -227.16498f, 42.43031f, 1f) },
            { 47, new Vector4(-1285.1365f, -223.94215f, 42.43031f, 1f) },
            { 48, new Vector4(-712.93567f, -818.4827f, 23.740658f, 1f) },
            { 49, new Vector4(-710.08276f, -818.4756f, 23.736336f, 1f) },
            { 50, new Vector4(-617.80347f, -708.8591f, 30.043213f, 1f) },
            { 51, new Vector4(-617.80347f, -706.8521f, 30.043213f, 1f) },
            { 52, new Vector4(-614.5187f, -705.5981f, 31.223999f, 1f) },
            { 53, new Vector4(-611.8581f, -705.5981f, 31.223999f, 1f) },
            { 54, new Vector4(-660.67633f, -854.48816f, 24.456635f, 1f) },
            { 55, new Vector4(-537.8052f, -854.93567f, 29.275429f, 1f) },
            { 56, new Vector4(-594.61444f, -1160.8519f, 22.333511f, 1f) },
            { 57, new Vector4(-596.12506f, -1160.8503f, 22.3336f, 1f) },
            { 58, new Vector4(-526.7791f, -1223.3737f, 18.45272f, 1f) },
            { 59, new Vector4(-1569.8396f, -547.0309f, 34.932163f, 1f) },
            { 60, new Vector4(-1570.7653f, -547.7035f, 34.932163f, 1f) },
            { 61, new Vector4(-1305.7078f, -706.6881f, 25.314468f, 1f) },
            { 62, new Vector4(-1315.416f, -834.431f, 16.952328f, 1f) },
            { 63, new Vector4(-1314.466f, -835.6913f, 16.952328f, 1f) },
            { 64, new Vector4(-2071.9285f, -317.2862f, 13.318085f, 1f) },
            { 65, new Vector4(-821.89355f, -1081.5546f, 11.136639f, 1f) },
            { 66, new Vector4(-1110.2284f, -1691.1538f, 4.378483f, 1f) },
            { 67, new Vector4(-2956.8481f, 487.21576f, 15.478001f, 1f) },
            { 68, new Vector4(-2958.977f, 487.30713f, 15.478001f, 1f) },
            { 69, new Vector4(-2974.5864f, 380.12692f, 15f, 1f) },
            { 70, new Vector4(-1091.8875f, 2709.0535f, 18.919415f, 1f) },
            { 71, new Vector4(-2295.8525f, 357.93475f, 174.60143f, 1f) },
            { 72, new Vector4(-2295.0693f, 356.2556f, 174.60143f, 1f) },
            { 73, new Vector4(-2294.2998f, 354.6056f, 174.60143f, 1f) },
            { 74, new Vector4(-3144.8875f, 1127.811f, 20.838036f, 1f) },
            { 75, new Vector4(-3043.8347f, 594.16394f, 7.732796f, 1f) },
            { 76, new Vector4(-3241.4546f, 997.9085f, 12.548369f, 1f) },
            { 77, new Vector4(2563.9995f, 2584.553f, 38.06807f, 1f) },
            { 78, new Vector4(2558.3242f, 350.988f, 108.597466f, 1f) },
            { 79, new Vector4(156.18863f, 6643.2f, 31.59372f, 1f) },
            { 80, new Vector4(173.8246f, 6638.2173f, 31.59372f, 1f) },
            { 81, new Vector4(-282.7141f, 6226.43f, 31.496475f, 1f) },
            { 82, new Vector4(-95.870285f, 6457.462f, 31.473938f, 1f) },
            { 83, new Vector4(-97.63721f, 6455.732f, 31.467934f, 1f) },
            { 84, new Vector4(-132.66629f, 6366.8765f, 31.47258f, 1f) },
            { 85, new Vector4(-386.4596f, 6046.4106f, 31.473991f, 1f) },
            { 86, new Vector4(1687.3951f, 4815.9f, 42.006466f, 1f) },
            { 87, new Vector4(1700.6941f, 6426.762f, 32.632965f, 1f) },
            { 88, new Vector4(1822.9714f, 3682.5771f, 34.267452f, 1f) },
            { 89, new Vector4(1171.523f, 2703.1394f, 38.147697f, 1f) },
            { 90, new Vector4(1172.4573f, 2703.1394f, 38.147697f, 1f) },
            { 91, new Vector4(238.26779f, 217.10918f, 106.40615f, 1f) },
            { 92, new Vector4(238.69781f, 216.18698f, 106.40615f, 1f) },
            { 93, new Vector4(237.83775f, 218.03137f, 106.40615f, 1f) },
            { 94, new Vector4(237.40773f, 218.95358f, 106.40615f, 1f) },
            { 95, new Vector4(236.9777f, 219.87578f, 106.40615f, 1f) },
            { 96, new Vector4(264.86896f, 209.94864f, 106.40615f, 1f) },
            { 97, new Vector4(265.21695f, 210.9048f, 106.40615f, 1f) },
            { 98, new Vector4(265.56497f, 211.86098f, 106.40615f, 1f) },
            { 99, new Vector4(265.913f, 212.81714f, 106.40615f, 1f) },
            { 100, new Vector4(266.26102f, 213.77332f, 106.40615f, 1f) },
            { 101, new Vector4(380.65576f, 322.8424f, 103.56634f, 1f) },
            { 102, new Vector4(1153.1111f, -326.90186f, 69.20503f, 1f) },
            { 103, new Vector4(33.19432f, -1348.8058f, 29.49696f, 1f) },
            { 104, new Vector4(130.57912f, -1292.3688f, 29.271421f, 1f) },
            { 105, new Vector4(130.15036f, -1291.6261f, 29.271421f, 1f) },
            { 106, new Vector4(129.69753f, -1290.8418f, 29.271421f, 1f) },
            { 107, new Vector4(-57.402237f, -1751.7471f, 29.420937f, 1f) },
            { 108, new Vector4(-718.26135f, -915.71277f, 19.21553f, 1f) },
            { 109, new Vector4(-273.36655f, -2024.2079f, 30.169643f, 1f) },
            { 110, new Vector4(-262.36078f, -2012.054f, 30.169643f, 1f) },
            { 111, new Vector4(-1391.3445f, -589.86273f, 30.315836f, 1f) },
            { 112, new Vector4(-1827.6887f, 784.465f, 138.31522f, 1f) },
            { 113, new Vector4(-3040.2046f, 593.29694f, 7.908859f, 1f) },
            { 114, new Vector4(-3240.028f, 1008.5453f, 12.830639f, 1f) },
            { 115, new Vector4(2559.0522f, 389.47443f, 108.62291f, 1f) },
            { 116, new Vector4(1703.3152f, 4934.0527f, 42.063587f, 1f) },
            { 117, new Vector4(1735.0105f, 6410.01f, 35.03717f, 1f) },
            { 118, new Vector4(2683.592f, 3286.3f, 55.241077f, 1f) },
            { 119, new Vector4(1968.3923f, 3743.0784f, 32.34369f, 1f) },
            { 120, new Vector4(540.22064f, 2671.683f, 42.15644f, 1f) },
        };

        public static void LoadAll()
        {
            var lines = new List<string>();

            foreach (var x in Banks)
                lines.Add($"new {nameof(BlaineRP.Client.Game.Misc.Bank)}({x.Key}, new {typeof(BlaineRP.Client.Utils.Vector4).FullName}[] {{ {string.Join(", ", x.Value.Select(x => x.ToCSharpStr()))} }});");

            Utils.FillFileToReplaceRegion(System.IO.Directory.GetCurrentDirectory() + Properties.Settings.Static.ClientScriptsTargetPath + @"\Game\Misc\Bank.Initialization.cs", "TO_REPLACE", lines);

            lines = new List<string>();

            foreach (var x in ATMs)
                lines.Add($"new {nameof(BlaineRP.Client.Game.Misc.ATM)}({x.Key}, {x.Value.ToCSharpStr()});");

            Utils.FillFileToReplaceRegion(System.IO.Directory.GetCurrentDirectory() + Properties.Settings.Static.ClientScriptsTargetPath + @"\Game\Misc\ATM.Initialization.cs", "TO_REPLACE", lines);
        }

        public static Vector4[] GetBankData(int id) => Banks.GetValueOrDefault(id);

        public static Vector4 GetAtmData(int id) => ATMs.GetValueOrDefault(id);

        public static bool IsPlayerNearBank(Player player, int id)
        {
            var bData = GetBankData(id);

            if (bData == null || player.Dimension != Properties.Settings.Static.MainDimension)
                return false;

            foreach (var x in bData)
            {
                if (player.Position.DistanceTo(x.Position) <= Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE)
                    return true;
            }

            return false;
        }

        public static bool IsPlayerNearAtm(Player player, int id)
        {
            var atmData = GetAtmData(id);

            if (atmData == null || player.Dimension != Properties.Settings.Static.MainDimension)
                return false;

            return player.Position.DistanceTo(atmData.Position) <= (atmData.RotationZ + 5f);
        }
    }
}
