using System.Collections.Generic;

namespace BlaineRP.Server.Game.Items
{
    public partial class Armour
    {
        public static readonly Dictionary<string, Item.ItemData> IdList = new Dictionary<string, Item.ItemData>()
        {
            {
                "arm_m_s", new ItemData("Обычный бронежилет",
                    0.5f,
                    true,
                    28,
                    new ItemData.Colours[]
                    {
                        ItemData.Colours.White,
                    },
                    19,
                    100,
                    "arm_m_s"
                )
            },
        };
    }
}