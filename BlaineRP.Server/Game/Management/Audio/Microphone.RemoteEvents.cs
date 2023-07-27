using BlaineRP.Server.EntitiesData.Players;
using BlaineRP.Server.Game.Management.Phone;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Management.Audio
{
    public static partial class Microphone
    {
        internal class RemoteEvents : Script
        {
            [RemoteEvent("Microphone::Switch")]
            private static void MicrophoneSwitch(Player player, bool state)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                float vRange = pData.VoiceRange;

                if (vRange < 0f)
                    return;

                if (state)
                {
                    if (vRange > 0f)
                        return;

                    float nvRange = Properties.Settings.Static.MICROPHONE_MAX_RANGE_DEFAULT;

                    pData.VoiceRange = nvRange;

                    if (pData.ActiveCall is Call activeCall && activeCall.StatusType == Call.StatusTypes.Process)
                    {
                        var callTarget = activeCall.Caller == pData ? activeCall.Receiver : activeCall.Caller;

                        if (callTarget.Player?.Exists == true)
                            pData.AddMicrophoneListener(callTarget.Player);
                    }
                }
                else
                {
                    Game.Management.Audio.Microphone.DisableMicrophone(pData);
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

                if (!player.IsNearToEntity(target, voiceRange + 5f))
                    return;

                pData.AddMicrophoneListener(target);
            }

            [RemoteEvent("mrl")]
            private static void MicrophoneRemoveListener(Player player, Player target)
            {
                var pData = PlayerData.Get(player);

                if (pData == null || target == null)
                    return;

                pData.RemoveMicrophoneListener(target);
            }
        }
    }
}