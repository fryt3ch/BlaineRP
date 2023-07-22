using GTANetworkAPI;
using System;
using System.Collections.Generic;

namespace BlaineRP.Server.Game.Businesses
{
    public class GasStation : Shop
    {
        public static Types DefaultType => Types.GasStation;

        public static MaterialsData InitMaterialsData => new MaterialsData(1, 2, 5)
        {
            Prices = new Dictionary<string, uint>()
            {
                { "gas_g_0", 1 },

                { "gas_e_0", 0 },
            }
        };

        public override string ClientData => $"{ID}, {PositionInfo.ToCSharpStr()}, {GovPrice}, {Rent}, {Tax}f, {GasolinesPosition.ToCSharpStr()}, {PositionInteract.ToCSharpStr()}";

        public Utils.Vector4 GasolinesPosition { get; set; }

        public GasStation(int ID, Vector3 PositionInfo, Utils.Vector4 PositionInteract, Utils.Vector4 GasolinesPosition) : base(ID, PositionInfo, PositionInteract, DefaultType)
        {
            this.GasolinesPosition = GasolinesPosition;
        }

        public static string GetGasBuyIdByFuelType(Data.Vehicles.Vehicle.FuelTypes fType) => fType == Data.Vehicles.Vehicle.FuelTypes.Electricity ? "gas_e_0" : "gas_g_0";

        public bool IsPlayerNearGasolinesPosition(PlayerData pData) => pData.Player.Dimension == Properties.Settings.Static.MainDimension && pData.Player.Position.DistanceTo(GasolinesPosition.Position) <= GasolinesPosition.RotationZ + 2.5f;

        public override bool TryBuyItem(PlayerData pData, bool useCash, string itemId)
        {
            var iData = itemId.Split('&');

            if (iData.Length != 4)
                return false;

            var item = iData[0];

            ushort vehicleRid;

            uint amount;

            if (!ushort.TryParse(iData[1], out vehicleRid) || !uint.TryParse(iData[2], out amount))
                return false;

            bool payByFraction = iData[3] == "1";

            var vData = Utils.FindVehicleOnline(vehicleRid);

            if (vData == null)
                return false;

            if (!pData.Player.IsNearToEntity(vData.Vehicle, 7.5f))
                return false;

            var vFuelType = vData.Data.FuelType;

            if (GetGasBuyIdByFuelType(vFuelType) != item)
                return false;

            var newFuelLevel = vData.FuelLevel + amount;

            if (newFuelLevel > vData.Data.Tank)
            {
                amount = (uint)Math.Ceiling(vData.Data.Tank - vData.FuelLevel);

                newFuelLevel = vData.Data.Tank;

                if (amount == 0)
                {
                    pData.Player.Notify(vFuelType == Game.Data.Vehicles.Vehicle.FuelTypes.Petrol ? "Vehicle::FOFP" : "Vehicle::FOFE");

                    return false;
                }
            }

            if (payByFraction)
            {
                if (!Game.Fractions.Fraction.IsMemberOfAnyFraction(pData, true))
                    return false;

                var fData = Game.Fractions.Fraction.Get(pData.Fraction);

                if (!fData.HasMemberPermission(pData.Info, 10_000, true))
                    return false;

                uint newMats;
                ulong newBalance, newFractionBalance;

                if (!TryProceedPaymentByFraction(pData, fData, item, amount, out newMats, out newBalance, out newFractionBalance))
                    return false;

                ProceedPaymentByFraction(pData, fData, newMats, newBalance, newFractionBalance);
            }
            else
            {
                uint newMats;
                ulong newBalance, newPlayerBalance;

                if (!TryProceedPayment(pData, useCash, item, amount, out newMats, out newBalance, out newPlayerBalance))
                    return false;

                ProceedPayment(pData, useCash, newMats, newBalance, newPlayerBalance);
            }

            vData.FuelLevel = newFuelLevel;

            pData.CurrentBusiness = null;

            pData.Player.CloseAll(true);

            return base.TryBuyItem(pData, useCash, itemId);
        }
    }
}
