using RAGE;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace BCRPClient
{
    class Settings : Events.Script
    {
        public const string LANGUAGE = "ru";

        /// <summary>Базовый FPS для выравнивания скорости работы некоторых механик</summary>
        /// <remarks>Используется, например, в тире, для обеспечения одинаковой скорости перемещения мишеней у игроков с разным FPS</remarks>
        public const float BASE_FPS = 185f;

        public static float STREAM_DISTANCE = 0f;

        public static float ENTITY_INTERACTION_MAX_DISTANCE = 0f;
        public static float ENTITY_INTERACTION_MAX_DISTANCE_RENDER = 0f;

        public static float MIN_CRUISE_CONTROL_SPEED = 0f;
        public static float MAX_CRUISE_CONTROL_SPEED = 0f;

        public static float MAX_INVENTORY_WEIGHT = 0f;

        public static System.Globalization.CultureInfo CultureInfo { get; } = new System.Globalization.CultureInfo("en-US", false)
        {
            NumberFormat = new System.Globalization.NumberFormatInfo()
            {
                CurrencyDecimalSeparator = ".",
                NumberDecimalSeparator = ".",
            }
        };

        #region TO_REPLACE
        public const uint MAIN_DIMENSION = 7;
        public const uint STUFF_DIMENSION = 1;

        public const float FINGER_POINT_ENTITY_MAX_DISTANCE = 10f;

        public const int SPEEDOMETER_UPDATE_SPEED = 10;

        public const int MAX_PLAYER_HEALTH = 100 + 100;

        public const int DEFAULT_FADE_IN_OUT_SPEED = 500;

        public const int DISCORD_STATUS_UPDATE_TIME = 5_000;

        public const int RENTED_VEHICLE_TIME_TO_AUTODELETE = 300_000;

        public const int PHONE_SMS_MAX_LENGTH = 120;
        public const int PHONE_SMS_MIN_LENGTH = 5;

        public const byte SERVER_TIME_UTC_OFFSET = 3;

        public const byte TAXI_ORDER_MAX_WAIT_RANGE = 10;
        public const byte POLICE_CALL_MAX_WAIT_RANGE = 10;
        public const byte EMS_CALL_MAX_WAIT_RANGE = 10;

        public const double DAMAGE_SYSTEM_WOUND_CHANCE = 0.15d;
        #endregion

        public const bool DISABLE_IDLE_CAM = true;
        public const int DISABLE_IDLE_CAM_TIMEOUT = 25_000;

        public const int SCREEN_RESOLUTION_CHANGE_CHECK_TIMEOUT = 2_500;

        public static readonly Color HUD_COLOUR = Color.FromArgb(255, 255, 0, 0);

        public Settings()
        {

        }

        #region Classes

        public class Native
        {
            public static bool Audio_MuteAudioOnFocusLoss => RAGE.Game.Misc.GetProfileSetting(318) > 0;
            public static int Audio_MusicVolume => RAGE.Game.Misc.GetProfileSetting(306); // 0-10
            public static int Audio_SFXVolume => RAGE.Game.Misc.GetProfileSetting(300); // 0-10
        }

        #region Interface
        public class Interface
        {
            public class Default
            {
                public static bool UseServerTime = true;
                public static bool HideHints = false;
                public static bool HideNames = false;
                public static bool HideCID = false;
                public static bool HideHUD = false;
                public static bool HideQuest = false;

                public static bool HideInteractionBtn = false;
                public static bool HideIOGNames = false;
                public static bool AutoReload = true;
                public static bool FingerOn = true;
            }

            private static bool _UseServerTime;
            private static bool _HideHints;
            private static bool _HideNames;
            private static bool _HideCID;
            private static bool _HideHUD;
            private static bool _HideQuest;

            private static bool _HideInteractionBtn;
            private static bool _HideIOGNames;
            private static bool _AutoReload;
            private static bool _FingerOn;

            public static bool UseServerTime { get => _UseServerTime; set { if (value != _UseServerTime) Additional.Storage.SetData("Settings::Interface::UseServerTime", value); _UseServerTime = value; CEF.Menu.UpdateToggle("sett-time", value); CEF.HUD.UpdateTime(); } }
            public static bool HideHints { get => _HideHints; set { if (value != _HideHints) Additional.Storage.SetData("Settings::Interface::HideHints", value); _HideHints = value; CEF.Menu.UpdateToggle("sett-help", value); CEF.HUD.ToggleHints(!value); CEF.Inventory.SwitchHint(!value); } }
            public static bool HideNames { get => _HideNames; set { if (value != _HideNames) Additional.Storage.SetData("Settings::Interface::HideNames", value); _HideNames = value; CEF.Menu.UpdateToggle("sett-names", value); NameTags.Enabled = !value; } }
            public static bool HideCID { get => _HideCID; set { if (value != _HideCID) Additional.Storage.SetData("Settings::Interface::HideCID", value); _HideCID = value; CEF.Menu.UpdateToggle("sett-cid", value); } }
            public static bool HideHUD { get => _HideHUD; set { if (value != _HideHUD) Additional.Storage.SetData("Settings::Interface::HideHUD", value); _HideHUD = value; CEF.Menu.UpdateToggle("sett-hud", value); CEF.HUD.ShowHUD(!value); } }
            public static bool HideQuest { get => _HideQuest; set { if (value != _HideQuest) Additional.Storage.SetData("Settings::Interface::HideQuest", value); CEF.Menu.UpdateToggle("sett-quest", value); _HideQuest = value; CEF.HUD.EnableQuest(!value); } }

            public static bool HideInteractionBtn { get => _HideInteractionBtn; set { if (value != _HideInteractionBtn) Additional.Storage.SetData("Settings::Interface::HideInteractionBtn", value); _HideInteractionBtn = value; CEF.Menu.UpdateToggle("sett-interact", value); Interaction.EnabledVisual = !value; } }
            public static bool HideIOGNames { get => _HideIOGNames; set { if (value != _HideIOGNames) Additional.Storage.SetData("Settings::Interface::HideIOGNames", value); _HideIOGNames = value; CEF.Menu.UpdateToggle("sett-items", value); } }
            public static bool AutoReload { get => _AutoReload; set { if (value != _AutoReload) Additional.Storage.SetData("Settings::Interface::AutoReload", value); _AutoReload = value; CEF.Menu.UpdateToggle("sett-reload", value); } }
            public static bool FingerOn { get => _FingerOn; set { if (value != _FingerOn) Additional.Storage.SetData("Settings::Interface::FingerOn", value); _FingerOn = value; CEF.Menu.UpdateToggle("sett-finger", value); } }
        }
        #endregion

        #region Aim
        public class Aim
        {
            public enum Types
            {
                Default = 1, Dot, Cross
            }

            public class Default
            {
                public static Types Type = Types.Default;
                public static Utils.Colour Color = new Utils.Colour(255, 255, 255);
                public static float Alpha = 1f;
                public static float Scale = 0.5f;
            }

            public static Types _Type;
            public static Utils.Colour _Color;
            public static float _Alpha;
            public static float _Scale;

            public static Types Type { get => _Type; set { if (value != _Type) Additional.Storage.SetData("Settings::Interface::Aim::Type", value); _Type = value; } }
            public static Utils.Colour Color { get => _Color; set { if (value != _Color) Additional.Storage.SetData("Settings::Interface::Aim::Color", value); _Color = value; } }
            public static float Alpha { get => _Alpha; set { if (value != _Alpha) Additional.Storage.SetData("Settings::Interface::Aim::Alpha", value); _Alpha = value; } }
            public static float Scale { get => _Scale; set { if (value != _Scale) Additional.Storage.SetData("Settings::Interface::Aim::Scale", value); _Scale = value; } }
        }
        #endregion

        #region Chat
        public class Chat
        {
            public class Default
            {
                public static bool UseFilter = true;
                public static bool ShowTime = false;
                public static int Height = 276;
                public static int FontSize = 14;
            }

            private static bool _UseFilter;
            private static bool _ShowTime;
            private static int _Height;
            private static int _FontSize;

            public static bool UseFilter { get => _UseFilter; set { if (value != _UseFilter) Additional.Storage.SetData("Settings::Chat::UseFilter", value); _UseFilter = value; CEF.Menu.UpdateToggle("sett-filter", value); } }
            public static bool ShowTime { get => _ShowTime; set { if (value != _ShowTime) Additional.Storage.SetData("Settings::Chat::ShowTime", value); _ShowTime = value; CEF.Menu.UpdateToggle("sett-timestamp", value); } }
            public static int Height { get => _Height; set { if (value != _Height) Additional.Storage.SetData("Settings::Chat::Height", value); _Height = value; CEF.Chat.SetHeight(value); } }
            public static int FontSize { get => _FontSize; set { if (value != _FontSize) Additional.Storage.SetData("Settings::Chat::FontSize", value); _FontSize = value; CEF.Chat.SetFontSize(value); } }
        }
        #endregion

        #region Audio
        public class Audio
        {
            public class Default
            {
                public static int VoiceVolume = 100;
                public static int SoundVolume = 50;
                public static float PlayerLocalRadioVolume = 0.5f;
            }

            private static int _VoiceVolume;
            private static int _SoundVolume;
            private static float _PlayerLocalRadioVolume;

            public static int VoiceVolume { get => _VoiceVolume; set { if (value < 0) value = 0; else if (value > 100) value = 100; if (value != _VoiceVolume) Additional.Storage.SetData("Settings::Audio::VoiceVolume", value); _VoiceVolume = value; } }
            public static int SoundVolume { get => _SoundVolume; set { if (value < 0) value = 0; else if (value > 100) value = 100; if (value != _SoundVolume) Additional.Storage.SetData("Settings::Audio::SoundVolume", value); _SoundVolume = value; } }
            public static float PlayerLocalRadioVolume { get => _PlayerLocalRadioVolume; set { if (value < 0f) value = 0f; else if (value > 1f) value = 1f; if (value != _PlayerLocalRadioVolume) Additional.Storage.SetData("Settings::Audio::PLRVolume", value); _PlayerLocalRadioVolume = value; } }
        }
        #endregion

        #region Special
        public class Special
        {
            public class Default
            {
                public static bool DisabledPerson = false;
            }

            private static bool _DisabledPerson;

            public static bool DisabledPerson { get => _DisabledPerson; set { if (value != _DisabledPerson) Additional.Storage.SetData("Settings::Special::DisabledPerson", value); _DisabledPerson = value; CEF.Menu.UpdateToggle("sett-special", value); } }
        }
        #endregion

        #region Other Settings
        public class Other
        {
            public class Default
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
        #endregion
        #endregion

        #region Loaders
        public static void Load()
        {
            Interface.UseServerTime = Additional.Storage.GetData<bool?>("Settings::Interface::UseServerTime") ?? Interface.Default.UseServerTime;
            Interface.HideHints = Additional.Storage.GetData<bool?>("Settings::Interface::HideHints") ?? Interface.Default.HideHints;
            Interface.HideNames = Additional.Storage.GetData<bool?>("Settings::Interface::HideNames") ?? Interface.Default.HideNames;
            Interface.HideCID = Additional.Storage.GetData<bool?>("Settings::Interface::HideCID") ?? Interface.Default.HideCID;
            Interface.HideHUD = Additional.Storage.GetData<bool?>("Settings::Interface::HideHUD") ?? Interface.Default.HideHUD;
            Interface.HideQuest = Additional.Storage.GetData<bool?>("Settings::Interface::HideQuest") ?? Interface.Default.HideQuest;

            Interface.HideInteractionBtn = Additional.Storage.GetData<bool?>("Settings::Interface::HideInteractionBtn") ?? Interface.Default.HideInteractionBtn;
            Interface.HideIOGNames = Additional.Storage.GetData<bool?>("Settings::Interface::HideIOGNames") ?? Interface.Default.HideIOGNames;
            Interface.AutoReload = Additional.Storage.GetData<bool?>("Settings::Interface::AutoReload") ?? Interface.Default.AutoReload;
            Interface.FingerOn = Additional.Storage.GetData<bool?>("Settings::Interface::FingerOn") ?? Interface.Default.FingerOn;

            Aim.Type = Additional.Storage.GetData<Aim.Types?>("Settings::Interface::Aim::Type") ?? Aim.Default.Type;
            Aim.Color = Additional.Storage.GetData<Utils.Colour>("Settings::Interface::Aim::Color") ?? Aim.Default.Color;
            Aim.Alpha = Additional.Storage.GetData<float?>("Settings::Interface::Aim::Alpha") ?? Aim.Default.Alpha;
            Aim.Scale = Additional.Storage.GetData<float?>("Settings::Interface::Aim::Scale") ?? Aim.Default.Scale;

            Chat.UseFilter = Additional.Storage.GetData<bool?>("Settings::Chat::UseFilter") ?? Chat.Default.UseFilter;
            Chat.ShowTime = Additional.Storage.GetData<bool?>("Settings::Chat::ShowTime") ?? Chat.Default.ShowTime;
            Chat.Height = Additional.Storage.GetData<int?>("Settings::Chat::Height") ?? Chat.Default.Height;
            Chat.FontSize = Additional.Storage.GetData<int?>("Settings::Chat::FontSize") ?? Chat.Default.FontSize;

            Audio.VoiceVolume = Additional.Storage.GetData<int?>("Settings::Audio::VoiceVolume") ?? Audio.Default.VoiceVolume;
            Audio.SoundVolume = Additional.Storage.GetData<int?>("Settings::Audio::SoundVolume") ?? Audio.Default.SoundVolume;
            Audio.PlayerLocalRadioVolume = Additional.Storage.GetData<float?>("Settings::Audio::PLRVolume") ?? Audio.Default.PlayerLocalRadioVolume;

            Special.DisabledPerson = Additional.Storage.GetData<bool?>("Settings::Special::DisabledPerson") ?? Special.Default.DisabledPerson;

            Other.AutoTeleportMarker = Additional.Storage.GetData<bool?>("Settings::Other::AutoTeleportMarker") ?? Other.Default.AutoTeleportMarker;
            Other.DebugLabels = Additional.Storage.GetData<bool?>("Settings::Other::DebugLabels") ?? Other.Default.DebugLabels;
            Other.HighPolygonsMode = Additional.Storage.GetData<bool?>("Settings::Other::HighPolygonsMode") ?? Other.Default.HighPolygonsMode;
            Other.ColshapesVisible = Additional.Storage.GetData<bool?>("Settings::Other::ColshapesVisible") ?? Other.Default.ColshapesVisible;
            Other.RaytraceEnabled = Additional.Storage.GetData<bool?>("Settings::Other::RaytraceEnabled") ?? Other.Default.RaytraceEnabled;

            Other.PhoneWallpaperNum = Additional.Storage.GetData<int?>("Settings::Phone::WallpaperNum") ?? Other.Default.PhoneWallpaperNum;
            Other.PhoneNotDisturb = Additional.Storage.GetData<bool?>("Settings::Phone::NotDisturb") ?? Other.Default.PhoneNotDisturb;

            Other.FavoriteAnimations = Additional.Storage.GetData<HashSet<string>>("Settings::Animations::Favorites") ?? Other.Default.FavoriteAnimations;

            var tStr = Additional.Storage.GetData<string>("Settings::Animations::Emotion") ?? Other.Default.CurrentEmotion.ToString();
            Sync.Animations.EmotionTypes emotion = Sync.Animations.EmotionTypes.None;

            if (!Enum.TryParse<Sync.Animations.EmotionTypes>(tStr, out emotion))
                Other.CurrentEmotion = Other.Default.CurrentEmotion;
            else
                Other.CurrentEmotion = emotion;

            tStr = Additional.Storage.GetData<string>("Settings::Animations::Walkstyle") ?? Other.Default.CurrentWalkstyle.ToString();
            Sync.Animations.WalkstyleTypes walkstyle = Sync.Animations.WalkstyleTypes.None;

            if (!Enum.TryParse<Sync.Animations.WalkstyleTypes>(tStr, out walkstyle))
                Other.CurrentWalkstyle = Other.Default.CurrentWalkstyle;
            else
                Other.CurrentWalkstyle = walkstyle;

            Other.LocalBlips = Additional.Storage.GetData<List<CEF.BlipsMenu.LocalBlip>>("Settings::LocalBlips") ?? Other.Default.LocalBlips;

            foreach (var x in Other.LocalBlips)
            {
                if (x.Enabled)
                    x.Toggle(true);
            }
        }
        #endregion

        #region Defaulters
        public static void DefaultAll()
        {
            Interface.UseServerTime = Interface.Default.UseServerTime;
            Interface.HideHints = Interface.Default.HideHints;
            Interface.HideNames = Interface.Default.HideNames;
            Interface.HideCID = Interface.Default.HideCID;
            Interface.HideHUD = Interface.Default.HideHUD;
            Interface.HideQuest = Interface.Default.HideQuest;

            Interface.HideInteractionBtn = Interface.Default.HideInteractionBtn;
            Interface.HideIOGNames = Interface.Default.HideIOGNames;
            Interface.AutoReload = Interface.Default.AutoReload;
            Interface.FingerOn = Interface.Default.FingerOn;

            Aim.Type = Aim.Default.Type;
            Aim.Color = Aim.Default.Color;
            Aim.Alpha = Aim.Default.Alpha;
            Aim.Scale = Aim.Default.Scale;

            Chat.UseFilter = Chat.Default.UseFilter;
            Chat.ShowTime = Chat.Default.ShowTime;
            Chat.Height = Chat.Default.Height;
            Chat.FontSize = Chat.Default.FontSize;

            Audio.VoiceVolume = Audio.Default.VoiceVolume;
            Audio.SoundVolume = Audio.Default.SoundVolume;

            Special.DisabledPerson = Special.Default.DisabledPerson;

            /*            Other.AutoTeleportMarker = Other.Default.AutoTeleportMarker;
                        Other.DebugLabels = Other.Default.DebugLabels;
                        Other.HighPolygonsMode = Other.Default.HighPolygonsMode;
                        Other.ColshapesVisible = Other.Default.ColshapesVisible;*/
        }
        #endregion
    }
}
