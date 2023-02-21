using RAGE;
using System.Threading;

namespace BCRPClient
{
    class Minimap : Events.Script
    {
        public static int MinimapZoomState = 0;
        private static Timer MinimapZoomTimer = null;

        public Minimap()
        {
            ResetZoom();
        }

        public static void ResetZoom()
        {
            RAGE.Game.Ui.SetRadarBigmapEnabled(false, false);
            RAGE.Game.Ui.Unknown._0x82CEDC33687E1F50(false);
            RAGE.Game.Ui.SetRadarZoom(1);

            CEF.HUD.UpdateLeftHUDPos();
        }

        public static void Toggle()
        {
            if (MinimapZoomState == 0)
            {
                RAGE.Game.Ui.SetRadarZoom(0);

                MinimapZoomState = 1;

                MinimapZoomTimer = new Timer((object unused) =>
                {
                    RAGE.Game.Ui.SetRadarBigmapEnabled(false, true);
                    RAGE.Game.Ui.SetRadarZoom(1);

                    MinimapZoomState = 0;
                    MinimapZoomTimer.Dispose();
                    MinimapZoomTimer = null;

                }, null, 10000, Timeout.Infinite);
            }
            else if (MinimapZoomState == 1)
            {
                if (MinimapZoomTimer != null)
                {
                    MinimapZoomTimer.Dispose();
                    MinimapZoomTimer = null;
                }

                RAGE.Game.Ui.SetRadarBigmapEnabled(true, false);
                RAGE.Game.Ui.Unknown._0x82CEDC33687E1F50(true);
                RAGE.Game.Ui.SetRadarZoom(0);

                MinimapZoomState = 2;

                CEF.HUD.UpdateLeftHUDPos();
            }
            else
            {
                if (MinimapZoomTimer != null)
                {
                    MinimapZoomTimer.Dispose();
                    MinimapZoomTimer = null;
                }

                MinimapZoomState = 0;

                ResetZoom();
            }
        }
    }
}
