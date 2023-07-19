using RAGE;

namespace BCRPClient
{
    [Script(int.MaxValue)]
    public class Minimap 
    {
        public static byte MinimapZoomState { get; private set; } = 0;

        private static AsyncTask _zoomTask;

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

                _zoomTask?.Cancel();

                _zoomTask = new AsyncTask(() =>
                {
                    RAGE.Game.Ui.SetRadarBigmapEnabled(false, true);
                    RAGE.Game.Ui.SetRadarZoom(1);

                    MinimapZoomState = 0;

                    if (_zoomTask != null)
                    {
                        _zoomTask.Cancel();

                        _zoomTask = null;
                    }
                }, 10_000, false, 0);

                _zoomTask.Run();
            }
            else if (MinimapZoomState == 1)
            {
                if (_zoomTask != null)
                {
                    _zoomTask.Cancel();

                    _zoomTask = null;
                }

                RAGE.Game.Ui.SetRadarBigmapEnabled(true, false);
                RAGE.Game.Ui.Unknown._0x82CEDC33687E1F50(true);
                RAGE.Game.Ui.SetRadarZoom(0);

                MinimapZoomState = 2;

                CEF.HUD.UpdateLeftHUDPos();
            }
            else
            {
                if (_zoomTask != null)
                {
                    _zoomTask.Cancel();

                    _zoomTask = null;
                }

                MinimapZoomState = 0;

                ResetZoom();
            }
        }
    }
}
