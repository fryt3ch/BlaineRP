using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;

namespace BCRPServer.Game.Businesses
{
    public class FurnitureShop : Shop
    {
        public static Types DefaultType => Types.FurnitureShop;

        public static MaterialsData InitMaterialsData => new MaterialsData(5, 7, 50)
        {
            Prices = new Dictionary<string, uint>()
            {
                // Chairs
                { "furn_91", 10 },

                // Tables
                { "furn_61", 10 },

                // Closets
                { "furn_51", 10 },

                // Plants
                { "furn_29", 10 },

                // Lamps
                { "furn_65", 10 },

                // Electronics
                { "furn_28", 10 },

                // Kitchen
                { "furn_22", 10 },

                // Bath
                { "furn_50", 10 },

                // Pictures
                { "furn_0", 10 },

                // Decores
                { "furn_6", 10 },
            }
        };

        public override string ClientData => $"{ID}, {PositionInfo.ToCSharpStr()}, {GovPrice}, {Rent}, {Tax}f, {PositionInteract.ToCSharpStr()}";

        public FurnitureShop(int ID, Vector3 PositionInfo, Utils.Vector4 PositionInteract) : base(ID, PositionInfo, PositionInteract, DefaultType)
        {

        }

        public override bool TryBuyItem(PlayerData pData, bool useCash, string itemId)
        {
            if (pData.Furniture.Count + 1 >= Settings.HOUSE_MAX_FURNITURE)
            {
                pData.Player.Notify("Inv::PMPF", Settings.HOUSE_MAX_FURNITURE);

                return false;
            }

            uint newMats;
            ulong newBalance, newPlayerBalance;

            if (!TryProceedPayment(pData, useCash, itemId, 1, out newMats, out newBalance, out newPlayerBalance))
                return false;

            var furn = new Game.Estates.Furniture(itemId);

            pData.AddFurniture(furn);

            ProceedPayment(pData, useCash, newMats, newBalance, newPlayerBalance);

            return true;
        }
    }
}