using Newtonsoft.Json;
using System.Collections.Generic;
using BlaineRP.Server.EntitiesData.Players;

namespace BlaineRP.Server.Game.Items
{
    public class Hat : Clothes, Clothes.IToggleable, Clothes.IProp
    {
        new public class ItemData : Clothes.ItemData, Clothes.ItemData.IToggleable
        {
            public ExtraData ExtraData { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {(ExtraData == null ? "null" : $"new Hat.ItemData.ExtraData({ExtraData.Drawable}, {ExtraData.BestTorso})")}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")}";

            public ItemData(string Name, bool Sex, int Drawable, int[] Textures, ExtraData ExtraData = null, string SexAlternativeID = null) : base(Name, 0.1f, "prop_proxy_hat_01", Sex, Drawable, Textures, SexAlternativeID)
            {
                this.ExtraData = ExtraData;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "hat_m_0", new ItemData("Шапка обычная", true, 2, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_m_1", new ItemData("Панама", true, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_m_2", new ItemData("Кепка Snapback", true, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_m_3", new ItemData("Шапка трикотажная", true, 28, new int[] { 0, 1, 2, 3, 4, 5 }, null, null) },
            { "hat_m_4", new ItemData("Шапка восьмиклинка", true, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_m_5", new ItemData("Кепка козырьком назад", true, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_m_6", new ItemData("Бандана", true, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_m_7", new ItemData("Наушники", true, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_m_8", new ItemData("Панама с принтами", true, 20, new int[] { 0, 1, 2, 3, 4, 5 }, null, null) },
            { "hat_m_9", new ItemData("Шляпа USA", true, 31, new int[] { 0 }, null, null) },
            { "hat_m_10", new ItemData("Цилиндр USA", true, 32, new int[] { 0 }, null, null) },
            { "hat_m_11", new ItemData("Шапка USA", true, 34, new int[] { 0 }, null, null) },
            { "hat_m_12", new ItemData("Каска болельщика", true, 37, new int[] { 0, 1, 2, 3, 4, 5 }, null, null) },
            { "hat_m_13", new ItemData("Кепка Snapback #2", true, 54, new int[] { 0, 1 }, null, null) },
            { "hat_m_14", new ItemData("Кепка с принтами", true, 56, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null, null) },
            { "hat_m_15", new ItemData("Кепка дизайнерская", true, 63, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null, null) },
            { "hat_m_16", new ItemData("Кепка SecuroServ", true, 65, new int[] { 0 }, null, null) },
            { "hat_m_17", new ItemData("Бандана байкерская", true, 83, new int[] { 0, 1, 2, 3, 4, 5, 6 }, null, null) },
            { "hat_m_18", new ItemData("Панама разноцветная", true, 94, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null, null) },
            { "hat_m_19", new ItemData("Шапка трикотажная #2", true, 120, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null, null) },
            { "hat_m_20", new ItemData("Кепка Diamond", true, 135, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, null, null) },
            { "hat_m_21", new ItemData("Шлем с принтами", true, 16, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_m_22", new ItemData("Шлем обычный", true, 50, new int[] { 0 }, null, null) },
            { "hat_m_23", new ItemData("Шлем зеркальный", true, 51, new int[] { 0 }, null, null) },
            { "hat_m_24", new ItemData("Шлем разноцветный", true, 73, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, null, null) },
            { "hat_m_25", new ItemData("Шлем It's Go Time", true, 78, new int[] { 0, 1, 2, 3, 4 }, null, null) },
            { "hat_m_26", new ItemData("Шлем It's Go Time #2", true, 80, new int[] { 0, 1, 2, 3 }, null, null) },
            { "hat_m_27", new ItemData("Шлем модника", true, 82, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, null, null) },
            { "hat_m_28", new ItemData("Каска байкерская #2", true, 85, new int[] { 0 }, null, null) },
            { "hat_m_29", new ItemData("Каска байкерская #2 (козырёк)", true, 86, new int[] { 0 }, null, null) },
            { "hat_m_30", new ItemData("Каска байкерская #2 (ирокез)", true, 87, new int[] { 0 }, null, null) },
            { "hat_m_31", new ItemData("Каска байкерская #2 (шипы)", true, 88, new int[] { 0 }, null, null) },
            { "hat_m_32", new ItemData("Кепка брендовая #1", true, 76, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, null, null) },
            { "hat_m_33", new ItemData("Кепка брендовая #2", true, 109, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null, null) },
            { "hat_m_34", new ItemData("Кепка брендовая #3", true, 130, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 }, null, null) },
            { "hat_m_35", new ItemData("Кепка с принтами #2", true, 139, new int[] { 0, 1, 2 }, null, null) },
            { "hat_m_36", new ItemData("Кепка цветная", true, 142, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null, null) },
            { "hat_m_37", new ItemData("Кепка современная", true, 151, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_m_38", new ItemData("Элегантная панама", true, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_m_39", new ItemData("Ковбойская шляпа", true, 13, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_m_40", new ItemData("Порк-пай", true, 21, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_m_41", new ItemData("Шляпа с бабочкой", true, 25, new int[] { 0, 1, 2 }, null, null) },
            { "hat_m_42", new ItemData("Котелок", true, 26, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, null, null) },
            { "hat_m_43", new ItemData("Цилиндр", true, 27, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, null, null) },
            { "hat_m_44", new ItemData("Трилби", true, 29, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_m_45", new ItemData("Борсалино", true, 30, new int[] { 0, 1 }, null, null) },
            { "hat_m_46", new ItemData("Цилиндр USA #2", true, 33, new int[] { 0, 1 }, null, null) },
            { "hat_m_47", new ItemData("Кепка Snapback модника", true, 55, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null, null) },
            { "hat_m_48", new ItemData("Хомбург", true, 61, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null, null) },
            { "hat_m_49", new ItemData("Хомбург с принтами", true, 64, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null, null) },
            { "hat_m_50", new ItemData("Каска байкерская", true, 84, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null, null) },
            { "hat_m_51", new ItemData("Каска байкерская #3", true, 89, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null, null) },
            { "hat_m_52", new ItemData("Каска байкерская (металлик)", true, 90, new int[] { 0 }, null, null) },
            { "hat_m_53", new ItemData("Кепка модника", true, 96, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null, null) },
            { "hat_m_54", new ItemData("Кепка автолюбителя", true, 154, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, null, null) },

            { "hat_f_0", new ItemData("Шапка обычная", false, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_f_1", new ItemData("Шапка с принтами", false, 5, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_f_2", new ItemData("Фуражка", false, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_f_3", new ItemData("Восьмиклинка", false, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_f_4", new ItemData("Шляпа фермерская", false, 20, new int[] { 0, 1, 2, 3, 4, 5, 6 }, null, null) },
            { "hat_f_5", new ItemData("Панама", false, 21, new int[] { 0, 1, 2, 3, 4, 5, 6 }, null, null) },
            { "hat_f_6", new ItemData("Шляпа пляжная", false, 22, new int[] { 0, 1, 2, 3, 4, 5, 6 }, null, null) },
            { "hat_f_7", new ItemData("Наушники", false, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_f_8", new ItemData("Шляпа USA", false, 30, new int[] { 0 }, null, null) },
            { "hat_f_9", new ItemData("Цилиндр USA", false, 31, new int[] { 0 }, null, null) },
            { "hat_f_10", new ItemData("Шапка USA", false, 33, new int[] { 0 }, null, null) },
            { "hat_f_11", new ItemData("Каска болельщицы", false, 36, new int[] { 0, 1, 2, 3, 4, 5 }, null, null) },
            { "hat_f_12", new ItemData("Кепка Snapback", false, 53, new int[] { 0, 1 }, null, null) },
            { "hat_f_13", new ItemData("Кепка Snapback #2", false, 56, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null, null) },
            { "hat_f_14", new ItemData("Кепка дизайнерская", false, 63, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null, null) },
            { "hat_f_15", new ItemData("Кепка SecuroServ", false, 64, new int[] { 0 }, null, null) },
            { "hat_f_16", new ItemData("Бандана байкерская", false, 82, new int[] { 0, 1, 2, 3, 4, 5, 6 }, null, null) },
            { "hat_f_17", new ItemData("Панама с принтами", false, 131, new int[] { 0, 1, 2, 3 }, null, null) },
            { "hat_f_18", new ItemData("Кепка модницы", false, 55, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null, null) },
            { "hat_f_19", new ItemData("Кепка Diamond", false, 134, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, null, null) },
            { "hat_f_20", new ItemData("Шлем с принтами", false, 16, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_f_21", new ItemData("Шлем зеркальный", false, 50, new int[] { 0 }, null, null) },
            { "hat_f_22", new ItemData("Шлем обычный", false, 49, new int[] { 0 }, null, null) },
            { "hat_f_23", new ItemData("Шлем разноцветный", false, 72, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, null, null) },
            { "hat_f_24", new ItemData("Шлем It's Go Time", false, 77, new int[] { 0, 1, 2, 3, 4 }, null, null) },
            { "hat_f_25", new ItemData("Шлем It's Go Time #2", false, 79, new int[] { 0, 1, 2, 3 }, null, null) },
            { "hat_f_26", new ItemData("Шлем модницы", false, 81, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, null, null) },
            { "hat_f_27", new ItemData("Каска байкерская #2", false, 84, new int[] { 0 }, null, null) },
            { "hat_f_28", new ItemData("Каска байкерская #2 (козырек)", false, 85, new int[] { 0 }, null, null) },
            { "hat_f_29", new ItemData("Каска байкерская #2 (ирокез)", false, 86, new int[] { 0 }, null, null) },
            { "hat_f_30", new ItemData("Каска байкерская #2 (шипы)", false, 87, new int[] { 0 }, null, null) },
            { "hat_f_31", new ItemData("Кепка брендовая", false, 75, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, null, null) },
            { "hat_f_32", new ItemData("Кепка брендовая #2", false, 108, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null, null) },
            { "hat_f_33", new ItemData("Кепка брендовая #3", false, 129, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 }, null, null) },
            { "hat_f_34", new ItemData("Кепка цветная", false, 141, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null, null) },
            { "hat_f_35", new ItemData("Панама разноцветная", false, 93, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null, null) },
            { "hat_f_36", new ItemData("Кепка современная", false, 150, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_f_37", new ItemData("Элегантная шляпа", false, 13, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_f_38", new ItemData("Элегантная панама", false, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_f_39", new ItemData("Порк-пай", false, 28, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_f_40", new ItemData("Котелок", false, 26, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, null, null) },
            { "hat_f_41", new ItemData("Цилиндр", false, 27, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, null, null) },
            { "hat_f_42", new ItemData("Трилби", false, 54, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_f_43", new ItemData("Цилиндр USA #2", false, 32, new int[] { 0, 1 }, null, null) },
            { "hat_f_44", new ItemData("Кепка модницы #2", false, 95, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null, null) },
            { "hat_f_45", new ItemData("Хомбург", false, 61, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null, null) },
            { "hat_f_46", new ItemData("Каска байкерская", false, 83, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null, null) },
            { "hat_f_47", new ItemData("Каска байкерская #3", false, 88, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null, null) },
            { "hat_f_48", new ItemData("Каска байкерская (металлик)", false, 89, new int[] { 0 }, null, null) },
            { "hat_f_49", new ItemData("Кепка автолюбителя", false, 153, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, null, null) },
        };

        [JsonIgnore]
        /// <summary>Переключено ли состояние</summary>
        public bool Toggled { get; set; }

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        new public ItemData SexAlternativeData { get => (ItemData)base.SexAlternativeData; }

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

            if (data.ExtraData != null)
            {
                if (Toggled)
                {
                    player.SetAccessories(Slot, data.ExtraData.Drawable, data.Textures[variation]);

                    pData.Hat = $"{data.ExtraData.Drawable}|{data.Textures[variation]}";
                }
                else
                {
                    player.SetAccessories(Slot, data.Drawable, data.Textures[variation]);

                    pData.Hat = $"{data.Drawable}|{data.Textures[variation]}";
                }
            }
            else
            {
                player.SetAccessories(Slot, data.Drawable, data.Textures[variation]);

                pData.Hat = $"{data.Drawable}|{data.Textures[variation]}";
            }
        }

        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            player.ClearAccessory(0);

            pData.Hat = null;
        }

        public void Action(PlayerData pData)
        {
            if (Data.ExtraData == null)
                return;

            Toggled = !Toggled;

            Wear(pData);
        }

        public Hat(string ID, int Variation) : base(ID, IDList[ID], typeof(Hat), Variation)
        {

        }
    }
}
