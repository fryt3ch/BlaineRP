using BlaineRP.Client.Utils;
using RAGE;
using RAGE.Elements;
using System.Linq;
using BlaineRP.Client.Game.EntitiesData.Components;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Enums;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;

namespace BlaineRP.Client.Data.Locations
{
    public static class NorthYankton
        {
            public static void Initialize()
            {
                var iplList = new string[]
                {
                    "prologue01",
                    "prologue01c",
                    "prologue01d",
                    "prologue01e",
                    "prologue01f",
                    "prologue01g",
                    "prologue01h",
                    "prologue01i",
                    "prologue01j",
                    "prologue01k",
                    "prologue01z",
                    "prologue02",
                    "prologue03",
                    "prologue03b",
                    "prologue03_grv_dug",
                    "prologue_grv_torch",
                    "prologue04",
                    "prologue04b",
                    "prologue04_cover",
                    "des_protree_end",
                    "des_protree_start",
                    "prologue05",
                    "prologue05b",
                    "prologue06",
                    "prologue06b",
                    "prologue06_int",
                    "prologue06_pannel",
                    "plg_occl_00",
                    "prologue_occl",
                    "prologuerd",
                    "prologuerdb",
                };

                for (int i = 0; i < iplList.Length; i++)
                {
                    RAGE.Game.Streaming.RemoveIpl(iplList[i]);
                }

                var demorganCs = new Circle(new Vector3(3217.697f, -4834.826f, 111.8152f), 4500f, false, Misc.RedColor, 2, null)
                {
                    ApproveType = ApproveTypes.None,

                    OnEnter = (cancel) =>
                    {
                        if (CayoPerico.MainColshape?.IsInside == true)
                            CayoPerico.MainColshape.OnExit.Invoke(null);

                        for (int i = 0; i < iplList.Length; i++)
                        {
                            RAGE.Game.Streaming.RequestIpl(iplList[i]);
                        }

                        var intid = RAGE.Game.Interior.GetInteriorAtCoords(3217.697f, -4834.826f, 111.8152f);

                        RAGE.Game.Interior.RefreshInterior(intid);

                        var demorganCs1 = new Cylinder(new Vector3(5332.779f, -5121.378f, 70.60863f), 350f, 100f, false, Misc.RedColor, 2, null)
                        {
                            ApproveType = ApproveTypes.None,

                            OnExit = (cancel) =>
                            {
                                var demorganData = Punishment.All.Where(x => x.Type == Punishment.Types.NRPPrison).FirstOrDefault();

                                if (demorganData != null)
                                {
                                    Events.CallRemote("Player::NRPP::TPME");
                                }
                            },
                        };

                        Player.LocalPlayer.SetData("NorthYankton::DemorganTempCs", demorganCs1);
                    },

                    OnExit = (cancel) =>
                    {
                        for (int i = 0; i < iplList.Length; i++)
                        {
                            RAGE.Game.Streaming.RemoveIpl(iplList[i]);
                        }

                        var demorganCs1 = Player.LocalPlayer.GetData<ExtraColshape>("NorthYankton::DemorganTempCs");

                        if (demorganCs1 != null)
                        {
                            demorganCs1.Destroy();

                            Player.LocalPlayer.ResetData("NorthYankton::DemorganTempCs");
                        }

                        if (CayoPerico.MainColshape?.IsInside == true)
                            CayoPerico.MainColshape.OnEnter.Invoke(null);
                    },
                };
            }
        }
}
