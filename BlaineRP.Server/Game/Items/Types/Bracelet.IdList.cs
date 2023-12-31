﻿using System.Collections.Generic;

namespace BlaineRP.Server.Game.Items
{
    public partial class Bracelet
    {
        public static readonly Dictionary<string, Item.ItemData> IdList = new Dictionary<string, Item.ItemData>()
        {
            #region Bracelets Male

            {
                "bracelet_m_0", new ItemData("Плетеный браслет",
                    true,
                    0,
                    new int[]
                    {
                        0
                    },
                    "bracelet_f_7"
                )
            },
            {
                "bracelet_m_1", new ItemData("Плетеный браслет #2",
                    true,
                    1,
                    new int[]
                    {
                        0
                    },
                    "bracelet_f_8"
                )
            },
            {
                "bracelet_m_2", new ItemData("Жесткое плетение",
                    true,
                    2,
                    new int[]
                    {
                        0
                    },
                    "bracelet_f_9"
                )
            },
            {
                "bracelet_m_3", new ItemData("Плетение с черепами",
                    true,
                    3,
                    new int[]
                    {
                        0
                    },
                    "bracelet_f_10"
                )
            },
            {
                "bracelet_m_4", new ItemData("Плетение Z",
                    true,
                    4,
                    new int[]
                    {
                        0
                    },
                    "bracelet_f_11"
                )
            },
            {
                "bracelet_m_5", new ItemData("Плетение с браслетом",
                    true,
                    5,
                    new int[]
                    {
                        0
                    },
                    "bracelet_f_12"
                )
            },
            {
                "bracelet_m_6", new ItemData("Браслет с шипами",
                    true,
                    6,
                    new int[]
                    {
                        0
                    },
                    "bracelet_f_13"
                )
            },
            {
                "bracelet_m_7", new ItemData("Кожаный напульсник",
                    true,
                    7,
                    new int[]
                    {
                        0,
                        1,
                        2,
                        3
                    },
                    "bracelet_f_14"
                )
            },
            {
                "bracelet_m_8", new ItemData("Светящиеся браслеты",
                    true,
                    8,
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
                        11
                    },
                    "bracelet_f_15"
                )
            },

            #endregion

            #region Bracelets Female

            {
                "bracelet_f_0", new ItemData("Золотой браслет",
                    false,
                    0,
                    new int[]
                    {
                        0
                    },
                    null
                )
            },
            {
                "bracelet_f_1", new ItemData("Золотой браслет #2",
                    false,
                    1,
                    new int[]
                    {
                        0
                    },
                    null
                )
            },
            {
                "bracelet_f_2", new ItemData("Золотой браслет #3",
                    false,
                    2,
                    new int[]
                    {
                        0
                    },
                    null
                )
            },
            {
                "bracelet_f_3", new ItemData("Золотой браслет #4",
                    false,
                    3,
                    new int[]
                    {
                        0
                    },
                    null
                )
            },
            {
                "bracelet_f_4", new ItemData("Золотой браслет #5",
                    false,
                    4,
                    new int[]
                    {
                        0
                    },
                    null
                )
            },
            {
                "bracelet_f_5", new ItemData("Золотой браслет #6",
                    false,
                    5,
                    new int[]
                    {
                        0
                    },
                    null
                )
            },
            {
                "bracelet_f_6", new ItemData("Золотой браслет #7",
                    false,
                    6,
                    new int[]
                    {
                        0
                    },
                    null
                )
            },
            {
                "bracelet_f_7", new ItemData("Плетеный браслет",
                    false,
                    7,
                    new int[]
                    {
                        0
                    },
                    "bracelet_m_0"
                )
            },
            {
                "bracelet_f_8", new ItemData("Плетеный браслет #2",
                    false,
                    8,
                    new int[]
                    {
                        0
                    },
                    "bracelet_m_1"
                )
            },
            {
                "bracelet_f_9", new ItemData("Жесткое плетение",
                    false,
                    9,
                    new int[]
                    {
                        0
                    },
                    "bracelet_m_2"
                )
            },
            {
                "bracelet_f_10", new ItemData("Плетение с черепами",
                    false,
                    10,
                    new int[]
                    {
                        0
                    },
                    "bracelet_m_3"
                )
            },
            {
                "bracelet_f_11", new ItemData("Плетение Z",
                    false,
                    11,
                    new int[]
                    {
                        0
                    },
                    "bracelet_m_4"
                )
            },
            {
                "bracelet_f_12", new ItemData("Плетение с браслетом",
                    false,
                    12,
                    new int[]
                    {
                        0
                    },
                    "bracelet_m_5"
                )
            },
            {
                "bracelet_f_13", new ItemData("Браслет с шипами",
                    false,
                    13,
                    new int[]
                    {
                        0
                    },
                    "bracelet_m_6"
                )
            },
            {
                "bracelet_f_14", new ItemData("Кожаный напульсник",
                    false,
                    14,
                    new int[]
                    {
                        0,
                        1,
                        2,
                        3
                    },
                    "bracelet_m_7"
                )
            },
            {
                "bracelet_f_15", new ItemData("Светящиеся браслеты",
                    false,
                    15,
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
                        11
                    },
                    "bracelet_m_8"
                )
            },

            #endregion
        };
    }
}