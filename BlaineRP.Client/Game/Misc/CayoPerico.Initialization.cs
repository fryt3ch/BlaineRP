using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.Game.Helpers.Colshapes.Enums;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Misc
{
    public static partial class CayoPerico
    {
        [Script]
        public class Initialization
        {
            public Initialization()
            {
                MainColshape = new Circle(new Vector3(4840.571f, -5174.425f, 0f), 2374f, false, new Utils.Colour(0, 0, 255, 125), uint.MaxValue, null)
                {
                    Name = "CayoPerico_Loader",
                    ApproveType = ApproveTypes.None,
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

                var mainBlip = new ExtraBlip(836, new Vector3(4900.16f, -5192.03f, 2.44f), "Cayo Perico", 1.1f, 49, 255, 0f, true, 0, 0f, Settings.App.Static.MainDimension);
            }
        }
    }
}