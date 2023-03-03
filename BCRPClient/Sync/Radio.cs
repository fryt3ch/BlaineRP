using RAGE;
using RAGE.Elements;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace BCRPClient.Sync
{
    public class Radio : Events.Script
    {
        public enum StationTypes : byte
        {
            /// <summary>Радио выкл.</summary>
            Off = 0,

            /// <summary>Los Santos Rock Radio</summary>
            LSRR,
            /// <summary>Non-Stop-Pop FM</summary>
            NSPFM,
            /// <summary>Radio Los Santos</summary>
            RLS,
            /// <summary>Channel X</summary>
            CHX,
            /// <summary>West Coast Talk Radio</summary>
            WCTR,
            /// <summary>Rebel Radio</summary>
            RR,
            /// <summary>Soulwax FM</summary>
            SWFM,
            /// <summary>East Los FM</summary>
            ELFM,
            /// <summary>West Coast Classics</summary>
            WCC,
            /// <summary>Blaine County Talk Radio</summary>
            BCTR,
            /// <summary>Blue Ark</summary>
            BA,
            /// <summary>Worldwide FM</summary>
            WWFM,
            /// <summary>FlyLo FM</summary>
            FLFM,
            /// <summary>The Lowdown 91.1</summary>
            TLD,
            /// <summary>Radio Mirror Park</summary>
            RMP,
            /// <summary>Space 103.2</summary>
            SPA,
            /// <summary>Vinewood Boulevard Radio</summary>
            VWBR,
            /// <summary>Self Radio</summary>
            SR,
            /// <summary>The Lab</summary>
            TL,
            /// <summary>Blonded Los Santos 97.8 FM</summary>
            BLS,
            /// <summary>Los Santos Underground Radiok</summary>
            LSUR,
            /// <summary>iFruit Radio</summary>
            IFR,
            /// <summary>Still Slipping Los Santos</summary>
            SSLS,
            /// <summary>Kult FM</summary>
            KFM,
            /// <summary>The Music Locker</summary>
            TML,
            /// <summary>MOTOMAMI Los Santos</summary>
            MMLS,

            /// <summary>Media Player</summary>
            /// <remarks>Фактически, радио Blaine RP</remarks>
            MP_BRP,

            BRP_0,
            BRP_1,
            BRP_2,
            BRP_3,
            BRP_4,
        }

        private static Dictionary<StationTypes, string> StationIds { get; set; } = new Dictionary<StationTypes, string>()
        {
            { StationTypes.Off, "OFF" },

            { StationTypes.LSRR, "RADIO_01_CLASS_ROCK" },
            { StationTypes.NSPFM, "RADIO_02_POP" },
            { StationTypes.RLS, "RADIO_03_HIPHOP_NEW" },
            { StationTypes.CHX, "RADIO_04_PUNK" },
            { StationTypes.WCTR, "RADIO_05_TALK_01" },
            { StationTypes.RR, "RADIO_06_COUNTRY" },
            { StationTypes.SWFM, "RADIO_07_DANCE_01" },
            { StationTypes.ELFM, "RADIO_08_MEXICAN" },
            { StationTypes.WCC, "RADIO_09_HIPHOP_OLD" },
            { StationTypes.BCTR, "RADIO_11_TALK_02" },
            { StationTypes.BA, "RADIO_12_REGGAE" },
            { StationTypes.WWFM, "RADIO_13_JAZZ" },
            { StationTypes.FLFM, "RADIO_14_DANCE_02" },
            { StationTypes.TLD, "RADIO_15_MOTOWN" },
            { StationTypes.RMP, "RADIO_16_SILVERLAKE" },
            { StationTypes.SPA, "RADIO_17_FUNK" },
            { StationTypes.VWBR, "RADIO_18_90S_ROCK" },
            { StationTypes.SR, "RADIO_19_USER" },
            { StationTypes.TL, "RADIO_20_THELAB" },
            { StationTypes.BLS, "RADIO_21_DLC_XM17" },
            { StationTypes.LSUR, "RADIO_22_DLC_BATTLE_MIX1_RADIO" },
            { StationTypes.IFR, "RADIO_23_DLC_XM19_RADIO" },
            { StationTypes.SSLS, "RADIO_27_DLC_PRHEI4" },
            { StationTypes.KFM, "RADIO_34_DLC_HEI4_KULT" },
            { StationTypes.TML, "RADIO_35_DLC_HEI4_MLR" },
            { StationTypes.MP_BRP, "RADIO_36_AUDIOPLAYER" },
            { StationTypes.MMLS, "RADIO_37_MOTOMAMI" },

            { StationTypes.BRP_0, "RADIO_999_BRPCR0_RADIO" }, // 46
            { StationTypes.BRP_1, "RADIO_998_BRPCR0_RADIO" }, // 47
            { StationTypes.BRP_2, "RADIO_997_BRPCR0_RADIO" }, // 48
            { StationTypes.BRP_3, "RADIO_996_BRPCR0_RADIO" }, // 49
            { StationTypes.BRP_4, "RADIO_995_BRPCR0_RADIO" }, // 50
        };

        private static Dictionary<StationTypes, CEF.Audio.TrackTypes> CustomStationTracks { get; set; } = new Dictionary<StationTypes, CEF.Audio.TrackTypes>()
        {
            { StationTypes.MP_BRP, CEF.Audio.TrackTypes.None },

            { StationTypes.BRP_0, CEF.Audio.TrackTypes.RadioEuropaPlus },
            { StationTypes.BRP_1, CEF.Audio.TrackTypes.RadioRecord },
            { StationTypes.BRP_2, CEF.Audio.TrackTypes.RadioRetroFM },
            { StationTypes.BRP_3, CEF.Audio.TrackTypes.RadioClassicFM },
            { StationTypes.BRP_4, CEF.Audio.TrackTypes.RadioSynthwave },
        };

        public static StationTypes CurrentStationType { get; private set; }

        public static bool IsMobilePhoneRadioEnabled { get; private set; }

        public Radio()
        {
            SetRadioStationName(StationTypes.MP_BRP, "Radio Blaine RP");
            SetRadioStationName(StationTypes.BRP_0, "Europa Plus");
            SetRadioStationName(StationTypes.BRP_1, "Record Radio");
            SetRadioStationName(StationTypes.BRP_2, "Retro FM");
            SetRadioStationName(StationTypes.BRP_3, "Classic UK");
            SetRadioStationName(StationTypes.BRP_4, "Synthwave FM");

            SetCustomRadioStationArtistName(null);
            SetCustomRadioStationTrackName(null);

            SetRadioStationLocked(StationTypes.SSLS, false);

            ToggleMobilePhoneRadio(false);

            (new AsyncTask(() =>
            {
                var sType = GetCurrentStationType();

                var stationChanged = CurrentStationType != sType;

                CurrentStationType = sType;

                if (Player.LocalPlayer.Vehicle is Vehicle veh)
                {
                    var vData = Sync.Vehicles.GetData(veh);

                    if (vData != null)
                    {
                        var actualStation = vData.Radio;

                        if (veh.GetPedInSeat(-1, 0) == Player.LocalPlayer.Handle || veh.GetPedInSeat(0, 0) == Player.LocalPlayer.Handle)
                        {
                            if (sType != actualStation && !Sync.Vehicles.LastRadioSent.IsSpam(1000, false, false))
                            {
                                Sync.Vehicles.LastRadioSent = Sync.World.ServerTime;

                                RAGE.Events.CallRemote("Vehicles::SetRadio", (byte)CurrentStationType);
                            }
                        }
                        else
                        {
                            if (CurrentStationType != actualStation)
                            {
                                SetCurrentRadioStationType(actualStation);
                            }
                        }
                    }
                }
                else
                {

                }
            }, 1000, true, 1000)).Run();
        }

        public static StationTypes GetCurrentStationType() => StationIds.Where(x => x.Value == RAGE.Game.Audio.GetPlayerRadioStationName()).FirstOrDefault().Key;

        public static string GetRadioIdByStationType(StationTypes sType) => StationIds.GetValueOrDefault(sType) ?? string.Empty;

        public static void SetCurrentRadioStationType(StationTypes sType)
        {
            var radioId = GetRadioIdByStationType(sType);

            RAGE.Game.Audio.SetRadioToStationName(radioId);

            if (Player.LocalPlayer.Vehicle is Vehicle veh)
            {
                SetVehicleRadioStation(veh, sType);
            }
            else if (IsRadioStationCustom(sType))
            {
                // todo
            }
        }

        public static void SetRadioStationName(StationTypes sType, string name)
        {
            var strName = StationIds.GetValueOrDefault(sType);

            if (strName == null || strName == "OFF")
                return;

            RAGE.Game.Gxt.Add(strName.ToLower(), name);
        }

        public static string GetRadioStationName(StationTypes sType)
        {
            var strName = StationIds.GetValueOrDefault(sType);

            if (strName == null || strName == "OFF")
                return string.Empty;

            return RAGE.Game.Gxt.Get(strName.ToLower());
        }

        public static void SetCustomRadioStationArtistName(string name) => RAGE.Game.Gxt.Add(0x43505A45, name?.ToUpper() ?? "");

        public static void SetCustomRadioStationTrackName(string name) => RAGE.Game.Gxt.Add(0x43FBDB98, name ?? "");

        public static void SetRadioStationLocked(StationTypes sType, bool state) => RAGE.Game.Invoker.Invoke(0x477D9DB48F889591, GetRadioIdByStationType(sType), state);

        public static void GetCurrentRadioTrackDetails(out string artistName, out string songName)
        {
            if (CurrentStationType == StationTypes.MP_BRP)
            {
                artistName = string.Empty;
                songName = string.Empty;
            }
            else
            {
                var details = RAGE.Game.Invoker.Invoke<int>(0x50B196FC9ED6545B).ToString();

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

            GameEvents.Render -= MobilePhoneRadioRender;

            if (state)
                GameEvents.Render += MobilePhoneRadioRender;
        }

        private static void MobilePhoneRadioRender()
        {
            if (Player.LocalPlayer.Vehicle is Vehicle veh && veh.GetIsEngineRunning())
                return;

            RAGE.Game.Pad.DisableControlAction(32, 81, true);
            RAGE.Game.Pad.DisableControlAction(32, 82, true);
            RAGE.Game.Pad.DisableControlAction(32, 83, true);
            RAGE.Game.Pad.DisableControlAction(32, 84, true);
            RAGE.Game.Pad.DisableControlAction(32, 85, true);

            RAGE.Game.Pad.DisableControlAction(32, 332, true);
            RAGE.Game.Pad.DisableControlAction(32, 333, true);
        }

        public static bool IsRadioStationCustom(StationTypes sType) => sType >= StationTypes.MP_BRP;

        public static void SetVehicleRadioStation(Vehicle veh, StationTypes sType)
        {
            var radioId = GetRadioIdByStationType(sType);

            veh.SetVehRadioStation(radioId);

            var audioData = CEF.Audio.AllAudios.Where(x => x.Entity == veh && x.Id.StartsWith("ENT_VR")).FirstOrDefault();

            if (IsRadioStationCustom(sType))
            {
                var trackType = CustomStationTracks.GetValueOrDefault(sType);

                if (audioData == null)
                    audioData = new CEF.Audio.Data($"ENT_VR_{veh.Handle}", veh, 7.5f, 1f);

                if (audioData.CurrentTrackType != trackType)
                {
                    audioData.RequestTrackUpdate(trackType);
                }
            }
            else
            {
                audioData?.Destroy();
            }
        }
    }
}
