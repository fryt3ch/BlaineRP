using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Sync
{
    public class Players
    {
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
                    var veh = pData.CurrentTuningVehicle;

                    if (veh?.Vehicle?.Exists != true)
                    {
                        player.Teleport(t.Position, false, Utils.Dimensions.Main, t.RotationZ, true);
                    }
                    else
                    {
                        if (player.Vehicle != veh.Vehicle)
                        {
                            veh.Vehicle.Teleport(t.Position, Utils.Dimensions.Main, t.RotationZ, false, Additional.AntiCheat.VehicleTeleportTypes.Default);

                            player.Teleport(t.Position, false, Utils.Dimensions.Main, t.RotationZ, true);

                            player.SetIntoVehicle(veh.Vehicle, 0);
                        }
                        else
                        {
                            veh.Vehicle.Teleport(t.Position, Utils.Dimensions.Main, t.RotationZ, true, Additional.AntiCheat.VehicleTeleportTypes.OnlyDriver);
                        }

                        veh.Tuning.Apply(veh.Vehicle);
                    }

                    pData.CurrentTuningVehicle = null;

                    player.TriggerEvent("Shop::Close::Server");
                }
                else
                {
                    if (teleport)
                        player.Teleport(t.Position, true, Utils.Dimensions.Main, t.RotationZ, true);

                    player.TriggerEvent("Shop::Close::Server");
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
