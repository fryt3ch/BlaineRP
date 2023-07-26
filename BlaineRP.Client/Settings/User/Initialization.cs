using System;
using System.Collections.Generic;
using BlaineRP.Client.Game.Animations;
using BlaineRP.Client.Game.Management;
using BlaineRP.Client.Game.UI.CEF;

namespace BlaineRP.Client.Settings.User
{
    public static class Initialization
    {
        public static void Load()
        {
            Interface.UseServerTime = RageStorage.GetData<bool?>("Settings::Interface::UseServerTime") ?? Interface.Default.UseServerTime;
            Interface.HideHints = RageStorage.GetData<bool?>("Settings::Interface::HideHints") ?? Interface.Default.HideHints;
            Interface.HideNames = RageStorage.GetData<bool?>("Settings::Interface::HideNames") ?? Interface.Default.HideNames;
            Interface.HideCID = RageStorage.GetData<bool?>("Settings::Interface::HideCID") ?? Interface.Default.HideCID;
            Interface.HideHUD = RageStorage.GetData<bool?>("Settings::Interface::HideHUD") ?? Interface.Default.HideHUD;
            Interface.HideQuest = RageStorage.GetData<bool?>("Settings::Interface::HideQuest") ?? Interface.Default.HideQuest;

            Interface.HideInteractionBtn = RageStorage.GetData<bool?>("Settings::Interface::HideInteractionBtn") ?? Interface.Default.HideInteractionBtn;
            Interface.HideIOGNames = RageStorage.GetData<bool?>("Settings::Interface::HideIOGNames") ?? Interface.Default.HideIOGNames;
            Interface.AutoReload = RageStorage.GetData<bool?>("Settings::Interface::AutoReload") ?? Interface.Default.AutoReload;
            Interface.FingerOn = RageStorage.GetData<bool?>("Settings::Interface::FingerOn") ?? Interface.Default.FingerOn;

            Aim.Type = RageStorage.GetData<Aim.Types?>("Settings::Interface::Aim::Type") ?? Aim.Default.Type;
            Aim.Color = RageStorage.GetData<Utils.Colour>("Settings::Interface::Aim::Color") ?? Aim.Default.Color;
            Aim.Alpha = RageStorage.GetData<float?>("Settings::Interface::Aim::Alpha") ?? Aim.Default.Alpha;
            Aim.Scale = RageStorage.GetData<float?>("Settings::Interface::Aim::Scale") ?? Aim.Default.Scale;

            Chat.UseFilter = RageStorage.GetData<bool?>("Settings::Chat::UseFilter") ?? Chat.Default.UseFilter;
            Chat.ShowTime = RageStorage.GetData<bool?>("Settings::Chat::ShowTime") ?? Chat.Default.ShowTime;
            Chat.Height = RageStorage.GetData<int?>("Settings::Chat::Height") ?? Chat.Default.Height;
            Chat.FontSize = RageStorage.GetData<int?>("Settings::Chat::FontSize") ?? Chat.Default.FontSize;

            Audio.VoiceVolume = RageStorage.GetData<int?>("Settings::Audio::VoiceVolume") ?? Audio.Default.VoiceVolume;
            Audio.SoundVolume = RageStorage.GetData<int?>("Settings::Audio::SoundVolume") ?? Audio.Default.SoundVolume;
            Audio.PlayerLocalRadioVolume = RageStorage.GetData<float?>("Settings::Audio::PLRVolume") ?? Audio.Default.PlayerLocalRadioVolume;

            Special.DisabledPerson = RageStorage.GetData<bool?>("Settings::Special::DisabledPerson") ?? Special.Default.DisabledPerson;

            Other.AutoTeleportMarker = RageStorage.GetData<bool?>("Settings::Other::AutoTeleportMarker") ?? Other.Default.AutoTeleportMarker;
            Other.DebugLabels = RageStorage.GetData<bool?>("Settings::Other::DebugLabels") ?? Other.Default.DebugLabels;
            Other.HighPolygonsMode = RageStorage.GetData<bool?>("Settings::Other::HighPolygonsMode") ?? Other.Default.HighPolygonsMode;
            Other.ColshapesVisible = RageStorage.GetData<bool?>("Settings::Other::ColshapesVisible") ?? Other.Default.ColshapesVisible;
            Other.RaytraceEnabled = RageStorage.GetData<bool?>("Settings::Other::RaytraceEnabled") ?? Other.Default.RaytraceEnabled;

            Other.PhoneWallpaperNum = RageStorage.GetData<int?>("Settings::Phone::WallpaperNum") ?? Other.Default.PhoneWallpaperNum;
            Other.PhoneNotDisturb = RageStorage.GetData<bool?>("Settings::Phone::NotDisturb") ?? Other.Default.PhoneNotDisturb;

            Other.FavoriteAnimations = RageStorage.GetData<HashSet<string>>("Settings::Animations::Favorites") ?? Other.Default.FavoriteAnimations;

            var tStr = RageStorage.GetData<string>("Settings::Animations::Emotion") ?? Other.Default.CurrentEmotion.ToString();
            EmotionTypes emotion = EmotionTypes.None;

            if (!Enum.TryParse<EmotionTypes>(tStr, out emotion))
                Other.CurrentEmotion = Other.Default.CurrentEmotion;
            else
                Other.CurrentEmotion = emotion;

            tStr = RageStorage.GetData<string>("Settings::Animations::Walkstyle") ?? Other.Default.CurrentWalkstyle.ToString();
            WalkstyleTypes walkstyle = WalkstyleTypes.None;

            if (!Enum.TryParse<WalkstyleTypes>(tStr, out walkstyle))
                Other.CurrentWalkstyle = Other.Default.CurrentWalkstyle;
            else
                Other.CurrentWalkstyle = walkstyle;

            Other.LocalBlips = RageStorage.GetData<List<BlipsMenu.LocalBlip>>("Settings::LocalBlips") ?? Other.Default.LocalBlips;

            foreach (var x in Other.LocalBlips)
            {
                if (x.Enabled)
                    x.Toggle(true);
            }
        }

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
    }
}
