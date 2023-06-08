using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient.Data.Fractions
{
    public class Government : Fraction, IUniformable
    {
        public Government(Types Type, string Name, uint StorageContainerId, string ContainerPos, string CWbPos, byte MaxRank, string LockerRoomPositionsStr, string CreationWorkbenchPricesJs) : base(Type, Name, StorageContainerId, ContainerPos, CWbPos, MaxRank, RAGE.Util.Json.Deserialize<Dictionary<string, uint>>(CreationWorkbenchPricesJs))
        {
            var lockerPoses = RAGE.Util.Json.Deserialize<Vector3[]>(LockerRoomPositionsStr);

            for (int i = 0; i < lockerPoses.Length; i++)
            {
                var pos = lockerPoses[i];

                var lockerRoomCs = new Additional.Cylinder(pos, 1f, 2.5f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                {
                    InteractionType = Additional.ExtraColshape.InteractionTypes.FractionLockerRoomInteract,

                    ActionType = Additional.ExtraColshape.ActionTypes.FractionInteract,

                    Data = $"{(int)Type}_{i}",
                };

                var lockerRoomText = new Additional.ExtraLabel(new Vector3(pos.X, pos.Y, pos.Z + 1f), "Раздевалка", new RGBA(255, 255, 255, 255), 5f, 0, false, Settings.MAIN_DIMENSION)
                {
                    Font = 0,
                };
            }

            if (Type == Types.GOV_LS)
            {
                UniformNames = new string[]
                {
                    "Стандартная форма",
                    "Форма для специальных операций",
                    "Форма руководства",
                };
            }
        }

        public string[] UniformNames { get; set; }

        public override void OnStartMembership(params object[] args)
        {
            base.OnStartMembership(args);
        }

        public override void OnEndMembership()
        {
            base.OnEndMembership();
        }
    }

    public class GovernmentEvents : Events.Script
    {
        public GovernmentEvents()
        {

        }
    }
}