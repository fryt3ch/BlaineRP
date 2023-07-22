using System;
using System.Collections.Generic;
using System.Text;

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
            public static Sync.Animations.EmotionTypes CurrentEmotion = Sync.Animations.EmotionTypes.None;
            public static Sync.Animations.WalkstyleTypes CurrentWalkstyle = Sync.Animations.WalkstyleTypes.None;

            public static List<CEF.BlipsMenu.LocalBlip> LocalBlips => new List<CEF.BlipsMenu.LocalBlip>();
        }

        private static HashSet<string> _FavoriteAnimations;
        private static Sync.Animations.EmotionTypes _CurrentEmotion;
        private static Sync.Animations.WalkstyleTypes _CurrentWalkstyle;

        private static List<CEF.BlipsMenu.LocalBlip> _LocalBlips;

        private static bool _AutoTeleportMarker;
        private static bool _DebugLabels;
        private static bool _HighPolygonsMode;
        private static bool _ColshapesVisible;
        private static bool _RaytraceEnabled;

        private static int _PhoneWallpaperNum;
        private static bool _PhoneNotDisturb;

        public static bool AutoTeleportMarker { get => _AutoTeleportMarker; set { if (value != _AutoTeleportMarker) Additional.Storage.SetData("Settings::Other::AutoTeleportMarker", value); _AutoTeleportMarker = value; } }
        public static bool DebugLabels { get => _DebugLabels; set { if (value != _DebugLabels) Additional.Storage.SetData("Settings::Other::DebugLabels", value); _DebugLabels = value; } }
        public static bool HighPolygonsMode { get => _HighPolygonsMode; set { if (value != _HighPolygonsMode) Additional.Storage.SetData("Settings::Other::HighPolygonsMode", value); _HighPolygonsMode = value; } }
        public static bool ColshapesVisible { get => _ColshapesVisible; set { if (value != _ColshapesVisible) Additional.Storage.SetData("Settings::Other::ColshapesVisible", value); _ColshapesVisible = value; } }
        public static bool RaytraceEnabled { get => _RaytraceEnabled; set { if (value != _RaytraceEnabled) Additional.Storage.SetData("Settings::Other::RaytraceEnabled", value); _RaytraceEnabled = value; } }

        public static int PhoneWallpaperNum { get => _PhoneWallpaperNum; set { if (value != _PhoneWallpaperNum) Additional.Storage.SetData("Settings::Phone::WallpaperNum", value); _PhoneWallpaperNum = value; } }
        public static bool PhoneNotDisturb { get => _PhoneNotDisturb; set { if (value != _PhoneNotDisturb) Additional.Storage.SetData("Settings::Phone::NotDisturb", value); _PhoneNotDisturb = value; } }

        public static HashSet<string> FavoriteAnimations { get => _FavoriteAnimations; set { Additional.Storage.SetData("Settings::Animations::Favorites", value); _FavoriteAnimations = value; } }
        public static Sync.Animations.EmotionTypes CurrentEmotion { get => _CurrentEmotion; set { if (value != _CurrentEmotion) Additional.Storage.SetData("Settings::Animations::Emotion", value.ToString()); _CurrentEmotion = value; } }
        public static Sync.Animations.WalkstyleTypes CurrentWalkstyle { get => _CurrentWalkstyle; set { if (value != _CurrentWalkstyle) Additional.Storage.SetData("Settings::Animations::Walkstyle", value.ToString()); _CurrentWalkstyle = value; } }

        public static List<CEF.BlipsMenu.LocalBlip> LocalBlips { get => _LocalBlips; set { Additional.Storage.SetData("Settings::LocalBlips", value); _LocalBlips = value; } }
    }
}
