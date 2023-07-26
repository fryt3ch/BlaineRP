using System;
using System.Collections.Generic;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.Wrappers.Colshapes.Enums;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Wrappers.Colshapes
{
    public abstract partial class ExtraColshape
    {
        private static readonly Dictionary<ApproveTypes, Func<bool>> _approveFuncs = new Dictionary<ApproveTypes, Func<bool>>()
        {
            {
                ApproveTypes.OnlyByFoot, () =>
                {
                    if (Player.LocalPlayer.Vehicle != null)
                        return false;

                    return true;
                }
            },
            {
                ApproveTypes.OnlyVehicle, () =>
                {
                    if (Player.LocalPlayer.Vehicle is Vehicle veh)
                        return true;

                    return false;
                }
            },
            {
                ApproveTypes.OnlyVehicleDriver, () =>
                {
                    if (Player.LocalPlayer.Vehicle is Vehicle veh)
                        if (veh.GetPedInSeat(-1, 0) == Player.LocalPlayer.Handle)
                            return true;

                    return false;
                }
            },
            {
                ApproveTypes.OnlyLocalVehicle, () =>
                {
                    if (Player.LocalPlayer.Vehicle is Vehicle veh)
                        if (veh.IsLocal)
                            return true;

                    return false;
                }
            },
            {
                ApproveTypes.OnlyLocalVehicleDriver, () =>
                {
                    if (Player.LocalPlayer.Vehicle is Vehicle veh)
                        if (veh.IsLocal && veh.GetPedInSeat(-1, 0) == Player.LocalPlayer.Handle)
                            return true;

                    return false;
                }
            },
            {
                ApproveTypes.OnlyServerVehicle, () =>
                {
                    if (Player.LocalPlayer.Vehicle is Vehicle veh)
                    {
                        var vData = VehicleData.GetData(veh);

                        if (vData != null)
                            return true;
                    }

                    return false;
                }
            },
            {
                ApproveTypes.OnlyServerVehicleDriver, () =>
                {
                    if (Player.LocalPlayer.Vehicle is Vehicle veh)
                    {
                        var vData = VehicleData.GetData(veh);

                        if (vData != null)
                            if (veh.GetPedInSeat(-1, 0) == Player.LocalPlayer.Handle)
                                return true;
                    }

                    return false;
                }
            },
        };
    }
}