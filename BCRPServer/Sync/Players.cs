﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Sync
{
    public class Players
    {
        public static HashSet<uint> UsedPhoneNumbers { get; private set; } = new HashSet<uint>();

        public static uint GenerateNewPhoneNumber()
        {
            while (true)
            {
                var num = (uint)Utils.Randoms.Chat.Next(100_000, 999_999_999);

                if (!UsedPhoneNumbers.Contains(num))
                {
                    UsedPhoneNumbers.Add(num);

                    return num;
                }
            }
        }

        public static void DisableMicrophone(PlayerData pData)
        {
            if (pData.VoiceRange == 0f)
                return;

            pData.VoiceRange = 0f;

            var player = pData.Player;

            for (int i = 0; i < pData.Listeners.Count; i++)
            {
                var target = pData.Listeners[i];

                if (target == null)
                    continue;

                player.DisableVoiceTo(target);
            }

            pData.Listeners.Clear();
        }

        public static void StopUsePhone(PlayerData pData)
        {
            var player = pData.Player;

            if (!pData.PhoneOn)
                return;

            pData.PhoneOn = false;

            var attachedPhone = pData.AttachedObjects.Where(x => x.Type == AttachSystem.Types.Phone).FirstOrDefault();

            if (attachedPhone != null)
                player.DetachObject(attachedPhone.Type);

            Sync.Chat.SendLocal(Sync.Chat.Types.Me, player, Locale.Chat.Player.PhoneOff);
        }

        public static void ExitFromBuiness(PlayerData pData, bool teleport = true)
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
                            player.Teleport(t.Position, false, Utils.Dimensions.Main, t.RotationZ, true);
                    }
                    else
                    {
                        if (player.Vehicle != veh.Vehicle || !teleport)
                        {
                            if (teleport)
                            {
                                player.Teleport(t.Position, false, Utils.Dimensions.Main, t.RotationZ, true);

                                player.WarpToVehicleSeat(veh.Vehicle, 0, 5000);
                            }

                            veh.AttachBoatToTrailer();

                            veh.Vehicle.Teleport(t.Position, Utils.Dimensions.Main, t.RotationZ, false, Additional.AntiCheat.VehicleTeleportTypes.Default);
                        }
                        else
                        {
                            veh.AttachBoatToTrailer();

                            veh.Vehicle.Teleport(t.Position, Utils.Dimensions.Main, t.RotationZ, true, Additional.AntiCheat.VehicleTeleportTypes.OnlyDriver);
                        }

                        veh.Tuning.Apply(veh.Vehicle);
                    }

                    pData.CurrentTuningVehicle = null;
                }
                else
                {
                    if (teleport)
                        player.Teleport(t.Position, true, Utils.Dimensions.Main, t.RotationZ, true);

                    player.TriggerEvent("Shop::Close::Server");

                    if (business.Type == Game.Businesses.Business.Types.BarberShop)
                        pData.UpdateCustomization();
                    else if (business.Type == Game.Businesses.Business.Types.TattooShop)
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
