﻿using RAGE;
using System;
using System.Collections.Generic;

namespace BCRPClient
{
    class NameTags : Events.Script
    {
        private const int MaxDistance = 450;
        private const float Width = 0.03f;
        private const float Height = 0.0065f;
        private const float Border = 0.001f;

        public const float Interval = 0.045f;

        private static Vector3 VoiceTextureResolution = new Vector3(32, 32, 0);

        private static bool _Enabled = false;

        public static bool Enabled { get => _Enabled; set { if (!_Enabled && value) { Events.Tick -= Render; Events.Tick += Render; } else if (_Enabled && !value) Events.Tick -= Render; _Enabled = value; } }

        public NameTags()
        {
            Nametags.Enabled = false;

            RAGE.Game.Graphics.RequestStreamedTextureDict("mpleaderboard", true);
        }

        #region Renders
        #region Main Render
        private static void Render(List<Events.TickNametagData> nametags)
        {
            if (nametags == null)
                return;

            var data = Sync.Players.GetData(RAGE.Elements.Player.LocalPlayer);

            if (data == null)
                return;

            float screenX = 0f, screenY = 0f;

            nametags.ForEach(nametag =>
            {
                if (nametag.Distance > MaxDistance)
                    return;

                var player = nametag.Player;

                if (player?.Exists != true)
                    return;

                player.SetResetFlag(200, true);

                var pData = Sync.Players.GetData(player);

                if (Settings.Other.DebugLabels && data.AdminLevel > -1)
                {
                    if (Utils.GetScreenCoordFromWorldCoord(player.Position, ref screenX, ref screenY))
                    {
                        if (pData == null)
                        {
                            Utils.DrawText($"ID: {player.RemoteId} | ISN'T LOGGED IN", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);

                            return;
                        }

                        Utils.DrawText($"ID: {player.RemoteId} | CID: {pData.CID}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                        Utils.DrawText($"HP: {player.GetRealHealth()} | Arm: {player.GetArmour()}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);

                        Utils.DrawText($"IsInvincible: {pData.IsInvincible} | IsFrozen: {pData.IsFrozen} | IsKnocked: {pData.IsKnocked}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                        Utils.DrawText($"Voice: {(pData.VoiceRange < 0f ? "muted" : (pData.VoiceRange == 0f ? "off" : $"{pData.VoiceRange} m"))}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);

                        Utils.DrawText($"Fraction: {pData.Fraction}", screenX, screenY += NameTags.Interval / 2f, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);
                    }
                }

                if (pData == null || player.GetAlpha() != 255)
                    return;

                float x = nametag.ScreenX;
                float y = nametag.ScreenY;

                float scale = nametag.Distance / MaxDistance;

                if (scale < 0.5f)
                    scale = 0.5f;

                float healthScale = player.GetRealHealth() / 100f;
                float armourScale = player.GetArmour() / 100f;

                y += scale * (0.05f * (GameEvents.ScreenResolution.Y / 1080));

                if (RAGE.Game.Graphics.HasStreamedTextureDictLoaded("mpleaderboard"))
                {
                    if (pData.VoiceRange < 0f)
                    {
                        RAGE.Game.Graphics.DrawSprite("mpleaderboard", "leaderboard_audio_mute", x, y - Interval / 2f, 0.8f * VoiceTextureResolution.X / GameEvents.ScreenResolution.X, 0.8f * VoiceTextureResolution.Y / GameEvents.ScreenResolution.Y, 0f, 255, 0, 0, 255, 0);
                    }
                    else if (pData.VoiceRange > 0f)
                    {
                        RAGE.Game.Graphics.DrawSprite("mpleaderboard", $"leaderboard_audio_{(player.VoiceVolume <= 0f ? "inactive" : Math.Ceiling(3f * player.VoiceVolume).ToString())}", x, y - Interval / 2f, 0.8f * VoiceTextureResolution.X / GameEvents.ScreenResolution.X, 0.8f * VoiceTextureResolution.Y / GameEvents.ScreenResolution.Y, 0f, 255, 255, 255, 255, 0);
                    }
                    else if (pData.IsInvalid)
                    {
                        RAGE.Game.Graphics.DrawSprite("mpleaderboard", "leaderboard_audio_mute", x, y - Interval / 2f, 0.8f * VoiceTextureResolution.X / GameEvents.ScreenResolution.X, 0.8f * VoiceTextureResolution.Y / GameEvents.ScreenResolution.Y, 0f, 255, 155, 0, 255, 0);
                    }
                }
                else
                    RAGE.Game.Graphics.RequestStreamedTextureDict("mpleaderboard", true);

                Utils.DrawText(player.GetName(true, false, true), x, y, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);

                if (!Settings.Interface.HideCID)
                    Utils.DrawText($"#{pData.CID}", x, y += Interval / 2, 255, 255, 255, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);

                if (pData.AdminLevel > -1)
                    Utils.DrawText(Locale.Get("PLAYER_ADMIN_L"), x, y += Interval / 2, 255, 0, 0, 255, 0.4f, RAGE.Game.Font.ChaletComprimeCologne, true);

                if (RAGE.Game.Player.IsPlayerFreeAimingAtEntity(player.Handle))
                {
                    y += Interval;

                    RAGE.Game.Graphics.DrawRect(x, y, (Width + Border * 2), (Height + Border * 2), 0, 0, 0, 200, 0);
                    RAGE.Game.Graphics.DrawRect(x, y, Width, Height, 150, 150, 150, 255, 0);
                    RAGE.Game.Graphics.DrawRect((x - Width / 2 * (1 - healthScale)), y, (Width * healthScale), (Height), 255, 255, 255, 200, 0);

                    if (armourScale > 0)
                    {
                        y += Height + Border;

                        RAGE.Game.Graphics.DrawRect(x, y, (Width + Border * 2), (Height + Border * 2), 0, 0, 0, 200, 0);
                        RAGE.Game.Graphics.DrawRect(x, y, Width, Height, 41, 66, 78, 255, 0);
                        RAGE.Game.Graphics.DrawRect((x - Width / 2 * (1 - armourScale)), y, (Width * armourScale), (Height), 48, 108, 135, 200, 0);
                    }
                }
            });
        }
        #endregion
        #endregion
    }
}
