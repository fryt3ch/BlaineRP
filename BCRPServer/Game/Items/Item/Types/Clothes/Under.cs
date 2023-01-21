using BCRPServer.Game.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Game.Items
{
    public class Under : Clothes, Clothes.IToggleable
    {
        new public class ItemData : Clothes.ItemData, Clothes.ItemData.IToggleable
        {
            public Top.ItemData BestTop { get; set; }

            public int BestTorso { get; set; }

            public ExtraData ExtraData { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {(BestTop == null ? "null" : $"new Top.ItemData({BestTop.Sex.ToString().ToLower()}, {BestTop.Drawable}, new int[] {{ {string.Join(", ", BestTop.Textures)} }}, {BestTorso}, {(BestTop.ExtraData == null ? "null" : $"new Under.ItemData.ExtraData({BestTop.ExtraData.Drawable}, {BestTop.ExtraData.BestTorso})")}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")})")} , {BestTorso}, {(ExtraData == null ? "null" : $"new Under.ItemData.ExtraData({ExtraData.Drawable}, {ExtraData.BestTorso})")}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")}";

            public ItemData(string Name, bool Sex, int Drawable, int[] Textures, Top.ItemData BestTop, int BestTorso, ExtraData ExtraData = null, string SexAlternativeID = null) : base(Name, 0.2f, "prop_ld_tshirt_02", Sex, Drawable, Textures, SexAlternativeID)
            {
                this.BestTop = BestTop;
                this.ExtraData = ExtraData;

                this.BestTorso = BestTorso;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "under_m_0", new ItemData("Футболка стандартная", true, 0, new int[] { 0, 1, 2, 3, 4, 5, 7, 8, 11 }, new Top.ItemData(true, 0, new int[] { 0, 1, 2, 3, 4, 5, 7, 8, 11 }, 0), 4, new ItemData.ExtraData(2, 4), null) },
            { "under_m_1", new ItemData("Майка стандартная", true, 5, new int[] { 0, 1, 2, 7 }, new Top.ItemData(true, 5, new int[] { 0, 1, 2, 7 }, 5), 6, null, null) },
            { "under_m_2", new ItemData("Футболка обычная", true, 1, new int[] { 0, 1, 3, 4, 5, 6, 7, 8, 11, 12, 14 }, new Top.ItemData(true, 1, new int[] { 0, 1, 3, 4, 5, 6, 7, 8, 11, 12, 14 }, 0), 1, new ItemData.ExtraData(14, 1), null) },
            { "under_m_3", new ItemData("Кофта обычная", true, 8, new int[] { 0, 10, 13, 14 }, new Top.ItemData(true, 8, new int[] { 0, 10, 13, 14 }, 11), 4, null, null) },
            { "under_m_4", new ItemData("Поло обычное", true, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, new Top.ItemData(true, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 0), 1, null, null) },
            { "under_m_5", new ItemData("Рубашка свободная", true, 63, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, new Top.ItemData(true, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 1), 1, new ItemData.ExtraData(64, 1), null) },
            { "under_m_6", new ItemData("Рубашка на выпуск", true, 29, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, new Top.ItemData(true, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 1), 4, new ItemData.ExtraData(30, 4), null) },
            { "under_m_7", new ItemData("Майка обычная", true, 17, new int[] { 0, 1, 2, 3, 4, 5 }, new Top.ItemData(true, 17, new int[] { 0, 1, 2, 3, 4, 5 }, 5), 6, null, null) },
            { "under_m_8", new ItemData("Свитшот", true, 41, new int[] { 0, 1, 2, 3, 4 }, new Top.ItemData(true, 38, new int[] { 0, 1, 2, 3, 4 }, 8), 4, null, null) },
            { "under_m_9", new ItemData("Поло с рисунками", true, 42, new int[] { 0, 1 }, new Top.ItemData(true, 39, new int[] { 0, 1 }, 0), 1, null, null) },
            { "under_m_10", new ItemData("Рубашка на выпуск", true, 43, new int[] { 0, 1, 2, 3 }, new Top.ItemData(true, 41, new int[] { 0, 1, 2, 3 }, 12), 1, null, null) },
            { "under_m_11", new ItemData("Рубашка с подтяжками", true, 45, new int[] { 0 }, new Top.ItemData(true, 42, new int[] { 0 }, 11, new ItemData.ExtraData(43, 11)), 4, new ItemData.ExtraData(46, 4), null) },
            { "under_m_12", new ItemData("Футболка обычная #2", true, 53, new int[] { 0, 1, 2, 3 }, new Top.ItemData(true, 47, new int[] { 0, 1, 2, 3 }, 0), 4, new ItemData.ExtraData(54, 4), null) },
            { "under_m_13", new ItemData("Рубашка приталенная", true, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null, 11, new ItemData.ExtraData(7, 11), null) },
            { "under_m_14", new ItemData("Рубашка приталенная #2", true, 27, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, new Top.ItemData(true, 26, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 11), 4, null, null) },
            { "under_m_15", new ItemData("Футболка золотая", true, 67, new int[] { 0 }, new Top.ItemData(true, 71, new int[] { 0 }, 0), 4, null, null) },
            { "under_m_16", new ItemData("Футболка модника", true, 65, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 }, new Top.ItemData(true, 73, new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 }, 0), 4, null, null) },
            { "under_m_17", new ItemData("Рубашка с жилетом", true, 4, new int[] { 0, 1, 2 }, null, 4, new ItemData.ExtraData(3, 4), null) },
            { "under_m_18", new ItemData("Рубашка обычная", true, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null, 4, new ItemData.ExtraData(11, 4), null) },
            { "under_m_19", new ItemData("Рубашка с разноцветным жилетом", true, 26, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, null, 4, new ItemData.ExtraData(25, 4), null) },
            { "under_m_20", new ItemData("Рубашка под смокинг", true, 33, new int[] { 0, 1, 2, 3, 4, 5, 6 }, null, 4, new ItemData.ExtraData(34, 4), null) },
            { "under_m_21", new ItemData("Рубашка с жилетом USA", true, 52, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, null, 4, new ItemData.ExtraData(51, 4), null) },
            { "under_m_22", new ItemData("Рубашка нараспашку", true, 69, new int[] { 0, 1, 2, 3, 4 }, null, 14, null, null) },
            { "under_m_24", new ItemData("Рубашка под жилетку #2", true, 22, new int[] { 0, 1, 2, 3, 4 }, null, 4, null, null) },
            { "under_m_25", new ItemData("Рубашка под жилетку #3", true, 93, new int[] { 0, 1 }, null, 11, null, null) },
            { "under_m_26", new ItemData("Рубашка под жилетку #4", true, 158, new int[] { 0 }, new Top.ItemData(true, 322, new int[] { 0 }, 1, new ItemData.ExtraData(321, 1)), 4, new ItemData.ExtraData(157, 4), null) },
            { "under_m_27", new ItemData("null", true, 79, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, new Top.ItemData(true, 152, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 1), 4, new ItemData.ExtraData(80, 4), null) },
            { "under_m_28", new ItemData("null", true, 109, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, new Top.ItemData(true, 235, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 0), 1, new ItemData.ExtraData(110, 1), null) },
            { "under_m_29", new ItemData("null", true, 187, new int[] { 0, 1, 2, 3, 4 }, new Top.ItemData(true, 392, new int[] { 0, 1, 2, 3, 4 }, 0), 4, new ItemData.ExtraData(188, 4), null) },
            { "under_m_30", new ItemData("null", true, 168, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, new Top.ItemData(true, 345, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 0), 4, new ItemData.ExtraData(169, 4), null) },
            { "under_m_31", new ItemData("null", true, 47, new int[] { 0, 1, 2, 3 }, new Top.ItemData(true, 44, new int[] { 0, 1, 2, 3 }, 0), 4, new ItemData.ExtraData(48, 4), null) },
            { "under_m_32", new ItemData("null", true, 16, new int[] { 0, 1, 2 }, new Top.ItemData(true, 16, new int[] { 0, 1, 2 }, 0), 1, new ItemData.ExtraData(18, 1), null) },
            { "under_m_33", new ItemData("null", true, 72, new int[] { 0, 1, 2, 3, 4, 5 }, new Top.ItemData(true, 139, new int[] { 0, 1, 2, 3, 4, 5 }, 4), 4, new ItemData.ExtraData(71, 4), null) },
            { "under_m_34", new ItemData("null", true, 76, new int[] { 0, 1, 2, 3, 4, 5, 7, 8 }, new Top.ItemData(true, 146, new int[] { 0, 1, 2, 3, 4, 5, 7, 8 }, 0), 4, new ItemData.ExtraData(77, 4), null) },
            { "under_m_35", new ItemData("null", true, 178, new int[] { 0, 1, 2, 3, 4, 5, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, null, 4, new ItemData.ExtraData(179, 4), null) },
            { "under_f_0", new ItemData("Майка дизайнерская", false, 23, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, new Top.ItemData(false, 26, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, 12), 0, null, null) },
            { "under_f_1", new ItemData("Футболка на выпуск", false, 26, new int[] { 0, 1, 2 }, new Top.ItemData(false, 30, new int[] { 0, 1, 2 }, 2), 0, null, null) },
            { "under_f_2", new ItemData("Майка с принтами", false, 27, new int[] { 0, 1, 2 }, new Top.ItemData(false, 32, new int[] { 0, 1, 2 }, 4), 0, null, null) },
            { "under_f_3", new ItemData("Футболка облегающая", false, 71, new int[] { 0, 1, 2 }, new Top.ItemData(false, 73, new int[] { 0, 1, 2 }, 14), 0, null, null) },
            { "under_f_4", new ItemData("Футболка USA", false, 31, new int[] { 0, 1 }, new Top.ItemData(false, 40, new int[] { 0, 1 }, 2), 0, null, null) },
            { "under_f_5", new ItemData("Свитшот", false, 82, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, new Top.ItemData(false, 149, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 1), 0, null, null) },
            { "under_f_6", new ItemData("Кофта обычная", false, 61, new int[] { 0, 1, 2, 3 }, new Top.ItemData(false, 75, new int[] { 0, 1, 2, 3 }, 9), 0, null, null) },
            { "under_f_7", new ItemData("Водолазка", false, 67, new int[] { 0, 1, 2, 3, 4, 5 }, new Top.ItemData(false, 103, new int[] { 0, 1, 2, 3, 4, 5 }, 3), 0, null, null) },
            { "under_f_8", new ItemData("Футболка обычная", false, 78, new int[] { 0, 1, 2, 3, 4, 5 }, new Top.ItemData(false, 141, new int[] { 0, 1, 2, 3, 4, 5 }, 14), 0, null, null) },
            { "under_f_9", new ItemData("Футболка черная", false, 147, new int[] { 0 }, new Top.ItemData(false, 236, new int[] { 0 }, 14), 0, null, null) },
            { "under_f_10", new ItemData("Корсет с кружевами", false, 22, new int[] { 0, 1, 2, 3, 4 }, new Top.ItemData(false, 22, new int[] { 0, 1, 2, 3, 4 }, 4), 0, null, null) },
            { "under_f_11", new ItemData("Майка с принтами #2", false, 29, new int[] { 0, 1, 2, 3, 4 }, new Top.ItemData(false, 36, new int[] { 0, 1, 2, 3, 4 }, 11), 0, null, null) },
            { "under_f_12", new ItemData("Футболка модницы", false, 45, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }, new Top.ItemData(false, 68, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }, 14), 0, null, null) },
            { "under_f_13", new ItemData("Футболка золотая", false, 49, new int[] { 0 }, new Top.ItemData(false, 67, new int[] { 0 }, 2), 0, null, null) },
            { "under_f_14", new ItemData("Майка модницы", false, 117, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, new Top.ItemData(false, 209, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, 12), 0, null, null) },
            { "under_f_15", new ItemData("Корсет с принтами", false, 13, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, new Top.ItemData(false, 13, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 4), 0, null, null) },
            { "under_f_16", new ItemData("Рубашка обычная", false, 38, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null, 0, null, null) },
            { "under_f_17", new ItemData("Рубашка в разноцветном жилете", false, 40, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null, 0, null, null) },
            { "under_f_18", new ItemData("Корсет с принтами #2", false, 68, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, new Top.ItemData(false, 111, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 4), 0, null, null) },
            { "under_f_19", new ItemData("Блузка с принтами", false, 176, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, new Top.ItemData(false, 283, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 12), 0, null, null) },
            { "under_f_20", new ItemData("Майка спортивная", false, 11, new int[] { 0, 1, 2, 10, 11, 15 }, new Top.ItemData(false, 11, new int[] { 0, 1, 2, 10, 11, 15 }, 11), 0, null, null) },
            { "under_f_21", new ItemData("Рубашка обычная #2", false, 24, new int[] { 0, 1, 2, 3, 4, 5 }, new Top.ItemData(false, 27, new int[] { 0, 1, 2, 3, 4, 5 }, 0), 0, null, null) },
            { "under_f_22", new ItemData("Футболка цветная", false, 30, new int[] { 0, 1, 2, 3 }, new Top.ItemData(false, 38, new int[] { 0, 1, 2, 3 }, 2), 0, null, null) },
            { "under_f_23", new ItemData("Футболка Xmas Criminal", false, 227, new int[] { 0, 1, 2, 3, 4 }, new Top.ItemData(false, 413, new int[] { 0, 1, 2, 3, 4  }, 14), 0, null, null) },
            { "under_f_24", new ItemData("Рубашка обычная #3", false, 217, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, null, 0, null, null) },
        };

        [JsonIgnore]
        public const int Slot = (int)Customization.ClothesTypes.Under;

        [JsonIgnore]
        /// <summary>Переключено ли состояние</summary>
        public bool Toggled { get; set; }

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        new public ItemData SexAlternativeData { get => (ItemData)base.SexAlternativeData; set => base.SexAlternativeData = value; }

        public override void Wear(PlayerData pData)
        {
            var player = pData.Player;

            var data = Data;

            var variation = Var;

            if (Data.Sex != pData.Sex)
            {
                data = SexAlternativeData;

                if (data == null)
                    return;

                if (variation >= data.Textures.Length)
                    variation = data.Textures.Length;
            }

            if (pData.Clothes[1] == null && data.BestTop != null)
            {
                if (Toggled && data.BestTop.ExtraData != null)
                {
                    player.SetClothes(Top.Slot, data.BestTop.ExtraData.Drawable, data.Textures[variation]);
                    player.SetClothes(Gloves.Slot, data.BestTop.ExtraData.BestTorso, 0);
                }
                else
                {
                    player.SetClothes(Top.Slot, data.BestTop.Drawable, data.Textures[variation]);
                    player.SetClothes(Gloves.Slot, data.BestTop.BestTorso, 0);
                }
            }
            else
            {
                if (Toggled && data.ExtraData != null)
                {
                    player.SetClothes(Slot, data.ExtraData.Drawable, data.Textures[variation]);
                    player.SetClothes(Gloves.Slot, data.ExtraData.BestTorso, 0);
                }
                else
                {
                    player.SetClothes(Slot, data.Drawable, data.Textures[variation]);
                    player.SetClothes(Gloves.Slot, data.BestTorso, 0);
                }
            }

            pData.Accessories[7]?.Wear(pData);
        }

        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            if (pData.Clothes[1] == null)
            {
                player.SetClothes(Top.Slot, Game.Data.Customization.GetNudeDrawable(Game.Data.Customization.ClothesTypes.Top, pData.Sex), 0);
                player.SetClothes(Slot, Game.Data.Customization.GetNudeDrawable(Game.Data.Customization.ClothesTypes.Top, pData.Sex), 0);
                player.SetClothes(Gloves.Slot, Game.Data.Customization.GetNudeDrawable(Game.Data.Customization.ClothesTypes.Gloves, pData.Sex), 0);

                pData.Accessories[7]?.Wear(pData);
            }
            else
            {
                player.SetClothes(Slot, Game.Data.Customization.GetNudeDrawable(Game.Data.Customization.ClothesTypes.Under, pData.Sex), 0);

                pData.Clothes[1].Wear(pData);
            }
        }

        public void Action(PlayerData pData)
        {
            if (Data.ExtraData == null)
                return;

            Toggled = !Toggled;

            Wear(pData);
        }

        public Under(string ID, int Variation) : base(ID, IDList[ID], typeof(Under), Variation)
        {

        }
    }
}
