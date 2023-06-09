using RAGE;
using System.Threading;

namespace BCRPClient
{
    class Minimap : Events.Script
    {
        public static byte MinimapZoomState { get; private set; } = 0;

        private static AsyncTask zoomTask;

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

                zoomTask?.Cancel();

                zoomTask = new AsyncTask(() =>
                {
                    RAGE.Game.Ui.SetRadarBigmapEnabled(false, true);
                    RAGE.Game.Ui.SetRadarZoom(1);

                    MinimapZoomState = 0;

                    if (zoomTask != null)
                    {
                        zoomTask.Cancel();

                        zoomTask = null;
                    }
                }, 10_000, false, 0);

                zoomTask.Run();
            }
            else if (MinimapZoomState == 1)
            {
                if (zoomTask != null)
                {
                    zoomTask.Cancel();

                    zoomTask = null;
                }

                RAGE.Game.Ui.SetRadarBigmapEnabled(true, false);
                RAGE.Game.Ui.Unknown._0x82CEDC33687E1F50(true);
                RAGE.Game.Ui.SetRadarZoom(0);

                MinimapZoomState = 2;

                CEF.HUD.UpdateLeftHUDPos();
            }
            else
            {
                if (zoomTask != null)
                {
                    zoomTask.Cancel();

                    zoomTask = null;
                }

                MinimapZoomState = 0;

                ResetZoom();
            }
        }
    }
}
