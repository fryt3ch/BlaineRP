﻿using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Game.Businesses
{
    public class FurnitureShop : Shop
    {
        public static Types DefaultType => Types.FurnitureShop;

        public static MaterialsData InitMaterialsData => new MaterialsData(5, 7, 50)
        {
            Prices = new Dictionary<string, int>()
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

        public enum SubTypes
        {
            /// <summary>Кресла, стулья</summary>
            Chairs = 0,
            /// <summary>Столы</summary>
            Tables,
            /// <summary>Кровати</summary>
            Beds,
            /// <summary>Шкафы</summary>
            Closets,
            /// <summary>Растения</summary>
            Plants,
            /// <summary>Лампы</summary>
            Lamps,
            /// <summary>Электроника</summary>
            Electronics,
            /// <summary>Все для кухни</summary>
            Kitchen,
            /// <summary>Все для ванной</summary>
            Bath,
            /// <summary>Картины</summary>
            Pictures,
            /// <summary>Прочий декор</summary>
            Decores,
        }

        public override string ClientData => $"{ID}, {PositionInfo.ToCSharpStr()}, {GovPrice}, {Rent}, {Tax}f, {PositionInteract.ToCSharpStr()}, new List<Vector3>(){{{string.Join(',', PositionsInteract.Select(x => x.ToCSharpStr()))}}}";

        public List<Vector3> PositionsInteract { get; private set; }

        public FurnitureShop(int ID, Vector3 PositionInfo, Utils.Vector4 PositionInteract, List<Vector3> PositionsInteract) : base(ID, PositionInfo, PositionInteract, DefaultType)
        {
            this.PositionsInteract = PositionsInteract;
        }

        public override bool BuyItem(PlayerData pData, bool useCash, string itemId)
        {
            var iData = itemId.Split('&');

            if (iData.Length != 1)
                return false;

            var res = CanBuy(pData, useCash, iData[0], 1);

            if (res == null)
                return false;

            if (pData.Furniture.Count + 1 >= Settings.HOUSE_MAX_FURNITURE)
            {


                return false;
            }

            var furn = new Game.Estates.Furniture(iData[0]);

            pData.AddFurniture(furn);

            PaymentProceed(pData, useCash, res.Value.MatPrice, res.Value.RealPrice);

            return true;
        }
    }
}