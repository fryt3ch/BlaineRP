using System;
using System.Collections.Generic;
using BlaineRP.Client.Animations.Enums;
using BlaineRP.Client.Sync;

namespace BlaineRP.Client.Settings.User
{
    public static class Initialization
    {
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
            EmotionTypes emotion = EmotionTypes.None;

            if (!Enum.TryParse<EmotionTypes>(tStr, out emotion))
                Other.CurrentEmotion = Other.Default.CurrentEmotion;
            else
                Other.CurrentEmotion = emotion;

            tStr = Additional.Storage.GetData<string>("Settings::Animations::Walkstyle") ?? Other.Default.CurrentWalkstyle.ToString();
            WalkstyleTypes walkstyle = WalkstyleTypes.None;

            if (!Enum.TryParse<WalkstyleTypes>(tStr, out walkstyle))
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
