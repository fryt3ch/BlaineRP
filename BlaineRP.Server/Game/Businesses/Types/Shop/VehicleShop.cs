using GTANetworkAPI;
using System.Collections.Generic;

namespace BlaineRP.Server.Game.Businesses
{
    public abstract class VehicleShop : Shop, IEnterable
    {

        public override string ClientData => $"{ID}, {PositionInfo.ToCSharpStr()}, {GovPrice}, {Rent}, {Tax}f, {PositionInteract.ToCSharpStr()}";

        public Utils.Vector4 EnterProperties { get; set; }

        public Utils.Vector4[] AfterBuyPositions { get; set; }

        public Utils.Vector4[] ExitProperties { get; set; }

        public int LastExitUsed { get; set; }

        public int LastAfterBuyExitUsed { get; set; }

        public VehicleShop(int ID, Vector3 PositionInfo, Utils.Vector4 EnterProperties, Types Type, Utils.Vector4[] AfterBuyPositions, Utils.Vector4 PositionInteract) : base(ID, PositionInfo, PositionInteract, Type)
        {
            this.EnterProperties = EnterProperties;

            this.AfterBuyPositions = AfterBuyPositions;

            this.ExitProperties = new Utils.Vector4[] { new Utils.Vector4(PositionInteract.Position.GetFrontOf(PositionInteract.RotationZ, 1.5f), Utils.GetOppositeAngle(PositionInteract.RotationZ)) };
        }

        public override bool TryBuyItem(PlayerData pData, bool useCash, string itemId)
        {
            var iData = itemId.Split('_');

            if (iData.Length != 7)
                return false;

            byte r1, g1, b1, r2, g2, b2;

            if (!byte.TryParse(iData[1], out r1) || !byte.TryParse(iData[2], out g1) || !byte.TryParse(iData[3], out b1) || !byte.TryParse(iData[4], out r2) || !byte.TryParse(iData[5], out g2) || !byte.TryParse(iData[6], out b2))
                return false;

            uint newMats;
            ulong newBalance, newPlayerBalance;

            var vType = Data.Vehicles.GetData(iData[0]);

            if (vType == null)
                return false;

            if (!TryProceedPayment(pData, useCash, iData[0], 1, out newMats, out newBalance, out newPlayerBalance))
                return false;

            if (Type >= Types.CarShop1 && Type <= Types.CarShop3)
            {
                if (!pData.HasLicense(PlayerData.LicenseTypes.B))
                    return false;
            }
            else if (Type == Types.MotoShop)
            {
                if (!pData.HasLicense(PlayerData.LicenseTypes.A))
                    return false;
            }
            else if (Type == Types.BoatShop)
            {
                if (!pData.HasLicense(PlayerData.LicenseTypes.Sea))
                    return false;
            }
            else if (Type == Types.AeroShop)
            {
                if (!pData.HasLicense(PlayerData.LicenseTypes.Fly))
                    return false;
            }

            if (pData.FreeVehicleSlots <= 0)
            {
                pData.Player.Notify("Trade::MVOW", pData.OwnedVehicles.Count);

                return false;
            }

            ProceedPayment(pData, useCash, newMats, newBalance, newPlayerBalance);

            var vPos = AfterBuyPositions[AfterBuyPositions.Length == 1 ? 0 : AfterBuyPositions.Length < LastExitUsed + 1 ? ++LastExitUsed : LastExitUsed = 0];

            var vData = VehicleData.New(pData, vType, new Utils.Colour(r1, g1, b1), new Utils.Colour(r2, g2, b2), vPos.Position, vPos.RotationZ, Properties.Settings.Static.MainDimension, true);

            Sync.Players.ExitFromBuiness(pData, false);

            pData.Player.Teleport(vPos.Position, false, Properties.Settings.Static.MainDimension, vPos.RotationZ, true);

            return true;
        }
    }

    public class CarShop1 : VehicleShop
    {
        public static Types DefaultType => Types.CarShop1;

        public static MaterialsData InitMaterialsData => new MaterialsData(5, 7, 50)
        {
            Prices = new Dictionary<string, uint>()
            {
                { "adder", 100 },
                { "airbus", 100 },
                { "airtug", 100 },
                { "alpha", 100 },
                { "ambulance", 100 },
                { "apc", 100 },
                { "ardent", 100 },
                { "asbo", 100 },
                { "asea", 100 },
                { "asea2", 100 },
                { "asterope", 100 },
                { "autarch", 100 },
                { "baller", 100 },
                { "baller2", 100 },
                { "baller3", 100 },
                { "baller4", 100 },
                { "baller5", 100 },
                { "baller6", 100 },
                { "banshee", 100 },
                { "banshee2", 100 },
                { "barracks", 100 },
                { "barracks2", 100 },
                { "barracks3", 100 },
                { "barrage", 100 },
                { "benson", 100 },
                { "bestiagts", 100 },
                { "bfinjection", 100 },
                { "biff", 100 },
                { "bifta", 100 },
                { "bison", 100 },
                { "bison2", 100 },
                { "bison3", 100 },
                { "bjxl", 100 },
                { "blade", 100 },
                { "blazer", 100 },
                { "blazer2", 100 },
                { "blazer3", 100 },
                { "blazer4", 100 },
                { "blazer5", 100 },
                { "blista", 100 },
                { "blista2", 100 },
                { "blista3", 100 },
                { "bobcatxl", 100 },
                { "bodhi2", 100 },
                { "boxville", 100 },
                { "boxville2", 100 },
                { "boxville3", 100 },
                { "boxville4", 100 },
                { "boxville5", 100 },
                { "brawler", 100 },
                { "brickade", 100 },
                { "brioso", 100 },
                { "brioso2", 100 },
                { "brioso3", 100 },
                { "bruiser", 100 },
                { "bruiser2", 100 },
                { "bruiser3", 100 },
                { "brutus", 100 },
                { "brutus2", 100 },
                { "brutus3", 100 },
                { "btype", 100 },
                { "btype2", 100 },
                { "btype3", 100 },
                { "buccaneer", 100 },
                { "buccaneer2", 100 },
                { "buffalo", 100 },
                { "buffalo2", 100 },
                { "buffalo3", 100 },
                { "bulldozer", 100 },
                { "bullet", 100 },
                { "burrito", 100 },
                { "burrito2", 100 },
                { "burrito3", 100 },
                { "burrito4", 100 },
                { "burrito5", 100 },
                { "bus", 100 },
                { "caddy", 100 },
                { "caddy2", 100 },
                { "caddy3", 100 },
                { "calico", 100 },
                { "camper", 100 },
                { "caracara", 100 },
                { "caracara2", 100 },
                { "carbonizzare", 100 },
                { "casco", 100 },
                { "cavalcade", 100 },
                { "cavalcade2", 100 },
                { "cerberus", 100 },
                { "cerberus2", 100 },
                { "cerberus3", 100 },
                { "cheburek", 100 },
                { "cheetah", 100 },
                { "cheetah2", 100 },
                { "chernobog", 100 },
                { "chino", 100 },
                { "chino2", 100 },
                { "clique", 100 },
                { "club", 100 },
                { "coach", 100 },
                { "cog55", 100 },
                { "cog552", 100 },
                { "cogcabrio", 100 },
                { "cognoscenti", 100 },
                { "cognoscenti2", 100 },
                { "comet2", 100 },
                { "comet3", 100 },
                { "comet4", 100 },
                { "comet5", 100 },
                { "comet6", 100 },
                { "contender", 100 },
                { "coquette", 100 },
                { "coquette2", 100 },
                { "coquette3", 100 },
                { "coquette4", 100 },
                { "corsita", 100 },
                { "crusader", 100 },
                { "cutter", 100 },
                { "cyclone", 100 },
                { "cypher", 100 },
                { "deluxo", 100 },
                { "deveste", 100 },
                { "deviant", 100 },
                { "dloader", 100 },
                { "docktug", 100 },
                { "dominator", 100 },
                { "dominator2", 100 },
                { "dominator3", 100 },
                { "dominator4", 100 },
                { "dominator5", 100 },
                { "dominator6", 100 },
                { "dominator7", 100 },
                { "dominator8", 100 },
                { "drafter", 100 },
                { "draugur", 100 },
                { "dubsta", 100 },
                { "dubsta2", 100 },
                { "dubsta3", 100 },
                { "dukes", 100 },
                { "dukes2", 100 },
                { "dukes3", 100 },
                { "dump", 100 },
                { "dune", 100 },
                { "dune2", 100 },
                { "dune3", 100 },
                { "dune4", 100 },
                { "dune5", 100 },
                { "dynasty", 100 },
                { "elegy", 100 },
                { "elegy2", 100 },
                { "ellie", 100 },
                { "emerus", 100 },
                { "emperor", 100 },
                { "emperor2", 100 },
                { "emperor3", 100 },
                { "entity2", 100 },
                { "entityxf", 100 },
                { "euros", 100 },
                { "everon", 100 },
                { "exemplar", 100 },
                { "f620", 100 },
                { "faction", 100 },
                { "faction2", 100 },
                { "faction3", 100 },
                { "fagaloa", 100 },
                { "fbi", 100 },
                { "fbi2", 100 },
                { "felon", 100 },
                { "felon2", 100 },
                { "feltzer2", 100 },
                { "feltzer3", 100 },
                { "firetruk", 100 },
                { "flashgt", 100 },
                { "flatbed", 100 },
                { "fmj", 100 },
                { "forklift", 100 },
                { "formula", 100 },
                { "formula2", 100 },
                { "fq2", 100 },
                { "freecrawler", 100 },
                { "fugitive", 100 },
                { "furia", 100 },
                { "furoregt", 100 },
                { "fusilade", 100 },
                { "futo", 100 },
                { "futo2", 100 },
                { "gauntlet", 100 },
                { "gauntlet2", 100 },
                { "gauntlet3", 100 },
                { "gauntlet4", 100 },
                { "gauntlet5", 100 },
                { "gb200", 100 },
                { "gburrito", 100 },
                { "gburrito2", 100 },
                { "glendale", 100 },
                { "glendale2", 100 },
                { "gp1", 100 },
                { "granger", 100 },
                { "greenwood", 100 },
                { "gresley", 100 },
                { "growler", 100 },
                { "gt500", 100 },
                { "guardian", 100 },
                { "habanero", 100 },
                { "halftrack", 100 },
                { "handler", 100 },
                { "hauler", 100 },
                { "hauler2", 100 },
                { "hellion", 100 },
                { "hermes", 100 },
                { "hotknife", 100 },
                { "hotring", 100 },
                { "huntley", 100 },
                { "hustler", 100 },
                { "imorgon", 100 },
                { "impaler", 100 },
                { "impaler2", 100 },
                { "impaler3", 100 },
                { "impaler4", 100 },
                { "imperator", 100 },
                { "imperator2", 100 },
                { "imperator3", 100 },
                { "infernus", 100 },
                { "infernus2", 100 },
                { "ingot", 100 },
                { "insurgent", 100 },
                { "insurgent2", 100 },
                { "insurgent3", 100 },
                { "intruder", 100 },
                { "issi2", 100 },
                { "issi3", 100 },
                { "issi4", 100 },
                { "issi5", 100 },
                { "issi6", 100 },
                { "issi7", 100 },
                { "italigtb", 100 },
                { "italigtb2", 100 },
                { "italigto", 100 },
                { "italirsx", 100 },
                { "jackal", 100 },
                { "jb700", 100 },
                { "jb7002", 100 },
                { "jester", 100 },
                { "jester2", 100 },
                { "jester3", 100 },
                { "jester4", 100 },
                { "journey", 100 },
                { "jugular", 100 },
                { "kalahari", 100 },
                { "kamacho", 100 },
                { "kanjo", 100 },
                { "kanjosj", 100 },
                { "khamelion", 100 },
                { "khanjali", 100 },
                { "komoda", 100 },
                { "krieger", 100 },
                { "kuruma", 100 },
                { "kuruma2", 100 },
                { "landstalker", 100 },
                { "landstalker2", 100 },
                { "le7b", 100 },
                { "lguard", 100 },
                { "limo2", 100 },
                { "lm87", 100 },
                { "locust", 100 },
                { "lurcher", 100 },
                { "lynx", 100 },
                { "mamba", 100 },
                { "manana", 100 },
                { "manana2", 100 },
                { "marshall", 100 },
                { "massacro", 100 },
                { "massacro2", 100 },
                { "menacer", 100 },
                { "mesa", 100 },
                { "mesa2", 100 },
                { "mesa3", 100 },
                { "michelli", 100 },
                { "minitank", 100 },
                { "minivan", 100 },
                { "minivan2", 100 },
                { "mixer", 100 },
                { "mixer2", 100 },
                { "monroe", 100 },
                { "monster", 100 },
                { "monster3", 100 },
                { "monster4", 100 },
                { "monster5", 100 },
                { "moonbeam", 100 },
                { "moonbeam2", 100 },
                { "mower", 100 },
                { "mule", 100 },
                { "mule2", 100 },
                { "mule3", 100 },
                { "mule4", 100 },
                { "nebula", 100 },
                { "neo", 100 },
                { "nero", 100 },
                { "nero2", 100 },
                { "nightshade", 100 },
                { "nightshark", 100 },
                { "ninef", 100 },
                { "ninef2", 100 },
                { "novak", 100 },
                { "omnis", 100 },
                { "openwheel1", 100 },
                { "openwheel2", 100 },
                { "oracle", 100 },
                { "oracle2", 100 },
                { "osiris", 100 },
                { "outlaw", 100 },
                { "packer", 100 },
                { "panto", 100 },
                { "paradise", 100 },
                { "paragon", 100 },
                { "paragon2", 100 },
                { "pariah", 100 },
                { "patriot", 100 },
                { "patriot2", 100 },
                { "pbus", 100 },
                { "pbus2", 100 },
                { "penetrator", 100 },
                { "penumbra", 100 },
                { "penumbra2", 100 },
                { "peyote", 100 },
                { "peyote2", 100 },
                { "peyote3", 100 },
                { "pfister811", 100 },
                { "phantom", 100 },
                { "phantom2", 100 },
                { "phantom3", 100 },
                { "phoenix", 100 },
                { "picador", 100 },
                { "pigalle", 100 },
                { "police", 100 },
                { "police2", 100 },
                { "police3", 100 },
                { "police4", 100 },
                { "policeold1", 100 },
                { "policeold2", 100 },
                { "policet", 100 },
                { "pony", 100 },
                { "pony2", 100 },
                { "postlude", 100 },
                { "pounder", 100 },
                { "pounder2", 100 },
                { "prairie", 100 },
                { "pranger", 100 },
                { "premier", 100 },
                { "previon", 100 },
                { "primo", 100 },
                { "primo2", 100 },
                { "prototipo", 100 },
                { "radi", 100 },
                { "rallytruck", 100 },
                { "rancherxl", 100 },
                { "rancherxl2", 100 },
                { "rapidgt", 100 },
                { "rapidgt2", 100 },
                { "rapidgt3", 100 },
                { "raptor", 100 },
                { "ratloader", 100 },
                { "ratloader2", 100 },
                { "rcbandito", 100 },
                { "reaper", 100 },
                { "rebel", 100 },
                { "rebel2", 100 },
                { "rebla", 100 },
                { "regina", 100 },
                { "remus", 100 },
                { "rentalbus", 100 },
                { "retinue", 100 },
                { "retinue2", 100 },
                { "revolter", 100 },
                { "rhapsody", 100 },
                { "rhinehart", 100 },
                { "rhino", 100 },
                { "riata", 100 },
                { "riot", 100 },
                { "riot2", 100 },
                { "ripley", 100 },
                { "rocoto", 100 },
                { "romero", 100 },
                { "rt3000", 100 },
                { "rubble", 100 },
                { "ruiner", 100 },
                { "ruiner2", 100 },
                { "ruiner3", 100 },
                { "ruiner4", 100 },
                { "rumpo", 100 },
                { "rumpo2", 100 },
                { "rumpo3", 100 },
                { "ruston", 100 },
                { "s80", 100 },
                { "sabregt", 100 },
                { "sabregt2", 100 },
                { "sadler", 100 },
                { "sadler2", 100 },
                { "sandking", 100 },
                { "sandking2", 100 },
                { "savestra", 100 },
                { "sc1", 100 },
                { "scarab", 100 },
                { "scarab2", 100 },
                { "scarab3", 100 },
                { "schafter2", 100 },
                { "schafter3", 100 },
                { "schafter4", 100 },
                { "schafter5", 100 },
                { "schafter6", 100 },
                { "schlagen", 100 },
                { "schwarzer", 100 },
                { "scramjet", 100 },
                { "scrap", 100 },
                { "seminole", 100 },
                { "seminole2", 100 },
                { "sentinel", 100 },
                { "sentinel2", 100 },
                { "sentinel3", 100 },
                { "sentinel4", 100 },
                { "serrano", 100 },
                { "seven70", 100 },
                { "sheava", 100 },
                { "sheriff", 100 },
                { "sheriff2", 100 },
                { "slamtruck", 100 },
                { "slamvan", 100 },
                { "slamvan2", 100 },
                { "slamvan3", 100 },
                { "slamvan4", 100 },
                { "slamvan5", 100 },
                { "slamvan6", 100 },
                { "sm722", 100 },
                { "specter", 100 },
                { "specter2", 100 },
                { "speedo", 100 },
                { "speedo2", 100 },
                { "speedo4", 100 },
                { "squaddie", 100 },
                { "stafford", 100 },
                { "stalion", 100 },
                { "stalion2", 100 },
                { "stanier", 100 },
                { "stinger", 100 },
                { "stingergt", 100 },
                { "stockade", 100 },
                { "stockade3", 100 },
                { "stratum", 100 },
                { "streiter", 100 },
                { "stretch", 100 },
                { "stromberg", 100 },
                { "sugoi", 100 },
                { "sultan", 100 },
                { "sultan2", 100 },
                { "sultan3", 100 },
                { "sultanrs", 100 },
                { "superd", 100 },
                { "surano", 100 },
                { "surfer", 100 },
                { "surfer2", 100 },
                { "swinger", 100 },
                { "t20", 100 },
                { "taco", 100 },
                { "tailgater", 100 },
                { "tailgater2", 100 },
                { "taipan", 100 },
                { "tampa", 100 },
                { "tampa2", 100 },
                { "tampa3", 100 },
                { "taxi", 100 },
                { "technical", 100 },
                { "technical2", 100 },
                { "technical3", 100 },
                { "tempesta", 100 },
                { "tenf", 100 },
                { "tenf2", 100 },
                { "terbyte", 100 },
                { "tezeract", 100 },
                { "thrax", 100 },
                { "tigon", 100 },
                { "tiptruck", 100 },
                { "tiptruck2", 100 },
                { "toreador", 100 },
                { "torero", 100 },
                { "torero2", 100 },
                { "tornado", 100 },
                { "tornado2", 100 },
                { "tornado3", 100 },
                { "tornado4", 100 },
                { "tornado5", 100 },
                { "tornado6", 100 },
                { "toros", 100 },
                { "tourbus", 100 },
                { "towtruck", 100 },
                { "towtruck2", 100 },
                { "tractor", 100 },
                { "tractor2", 100 },
                { "tractor3", 100 },
                { "trash", 100 },
                { "trash2", 100 },
                { "trophytruck", 100 },
                { "trophytruck2", 100 },
                { "tropos", 100 },
                { "tulip", 100 },
                { "turismo2", 100 },
                { "turismor", 100 },
                { "tyrant", 100 },
                { "tyrus", 100 },
                { "utillitruck", 100 },
                { "utillitruck2", 100 },
                { "utillitruck3", 100 },
                { "vacca", 100 },
                { "vagner", 100 },
                { "vagrant", 100 },
                { "vamos", 100 },
                { "vectre", 100 },
                { "verlierer2", 100 },
                { "verus", 100 },
                { "vetir", 100 },
                { "veto", 100 },
                { "veto2", 100 },
                { "vigero", 100 },
                { "vigero2", 100 },
                { "vigilante", 100 },
                { "virgo", 100 },
                { "virgo2", 100 },
                { "virgo3", 100 },
                { "viseris", 100 },
                { "visione", 100 },
                { "voodoo", 100 },
                { "voodoo2", 100 },
                { "vstr", 100 },
                { "warrener", 100 },
                { "washington", 100 },
                { "wastelander", 100 },
                { "weevil", 100 },
                { "weevil2", 100 },
                { "windsor", 100 },
                { "windsor2", 100 },
                { "winky", 100 },
                { "xa21", 100 },
                { "xls", 100 },
                { "xls2", 100 },
                { "yosemite", 100 },
                { "yosemite2", 100 },
                { "yosemite3", 100 },
                { "youga", 100 },
                { "youga2", 100 },
                { "youga3", 100 },
                { "z190", 100 },
                { "zentorno", 100 },
                { "zhaba", 100 },
                { "zion", 100 },
                { "zion2", 100 },
                { "zion3", 100 },
                { "zorrusso", 100 },
                { "zr350", 100 },
                { "zr380", 100 },
                { "zr3802", 100 },
                { "zr3803", 100 },
                { "ztype", 100 },
                { "dilettante", 100 },
                { "dilettante2", 100 },
                { "neon", 100 },
                { "omnisegt", 100 },
                { "raiden", 100 },
                { "surge", 100 },
                { "voltic", 100 },
                { "voltic2", 100 },
            }
        };

        public CarShop1(int ID, Vector3 Position, Utils.Vector4 EnterProperties, Utils.Vector4[] AfterBuyPositions, Utils.Vector4 PositionInteract) : base(ID, Position, EnterProperties, DefaultType, AfterBuyPositions, PositionInteract)
        {

        }
    }

    public class CarShop2 : VehicleShop
    {
        public static Types DefaultType => Types.CarShop2;

        public static MaterialsData InitMaterialsData => new MaterialsData(5, 7, 50)
        {
            Prices = new Dictionary<string, uint>()
            {

            }
        };

        public CarShop2(int ID, Vector3 Position, Utils.Vector4 EnterProperties, Utils.Vector4[] AfterBuyPositions, Utils.Vector4 PositionInteract) : base(ID, Position, EnterProperties, DefaultType, AfterBuyPositions, PositionInteract)
        {

        }
    }

    public class CarShop3 : VehicleShop
    {
        public static Types DefaultType => Types.CarShop3;

        public static MaterialsData InitMaterialsData => new MaterialsData(5, 7, 50)
        {
            Prices = new Dictionary<string, uint>()
            {

            }
        };

        public CarShop3(int ID, Vector3 Position, Utils.Vector4 EnterProperties, Utils.Vector4[] AfterBuyPositions, Utils.Vector4 PositionInteract) : base(ID, Position, EnterProperties, DefaultType, AfterBuyPositions, PositionInteract)
        {

        }
    }

    public class MotoShop : VehicleShop
    {
        public static Types DefaultType => Types.MotoShop;

        public static MaterialsData InitMaterialsData => new MaterialsData(5, 7, 50)
        {
            Prices = new Dictionary<string, uint>()
            {
                { "akuma", 100 },
                { "avarus", 100 },
                { "bagger", 100 },
                { "bati", 100 },
                { "bati2", 100 },
                { "bf400", 100 },
                { "carbonrs", 100 },
                { "chimera", 100 },
                { "cliffhanger", 100 },
                { "daemon", 100 },
                { "daemon2", 100 },
                { "deathbike", 100 },
                { "deathbike2", 100 },
                { "deathbike3", 100 },
                { "defiler", 100 },
                { "diablous", 100 },
                { "diablous2", 100 },
                { "double", 100 },
                { "enduro", 100 },
                { "esskey", 100 },
                { "faggio", 100 },
                { "faggio2", 100 },
                { "faggio3", 100 },
                { "fcr", 100 },
                { "fcr2", 100 },
                { "gargoyle", 100 },
                { "hakuchou", 100 },
                { "hakuchou2", 100 },
                { "hexer", 100 },
                { "innovation", 100 },
                { "lectro", 100 },
                { "manchez", 100 },
                { "manchez2", 100 },
                { "nemesis", 100 },
                { "nightblade", 100 },
                { "oppressor", 100 },
                { "oppressor2", 100 },
                { "pcj", 100 },
                { "ratbike", 100 },
                { "ruffian", 100 },
                { "rrocket", 100 },
                { "sanchez", 100 },
                { "sanchez2", 100 },
                { "sanctus", 100 },
                { "shotaro", 100 },
                { "sovereign", 100 },
                { "stryder", 100 },
                { "thrust", 100 },
                { "vader", 100 },
                { "vindicator", 100 },
                { "vortex", 100 },
                { "wolfsbane", 100 },
                { "zombiea", 100 },
                { "zombieb", 100 },
                { "policeb", 100 },
                { "bmx", 100 },
                { "cruiser", 100 },
                { "fixter", 100 },
                { "scorcher", 100 },
                { "tribike", 100 },
                { "tribike2", 100 },
                { "tribike3", 100 },
            }
        };

        public MotoShop(int ID, Vector3 Position, Utils.Vector4 EnterProperties, Utils.Vector4[] AfterBuyPositions, Utils.Vector4 PositionInteract) : base(ID, Position, EnterProperties, DefaultType, AfterBuyPositions, PositionInteract)
        {

        }
    }

    public class BoatShop : VehicleShop
    {
        public static Types DefaultType => Types.BoatShop;

        public static MaterialsData InitMaterialsData => new MaterialsData(5, 7, 50)
        {
            Prices = new Dictionary<string, uint>()
            {
                { "avisa", 100 },
                { "dinghy", 100 },
                { "dinghy2", 100 },
                { "dinghy3", 100 },
                { "dinghy4", 100 },
                { "dinghy5", 100 },
                { "jetmax", 100 },
                { "kosatka", 100 },
                { "longfin", 100 },
                { "marquis", 100 },
                { "patrolboat", 100 },
                { "predator", 100 },
                { "seashark", 100 },
                { "seashark2", 100 },
                { "seashark3", 100 },
                { "speeder", 100 },
                { "speeder2", 100 },
                { "squalo", 100 },
                { "submersible", 100 },
                { "submersible2", 100 },
                { "suntrap", 100 },
                { "toro", 100 },
                { "toro2", 100 },
                { "tropic", 100 },
                { "tropic2", 100 },
                { "tug", 100 },
            }
        };

        public BoatShop(int ID, Vector3 Position, Utils.Vector4 EnterProperties, Utils.Vector4[] AfterBuyPositions, Utils.Vector4 PositionInteract) : base(ID, Position, EnterProperties, DefaultType, AfterBuyPositions, PositionInteract)
        {

        }
    }

    public class AeroShop : VehicleShop
    {
        public static Types DefaultType => Types.AeroShop;

        public static MaterialsData InitMaterialsData => new MaterialsData(5, 7, 50)
        {
            Prices = new Dictionary<string, uint>()
            {
                { "akula", 100 },
                { "annihilator", 100 },
                { "annihilator2", 100 },
                { "buzzard", 100 },
                { "buzzard2", 100 },
                { "cargobob", 100 },
                { "cargobob2", 100 },
                { "cargobob3", 100 },
                { "cargobob4", 100 },
                { "conada", 100 },
                { "frogger", 100 },
                { "frogger2", 100 },
                { "havok", 100 },
                { "hunter", 100 },
                { "maverick", 100 },
                { "savage", 100 },
                { "seasparrow", 100 },
                { "seasparrow2", 100 },
                { "seasparrow3", 100 },
                { "skylift", 100 },
                { "supervolito", 100 },
                { "supervolito2", 100 },
                { "swift", 100 },
                { "swift2", 100 },
                { "valkyrie", 100 },
                { "valkyrie2", 100 },
                { "volatus", 100 },
                { "polmav", 100 },
                { "alphaz1", 100 },
                { "avenger", 100 },
                { "avenger2", 100 },
                { "besra", 100 },
                { "bombushka", 100 },
                { "cargoplane", 100 },
                { "cuban800", 100 },
                { "dodo", 100 },
                { "duster", 100 },
                { "howard", 100 },
                { "hydra", 100 },
                { "jet", 100 },
                { "lazer", 100 },
                { "luxor", 100 },
                { "luxor2", 100 },
                { "mammatus", 100 },
                { "microlight", 100 },
                { "miljet", 100 },
                { "mogul", 100 },
                { "molotok", 100 },
                { "nimbus", 100 },
                { "nokota", 100 },
                { "pyro", 100 },
                { "rogue", 100 },
                { "seabreeze", 100 },
                { "shamal", 100 },
                { "starling", 100 },
                { "strikeforce", 100 },
                { "stunt", 100 },
                { "titan", 100 },
                { "tula", 100 },
                { "velum", 100 },
                { "velum2", 100 },
                { "vestra", 100 },
                { "volatol", 100 },
                { "alkonost", 100 },
            }
        };

        public AeroShop(int ID, Vector3 Position, Utils.Vector4 EnterProperties, Utils.Vector4[] AfterBuyPositions, Utils.Vector4 PositionInteract) : base(ID, Position, EnterProperties, DefaultType, AfterBuyPositions, PositionInteract)
        {

        }
    }
}
