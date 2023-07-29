using System.Collections.Generic;

namespace BlaineRP.Server.Game.Items
{
    public partial class WeaponSkin
    {
        public static readonly Dictionary<string, Item.ItemData> IdList = new Dictionary<string, Item.ItemData>()
        {
            { "ws_0_0", new ItemData("Зеленая раскраска (уни, обыч.)", 0f, "w_am_case", ItemData.Types.UniDef, 1) },
            { "ws_0_1", new ItemData("Золотая раскраска (уни, обыч.)", 0f, "w_am_case", ItemData.Types.UniDef, 2) },
            { "ws_0_2", new ItemData("Розовая раскраска (уни, обыч.)", 0f, "w_am_case", ItemData.Types.UniDef, 3) },
            { "ws_0_3", new ItemData("Армейская раскраска (уни, обыч.)", 0f, "w_am_case", ItemData.Types.UniDef, 4) },
            { "ws_0_4", new ItemData("Синяя раскраска (уни, обыч.)", 0f, "w_am_case", ItemData.Types.UniDef, 5) },
            { "ws_0_5", new ItemData("Оранжевая раскраска (уни, обыч.)", 0f, "w_am_case", ItemData.Types.UniDef, 6) },
            { "ws_0_6", new ItemData("Платиновая раскраска (уни, обыч.)", 0f, "w_am_case", ItemData.Types.UniDef, 7) },

            { "ws_1_1", new ItemData("Черно-белая раскраска (уни, Mk2)", 0f, "w_am_case", ItemData.Types.UniMk2, 2) },
        };
    }
}