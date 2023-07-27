using BlaineRP.Server.Extensions.System;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Management.Audio
{
    public static class Service
    {
        // Audio Shared Data = "/TrackType/&/Range/&/BaseVolume/&/StartTimestamp (0 - paused)/&TrackData (string.Empty if not needed)/"

        public static string GetEntityAudioData(Entity entity) => entity.GetSharedData<string>("AUD");

        public static string SetEntityAudioData(Entity entity, TrackType trackType, float range, float baseVolume, int offset, string trackData)
        {
            var audioData = $"{(int)trackType}&{range}&{baseVolume}&{Utils.GetCurrentTime().GetUnixTimestamp() + offset}&{trackData ?? string.Empty}";

            entity.SetData("AUD", audioData);

            return audioData;
        }

        public static string SetEntityAudioVolume(Entity entity, string audioData, float volume)
        {
            var audioDataArr = audioData.Split('&');

            audioData = $"{audioDataArr[0]}&{audioDataArr[1]}&{volume}&{audioDataArr[3]}&{audioDataArr[4]}";

            entity.SetData("AUD", audioData);

            return audioData;
        }

        public static string SetEntityAudioPaused(Entity entity, string audioData)
        {
            var audioDataArr = audioData.Split('&');

            audioData = $"{audioDataArr[0]}&{audioDataArr[1]}&{audioDataArr[2]}&0&{audioDataArr[4]}";

            entity.SetData("AUD", audioData);

            return audioData;
        }
    }
}
