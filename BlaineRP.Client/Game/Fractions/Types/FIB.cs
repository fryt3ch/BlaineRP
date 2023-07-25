using BlaineRP.Client.Utils;
using RAGE;
using System.Collections.Generic;
using BlaineRP.Client.Game.Wrappers;
using BlaineRP.Client.Game.Wrappers.Colshapes;
using BlaineRP.Client.Game.Wrappers.Colshapes.Enums;
using BlaineRP.Client.Game.Wrappers.Colshapes.Types;

namespace BlaineRP.Client.Data.Fractions
{
    public class FIB : Fraction, IUniformable
    {
        public FIB(Types Type, string Name, uint StorageContainerId, string ContainerPos, string CWbPos, byte MaxRank, string LockerRoomPositionsStr, string CreationWorkbenchPricesJs, uint MetaFlags) : base(Type, Name, StorageContainerId, ContainerPos, CWbPos, MaxRank, RAGE.Util.Json.Deserialize<Dictionary<string, uint>>(CreationWorkbenchPricesJs), MetaFlags)
        {
            var lockerPoses = RAGE.Util.Json.Deserialize<Vector3[]>(LockerRoomPositionsStr);

            for (int i = 0; i < lockerPoses.Length; i++)
            {
                var pos = lockerPoses[i];

                var lockerRoomCs = new Cylinder(pos, 1f, 2.5f, false, Misc.RedColor, Settings.App.Static.MainDimension, null)
                {
                    InteractionType = InteractionTypes.FractionLockerRoomInteract,

                    ActionType = ActionTypes.FractionInteract,

                    Data = $"{(int)Type}_{i}",
                };

                var lockerRoomText = new ExtraLabel(new Vector3(pos.X, pos.Y, pos.Z + 1f), "Раздевалка", new RGBA(255, 255, 255, 255), 5f, 0, false, Settings.App.Static.MainDimension)
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

    [Script(int.MaxValue)]
    public class FIBEvents
    {
        public FIBEvents()
        {

        }
    }
}