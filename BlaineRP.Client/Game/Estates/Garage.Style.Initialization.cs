using System.Collections.Generic;
using RAGE;

namespace BlaineRP.Client.Game.Estates
{
    public partial class Garage
    {
        public partial class Style
        {
            public static void LoadAll()
            {
                new Style(Types.Two, 0, new Vector3(179.0708f, -1005.729f, -98.99996f), null, null);
                new Style(Types.Six, 0, new Vector3(207.0894f, -998.9854f, -98.99996f), null, null);
                new Style(Types.Ten, 0, new Vector3(238.0103f, -1004.861f, -98.99996f), null, null);

                new Style(Types.Ten,
                    1,
                    new Vector3(238.0103f, -1004.861f, -98.99996f),
                    () =>
                    {
                        int intId = RAGE.Game.Interior.GetInteriorAtCoords(520f, -2625f, -50f);
                        Utils.Game.Misc.ToggleInteriorEntitySet(intId, "entity_set_tint_01", true);
                        Utils.Game.Misc.ToggleInteriorEntitySet(intId, "entity_set_shell_01", true);
                    },
                    () =>
                    {
                        int intId = RAGE.Game.Interior.GetInteriorAtCoords(520f, -2625f, -50f);
                        Utils.Game.Misc.ToggleInteriorEntitySet(intId, "entity_set_tint_01", false);
                        Utils.Game.Misc.ToggleInteriorEntitySet(intId, "entity_set_shell_01", false);
                    }
                );

                new Style(Types.Ten,
                    2,
                    new Vector3(238.0103f, -1004.861f, -98.99996f),
                    () =>
                    {
                        int intId = RAGE.Game.Interior.GetInteriorAtCoords(520f, -2625f, -50f);
                        Utils.Game.Misc.ToggleInteriorEntitySet(intId, "entity_set_tint_01", true);
                        Utils.Game.Misc.ToggleInteriorEntitySet(intId, "entity_set_shell_03", true);
                    },
                    () =>
                    {
                        int intId = RAGE.Game.Interior.GetInteriorAtCoords(520f, -2625f, -50f);
                        Utils.Game.Misc.ToggleInteriorEntitySet(intId, "entity_set_tint_01", false);
                        Utils.Game.Misc.ToggleInteriorEntitySet(intId, "entity_set_shell_03", false);
                    }
                );

                foreach (Dictionary<byte, Style> x in All.Values)
                {
                    foreach (Style y in x.Values)
                    {
                        y.OffAction?.Invoke();
                    }
                }
            }
        }
    }
}