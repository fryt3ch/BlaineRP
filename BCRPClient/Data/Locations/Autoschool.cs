﻿using RAGE;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.Data
{
    public partial class Locations
    {
        public class Autoschool
        {
            public static Dictionary<Sync.Players.LicenseTypes, uint> Prices { get; set; }

            public static List<Autoschool> All { get; private set; } = new List<Autoschool>();

            public static Autoschool Get(int id) => id < 1 || id > All.Count ? null : All[id - 1];

            public Dictionary<Sync.Players.LicenseTypes, Vector3[]> PracticeRoutes { get; set; }

            public Dictionary<Sync.Players.LicenseTypes, Vector3> VehiclesPositions { get; set; }

            public int Id => All.IndexOf(this) + 1;

            public Autoschool(Vector3 Position, string PracticeRoutesStr, string VehiclesPositionsStr)
            {
                All.Add(this);

                this.PracticeRoutes = RAGE.Util.Json.Deserialize<Dictionary<Sync.Players.LicenseTypes, Vector3[]>>(PracticeRoutesStr);

                this.VehiclesPositions = RAGE.Util.Json.Deserialize<Dictionary<Sync.Players.LicenseTypes, Vector3>>(VehiclesPositionsStr);

                var id = Id;

                var cs = new Additional.Cylinder(new Vector3(Position.X, Position.Y, Position.Z), 1.5f, 2.5f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                {
                    InteractionType = Additional.ExtraColshape.InteractionTypes.DrivingSchoolInteract,

                    ActionType = Additional.ExtraColshape.ActionTypes.DrivingSchoolInteract,

                    Data = id,
                };

                var marker = new RAGE.Elements.Marker(32, new Vector3(Position.X, Position.Y, Position.Z + 1f), 1f, Vector3.Zero, Vector3.Zero, new RGBA(255, 255, 255, 255), true, Settings.MAIN_DIMENSION);

                var blip = new RAGE.Elements.Blip(545, Position, "Автошкола", 1f, 3, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);
            }

            public static Sync.Players.LicenseTypes GetLicenseTypeForPracticeRoute(Sync.Players.LicenseTypes licType) => licType == Sync.Players.LicenseTypes.B || licType == Sync.Players.LicenseTypes.A || licType == Sync.Players.LicenseTypes.C || licType == Sync.Players.LicenseTypes.D ? Sync.Players.LicenseTypes.B : licType;
        }
    }
}
