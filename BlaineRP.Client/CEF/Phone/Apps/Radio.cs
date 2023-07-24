﻿using BlaineRP.Client.CEF.Phone.Enums;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Utils.Game;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlaineRP.Client.CEF.Phone.Apps
{
    [Script(int.MaxValue)]
    public class Radio
    {
        public Radio()
        {
            Events.Add("Phone::Radio", (args) =>
            {
                var type = (string)args[0];

                if (type == "play")
                {
                    var state = !(bool)args[1];

                    if (state)
                    {
                        var sType = Sync.Radio.CurrentStationType == Sync.Radio.StationTypes.Off ? Sync.Radio.StationTypes.NSPFM : Sync.Radio.CurrentStationType;

                        Sync.Radio.ToggleMobilePhoneRadio(true);

                        Sync.Radio.SetCurrentRadioStationType(sType);

                        UpdateRadioStation(sType);

                        UpdateRadioStationState(true);
                    }
                    else
                    {
                        Sync.Radio.ToggleMobilePhoneRadio(false);

                        UpdateRadioStationState(false);
                    }
                }
                else if (type == "rewind")
                {
                    var allStations = Enum.GetValues(typeof(Sync.Radio.StationTypes)).Cast<Sync.Radio.StationTypes>().ToList();

                    var deltaIdx = (int)args[2] < 0 ? -1 : 1;

                    var newStationIdx = allStations.IndexOf(Sync.Radio.CurrentStationType == Sync.Radio.StationTypes.Off ? Sync.Radio.StationTypes.NSPFM : Sync.Radio.CurrentStationType) + deltaIdx;

                    if (newStationIdx <= 0)
                        newStationIdx = allStations.Count - 1;
                    else if (newStationIdx >= allStations.Count)
                        newStationIdx = 1;

                    var sType = allStations[newStationIdx];

                    var pos = Player.LocalPlayer.Position;

                    if (sType == Sync.Radio.StationTypes.WCTR)
                    {
                        if (Misc.IsCoordInCountrysideV(pos.X, pos.Y, pos.Z))
                            sType = allStations[newStationIdx + deltaIdx];
                    }
                    else if (sType == Sync.Radio.StationTypes.BCTR)
                    {
                        if (Misc.IsCoordInCountrysideV(pos.X, pos.Y, pos.Z))
                            sType = allStations[newStationIdx + deltaIdx];
                    }

                    UpdateRadioStation(sType);
                }
                else if (type == "volume")
                {
                    var volume = Utils.Convert.ToSingle(args[2]);

                    var localStreamData = Sync.Radio.LocalPlayerStreamRadioAudioData;

                    if (localStreamData == null)
                    {
                        //CEF.Browser.Window.ExecuteJs("Phone.setSliderVal", "phone-value", Settings.User.Audio.PlayerLocalRadioVolume);
                    }
                    else
                    {
                        BlaineRP.Client.Settings.User.Audio.PlayerLocalRadioVolume = volume;

                        localStreamData.SetVolume(volume);
                    }
                }
            });
        }

        public static Action<int> CurrentTransactionAction { get; set; }

        public static void Show()
        {
            if (CEF.Phone.Phone.CurrentApp == AppTypes.None)
                CEF.Phone.Phone.SwitchMenu(false);

            CEF.Phone.Phone.CurrentApp = AppTypes.Radio;

            CEF.Phone.Phone.CurrentAppTab = -1;

            var sType = Sync.Radio.CurrentStationType == Sync.Radio.StationTypes.Off ? Sync.Radio.StationTypes.NSPFM : Sync.Radio.CurrentStationType;

            string trackName1, trackName2;

            Sync.Radio.GetCurrentRadioTrackDetails(out trackName1, out trackName2);

            Browser.Window.ExecuteJs("Phone.drawRadioApp", new List<object> { (int)sType, Sync.Radio.GetRadioStationName(sType), trackName1, trackName2, Sync.Radio.IsMobilePhoneRadioEnabled, BlaineRP.Client.Settings.User.Audio.PlayerLocalRadioVolume });
        }

        public static void UpdateRadioStation(Sync.Radio.StationTypes sType)
        {
            if (CEF.Phone.Phone.CurrentApp != AppTypes.Radio)
                return;

            var mRadioWasEnabled = Sync.Radio.IsMobilePhoneRadioEnabled;

            if (!mRadioWasEnabled)
                Sync.Radio.ToggleMobilePhoneRadio(true);

            Sync.Radio.SetCurrentRadioStationType(sType);

            if (!mRadioWasEnabled)
                Sync.Radio.ToggleMobilePhoneRadio(false);

            string trackName1, trackName2;

            Sync.Radio.GetCurrentRadioTrackDetails(out trackName1, out trackName2);

            Browser.Window.ExecuteJs("Phone.updateRadioStation", (int)sType, Sync.Radio.GetRadioStationName(sType), trackName1, trackName2);
        }

        public static void UpdateRadioStationState(bool state)
        {
            if (CEF.Phone.Phone.CurrentApp != AppTypes.Radio)
                return;

            Browser.Window.ExecuteJs("Phone.updateRadioPlay", state);
        }
    }
}