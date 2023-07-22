using GTANetworkAPI;

namespace BlaineRP.Server.Sync
{
    public class Audio
    {
        // Audio Shared Data = "/TrackType/&/Range/&/BaseVolume/&/StartTimestamp (0 - paused)/&TrackData (string.Empty if not needed)/"

        public enum TrackTypes
        {
            None = 0,

            Auth1, Auth2, Auth3,

            RadioRetroFM, RadioEuropaPlus, RadioClassicFM, RadioRecord, RadioSynthwave,
        }

        public string GetEntityAudioData(Entity entity) => entity.GetSharedData<string>("AUD");

        public string SetEntityAudioData(Entity entity, TrackTypes trackType, float range, float baseVolume, int offset, string trackData)
        {
            var audioData = $"{(int)trackType}&{range}&{baseVolume}&{Utils.GetCurrentTime().GetUnixTimestamp() + offset}&{trackData ?? string.Empty}";

            entity.SetData("AUD", audioData);

            return audioData;
        }

        public string SetEntityAudioVolume(Entity entity, string audioData, float volume)
        {
            var audioDataArr = audioData.Split('&');

            audioData = $"{audioDataArr[0]}&{audioDataArr[1]}&{volume}&{audioDataArr[3]}&{audioDataArr[4]}";

            entity.SetData("AUD", audioData);

            return audioData;
        }

        public string SetEntityAudioPaused(Entity entity, string audioData)
        {
            var audioDataArr = audioData.Split('&');

            audioData = $"{audioDataArr[0]}&{audioDataArr[1]}&{audioDataArr[2]}&0&{audioDataArr[4]}";

            entity.SetData("AUD", audioData);

            return audioData;
        }
    }
}
