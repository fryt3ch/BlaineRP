using System.Collections.Generic;

namespace BlaineRP.Server.Game.Items
{
    public partial class Bag
    {
        public static readonly Dictionary<string, Item.ItemData> IdList = new Dictionary<string, Item.ItemData>()
        {
            {
                "bag_m_0", new ItemData("Обычная сумка",
                    true,
                    81,
                    new int[]
                    {
                        0,
                        1,
                        2,
                        3,
                        4,
                        5,
                        6,
                        7,
                        8,
                        9,
                        10,
                        11,
                        12,
                        13,
                        14,
                        15
                    },
                    10,
                    5f,
                    "bag_f_0"
                )
            },
            {
                "bag_m_1", new ItemData("Большая сумка",
                    true,
                    82,
                    new int[]
                    {
                        0,
                        1,
                        2,
                        3,
                        4,
                        5,
                        6,
                        7,
                        8,
                        9,
                        10,
                        11,
                        12,
                        13,
                        14,
                        15
                    },
                    15,
                    10f,
                    "bag_f_0"
                )
            },

            {
                "bag_m_2", new ItemData("Сумка BIGNESS",
                    true,
                    85,
                    new int[]
                    {
                        0,
                        1,
                        2,
                        3,
                        4,
                        5,
                        6,
                        7,
                        8,
                        9,
                        10,
                        11,
                        12,
                        13,
                        14,
                        15,
                        16,
                        17,
                        18,
                        19,
                        20,
                        21,
                        22,
                        23,
                        24
                    },
                    10,
                    5f,
                    "bag_f_0"
                )
            },
            {
                "bag_m_3", new ItemData("Сумка BIGNESS (большая)",
                    true,
                    86,
                    new int[]
                    {
                        0,
                        1,
                        2,
                        3,
                        4,
                        5,
                        6,
                        7,
                        8,
                        9,
                        10,
                        11,
                        12,
                        13,
                        14,
                        15,
                        16,
                        17,
                        18,
                        19,
                        20,
                        21,
                        22,
                        23,
                        24
                    },
                    15,
                    10f,
                    "bag_f_0"
                )
            },

            {
                "bag_m_4", new ItemData("Сумка Hinterland",
                    true,
                    40,
                    new int[]
                    {
                        0
                    },
                    10,
                    5f,
                    "bag_f_0"
                )
            },
            {
                "bag_m_5", new ItemData("Сумка Hinterland (большая)",
                    true,
                    41,
                    new int[]
                    {
                        0
                    },
                    15,
                    10f,
                    "bag_f_0"
                )
            },

            {
                "bag_m_6", new ItemData("Обычная сумка #2",
                    true,
                    44,
                    new int[]
                    {
                        0
                    },
                    10,
                    5f,
                    "bag_f_0"
                )
            },
            {
                "bag_m_7", new ItemData("Большая сумка #2",
                    true,
                    45,
                    new int[]
                    {
                        0
                    },
                    15,
                    10f,
                    "bag_f_0"
                )
            },

            {
                "bag_f_0", new ItemData("Обычная сумка",
                    false,
                    81,
                    new int[]
                    {
                        0,
                        1,
                        2,
                        3,
                        4,
                        5
                    },
                    10,
                    5f,
                    "bag_m_0"
                )
            },
        };
    }
}