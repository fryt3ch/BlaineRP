using BlaineRP.Server.EntitiesData.Players;
using BlaineRP.Server.EntitiesData.Vehicles;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Businesses
{
    public abstract class VehicleShop : Shop, IEnterable
    {
        public override string ClientData => $"{ID}, {PositionInfo.ToCSharpStr()}, {GovPrice}, {Rent}, {Tax}f, {PositionInteract.ToCSharpStr()}";

        public Vector4 EnterProperties { get; set; }

        public Vector4[] AfterBuyPositions { get; set; }

        public Vector4[] ExitProperties { get; set; }

        public int LastExitUsed { get; set; }

        public int LastAfterBuyExitUsed { get; set; }

        public VehicleShop(int ID, Vector3 PositionInfo, Vector4 EnterProperties, BusinessType Type, Vector4[] AfterBuyPositions, Vector4 PositionInteract) : base(ID, PositionInfo, PositionInteract, Type)
        {
            this.EnterProperties = EnterProperties;

            this.AfterBuyPositions = AfterBuyPositions;

            this.ExitProperties = new Vector4[] { new Vector4(PositionInteract.Position.GetFrontOf(PositionInteract.RotationZ, 1.5f), Utils.GetOppositeAngle(PositionInteract.RotationZ)) };
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

            if (Type >= BusinessType.CarShop1 && Type <= BusinessType.CarShop3)
            {
                if (!pData.HasLicense(LicenseType.B))
                    return false;
            }
            else if (Type == BusinessType.MotoShop)
            {
                if (!pData.HasLicense(LicenseType.A))
                    return false;
            }
            else if (Type == BusinessType.BoatShop)
            {
                if (!pData.HasLicense(LicenseType.Sea))
                    return false;
            }
            else if (Type == BusinessType.AeroShop)
            {
                if (!pData.HasLicense(LicenseType.Fly))
                    return false;
            }

            if (pData.FreeVehicleSlots <= 0)
            {
                pData.Player.Notify("Trade::MVOW", pData.OwnedVehicles.Count);

                return false;
            }

            ProceedPayment(pData, useCash, newMats, newBalance, newPlayerBalance);

            var vPos = AfterBuyPositions[AfterBuyPositions.Length == 1 ? 0 : AfterBuyPositions.Length < LastExitUsed + 1 ? ++LastExitUsed : LastExitUsed = 0];

            var vData = VehicleData.New(pData, vType, new Colour(r1, g1, b1), new Colour(r2, g2, b2), vPos.Position, vPos.RotationZ, Properties.Settings.Static.MainDimension, true);

            Sync.Players.ExitFromBusiness(pData, false);

            pData.Player.Teleport(vPos.Position, false, Properties.Settings.Static.MainDimension, vPos.RotationZ, true);

            return true;
        }
    }
}
