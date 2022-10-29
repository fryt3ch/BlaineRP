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
        private static bool _Enabled = false;

        public static bool Enabled { get => _Enabled; set { if (!_Enabled && value) { GameEvents.Render -= Render; GameEvents.Render += Render; _Enabled = value; } else if (_Enabled && !value) { GameEvents.Render -= Render; _Enabled = value; CurrentEntity = null; } } }
       
        public static bool EnabledVisual { get; set; }

        public static RAGE.Elements.Entity CurrentEntity { get; set; }

        public Interaction()
        {

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
    }
}
