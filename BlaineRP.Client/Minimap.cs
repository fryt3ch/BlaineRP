using BlaineRP.Client.Utils;

namespace BlaineRP.Client
{
    [Script(int.MaxValue)]
    public class MiniMap
    {
        public static byte ZoomState { get; private set; } = 0;

        private static AsyncTask _zoomTask;

        public MiniMap()
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
            switch (ZoomState)
            {
                case 0:
                    RAGE.Game.Ui.SetRadarZoom(0);

                    ZoomState = 1;

                    _zoomTask?.Cancel();

                    _zoomTask = new AsyncTask(() =>
                    {
                        RAGE.Game.Ui.SetRadarBigmapEnabled(false, true);
                        RAGE.Game.Ui.SetRadarZoom(1);

                        ZoomState = 0;

                        if (_zoomTask != null)
                        {
                            _zoomTask.Cancel();

                            _zoomTask = null;
                        }
                    }, 10_000, false, 0);

                    _zoomTask.Run();
                    break;
                case 1:
                    {
                        if (_zoomTask != null)
                        {
                            _zoomTask.Cancel();

                            _zoomTask = null;
                        }

                        RAGE.Game.Ui.SetRadarBigmapEnabled(true, false);
                        RAGE.Game.Ui.Unknown._0x82CEDC33687E1F50(true);
                        RAGE.Game.Ui.SetRadarZoom(0);

                        ZoomState = 2;

                        CEF.HUD.UpdateLeftHUDPos();
                        break;
                    }
                default:
                    {
                        if (_zoomTask != null)
                        {
                            _zoomTask.Cancel();

                            _zoomTask = null;
                        }

                        ZoomState = 0;

                        ResetZoom();
                        break;
                    }
            }
        }
    }
}
