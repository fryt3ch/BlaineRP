using System.Collections.Generic;
using BlaineRP.Server.Game.EntitiesData.Players.Customization;

namespace BlaineRP.Server.Game.Items
{
    public partial class Holster
    {
        public static readonly Dictionary<string, Item.ItemData> IdList = new Dictionary<string, Item.ItemData>()
        {
            {
                "hl_m_0", new ItemData("Кобура на ногу",
                    true,
                    Service.DECLS_10_MALE_COMP_IDX_BASE_OFFSET + 2,
                    new int[]
                    {
                        0
                    },
                    Service.DECLS_10_MALE_COMP_IDX_BASE_OFFSET + 0,
                    "hl_f_0"
                )
            },
            {
                "hl_m_1", new ItemData("Кобура простая",
                    true,
                    Service.DECLS_10_MALE_COMP_IDX_BASE_OFFSET + 3,
                    new int[]
                    {
                        0
                    },
                    Service.DECLS_10_MALE_COMP_IDX_BASE_OFFSET + 1,
                    "hl_f_1"
                )
            },

            {
                "hl_f_0", new ItemData("Кобура на ногу",
                    false,
                    Service.DECLS_10_FEMALE_COMP_IDX_BASE_OFFSET + 2,
                    new int[]
                    {
                        0
                    },
                    Service.DECLS_10_FEMALE_COMP_IDX_BASE_OFFSET + 0,
                    "hl_m_0"
                )
            },
            {
                "hl_f_1", new ItemData("Кобура простая",
                    false,
                    Service.DECLS_10_FEMALE_COMP_IDX_BASE_OFFSET + 3,
                    new int[]
                    {
                        0
                    },
                    Service.DECLS_10_FEMALE_COMP_IDX_BASE_OFFSET + 1,
                    "hl_m_1"
                )
            },
        };
    }
}