using RAGE;
using RAGE.Elements;

namespace BCRPClient.Data
{
    public partial class Locations : Events.Script
    {
        public static class CayoPerico
        {
            public static Blip MainBlip { get; set; }

            public static bool IslandLoaded { get; set; }

            public static Additional.ExtraColshape MainColshape { get; set; }

            private static AsyncTask LoadTask { get; set; }

            public static void Initialize()
            {
                MainColshape = new Additional.Circle(new Vector3(4840.571f, -5174.425f, 0f), 2374f, false, new Utils.Colour(0, 0, 255, 125), uint.MaxValue, null)
                {
                    Name = "CayoPerico_Loader",

                    ApproveType = Additional.ExtraColshape.ApproveTypes.None,

                    OnEnter = (cancel) =>
                    {
                        if (Player.LocalPlayer.Dimension == 2)
                            return;

                        if (IslandLoaded)
                            return;

                        ToggleCayoPericoIsland(true, true);
                    },

                    OnExit = (cancel) =>
                    {
                        if (!IslandLoaded)
                            return;

                        ToggleCayoPericoIsland(false, true);
                    },

                };

                ToggleCayoPericoIsland(false, false);

                MainBlip = new Blip(836, new Vector3(4900.16f, -5192.03f, 2.44f), "Cayo Perico", 1.1f, 49, 255, 0f, true, 0, 0f, Settings.MAIN_DIMENSION);
            }

            public static void ToggleCayoPericoIsland(bool state, bool updateCustomWeather)
            {
                SetIslandHopperEnabledHeistIsland(state);

                SetToggleMinimapHeistIsland(state);

                if (updateCustomWeather)
                    Sync.World.SetSpecialWeather(state ? (Sync.World.WeatherTypes?)Sync.World.WeatherTypes.EXTRASUNNY : null);

                LoadTask?.Cancel();

                if (state)
                {
                    LoadTask = new AsyncTask(() =>
                    {
                        SetIslandHopperEnabledHeistIsland(true);

                        RAGE.Game.Streaming.RemoveIpl("h4_islandx_sea_mines");

                        LoadTask = null;
                    }, 2000, false, 0);
                }
                else
                {
                    LoadTask = new AsyncTask(() =>
                    {
                        SetIslandHopperEnabledHeistIsland(false);

                        RAGE.Game.Streaming.RequestIpl("h4_islandx_terrain_01_slod");
                        RAGE.Game.Streaming.RequestIpl("h4_islandx_terrain_02_slod");
                        RAGE.Game.Streaming.RequestIpl("h4_islandx_terrain_03_lod");
                        RAGE.Game.Streaming.RequestIpl("h4_islandx_terrain_04_slod");
                        RAGE.Game.Streaming.RequestIpl("h4_islandx_terrain_05_slod");
                        RAGE.Game.Streaming.RequestIpl("h4_islandx_terrain_06_slod");

                        var intid = RAGE.Game.Interior.GetInteriorAtCoords(4840.571f, -5174.425f, 2f);

                        RAGE.Game.Interior.RefreshInterior(intid);

                        LoadTask = null;
                    }, 1550, false, 0);
                }

                LoadTask.Run();

                IslandLoaded = state;
            }

            private static void SetIslandHopperEnabledHeistIsland(bool state) => RAGE.Game.Invoker.Invoke(0x9A9D1BA639675CF1, "HeistIsland", state); // SetIslandHopperEnabled

            private static void SetToggleMinimapHeistIsland(bool state) => RAGE.Game.Invoker.Invoke(0x5E1460624D194A38, state); // SetToggleMinimapHeistIsland
        }
    }
}
