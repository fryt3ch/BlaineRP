using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient
{
    class Interaction : Events.Script
    {
        public const float DLRange = 15f;

        private static Dictionary<Player, float> DistancesPlayers;
        private static Dictionary<Vehicle, float> DistancesVehicles;

        private static bool _Enabled = false;
        private static bool _EnabledVisual = false;

        private static bool _EnabledDL = false;

        public static bool Enabled { get => _Enabled; set { if (!_Enabled && value) { GameEvents.Render -= Render; GameEvents.Render += Render; _Enabled = value; } else if (_Enabled && !value) { GameEvents.Render -= Render; _Enabled = value; CurrentEntity = null; } } }
        public static bool EnabledVisual { get => _EnabledVisual; set => _EnabledVisual = value; }

        public static bool EnabledDL { get => _EnabledDL; set { if (!_EnabledDL && value) { GameEvents.Render -= RenderDL; GameEvents.Render += RenderDL; } else if (_EnabledDL && !value) GameEvents.Render -= RenderDL; _EnabledDL = value; } }

        public static RAGE.Elements.Entity CurrentEntity { get; set; }

        public Interaction()
        {
            DistancesPlayers = new Dictionary<Player, float>();
            DistancesVehicles = new Dictionary<Vehicle, float>();
        }

        private static void Render()
        {
            if (RAGE.Elements.Player.LocalPlayer.Vehicle != null)
            {
                CurrentEntity = RAGE.Elements.Player.LocalPlayer.Vehicle;

                return;
            }

            CurrentEntity = Utils.GetEntityPlayerLookAt(Settings.ENTITY_INTERACTION_MAX_DISTANCE_RENDER);

            if (CurrentEntity == null || !EnabledVisual)
                return;

            float x = 0f, y = 0f;

            if (!CurrentEntity.GetScreenPosition(ref x, ref y))
                return;

            Utils.DrawText(KeyBinds.Binds[KeyBinds.Types.Interaction].GetKeyString(), x, y, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
        }

        private static void RenderDL()
        {
            DistancesPlayers.Clear();
            DistancesVehicles.Clear();

            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            float screenX = 0f, screenY = 0f;

            if (pData.AdminLevel > -1)
            {
                foreach (var x in RAGE.Elements.Entities.Players.Streamed.ToList())
                {
                    if (x.Handle == Player.LocalPlayer.Handle || !x.IsOnScreen())
                        continue;

                    var dist = Vector3.Distance(Player.LocalPlayer.Position, x.Position);

                    if (dist <= DLRange)
                        DistancesPlayers.Add(x, dist);
                }

                foreach (var x in DistancesPlayers.OrderBy(x => x.Value).Select(x => x.Key).Take(5))
                {
                    if (x?.Exists != true)
                        continue;

                    if (!x.GetScreenPosition(ref screenX, ref screenY))
                        continue;

                    var data = Sync.Players.GetData(x);

                    if (data == null)
                    {
                        Utils.DrawText($"ISN'T LOGGED IN", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);

                        continue;
                    }

                    Utils.DrawText($"ID: {x.RemoteId} | CID: {data.CID}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                    Utils.DrawText($"HP: {x.GetRealHealth()} | Arm: {x.GetArmour()}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);

                    Utils.DrawText($"IsInvincible: {data.IsInvincible} | IsFrozen: {data.IsFrozen} | IsKnocked: {data.Knocked}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                    Utils.DrawText($"Voice: {(data.VoiceRange < 0f ? "muted" : (data.VoiceRange == 0f ? "off" : $"{data.VoiceRange} m"))}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);

                    Utils.DrawText($"Fraction: {data.Fraction}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                }
            }

            foreach (var x in RAGE.Elements.Entities.Vehicles.Streamed.ToList())
            {
                if (x?.Exists != true)
                    continue;

                if (!x.IsOnScreen())
                    continue;

                var dist = Vector3.Distance(Player.LocalPlayer.Position, x.Position);

                if (dist <= DLRange)
                    DistancesVehicles.Add(x, dist);
            }

            foreach (var x in DistancesVehicles.OrderBy(x => x.Value).Select(x => x.Key).Take(5))
            {
                if (x?.Exists != true)
                    continue;

                var data = Sync.Vehicles.GetData(x);

                if (x == null)
                    continue;

                if (!x.GetScreenPosition(ref screenX, ref screenY))
                    continue;

                if (pData.AdminLevel > -1)
                {
                    Utils.DrawText($"ID: {x.RemoteId} | VID: {data.VID} | TID: {(data.TID == null ? "null" : data.TID.ToString())}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                    Utils.DrawText($"EngineOn: {data.EngineOn} | Locked: {data.DoorsLocked} | TrunkLocked: {data.TrunkLocked}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                    Utils.DrawText($"Fuel: {data.FuelLevel.ToString("0.00")} | Mileage: {data.Mileage.ToString("0.00")}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                    Utils.DrawText($"EngineHP: {x.GetEngineHealth()} | IsInvincible: {data.IsInvincible}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                    Utils.DrawText($"Speed: {x.GetSpeedKm().ToString("0.00")} | ForcedSpeed: {(data.ForcedSpeed * 3.6f).ToString("0.00")}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                }
                else
                {
                    Utils.DrawText($"ID: {x.RemoteId} | VID: {data.VID}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                    Utils.DrawText($"EngineHP: {x.GetEngineHealth()}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, Utils.ScreenTextFontTypes.CharletComprimeColonge, true);
                }
            }
        }
    }
}
