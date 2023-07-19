using System.Collections.Generic;

namespace BCRPServer.Sync
{
    public class Players
    {
        public enum PhoneStateTypes : byte
        {
            /// <summary>Телефон не используется</summary>
            Off = 0,
            /// <summary>Телефон используется без анимаций</summary>
            JustOn,
            /// <summary>Телефон используется c обычной анимацией</summary>
            Idle,
            /// <summary>Телефон используется с анимацией разговора</summary>
            Call,
            /// <summary>Телефон используется с анимацией камеры 0</summary>
            Camera,
        }

        public static HashSet<uint> UsedPhoneNumbers { get; private set; } = new HashSet<uint>();

        public static uint GenerateNewPhoneNumber()
        {
            while (true)
            {
                var num = (uint)SRandom.NextInt32(100_000, 999_999_999);

                if (!UsedPhoneNumbers.Contains(num))
                {
                    UsedPhoneNumbers.Add(num);

                    return num;
                }
            }
        }

        public static void DisableMicrophone(PlayerData pData)
        {
            if (pData.VoiceRange == 0f)
                return;

            pData.VoiceRange = 0f;

            var player = pData.Player;

            for (int i = 0; i < pData.Listeners.Count; i++)
            {
                var target = pData.Listeners[i];

                if (target == null)
                    continue;

                player.DisableVoiceTo(target);
            }

            pData.Listeners.Clear();
        }

        public static void StopUsePhone(PlayerData pData)
        {
            var player = pData.Player;

            pData.PhoneStateType = PhoneStateTypes.Off;

            if (pData.ActiveCall is Sync.Phone.Call activeCall)
            {
                activeCall.Cancel(activeCall.Caller == pData ? Phone.Call.CancelTypes.Caller : Phone.Call.CancelTypes.Receiver);
            }

            player.DetachObject(AttachSystem.Types.PhoneSync);

            Sync.Chat.SendLocal(Sync.Chat.MessageTypes.Me, player, Language.Strings.Get("CHAT_PLAYER_PHONE_OFF"));
        }

        public static void ExitFromBuiness(PlayerData pData, bool teleport = true)
        {
            var player = pData.Player;

            var business = pData.CurrentBusiness;

            if (business is Game.Businesses.IEnterable enterable)
            {
                pData.IsInvincible = false;

                pData.CurrentBusiness = null;

                var t = Game.Businesses.Business.GetNextExitProperty(enterable);

                if (business is Game.Businesses.TuningShop ts)
                {
                    player.TriggerEvent("Shop::Close::Server");

                    var veh = pData.CurrentTuningVehicle;

                    if (veh?.Vehicle?.Exists != true)
                    {
                        if (teleport)
                            player.Teleport(t.Position, false, Properties.Settings.Profile.Current.Game.MainDimension, t.RotationZ, true);
                    }
                    else
                    {
                        if (player.Vehicle != veh.Vehicle || !teleport)
                        {
                            if (teleport)
                            {
                                player.Teleport(t.Position, false, Properties.Settings.Profile.Current.Game.MainDimension, t.RotationZ, true);

                                player.WarpToVehicleSeat(veh.Vehicle, 0, 5000);
                            }

                            veh.Vehicle.Teleport(t.Position, Properties.Settings.Profile.Current.Game.MainDimension, t.RotationZ, false, Additional.AntiCheat.VehicleTeleportTypes.Default);

                            veh.AttachBoatToTrailer();
                        }
                        else
                        {
                            veh.Vehicle.Teleport(t.Position, Properties.Settings.Profile.Current.Game.MainDimension, t.RotationZ, true, Additional.AntiCheat.VehicleTeleportTypes.OnlyDriver);

                            veh.AttachBoatToTrailer();
                        }

                        veh.Tuning.Apply(veh.Vehicle);
                    }

                    pData.CurrentTuningVehicle = null;
                }
                else
                {
                    if (teleport)
                        player.Teleport(t.Position, true, Properties.Settings.Profile.Current.Game.MainDimension, t.RotationZ, true);

                    player.TriggerEvent("Shop::Close::Server");

                    if (business.Type == Game.Businesses.Business.Types.BarberShop)
                        pData.UpdateCustomization();
                    else if (business.Type == Game.Businesses.Business.Types.TattooShop)
                        pData.UpdateDecorations();
                }
            }
            else
            {
                pData.CurrentBusiness = null;

                player.TriggerEvent("Shop::Close::Server");
            }
        }
    }
}
