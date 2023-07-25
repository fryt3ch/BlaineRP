using BlaineRP.Client.Utils;
using RAGE;
using System.Collections.Generic;
using BlaineRP.Client.Game.Wrappers.Blips;
using BlaineRP.Client.Game.Wrappers.Colshapes;
using BlaineRP.Client.Game.Wrappers.Colshapes.Enums;
using BlaineRP.Client.Game.Wrappers.Colshapes.Types;

namespace BlaineRP.Client.Data
{
    public partial class Locations
    {
        public class EstateAgency
        {
            public EstateAgency(int id, string PositionsStr)
            {
                var poses = RAGE.Util.Json.Deserialize<List<Vector3>>(PositionsStr);

                var centerPos = new Vector3(0f, 0f, 0f);

                for (int i = 0; i < poses.Count; i++)
                {
                    var x = poses[i];

                    centerPos += x;

                    var cs = new Cylinder(new Vector3(x.X, x.Y, x.Z - 1f), 1.5f, 2f, false, Misc.RedColor, Settings.App.Static.MainDimension, null)
                    {
                        InteractionType = InteractionTypes.EstateAgencyInteract,

                        Data = $"{id}_{i}",

                        ActionType = ActionTypes.EstateAgencyInteract,
                    };

                    var marker = new RAGE.Elements.Marker(2, new Vector3(x.X, x.Y, x.Z - 0.5f), 1f, Vector3.Zero, Vector3.Zero, new RGBA(255, 255, 255, 125), true, Settings.App.Static.MainDimension);
                }

                centerPos /= poses.Count;

                var blip = new ExtraBlip(837, centerPos, "Агенство недвижимости", 1f, 2, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);
            }
        }
    }
}
