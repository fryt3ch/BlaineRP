using System.Collections.Generic;
using BlaineRP.Client.Game.Animations;
using BlaineRP.Client.Game.Management;
using BlaineRP.Client.Game.UI.CEF;

namespace BlaineRP.Client.Settings.User
{
    public static class Other
    {
        public static class Default
        {
            public static bool AutoTeleportMarker = false;
            public static bool DebugLabels = false;
            public static bool HighPolygonsMode = false;
            public static bool ColshapesVisible = false;
            public static bool RaytraceEnabled = false;

            public static int PhoneWallpaperNum = 1;
            public static bool PhoneNotDisturb = false;

            public static HashSet<string> FavoriteAnimations => new HashSet<string>();
            public static EmotionTypes CurrentEmotion = EmotionTypes.None;
            public static WalkstyleTypes CurrentWalkstyle = WalkstyleTypes.None;

            public static List<BlipsMenu.LocalBlip> LocalBlips => new List<BlipsMenu.LocalBlip>();
        }

        private static HashSet<string> _FavoriteAnimations;
        private static EmotionTypes _CurrentEmotion;
        private static WalkstyleTypes _CurrentWalkstyle;

        private static List<BlipsMenu.LocalBlip> _LocalBlips;

        private static bool _AutoTeleportMarker;
        private static bool _DebugLabels;
        private static bool _HighPolygonsMode;
        private static bool _ColshapesVisible;
        private static bool _RaytraceEnabled;

        private static int _PhoneWallpaperNum;
        private static bool _PhoneNotDisturb;

        public static bool AutoTeleportMarker { get => _AutoTeleportMarker; set { if (value != _AutoTeleportMarker) RageStorage.SetData("Settings::Other::AutoTeleportMarker", value); _AutoTeleportMarker = value; } }
        public static bool DebugLabels { get => _DebugLabels; set { if (value != _DebugLabels) RageStorage.SetData("Settings::Other::DebugLabels", value); _DebugLabels = value; } }
        public static bool HighPolygonsMode { get => _HighPolygonsMode; set { if (value != _HighPolygonsMode) RageStorage.SetData("Settings::Other::HighPolygonsMode", value); _HighPolygonsMode = value; } }
        public static bool ColshapesVisible { get => _ColshapesVisible; set { if (value != _ColshapesVisible) RageStorage.SetData("Settings::Other::ColshapesVisible", value); _ColshapesVisible = value; } }
        public static bool RaytraceEnabled { get => _RaytraceEnabled; set { if (value != _RaytraceEnabled) RageStorage.SetData("Settings::Other::RaytraceEnabled", value); _RaytraceEnabled = value; } }

        public static int PhoneWallpaperNum { get => _PhoneWallpaperNum; set { if (value != _PhoneWallpaperNum) RageStorage.SetData("Settings::Phone::WallpaperNum", value); _PhoneWallpaperNum = value; } }
        public static bool PhoneNotDisturb { get => _PhoneNotDisturb; set { if (value != _PhoneNotDisturb) RageStorage.SetData("Settings::Phone::NotDisturb", value); _PhoneNotDisturb = value; } }

        public static HashSet<string> FavoriteAnimations { get => _FavoriteAnimations; set { RageStorage.SetData("Settings::Animations::Favorites", value); _FavoriteAnimations = value; } }
        public static EmotionTypes CurrentEmotion { get => _CurrentEmotion; set { if (value != _CurrentEmotion) RageStorage.SetData("Settings::Animations::Emotion", value.ToString()); _CurrentEmotion = value; } }
        public static WalkstyleTypes CurrentWalkstyle { get => _CurrentWalkstyle; set { if (value != _CurrentWalkstyle) RageStorage.SetData("Settings::Animations::Walkstyle", value.ToString()); _CurrentWalkstyle = value; } }

        public static List<BlipsMenu.LocalBlip> LocalBlips { get => _LocalBlips; set { RageStorage.SetData("Settings::LocalBlips", value); _LocalBlips = value; } }
    }
}
