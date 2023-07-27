using BlaineRP.Server.EntitiesData.Players;

namespace BlaineRP.Server.Game.Management.Audio
{
    public static partial class Microphone
    {
        public static void DisableMicrophone(PlayerData pData)
        {
            if (pData.VoiceRange <= 0f)
                return;

            pData.VoiceRange = 0f;

            pData.RemoveAllMicrophoneListeners();
        }
    }
}