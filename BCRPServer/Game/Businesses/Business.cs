using BCRPServer.Game.Items;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCRPServer.Game.Businesses
{
    public abstract class Business
    {
        public static string AllNames { get; set; }

        public enum Types
        {
            ClothesShop1 = 0,
            ClothesShop2,
            ClothesShop3,

            BagShop,

            CarShop1,
            CarShop2,
            CarShop3,
            MotoShop,
            BoatShop,
            AeroShop,

            Market,

            GasStation,
        }

        public static Dictionary<int, Business> All;

        /// <summary>Тип бизнеса</summary>
        public Types Type { get; set; }

        /// <summary>ID бизнеса (уникальный)</summary>
        public int ID { get; set; }

        /// <summary>2й ID бизнеса</summary>
        /// <remarks>Уникален лишь для каждого вида бизнеса</remarks>
        public int SubID { get; set; }

        /// <summary>Название</summary>
        public string Name { get; set; }

        /// <summary>Название + SubID</summary>
        /// <value>Строка вида: Name #SubID</value>
        public string NameAndSubID { get => Name ?? " " + $" #{SubID}"; }

        /// <summary>CID владельца</summary>
        /// <remarks>Если владельца нет, то -1</remarks>
        public int Owner { get; set; }

        public string OwnerName { get => Owner == -1 ? Locale.Businesses.Government : "Игрок"; }

        /// <summary>Наличных в кассе</summary>
        public int Cash { get; set; }

        /// <summary>Денег в банке</summary>
        public int Bank { get; set; }

        /// <summary>Кол-во материалов</summary>
        public int Materials { get; set; }

        /// <summary>Кол-во заказанных материалов</summary>
        public int OrderedMaterials { get; set; }

        /// <summary>Гос. цена</summary>
        public int Price { get; set; }

        /// <summary>Наценка на товары</summary>
        /// <remarks>m*X=N, где m - стандартная цена товара, X - наценка, а N - итоговая цена товара</remarks>
        public float Margin { get; set; }

        /// <summary>Позиция бизнеса</summary>
        public Vector3 Position { get; set; }

        /// <summary>Информация о прибыли за последний месяц</summary>
        /// <remarks>Массив длиной 31, где индекс - (день месяца - 1), а значение - прибыль за этот день (если значение -1, то нет данных)</remarks>
        public int[] Statistics { get; set; }

        public Business(int ID, Vector3 Position, Types Type, int Price = 100)
        {
            this.Price = Price;

            this.ID = ID;

            this.Type = Type;

            this.Position = Position;

            All.Add(ID, this);
        }

        public static int LoadAll()
        {
            if (All != null)
                return All.Count;

            All = new Dictionary<int, Business>();

            #region Clothes (Cheap)
            new ClothesShop1(1, new Vector3(1198f, 2701f, 38f), new Vector3(1190.645f, 2714.381f, 39.222f), 176f);
            new ClothesShop1(3, new Vector3(-1093.5f, 2703.7f, 19f), new Vector3(1190.645f, 2714.381f, 39.222f), 176f);
            new ClothesShop1(4, new Vector3(1685.5f, 4820.2f, 42f), new Vector3(1190.645f, 2714.381f, 39.222f), 176f);
            new ClothesShop1(5, new Vector3(-1.5f, 6517.2f, 31.2f), new Vector3(1190.645f, 2714.381f, 39.222f), 176f);

            new ClothesShop1(11, new Vector3(-817.3f, -1079.856f, 11.133f), new Vector3(1190.645f, 2714.381f, 39.222f), 176f);
            new ClothesShop1(12, new Vector3(83.64771f, -1391.713f, 29.41865f), new Vector3(1190.645f, 2714.381f, 39.222f), 176f);
            new ClothesShop1(13, new Vector3(416.7564f, -807.4344f, 29.38187f), new Vector3(1190.645f, 2714.381f, 39.222f), 176f);
            #endregion

            #region Clothes (Expensive)
            new ClothesShop2(2, new Vector3(618.5f, 2747.7f, 42f), new Vector3(617.65f, 2766.828f, 42.0881f), 176f);
            new ClothesShop2(14, new Vector3(-3167.542f, 1057.887f, 20.85858f), new Vector3(617.65f, 2766.828f, 42.0881f), 176f);

            new ClothesShop2(9, new Vector3(128.3956f, -207.6191f, 54.58f), new Vector3(617.65f, 2766.828f, 42.0881f), 176f);
            new ClothesShop2(10, new Vector3(-1202.328f, -778.6373f, 17.33572f), new Vector3(617.65f, 2766.828f, 42.0881f), 176f);

            #endregion

            #region Clothes (Brand)
            new ClothesShop3(6, new Vector3(-1456f, -232f, 49.5f), new Vector3(-1447.433f, -243.1756f, 49.82227f), 70f);
            new ClothesShop3(7, new Vector3(-718.46f, -157.63f, 37f), new Vector3(-1447.433f, -243.1756f, 49.82227f), 70f);
            new ClothesShop3(8, new Vector3(-155.5432f, -305.705f, 39.08f), new Vector3(-1447.433f, -243.1756f, 49.82227f), 70f);
            #endregion

            new Market(15, new Vector3(549.1185f, 2671.407f, 42.1565f));

            new GasStation(16, new Vector3(270.1317f, 2601.239f, 44.64737f), new Vector3(263.9698f, 2607.402f, 44.98298f));

            new CarShop1(17, new Vector3(-62.48621f, -1089.3f, 26.69341f), new Vector3(-55.08611f, -1111.217f, 26.05543f), 36.2f);

            new BoatShop(18, new Vector3(-813.3688f, -1336.428f, 5.150263f), new Vector3(-852.8972f, -1335.998f, 0.1195435f), 108.4271f);

            new AeroShop(19, new Vector3(1757.495f, 3239.969f, 41.94524f), new Vector3(1770.4f, 3239.908f, 42.02776f), 352.3067f);

            for (int i = 1; i < All.Count + 1; i++)
            {
                All[i] = MySQL.GetBusiness(All[i]);
            }

            AllNames = All.ToDictionary(x => x.Key, x => x.Value.Name).SerializeToJson();

            var lines = new List<string>();

            var insIdx = 0;

            using (var sr = new StreamReader(Settings.DIR_CLIENT_SHOP_DATA_PATH))
            {
                bool ignore = false;

                string line;

                var i = 0;

                while ((line = sr.ReadLine()) != null)
                {
                    if (!ignore)
                    {
                        if (line.Contains("#region TO_REPLACE"))
                        {
                            ignore = true;

                            insIdx = i;
                        }

                        lines.Add(line);
                    }
                    else
                    {
                        if (line.Contains("#endregion"))
                        {
                            ignore = false;

                            lines.Add(line);
                        }
                    }

                    i++;
                }
            }

            foreach (var x in Shop.AllPrices)
            {
                lines.Insert(++insIdx, $"Prices.Add(Types.{x.Key}, new Dictionary<string, int>() {{{string.Join(", ", x.Value.Select(x => $"{{\"{x.Key}\", {x.Value}}}"))}}});");
            }

            File.WriteAllLines(Settings.DIR_CLIENT_SHOP_DATA_PATH, lines);

            return All.Count;
        }

        public static Business Get(int id) => All.GetValueOrDefault(id);
    }

    public interface IEnterable
    {
        public Vector3 EnterPosition { get; set; }

        public float Heading { get; set; }
    }

    public abstract class Shop : Business
    {
        public static Dictionary<Types, Dictionary<string, int>> AllPrices = new Dictionary<Types, Dictionary<string, int>>()
        {
            #region ClothesShop1
            {
                Types.ClothesShop1,

                new Dictionary<string, int>()
                {
                    { "top_m_0", 100 },
                    { "top_m_1", 100 },
                    { "top_m_2", 100 },
                    { "top_m_3", 100 },
                    { "top_m_4", 100 },
                    { "top_m_5", 100 },
                    { "top_m_6", 100 },
                    { "top_m_7", 100 },
                    { "top_m_8", 100 },
                    { "top_m_9", 100 },
                    { "top_m_10", 100 },
                    { "top_m_11", 100 },
                    { "top_m_12", 100 },
                    { "top_m_13", 100 },
                    { "top_m_14", 100 },
                    { "top_m_15", 100 },
                    { "top_m_16", 100 },
                    { "top_m_17", 100 },
                    { "top_m_18", 100 },
                    { "top_m_19", 100 },
                    { "top_m_20", 100 },
                    { "top_m_21", 100 },
                    { "top_m_22", 100 },
                    { "top_m_23", 100 },
                    { "top_m_24", 100 },
                    { "top_m_25", 100 },
                    { "top_m_26", 100 },
                    { "top_m_27", 100 },
                    { "top_m_28", 100 },
                    { "top_m_29", 100 },
                    { "top_m_30", 100 },
                    { "top_m_31", 100 },
                    { "top_m_32", 100 },
                    { "top_m_33", 100 },
                    { "top_m_34", 100 },
                    { "top_m_35", 100 },
                    { "top_m_36", 100 },
                    { "top_m_37", 100 },
                    { "top_m_38", 100 },
                    { "top_m_39", 100 },
                    { "top_m_40", 100 },
                    { "top_m_41", 100 },
                    { "top_m_42", 100 },
                    { "top_m_43", 100 },
                    { "top_m_44", 100 },
                    { "top_m_45", 100 },
                    { "top_m_46", 100 },
                    { "top_m_47", 100 },
                    { "top_m_48", 100 },
                    { "top_m_49", 100 },
                    { "top_m_50", 100 },
                    { "top_m_51", 100 },
                    { "top_m_52", 100 },
                    { "top_m_53", 100 },
                    { "top_m_54", 100 },
                    { "top_m_55", 100 },
                    { "top_m_56", 100 },
                    { "top_m_57", 100 },
                    { "top_m_58", 100 },
                    { "top_m_59", 100 },
                    { "top_m_60", 100 },
                    { "top_m_61", 100 },
                    { "top_m_62", 100 },
                    { "top_m_63", 100 },
                    { "top_m_64", 100 },
                    { "top_m_65", 100 },
                    { "top_m_66", 100 },
                    { "top_m_67", 100 },
                    { "top_m_68", 100 },
                    { "top_m_69", 100 },
                    { "top_m_70", 100 },
                    { "top_m_71", 100 },
                    { "top_m_72", 100 },
                    { "top_m_73", 100 },
                    { "top_m_74", 100 },
                    { "top_m_75", 100 },
                    { "top_m_76", 100 },
                    { "top_m_77", 100 },
                    { "top_m_78", 100 },
                    { "top_m_79", 100 },
                    { "top_m_80", 100 },
                    { "top_m_81", 100 },
                    { "top_m_82", 100 },
                    { "top_m_83", 100 },
                    { "top_m_84", 100 },
                    { "top_m_85", 100 },
                    { "top_m_86", 100 },
                    { "top_m_87", 100 },
                    { "top_m_88", 100 },
                    { "top_m_89", 100 },
                    { "top_m_90", 100 },
                    { "top_m_91", 100 },
                    { "top_m_92", 100 },
                    { "top_m_93", 100 },
                    { "top_m_94", 100 },
                    { "top_m_97", 100 },
                    { "top_m_98", 100 },
                    { "top_m_99", 100 },
                    { "top_m_100", 100 },
                    { "top_m_101", 100 },
                    { "top_m_102", 100 },
                    { "top_m_103", 100 },
                    { "top_m_104", 100 },
                    { "top_m_105", 100 },
                    { "top_m_106", 100 },
                    { "top_m_107", 100 },
                    { "top_m_108", 100 },
                    { "top_m_109", 100 },
                    { "top_m_110", 100 },
                    { "top_m_112", 100 },
                    { "top_m_113", 100 },
                    { "top_m_114", 100 },
                    { "top_m_115", 100 },
                    { "top_m_116", 100 },
                    { "top_m_117", 100 },
                    { "top_m_118", 100 },
                    { "top_m_119", 100 },
                    { "top_m_120", 100 },
                    { "top_m_121", 100 },
                    { "top_m_122", 100 },
                    { "top_m_123", 100 },
                    { "top_m_124", 100 },
                    { "top_m_126", 100 },
                    { "top_m_127", 100 },
                    { "top_m_128", 100 },
                    { "top_m_129", 100 },
                    { "top_m_130", 100 },
                    { "top_m_132", 100 },
                    { "top_m_133", 100 },

                    { "top_f_1", 100 },
                    { "top_f_2", 100 },
                    { "top_f_3", 100 },
                    { "top_f_4", 100 },
                    { "top_f_5", 100 },
                    { "top_f_6", 100 },
                    { "top_f_8", 100 },
                    { "top_f_10", 100 },
                    { "top_f_11", 100 },
                    { "top_f_12", 100 },
                    { "top_f_13", 100 },
                    { "top_f_14", 100 },
                    { "top_f_15", 100 },
                    { "top_f_16", 100 },
                    { "top_f_17", 100 },
                    { "top_f_18", 100 },
                    { "top_f_19", 100 },
                    { "top_f_20", 100 },
                    { "top_f_21", 100 },
                    { "top_f_22", 100 },
                    { "top_f_23", 100 },
                    { "top_f_24", 100 },
                    { "top_f_25", 100 },
                    { "top_f_26", 100 },
                    { "top_f_27", 100 },
                    { "top_f_28", 100 },
                    { "top_f_29", 100 },
                    { "top_f_30", 100 },
                    { "top_f_31", 100 },
                    { "top_f_32", 100 },
                    { "top_f_33", 100 },
                    { "top_f_34", 100 },
                    { "top_f_35", 100 },
                    { "top_f_36", 100 },
                    { "top_f_37", 100 },
                    { "top_f_38", 100 },
                    { "top_f_39", 100 },
                    { "top_f_40", 100 },
                    { "top_f_41", 100 },
                    { "top_f_42", 100 },
                    { "top_f_43", 100 },
                    { "top_f_44", 100 },
                    { "top_f_45", 100 },
                    { "top_f_46", 100 },
                    { "top_f_47", 100 },
                    { "top_f_48", 100 },
                    { "top_f_49", 100 },
                    { "top_f_50", 100 },
                    { "top_f_51", 100 },
                    { "top_f_52", 100 },
                    { "top_f_53", 100 },
                    { "top_f_54", 100 },
                    { "top_f_55", 100 },
                    { "top_f_56", 100 },
                    { "top_f_57", 100 },
                    { "top_f_58", 100 },
                    { "top_f_59", 100 },
                    { "top_f_60", 100 },
                    { "top_f_61", 100 },
                    { "top_f_62", 100 },
                    { "top_f_63", 100 },
                    { "top_f_64", 100 },
                    { "top_f_65", 100 },
                    { "top_f_66", 100 },
                    { "top_f_68", 100 },
                    { "top_f_69", 100 },
                    { "top_f_70", 100 },
                    { "top_f_71", 100 },
                    { "top_f_72", 100 },
                    { "top_f_73", 100 },
                    { "top_f_74", 100 },
                    { "top_f_75", 100 },
                    { "top_f_76", 100 },
                    { "top_f_77", 100 },
                    { "top_f_78", 100 },
                    { "top_f_79", 100 },
                    { "top_f_80", 100 },
                    { "top_f_81", 100 },
                    { "top_f_82", 100 },
                    { "top_f_83", 100 },
                    { "top_f_84", 100 },
                    { "top_f_85", 100 },
                    { "top_f_86", 100 },
                    { "top_f_87", 100 },
                    { "top_f_88", 100 },
                    { "top_f_89", 100 },
                    { "top_f_90", 100 },
                    { "top_f_91", 100 },
                    { "top_f_92", 100 },
                    { "top_f_93", 100 },
                    { "top_f_94", 100 },
                    { "top_f_95", 100 },
                    { "top_f_97", 100 },
                    { "top_f_98", 100 },
                    { "top_f_99", 100 },
                    { "top_f_100", 100 },
                    { "top_f_101", 100 },
                    { "top_f_102", 100 },
                    { "top_f_103", 100 },
                    { "top_f_104", 100 },
                    { "top_f_105", 100 },
                    { "top_f_106", 100 },
                    { "top_f_107", 100 },
                    { "top_f_108", 100 },
                    { "top_f_109", 100 },
                    { "top_f_110", 100 },
                    { "top_f_111", 100 },
                    { "top_f_112", 100 },
                    { "top_f_113", 100 },
                    { "top_f_114", 100 },
                    { "top_f_115", 100 },
                    { "top_f_117", 100 },
                    { "top_f_118", 100 },
                    { "top_f_119", 100 },
                    { "top_f_120", 100 },
                    { "top_f_121", 100 },
                    { "top_f_122", 100 },
                    { "top_f_123", 100 },
                    { "top_f_124", 100 },
                    { "top_f_125", 100 },
                    { "top_f_126", 100 },
                    { "top_f_127", 100 },
                    { "top_f_128", 100 },
                    { "top_f_129", 100 },
                    { "top_f_130", 100 },
                    { "top_f_131", 100 },
                    { "top_f_132", 100 },
                    { "top_f_133", 100 },
                    { "top_f_134", 100 },
                    { "top_f_135", 100 },
                    { "top_f_137", 100 },
                    { "top_f_138", 100 },
                    { "top_f_139", 100 },

                    { "under_m_0", 100 },
                    { "under_m_1", 100 },
                    { "under_m_2", 100 },
                    { "under_m_3", 100 },
                    { "under_m_4", 100 },
                    { "under_m_5", 100 },
                    { "under_m_6", 100 },
                    { "under_m_7", 100 },
                    { "under_m_8", 100 },
                    { "under_m_9", 100 },
                    { "under_m_10", 100 },
                    { "under_m_11", 100 },
                    { "under_m_12", 100 },
                    { "under_m_13", 100 },
                    { "under_m_14", 100 },
                    { "under_m_15", 100 },
                    { "under_m_16", 100 },
                    { "under_m_17", 100 },
                    { "under_m_18", 100 },
                    { "under_m_19", 100 },
                    { "under_m_20", 100 },
                    { "under_m_21", 100 },
                    { "under_m_22", 100 },
                    { "under_m_23", 100 },
                    { "under_m_24", 100 },
                    { "under_m_25", 100 },
                    { "under_m_26", 100 },

                    { "under_f_0", 100 },
                    { "under_f_1", 100 },
                    { "under_f_2", 100 },
                    { "under_f_3", 100 },
                    { "under_f_4", 100 },
                    { "under_f_5", 100 },
                    { "under_f_6", 100 },
                    { "under_f_7", 100 },
                    { "under_f_8", 100 },
                    { "under_f_9", 100 },
                    { "under_f_10", 100 },
                    { "under_f_11", 100 },
                    { "under_f_12", 100 },
                    { "under_f_13", 100 },
                    { "under_f_14", 100 },
                    { "under_f_15", 100 },
                    { "under_f_16", 100 },
                    { "under_f_17", 100 },
                    { "under_f_18", 100 },
                    { "under_f_19", 100 },

                    { "pants_m_0", 100 },
                    { "pants_m_1", 100 },
                    { "pants_m_2", 100 },
                    { "pants_m_3", 100 },
                    { "pants_m_4", 100 },
                    { "pants_m_5", 100 },
                    { "pants_m_6", 100 },
                    { "pants_m_7", 100 },
                    { "pants_m_8", 100 },
                    { "pants_m_9", 100 },
                    { "pants_m_10", 100 },
                    { "pants_m_11", 100 },
                    { "pants_m_12", 100 },
                    { "pants_m_13", 100 },
                    { "pants_m_14", 100 },
                    { "pants_m_15", 100 },
                    { "pants_m_16", 100 },
                    { "pants_m_17", 100 },
                    { "pants_m_18", 100 },
                    { "pants_m_19", 100 },
                    { "pants_m_20", 100 },
                    { "pants_m_21", 100 },
                    { "pants_m_22", 100 },
                    { "pants_m_23", 100 },
                    { "pants_m_24", 100 },
                    { "pants_m_25", 100 },
                    { "pants_m_26", 100 },
                    { "pants_m_27", 100 },
                    { "pants_m_28", 100 },
                    { "pants_m_29", 100 },
                    { "pants_m_30", 100 },
                    { "pants_m_31", 100 },
                    { "pants_m_32", 100 },
                    { "pants_m_33", 100 },
                    { "pants_m_34", 100 },
                    { "pants_m_35", 100 },
                    { "pants_m_36", 100 },
                    { "pants_m_37", 100 },
                    { "pants_m_38", 100 },
                    { "pants_m_39", 100 },
                    { "pants_m_40", 100 },
                    { "pants_m_41", 100 },
                    { "pants_m_42", 100 },
                    { "pants_m_43", 100 },
                    { "pants_m_44", 100 },

                    { "pants_f_0", 100 },
                    { "pants_f_1", 100 },
                    { "pants_f_2", 100 },
                    { "pants_f_3", 100 },
                    { "pants_f_4", 100 },
                    { "pants_f_5", 100 },
                    { "pants_f_6", 100 },
                    { "pants_f_7", 100 },
                    { "pants_f_8", 100 },
                    { "pants_f_9", 100 },
                    { "pants_f_10", 100 },
                    { "pants_f_11", 100 },
                    { "pants_f_12", 100 },
                    { "pants_f_13", 100 },
                    { "pants_f_14", 100 },
                    { "pants_f_15", 100 },
                    { "pants_f_16", 100 },
                    { "pants_f_17", 100 },
                    { "pants_f_18", 100 },
                    { "pants_f_19", 100 },
                    { "pants_f_20", 100 },
                    { "pants_f_21", 100 },
                    { "pants_f_22", 100 },
                    { "pants_f_23", 100 },
                    { "pants_f_24", 100 },
                    { "pants_f_25", 100 },
                    { "pants_f_26", 100 },
                    { "pants_f_27", 100 },
                    { "pants_f_28", 100 },
                    { "pants_f_29", 100 },
                    { "pants_f_30", 100 },
                    { "pants_f_31", 100 },
                    { "pants_f_32", 100 },
                    { "pants_f_33", 100 },
                    { "pants_f_34", 100 },
                    { "pants_f_35", 100 },
                    { "pants_f_36", 100 },
                    { "pants_f_37", 100 },
                    { "pants_f_38", 100 },
                    { "pants_f_39", 100 },
                    { "pants_f_40", 100 },
                    { "pants_f_41", 100 },
                    { "pants_f_42", 100 },
                    { "pants_f_43", 100 },
                    { "pants_f_44", 100 },
                    { "pants_f_45", 100 },
                    { "pants_f_46", 100 },
                    { "pants_f_47", 100 },
                    { "pants_f_48", 100 },

                    { "shoes_m_0", 100 },
                    { "shoes_m_1", 100 },
                    { "shoes_m_2", 100 },
                    { "shoes_m_3", 100 },
                    { "shoes_m_4", 100 },
                    { "shoes_m_5", 100 },
                    { "shoes_m_6", 100 },
                    { "shoes_m_7", 100 },
                    { "shoes_m_8", 100 },
                    { "shoes_m_9", 100 },
                    { "shoes_m_10", 100 },
                    { "shoes_m_11", 100 },
                    { "shoes_m_12", 100 },
                    { "shoes_m_13", 100 },
                    { "shoes_m_14", 100 },
                    { "shoes_m_15", 100 },
                    { "shoes_m_16", 100 },
                    { "shoes_m_17", 100 },
                    { "shoes_m_18", 100 },
                    { "shoes_m_19", 100 },
                    { "shoes_m_20", 100 },
                    { "shoes_m_21", 100 },
                    { "shoes_m_22", 100 },
                    { "shoes_m_23", 100 },
                    { "shoes_m_24", 100 },
                    { "shoes_m_25", 100 },
                    { "shoes_m_26", 100 },
                    { "shoes_m_27", 100 },
                    { "shoes_m_28", 100 },
                    { "shoes_m_29", 100 },
                    { "shoes_m_30", 100 },
                    { "shoes_m_31", 100 },
                    { "shoes_m_32", 100 },
                    { "shoes_m_33", 100 },
                    { "shoes_m_34", 100 },
                    { "shoes_m_35", 100 },
                    { "shoes_m_36", 100 },
                    { "shoes_m_37", 100 },
                    { "shoes_m_38", 100 },
                    { "shoes_m_39", 100 },
                    { "shoes_m_40", 100 },
                    { "shoes_m_41", 100 },
                    { "shoes_m_42", 100 },
                    { "shoes_m_43", 100 },
                    { "shoes_m_44", 100 },
                    { "shoes_m_45", 100 },
                    { "shoes_m_46", 100 },
                    { "shoes_m_47", 100 },

                    { "shoes_f_0", 100 },
                    { "shoes_f_1", 100 },
                    { "shoes_f_2", 100 },
                    { "shoes_f_3", 100 },
                    { "shoes_f_4", 100 },
                    { "shoes_f_5", 100 },
                    { "shoes_f_6", 100 },
                    { "shoes_f_7", 100 },
                    { "shoes_f_8", 100 },
                    { "shoes_f_9", 100 },
                    { "shoes_f_10", 100 },
                    { "shoes_f_11", 100 },
                    { "shoes_f_12", 100 },
                    { "shoes_f_13", 100 },
                    { "shoes_f_14", 100 },
                    { "shoes_f_15", 100 },
                    { "shoes_f_16", 100 },
                    { "shoes_f_17", 100 },
                    { "shoes_f_18", 100 },
                    { "shoes_f_19", 100 },
                    { "shoes_f_20", 100 },
                    { "shoes_f_21", 100 },
                    { "shoes_f_22", 100 },
                    { "shoes_f_23", 100 },
                    { "shoes_f_24", 100 },
                    { "shoes_f_25", 100 },
                    { "shoes_f_26", 100 },
                    { "shoes_f_27", 100 },
                    { "shoes_f_28", 100 },
                    { "shoes_f_29", 100 },
                    { "shoes_f_30", 100 },
                    { "shoes_f_31", 100 },
                    { "shoes_f_32", 100 },
                    { "shoes_f_33", 100 },
                    { "shoes_f_34", 100 },
                    { "shoes_f_35", 100 },
                    { "shoes_f_36", 100 },
                    { "shoes_f_37", 100 },
                    { "shoes_f_38", 100 },
                    { "shoes_f_39", 100 },
                    { "shoes_f_40", 100 },
                    { "shoes_f_41", 100 },
                    { "shoes_f_42", 100 },
                    { "shoes_f_43", 100 },
                    { "shoes_f_44", 100 },
                    { "shoes_f_45", 100 },
                    { "shoes_f_46", 100 },

                    { "hat_m_0", 100 },
                    { "hat_m_1", 100 },
                    { "hat_m_2", 100 },
                    { "hat_m_3", 100 },
                    { "hat_m_4", 100 },
                    { "hat_m_5", 100 },
                    { "hat_m_6", 100 },
                    { "hat_m_7", 100 },
                    { "hat_m_8", 100 },
                    { "hat_m_9", 100 },
                    { "hat_m_10", 100 },
                    { "hat_m_11", 100 },
                    { "hat_m_12", 100 },
                    { "hat_m_13", 100 },
                    { "hat_m_14", 100 },
                    { "hat_m_15", 100 },
                    { "hat_m_16", 100 },
                    { "hat_m_17", 100 },
                    { "hat_m_18", 100 },
                    { "hat_m_19", 100 },
                    { "hat_m_20", 100 },
                    { "hat_m_21", 100 },
                    { "hat_m_22", 100 },
                    { "hat_m_23", 100 },
                    { "hat_m_24", 100 },
                    { "hat_m_25", 100 },
                    { "hat_m_26", 100 },
                    { "hat_m_27", 100 },
                    { "hat_m_28", 100 },
                    { "hat_m_29", 100 },
                    { "hat_m_30", 100 },
                    { "hat_m_31", 100 },
                    { "hat_m_32", 100 },
                    { "hat_m_33", 100 },
                    { "hat_m_34", 100 },
                    { "hat_m_35", 100 },
                    { "hat_m_36", 100 },
                    { "hat_m_37", 100 },

                    { "hat_f_0", 100 },
                    { "hat_f_1", 100 },
                    { "hat_f_2", 100 },
                    { "hat_f_3", 100 },
                    { "hat_f_4", 100 },
                    { "hat_f_5", 100 },
                    { "hat_f_6", 100 },
                    { "hat_f_7", 100 },
                    { "hat_f_8", 100 },
                    { "hat_f_9", 100 },
                    { "hat_f_10", 100 },
                    { "hat_f_11", 100 },
                    { "hat_f_12", 100 },
                    { "hat_f_13", 100 },
                    { "hat_f_14", 100 },
                    { "hat_f_15", 100 },
                    { "hat_f_16", 100 },
                    { "hat_f_17", 100 },
                    { "hat_f_18", 100 },
                    { "hat_f_19", 100 },
                    { "hat_f_20", 100 },
                    { "hat_f_21", 100 },
                    { "hat_f_22", 100 },
                    { "hat_f_23", 100 },
                    { "hat_f_24", 100 },
                    { "hat_f_25", 100 },
                    { "hat_f_26", 100 },
                    { "hat_f_27", 100 },
                    { "hat_f_28", 100 },
                    { "hat_f_29", 100 },
                    { "hat_f_30", 100 },
                    { "hat_f_31", 100 },
                    { "hat_f_32", 100 },
                    { "hat_f_33", 100 },
                    { "hat_f_34", 100 },
                    { "hat_f_35", 100 },
                    { "hat_f_36", 100 },

                    { "accs_m_0", 100 },
                    { "accs_m_1", 100 },
                    { "accs_m_2", 100 },
                    { "accs_m_3", 100 },
                    { "accs_m_4", 100 },
                    { "accs_m_5", 100 },
                    { "accs_m_6", 100 },
                    { "accs_m_7", 100 },
                    { "accs_m_8", 100 },
                    { "accs_m_9", 100 },
                    { "accs_m_10", 100 },
                    { "accs_m_11", 100 },
                    { "accs_m_12", 100 },

                    { "accs_f_0", 100 },
                    { "accs_f_1", 100 },
                    { "accs_f_2", 100 },
                    { "accs_f_3", 100 },
                    { "accs_f_4", 100 },
                    { "accs_f_5", 100 },

                    { "watches_m_0", 100 },
                    { "watches_m_1", 100 },
                    { "watches_m_2", 100 },
                    { "watches_m_3", 100 },
                    { "watches_m_4", 100 },
                    { "watches_m_5", 100 },
                    { "watches_m_6", 100 },
                    { "watches_m_7", 100 },
                    { "watches_m_8", 100 },
                    { "watches_m_9", 100 },
                    { "watches_m_10", 100 },
                    { "watches_m_11", 100 },

                    { "watches_f_0", 100 },
                    { "watches_f_1", 100 },
                    { "watches_f_2", 100 },
                    { "watches_f_3", 100 },
                    { "watches_f_4", 100 },
                    { "watches_f_5", 100 },
                    { "watches_f_6", 100 },
                    { "watches_f_7", 100 },

                    { "glasses_m_0", 100 },
                    { "glasses_m_1", 100 },
                    { "glasses_m_2", 100 },
                    { "glasses_m_3", 100 },
                    { "glasses_m_4", 100 },
                    { "glasses_m_5", 100 },
                    { "glasses_m_6", 100 },
                    { "glasses_m_7", 100 },
                    { "glasses_m_8", 100 },
                    { "glasses_m_9", 100 },

                    { "glasses_f_0", 100 },
                    { "glasses_f_1", 100 },
                    { "glasses_f_2", 100 },
                    { "glasses_f_3", 100 },
                    { "glasses_f_4", 100 },
                    { "glasses_f_5", 100 },
                    { "glasses_f_6", 100 },
                    { "glasses_f_7", 100 },

                    { "gloves_m_0", 100 },
                    { "gloves_m_1", 100 },
                    { "gloves_m_2", 100 },
                    { "gloves_m_3", 100 },
                    { "gloves_m_4", 100 },

                    { "gloves_f_0", 100 },
                    { "gloves_f_1", 100 },
                    { "gloves_f_2", 100 },
                    { "gloves_f_3", 100 },
                    { "gloves_f_4", 100 },

                    { "bracelet_m_6", 100 },
                    { "bracelet_m_7", 100 },
                    { "bracelet_m_8", 100 },

                    { "bracelet_f_13", 100 },
                    { "bracelet_f_14", 100 },
                    { "bracelet_f_15", 100 },
                }
            },
            #endregion

            #region ClothesShop2
            {
                Types.ClothesShop2,

                new Dictionary<string, int>()
                {
                    { "top_m_134", 100},
                    { "top_m_135", 100},
                    { "top_m_136", 100},
                    { "top_m_137", 100},
                    { "top_m_138", 100},
                    { "top_m_139", 100},
                    { "top_m_140", 100},
                    { "top_m_141", 100},
                    { "top_m_142", 100},
                    { "top_m_143", 100},
                    { "top_m_144", 100},
                    { "top_m_145", 100},
                    { "top_m_146", 100},
                    { "top_m_147", 100},
                    { "top_m_148", 100},
                    { "top_m_149", 100},
                    { "top_m_150", 100},
                    { "top_m_151", 100},
                    { "top_m_152", 100},
                    { "top_m_153", 100},
                    { "top_m_154", 100},
                    { "top_m_155", 100},
                    { "top_m_156", 100},
                    { "top_m_157", 100},
                    { "top_m_158", 100},
                    { "top_m_159", 100},
                    { "top_m_160", 100},
                    { "top_m_161", 100},
                    { "top_m_162", 100},
                    { "top_m_163", 100},
                    { "top_m_164", 100},
                    { "top_m_165", 100},
                    { "top_m_166", 100},
                    { "top_m_167", 100},
                    { "top_m_168", 100},
                    { "top_m_170", 100},
                    { "top_m_171", 100},
                    { "top_m_172", 100},
                    { "top_m_173", 100},
                    { "top_m_174", 100},
                    { "top_m_175", 100},
                    { "top_m_176", 100},
                    { "top_m_177", 100},
                    { "top_m_178", 100},
                    { "top_m_179", 100},
                    { "top_m_180", 100},
                    { "top_m_181", 100},
                    { "top_m_182", 100},
                    { "top_m_183", 100},
                    { "top_m_184", 100},
                    { "top_m_185", 100},
                    { "top_m_186", 100},
                    { "top_m_187", 100},
                    { "top_m_188", 100},
                    { "top_m_189", 100},
                    { "top_m_190", 100},
                    { "top_m_191", 100},
                    { "top_m_192", 100},
                    { "top_m_193", 100},
                    { "top_m_194", 100},
                    { "top_m_195", 100},
                    { "top_m_196", 100},
                    { "top_m_197", 100},
                    { "top_m_198", 100},

                    { "top_f_140", 100 },
                    { "top_f_141", 100 },
                    { "top_f_142", 100 },
                    { "top_f_143", 100 },
                    { "top_f_144", 100 },
                    { "top_f_145", 100 },
                    { "top_f_146", 100 },
                    { "top_f_147", 100 },
                    { "top_f_148", 100 },
                    { "top_f_149", 100 },
                    { "top_f_150", 100 },
                    { "top_f_151", 100 },
                    { "top_f_152", 100 },
                    { "top_f_153", 100 },
                    { "top_f_154", 100 },
                    { "top_f_155", 100 },
                    { "top_f_156", 100 },
                    { "top_f_157", 100 },
                    { "top_f_158", 100 },
                    { "top_f_159", 100 },
                    { "top_f_160", 100 },
                    { "top_f_161", 100 },
                    { "top_f_162", 100 },
                    { "top_f_163", 100 },
                    { "top_f_164", 100 },
                    { "top_f_165", 100 },
                    { "top_f_166", 100 },
                    { "top_f_167", 100 },
                    { "top_f_168", 100 },
                    { "top_f_169", 100 },
                    { "top_f_170", 100 },
                    { "top_f_171", 100 },
                    { "top_f_172", 100 },
                    { "top_f_173", 100 },
                    { "top_f_174", 100 },
                    { "top_f_175", 100 },
                    { "top_f_176", 100 },
                    { "top_f_177", 100 },
                    { "top_f_178", 100 },
                    { "top_f_179", 100 },
                    { "top_f_180", 100 },
                    { "top_f_181", 100 },
                    { "top_f_182", 100 },
                    { "top_f_183", 100 },
                    { "top_f_184", 100 },
                    { "top_f_185", 100 },
                    { "top_f_186", 100 },
                    { "top_f_187", 100 },
                    { "top_f_188", 100 },
                    { "top_f_189", 100 },
                    { "top_f_191", 100 },
                    { "top_f_192", 100 },
                    { "top_f_193", 100 },
                    { "top_f_194", 100 },
                    { "top_f_195", 100 },
                    { "top_f_196", 100 },
                    { "top_f_197", 100 },
                    { "top_f_198", 100 },
                    { "top_f_199", 100 },
                    { "top_f_200", 100 },
                    { "top_f_201", 100 },
                    { "top_f_202", 100 },
                    { "top_f_203", 100 },
                    { "top_f_204", 100 },
                    { "top_f_205", 100 },
                    { "top_f_206", 100 },
                    { "top_f_207", 100 },
                    { "top_f_208", 100 },
                    { "top_f_209", 100 },
                    { "top_f_210", 100 },

                    { "under_m_27", 100 },
                    { "under_m_28", 100 },
                    { "under_m_29", 100 },
                    { "under_m_30", 100 },
                    { "under_m_31", 100 },
                    { "under_m_32", 100 },
                    { "under_m_33", 100 },
                    { "under_m_34", 100 },
                    { "under_m_35", 100 },

                    { "under_f_20", 100 },
                    { "under_f_21", 100 },
                    { "under_f_22", 100 },
                    { "under_f_23", 100 },
                    { "under_f_24", 100 },

                    { "pants_m_45", 100 },
                    { "pants_m_46", 100 },
                    { "pants_m_47", 100 },
                    { "pants_m_48", 100 },
                    { "pants_m_49", 100 },
                    { "pants_m_50", 100 },
                    { "pants_m_51", 100 },
                    { "pants_m_52", 100 },
                    { "pants_m_53", 100 },
                    { "pants_m_54", 100 },
                    { "pants_m_55", 100 },
                    { "pants_m_56", 100 },
                    { "pants_m_57", 100 },
                    { "pants_m_58", 100 },
                    { "pants_m_59", 100 },
                    { "pants_m_60", 100 },
                    { "pants_m_61", 100 },
                    { "pants_m_62", 100 },
                    { "pants_m_63", 100 },
                    { "pants_m_64", 100 },
                    { "pants_m_65", 100 },
                    { "pants_m_66", 100 },
                    { "pants_m_67", 100 },
                    { "pants_m_68", 100 },
                    { "pants_m_69", 100 },
                    { "pants_m_70", 100 },
                    { "pants_m_71", 100 },
                    { "pants_m_72", 100 },
                    { "pants_m_73", 100 },
                    { "pants_m_74", 100 },
                    { "pants_m_75", 100 },

                    { "pants_f_49", 100 },
                    { "pants_f_50", 100 },
                    { "pants_f_51", 100 },
                    { "pants_f_52", 100 },
                    { "pants_f_53", 100 },
                    { "pants_f_54", 100 },
                    { "pants_f_55", 100 },
                    { "pants_f_56", 100 },
                    { "pants_f_57", 100 },
                    { "pants_f_58", 100 },
                    { "pants_f_59", 100 },
                    { "pants_f_60", 100 },
                    { "pants_f_61", 100 },
                    { "pants_f_62", 100 },
                    { "pants_f_63", 100 },
                    { "pants_f_64", 100 },
                    { "pants_f_65", 100 },
                    { "pants_f_66", 100 },
                    { "pants_f_67", 100 },
                    { "pants_f_68", 100 },
                    { "pants_f_69", 100 },
                    { "pants_f_70", 100 },
                    { "pants_f_71", 100 },
                    { "pants_f_72", 100 },
                    { "pants_f_73", 100 },
                    { "pants_f_74", 100 },
                    { "pants_f_75", 100 },
                    { "pants_f_76", 100 },
                    { "pants_f_77", 100 },
                    { "pants_f_78", 100 },
                    { "pants_f_79", 100 },
                    { "pants_f_80", 100 },
                    { "pants_f_81", 100 },
                    { "pants_f_82", 100 },
                    { "pants_f_83", 100 },
                    { "pants_f_84", 100 },

                    { "shoes_m_48", 100 },
                    { "shoes_m_49", 100 },
                    { "shoes_m_50", 100 },
                    { "shoes_m_51", 100 },
                    { "shoes_m_52", 100 },
                    { "shoes_m_53", 100 },
                    { "shoes_m_54", 100 },
                    { "shoes_m_55", 100 },
                    { "shoes_m_56", 100 },
                    { "shoes_m_57", 100 },
                    { "shoes_m_58", 100 },
                    { "shoes_m_59", 100 },
                    { "shoes_m_60", 100 },
                    { "shoes_m_61", 100 },
                    { "shoes_m_62", 100 },
                    { "shoes_m_63", 100 },
                    { "shoes_m_64", 100 },
                    { "shoes_m_65", 100 },
                    { "shoes_m_66", 100 },
                    { "shoes_m_67", 100 },
                    { "shoes_m_68", 100 },
                    { "shoes_m_69", 100 },
                    { "shoes_m_70", 100 },

                    { "shoes_f_47", 100 },
                    { "shoes_f_48", 100 },
                    { "shoes_f_49", 100 },
                    { "shoes_f_50", 100 },
                    { "shoes_f_51", 100 },
                    { "shoes_f_52", 100 },
                    { "shoes_f_53", 100 },
                    { "shoes_f_54", 100 },
                    { "shoes_f_55", 100 },
                    { "shoes_f_56", 100 },
                    { "shoes_f_57", 100 },
                    { "shoes_f_58", 100 },
                    { "shoes_f_59", 100 },
                    { "shoes_f_60", 100 },
                    { "shoes_f_61", 100 },
                    { "shoes_f_62", 100 },
                    { "shoes_f_63", 100 },
                    { "shoes_f_64", 100 },
                    { "shoes_f_65", 100 },
                    { "shoes_f_66", 100 },
                    { "shoes_f_67", 100 },
                    { "shoes_f_68", 100 },
                    { "shoes_f_69", 100 },
                    { "shoes_f_70", 100 },
                    { "shoes_f_71", 100 },
                    { "shoes_f_72", 100 },
                    { "shoes_f_73", 100 },
                    { "shoes_f_74", 100 },
                    { "shoes_f_75", 100 },
                    { "shoes_f_76", 100 },
                    { "shoes_f_77", 100 },
                    { "shoes_f_78", 100 },
                    { "shoes_f_79", 100 },

                    { "hat_m_38", 100 },
                    { "hat_m_39", 100 },
                    { "hat_m_40", 100 },
                    { "hat_m_41", 100 },
                    { "hat_m_42", 100 },
                    { "hat_m_43", 100 },
                    { "hat_m_44", 100 },
                    { "hat_m_45", 100 },
                    { "hat_m_46", 100 },
                    { "hat_m_47", 100 },
                    { "hat_m_48", 100 },
                    { "hat_m_49", 100 },
                    { "hat_m_50", 100 },
                    { "hat_m_51", 100 },
                    { "hat_m_52", 100 },
                    { "hat_m_53", 100 },
                    { "hat_m_54", 100 },

                    { "hat_f_37", 100 },
                    { "hat_f_38", 100 },
                    { "hat_f_39", 100 },
                    { "hat_f_40", 100 },
                    { "hat_f_41", 100 },
                    { "hat_f_42", 100 },
                    { "hat_f_43", 100 },
                    { "hat_f_44", 100 },
                    { "hat_f_45", 100 },
                    { "hat_f_46", 100 },
                    { "hat_f_47", 100 },
                    { "hat_f_48", 100 },
                    { "hat_f_49", 100 },

                    { "accs_m_13", 100 },
                    { "accs_m_14", 100 },
                    { "accs_m_15", 100 },
                    { "accs_m_16", 100 },
                    { "accs_m_17", 100 },
                    { "accs_m_18", 100 },
                    { "accs_m_19", 100 },
                    { "accs_m_20", 100 },
                    { "accs_m_21", 100 },
                    { "accs_m_22", 100 },

                    { "accs_f_6", 100 },
                    { "accs_f_7", 100 },
                    { "accs_f_8", 100 },
                    { "accs_f_9", 100 },
                    { "accs_f_10", 100 },
                    { "accs_f_11", 100 },

                    { "watches_m_12", 100 },
                    { "watches_m_13", 100 },
                    { "watches_m_14", 100 },
                    { "watches_m_15", 100 },
                    { "watches_m_16", 100 },
                    { "watches_m_17", 100 },
                    { "watches_m_18", 100 },
                    { "watches_m_19", 100 },
                    { "watches_m_20", 100 },
                    { "watches_m_21", 100 },
                    { "watches_m_22", 100 },
                    { "watches_m_23", 100 },
                    { "watches_m_24", 100 },
                    { "watches_m_25", 100 },

                    { "watches_f_8", 100 },
                    { "watches_f_9", 100 },
                    { "watches_f_10", 100 },
                    { "watches_f_11", 100 },
                    { "watches_f_12", 100 },
                    { "watches_f_13", 100 },
                    { "watches_f_14", 100 },
                    { "watches_f_15", 100 },

                    { "glasses_m_10", 100 },
                    { "glasses_m_11", 100 },
                    { "glasses_m_12", 100 },
                    { "glasses_m_13", 100 },
                    { "glasses_m_14", 100 },
                    { "glasses_m_15", 100 },
                    { "glasses_m_16", 100 },
                    { "glasses_m_17", 100 },
                    { "glasses_m_18", 100 },
                    { "glasses_m_19", 100 },
                    { "glasses_m_20", 100 },
                    { "glasses_m_21", 100 },
                    { "glasses_m_22", 100 },
                    { "glasses_m_23", 100 },

                    { "glasses_f_8", 100 },
                    { "glasses_f_9", 100 },
                    { "glasses_f_10", 100 },
                    { "glasses_f_11", 100 },
                    { "glasses_f_12", 100 },
                    { "glasses_f_13", 100 },
                    { "glasses_f_14", 100 },
                    { "glasses_f_15", 100 },
                    { "glasses_f_16", 100 },
                    { "glasses_f_17", 100 },
                    { "glasses_f_18", 100 },
                    { "glasses_f_19", 100 },
                    { "glasses_f_20", 100 },
                    { "glasses_f_21", 100 },
                    { "glasses_f_22", 100 },
                    { "glasses_f_23", 100 },
                    { "glasses_f_24", 100 },
                    { "glasses_f_25", 100 },

                    { "gloves_m_5", 100 },
                    { "gloves_m_6", 100 },
                    { "gloves_m_7", 100 },
                    { "gloves_m_8", 100 },

                    { "gloves_f_5", 100 },
                    { "gloves_f_6", 100 },
                    { "gloves_f_7", 100 },
                    { "gloves_f_8", 100 },

                    { "bracelet_m_0", 100 },
                    { "bracelet_m_1", 100 },
                    { "bracelet_m_2", 100 },
                    { "bracelet_m_3", 100 },
                    { "bracelet_m_4", 100 },
                    { "bracelet_m_5", 100 },

                    { "bracelet_f_0", 100 },
                    { "bracelet_f_1", 100 },
                    { "bracelet_f_2", 100 },
                    { "bracelet_f_3", 100 },
                    { "bracelet_f_4", 100 },
                    { "bracelet_f_5", 100 },
                    { "bracelet_f_6", 100 },
                    { "bracelet_f_7", 100 },
                    { "bracelet_f_8", 100 },
                    { "bracelet_f_9", 100 },
                    { "bracelet_f_10", 100 },
                    { "bracelet_f_11", 100 },
                    { "bracelet_f_12", 100 },
                }
            },
            #endregion

            {
                Types.Market,

                new Dictionary<string, int>()
                {
                    { "f_burger", 100 },

                    { "med_b_0", 100 },
                    { "med_kit_0", 100 },
                    { "med_kit_1", 100 },

                    { "cigs_0", 100 },
                    { "cigs_1", 100 },
                    { "cig_0", 100 },
                    { "cig_1", 100 },
                }
            },

            {
                Types.GasStation,

                new Dictionary<string, int>()
                {

                }
            },

            {
                Types.CarShop1,

                new Dictionary<string, int>()
                {
                    { "adder", 100 },
                    { "airbus", 100 },
                    { "airtug", 100 },
                    { "alpha", 100 },
                    { "ambulance", 100 },
                    { "apc", 100 },
                    { "ardent", 100 },
                    { "asbo", 100 },
                    { "asea", 100 },
                    { "asea2", 100 },
                    { "asterope", 100 },
                    { "autarch", 100 },
                    { "baller", 100 },
                    { "baller2", 100 },
                    { "baller3", 100 },
                    { "baller4", 100 },
                    { "baller5", 100 },
                    { "baller6", 100 },
                    { "banshee", 100 },
                    { "banshee2", 100 },
                    { "barracks", 100 },
                    { "barracks2", 100 },
                    { "barracks3", 100 },
                    { "barrage", 100 },
                    { "benson", 100 },
                    { "bestiagts", 100 },
                    { "bfinjection", 100 },
                    { "biff", 100 },
                    { "bifta", 100 },
                    { "bison", 100 },
                    { "bison2", 100 },
                    { "bison3", 100 },
                    { "bjxl", 100 },
                    { "blade", 100 },
                    { "blazer", 100 },
                    { "blazer2", 100 },
                    { "blazer3", 100 },
                    { "blazer4", 100 },
                    { "blazer5", 100 },
                    { "blista", 100 },
                    { "blista2", 100 },
                    { "blista3", 100 },
                    { "bobcatxl", 100 },
                    { "bodhi2", 100 },
                    { "boxville", 100 },
                    { "boxville2", 100 },
                    { "boxville3", 100 },
                    { "boxville4", 100 },
                    { "boxville5", 100 },
                    { "brawler", 100 },
                    { "brickade", 100 },
                    { "brioso", 100 },
                    { "brioso2", 100 },
                    { "brioso3", 100 },
                    { "bruiser", 100 },
                    { "bruiser2", 100 },
                    { "bruiser3", 100 },
                    { "brutus", 100 },
                    { "brutus2", 100 },
                    { "brutus3", 100 },
                    { "btype", 100 },
                    { "btype2", 100 },
                    { "btype3", 100 },
                    { "buccaneer", 100 },
                    { "buccaneer2", 100 },
                    { "buffalo", 100 },
                    { "buffalo2", 100 },
                    { "buffalo3", 100 },
                    { "bulldozer", 100 },
                    { "bullet", 100 },
                    { "burrito", 100 },
                    { "burrito2", 100 },
                    { "burrito3", 100 },
                    { "burrito4", 100 },
                    { "burrito5", 100 },
                    { "bus", 100 },
                    { "caddy", 100 },
                    { "caddy2", 100 },
                    { "caddy3", 100 },
                    { "calico", 100 },
                    { "camper", 100 },
                    { "caracara", 100 },
                    { "caracara2", 100 },
                    { "carbonizzare", 100 },
                    { "casco", 100 },
                    { "cavalcade", 100 },
                    { "cavalcade2", 100 },
                    { "cerberus", 100 },
                    { "cerberus2", 100 },
                    { "cerberus3", 100 },
                    { "cheburek", 100 },
                    { "cheetah", 100 },
                    { "cheetah2", 100 },
                    { "chernobog", 100 },
                    { "chino", 100 },
                    { "chino2", 100 },
                    { "clique", 100 },
                    { "club", 100 },
                    { "coach", 100 },
                    { "cog55", 100 },
                    { "cog552", 100 },
                    { "cogcabrio", 100 },
                    { "cognoscenti", 100 },
                    { "cognoscenti2", 100 },
                    { "comet2", 100 },
                    { "comet3", 100 },
                    { "comet4", 100 },
                    { "comet5", 100 },
                    { "comet6", 100 },
                    { "contender", 100 },
                    { "coquette", 100 },
                    { "coquette2", 100 },
                    { "coquette3", 100 },
                    { "coquette4", 100 },
                    { "corsita", 100 },
                    { "crusader", 100 },
                    { "cutter", 100 },
                    { "cyclone", 100 },
                    { "cypher", 100 },
                    { "deluxo", 100 },
                    { "deveste", 100 },
                    { "deviant", 100 },
                    { "dloader", 100 },
                    { "docktug", 100 },
                    { "dominator", 100 },
                    { "dominator2", 100 },
                    { "dominator3", 100 },
                    { "dominator4", 100 },
                    { "dominator5", 100 },
                    { "dominator6", 100 },
                    { "dominator7", 100 },
                    { "dominator8", 100 },
                    { "drafter", 100 },
                    { "draugur", 100 },
                    { "dubsta", 100 },
                    { "dubsta2", 100 },
                    { "dubsta3", 100 },
                    { "dukes", 100 },
                    { "dukes2", 100 },
                    { "dukes3", 100 },
                    { "dump", 100 },
                    { "dune", 100 },
                    { "dune2", 100 },
                    { "dune3", 100 },
                    { "dune4", 100 },
                    { "dune5", 100 },
                    { "dynasty", 100 },
                    { "elegy", 100 },
                    { "elegy2", 100 },
                    { "ellie", 100 },
                    { "emerus", 100 },
                    { "emperor", 100 },
                    { "emperor2", 100 },
                    { "emperor3", 100 },
                    { "entity2", 100 },
                    { "entityxf", 100 },
                    { "euros", 100 },
                    { "everon", 100 },
                    { "exemplar", 100 },
                    { "f620", 100 },
                    { "faction", 100 },
                    { "faction2", 100 },
                    { "faction3", 100 },
                    { "fagaloa", 100 },
                    { "fbi", 100 },
                    { "fbi2", 100 },
                    { "felon", 100 },
                    { "felon2", 100 },
                    { "feltzer2", 100 },
                    { "feltzer3", 100 },
                    { "firetruk", 100 },
                    { "flashgt", 100 },
                    { "flatbed", 100 },
                    { "fmj", 100 },
                    { "forklift", 100 },
                    { "formula", 100 },
                    { "formula2", 100 },
                    { "fq2", 100 },
                    { "freecrawler", 100 },
                    { "fugitive", 100 },
                    { "furia", 100 },
                    { "furoregt", 100 },
                    { "fusilade", 100 },
                    { "futo", 100 },
                    { "futo2", 100 },
                    { "gauntlet", 100 },
                    { "gauntlet2", 100 },
                    { "gauntlet3", 100 },
                    { "gauntlet4", 100 },
                    { "gauntlet5", 100 },
                    { "gb200", 100 },
                    { "gburrito", 100 },
                    { "gburrito2", 100 },
                    { "glendale", 100 },
                    { "glendale2", 100 },
                    { "gp1", 100 },
                    { "granger", 100 },
                    { "greenwood", 100 },
                    { "gresley", 100 },
                    { "growler", 100 },
                    { "gt500", 100 },
                    { "guardian", 100 },
                    { "habanero", 100 },
                    { "halftrack", 100 },
                    { "handler", 100 },
                    { "hauler", 100 },
                    { "hauler2", 100 },
                    { "hellion", 100 },
                    { "hermes", 100 },
                    { "hotknife", 100 },
                    { "hotring", 100 },
                    { "huntley", 100 },
                    { "hustler", 100 },
                    { "imorgon", 100 },
                    { "impaler", 100 },
                    { "impaler2", 100 },
                    { "impaler3", 100 },
                    { "impaler4", 100 },
                    { "imperator", 100 },
                    { "imperator2", 100 },
                    { "imperator3", 100 },
                    { "infernus", 100 },
                    { "infernus2", 100 },
                    { "ingot", 100 },
                    { "insurgent", 100 },
                    { "insurgent2", 100 },
                    { "insurgent3", 100 },
                    { "intruder", 100 },
                    { "issi2", 100 },
                    { "issi3", 100 },
                    { "issi4", 100 },
                    { "issi5", 100 },
                    { "issi6", 100 },
                    { "issi7", 100 },
                    { "italigtb", 100 },
                    { "italigtb2", 100 },
                    { "italigto", 100 },
                    { "italirsx", 100 },
                    { "jackal", 100 },
                    { "jb700", 100 },
                    { "jb7002", 100 },
                    { "jester", 100 },
                    { "jester2", 100 },
                    { "jester3", 100 },
                    { "jester4", 100 },
                    { "journey", 100 },
                    { "jugular", 100 },
                    { "kalahari", 100 },
                    { "kamacho", 100 },
                    { "kanjo", 100 },
                    { "kanjosj", 100 },
                    { "khamelion", 100 },
                    { "khanjali", 100 },
                    { "komoda", 100 },
                    { "krieger", 100 },
                    { "kuruma", 100 },
                    { "kuruma2", 100 },
                    { "landstalker", 100 },
                    { "landstalker2", 100 },
                    { "le7b", 100 },
                    { "lguard", 100 },
                    { "limo2", 100 },
                    { "lm87", 100 },
                    { "locust", 100 },
                    { "lurcher", 100 },
                    { "lynx", 100 },
                    { "mamba", 100 },
                    { "manana", 100 },
                    { "manana2", 100 },
                    { "marshall", 100 },
                    { "massacro", 100 },
                    { "massacro2", 100 },
                    { "menacer", 100 },
                    { "mesa", 100 },
                    { "mesa2", 100 },
                    { "mesa3", 100 },
                    { "michelli", 100 },
                    { "minitank", 100 },
                    { "minivan", 100 },
                    { "minivan2", 100 },
                    { "mixer", 100 },
                    { "mixer2", 100 },
                    { "monroe", 100 },
                    { "monster", 100 },
                    { "monster3", 100 },
                    { "monster4", 100 },
                    { "monster5", 100 },
                    { "moonbeam", 100 },
                    { "moonbeam2", 100 },
                    { "mower", 100 },
                    { "mule", 100 },
                    { "mule2", 100 },
                    { "mule3", 100 },
                    { "mule4", 100 },
                    { "nebula", 100 },
                    { "neo", 100 },
                    { "nero", 100 },
                    { "nero2", 100 },
                    { "nightshade", 100 },
                    { "nightshark", 100 },
                    { "ninef", 100 },
                    { "ninef2", 100 },
                    { "novak", 100 },
                    { "omnis", 100 },
                    { "openwheel1", 100 },
                    { "openwheel2", 100 },
                    { "oracle", 100 },
                    { "oracle2", 100 },
                    { "osiris", 100 },
                    { "outlaw", 100 },
                    { "packer", 100 },
                    { "panto", 100 },
                    { "paradise", 100 },
                    { "paragon", 100 },
                    { "paragon2", 100 },
                    { "pariah", 100 },
                    { "patriot", 100 },
                    { "patriot2", 100 },
                    { "pbus", 100 },
                    { "pbus2", 100 },
                    { "penetrator", 100 },
                    { "penumbra", 100 },
                    { "penumbra2", 100 },
                    { "peyote", 100 },
                    { "peyote2", 100 },
                    { "peyote3", 100 },
                    { "pfister811", 100 },
                    { "phantom", 100 },
                    { "phantom2", 100 },
                    { "phantom3", 100 },
                    { "phoenix", 100 },
                    { "picador", 100 },
                    { "pigalle", 100 },
                    { "police", 100 },
                    { "police2", 100 },
                    { "police3", 100 },
                    { "police4", 100 },
                    { "policeold1", 100 },
                    { "policeold2", 100 },
                    { "policet", 100 },
                    { "pony", 100 },
                    { "pony2", 100 },
                    { "postlude", 100 },
                    { "pounder", 100 },
                    { "pounder2", 100 },
                    { "prairie", 100 },
                    { "pranger", 100 },
                    { "premier", 100 },
                    { "previon", 100 },
                    { "primo", 100 },
                    { "primo2", 100 },
                    { "prototipo", 100 },
                    { "radi", 100 },
                    { "rallytruck", 100 },
                    { "rancherxl", 100 },
                    { "rancherxl2", 100 },
                    { "rapidgt", 100 },
                    { "rapidgt2", 100 },
                    { "rapidgt3", 100 },
                    { "raptor", 100 },
                    { "ratloader", 100 },
                    { "ratloader2", 100 },
                    { "rcbandito", 100 },
                    { "reaper", 100 },
                    { "rebel", 100 },
                    { "rebel2", 100 },
                    { "rebla", 100 },
                    { "regina", 100 },
                    { "remus", 100 },
                    { "rentalbus", 100 },
                    { "retinue", 100 },
                    { "retinue2", 100 },
                    { "revolter", 100 },
                    { "rhapsody", 100 },
                    { "rhinehart", 100 },
                    { "rhino", 100 },
                    { "riata", 100 },
                    { "riot", 100 },
                    { "riot2", 100 },
                    { "ripley", 100 },
                    { "rocoto", 100 },
                    { "romero", 100 },
                    { "rt3000", 100 },
                    { "rubble", 100 },
                    { "ruiner", 100 },
                    { "ruiner2", 100 },
                    { "ruiner3", 100 },
                    { "ruiner4", 100 },
                    { "rumpo", 100 },
                    { "rumpo2", 100 },
                    { "rumpo3", 100 },
                    { "ruston", 100 },
                    { "s80", 100 },
                    { "sabregt", 100 },
                    { "sabregt2", 100 },
                    { "sadler", 100 },
                    { "sadler2", 100 },
                    { "sandking", 100 },
                    { "sandking2", 100 },
                    { "savestra", 100 },
                    { "sc1", 100 },
                    { "scarab", 100 },
                    { "scarab2", 100 },
                    { "scarab3", 100 },
                    { "schafter2", 100 },
                    { "schafter3", 100 },
                    { "schafter4", 100 },
                    { "schafter5", 100 },
                    { "schafter6", 100 },
                    { "schlagen", 100 },
                    { "schwarzer", 100 },
                    { "scramjet", 100 },
                    { "scrap", 100 },
                    { "seminole", 100 },
                    { "seminole2", 100 },
                    { "sentinel", 100 },
                    { "sentinel2", 100 },
                    { "sentinel3", 100 },
                    { "sentinel4", 100 },
                    { "serrano", 100 },
                    { "seven70", 100 },
                    { "sheava", 100 },
                    { "sheriff", 100 },
                    { "sheriff2", 100 },
                    { "slamtruck", 100 },
                    { "slamvan", 100 },
                    { "slamvan2", 100 },
                    { "slamvan3", 100 },
                    { "slamvan4", 100 },
                    { "slamvan5", 100 },
                    { "slamvan6", 100 },
                    { "sm722", 100 },
                    { "specter", 100 },
                    { "specter2", 100 },
                    { "speedo", 100 },
                    { "speedo2", 100 },
                    { "speedo4", 100 },
                    { "squaddie", 100 },
                    { "stafford", 100 },
                    { "stalion", 100 },
                    { "stalion2", 100 },
                    { "stanier", 100 },
                    { "stinger", 100 },
                    { "stingergt", 100 },
                    { "stockade", 100 },
                    { "stockade3", 100 },
                    { "stratum", 100 },
                    { "streiter", 100 },
                    { "stretch", 100 },
                    { "stromberg", 100 },
                    { "sugoi", 100 },
                    { "sultan", 100 },
                    { "sultan2", 100 },
                    { "sultan3", 100 },
                    { "sultanrs", 100 },
                    { "superd", 100 },
                    { "surano", 100 },
                    { "surfer", 100 },
                    { "surfer2", 100 },
                    { "swinger", 100 },
                    { "t20", 100 },
                    { "taco", 100 },
                    { "tailgater", 100 },
                    { "tailgater2", 100 },
                    { "taipan", 100 },
                    { "tampa", 100 },
                    { "tampa2", 100 },
                    { "tampa3", 100 },
                    { "taxi", 100 },
                    { "technical", 100 },
                    { "technical2", 100 },
                    { "technical3", 100 },
                    { "tempesta", 100 },
                    { "tenf", 100 },
                    { "tenf2", 100 },
                    { "terbyte", 100 },
                    { "tezeract", 100 },
                    { "thrax", 100 },
                    { "tigon", 100 },
                    { "tiptruck", 100 },
                    { "tiptruck2", 100 },
                    { "toreador", 100 },
                    { "torero", 100 },
                    { "torero2", 100 },
                    { "tornado", 100 },
                    { "tornado2", 100 },
                    { "tornado3", 100 },
                    { "tornado4", 100 },
                    { "tornado5", 100 },
                    { "tornado6", 100 },
                    { "toros", 100 },
                    { "tourbus", 100 },
                    { "towtruck", 100 },
                    { "towtruck2", 100 },
                    { "tractor", 100 },
                    { "tractor2", 100 },
                    { "tractor3", 100 },
                    { "trash", 100 },
                    { "trash2", 100 },
                    { "trophytruck", 100 },
                    { "trophytruck2", 100 },
                    { "tropos", 100 },
                    { "tulip", 100 },
                    { "turismo2", 100 },
                    { "turismor", 100 },
                    { "tyrant", 100 },
                    { "tyrus", 100 },
                    { "utillitruck", 100 },
                    { "utillitruck2", 100 },
                    { "utillitruck3", 100 },
                    { "vacca", 100 },
                    { "vagner", 100 },
                    { "vagrant", 100 },
                    { "vamos", 100 },
                    { "vectre", 100 },
                    { "verlierer2", 100 },
                    { "verus", 100 },
                    { "vetir", 100 },
                    { "veto", 100 },
                    { "veto2", 100 },
                    { "vigero", 100 },
                    { "vigero2", 100 },
                    { "vigilante", 100 },
                    { "virgo", 100 },
                    { "virgo2", 100 },
                    { "virgo3", 100 },
                    { "viseris", 100 },
                    { "visione", 100 },
                    { "voodoo", 100 },
                    { "voodoo2", 100 },
                    { "vstr", 100 },
                    { "warrener", 100 },
                    { "washington", 100 },
                    { "wastelander", 100 },
                    { "weevil", 100 },
                    { "weevil2", 100 },
                    { "windsor", 100 },
                    { "windsor2", 100 },
                    { "winky", 100 },
                    { "xa21", 100 },
                    { "xls", 100 },
                    { "xls2", 100 },
                    { "yosemite", 100 },
                    { "yosemite2", 100 },
                    { "yosemite3", 100 },
                    { "youga", 100 },
                    { "youga2", 100 },
                    { "youga3", 100 },
                    { "z190", 100 },
                    { "zentorno", 100 },
                    { "zhaba", 100 },
                    { "zion", 100 },
                    { "zion2", 100 },
                    { "zion3", 100 },
                    { "zorrusso", 100 },
                    { "zr350", 100 },
                    { "zr380", 100 },
                    { "zr3802", 100 },
                    { "zr3803", 100 },
                    { "ztype", 100 },
                    { "dilettante", 100 },
                    { "dilettante2", 100 },
                    { "neon", 100 },
                    { "omnisegt", 100 },
                    { "raiden", 100 },
                    { "surge", 100 },
                    { "voltic", 100 },
                    { "voltic2", 100 },
                }
            },

            {
                Types.CarShop2,

                new Dictionary<string, int>()
                {

                }
            },

            {
                Types.CarShop3,

                new Dictionary<string, int>()
                {

                }
            },

            {
                Types.MotoShop,

                new Dictionary<string, int>()
                {
                    { "akuma", 100 },
                    { "avarus", 100 },
                    { "bagger", 100 },
                    { "bati", 100 },
                    { "bati2", 100 },
                    { "bf400", 100 },
                    { "carbonrs", 100 },
                    { "chimera", 100 },
                    { "cliffhanger", 100 },
                    { "daemon", 100 },
                    { "daemon2", 100 },
                    { "deathbike", 100 },
                    { "deathbike2", 100 },
                    { "deathbike3", 100 },
                    { "defiler", 100 },
                    { "diablous", 100 },
                    { "diablous2", 100 },
                    { "double", 100 },
                    { "enduro", 100 },
                    { "esskey", 100 },
                    { "faggio", 100 },
                    { "faggio2", 100 },
                    { "faggio3", 100 },
                    { "fcr", 100 },
                    { "fcr2", 100 },
                    { "gargoyle", 100 },
                    { "hakuchou", 100 },
                    { "hakuchou2", 100 },
                    { "hexer", 100 },
                    { "innovation", 100 },
                    { "lectro", 100 },
                    { "manchez", 100 },
                    { "manchez2", 100 },
                    { "nemesis", 100 },
                    { "nightblade", 100 },
                    { "oppressor", 100 },
                    { "oppressor2", 100 },
                    { "pcj", 100 },
                    { "ratbike", 100 },
                    { "ruffian", 100 },
                    { "rrocket", 100 },
                    { "sanchez", 100 },
                    { "sanchez2", 100 },
                    { "sanctus", 100 },
                    { "shotaro", 100 },
                    { "sovereign", 100 },
                    { "stryder", 100 },
                    { "thrust", 100 },
                    { "vader", 100 },
                    { "vindicator", 100 },
                    { "vortex", 100 },
                    { "wolfsbane", 100 },
                    { "zombiea", 100 },
                    { "zombieb", 100 },
                    { "policeb", 100 },
                    { "bmx", 100 },
                    { "cruiser", 100 },
                    { "fixter", 100 },
                    { "scorcher", 100 },
                    { "tribike", 100 },
                    { "tribike2", 100 },
                    { "tribike3", 100 },
                }
            },

            {
                Types.BoatShop,

                new Dictionary<string, int>()
                {
                    { "avisa", 100 },
                    { "dinghy", 100 },
                    { "dinghy2", 100 },
                    { "dinghy3", 100 },
                    { "dinghy4", 100 },
                    { "dinghy5", 100 },
                    { "jetmax", 100 },
                    { "kosatka", 100 },
                    { "longfin", 100 },
                    { "marquis", 100 },
                    { "patrolboat", 100 },
                    { "predator", 100 },
                    { "seashark", 100 },
                    { "seashark2", 100 },
                    { "seashark3", 100 },
                    { "speeder", 100 },
                    { "speeder2", 100 },
                    { "squalo", 100 },
                    { "submersible", 100 },
                    { "submersible2", 100 },
                    { "suntrap", 100 },
                    { "toro", 100 },
                    { "toro2", 100 },
                    { "tropic", 100 },
                    { "tropic2", 100 },
                    { "tug", 100 },
                }
            },

            {
                Types.AeroShop,

                new Dictionary<string, int>()
                {
                    { "akula", 100 },
                    { "annihilator", 100 },
                    { "annihilator2", 100 },
                    { "buzzard", 100 },
                    { "buzzard2", 100 },
                    { "cargobob", 100 },
                    { "cargobob2", 100 },
                    { "cargobob3", 100 },
                    { "cargobob4", 100 },
                    { "conada", 100 },
                    { "frogger", 100 },
                    { "frogger2", 100 },
                    { "havok", 100 },
                    { "hunter", 100 },
                    { "maverick", 100 },
                    { "savage", 100 },
                    { "seasparrow", 100 },
                    { "seasparrow2", 100 },
                    { "seasparrow3", 100 },
                    { "skylift", 100 },
                    { "supervolito", 100 },
                    { "supervolito2", 100 },
                    { "swift", 100 },
                    { "swift2", 100 },
                    { "valkyrie", 100 },
                    { "valkyrie2", 100 },
                    { "volatus", 100 },
                    { "polmav", 100 },
                    { "alphaz1", 100 },
                    { "avenger", 100 },
                    { "avenger2", 100 },
                    { "besra", 100 },
                    { "bombushka", 100 },
                    { "cargoplane", 100 },
                    { "cuban800", 100 },
                    { "dodo", 100 },
                    { "duster", 100 },
                    { "howard", 100 },
                    { "hydra", 100 },
                    { "jet", 100 },
                    { "lazer", 100 },
                    { "luxor", 100 },
                    { "luxor2", 100 },
                    { "mammatus", 100 },
                    { "microlight", 100 },
                    { "miljet", 100 },
                    { "mogul", 100 },
                    { "molotok", 100 },
                    { "nimbus", 100 },
                    { "nokota", 100 },
                    { "pyro", 100 },
                    { "rogue", 100 },
                    { "seabreeze", 100 },
                    { "shamal", 100 },
                    { "starling", 100 },
                    { "strikeforce", 100 },
                    { "stunt", 100 },
                    { "titan", 100 },
                    { "tula", 100 },
                    { "velum", 100 },
                    { "velum2", 100 },
                    { "vestra", 100 },
                    { "volatol", 100 },
                    { "alkonost", 100 },
                }
            },
        };

        public int GetPrice(string id, bool addMargin = true)
        {
            int price;

            if (id == null || !AllPrices[Type].TryGetValue(id, out price))
                return -1;

            if (addMargin)
                return (int)Math.Floor(price * Margin);
            else
                return price;
        }

        public Shop(int ID, Vector3 Position, Types Type) : base(ID, Position, Type)
        {

        }
    }

    public abstract class ClothesShop : Shop, IEnterable
    {
        public Vector3 EnterPosition { get; set; }

        public float Heading { get; set; }

        public ClothesShop(int ID, Vector3 Position, Vector3 EnterPosition, float Heading, Types Type) : base(ID, Position, Type)
        {
            this.EnterPosition = EnterPosition;

            this.Heading = Heading;
        }
    }

    public class ClothesShop1 : ClothesShop
    {
        private static int Counter = 1;

        public ClothesShop1(int ID, Vector3 Position, Vector3 EnterPosition, float Heading) : base(ID, Position, EnterPosition, Heading, Types.ClothesShop1)
        {
            SubID = Counter++;
        }
    }

    public class ClothesShop2 : ClothesShop
    {
        private static int Counter = 1;

        public ClothesShop2(int ID, Vector3 Position, Vector3 EnterPosition, float Heading) : base(ID, Position, EnterPosition, Heading, Types.ClothesShop2)
        {
            SubID = Counter++;
        }
    }

    public class ClothesShop3 : ClothesShop
    {
        private static int Counter = 1;

        public ClothesShop3(int ID, Vector3 Position, Vector3 EnterPosition, float Heading) : base(ID, Position, EnterPosition, Heading, Types.ClothesShop2)
        {
            SubID = Counter++;
        }
    }

    public class Market : Shop
    {
        private static int Counter = 1;

        public Market(int ID, Vector3 Position) : base(ID, Position, Types.Market)
        {
            SubID = Counter++;
        }
    }

    public class GasStation : Shop
    {
        private static int Counter = 1;

        private static Dictionary<Game.Data.Vehicles.Vehicle.FuelTypes, int> GasPrices = new Dictionary<Game.Data.Vehicles.Vehicle.FuelTypes, int>()
        {
            { Game.Data.Vehicles.Vehicle.FuelTypes.Petrol, 10 },

            { Game.Data.Vehicles.Vehicle.FuelTypes.Electricity, 5 },
        };

        public Vector3 GasolinesPosition { get; set; }

        public int GetGasPrice(Game.Data.Vehicles.Vehicle.FuelTypes fType, bool addMargin)
        {
            int price;

            if (!GasPrices.TryGetValue(fType, out price))
                return -1;

            if (addMargin)
                return (int)Math.Floor(price * Margin);
            else
                return price;
        }

        public GasStation(int ID, Vector3 Position, Vector3 GasolinesPosition) : base(ID, Position, Types.GasStation)
        {
            this.GasolinesPosition = GasolinesPosition;

            SubID = Counter++;
        }
    }

    public abstract class VehicleShop : Shop, IEnterable
    {
        public Vector3 EnterPosition { get; set; }

        public float Heading { get; set; }

        public VehicleShop(int ID, Vector3 Position, Vector3 EnterPosition, float Heading, Types Type) : base(ID, Position, Type)
        {
            this.EnterPosition = EnterPosition;
            this.Heading = Heading;
        }
    }

    public class CarShop1 : VehicleShop
    {
        public CarShop1(int ID, Vector3 Position, Vector3 EnterPosition, float Heading) : base(ID, Position, EnterPosition, Heading, Types.CarShop1)
        {

        }
    }

    public class CarShop2 : VehicleShop
    {
        public CarShop2(int ID, Vector3 Position, Vector3 EnterPosition, float Heading) : base(ID, Position, EnterPosition, Heading, Types.CarShop2)
        {

        }
    }

    public class CarShop3 : VehicleShop
    {
        public CarShop3(int ID, Vector3 Position, Vector3 EnterPosition, float Heading) : base(ID, Position, EnterPosition, Heading, Types.CarShop3)
        {

        }
    }

    public class MotoShop : VehicleShop
    {
        public MotoShop(int ID, Vector3 Position, Vector3 EnterPosition, float Heading) : base(ID, Position, EnterPosition, Heading, Types.MotoShop)
        {

        }
    }

    public class BoatShop : VehicleShop
    {
        public BoatShop(int ID, Vector3 Position, Vector3 EnterPosition, float Heading) : base(ID, Position, EnterPosition, Heading, Types.BoatShop)
        {

        }
    }

    public class AeroShop : VehicleShop
    {
        public AeroShop(int ID, Vector3 Position, Vector3 EnterPosition, float Heading) : base(ID, Position, EnterPosition, Heading, Types.AeroShop)
        {

        }
    }

    /*    public class Masks : Business
        {
            public static Dictionary<string, int> Prices = new Dictionary<string, int>()
            {

            };

            private static int Counter = 1;

            public Masks(int ID, Vector3 Position, Vector3 PositionInfo, Vector3 PositionEnter) : base(ID, PositionInfo)
            {
                Blip = NAPI.Blip.CreateBlip(362, Position, 1f, 5, "", 255, 0, true);

                SubID = Counter++;
            }
        }

        public class Bags : Business
        {
            public static Dictionary<string, int> Prices = new Dictionary<string, int>()
            {

            };

            private static int Counter = 1;

            public Bags(int ID, Vector3 Position, Vector3 PositionInfo, Vector3 PositionEnter) : base(ID, PositionInfo, PositionEnter)
            {
                Blip = NAPI.Blip.CreateBlip(676, Position, 1f, 5, "", 255, 0, true);

                SubID = Counter++;
            }
        }

        public class Jewellery : Business
        {
            public static Dictionary<string, int> Prices = new Dictionary<string, int>()
            {

            };

            private static int Counter = 1;

            public Jewellery(int ID, Vector3 Position, Vector3 PositionInfo, Vector3 PositionEnter) : base(ID, PositionInfo, PositionEnter)
            {
                Blip = NAPI.Blip.CreateBlip(617, Position, 1f, 5, "", 255, 0, true);

                SubID = Counter++;
            }
        }

        public class Shop : Business
        {
            public static Dictionary<string, int> Prices = new Dictionary<string, int>()
            {

            };

            private static int Counter = 1;

            public Shop(int ID, Vector3 Position, Vector3 PositionInfo, Vector3 PositionEnter) : base(ID, PositionInfo, PositionEnter)
            {
                Blip = NAPI.Blip.CreateBlip(11, Position, 1f, 5, "", 255, 0, true);

                SubID = Counter++;
            }
        }

        public class WeaponShop : Business
        {
            public static Dictionary<string, int> Prices = new Dictionary<string, int>()
            {

            };

            private static int Counter = 1;

            public WeaponShop(int ID, Vector3 Position, Vector3 PositionInfo, Vector3 PositionEnter) : base(ID, PositionInfo, PositionEnter)
            {
                Blip = NAPI.Blip.CreateBlip(110, Position, 1f, 5, "", 255, 0, true);

                SubID = Counter++;
            }
        }

        public class Petrol : Business
        {
            public static Dictionary<string, int> Prices = new Dictionary<string, int>()
            {

            };

            private static int Counter = 1;

            public Petrol(int ID, Vector3 Position, Vector3 PositionInfo, Vector3 PositionEnter) : base(ID, PositionInfo, PositionEnter)
            {
                Blip = NAPI.Blip.CreateBlip(361, Position, 1f, 5, "", 255, 0, true);

                SubID = Counter++;
            }
        }

        public class Farm : Business
        {
            public Farm(int ID, Vector3 PositionInfo, Vector3 Position) : base(ID, PositionInfo, Position)
            {
                Blip = NAPI.Blip.CreateBlip(73, PositionInfo, 1f, 0, "");

            }
        }*/
}
