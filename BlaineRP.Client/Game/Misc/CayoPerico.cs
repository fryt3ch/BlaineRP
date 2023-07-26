using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.World.Enums;
using BlaineRP.Client.Utils;

namespace BlaineRP.Client.Game.Misc
{
    public static partial class CayoPerico
    {
        public static bool IslandLoaded { get; set; }

        public static ExtraColshape MainColshape { get; set; }

        private static AsyncTask LoadTask { get; set; }

        public static void ToggleCayoPericoIsland(bool state, bool updateCustomWeather)
        {
            SetIslandHopperEnabledHeistIsland(state);

            SetToggleMinimapHeistIsland(state);

            if (updateCustomWeather)
                World.Core.SetSpecialWeather(state ? (WeatherTypes?)WeatherTypes.EXTRASUNNY : null);

            LoadTask?.Cancel();

            if (state)
                LoadTask = new AsyncTask(() =>
                    {
                        SetIslandHopperEnabledHeistIsland(true);

                        RAGE.Game.Streaming.RemoveIpl("h4_islandx_sea_mines");

                        LoadTask = null;
                    },
                    2000,
                    false,
                    0
                );
            else
                LoadTask = new AsyncTask(() =>
                    {
                        SetIslandHopperEnabledHeistIsland(false);

                        RAGE.Game.Streaming.RequestIpl("h4_islandx_terrain_01_slod");
                        RAGE.Game.Streaming.RequestIpl("h4_islandx_terrain_02_slod");
                        RAGE.Game.Streaming.RequestIpl("h4_islandx_terrain_03_lod");
                        RAGE.Game.Streaming.RequestIpl("h4_islandx_terrain_04_slod");
                        RAGE.Game.Streaming.RequestIpl("h4_islandx_terrain_05_slod");
                        RAGE.Game.Streaming.RequestIpl("h4_islandx_terrain_06_slod");

                        int intid = RAGE.Game.Interior.GetInteriorAtCoords(4840.571f, -5174.425f, 2f);

                        RAGE.Game.Interior.RefreshInterior(intid);

                        LoadTask = null;
                    },
                    1550,
                    false,
                    0
                );

            LoadTask.Run();

            IslandLoaded = state;
        }

        private static void SetIslandHopperEnabledHeistIsland(bool state)
        {
            RAGE.Game.Invoker.Invoke(0x9A9D1BA639675CF1, "HeistIsland", state);
            // SetIslandHopperEnabled
        }

        private static void SetToggleMinimapHeistIsland(bool state)
        {
            RAGE.Game.Invoker.Invoke(0x5E1460624D194A38, state);
            // SetToggleMinimapHeistIsland
        }
    }
}