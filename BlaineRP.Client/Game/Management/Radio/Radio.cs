using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.Management.Radio.Enums;
using BlaineRP.Client.Game.Scripts.Sync;
using BlaineRP.Client.Game.UI.CEF;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Management.Radio
{
    [Script(int.MaxValue)]
    public class Core
    {
        public Core()
        {
            SetRadioStationName(RadioStationTypes.MP_BRP, "Radio Blaine RP");

            SetRadioStationName(RadioStationTypes.BRP_0, "Europa Plus");
            SetRadioStationName(RadioStationTypes.BRP_1, "Record Radio");
            SetRadioStationName(RadioStationTypes.BRP_2, "Retro FM");
            SetRadioStationName(RadioStationTypes.BRP_3, "Classic UK");
            SetRadioStationName(RadioStationTypes.BRP_4, "Synthwave FM");

            SetCustomRadioStationArtistName(null);
            SetCustomRadioStationTrackName(null);

            SetRadioStationLocked(RadioStationTypes.SSLS, false);

            RAGE.Game.Invoker.Invoke(0x477D9DB48F889591, "RADIO_19_USER", true);

            ToggleMobilePhoneRadio(false);

            var updateTask = new Utils.AsyncTask(() =>
                {
                    RadioStationTypes sType = GetCurrentStationType();

                    bool stationChanged = CurrentStationType != sType;

                    CurrentStationType = sType;

                    if (Player.LocalPlayer.Vehicle is Vehicle veh)
                    {
                        var vData = VehicleData.GetData(veh);

                        if (vData != null)
                        {
                            RadioStationTypes actualStation = vData.Radio;

                            if (veh.GetPedInSeat(-1, 0) == Player.LocalPlayer.Handle || veh.GetPedInSeat(0, 0) == Player.LocalPlayer.Handle)
                            {
                                if (sType != actualStation && !Vehicles.LastRadioSent.IsSpam(1000, false, false))
                                {
                                    Vehicles.LastRadioSent = World.Core.ServerTime;

                                    RAGE.Events.CallRemote("Vehicles::SetRadio", (byte)CurrentStationType);
                                }
                            }
                            else
                            {
                                if (CurrentStationType != actualStation)
                                    SetCurrentRadioStationType(actualStation);
                            }
                        }
                    }
                    else
                    {
                    }

                    UI.CEF.Phone.Apps.Radio.UpdateRadioStation(CurrentStationType == RadioStationTypes.Off ? RadioStationTypes.NSPFM : CurrentStationType);
                },
                1_000,
                true,
                0
            );

            updateTask.Run();
        }

        private static Dictionary<RadioStationTypes, string> StationIds { get; set; } = new Dictionary<RadioStationTypes, string>()
        {
            { RadioStationTypes.Off, "OFF" },
            { RadioStationTypes.LSRR, "RADIO_01_CLASS_ROCK" },
            { RadioStationTypes.NSPFM, "RADIO_02_POP" },
            { RadioStationTypes.RLS, "RADIO_03_HIPHOP_NEW" },
            { RadioStationTypes.CHX, "RADIO_04_PUNK" },
            { RadioStationTypes.WCTR, "RADIO_05_TALK_01" },
            { RadioStationTypes.RR, "RADIO_06_COUNTRY" },
            { RadioStationTypes.SWFM, "RADIO_07_DANCE_01" },
            { RadioStationTypes.ELFM, "RADIO_08_MEXICAN" },
            { RadioStationTypes.WCC, "RADIO_09_HIPHOP_OLD" },
            { RadioStationTypes.BCTR, "RADIO_11_TALK_02" },
            { RadioStationTypes.BA, "RADIO_12_REGGAE" },
            { RadioStationTypes.WWFM, "RADIO_13_JAZZ" },
            { RadioStationTypes.FLFM, "RADIO_14_DANCE_02" },
            { RadioStationTypes.TLD, "RADIO_15_MOTOWN" },
            { RadioStationTypes.RMP, "RADIO_16_SILVERLAKE" },
            { RadioStationTypes.SPA, "RADIO_17_FUNK" },
            { RadioStationTypes.VWBR, "RADIO_18_90S_ROCK" },
            //{ StationTypes.SR, "RADIO_19_USER" }, 18
            { RadioStationTypes.TL, "RADIO_20_THELAB" },
            { RadioStationTypes.BLS, "RADIO_21_DLC_XM17" },
            { RadioStationTypes.LSUR, "RADIO_22_DLC_BATTLE_MIX1_RADIO" },
            { RadioStationTypes.IFR, "RADIO_23_DLC_XM19_RADIO" },
            { RadioStationTypes.SSLS, "RADIO_27_DLC_PRHEI4" },
            { RadioStationTypes.KFM, "RADIO_34_DLC_HEI4_KULT" },
            { RadioStationTypes.TML, "RADIO_35_DLC_HEI4_MLR" },
            { RadioStationTypes.MP_BRP, "RADIO_36_AUDIOPLAYER" },
            { RadioStationTypes.MMLS, "RADIO_37_MOTOMAMI" },
            { RadioStationTypes.BRP_0, "RADIO_999_BRPCR0_RADIO" }, // 46
            { RadioStationTypes.BRP_1, "RADIO_998_BRPCR0_RADIO" }, // 47
            { RadioStationTypes.BRP_2, "RADIO_997_BRPCR0_RADIO" }, // 48
            { RadioStationTypes.BRP_3, "RADIO_996_BRPCR0_RADIO" }, // 49
            { RadioStationTypes.BRP_4, "RADIO_995_BRPCR0_RADIO" }, // 50
        };

        private static Dictionary<RadioStationTypes, Audio.TrackTypes> CustomStationTracks { get; set; } = new Dictionary<RadioStationTypes, Audio.TrackTypes>()
        {
            { RadioStationTypes.MP_BRP, Audio.TrackTypes.None },
            { RadioStationTypes.BRP_0, Audio.TrackTypes.RadioEuropaPlus },
            { RadioStationTypes.BRP_1, Audio.TrackTypes.RadioRecord },
            { RadioStationTypes.BRP_2, Audio.TrackTypes.RadioRetroFM },
            { RadioStationTypes.BRP_3, Audio.TrackTypes.RadioClassicFM },
            { RadioStationTypes.BRP_4, Audio.TrackTypes.RadioSynthwave },
        };

        public static RadioStationTypes CurrentStationType { get; private set; }

        public static bool IsMobilePhoneRadioEnabled { get; private set; }

        public static Audio.Data LocalPlayerStreamRadioAudioData => Audio.AllAudios.Where(x => x.Id == "PLAYER_LOCAL_RADIO").FirstOrDefault();

        public static RadioStationTypes GetCurrentStationType()
        {
            return StationIds.Where(x => x.Value == RAGE.Game.Audio.GetPlayerRadioStationName()).FirstOrDefault().Key;
        }

        public static string GetRadioIdByStationType(RadioStationTypes sType)
        {
            return StationIds.GetValueOrDefault(sType) ?? string.Empty;
        }

        public static void SetCurrentRadioStationType(RadioStationTypes sType)
        {
            string radioId = GetRadioIdByStationType(sType);

            RAGE.Game.Audio.SetRadioToStationName(radioId);

            Vehicle curVehicle = Player.LocalPlayer.Vehicle;

            if (curVehicle != null)
                SetVehicleRadioStation(curVehicle, sType);

            if (curVehicle == null || IsMobilePhoneRadioEnabled)
            {
                Audio.Data audioData = LocalPlayerStreamRadioAudioData;

                if (IsRadioStationCustom(sType) && IsMobilePhoneRadioEnabled)
                {
                    Audio.TrackTypes trackType = CustomStationTracks.GetValueOrDefault(sType);

                    if (audioData == null)
                        audioData = new Audio.Data("PLAYER_LOCAL_RADIO", 1f);

                    if (audioData.CurrentTrackType != trackType)
                    {
                        audioData.Stop();

                        audioData.Play(trackType, Settings.User.Audio.PlayerLocalRadioVolume, false, 0);
                    }
                }
                else
                {
                    audioData?.Destroy();
                }
            }
        }

        public static void SetRadioStationName(RadioStationTypes sType, string name)
        {
            string strName = StationIds.GetValueOrDefault(sType);

            if (strName == null || strName == "OFF")
                return;

            RAGE.Game.Gxt.Add(strName.ToLower(), name);
        }

        public static string GetRadioStationName(RadioStationTypes sType)
        {
            string strName = StationIds.GetValueOrDefault(sType);

            if (strName == null || strName == "OFF")
                return string.Empty;

            return RAGE.Game.Gxt.Get(strName.ToLower());
        }

        public static void SetCustomRadioStationArtistName(string name)
        {
            RAGE.Game.Gxt.Add(0x43505A45, name?.ToUpper() ?? "");
        }

        public static void SetCustomRadioStationTrackName(string name)
        {
            RAGE.Game.Gxt.Add(0x43FBDB98, name ?? "");
        }

        public static void SetRadioStationLocked(RadioStationTypes sType, bool state)
        {
            RAGE.Game.Invoker.Invoke(0x477D9DB48F889591, GetRadioIdByStationType(sType), state);
        }

        public static void GetCurrentRadioTrackDetails(out string artistName, out string songName)
        {
            if (CurrentStationType == RadioStationTypes.MP_BRP)
            {
                artistName = string.Empty;
                songName = string.Empty;
            }
            else
            {
                RAGE.Game.Ui.ShowHudComponentThisFrame(16);

                var details = RAGE.Game.Audio.GetAudibleMusicTrackTextId().ToString();

                artistName = RAGE.Game.Ui.GetLabelText(details + "A");
                songName = RAGE.Game.Ui.GetLabelText(details + "S");

                if (artistName == "NULL")
                    artistName = string.Empty;

                if (songName == "NULL")
                    songName = string.Empty;
            }
        }

        public static void ToggleMobilePhoneRadio(bool state)
        {
            IsMobilePhoneRadioEnabled = state;

            RAGE.Game.Audio.SetMobilePhoneRadioState(state);

            RAGE.Game.Audio.SetAudioFlag("MobileRadioInGame", state);
            RAGE.Game.Audio.SetAudioFlag("AllowRadioDuringSwitch", state);

            Main.Render -= MobilePhoneRadioRender;

            if (state)
                Main.Render += MobilePhoneRadioRender;
            else
                LocalPlayerStreamRadioAudioData?.Destroy();
        }

        private static void MobilePhoneRadioRender()
        {
            if (Player.LocalPlayer.Vehicle is Vehicle veh && veh.GetIsEngineRunning())
            {
                UI.CEF.Phone.Apps.Radio.UpdateRadioStationState(false);

                ToggleMobilePhoneRadio(false);

                return;
            }

            RAGE.Game.Pad.DisableControlAction(32, 81, true);
            RAGE.Game.Pad.DisableControlAction(32, 82, true);
            RAGE.Game.Pad.DisableControlAction(32, 83, true);
            RAGE.Game.Pad.DisableControlAction(32, 84, true);
            RAGE.Game.Pad.DisableControlAction(32, 85, true);

            RAGE.Game.Pad.DisableControlAction(32, 332, true);
            RAGE.Game.Pad.DisableControlAction(32, 333, true);
        }

        public static bool IsRadioStationCustom(RadioStationTypes sType)
        {
            return sType >= RadioStationTypes.MP_BRP;
        }

        public static void SetVehicleRadioStation(Vehicle veh, RadioStationTypes sType)
        {
            string radioId = GetRadioIdByStationType(sType);

            veh.SetVehRadioStation(radioId);

            Audio.Data audioData = Audio.AllAudios.Where(x => x.Entity == veh && x.Id.StartsWith("ENT_VR")).FirstOrDefault();

            if (IsRadioStationCustom(sType))
            {
                Audio.TrackTypes trackType = CustomStationTracks.GetValueOrDefault(sType);

                if (audioData == null)
                    audioData = new Audio.Data($"ENT_VR_{veh.Handle}", veh, 7.5f, 1f);

                if (audioData.CurrentTrackType != trackType)
                    audioData.RequestTrackUpdate(trackType);
            }
            else
            {
                audioData?.Destroy();
            }
        }
    }
}