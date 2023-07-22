using Newtonsoft.Json;
using System.Collections.Generic;

namespace BlaineRP.Server.Game.Items
{
    public class Shoes : Clothes
    {
        new public class ItemData : Clothes.ItemData
        {
            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")}";

            public ItemData(string Name, bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(Name, 0.3f, "prop_ld_shoe_01", Sex, Drawable, Textures, SexAlternativeID) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "shoes_m_0", new ItemData("Кроссовки обычные", true, 2, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_1", new ItemData("Кеды", true, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_2", new ItemData("Сланцы", true, 5, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_3", new ItemData("Шлёпки с носками", true, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_4", new ItemData("Кроссовки скейтерские", true, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_5", new ItemData("Кроссовки спортивные", true, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_6", new ItemData("Сапоги Челси", true, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_7", new ItemData("Кеды большие", true, 22, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "shoes_m_8", new ItemData("Берцы", true, 24, new int[] { 0 }, null) },
            { "shoes_m_9", new ItemData("Берцы #2", true, 25, new int[] { 0 }, null) },
            { "shoes_m_10", new ItemData("Кеды обычные", true, 26, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_11", new ItemData("Кеды с шипами", true, 28, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "shoes_m_12", new ItemData("Кроссовки для бега", true, 31, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "shoes_m_13", new ItemData("Кроссовки высокие", true, 32, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_14", new ItemData("Сапоги", true, 44, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "shoes_m_15", new ItemData("Кроссовки скользящие", true, 42, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "shoes_m_16", new ItemData("Кроссовки с ремешком", true, 46, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "shoes_m_17", new ItemData("Сапоги байкерские", true, 50, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "shoes_m_18", new ItemData("Сапоги низкие", true, 53, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "shoes_m_19", new ItemData("Кроссовки с язычком", true, 57, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "shoes_m_20", new ItemData("Тапочки с подсветкой", true, 58, new int[] { 0, 1, 2 }, null) },
            { "shoes_m_21", new ItemData("Кроссовки боксерки", true, 64, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, null) },
            { "shoes_m_22", new ItemData("Сапоги разноцветные", true, 70, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "shoes_m_23", new ItemData("Сапоги с принтами", true, 79, new int[] { 0, 1 }, null) },
            { "shoes_m_24", new ItemData("Сапоги с принтами укороченные", true, 80, new int[] { 0, 1 }, null) },
            { "shoes_m_25", new ItemData("Кеды с носками", true, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_26", new ItemData("Потрёпанные берцы", true, 27, new int[] { 0 }, null) },
            { "shoes_m_27", new ItemData("Ботинки брутальные", true, 85, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_28", new ItemData("Ботинки брутальные укороченные", true, 86, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_29", new ItemData("Тапочки с подсветкой #2", true, 83, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }, null) },
            { "shoes_m_30", new ItemData("Казаки кожаные", true, 37, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "shoes_m_31", new ItemData("Казаки кожаные укороченные", true, 38, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "shoes_m_32", new ItemData("Хайкеры низкие с двойной шнуровкой", true, 43, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "shoes_m_33", new ItemData("Хайкеры высокие на шнуровке", true, 60, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "shoes_m_34", new ItemData("Хайкеры на шнуровке укороченные", true, 61, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "shoes_m_35", new ItemData("Хайкеры крепкие на шнуровке", true, 62, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "shoes_m_36", new ItemData("Хайкеры крепкие на шнуровке укороченные", true, 63, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "shoes_m_37", new ItemData("Хайкеры разноцветные", true, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_38", new ItemData("Ботинки высокие на шнуровке", true, 35, new int[] { 0, 1 }, null) },
            { "shoes_m_39", new ItemData("Ботинки без шнуровки", true, 52, new int[] { 0, 1 }, null) },
            { "shoes_m_40", new ItemData("Походные ботинки со шнуровкой", true, 65, new int[] { 0, 1, 2, 3, 4, 5, 6 }, null) },
            { "shoes_m_41", new ItemData("Походные ботинки со шнуровкой укороченные", true, 66, new int[] { 0, 1, 2, 3, 4, 5, 6 }, null) },
            { "shoes_m_42", new ItemData("Ботинки прорезиненные", true, 72, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "shoes_m_43", new ItemData("Ботинки прорезиненные укороченные", true, 73, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "shoes_m_44", new ItemData("Кеды с белыми вставками", true, 74, new int[] { 0, 1 }, null) },
            { "shoes_m_45", new ItemData("Ботинки зимние высокие", true, 81, new int[] { 0, 1, 2 }, null) },
            { "shoes_m_46", new ItemData("Ботинки зимние укороченные", true, 82, new int[] { 0, 1, 2 }, null) },
            { "shoes_m_47", new ItemData("Высокие сапоги без шнуровки", true, 88, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "shoes_m_48", new ItemData("Мокасины", true, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_49", new ItemData("Оксфорды черные", true, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_50", new ItemData("Ботинки из замши", true, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_51", new ItemData("Оксфорды разноцветные", true, 20, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "shoes_m_52", new ItemData("Лоферы", true, 21, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "shoes_m_53", new ItemData("Топсайдеры", true, 23, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_54", new ItemData("Лоферы с пряжками", true, 36, new int[] { 0, 1, 2, 3 }, null) },
            { "shoes_m_55", new ItemData("Кроссовки с подсветкой", true, 55, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "shoes_m_56", new ItemData("Кроссовки модника", true, 75, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "shoes_m_57", new ItemData("Кроссовки модника укороченные", true, 76, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "shoes_m_58", new ItemData("Кроссовки модника #2", true, 93, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, null) },
            { "shoes_m_59", new ItemData("Кроссовки с подсветкой #2", true, 77, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "shoes_m_60", new ItemData("Кроссовки модника #2 укороченные", true, 94, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, null) },
            { "shoes_m_61", new ItemData("Двухцветные лоферы", true, 30, new int[] { 0, 1 }, null) },
            { "shoes_m_62", new ItemData("Черно-белые оксфорды", true, 18, new int[] { 0, 1 }, null) },
            { "shoes_m_63", new ItemData("Оксфорды с носками цветные", true, 40, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "shoes_m_64", new ItemData("Джодпуры черно-белые", true, 19, new int[] { 0 }, null) },
            { "shoes_m_65", new ItemData("Кроссовки дутые глянцевые", true, 29, new int[] { 0 }, null) },
            { "shoes_m_66", new ItemData("Слипперы красные", true, 41, new int[] { 0 }, null) },
            { "shoes_m_67", new ItemData("Ботинки модника с вставками", true, 69, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "shoes_m_68", new ItemData("Кроссовки-носки модника", true, 87, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, null) },
            { "shoes_m_69", new ItemData("Слипоны с бананами", true, 95, new int[] { 0 }, null) },
            { "shoes_m_70", new ItemData("Кроссовки автолюбителя", true, 99, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, null) },
            { "shoes_f_0", new ItemData("Кроссовки обычные", false, 1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_1", new ItemData("Валенки", false, 2, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_2", new ItemData("Кеды обычные", false, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_3", new ItemData("Кроссовки спортивные", false, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_4", new ItemData("Сланцы", false, 5, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_5", new ItemData("Туфли закрытые", false, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_6", new ItemData("Сапоги высокие", false, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_7", new ItemData("Кроссовки баскетбольные", false, 11, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_8", new ItemData("Балетки", false, 13, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_9", new ItemData("Сандалии", false, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_10", new ItemData("Сапоги разноцветные", false, 21, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "shoes_f_11", new ItemData("Туфли с 2 язычком", false, 22, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_12", new ItemData("Ботильоны", false, 30, new int[] { 0 }, null) },
            { "shoes_f_13", new ItemData("Кеды закрытые", false, 33, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "shoes_f_14", new ItemData("Ботинки дезерты", false, 29, new int[] { 0, 1, 2 }, null) },
            { "shoes_f_15", new ItemData("Кроссовки на высокой подошве", false, 32, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "shoes_f_16", new ItemData("Ботильоны #2", false, 44, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "shoes_f_17", new ItemData("Кроссовки скейтерские", false, 60, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "shoes_f_18", new ItemData("Тапочки с подсветкой", false, 61, new int[] { 0, 1, 2 }, null) },
            { "shoes_f_19", new ItemData("Кроссовки боксерки", false, 67, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, null) },
            { "shoes_f_20", new ItemData("Сапоги сникерсы", false, 77, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, null) },
            { "shoes_f_21", new ItemData("Сапоги байкерские", false, 85, new int[] { 0, 1, 2 }, null) },
            { "shoes_f_22", new ItemData("Ботинки высокие", false, 26, new int[] { 0 }, null) },
            { "shoes_f_23", new ItemData("Казаки", false, 38, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "shoes_f_24", new ItemData("Валенки", false, 2, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_25", new ItemData("Казаки #2", false, 45, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "shoes_f_26", new ItemData("Казаки #1 укороченные", false, 39, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "shoes_f_27", new ItemData("Казаки #2 укороченные", false, 46, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "shoes_f_28", new ItemData("Берцы", false, 51, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "shoes_f_29", new ItemData("Берцы укороченные", false, 52, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "shoes_f_30", new ItemData("Берцы #2", false, 54, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "shoes_f_31", new ItemData("Берцы #2 укороченные", false, 55, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "shoes_f_32", new ItemData("Хайкеры крепкие на шнуровке", false, 63, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "shoes_f_33", new ItemData("Хайкеры крепкие на шнуровке укороченные", false, 64, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "shoes_f_34", new ItemData("Походные ботинки со шнуровкой", false, 65, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "shoes_f_35", new ItemData("Походные ботинки со шнуровкой укороченные", false, 66, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "shoes_f_36", new ItemData("Сапоги с принтами", false, 83, new int[] { 0, 1 }, null) },
            { "shoes_f_37", new ItemData("Сапоги с принтами укороченные", false, 84, new int[] { 0, 1 }, null) },
            { "shoes_f_38", new ItemData("Кеды простые", false, 27, new int[] { 0 }, null) },
            { "shoes_f_39", new ItemData("Кеды беговые", false, 28, new int[] { 0 }, null) },
            { "shoes_f_40", new ItemData("Полусапожки", false, 53, new int[] { 0, 1 }, null) },
            { "shoes_f_41", new ItemData("Полусапожки укороченные", false, 59, new int[] { 0, 1 }, null) },
            { "shoes_f_42", new ItemData("Тапочки с подсветкой #2", false, 87, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }, null) },
            { "shoes_f_43", new ItemData("Ботинки брутальные", false, 89, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_44", new ItemData("Ботинки брутальные укороченные", false, 90, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_45", new ItemData("Ботинки прорезиненные", false, 75, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "shoes_f_46", new ItemData("Ботинки прорезиненные укороченные", false, 76, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "shoes_f_47", new ItemData("Туфли обычные", false, 0, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_48", new ItemData("Туфли Стилеты", false, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_49", new ItemData("Туфли Lita", false, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_50", new ItemData("Кроссовки спортивные #2", false, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_51", new ItemData("Туфли открытые", false, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_52", new ItemData("Туфли на шпильках", false, 19, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "shoes_f_53", new ItemData("Лодочки", false, 20, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "shoes_f_54", new ItemData("Сапоги Хайкеры", false, 68, new int[] { 0, 1, 2, 3, 4, 5, 6 }, null) },
            { "shoes_f_55", new ItemData("Лоферы", false, 37, new int[] { 0, 1, 2, 3 }, null) },
            { "shoes_f_56", new ItemData("Туфли на платформе", false, 42, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "shoes_f_57", new ItemData("Кроссовки позолоченные", false, 31, new int[] { 0 }, null) },
            { "shoes_f_58", new ItemData("Кроссовки с подсветкой", false, 58, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "shoes_f_59", new ItemData("Кроссовки модницы", false, 79, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "shoes_f_60", new ItemData("Кроссовки модницы укороченные", false, 80, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "shoes_f_61", new ItemData("Кроссовки с подсветкой #2", false, 81, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "shoes_f_62", new ItemData("Кроссовки модницы #2", false, 96, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, null) },
            { "shoes_f_63", new ItemData("Кроссовки модницы #2 укороченные", false, 97, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, null) },
            { "shoes_f_64", new ItemData("Сапоги с узорами", false, 73, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "shoes_f_65", new ItemData("Сапоги с узорами укороченные", false, 74, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "shoes_f_66", new ItemData("Туфли с носками", false, 18, new int[] { 0, 1, 2 }, null) },
            { "shoes_f_67", new ItemData("Сапоги высокие с застежками", false, 56, new int[] { 0, 1, 2 }, null) },
            { "shoes_f_68", new ItemData("Сапоги укороченные с застежками", false, 57, new int[] { 0, 1, 2 }, null) },
            { "shoes_f_69", new ItemData("Сапоги высокие на шнуровке", false, 24, new int[] { 0 }, null) },
            { "shoes_f_70", new ItemData("Ботинки высокие кожаные", false, 25, new int[] { 0 }, null) },
            { "shoes_f_71", new ItemData("Походные ботинки", false, 36, new int[] { 0, 1 }, null) },
            { "shoes_f_72", new ItemData("Кроссовки с белыми вставками", false, 78, new int[] { 0, 1 }, null) },
            { "shoes_f_73", new ItemData("Кроссовки с застежкой", false, 47, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "shoes_f_74", new ItemData("Кроссовки-носки модницы", false, 91, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, null) },
            { "shoes_f_75", new ItemData("Высокие сапоги без шнуровки", false, 92, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "shoes_f_76", new ItemData("Слипоны модницы", false, 98, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "shoes_f_77", new ItemData("Слипоны с бананами", false, 99, new int[] { 0 }, null) },
            { "shoes_f_78", new ItemData("Туфли на шпильке с ремешком", false, 23, new int[] { 0, 1, 2 }, null) },
            { "shoes_f_79", new ItemData("Кроссовки автолюбителя", false, 103, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, null) },
        };

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

            player.SetClothes(Slot, data.Drawable, data.Textures[variation]);
        }

        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            player.SetClothes(Slot, Game.Data.Customization.GetNudeDrawable(Game.Data.Customization.ClothesTypes.Shoes, pData.Sex), 0);
        }

        public Shoes(string ID, int Variation) : base(ID, IDList[ID], typeof(Shoes), Variation)
        {

        }
    }
}
