using System.Collections.Generic;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Estates
{
    public partial class GarageRoot
    {
        public static void LoadAll()
        {
            new GarageRoot(1,
                new Vector4(1709.854f, 4728.354f, 42.15108f, 101.0983f),
                new Vector4(1721.625f, 4711.867f, 42.18731f, 5f),
                new List<Vector4>()
                {
                    new Vector4(1721.733f, 4748.438f, 41.53787f, 92.14223f),
                    new Vector4(1705.411f, 4747.812f, 41.60215f, 92.28343f),
                    new Vector4(1697.927f, 4733.728f, 41.69814f, 196.8979f),
                    new Vector4(1740.28f, 4700.589f, 42.26362f, 89.37487f),
                }
            );

            new GarageRoot(2,
                new Vector4(-341.5808f, 6066.215f, 31.48684f, 318.1166f),
                new Vector4(-355.4108f, 6067.475f, 31.49911f, 5f),
                new List<Vector4>()
                {
                    new Vector4(-355.8688f, 6085.566f, 31.04557f, 224.6337f),
                    new Vector4(-358.3138f, 6083.337f, 31.08944f, 226.2532f),
                    new Vector4(-361.7703f, 6080.033f, 31.10662f, 224.8967f),
                    new Vector4(-365.6643f, 6076.228f, 31.10655f, 224.6852f),
                    new Vector4(-369.9357f, 6072.557f, 31.10487f, 224.3345f),
                    new Vector4(-368.7952f, 6060.91f, 31.12005f, 314.1411f),
                    new Vector4(-372.1787f, 6064.764f, 31.12002f, 310.8802f),
                }
            );

            new GarageRoot(3,
                new Vector4(-1167.71f, -700.1437f, 21.89413f, 295.6788f),
                new Vector4(-1204.965f, -715.033f, 21.62106f, 5f),
                new List<Vector4>()
                {
                    new Vector4(-1191.297f, -735.8434f, 20.17742f, 307.4758f),
                    new Vector4(-1189.372f, -738.7911f, 19.98976f, 307.4758f),
                    new Vector4(-1186.283f, -742.4839f, 19.73591f, 307.4758f),
                    new Vector4(-1184.012f, -745.5002f, 19.54056f, 307.4758f),
                }
            );

            new GarageRoot(4,
                new Vector4(316.4698f, -685.1124f, 29.48024f, 246.4526f),
                new Vector4(322.5291f, -680.5625f, 29.3077f, 5f),
                new List<Vector4>()
                {
                    new Vector4(290.2844f, -695.6075f, 28.91822f, 248.2921f),
                    new Vector4(292.0151f, -690.6246f, 28.91821f, 248.6051f),
                    new Vector4(293.4517f, -686.2487f, 28.91976f, 251.8784f),
                    new Vector4(306.4645f, -701.316f, 28.92953f, 249.9491f),
                    new Vector4(308.2393f, -696.9249f, 28.94152f, 248.7504f),
                    new Vector4(310.027f, -692.1644f, 28.96142f, 250.5269f),
                }
            );

            new GarageRoot(5,
                new Vector4(926.3544f, -1560.203f, 30.74199f, 91.18516f),
                new Vector4(947.8146f, -1570.903f, 30.51659f, 5f),
                new List<Vector4>()
                {
                    new Vector4(922.8512f, -1563.964f, 30.34886f, 89.51781f),
                    new Vector4(922.3788f, -1556.564f, 30.39615f, 89.63982f),
                    new Vector4(921.799f, -1548.615f, 30.41576f, 89.55083f),
                    new Vector4(913.3278f, -1578.475f, 30.29019f, 0.1281081f),
                    new Vector4(920.2516f, -1577.328f, 30.21041f, 3.114594f),
                    new Vector4(927.0467f, -1578.27f, 30.06562f, 1.26491f),
                }
            );

            var lines = new List<string>();

            foreach (var x in All.Values)
            {
                lines.Add($"new GarageRoot({x.Id}, {x.EnterPosition.Position.ToCSharpStr()}, {x.EnterPositionVehicle.ToCSharpStr()});");
            }

            Utils.FillFileToReplaceRegion(System.IO.Directory.GetCurrentDirectory() + Properties.Settings.Static.ClientScriptsTargetPath + @"\Game\Estates\Initialization.cs",
                "GROOTS_TO_REPLACE",
                lines
            );
        }
    }
}