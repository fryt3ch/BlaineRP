﻿using System.Collections.Generic;

namespace BlaineRP.Client.Game.Items
{
    public class Bag : Clothes, IContainer
    {
        public static Dictionary<string, Item.ItemData> IdList { get; set; } = new Dictionary<string, Item.ItemData>();

        public new class ItemData : Clothes.ItemData
        {
            public ItemData(string name, float weight, bool sex, int drawable, int[] textures, byte maxSlots, float maxWeight, string sexAlternativeId = null) : base(name,
                weight,
                sex,
                drawable,
                textures,
                sexAlternativeId
            )
            {
                MaxSlots = maxSlots;

                MaxWeight = maxWeight;
            }

            /// <summary>Максимальное кол-во слотов</summary>
            public byte MaxSlots { get; set; }

            /// <summary>Максимальный вес содержимого</summary>
            public float MaxWeight { get; set; }
        }
    }
}