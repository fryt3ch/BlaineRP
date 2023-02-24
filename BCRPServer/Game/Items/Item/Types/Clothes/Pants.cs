using BCRPServer.Game.Data;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BCRPServer.Game.Items
{
    public class Pants : Clothes
    {
        new public class ItemData : Clothes.ItemData
        {
            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")}";

            public ItemData(string Name, bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(Name, 0.4f, "p_laz_j02_s", Sex, Drawable, Textures, SexAlternativeID) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "pants_m_0", new ItemData("Джинсы обычные", true, 0, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_1", new ItemData("Джинсы свободные", true, 1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_2", new ItemData("Спортивные штаны", true, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_3", new ItemData("Джинсы джогеры", true, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_4", new ItemData("Свободные спортивные штаны", true, 5, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_5", new ItemData("Шорты на веревках", true, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_6", new ItemData("Джинсы очень свободные", true, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_7", new ItemData("Мятые брюки", true, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_8", new ItemData("Брюки с карманами", true, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_9", new ItemData("Шорты обычные", true, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_10", new ItemData("Шорты беговые", true, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_11", new ItemData("Бриджи с карманами", true, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_12", new ItemData("Бриджи с принтами", true, 16, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "pants_m_13", new ItemData("Бриджи обычные", true, 17, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "pants_m_14", new ItemData("Шорты беговые #2", true, 18, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "pants_m_15", new ItemData("Брюки хулигана", true, 26, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "pants_m_16", new ItemData("Брюки карго", true, 27, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "pants_m_17", new ItemData("Брюки хаки", true, 29, new int[] { 0, 1, 2 }, null) },
            { "pants_m_18", new ItemData("Трико", true, 32, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_m_19", new ItemData("Шорты длинные", true, 42, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "pants_m_20", new ItemData("Джинсы бандитские", true, 43, new int[] { 0, 1 }, null) },
            { "pants_m_21", new ItemData("Штаны спортивки", true, 64, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "pants_m_22", new ItemData("Штаны с принтами", true, 58, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_23", new ItemData("Шорты хулигана", true, 62, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_m_24", new ItemData("Штаны гонщика", true, 68, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "pants_m_25", new ItemData("Штаны с принтами #2", true, 69, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, null) },
            { "pants_m_26", new ItemData("Штаны байкерские", true, 71, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "pants_m_27", new ItemData("Штаны байкерские #2", true, 73, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "pants_m_28", new ItemData("Джинсы рваные", true, 76, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "pants_m_29", new ItemData("Штаны хулигана на веревках", true, 78, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "pants_m_30", new ItemData("Штаны хулигана на веревках #2", true, 83, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_m_31", new ItemData("Бриджи зауженные", true, 80, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "pants_m_32", new ItemData("Джинсы хулигана", true, 82, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "pants_m_33", new ItemData("Комбинезон", true, 90, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "pants_m_34", new ItemData("Штаны разноцветные", true, 98, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "pants_m_35", new ItemData("Штаны пижамные модника", true, 100, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, null) },
            { "pants_m_36", new ItemData("Шорты модника", true, 117, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "pants_m_37", new ItemData("Штаны спортивные", true, 55, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_m_38", new ItemData("Кимоно (М)", true, 56, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "pants_m_39", new ItemData("Штаны хаки разноцветные", true, 59, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "pants_m_40", new ItemData("Джинсы ретро", true, 63, new int[] { 0 }, null) },
            { "pants_m_41", new ItemData("Джинсы старого стиля", true, 75, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "pants_m_42", new ItemData("Штаны с веревками", true, 81, new int[] { 0, 1, 2 }, null) },
            { "pants_m_43", new ItemData("Штаны хулигана на веревках #2", true, 83, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_m_44", new ItemData("Шорты баскетбольные", true, 132, new int[] { 0, 1, 2 }, null) },
            { "pants_m_45", new ItemData("Брюки обычные", true, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_46", new ItemData("Брюки свободные", true, 13, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_47", new ItemData("Брюки классические", true, 20, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_m_48", new ItemData("Брюки классические #2", true, 22, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, null) },
            { "pants_m_49", new ItemData("Брюки классические свободные", true, 23, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, null) },
            { "pants_m_50", new ItemData("Брюки зауженные", true, 24, new int[] { 0, 1, 2, 3, 4, 5, 6 }, null) },
            { "pants_m_51", new ItemData("Брюки вельветовые", true, 25, new int[] { 0, 1, 2, 3, 4, 5, 6 }, null) },
            { "pants_m_52", new ItemData("Брюки слаксы", true, 28, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_53", new ItemData("Брюки вельветовые #2", true, 35, new int[] { 0 }, null) },
            { "pants_m_54", new ItemData("Брюки хаки #2", true, 37, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_m_55", new ItemData("Штаны модника", true, 44, new int[] { 0 }, null) },
            { "pants_m_56", new ItemData("Брюки слаксы #2", true, 49, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "pants_m_57", new ItemData("Бриджи модника", true, 54, new int[] { 0, 1, 2, 3, 4, 5, 6 }, null) },
            { "pants_m_58", new ItemData("Брюки с узорами", true, 60, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "pants_m_59", new ItemData("Шорты для бега с принтами", true, 61, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, null) },
            { "pants_m_60", new ItemData("Бриджи с цепочкой", true, 103, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, null) },
            { "pants_m_61", new ItemData("Брюки модника", true, 116, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "pants_m_62", new ItemData("Брюки вельветовые разноцветные", true, 19, new int[] { 0, 1 }, null) },
            { "pants_m_63", new ItemData("Брюки классические разноцветные", true, 48, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "pants_m_64", new ItemData("Брюки с узорами", true, 51, new int[] { 0 }, null) },
            { "pants_m_65", new ItemData("Брюки слаксы разноцветные", true, 52, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_m_66", new ItemData("Брюки слаксы с узорами", true, 53, new int[] { 0 }, null) },
            { "pants_m_67", new ItemData("Штаны модника c принтами", true, 118, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "pants_m_68", new ItemData("Кимоно модника", true, 119, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "pants_m_69", new ItemData("Брюки дизайнерские", true, 131, new int[] { 0 }, null) },
            { "pants_m_70", new ItemData("Джоггеры модника", true, 138, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "pants_m_71", new ItemData("Джоггеры модника #2", true, 139, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "pants_m_72", new ItemData("Джоггеры модника #3", true, 140, new int[] { 0, 1, 2 }, null) },
            { "pants_m_73", new ItemData("Слаксы современные", true, 141, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "pants_m_74", new ItemData("Слаксы современные #2", true, 142, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 }, null) },
            { "pants_m_75", new ItemData("Слаксы кислотные", true, 143, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "pants_f_0", new ItemData("Джинсы обычные", false, 0, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_1", new ItemData("Джинсы свободные", false, 1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_2", new ItemData("Спортивные штаны", false, 2, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_3", new ItemData("Джоггеры", false, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_4", new ItemData("Юбка обычная", false, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_5", new ItemData("Юбка строгая", false, 18, new int[] { 0, 1 }, null) },
            { "pants_f_6", new ItemData("Юбка с принтами", false, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_7", new ItemData("Шорты обычные", false, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_8", new ItemData("Трусы с принтами", false, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_9", new ItemData("Чулки", false, 22, new int[] { 0, 1, 2 }, null) },
            { "pants_f_10", new ItemData("Брюки вельветовые", false, 11, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_11", new ItemData("Юбка укороченная", false, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_12", new ItemData("Шорты в обтяжку", false, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_13", new ItemData("Шорты в обтяжку #2", false, 16, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "pants_f_14", new ItemData("Штаны зауженные", false, 27, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_15", new ItemData("Юбка кимоно", false, 57, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "pants_f_16", new ItemData("Штаны спортивки", false, 66, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "pants_f_17", new ItemData("Штаны свободные с принтами", false, 60, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_18", new ItemData("Штаны обычные с принтами", false, 67, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, null) },
            { "pants_f_19", new ItemData("Штаны с низкой посадкой", false, 43, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "pants_f_20", new ItemData("Штаны карго с принтами", false, 71, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, null) },
            { "pants_f_21", new ItemData("Джинсы Slim", false, 73, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "pants_f_22", new ItemData("Джинсы Skinny", false, 74, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "pants_f_23", new ItemData("Джинсы хулиганки", false, 84, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "pants_f_24", new ItemData("Леггинсы с принтами", false, 87, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_25", new ItemData("Штаны кокетки", false, 102, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, null) },
            { "pants_f_26", new ItemData("Брюки хаки", false, 109, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, null) },
            { "pants_f_27", new ItemData("Штаны гонщицы", false, 68, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "pants_f_28", new ItemData("Штаны хулиганки на веревках", false, 80, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "pants_f_29", new ItemData("Штаны хулиганки на веревках #2", false, 81, new int[] { 0, 1, 2 }, null) },
            { "pants_f_30", new ItemData("Комбинезон", false, 92, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "pants_f_31", new ItemData("Чулки кружевные", false, 20, new int[] { 0, 1, 2 }, null) },
            { "pants_f_32", new ItemData("Леопардовое мини", false, 26, new int[] { 0 }, null) },
            { "pants_f_33", new ItemData("Мини в полоску", false, 28, new int[] { 0 }, null) },
            { "pants_f_34", new ItemData("Бриджи Military", false, 30, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "pants_f_35", new ItemData("Юбка карандаш легкая", false, 36, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_f_36", new ItemData("Брюки с боковыми разрезами", false, 44, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "pants_f_37", new ItemData("Брюки льняные", false, 45, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_f_38", new ItemData("Брюки облегающие прямые", false, 52, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_f_39", new ItemData("Леггинсы матовые", false, 54, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_f_40", new ItemData("Широкие спортивные штаны", false, 58, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_f_41", new ItemData("Штаны хаки разноцветные", false, 61, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "pants_f_42", new ItemData("Штаны мятые прямые", false, 64, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_f_43", new ItemData("Бриджи хулиганки на веревках", false, 82, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "pants_f_44", new ItemData("Бриджи на веревках", false, 83, new int[] { 0, 1, 2 }, null) },
            { "pants_f_45", new ItemData("Штаны хулиганки кожаные", false, 85, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_f_46", new ItemData("Комбинезон джинсовый", false, 93, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "pants_f_47", new ItemData("Брюки цветные с порезами", false, 106, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "pants_f_48", new ItemData("Шорты баскетбольные", false, 139, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_f_49", new ItemData("Брюки обычные", false, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_50", new ItemData("Трусы с кружевами", false, 19, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "pants_f_51", new ItemData("Шорты джинсовые с принтами", false, 25, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, null) },
            { "pants_f_52", new ItemData("Юбка строгая с принтами", false, 24, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, null) },
            { "pants_f_53", new ItemData("Брюки свободные", false, 23, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, null) },
            { "pants_f_54", new ItemData("Брюки узкие", false, 51, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "pants_f_55", new ItemData("Брюки классически #2", false, 50, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "pants_f_56", new ItemData("Юбка классическая", false, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_57", new ItemData("Трусы модницы", false, 56, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "pants_f_58", new ItemData("Трусы Кюлот", false, 62, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "pants_f_59", new ItemData("Брюки слаксы", false, 37, new int[] { 0, 1, 2, 3, 4, 5, 6 }, null) },
            { "pants_f_60", new ItemData("Чулки со швом", false, 63, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "pants_f_61", new ItemData("Штаны Skinny", false, 75, new int[] { 0, 1, 2 }, null) },
            { "pants_f_62", new ItemData("Юбка модницы", false, 108, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_63", new ItemData("Шорты с чулками", false, 78, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_f_64", new ItemData("Штаны с принтами", false, 104, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, null) },
            { "pants_f_65", new ItemData("Штаны Skinny #2", false, 112, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "pants_f_66", new ItemData("Штаны с рисунками", false, 124, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "pants_f_67", new ItemData("Кимоно с рисунками", false, 125, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "pants_f_68", new ItemData("Чиносы современные", false, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_69", new ItemData("Брюки строгие", false, 34, new int[] { 0 }, null) },
            { "pants_f_70", new ItemData("Чиносы легкие", false, 41, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_f_71", new ItemData("Чиносы льняные", false, 49, new int[] { 0, 1 }, null) },
            { "pants_f_72", new ItemData("Брюки Skinny глянцевые", false, 76, new int[] { 0, 1, 2 }, null) },
            { "pants_f_73", new ItemData("Брюки Skinny кожаные", false, 77, new int[] { 0, 1, 2 }, null) },
            { "pants_f_74", new ItemData("Шорты модницы с принтами", false, 107, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "pants_f_75", new ItemData("Шорты длинные с принтами", false, 123, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "pants_f_76", new ItemData("Брюки облегающие с узором", false, 53, new int[] { 0 }, null) },
            { "pants_f_77", new ItemData("Леггинсы с узором", false, 55, new int[] { 0 }, null) },
            { "pants_f_78", new ItemData("Брюки Метро", false, 138, new int[] { 0 }, null) },
            { "pants_f_79", new ItemData("Джоггеры модницы", false, 145, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "pants_f_80", new ItemData("Джоггеры модницы #2", false, 146, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "pants_f_81", new ItemData("Джоггеры модницы #3", false, 147, new int[] { 0, 1, 2 }, null) },
            { "pants_f_82", new ItemData("Слаксы современные", false, 148, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "pants_f_83", new ItemData("Слаксы современные #2", false, 149, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 }, null) },
            { "pants_f_84", new ItemData("Слаксы кислотные", false, 150, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
        };

        [JsonIgnore]
        public const int Slot = (int)Customization.ClothesTypes.Pants;

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

            player.SetClothes(4, data.Drawable, data.Textures[variation]);
        }

        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            player.SetClothes(Slot, Game.Data.Customization.GetNudeDrawable(Game.Data.Customization.ClothesTypes.Pants, pData.Sex), 0);
        }

        public Pants(string ID, int Variation) : base(ID, IDList[ID], typeof(Pants), Variation)
        {

        }
    }
}
