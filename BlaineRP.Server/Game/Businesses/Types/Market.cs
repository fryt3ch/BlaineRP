﻿using GTANetworkAPI;
using System.Collections.Generic;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Businesses
{
    public class Market : Shop
    {
        public static BusinessType DefaultType => BusinessType.Market;

        public static MaterialsData InitMaterialsData => new MaterialsData(5, 7, 50)
        {
            Prices = new Dictionary<string, uint>()
            {
                { "f_burger", 100 },

                { "med_b_0", 100 },
                { "med_kit_0", 100 },
                { "med_kit_1", 100 },

                { "cigs_0", 100 },
                { "cigs_1", 100 },
                { "cig_0", 100 },
                { "cig_1", 100 },
            }
        };

        public override string ClientData => $"{ID}, {PositionInfo.ToCSharpStr()}, {GovPrice}, {Rent}, {Tax}f, {PositionInteract.ToCSharpStr()}";

        public Market(int ID, Vector3 PositionInfo, Vector4 PositionInteract) : base(ID, PositionInfo, PositionInteract, DefaultType)
        {

        }
    }
}
