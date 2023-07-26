namespace BlaineRP.Client.Settings.User
{
    public static class Native
    {
        public static bool Audio_MuteAudioOnFocusLoss => RAGE.Game.Misc.GetProfileSetting(318) > 0;
        public static int Audio_MusicVolume => RAGE.Game.Misc.GetProfileSetting(306); // 0-10
        public static int Audio_SFXVolume => RAGE.Game.Misc.GetProfileSetting(300); // 0-10
    }
}