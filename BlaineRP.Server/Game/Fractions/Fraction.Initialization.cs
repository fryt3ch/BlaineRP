using GTANetworkAPI;
using System.Collections.Generic;
using BlaineRP.Server.Game.Containers;
using BlaineRP.Server.Game.EntitiesData.Players.Customization.Clothes.Uniforms;
using BlaineRP.Server.Game.NPCs;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Fractions
{
    public abstract partial class Fraction
    {
        public static int InitializeAll()
        {
            Container.AllSIDs.Add("f_storage", new Container.Data(125, 100_000f, Container.AllowedItemTypes.All, ContainerTypes.Storage));

            new Police(FractionType.COP_BLAINE, Language.Strings.Get("FRACTION_COP_BLAINE_NAME"))
            {
                SpawnPositions = new Vector4[]
                {
                    new Vector4(-438.325f, 5990.785f, 31.71619f, 310.7526f),
                    new Vector4(1853.851f, 3686.156f, 34.26704f, 212.9599f),
                },

                ContainerPositions = new Vector4[]
                {
                     new Vector4(-441.8761f, 5987.493f, 30.7162f, 2.5f),

                     new Vector4(1848.793f, 3689.671f, 33.26704f, 1.5f),
                },

                CreationWorkbenchPositions = new Vector4[]
                {
                    new Vector4(-437.8557f, 5988.477f, 30.71618f, 1f),

                    new Vector4(1851.802f, 3691.445f, 33.26705f, 1f),
                },

                LockerRoomPositions = new Vector3[]
                {
                    new Vector3(-439.1087f, 5993.017f, 30.71619f),

                    new Vector3(1857.349f, 3689.408f, 33.26704f),
                },

                UniformTypes = new List<UniformTypes>()
                {
                    UniformTypes.FractionPaletoPolice_0,
                    UniformTypes.FractionPaletoPolice_1,
                    UniformTypes.FractionPaletoPolice_2,
                },

                ArrestCellsPositions = new Vector4[]
                {
                    new Vector4(-429.6015f, 6001.549f, 31.71618f, 3f),
                    new Vector4(-426.6064f, 5998.16f, 31.71618f, 3f),
                },

                ArrestFreePosition = new Vector4(-442.0793f, 6017.475f, 31.67867f, 314.3072f),

                ArrestMenuPositions = new Vector3[]
                {
                    new Vector3(-435.4453f, 5997.362f, 31.71618f),
                },

                ArrestColshapePosition = new Vector4(-430.256775f, 5997.575f, 32.45621f, 10f),

                CreationWorkbenchPrices = new Dictionary<string, uint>()
                {
                    { "w_pistol", 100 },

                    { "mis_gpstr", 10 },
                },

                ItemTag = "BCPD",

                MetaFlags = FlagTypes.IsGov | FlagTypes.MembersHaveDocs,
            };

            new Police(FractionType.COP_LS, Language.Strings.Get("FRACTION_COP_LS_NAME"))
            {
                SpawnPositions = new Vector4[]
                {
                    new Vector4(455.8622f, -991.1062f, 30.68932f, 88.41425f),
                },

                ContainerPositions = new Vector4[]
                {
                    new Vector4(477.5417f, -989.4244f, 23.91471f, 2.5f),
                },

                CreationWorkbenchPositions = new Vector4[]
                {
                    new Vector4(472.8697f, -989.4165f, 23.91472f, 1f),
                },

                LockerRoomPositions = new Vector3[]
                {
                    new Vector3(451.3782f, -992.9793f, 29.68934f),
                },

                UniformTypes = new List<UniformTypes>()
                {
                    UniformTypes.FractionPaletoPolice_0,
                    UniformTypes.FractionPaletoPolice_1,
                    UniformTypes.FractionPaletoPolice_2,
                },

                ArrestCellsPositions = new Vector4[]
                {
                    new Vector4(459.9757f, -994.5201f, 24.91471f, 269.2255f),
                    new Vector4(459.4457f, -997.96f, 24.91471f, 269.2255f),
                    new Vector4(459.7129f, -1001.613f, 24.91471f, 269.2255f),

                    new Vector4(467.5764f, -994.4102f, 24.91471f, 275f),
                    new Vector4(472.0844f, -994.6555f, 24.91471f, 275f),
                    new Vector4(476.5382f, -994.5233f, 24.91471f, 275f),
                    new Vector4(480.6288f, -994.2647f, 24.91471f, 275f),
                },

                ArrestFreePosition = new Vector4(433.1303f, -981.7498f, 30.71028f, 86.3075f),

                ArrestMenuPositions = new Vector3[]
                {

                },

                ArrestColshapePosition = new Vector4(472.494965f, -998.1451f, 25.3779182f, 21f),

                CreationWorkbenchPrices = new Dictionary<string, uint>()
                {
                    { "w_pistol", 100 },
                },

                ItemTag = "LSPD",

                MetaFlags = FlagTypes.IsGov | FlagTypes.MembersHaveDocs,
            };

            new WeazelNews(FractionType.MEDIA_LS, Language.Strings.Get("FRACTION_MEDIA_LS_NAME"))
            {
                SpawnPositions = new Vector4[]
                {
                    new Vector4(-594.6252f, -930.1151f, 28.15707f, 266.3295f),
                },

                ContainerPositions = new Vector4[]
                {
                    new Vector4(-578.9703f, -915.7086f, 27.15708f, 1.5f),
                },

                CreationWorkbenchPositions = new Vector4[]
                {
                    new Vector4(-601.053f, -917.2875f, 27.15707f, 1f),
                },

                CreationWorkbenchPrices = new Dictionary<string, uint>()
                {

                },

                ItemTag = "WZLN",

                MetaFlags = FlagTypes.MembersHaveDocs,
            };

            new EMS(FractionType.EMS_BLAINE, Language.Strings.Get("FRACTION_EMS_BLAINE_NAME"))
            {
                SpawnPositions = new Vector4[]
                {
                    new Vector4(-258.9563f, 6330.19f, 32.42728f, 218.3881f),

                    new Vector4(1842.196f, 3679.172f, 34.27489f, 118.615f),
                },

                LockerRoomPositions = new Vector3[]
                {
                    new Vector3(-256.1902f, 6327.726f, 31.42725f),

                    new Vector3(1836.598f, 3685.146f, 33.27487f),
                },

                UniformTypes = new List<UniformTypes>()
                {
                    UniformTypes.FractionPaletoEMS_0,
                    UniformTypes.FractionPaletoEMS_2,
                    UniformTypes.FractionPaletoEMS_1,
                },

                ContainerPositions = new Vector4[]
                {
                    new Vector4(-264.9498f, 6321.589f, 31.4273f, 1.5f),

                    new Vector4(1842.006f, 3684.026f, 33.27487f, 1.5f),
                },

                CreationWorkbenchPositions = new Vector4[]
                {
                    new Vector4(-262.7031f, 6319.312f, 31.4273f, 1f),

                    new Vector4(1839.462f, 3682.543f, 33.27487f, 1f),
                },

                CreationWorkbenchPrices = new Dictionary<string, uint>()
                {

                },

                ItemTag = "BCEMS",

                MetaFlags = FlagTypes.IsGov | FlagTypes.MembersHaveDocs,

                AfterDeathSpawnPositions = new Vector4[]
                {
                    new Vector4(-249.0699f, 6314.924f, 32.42727f, 39.13411f),

                    new Vector4(1819.797f, 3675.556f, 34.27489f, 298.6678f),
                },
            };

            new EMS(FractionType.EMS_LS, Language.Strings.Get("FRACTION_EMS_LS_NAME"))
            {
                SpawnPositions = new Vector4[]
                {
                    new Vector4(306.8656f, -572.422f, 43.28408f, 161.3542f),
                },

                LockerRoomPositions = new Vector3[]
                {
                    new Vector3(302.8677f, -572.0403f, 42.28407f),
                },

                UniformTypes = new List<UniformTypes>()
                {
                    UniformTypes.FractionPaletoEMS_0,
                },

                ContainerPositions = new Vector4[]
                {
                    new Vector4(338.8343f, -595.1829f, 42.2841f, 1.5f),
                },

                CreationWorkbenchPositions = new Vector4[]
                {
                    new Vector4(341.1305f, -588.7886f, 42.2841f, 1f),
                },

                CreationWorkbenchPrices = new Dictionary<string, uint>()
                {

                },

                ItemTag = "LSEMS",

                MetaFlags = FlagTypes.IsGov | FlagTypes.MembersHaveDocs,

                AfterDeathSpawnPositions = new Vector4[]
                {
                    new Vector4(321.2299f, -584.1389f, 43.28405f, 66.56934f),
                },
            };

            new Government(FractionType.GOV_LS, Language.Strings.Get("FRACTION_GOV_LS_NAME"))
            {
                SpawnPositions = new Vector4[]
                {
                    new Vector4(-540.1923f, -197.8367f, 47.42305f, 0.4962777f),
                },

                LockerRoomPositions = new Vector3[]
                {
                    new Vector3(-541.5637f, -192.8628f, 46.42308f),
                },

                UniformTypes = new List<UniformTypes>()
                {
                    UniformTypes.FractionPaletoEMS_0,
                },

                CreationWorkbenchPositions = new Vector4[]
                {
                    new Vector4(-549.821f, -202.9997f, 46.41494f, 1.5f),
                },

                ContainerPositions = new Vector4[]
                {
                    new Vector4(-543.8552f, -198.2676f, 46.41494f, 1f),
                },

                CreationWorkbenchPrices = new Dictionary<string, uint>()
                {

                },

                ItemTag = "LSGOV",

                MetaFlags = FlagTypes.IsGov | FlagTypes.MembersHaveDocs,
            };

            new FIB(FractionType.FIB_LS, Language.Strings.Get("FRACTION_FIB_LS_NAME"))
            {
                SpawnPositions = new Vector4[]
    {
                    new Vector4(125.2087f, -765.3222f, 242.1521f, 342.1197f),
    },

                CreationWorkbenchPositions = new Vector4[]
    {
                    new Vector4(145.053f, -767.7247f, 241.1521f, 1.5f),
    },

                ContainerPositions = new Vector4[]
    {
                    new Vector4(148.1316f, -763.9494f, 241.1521f, 1f),
    },

                CreationWorkbenchPrices = new Dictionary<string, uint>()
                {

                },

                LockerRoomPositions = new Vector3[]
    {
                    new Vector3(146.2185f, -771.2402f, 241.1521f),
    },

                UniformTypes = new List<UniformTypes>()
                {
                    UniformTypes.FractionPaletoEMS_0,
                },

                ItemTag = "FIB",

                MetaFlags = FlagTypes.IsGov | FlagTypes.MembersHaveDocs,
            };

            new Prison(FractionType.PRISON_BB, Language.Strings.Get("FRACTION_PRISON_BB_NAME"))
            {
                SpawnPositions = new Vector4[]
                {
                    new Vector4(1785.521f, 2543.936f, 45.79783f, 353.0211f),
                },

                CreationWorkbenchPositions = new Vector4[]
                {
                    new Vector4(1780.982f, 2542.635f, 44.79783f, 1.5f),
                },

                ContainerPositions = new Vector4[]
                {
                    new Vector4(1777.915f, 2543.505f, 44.79783f, 1f),
                },

                CreationWorkbenchPrices = new Dictionary<string, uint>()
                {

                },

                LockerRoomPositions = new Vector3[]
                {
                    new Vector3(1778.769f, 2548.869f, 44.79782f),
                },

                UniformTypes = new List<UniformTypes>()
                {
                    UniformTypes.FractionPaletoEMS_0,
                },

                ItemTag = "SASPA",

                MetaFlags = FlagTypes.IsGov | FlagTypes.MembersHaveDocs,
            };

            new Army(FractionType.ARMY_FZ, Language.Strings.Get("FRACTION_ARMY_FZ_NAME"))
            {
                SpawnPositions = new Vector4[]
                {
                    new Vector4(-1745.333f, 3186.611f, 32.90986f, 141.3292f),
                },

                CreationWorkbenchPositions = new Vector4[]
                {
                    new Vector4(-1758.071f, 3176.766f, 31.90697f, 1.5f),
                },

                ContainerPositions = new Vector4[]
                {
                    new Vector4(-1753.869f, 3179.43f, 31.90697f, 1f),
                },

                CreationWorkbenchPrices = new Dictionary<string, uint>()
                {

                },

                LockerRoomPositions = new Vector3[]
                {
                    new Vector3(-1748.607f, 3171.312f, 31.90697f),
                },

                UniformTypes = new List<UniformTypes>()
                {
                    UniformTypes.FractionPaletoEMS_0,
                },

                ItemTag = "ARMY",

                MetaFlags = FlagTypes.IsGov | FlagTypes.MembersHaveDocs,
            };

            new Gang(FractionType.GANG_MARA, Language.Strings.Get("FRACTION_GANG_MARA_NAME"))
            {
                SpawnPositions = new Vector4[]
                {
                    new Vector4(-25.54851f, -1397.775f, 29.51251f, 214.8488f),
                },

                CreationWorkbenchPositions = new Vector4[]
                {
                    new Vector4(-30.51058f, -1412.792f, 28.51252f, 1.5f),
                },

                ContainerPositions = new Vector4[]
                {
                    new Vector4(-29.67174f, -1408.475f, 28.51251f, 1f),
                },

                CreationWorkbenchPrices = new Dictionary<string, uint>()
                {

                },

                ItemTag = "MARAG",

                MetaFlags = FlagTypes.None,
            };

            new Gang(FractionType.GANG_FAMS, Language.Strings.Get("FRACTION_GANG_FAMS_NAME"))
            {
                SpawnPositions = new Vector4[]
                {
                    new Vector4(-155.9873f, -1602.842f, 35.04389f, 341.8835f),
                },

                CreationWorkbenchPositions = new Vector4[]
                {
                    new Vector4(-134.8378f, -1610.609f, 34.03021f, 1.5f),
                },

                ContainerPositions = new Vector4[]
                {
                    new Vector4(-136.8921f, -1609.587f, 34.03021f, 1f),
                },

                CreationWorkbenchPrices = new Dictionary<string, uint>()
                {

                },

                ItemTag = "FAMS",

                MetaFlags = FlagTypes.None,
            };

            new Gang(FractionType.GANG_BALS, Language.Strings.Get("FRACTION_GANG_BALS_NAME"))
            {
                SpawnPositions = new Vector4[]
                {
                    new Vector4(80.45615f, -1969.136f, 20.74941f, 4.675246f),
                },

                CreationWorkbenchPositions = new Vector4[]
                {
                    new Vector4(83.94167f, -1964.139f, 17.04321f, 1.5f),
                },

                ContainerPositions = new Vector4[]
                {
                    new Vector4(78.93073f, -1962.453f, 17.04321f, 1f),
                },

                CreationWorkbenchPrices = new Dictionary<string, uint>()
                {

                },

                ItemTag = "BALS",

                MetaFlags = FlagTypes.None,
            };

            new Gang(FractionType.GANG_VAGS, Language.Strings.Get("FRACTION_GANG_VAGS_NAME"))
            {
                SpawnPositions = new Vector4[]
                {
                    new Vector4(315.2158f, -2050.231f, 20.97667f, 277.3596f),
                },

                CreationWorkbenchPositions = new Vector4[]
                {
                    new Vector4(319.178f, -2056.716f, 23.00942f, 1.5f),
                },

                ContainerPositions = new Vector4[]
                {
                    new Vector4(323.7132f, -2057.552f, 23.03697f, 1f),
                },

                CreationWorkbenchPrices = new Dictionary<string, uint>()
                {

                },

                ItemTag = "VAGS",

                MetaFlags = FlagTypes.None,
            };

            new Mafia(FractionType.MAFIA_ITALY, Language.Strings.Get("FRACTION_MAFIA_ITA_NAME"))
            {
                SpawnPositions = new Vector4[]
                {
                    new Vector4(420.0956f, -1484.346f, 33.80732f, 117.6836f),
                },

                CreationWorkbenchPositions = new Vector4[]
                {
                    new Vector4(414.901f, -1505.713f, 32.80729f, 1.5f),
                },

                ContainerPositions = new Vector4[]
                {
                    new Vector4(417.7353f, -1503.774f, 32.80729f, 1f),
                },

                CreationWorkbenchPrices = new Dictionary<string, uint>()
                {

                },

                ItemTag = "LCN",

                MetaFlags = FlagTypes.None,
            };

            new Mafia(FractionType.MAFIA_RUSSIA, Language.Strings.Get("FRACTION_MAFIA_RUS_NAME"))
            {
                SpawnPositions = new Vector4[]
                {
                    new Vector4(-75.15957f, 1000.613f, 239.4772f, 111.7685f),
                },

                CreationWorkbenchPositions = new Vector4[]
                {
                    new Vector4(-79.53702f, 1004.585f, 238.5072f, 1.5f),
                },

                ContainerPositions = new Vector4[]
                {
                    new Vector4(-76.85577f, 1007.711f, 238.4774f, 1f),
                },

                CreationWorkbenchPrices = new Dictionary<string, uint>()
                {

                },

                ItemTag = "RUS",

                MetaFlags = FlagTypes.None,
            };

            new Mafia(FractionType.MAFIA_JAPAN, Language.Strings.Get("FRACTION_MAFIA_JAP_NAME"))
            {
                SpawnPositions = new Vector4[]
                {
                    new Vector4(-1510.689f, 113.2561f, 60.79906f, 41.29203f),
                },

                CreationWorkbenchPositions = new Vector4[]
                {
                    new Vector4(-1519.416f, 109.8969f, 59.79875f, 1.5f),
                },

                ContainerPositions = new Vector4[]
                {
                    new Vector4(-1515.389f, 105.9638f, 59.79875f, 1f),
                },

                CreationWorkbenchPrices = new Dictionary<string, uint>()
                {

                },

                ItemTag = "YKZ",

                MetaFlags = FlagTypes.None,
            };

            NPCs.Service.AddNpc($"cop0_{(int)Game.Fractions.FractionType.COP_BLAINE}", new Vector3(-448.2888f, 6012.634f, 31.71635f)); // cop0_1

            foreach (var x in All.Values)
            {
                x.Initialize();
            }

            Gang.GangZone.Initialize();

            return All.Count;
        }

        public static void PostInitializeAll()
        {
            var lines = new List<string>();

            foreach (var x in All.Values)
            {
                x.PostInitialize();

                lines.Add($"new {x.GetType().Name}({x.ClientData});");
            }

            foreach (var x in Gang.GangZone.All)
            {
                if (x.OwnerType == FractionType.None)
                    continue;

                x.UpdateOwner(false);

                lines.Add($"Gang.GangZone.AddZone({x.Id}, {x.Position.X}f, {x.Position.Y}f);");
            }

            Utils.FillFileToReplaceRegion(System.IO.Directory.GetCurrentDirectory() + Properties.Settings.Static.ClientScriptsTargetPath + @"\Game\Fractions\Fraction.Initialization.cs", "TO_REPLACE", lines);
        }
    }
}
