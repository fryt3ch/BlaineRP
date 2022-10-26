using GTANetworkAPI;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BCRPServer.Sync
{
    class Microphone : Script
    {
        [RemoteEvent("Microphone::Switch")]
        public static async Task MicrophoneSwitch(Player player, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            NAPI.Task.Run(() =>
            {
                if (player?.Exists != true)
                    return;

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
                    DisableMicrophone(pData);
                }
            });

            pData.Release();
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

        [RemoteEvent("mal")]
        public static async Task MicrophoneAddListener(Player player, Player target)
        {
            var sRes = player.CheckSpamAttack(0);

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(() =>
            {
                if (player?.Exists != true || target?.Exists != true || pData.VoiceRange == 0f)
                    return;

                if (!player.AreEntitiesNearby(target, pData.VoiceRange) || pData.Listeners.Contains(target))
                    return;

                player.EnableVoiceTo(target);

                pData.Listeners.Add(target);
            });

            pData.Release();
        }

        [RemoteEvent("mrl")]
        public static async Task MicrophoneRemoveListener(Player player, Player target)
        {
            var sRes = player.CheckSpamAttack(0);

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(() =>
            {
                if (player?.Exists != true || target?.Exists != true)
                    return;

                player.DisableVoiceTo(target);

                pData.Listeners.Remove(target);
            });

            pData.Release();
        }
    }
}
