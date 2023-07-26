using System.Collections.Generic;
using BlaineRP.Client.Game.Helpers;
using BlaineRP.Client.Game.Helpers.Colshapes.Enums;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using RAGE;

namespace BlaineRP.Client.Game.Fractions
{
    public class FIB : Fraction, IUniformable
    {
        public FIB(FractionTypes type, string name, uint storageContainerId, string containerPos, string cWbPos, byte maxRank, string lockerRoomPositionsStr, string creationWorkbenchPricesJs, uint metaFlags) : base(type, name, storageContainerId, containerPos, cWbPos, maxRank, RAGE.Util.Json.Deserialize<Dictionary<string, uint>>(creationWorkbenchPricesJs), metaFlags)
        {
            var lockerPoses = RAGE.Util.Json.Deserialize<Vector3[]>(lockerRoomPositionsStr);

            for (int i = 0; i < lockerPoses.Length; i++)
            {
                var pos = lockerPoses[i];

                var lockerRoomCs = new Cylinder(pos, 1f, 2.5f, false, Utils.Misc.RedColor, Settings.App.Static.MainDimension, null)
                {
                    InteractionType = InteractionTypes.FractionLockerRoomInteract,

                    ActionType = ActionTypes.FractionInteract,

                    Data = $"{(int)type}_{i}",
                };

                var lockerRoomText = new ExtraLabel(new Vector3(pos.X, pos.Y, pos.Z + 1f), "Раздевалка", new RGBA(255, 255, 255, 255), 5f, 0, false, Settings.App.Static.MainDimension)
                {
                    Font = 0,
                };
            }

            if (type == FractionTypes.GOV_LS)
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

    [Script(int.MaxValue)]
    public class FIBEvents
    {
        public FIBEvents()
        {

        }
    }
}