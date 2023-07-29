using System.Collections.Generic;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Items
{
    public partial class WeaponComponent
    {
        public static readonly Dictionary<string, Item.ItemData> IdList = new Dictionary<string, Item.ItemData>()
        {
            {
                "wc_s",
                new ItemData("Глушитель (компонент)",
                    0.01f,
                    "w_am_case",
                    ItemData.Types.Suppressor,
                    (uint)WeaponHash.Pistol,
                    (uint)WeaponHash.Combatpistol,
                    (uint)WeaponHash.Appistol,
                    (uint)WeaponHash.Pistol50,
                    (uint)WeaponHash.Heavypistol,
                    (uint)WeaponHash.Snspistol_mk2
                )
            },
            { "wc_f", new ItemData("Фонарик (компонент)", 0.01f, "w_am_case", ItemData.Types.Flashlight) },
            { "wc_g", new ItemData("Рукоятка (компонент)", 0.01f, "w_am_case", ItemData.Types.Grip) },
            { "wc_sc", new ItemData("Прицел (компонент)", 0.01f, "w_am_case", ItemData.Types.Scope) },
        };
    }
}