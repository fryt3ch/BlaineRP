﻿using System.Collections.Generic;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Misc
{
    public partial class EstateAgency
    {
        public static void InitializeAll()
        {
            if (All != null)
                return;

            All = new EstateAgency[]
            {
                new EstateAgency()
                {
                    Positions = new Vector3[]
                    {
                        new Vector3(-707.7374f, 265.9574f, 83.14727f),
                        new Vector3(-704.3141f, 267.6302f, 83.14727f),
                        new Vector3(-707.1259f, 270.7521f, 83.14727f),
                    },

                    HouseGPSPrice = 250,
                },

                new EstateAgency()
                {
                    Positions = new Vector3[]
                    {
                        new Vector3(-140.6281f, 6298.156f, 35.69427f),
                        new Vector3(-144.4867f, 6284.27f, 35.67945f),
                        new Vector3(-149.5213f, 6289.296f, 35.67948f),
                    },

                    HouseGPSPrice = 150,
                },
            };

            var lines = new List<string>();

            for (int i = 0; i < All.Length; i++)
            {
                lines.Add($"new {nameof(BlaineRP.Client.Game.Misc.EstateAgency)}({i}, \"{All[i].Positions.SerializeToJson().Replace('\"', '\'')}\");");
            }

            Utils.FillFileToReplaceRegion(System.IO.Directory.GetCurrentDirectory() +
                                          Properties.Settings.Static.ClientScriptsTargetPath +
                                          @"\Game\Misc\EstateAgency.Initialization.cs",
                "TO_REPLACE",
                lines
            );
        }
    }
}