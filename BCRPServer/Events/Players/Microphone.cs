using GTANetworkAPI;

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

            var vRange = pData.VoiceRange;

            if (vRange < 0f)
                return;

            if (state)
            {
                if (vRange > 0f)
                    return;

                var nvRange = Properties.Settings.Static.MICROPHONE_MAX_RANGE_DEFAULT;

                pData.VoiceRange = nvRange;

                if (pData.ActiveCall is Sync.Phone.Call activeCall && activeCall.StatusType == Sync.Phone.Call.StatusTypes.Process)
                {
                    var callTarget = activeCall.Caller == pData ? activeCall.Receiver : activeCall.Caller;

                    if (callTarget.Player?.Exists == true)
                    {
                        player.EnableVoiceTo(callTarget.Player);

                        pData.Listeners.Add(callTarget.Player);
                    }
                }
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

            if (pData == null || target?.Exists != true)
                return;

            var voiceRange = pData.VoiceRange;

            if (voiceRange <= 0f)
                return;

            if (pData.Listeners.Contains(target) || !player.IsNearToEntity(target, voiceRange + 5f))
                return;

            player.EnableVoiceTo(target);

            pData.Listeners.Add(target);
        }

        [RemoteEvent("mrl")]
        private static void MicrophoneRemoveListener(Player player, Player target)
        {
            var pData = PlayerData.Get(player);

            if (pData == null || target == null || !pData.Listeners.Remove(target))
                return;

            player.DisableVoiceTo(target);
        }
    }
}
