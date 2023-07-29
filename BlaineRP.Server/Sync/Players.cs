using System.Collections.Generic;
using BlaineRP.Server.Additional;
using BlaineRP.Server.Game.Attachments;
using BlaineRP.Server.Game.Businesses;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.Management.AntiCheat;
using BlaineRP.Server.Game.Management.Chat;
using BlaineRP.Server.Game.Phone;

namespace BlaineRP.Server.Sync
{
    public static class Players
    {
        public static void StopUsePhone(PlayerData pData)
        {
            var player = pData.Player;

            pData.PhoneStateType = PlayerPhoneState.Off;

            if (pData.ActiveCall is Call activeCall)
            {
                activeCall.Cancel(activeCall.Caller == pData ? Call.CancelTypes.Caller : Call.CancelTypes.Receiver);
            }

            player.DetachObject(AttachmentType.PhoneSync);

            Game.Management.Chat.Service.SendLocal(MessageType.Me, player, Language.Strings.Get("CHAT_PLAYER_PHONE_OFF"));
        }

        public static void ExitFromBusiness(PlayerData pData, bool teleport = true)
        {
            var player = pData.Player;

            var business = pData.CurrentBusiness;

            if (business is Game.Businesses.IEnterable enterable)
            {
                pData.IsInvincible = false;

                pData.CurrentBusiness = null;

                var t = Game.Businesses.Business.GetNextExitProperty(enterable);

                if (business is Game.Businesses.TuningShop ts)
                {
                    player.TriggerEvent("Shop::Close::Server");

                    var veh = pData.CurrentTuningVehicle;

                    if (veh?.Vehicle?.Exists != true)
                    {
                        if (teleport)
                            player.Teleport(t.Position, false, Properties.Settings.Static.MainDimension, t.RotationZ, true);
                    }
                    else
                    {
                        if (player.Vehicle != veh.Vehicle || !teleport)
                        {
                            if (teleport)
                            {
                                player.Teleport(t.Position, false, Properties.Settings.Static.MainDimension, t.RotationZ, true);

                                player.WarpToVehicleSeat(veh.Vehicle, 0, 5000);
                            }

                            veh.Vehicle.Teleport(t.Position, Properties.Settings.Static.MainDimension, t.RotationZ, false, VehicleTeleportType.Default);

                            veh.AttachBoatToTrailer();
                        }
                        else
                        {
                            veh.Vehicle.Teleport(t.Position, Properties.Settings.Static.MainDimension, t.RotationZ, true, VehicleTeleportType.OnlyDriver);

                            veh.AttachBoatToTrailer();
                        }

                        veh.Tuning.Apply(veh.Vehicle);
                    }

                    pData.CurrentTuningVehicle = null;
                }
                else
                {
                    if (teleport)
                        player.Teleport(t.Position, true, Properties.Settings.Static.MainDimension, t.RotationZ, true);

                    player.TriggerEvent("Shop::Close::Server");

                    if (business.Type == BusinessType.BarberShop)
                        pData.UpdateCustomization();
                    else if (business.Type == BusinessType.TattooShop)
                        pData.UpdateDecorations();
                }
            }
            else
            {
                pData.CurrentBusiness = null;

                player.TriggerEvent("Shop::Close::Server");
            }
        }
    }
}
