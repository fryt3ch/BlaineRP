using BlaineRP.Client.Game.Management;

namespace BlaineRP.Client.Settings.User
{
    public static class Audio
    {
        private static int _VoiceVolume;
        private static int _SoundVolume;
        private static float _PlayerLocalRadioVolume;

        public static int VoiceVolume
        {
            get => _VoiceVolume;
            set
            {
                if (value < 0)
                    value = 0;
                else if (value > 100)
                    value = 100;
                if (value != _VoiceVolume)
                    RageStorage.SetData("Settings::Audio::VoiceVolume", value);
                _VoiceVolume = value;
            }
        }

        public static int SoundVolume
        {
            get => _SoundVolume;
            set
            {
                if (value < 0)
                    value = 0;
                else if (value > 100)
                    value = 100;
                if (value != _SoundVolume)
                    RageStorage.SetData("Settings::Audio::SoundVolume", value);
                _SoundVolume = value;
            }
        }

        public static float PlayerLocalRadioVolume
        {
            get => _PlayerLocalRadioVolume;
            set
            {
                if (value < 0f)
                    value = 0f;
                else if (value > 1f)
                    value = 1f;
                if (value != _PlayerLocalRadioVolume)
                    RageStorage.SetData("Settings::Audio::PLRVolume", value);
                _PlayerLocalRadioVolume = value;
            }
        }

        public static class Default
        {
            public static int VoiceVolume = 100;
            public static int SoundVolume = 50;
            public static float PlayerLocalRadioVolume = 0.5f;
        }
    }
}