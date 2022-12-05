using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Events.Players
{
    class Microphone : Script
    {
        [RemoteEvent("Microphone::Switch")]
        private static void MicrophoneSwitch(Player player, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsMuted)
                return;

            if (state)
            {
                var vRange = Settings.MICROPHONE_MAX_RANGE_DEFAULT;

                if (pData.VoiceRange == vRange)
                    return;

                pData.VoiceRange = vRange;
            }
            else
            {
                Sync.Players.DisableMicrophone(pData);
            }
        }

        [RemoteEvent("mal")]
        private static void MicrophoneAddListener(Player player, Player target)
        {
            var pData = PlayerData.Get(player);

            if (target?.Exists != true || pData.VoiceRange == 0f)
                return;

            if (!player.AreEntitiesNearby(target, pData.VoiceRange) || pData.Listeners.Contains(target))
                return;

            player.EnableVoiceTo(target);

            pData.Listeners.Add(target);
        }

        [RemoteEvent("mrl")]
        private static void MicrophoneRemoveListener(Player player, Player target)
        {
            var pData = PlayerData.Get(player);

            if (target?.Exists != true)
                return;

            player.DisableVoiceTo(target);

            pData.Listeners.Remove(target);
        }
    }
}
