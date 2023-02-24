using BCRPServer.Game.Data;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BCRPServer.Game.Items
{
    public class Top : Clothes, Clothes.IToggleable
    {
        new public class ItemData : Clothes.ItemData, Clothes.ItemData.IToggleable
        {
            public int BestTorso { get; set; }

            public ExtraData ExtraData { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {BestTorso}, {(ExtraData == null ? "null" : $"new Top.ItemData.ExtraData({ExtraData.Drawable}, {ExtraData.BestTorso})")}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")}";

            public ItemData(string Name, bool Sex, int Drawable, int[] Textures, int BestTorso, ExtraData ExtraData = null, string SexAlternativeID = null) : base(Name, 0.3f, "prop_ld_shirt_01", Sex, Drawable, Textures, SexAlternativeID)
            {
                this.BestTorso = BestTorso;
                this.ExtraData = ExtraData;
            }

            public ItemData(bool Sex, int Drawable, int[] Textures, int BestTorso, ExtraData ExtraData = null, string SexAlternativeID = null) : this(null, Sex, Drawable, Textures, BestTorso, ExtraData, SexAlternativeID) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "top_m_0", new ItemData("Олимпийка", true, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14, null, null) },
            { "top_m_1", new ItemData("Куртка рейсера", true, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14, null, null) },
            { "top_m_3", new ItemData("Худи открытое", true, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14, null, null) },
            { "top_m_4", new ItemData("Куртка рейсера #2", true, 37, new int[] { 0, 1, 2 }, 14, null, null) },
            { "top_m_5", new ItemData("Худи закрытое", true, 57, new int[] { 0 }, 4, null, null) },
            { "top_m_6", new ItemData("Рубашка с узорами", true, 105, new int[] { 0 }, 0, null, null) },
            { "top_m_7", new ItemData("Футболка регби", true, 81, new int[] { 0, 1, 2 }, 0, null, null) },
            { "top_m_8", new ItemData("Поло Н", true, 123, new int[] { 0, 1, 2 }, 0, null, null) },
            { "top_m_9", new ItemData("Футболка хоккейная", true, 128, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 0, null, null) },
            { "top_m_10", new ItemData("Футболка на выпуск", true, 83, new int[] { 0, 1, 2, 3, 4 }, 0, null, null) },
            { "top_m_11", new ItemData("Поло длинное разноцветное", true, 82, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 0, null, null) },
            { "top_m_12", new ItemData("Худи спортивное", true, 84, new int[] { 0, 1, 2, 3, 4, 5 }, 4, null, null) },
            { "top_m_13", new ItemData("Куртка брутальная", true, 61, new int[] { 0, 1, 2, 3 }, 14, null, null) },
            { "top_m_14", new ItemData("Куртка кожаная открытая", true, 62, new int[] { 0 }, 14, null, null) },
            { "top_m_15", new ItemData("Куртка кожаная закрытая", true, 64, new int[] { 0 }, 14, null, null) },
            { "top_m_16", new ItemData("Олимпийка спортивная", true, 141, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 6, null, null) },
            { "top_m_17", new ItemData("Куртка с капюшоном", true, 69, new int[] { 0, 1, 2, 3, 4, 5 }, 14, new ItemData.ExtraData(68, 14), null) },
            { "top_m_18", new ItemData("Бомбер", true, 79, new int[] { 0 }, 6, null, null) },
            { "top_m_19", new ItemData("Поло рабочее", true, 235, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 0, new ItemData.ExtraData(236, 0), null) },
            { "top_m_20", new ItemData("Худи обычное", true, 86, new int[] { 0, 1, 2, 3, 4 }, 4, null, null) },
            { "top_m_22", new ItemData("Бомбер с принтами", true, 88, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14, new ItemData.ExtraData(87, 14), null) },
            { "top_m_23", new ItemData("Куртка стёганая", true, 106, new int[] { 0 }, 14, null, null) },
            { "top_m_24", new ItemData("Рубашка с принтами", true, 234, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 11, null, null) },
            { "top_m_25", new ItemData("Азиатский стиль", true, 107, new int[] { 0, 1, 2, 3, 4 }, 4, null, null) },
            { "top_m_26", new ItemData("Кожаная куртка преследователя", true, 110, new int[] { 0 }, 4, null, null) },
            { "top_m_27", new ItemData("Куртка спортивная", true, 113, new int[] { 0, 1, 2, 3 }, 6, null, null) },
            { "top_m_28", new ItemData("Кимоно", true, 114, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 14, null, null) },
            { "top_m_29", new ItemData("Куртка с узорами", true, 118, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 14, null, null) },
            { "top_m_30", new ItemData("Худи необычное", true, 121, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 4, null, null) },
            { "top_m_31", new ItemData("Куртка обычная", true, 122, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 14, null, null) },
            { "top_m_32", new ItemData("Рубашка гангстера", true, 126, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 4, new ItemData.ExtraData(127, 14), null) },
            { "top_m_33", new ItemData("Куртка SecuroServ", true, 130, new int[] { 0 }, 14, null, null) },
            { "top_m_34", new ItemData("Тишка", true, 164, new int[] { 0, 1, 2 }, 0, null, null) },
            { "top_m_35", new ItemData("Куртка стёганая разноцветная", true, 136, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 14, null, null) },
            { "top_m_36", new ItemData("Жилет вязаный", true, 137, new int[] { 0, 1, 2 }, 15, null, null) },
            { "top_m_37", new ItemData("Бомбер разноцветный", true, 143, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 4, null, null) },
            { "top_m_38", new ItemData("Куртка стритрейсера", true, 147, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 4, null, null) },
            { "top_m_39", new ItemData("Куртка гонщика", true, 148, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 4, null, null) },
            { "top_m_40", new ItemData("Куртка с вырезом", true, 149, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 14, null, null) },
            { "top_m_41", new ItemData("Куртка с принтами", true, 150, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 4, null, null) },
            { "top_m_42", new ItemData("Куртка из старой кожи", true, 151, new int[] { 0, 1, 2, 3, 4, 5 }, 14, null, null) },
            { "top_m_43", new ItemData("Куртка с принтами #2", true, 153, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4, null, null) },
            { "top_m_44", new ItemData("Куртка стритрейсера #2", true, 154, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 12, null, null) },
            { "top_m_45", new ItemData("Байкерская жилетка", true, 157, new int[] { 0, 1, 2, 3 }, 112, null, null) },
            { "top_m_46", new ItemData("Жилетка расстёгнутая", true, 160, new int[] { 0, 1 }, 112, null, null) },
            { "top_m_47", new ItemData("Жилетка кожаная", true, 162, new int[] { 0, 1, 2, 3 }, 114, null, null) },
            { "top_m_48", new ItemData("Куртка чёрная", true, 163, new int[] { 0 }, 14, null, null) },
            { "top_m_49", new ItemData("Куртка кожаная расстёгнутая", true, 166, new int[] { 0, 1, 2, 3, 4, 5 }, 14, null, null) },
            { "top_m_50", new ItemData("Пуховик", true, 167, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14, null, null) },
            { "top_m_51", new ItemData("Куртка джинсовая", true, 169, new int[] { 0, 1, 2, 3 }, 14, null, null) },
            { "top_m_52", new ItemData("Жилет джинсовый", true, 170, new int[] { 0, 1, 2, 3 }, 112, null, null) },
            { "top_m_53", new ItemData("Джинсовка байкерская", true, 172, new int[] { 0, 1, 2, 3 }, 14, null, null) },
            { "top_m_54", new ItemData("Жилетка джинсовая байкерская", true, 173, new int[] { 0, 1, 2, 3 }, 112, null, null) },
            { "top_m_55", new ItemData("Куртка кожаная байкерская", true, 174, new int[] { 0, 1, 2, 3 }, 6, null, null) },
            { "top_m_56", new ItemData("Жилет кожаный байкерский", true, 175, new int[] { 0, 1, 2, 3 }, 114, null, null) },
            { "top_m_57", new ItemData("Ветровка с капюшоном", true, 184, new int[] { 0, 1, 2, 3 }, 6, new ItemData.ExtraData(185, 14), null) },
            { "top_m_58", new ItemData("Ветровка удлиненная", true, 187, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, 6, new ItemData.ExtraData(204, 6), null) },
            { "top_m_59", new ItemData("Худи модника", true, 200, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4, new ItemData.ExtraData(203, 4), null) },
            { "top_m_60", new ItemData("Жилет с капюшоном", true, 205, new int[] { 0, 1, 2, 3, 4 }, 114, new ItemData.ExtraData(202, 114), null) },
            { "top_m_61", new ItemData("Бомбер открытый", true, 230, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14, new ItemData.ExtraData(229, 14), null) },
            { "top_m_62", new ItemData("Куртка с принтами #3", true, 251, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4, new ItemData.ExtraData(253, 4), null) },
            { "top_m_63", new ItemData("Футболка модника", true, 193, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 0, null, null) },
            { "top_m_64", new ItemData("Свитер вязаный", true, 258, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 6, null, null) },
            { "top_m_65", new ItemData("Бомбер модника открытый", true, 261, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14, null, null) },
            { "top_m_66", new ItemData("Спортивная водолазка", true, 255, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4, null, null) },
            { "top_m_67", new ItemData("Куртка модника", true, 257, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, 6, null, null) },
            { "top_m_68", new ItemData("Толстовка модника", true, 259, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 6, null, null) },
            { "top_m_69", new ItemData("Худи модника #2", true, 262, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 4, new ItemData.ExtraData(263, 4), null) },
            { "top_m_70", new ItemData("Куртка с ремнями", true, 264, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 6, null, null) },
            { "top_m_71", new ItemData("Футболка с логотипами", true, 271, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 0, null, null) },
            { "top_m_72", new ItemData("Футболка модника #2", true, 273, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 }, 0, null, null) },
            { "top_m_73", new ItemData("Худи с принтами", true, 279, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, 4, new ItemData.ExtraData(280, 4), null) },
            { "top_m_74", new ItemData("Толстовка с логотипами", true, 281, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, 6, null, null) },
            { "top_m_75", new ItemData("Футболка удлиненная", true, 282, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 0, null, null) },
            { "top_m_76", new ItemData("Ветровка с принтами #2", true, 296, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4, new ItemData.ExtraData(297, 4), null) },
            { "top_m_77", new ItemData("Толстовка с принтами", true, 308, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 4, null, null) },
            { "top_m_78", new ItemData("Футболка с картинками", true, 313, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 0, null, null) },
            { "top_m_79", new ItemData("Футболка с рисунками", true, 325, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, 0, null, null) },
            { "top_m_80", new ItemData("Футболка с рисунками #2", true, 323, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 0, null, null) },
            { "top_m_81", new ItemData("Свитер боевой", true, 50, new int[] { 0, 1, 2, 3, 4 }, 4, null, null) },
            { "top_m_82", new ItemData("Пиджак мятый", true, 59, new int[] { 0, 1, 2, 3 }, 14, null, null) },
            { "top_m_83", new ItemData("Водолазка мятая", true, 67, new int[] { 0, 1, 2, 3 }, 4, null, null) },
            { "top_m_84", new ItemData("Ветровка осенняя", true, 85, new int[] { 0 }, 1, null, null) },
            { "top_m_85", new ItemData("Свитшот разноцветный", true, 89, new int[] { 0, 1, 2, 3 }, 6, null, null) },
            { "top_m_86", new ItemData("Жилетка вязаная", true, 109, new int[] { 0 }, 15, null, null) },
            { "top_m_87", new ItemData("Водолазка разноцветная", true, 111, new int[] { 0, 1, 2, 3, 4, 5 }, 4, null, null) },
            { "top_m_88", new ItemData("Куртка с капюшоном", true, 124, new int[] { 0 }, 14, null, null) },
            { "top_m_89", new ItemData("Куртка закрытая с воротником", true, 125, new int[] { 0 }, 4, null, null) },
            { "top_m_90", new ItemData("Поло Liberty", true, 131, new int[] { 0 }, 0, new ItemData.ExtraData(132, 0), null) },
            { "top_m_92", new ItemData("Худи Liberty", true, 134, new int[] { 0, 1, 2 }, 4, null, null) },
            { "top_m_93", new ItemData("Рубашка с принтами", true, 135, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 0, null, null) },
            { "top_m_94", new ItemData("Пальто кожаное на ремнях", true, 138, new int[] { 0, 1, 2 }, 4, null, null) },
            { "top_m_97", new ItemData("Куртка танцора", true, 155, new int[] { 0, 1, 2, 3 }, 14, null, null) },
            { "top_m_98", new ItemData("Жилет с ремнями", true, 158, new int[] { 0, 1, 2 }, 113, null, null) },
            { "top_m_99", new ItemData("Жилет кожаный", true, 159, new int[] { 0, 1 }, 114, null, null) },
            { "top_m_100", new ItemData("Толстовка гонщика", true, 165, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 6, null, null) },
            { "top_m_101", new ItemData("Толстовка уличная", true, 168, new int[] { 0, 1, 2 }, 12, null, null) },
            { "top_m_102", new ItemData("Худи уличное", true, 171, new int[] { 0, 1 }, 4, null, null) },
            { "top_m_103", new ItemData("Жилет STFU", true, 176, new int[] { 0 }, 114, null, null) },
            { "top_m_104", new ItemData("Жилет гонщика", true, 177, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 2, null, null) },
            { "top_m_105", new ItemData("Куртка кожаная с застёжками #2", true, 181, new int[] { 0, 1, 2, 3, 4, 5 }, 14, null, null) },
            { "top_m_106", new ItemData("Парка разноцветная открытая", true, 189, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 14, new ItemData.ExtraData(188, 14), null) },
            { "top_m_107", new ItemData("Толстовка модника #3", true, 190, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 6, null, null) },
            { "top_m_108", new ItemData("Жилет стёганый", true, 223, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 2, null, null) },
            { "top_m_109", new ItemData("Куртка стёганая #2", true, 224, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 12, null, null) },
            { "top_m_110", new ItemData("Толстовка Class Of", true, 225, new int[] { 0, 1 }, 8, null, null) },
            { "top_m_112", new ItemData("Майка разноцветная #2", true, 237, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 5, null, null) },
            { "top_m_113", new ItemData("Футболка без рукавов", true, 238, new int[] { 0, 1, 2, 3, 4, 5 }, 2, null, null) },
            { "top_m_114", new ItemData("Поло разноцветное", true, 241, new int[] { 0, 1, 2, 3, 4, 5 }, 0, new ItemData.ExtraData(242, 0), null) },
            { "top_m_116", new ItemData("Куртка с фиолетовыми чертами", true, 329, new int[] { 0 }, 4, null, null) },
            { "top_m_117", new ItemData("Толстовка с фиолетовыми чертами", true, 330, new int[] { 0 }, 4, new ItemData.ExtraData(331, 4), null) },
            { "top_m_118", new ItemData("Спортивная толстовка", true, 332, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4, null, null) },
            { "top_m_119", new ItemData("Фиолетовая удлиненная футболка", true, 334, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 0, null, null) },
            { "top_m_120", new ItemData("Толстовка Baseball", true, 335, new int[] { 0, 1, 2, 3, 4, 5 }, 8, null, null) },
            { "top_m_121", new ItemData("Рубашка гангстера расстегнутая", true, 340, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, 14, new ItemData.ExtraData(341, 1), null) },
            { "top_m_122", new ItemData("Толстовка с желтыми принтами", true, 342, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 4, null, null) },
            { "top_m_123", new ItemData("Бомбер с желтыми принтами", true, 344, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 14, new ItemData.ExtraData(343, 14), null) },
            { "top_m_124", new ItemData("Футболка свободная Bigness", true, 350, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 0, null, null) },
            { "top_m_126", new ItemData("Ветровка с капюшоном цветная", true, 217, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 6, null, null) },
            { "top_m_127", new ItemData("Джинсовая ветровка", true, 233, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 14, new ItemData.ExtraData(232, 14), null) },
            { "top_m_128", new ItemData("Футболка лёгкая", true, 351, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 0, null, null) },
            { "top_m_129", new ItemData("Худи лёгкое", true, 352, new int[] { 0, 1, 2 }, 4, new ItemData.ExtraData(353, 4), null) },
            { "top_m_130", new ItemData("Майка баскетбольная", true, 357, new int[] { 0, 1 }, 2, null, null) },
            { "top_m_132", new ItemData("Поло гольфиста", true, 382, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 0, new ItemData.ExtraData(383, 0), null) },
            { "top_m_133", new ItemData("Худи свободное", true, 384, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4, new ItemData.ExtraData(385, 4), null) },
            { "top_m_134", new ItemData("Пиджак Блэйзер", true, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14, new ItemData.ExtraData(10, 14), null) },
            { "top_m_135", new ItemData("Пиджак двубортный праздничный", true, 119, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14, null, null) },
            { "top_m_136", new ItemData("Жилет с цепочкой", true, 21, new int[] { 0, 1, 2, 3 }, 15, null, null) },
            { "top_m_137", new ItemData("Жилет обычный", true, 25, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 15, null, null) },
            { "top_m_138", new ItemData("Пиджак Блэйзер с принтами", true, 24, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, 14, null, null) },
            { "top_m_139", new ItemData("Пиджак двубортный", true, 27, new int[] { 0, 1, 2 }, 14, null, null) },
            { "top_m_140", new ItemData("Пиджак однобортный", true, 28, new int[] { 0, 1, 2 }, 14, null, null) },
            { "top_m_141", new ItemData("Пиджак модника", true, 35, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 14, null, null) },
            { "top_m_142", new ItemData("Жилет USA", true, 45, new int[] { 0, 1, 2 }, 15, null, null) },
            { "top_m_143", new ItemData("Фрак USA", true, 46, new int[] { 0, 1, 2 }, 14, null, null) },
            { "top_m_144", new ItemData("Фрак", true, 58, new int[] { 0 }, 14, null, null) },
            { "top_m_145", new ItemData("Дубленка с мехом", true, 70, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14, null, null) },
            { "top_m_146", new ItemData("Пальто Редингот", true, 72, new int[] { 0, 1, 2, 3 }, 14, null, null) },
            { "top_m_147", new ItemData("Куртка модника открытая", true, 74, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 14, new ItemData.ExtraData(75, 14), null) },
            { "top_m_148", new ItemData("Пальто Бушлат", true, 76, new int[] { 0, 1, 2, 3, 4 }, 14, null, null) },
            { "top_m_149", new ItemData("Пальто Кромби удлиненное", true, 77, new int[] { 0, 1, 2, 3 }, 14, null, null) },
            { "top_m_150", new ItemData("Пиджак для встреч открытый", true, 99, new int[] { 0, 1, 2, 3, 4 }, 14, new ItemData.ExtraData(100, 14), null) },
            { "top_m_151", new ItemData("Пиджак хозяина", true, 108, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 14, null, null) },
            { "top_m_152", new ItemData("Пальто строгое", true, 142, new int[] { 0, 1, 2 }, 14, null, null) },
            { "top_m_153", new ItemData("Пиджак строгий", true, 140, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 14, null, null) },
            { "top_m_154", new ItemData("Пиджак хозяина #2", true, 145, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 14, null, null) },
            { "top_m_155", new ItemData("Пиджак праздничный", true, 183, new int[] { 0, 1, 2, 3, 4, 5 }, 14, null, null) },
            { "top_m_156", new ItemData("Пуховик модника", true, 191, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 14, null, null) },
            { "top_m_157", new ItemData("Пальто строгое разноцветное", true, 192, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14, null, null) },
            { "top_m_158", new ItemData("Пуховик модника #2", true, 269, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14, null, null) },
            { "top_m_159", new ItemData("Бомбер модника #2", true, 266, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, 14, new ItemData.ExtraData(265, 14), null) },
            { "top_m_160", new ItemData("Рубашка модника", true, 260, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 11, null, null) },
            { "top_m_161", new ItemData("Куртка спортивная с принтами", true, 298, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 }, 4, null, null) },
            { "top_m_162", new ItemData("Рубашка на выпуск с принтами", true, 299, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 0, null, null) },
            { "top_m_163", new ItemData("Парка модника", true, 303, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 14, new ItemData.ExtraData(300, 6), null) },
            { "top_m_164", new ItemData("Парка модника с капюшоном", true, 301, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 14, new ItemData.ExtraData(302, 14), null) },
            { "top_m_165", new ItemData("Куртка на меху", true, 304, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 14, null, null) },
            { "top_m_166", new ItemData("Худи Diamond", true, 305, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4, new ItemData.ExtraData(306, 4), null) },
            { "top_m_167", new ItemData("Толстовка модника #2", true, 307, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, 6, null, null) },
            { "top_m_168", new ItemData("Пуховик модника #3", true, 309, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 14, null, null) },
            { "top_m_170", new ItemData("Жилет обычный #2", true, 11, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 15, null, null) },
            { "top_m_171", new ItemData("Пиджак жениха", true, 20, new int[] { 0, 1, 2, 3 }, 14, null, null) },
            { "top_m_172", new ItemData("Пиджак Блейзер #2 открытый", true, 23, new int[] { 0, 1, 2, 3 }, 14, null, null) },
            { "top_m_173", new ItemData("Жилет праздничный", true, 40, new int[] { 0, 1 }, 15, null, null) },
            { "top_m_174", new ItemData("Пиджак с узорами", true, 103, new int[] { 0 }, 14, null, null) },
            { "top_m_175", new ItemData("Пальто закрытое", true, 112, new int[] { 0 }, 14, null, null) },
            { "top_m_176", new ItemData("Пальто серое", true, 115, new int[] { 0 }, 14, null, null) },
            { "top_m_177", new ItemData("Жилет с принтами", true, 120, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 15, null, null) },
            { "top_m_178", new ItemData("Куртка с застежками", true, 161, new int[] { 0, 1, 2, 3 }, 6, null, null) },
            { "top_m_179", new ItemData("Куртка с мехом", true, 240, new int[] { 0, 1, 2, 3, 4, 5 }, 14, null, null) },
            { "top_m_180", new ItemData("Кимоно с принтами", true, 310, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 14, null, null) },
            { "top_m_181", new ItemData("Кожаная разноцветная куртка", true, 338, new int[] { 0, 1, 2, 3, 4, 5 }, 14, null, null) },
            { "top_m_182", new ItemData("Рубашка солидная с принтами", true, 348, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }, 1, new ItemData.ExtraData(349, 1), null) },
            { "top_m_183", new ItemData("Свитшот модника", true, 78, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 6, null, null) },
            { "top_m_184", new ItemData("Рубашка гавайская", true, 355, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 184, new ItemData.ExtraData(354, 184), null) },
            { "top_m_185", new ItemData("Свитшот тусовщика", true, 358, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 6, null, null) },
            { "top_m_186", new ItemData("Бомбер с тигром", true, 360, new int[] { 0 }, 14, new ItemData.ExtraData(359, 14), null) },
            { "top_m_187", new ItemData("Бомбер Cayo Perico", true, 361, new int[] { 0 }, 4, null, null) },
            { "top_m_188", new ItemData("Жилет на молнии брендированный", true, 369, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 2, null, null) },
            { "top_m_189", new ItemData("Куртка дутая брендированная", true, 370, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 12, null, null) },
            { "top_m_190", new ItemData("Куртка автолюбителя", true, 371, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 4, null, null) },
            { "top_m_191", new ItemData("Худи автолюбителя", true, 374, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 4, new ItemData.ExtraData(373, 4), null) },
            { "top_m_192", new ItemData("Регби брендированная", true, 376, new int[] { 0, 1, 2 }, 14, new ItemData.ExtraData(375, 14), null) },
            { "top_m_193", new ItemData("Футболка автолюбителя", true, 377, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 0, null, null) },
            { "top_m_194", new ItemData("Ветровка автолюбителя", true, 378, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 12, null, null) },
            { "top_m_195", new ItemData("Кожанка глянцевая", true, 381, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14, new ItemData.ExtraData(379, 14), null) },
            { "top_m_196", new ItemData("Кожанка на молнии", true, 387, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 14, new ItemData.ExtraData(386, 14), null) },
            { "top_m_197", new ItemData("Куртка Broker", true, 390, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 14, new ItemData.ExtraData(388, 14), null) },
            { "top_m_198", new ItemData("Куртка Sweatbox", true, 391, new int[] { 0, 1, 2 }, 14, new ItemData.ExtraData(389, 14), null) },
            { "top_m_199", new ItemData("null", true, 29, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 14, new ItemData.ExtraData(30, 14), null) },
            { "top_m_200", new ItemData("null", true, 31, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 14, new ItemData.ExtraData(32, 14), null) },
            { "top_f_1", new ItemData("Бейсбольная рубашка", false, 161, new int[] { 0, 1, 2 }, 9, null, null) },
            { "top_f_2", new ItemData("Майка обычная", false, 16, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 4, null, null) },
            { "top_f_3", new ItemData("Джинсовка", false, 1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5, null, null) },
            { "top_f_4", new ItemData("Косуха", false, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5, null, null) },
            { "top_f_5", new ItemData("Куртка спортивная", false, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 7, null, null) },
            { "top_f_6", new ItemData("Поло экзотическое", false, 17, new int[] { 0 }, 9, null, null) },
            { "top_f_8", new ItemData("Джинсовка с рукавами", false, 31, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 5, null, null) },
            { "top_f_10", new ItemData("Топик с принтами", false, 33, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, 4, null, null) },
            { "top_f_11", new ItemData("Летний сарафан", false, 37, new int[] { 0, 1, 2, 3, 4, 5 }, 4, null, null) },
            { "top_f_12", new ItemData("Свитер боевой", false, 43, new int[] { 0, 1, 2, 3, 4 }, 3, null, null) },
            { "top_f_13", new ItemData("Кофта с капюшоном", false, 50, new int[] { 0 }, 3, null, null) },
            { "top_f_14", new ItemData("Куртка брутальная", false, 54, new int[] { 0, 1, 2, 3 }, 3, null, null) },
            { "top_f_15", new ItemData("Куртка кожаная", false, 55, new int[] { 0 }, 3, null, null) },
            { "top_f_16", new ItemData("Куртка с капюшоном", false, 63, new int[] { 0, 1, 2, 3, 4, 5 }, 3, null, null) },
            { "top_f_17", new ItemData("Куртка кожаная с ремнем", false, 69, new int[] { 0 }, 5, null, null) },
            { "top_f_18", new ItemData("Кофта модницы", false, 71, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 1, null, null) },
            { "top_f_19", new ItemData("Бомбер с волком", false, 72, new int[] { 0 }, 1, null, null) },
            { "top_f_20", new ItemData("Рубашка на выпуск", false, 76, new int[] { 0, 1, 2, 3, 4 }, 9, null, null) },
            { "top_f_21", new ItemData("Худи обычное", false, 78, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 3, null, null) },
            { "top_f_22", new ItemData("Кофта обычная", false, 79, new int[] { 0, 1, 2, 3 }, 1, null, null) },
            { "top_f_23", new ItemData("Бомбер с принтами", false, 81, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 3, null, null) },
            { "top_f_24", new ItemData("Поло обычное #2", false, 86, new int[] { 0, 1, 2 }, 9, null, null) },
            { "top_f_25", new ItemData("Поло с узорами", false, 96, new int[] { 0 }, 9, null, null) },
            { "top_f_26", new ItemData("Куртка стеганая", false, 97, new int[] { 0 }, 6, null, null) },
            { "top_f_27", new ItemData("Азиатский стиль", false, 98, new int[] { 0, 1, 2, 3, 4 }, 3, null, null) },
            { "top_f_28", new ItemData("Рубашка гангстера", false, 121, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, 3, null, null) },
            { "top_f_29", new ItemData("Жилет вязаный", false, 100, new int[] { 0 }, 6, null, null) },
            { "top_f_30", new ItemData("Кимоно", false, 105, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 0, null, null) },
            { "top_f_31", new ItemData("Куртка спортивная", false, 106, new int[] { 0, 1, 2, 3 }, 6, null, null) },
            { "top_f_32", new ItemData("Рубашка с принтами", false, 109, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 0, null, null) },
            { "top_f_33", new ItemData("Куртка рейсера", false, 110, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 3, null, null) },
            { "top_f_34", new ItemData("Платье с узорами", false, 116, new int[] { 0, 1, 2 }, 11, null, null) },
            { "top_f_35", new ItemData("Поло H", false, 119, new int[] { 0, 1, 2 }, 14, null, null) },
            { "top_f_36", new ItemData("Футболка H", false, 125, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 14, null, null) },
            { "top_f_37", new ItemData("Куртка SecuroServ закрытая", false, 127, new int[] { 0 }, 3, null, null) },
            { "top_f_38", new ItemData("Рубашка с принтами #2", false, 132, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 9, null, null) },
            { "top_f_39", new ItemData("Куртка стеганая разноцветная", false, 133, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 6, null, null) },
            { "top_f_40", new ItemData("Олимпийка", false, 138, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 6, null, null) },
            { "top_f_41", new ItemData("Бомбер разноцветный", false, 140, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 3, null, null) },
            { "top_f_42", new ItemData("Куртка стритрейсера", false, 144, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 3, null, null) },
            { "top_f_43", new ItemData("Худи необычное", false, 123, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 3, null, null) },
            { "top_f_44", new ItemData("Куртка гонщика", false, 145, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 3, null, null) },
            { "top_f_45", new ItemData("Куртка с вырезом", false, 146, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 7, null, null) },
            { "top_f_46", new ItemData("Куртка с принтами", false, 147, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 3, null, null) },
            { "top_f_47", new ItemData("Куртка с принтами #2", false, 150, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3, null, null) },
            { "top_f_48", new ItemData("Куртка стритрейсера #2", false, 151, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 1, null, null) },
            { "top_f_49", new ItemData("Байкерская жилетка", false, 154, new int[] { 0, 1, 2, 3 }, 129, null, null) },
            { "top_f_50", new ItemData("Жилетка расстегнутая", false, 157, new int[] { 0, 1 }, 132, null, null) },
            { "top_f_51", new ItemData("Жилетка кожаная", false, 159, new int[] { 0, 1, 2, 3 }, 131, null, null) },
            { "top_f_52", new ItemData("Куртка черная", false, 160, new int[] { 0 }, 5, null, null) },
            { "top_f_53", new ItemData("Куртка кожаная расстегнутая", false, 163, new int[] { 0, 1, 2, 3, 4, 5 }, 5, null, null) },
            { "top_f_54", new ItemData("Пуховик", false, 164, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5, null, null) },
            { "top_f_55", new ItemData("Куртка джинсовая", false, 166, new int[] { 0, 1, 2, 3 }, 5, null, null) },
            { "top_f_56", new ItemData("Жилет джинсовый", false, 167, new int[] { 0, 1, 2, 3 }, 129, null, null) },
            { "top_f_57", new ItemData("Джинсовый топ", false, 171, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 153, null, null) },
            { "top_f_58", new ItemData("Джинсовка байкерская", false, 174, new int[] { 0, 1, 2, 3 }, 5, null, null) },
            { "top_f_59", new ItemData("Жилетка джинсовая байкерская", false, 175, new int[] { 0, 1, 2, 3 }, 129, null, null) },
            { "top_f_60", new ItemData("Куртка с застежками", false, 158, new int[] { 0, 1, 2, 3 }, 7, null, null) },
            { "top_f_61", new ItemData("Куртка кожаная байкерская", false, 176, new int[] { 0, 1, 2, 3 }, 7, null, null) },
            { "top_f_62", new ItemData("Жилет кожаный байкерский", false, 177, new int[] { 0, 1, 2, 3 }, 131, null, null) },
            { "top_f_63", new ItemData("Ветровка с капюшоном", false, 186, new int[] { 0, 1, 2, 3 }, 3, null, null) },
            { "top_f_64", new ItemData("Кофта с принтами", false, 192, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 1, null, null) },
            { "top_f_65", new ItemData("Топик модницы", false, 195, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 153, null, null) },
            { "top_f_66", new ItemData("Худи модницы", false, 202, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3, null, null) },
            { "top_f_68", new ItemData("Жилет стеганый с принтами", false, 233, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 11, null, null) },
            { "top_f_69", new ItemData("Куртка стегеная с принтами", false, 234, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 6, null, null) },
            { "top_f_70", new ItemData("Куртка с принтами #3", false, 251, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3, null, null) },
            { "top_f_71", new ItemData("Бомбер открытый", false, 239, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 3, null, null) },
            { "top_f_72", new ItemData("Спортивная ветровка", false, 259, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3, null, null) },
            { "top_f_73", new ItemData("Рейсерский стиль", false, 262, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 7, null, null) },
            { "top_f_74", new ItemData("Бомбер модницы", false, 270, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5, null, null) },
            { "top_f_75", new ItemData("Свитер вязаный", false, 267, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 1, null, null) },
            { "top_f_76", new ItemData("Куртка модницы", false, 266, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, 6, null, null) },
            { "top_f_77", new ItemData("Толстовка модницы", false, 268, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 1, null, null) },
            { "top_f_78", new ItemData("Худи модницы #2", false, 271, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 3, null, null) },
            { "top_f_79", new ItemData("Футболка с логотипами", false, 280, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 14, null, null) },
            { "top_f_80", new ItemData("Футболка модницы #2", false, 286, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 }, 14, null, null) },
            { "top_f_81", new ItemData("Худи с принтами", false, 292, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, 3, null, null) },
            { "top_f_82", new ItemData("Толстовка с логотипами", false, 294, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, 1, null, null) },
            { "top_f_83", new ItemData("Футболка удлиненная", false, 295, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14, null, null) },
            { "top_f_84", new ItemData("Толстовка с принтами", false, 319, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 3, null, null) },
            { "top_f_85", new ItemData("Кимоно с принтами", false, 321, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 0, null, null) },
            { "top_f_86", new ItemData("Футболка с картинками", false, 324, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 14, null, null) },
            { "top_f_87", new ItemData("Футболка с картинками #2", false, 338, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14, null, null) },
            { "top_f_88", new ItemData("Футболка с рисунками", false, 337, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, 14, null, null) },
            { "top_f_89", new ItemData("Футболка с рисунками #2", false, 335, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 14, null, null) },
            { "top_f_90", new ItemData("Куртка на ремнях", false, 273, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5, null, null) },
            { "top_f_91", new ItemData("Топик с принтами", false, 284, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 15, null, null) },
            { "top_f_92", new ItemData("Спортивная водолазка", false, 264, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3, null, null) },
            { "top_f_93", new ItemData("Платье с бахромой", false, 21, new int[] { 0, 1, 2, 3, 4, 5 }, 16, null, null) },
            { "top_f_94", new ItemData("Топик обычный", false, 74, new int[] { 0, 1, 2 }, 15, null, null) },
            { "top_f_95", new ItemData("Куртка весенняя", false, 77, new int[] { 0 }, 6, null, null) },
            { "top_f_97", new ItemData("Платье с узорами #2", false, 112, new int[] { 0, 1, 2 }, 11, null, null) },
            { "top_f_98", new ItemData("Платье с узорами #3", false, 113, new int[] { 0, 1, 2 }, 11, null, null) },
            { "top_f_99", new ItemData("Платье с узорами #4", false, 114, new int[] { 0, 1, 2 }, 11, null, null) },
            { "top_f_100", new ItemData("Футболка хоккейная", false, 126, new int[] { 0, 1, 2 }, 14, null, null) },
            { "top_f_101", new ItemData("Худи Liberty", false, 131, new int[] { 0, 1, 2 }, 3, null, null) },
            { "top_f_102", new ItemData("Футболка Libery на выпуск", false, 128, new int[] { 0 }, 14, null, null) },
            { "top_f_103", new ItemData("Джинсовка", false, 1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5, null, null) },
            { "top_f_104", new ItemData("Куртка JackCandy", false, 148, new int[] { 0, 1, 2, 3, 4, 5 }, 3, null, null) },
            { "top_f_105", new ItemData("Жилет гонщицы", false, 155, new int[] { 0, 1, 2 }, 130, null, null) },
            { "top_f_106", new ItemData("Жилет кожаный", false, 156, new int[] { 0, 1 }, 131, null, null) },
            { "top_f_107", new ItemData("Куртка кожаная с капюшоном", false, 165, new int[] { 0, 1, 2 }, 0, null, null) },
            { "top_f_108", new ItemData("Майка порезанная", false, 168, new int[] { 0, 1, 2, 3, 4, 5 }, 161, null, null) },
            { "top_f_109", new ItemData("Топик порезанный", false, 169, new int[] { 0, 1, 2, 3, 4, 5 }, 153, null, null) },
            { "top_f_110", new ItemData("Майка порезанная #2", false, 170, new int[] { 0, 1, 2, 3, 4, 5 }, 15, null, null) },
            { "top_f_111", new ItemData("Худи обычное", false, 172, new int[] { 0, 1 }, 3, null, null) },
            { "top_f_112", new ItemData("Жилет STFU", false, 178, new int[] { 0 }, 131, null, null) },
            { "top_f_113", new ItemData("Ветровка разноцветная", false, 190, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 3, null, null) },
            { "top_f_114", new ItemData("Ветровка удлиненная", false, 189, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, 7, null, null) },
            { "top_f_115", new ItemData("Худи без рукавов", false, 207, new int[] { 0, 1, 2, 3, 4 }, 131, null, null) },
            { "top_f_117", new ItemData("Ветровка разноцветная с капюшоном", false, 227, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 3, null, null) },
            { "top_f_118", new ItemData("Блузка Class Of", false, 235, new int[] { 0, 1 }, 9, null, null) },
            { "top_f_119", new ItemData("Поло с принтами", false, 244, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 9, null, null) },
            { "top_f_120", new ItemData("Поло с брендами", false, 246, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14, null, null) },
            { "top_f_121", new ItemData("Майка разноцветная", false, 247, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4, null, null) },
            { "top_f_122", new ItemData("Поло обычное", false, 249, new int[] { 0, 1, 2, 3, 4, 5 }, 14, null, null) },
            { "top_f_123", new ItemData("Спортивная толстовка", false, 347, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3, null, null) },
            { "top_f_124", new ItemData("Фиолетовая удлиненная футболка", false, 349, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 14, null, null) },
            { "top_f_125", new ItemData("Толстовка Base Ball", false, 350, new int[] { 0, 1, 2, 3, 4, 5 }, 9, null, null) },
            { "top_f_126", new ItemData("Рубашка гангстера #2", false, 354, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 }, 5, null, null) },
            { "top_f_127", new ItemData("Рубашка гангстера #3", false, 356, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 3, null, null) },
            { "top_f_128", new ItemData("Толстовка с желтыми принтами", false, 361, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 3, null, null) },
            { "top_f_129", new ItemData("Бомбер с желтыми принтами", false, 363, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 5, null, null) },
            { "top_f_130", new ItemData("Бомбер с фиолетовыми чертами", false, 344, new int[] { 0 }, 3, null, null) },
            { "top_f_131", new ItemData("Толстовка с фиолетовыми чертами", false, 345, new int[] { 0 }, 3, null, null) },
            { "top_f_132", new ItemData("Поло фиолетовое Bigness", false, 368, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 14, null, null) },
            { "top_f_133", new ItemData("Футболка модницы #3", false, 369, new int[] { 0, 1, 2, 3, 4 }, 14, null, null) },
            { "top_f_134", new ItemData("Худи легкое", false, 370, new int[] { 0, 1, 2 }, 3, null, null) },
            { "top_f_135", new ItemData("Футболка легкая", false, 377, new int[] { 0, 1, 2, 3, 4, 5 }, 14, null, null) },
            { "top_f_137", new ItemData("Поло гольфиста", false, 400, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 14, null, null) },
            { "top_f_138", new ItemData("Худи свободное", false, 407, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3, null, null) },
            { "top_f_139", new ItemData("Джемпер с V вырезом", false, 406, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 7, null, null) },
            { "top_f_140", new ItemData("Жакет закрытый", false, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5, null, null) },
            { "top_f_141", new ItemData("Пиджак строгий", false, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 6, null, null) },
            { "top_f_142", new ItemData("Пиджак с принтами", false, 25, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 6, null, null) },
            { "top_f_143", new ItemData("Лифчик с принтами", false, 18, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 15, null, null) },
            { "top_f_144", new ItemData("Лифчик модницы", false, 279, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 15, null, null) },
            { "top_f_145", new ItemData("Жакет модницы", false, 24, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 5, null, null) },
            { "top_f_146", new ItemData("Жилет обычный", false, 28, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 0, null, null) },
            { "top_f_147", new ItemData("Жакет из кожи", false, 35, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 5, null, null) },
            { "top_f_148", new ItemData("Фрак USA", false, 39, new int[] { 0 }, 5, null, null) },
            { "top_f_149", new ItemData("Фрак", false, 51, new int[] { 0 }, 3, null, null) },
            { "top_f_150", new ItemData("Пиджак Блэйзер", false, 52, new int[] { 0, 1, 2, 3 }, 3, null, null) },
            { "top_f_151", new ItemData("Пальто шинель", false, 64, new int[] { 0, 1, 2, 3, 4 }, 5, null, null) },
            { "top_f_152", new ItemData("Куртка с мехом", false, 65, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 5, null, null) },
            { "top_f_153", new ItemData("Пиджак на поясе", false, 66, new int[] { 0, 1, 2, 3 }, 5, null, null) },
            { "top_f_154", new ItemData("Пальто тренчкот", false, 70, new int[] { 0, 1, 2, 3, 4 }, 5, null, null) },
            { "top_f_155", new ItemData("Пиджак приталенный закрытый", false, 58, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, 3, null, null) },
            { "top_f_156", new ItemData("Рубашка модницы", false, 83, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 0, null, null) },
            { "top_f_157", new ItemData("Пиджак строгий #2", false, 90, new int[] { 0, 1, 2, 3, 4 }, 3, null, null) },
            { "top_f_158", new ItemData("Пиджак хозяйки", false, 99, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 7, null, null) },
            { "top_f_159", new ItemData("Кожаный топ", false, 173, new int[] { 0 }, 4, null, null) },
            { "top_f_160", new ItemData("Лифчик модницы #2", false, 101, new int[] { 0, 1, 2, 3, 4, 5 }, 15, null, null) },
            { "top_f_161", new ItemData("Пиджак праздничный", false, 185, new int[] { 0, 1, 2, 3, 4, 5 }, 3, null, null) },
            { "top_f_162", new ItemData("Пуховик модницы", false, 193, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 5, null, null) },
            { "top_f_163", new ItemData("Пальто строгое расстегнутое", false, 194, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 6, null, null) },
            { "top_f_164", new ItemData("Поло расстегнутое", false, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 9, null, null) },
            { "top_f_165", new ItemData("Пиджак в камуфляжной расцветке", false, 34, new int[] { 0 }, 6, null, null) },
            { "top_f_166", new ItemData("Пиджак Блэйзер", false, 52, new int[] { 0, 1, 2, 3 }, 3, null, null) },
            { "top_f_167", new ItemData("Пиджак строгий #2", false, 57, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, 3, null, null) },
            { "top_f_168", new ItemData("Куртка кожаная коричневая", false, 102, new int[] { 0 }, 3, null, null) },
            { "top_f_169", new ItemData("Пиджак на заклепках", false, 137, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 7, null, null) },
            { "top_f_170", new ItemData("Пальто удлиненное", false, 139, new int[] { 0, 1, 2 }, 6, null, null) },
            { "top_f_171", new ItemData("Пижама с принтами", false, 142, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 0, null, null) },
            { "top_f_172", new ItemData("Пижама хозяйки", false, 143, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 7, null, null) },
            { "top_f_173", new ItemData("Куртка с мехом", false, 65, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 5, null, null) },
            { "top_f_174", new ItemData("Пиджак солидный (с платком)", false, 305, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 3, null, null) },
            { "top_f_175", new ItemData("Ветровка Bigness", false, 307, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3, null, null) },
            { "top_f_176", new ItemData("Толстовка с принтами #2", false, 318, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, 1, null, null) },
            { "top_f_177", new ItemData("Кожаная разноцветная куртка", false, 353, new int[] { 0, 1, 2, 3, 4, 5 }, 3, null, null) },
            { "top_f_178", new ItemData("Рубашка солидная с принтами", false, 366, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }, 3, null, null) },
            { "top_f_179", new ItemData("Пуховик модницы #2", false, 278, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5, null, null) },
            { "top_f_180", new ItemData("Бомбер модницы #2", false, 274, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, 3, null, null) },
            { "top_f_181", new ItemData("Рубашка модницы", false, 269, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 9, null, null) },
            { "top_f_182", new ItemData("Куртка спортивная с принтами", false, 309, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 }, 3, null, null) },
            { "top_f_183", new ItemData("Рубашка на выпуск с принтами", false, 310, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 9, null, null) },
            { "top_f_184", new ItemData("Парка модницы", false, 311, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3, null, null) },
            { "top_f_185", new ItemData("Парка модницы #2", false, 314, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 5, null, null) },
            { "top_f_186", new ItemData("Куртка на меху", false, 315, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 5, null, null) },
            { "top_f_187", new ItemData("Худи Diamond", false, 316, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3, null, null) },
            { "top_f_188", new ItemData("Пуховик модницы #3", false, 320, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 5, null, null) },
            { "top_f_189", new ItemData("Рубашка с рукавами", false, 332, new int[] { 0 }, 3, null, null) },
            { "top_f_191", new ItemData("Платье модницы", false, 322, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 11, null, null) },
            { "top_f_192", new ItemData("Платье модницы #2", false, 323, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 11, null, null) },
            { "top_f_193", new ItemData("Пиджак разноцветный", false, 339, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 3, null, null) },
            { "top_f_194", new ItemData("Рубашка гавайская", false, 373, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 229, null, null) },
            { "top_f_195", new ItemData("Худи тусовщицы", false, 376, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 1, null, null) },
            { "top_f_196", new ItemData("Бомбер с тигром", false, 379, new int[] { 0 }, 5, null, null) },
            { "top_f_197", new ItemData("Бомбер Cayo Perico", false, 380, new int[] { 0 }, 3, null, null) },
            { "top_f_198", new ItemData("Жилет на молнии брендированный", false, 388, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 11, null, null) },
            { "top_f_199", new ItemData("Куртка дутая брендированная", false, 389, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 6, null, null) },
            { "top_f_200", new ItemData("Куртка автолюбительницы", false, 390, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 3, null, null) },
            { "top_f_201", new ItemData("Худи автолюбительнцы", false, 392, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 3, null, null) },
            { "top_f_202", new ItemData("Регби брендированная", false, 394, new int[] { 0, 1, 2 }, 3, null, null) },
            { "top_f_203", new ItemData("Футболка автолюбительнцы", false, 395, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 14, null, null) },
            { "top_f_204", new ItemData("Ветровка автолюбительницы", false, 396, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 1, null, null) },
            { "top_f_205", new ItemData("Кожанка глянцевая", false, 399, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 5, null, null) },
            { "top_f_206", new ItemData("Кожанка на молнии", false, 403, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 5, null, null) },
            { "top_f_207", new ItemData("Куртка Broker", false, 411, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 5, null, null) },
            { "top_f_208", new ItemData("Куртка Sweatbox", false, 412, new int[] { 0, 1, 2 }, 5, null, null) },
            { "top_f_209", new ItemData("Блуза без рукавов", false, 404, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 12, null, null) },
            { "top_f_210", new ItemData("Блуза без рукавов #2", false, 405, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, 12, null, null) },
        };

        [JsonIgnore]
        public const int Slot = (int)Customization.ClothesTypes.Top;

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

            if (Toggled)
            {
                player.SetClothes(Slot, data.ExtraData.Drawable, data.Textures[variation]);
                player.SetClothes(Gloves.Slot, data.ExtraData.BestTorso, 0);
            }
            else
            {
                player.SetClothes(Slot, data.Drawable, data.Textures[variation]);
                player.SetClothes(Gloves.Slot, data.BestTorso, 0);
            }

            if (pData.Armour != null)
            {
                var aData = pData.Armour.Data;

                var aVar = pData.Armour.Var;

                if (Data.Sex != pData.Sex)
                {
                    aData = pData.Armour.SexAlternativeData;

                    if (aData == null)
                        return;

                    if (aVar >= aData.Textures.Length)
                        aVar = aData.Textures.Length;
                }

                player.SetClothes(9, aData.DrawableTop, aData.Textures[aVar]);
            }

            if (pData.Clothes[2] != null)
                pData.Clothes[2].Wear(pData);
            else
                pData.Accessories[7]?.Wear(pData);
        }

        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            player.SetClothes(Slot, Game.Data.Customization.GetNudeDrawable(Game.Data.Customization.ClothesTypes.Top, pData.Sex), 0);
            player.SetClothes(Gloves.Slot, Game.Data.Customization.GetNudeDrawable(Game.Data.Customization.ClothesTypes.Gloves, pData.Sex), 0);

            if (pData.Armour != null)
            {
                var aData = pData.Armour.Data;

                var aVar = pData.Armour.Var;

                if (Data.Sex != pData.Sex)
                {
                    aData = pData.Armour.SexAlternativeData;

                    if (aData == null)
                        return;

                    if (aVar >= aData.Textures.Length)
                        aVar = aData.Textures.Length;
                }

                player.SetClothes(9, aData.Drawable, aData.Textures[aVar]);
            }

            if (pData.Clothes[2] != null)
                pData.Clothes[2].Wear(pData);
            else
                pData.Accessories[7]?.Wear(pData);
        }

        public void Action(PlayerData pData)
        {
            if (Data.ExtraData == null)
                return;

            Toggled = !Toggled;

            Wear(pData);
        }

        public Top(string ID, int Variation) : base(ID, IDList[ID], typeof(Top), Variation)
        {

        }
    }
}
