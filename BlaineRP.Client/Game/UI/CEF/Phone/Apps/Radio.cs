﻿using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Game.Management.Radio;
using BlaineRP.Client.Game.Management.Radio.Enums;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.UI.CEF.Phone.Apps
{
    [Script(int.MaxValue)]
    public class Radio
    {
        public Radio()
        {
            Events.Add("Phone::Radio",
                (args) =>
                {
                    var type = (string)args[0];

                    if (type == "play")
                    {
                        bool state = !(bool)args[1];

                        if (state)
                        {
                            RadioStationTypes sType = Core.CurrentStationType == RadioStationTypes.Off ? RadioStationTypes.NSPFM : Core.CurrentStationType;

                            Core.ToggleMobilePhoneRadio(true);

                            Core.SetCurrentRadioStationType(sType);

                            UpdateRadioStation(sType);

                            UpdateRadioStationState(true);
                        }
                        else
                        {
                            Core.ToggleMobilePhoneRadio(false);

                            UpdateRadioStationState(false);
                        }
                    }
                    else if (type == "rewind")
                    {
                        var allStations = Enum.GetValues(typeof(RadioStationTypes)).Cast<RadioStationTypes>().ToList();

                        int deltaIdx = (int)args[2] < 0 ? -1 : 1;

                        int newStationIdx = allStations.IndexOf(Core.CurrentStationType == RadioStationTypes.Off ? RadioStationTypes.NSPFM : Core.CurrentStationType) + deltaIdx;

                        if (newStationIdx <= 0)
                            newStationIdx = allStations.Count - 1;
                        else if (newStationIdx >= allStations.Count)
                            newStationIdx = 1;

                        RadioStationTypes sType = allStations[newStationIdx];

                        Vector3 pos = Player.LocalPlayer.Position;

                        if (sType == RadioStationTypes.WCTR)
                        {
                            if (Utils.Game.Misc.IsCoordInCountrysideV(pos.X, pos.Y, pos.Z))
                                sType = allStations[newStationIdx + deltaIdx];
                        }
                        else if (sType == RadioStationTypes.BCTR)
                        {
                            if (Utils.Game.Misc.IsCoordInCountrysideV(pos.X, pos.Y, pos.Z))
                                sType = allStations[newStationIdx + deltaIdx];
                        }

                        UpdateRadioStation(sType);
                    }
                    else if (type == "volume")
                    {
                        var volume = Utils.Convert.ToSingle(args[2]);

                        Audio.Data localStreamData = Core.LocalPlayerStreamRadioAudioData;

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
                }
            );
        }

        public static Action<int> CurrentTransactionAction { get; set; }

        public static void Show()
        {
            if (CEF.Phone.Phone.CurrentApp == AppType.None)
                CEF.Phone.Phone.SwitchMenu(false);

            CEF.Phone.Phone.CurrentApp = AppType.Radio;

            CEF.Phone.Phone.CurrentAppTab = -1;

            RadioStationTypes sType = Core.CurrentStationType == RadioStationTypes.Off ? RadioStationTypes.NSPFM : Core.CurrentStationType;

            string trackName1, trackName2;

            Core.GetCurrentRadioTrackDetails(out trackName1, out trackName2);

            Browser.Window.ExecuteJs("Phone.drawRadioApp",
                new List<object>
                {
                    (int)sType,
                    Core.GetRadioStationName(sType),
                    trackName1,
                    trackName2,
                    Core.IsMobilePhoneRadioEnabled,
                    BlaineRP.Client.Settings.User.Audio.PlayerLocalRadioVolume,
                }
            );
        }

        public static void UpdateRadioStation(RadioStationTypes sType)
        {
            if (CEF.Phone.Phone.CurrentApp != AppType.Radio)
                return;

            bool mRadioWasEnabled = Core.IsMobilePhoneRadioEnabled;

            if (!mRadioWasEnabled)
                Core.ToggleMobilePhoneRadio(true);

            Core.SetCurrentRadioStationType(sType);

            if (!mRadioWasEnabled)
                Core.ToggleMobilePhoneRadio(false);

            string trackName1, trackName2;

            Core.GetCurrentRadioTrackDetails(out trackName1, out trackName2);

            Browser.Window.ExecuteJs("Phone.updateRadioStation", (int)sType, Core.GetRadioStationName(sType), trackName1, trackName2);
        }

        public static void UpdateRadioStationState(bool state)
        {
            if (CEF.Phone.Phone.CurrentApp != AppType.Radio)
                return;

            Browser.Window.ExecuteJs("Phone.updateRadioPlay", state);
        }
    }
}