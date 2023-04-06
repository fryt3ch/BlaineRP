using GTANetworkAPI;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BCRPServer.Game.Estates
{
    public class Furniture
    {
        public static Dictionary<uint, Furniture> All { get; private set; } = new Dictionary<uint, Furniture>();

        public static UidHandlerUInt32 UidHandler { get; private set; } = new UidHandlerUInt32(1);

        public static void AddOnLoad(Furniture f)
        {
            if (f == null)
                return;

            All.Add(f.UID, f);

            UidHandler.TryUpdateLastAddedMaxUid(f.UID);
        }

        public static void Add(Furniture f)
        {
            if (f == null)
                return;

            f.UID = UidHandler.MoveNextUid();

            All.Add(f.UID, f);

            MySQL.FurnitureAdd(f);
        }

        public static void Remove(Furniture f)
        {
            if (f == null)
                return;

            var id = f.UID;

            UidHandler.SetUidAsFree(id);

            All.Remove(id);

            MySQL.FurnitureDelete(f);
        }

        public static Furniture Get(uint id) => All.GetValueOrDefault(id);

        public class ItemData
        {
            public static Dictionary<string, ItemData> AllData { get; private set; } = new Dictionary<string, ItemData>();

            private static Dictionary<Types, Game.Items.Craft.Workbench.WorkbenchTypes?> WorkbenchTypes { get; set; } = new Dictionary<Types, Items.Craft.Workbench.WorkbenchTypes?>()
            {
                { Types.KitchenSet, Game.Items.Craft.Workbench.WorkbenchTypes.KitchenSet },
            };

            public enum Types
            {
                Chair = 0,
                Bed,
                Table,

                Locker,
                Wardrobe,
                Fridge,

                Plant,
                Lamp,
                TV,
                Electronics,

                KitchenSet,

                KitchenStuff,

                Bath,
                Toilet,
                Painting,
                Decor,

                Washer,

                BathStuff,

                Workbench,
                Storage,
                PC,
            }

            public Types Type { get; private set; }

            public string Name { get; private set; }

            public uint Model { get; private set; }

            public Game.Items.Craft.Workbench.WorkbenchTypes? WorkbenchType => WorkbenchTypes.GetValueOrDefault(Type);

            public ItemData(string Id, Types Type, string Name, string Model)
            {
                this.Type = Type;

                this.Name = Name;

                this.Model = NAPI.Util.GetHashKey(Model);

                AllData.Add(Id, this);
            }

            public static ItemData Get(string id) => AllData.GetValueOrDefault(id);

            public static int LoadAll()
            {
                new ItemData("furn_91", Types.Chair, "Позднеренессансное кресло", "apa_mp_h_stn_chairarm_01");
                new ItemData("furn_96", Types.Chair, "Кресло в стиле модерн", "apa_mp_h_stn_chairarm_12");
                new ItemData("furn_98", Types.Chair, "Раскладное кресло (малиновое)", "apa_mp_h_stn_chairarm_23");
                new ItemData("furn_99", Types.Chair, "Кресло обычное", "apa_mp_h_stn_chairarm_24");
                new ItemData("furn_100", Types.Chair, "Пуфик оранжевый", "apa_mp_h_stn_chairarm_25");
                new ItemData("furn_101", Types.Chair, "Современное кресло (фиолотовое)", "apa_mp_h_stn_chairarm_26");
                new ItemData("furn_103", Types.Chair, "Раскладное кресло (оранжевое)", "apa_mp_h_stn_chairstrip_01");
                new ItemData("furn_104", Types.Chair, "Позднеренессансное кресло (оранжевое)", "apa_mp_h_stn_chairstrip_02");
                new ItemData("furn_105", Types.Chair, "Раскладное кресло (вишневое)", "apa_mp_h_stn_chairstrip_03");
                new ItemData("furn_106", Types.Chair, "Раскладное кресло (бордовое)", "apa_mp_h_stn_chairstrip_04");
                new ItemData("furn_107", Types.Chair, "Раскладное кресло (белое)", "apa_mp_h_stn_chairstrip_05");
                new ItemData("furn_108", Types.Chair, "Позднеренессансное кресло (ярко оранжевое)", "apa_mp_h_stn_chairstrip_06");
                new ItemData("furn_109", Types.Chair, "Позднеренессансное кресло (черное)", "apa_mp_h_stn_chairstrip_07");
                new ItemData("furn_110", Types.Chair, "Позднеренессансное кресло (бирюзовое)", "apa_mp_h_stn_chairstrip_08");
                new ItemData("furn_147", Types.Chair, "Раскладное кресло (кремовое)", "apa_mp_h_yacht_armchair_01");
                new ItemData("furn_148", Types.Chair, "Дизайнерское кресло", "apa_mp_h_yacht_armchair_03");
                new ItemData("furn_149", Types.Chair, "Кресло низкое (коричневое)", "apa_mp_h_yacht_armchair_04");
                new ItemData("furn_157", Types.Chair, "Раскладное кресло (снежное)", "apa_mp_h_yacht_strip_chair_01");
                new ItemData("furn_229", Types.Chair, "Кресло босса", "ba_prop_battle_club_chair_02");
                new ItemData("furn_230", Types.Chair, "Кресло офисное серое", "ba_prop_battle_club_chair_03");
                new ItemData("furn_239", Types.Chair, "Раскладное кресло (коричневое)", "bkr_prop_biker_chairstrip_01");
                new ItemData("furn_240", Types.Chair, "Кресло старое", "bkr_prop_biker_chairstrip_02");
                new ItemData("furn_284", Types.Chair, "Кресло кожаное", "ex_prop_offchair_exec_04");
                new ItemData("furn_289", Types.Chair, "Кресло кухонное (линии)", "gr_dlc_gr_yacht_props_seat_02");
                new ItemData("furn_328", Types.Chair, "Позднеренессансное кресло (пестрое)", "hei_heist_stn_chairarm_01");
                new ItemData("furn_329", Types.Chair, "Кресло элегантное", "hei_heist_stn_chairarm_03");
                new ItemData("furn_330", Types.Chair, "Кресло в стиле модерн (сливовое)", "hei_heist_stn_chairarm_04");
                new ItemData("furn_331", Types.Chair, "Кресло крапо", "hei_heist_stn_chairarm_06");
                new ItemData("furn_332", Types.Chair, "Позднеренессансное кресло (апельсиновое)", "hei_heist_stn_chairstrip_01");
                new ItemData("furn_367", Types.Chair, "Коресло офисное", "imp_prop_impexp_offchair_01a");
                new ItemData("furn_563", Types.Chair, "Кресло босса черное", "prop_sol_chair");
                new ItemData("furn_637", Types.Chair, "Кресло кухонное( желтое)", "prop_yaught_chair_01");
                new ItemData("furn_639", Types.Chair, "Кресло модное (белое)", "p_armchair_01_s");
                new ItemData("furn_642", Types.Chair, "Кресло стильное (серое)", "p_ilev_p_easychair_s");
                new ItemData("furn_652", Types.Chair, "Кресло руководителя", "p_soloffchair_s");
                new ItemData("furn_659", Types.Chair, "Кресло руководителя черное", "sm_prop_offchair_smug_01");
                new ItemData("furn_663", Types.Chair, "Кресло парикмахерское", "v_ilev_hd_chair");
                new ItemData("furn_664", Types.Chair, "Кресло руководителя кожаное", "v_ilev_leath_chr");
                new ItemData("furn_693", Types.Chair, "Кресло в стиле модерн (черное)", "xm_lab_chairarm_12");
                new ItemData("furn_695", Types.Chair, "Современное кресло (серое)", "xm_lab_chairarm_26");

                new ItemData("furn_57", Types.Chair, "Стул белый офисный", "apa_mp_h_din_chair_04");
                new ItemData("furn_58", Types.Chair, "Стул белый офисный №2", "apa_mp_h_din_chair_08");
                new ItemData("furn_59", Types.Chair, "Стул Карлос", "apa_mp_h_din_chair_09");
                new ItemData("furn_60", Types.Chair, "Стул деревянный", "apa_mp_h_din_chair_12");
                new ItemData("furn_92", Types.Chair, "Стул история", "apa_mp_h_stn_chairarm_02");
                new ItemData("furn_93", Types.Chair, "Стул Самба", "apa_mp_h_stn_chairarm_03");
                new ItemData("furn_94", Types.Chair, "Стул круговой", "apa_mp_h_stn_chairarm_09");
                new ItemData("furn_95", Types.Chair, "Стул портфино", "apa_mp_h_stn_chairarm_11");
                new ItemData("furn_97", Types.Chair, "Стул компьютерный", "apa_mp_h_stn_chairarm_13");
                new ItemData("furn_102", Types.Chair, "Стул дизайнерский", "apa_mp_h_stn_chairstool_12");
                new ItemData("furn_150", Types.Chair, "Стул клубный", "apa_mp_h_yacht_barstool_01");
                new ItemData("furn_234", Types.Chair, "Стул клубный №2", "bkr_prop_biker_barstool_02");
                new ItemData("furn_235", Types.Chair, "Стул барный", "bkr_prop_biker_barstool_03");
                new ItemData("furn_236", Types.Chair, "Стул барный №2", "bkr_prop_biker_barstool_04");
                new ItemData("furn_237", Types.Chair, "Стул офисный", "bkr_prop_biker_boardchair01");
                new ItemData("furn_241", Types.Chair, "Стул деревянный №2", "bkr_prop_biker_chair_01");
                new ItemData("furn_245", Types.Chair, "Стул старый", "bkr_prop_clubhouse_arm_wrestle_01a");
                new ItemData("furn_247", Types.Chair, "Стул клубный", "bkr_prop_clubhouse_chair_01");
                new ItemData("furn_252", Types.Chair, "Стул компьютерный старый", "bkr_prop_clubhouse_offchair_01a");
                new ItemData("furn_256", Types.Chair, "Стул Магнат", "bkr_prop_weed_chair_01a");
                new ItemData("furn_259", Types.Chair, "Стул роскошный красный", "cls_h4_int_04_desk_chair");
                new ItemData("furn_282", Types.Chair, "Стул офисный деловой", "ex_prop_offchair_exec_01");
                new ItemData("furn_283", Types.Chair, "Стул офисный №2 (черный)", "ex_prop_offchair_exec_03");
                new ItemData("furn_288", Types.Chair, "Стул кухонный (в полоску)", "gr_dlc_gr_yacht_props_seat_01");
                new ItemData("furn_364", Types.Chair, "Стул для отдыха", "hei_prop_hei_skid_chair");
                new ItemData("furn_373", Types.Chair, "Стул старый вязаный", "prop_armchair_01");
                new ItemData("furn_391", Types.Chair, "Стул барный №3", "prop_bar_stool_01");
                new ItemData("furn_406", Types.Chair, "Стул открытый", "prop_chair_01a");
                new ItemData("furn_407", Types.Chair, "Стул открытый №2", "prop_chair_01b");
                new ItemData("furn_408", Types.Chair, "Стул Виктор", "prop_chair_02");
                new ItemData("furn_409", Types.Chair, "Стул греческий", "prop_chair_03");
                new ItemData("furn_410", Types.Chair, "Стул с тигровым принтом", "prop_chair_04a");
                new ItemData("furn_411", Types.Chair, "Стул динко (белый)", "prop_chair_04b");
                new ItemData("furn_412", Types.Chair, "Стул кухонный (старый)", "prop_chair_05");
                new ItemData("furn_413", Types.Chair, "Стул столовый", "prop_chair_06");
                new ItemData("furn_414", Types.Chair, "Стул классический", "prop_chair_07");
                new ItemData("furn_415", Types.Chair, "Стул пластмассовый", "prop_chair_08");
                new ItemData("furn_416", Types.Chair, "Стул садовый", "prop_chair_09");
                new ItemData("furn_417", Types.Chair, "Стул садовый №2", "prop_chair_10");
                new ItemData("furn_418", Types.Chair, "Стулья пластмассовые ", "prop_chair_pile_01");
                new ItemData("furn_421", Types.Chair, "Стул Chateau", "prop_chateau_chair_01");
                new ItemData("furn_494", Types.Chair, "Стул офисный №3", "prop_off_chair_04");
                new ItemData("furn_495", Types.Chair, "Стул офисный №4", "prop_off_chair_04_s");
                new ItemData("furn_496", Types.Chair, "Стул офисный №5", "prop_off_chair_05");
                new ItemData("furn_578", Types.Chair, "Стул деревянный №2", "prop_table_01_chr_a");
                new ItemData("furn_580", Types.Chair, "Стул деревяный №3", "prop_table_02_chr");
                new ItemData("furn_582", Types.Chair, "Стул пластмассовый №2", "prop_table_03_chr");
                new ItemData("furn_584", Types.Chair, "Стул садовый №3", "prop_table_04_chr");
                new ItemData("furn_586", Types.Chair, "Стул в переплете", "prop_table_05_chr");
                new ItemData("furn_588", Types.Chair, "Стул Генрих", "prop_table_06_chr");
                new ItemData("furn_622", Types.Chair, "Стул кожаный", "prop_waiting_seat_01");
                new ItemData("furn_641", Types.Chair, "Стул Dine", "p_dinechair_01_s");
                new ItemData("furn_680", Types.Chair, "Стул колониальный", "v_res_m_l_chair1");
                new ItemData("furn_687", Types.Chair, "Стул колониальный красный", "v_ret_chair");
                new ItemData("furn_688", Types.Chair, "Стул колониальный белый", "v_ret_chair_white");
                new ItemData("furn_690", Types.Chair, "Стул Лициор", "xm_lab_chairarm_02");
                new ItemData("furn_691", Types.Chair, "Стул рико", "xm_lab_chairarm_03");
                new ItemData("furn_692", Types.Chair, "Стул рико №2", "xm_lab_chairarm_11");
                new ItemData("furn_694", Types.Chair, "Стул офисный дизайнерский", "xm_lab_chairarm_24");

                new ItemData("furn_52", Types.Bed, "Кровать двуспальная", "apa_mp_h_bed_double_08");
                new ItemData("furn_53", Types.Bed, "Кровать двуспальная на платформе", "apa_mp_h_bed_double_09");
                new ItemData("furn_55", Types.Bed, "Кровать двуспальная на красной платформе", "apa_mp_h_bed_wide_05");
                new ItemData("furn_56", Types.Bed, "Кровать двуспальная современная", "apa_mp_h_bed_with_table_02");
                new ItemData("furn_111", Types.Bed, "Диван обычный с подушками", "apa_mp_h_stn_sofa2seat_02");
                new ItemData("furn_112", Types.Bed, "Угловой серый диван", "apa_mp_h_stn_sofacorn_01");
                new ItemData("furn_113", Types.Bed, "Угловой серый дизайнерский диван", "apa_mp_h_stn_sofacorn_05");
                new ItemData("furn_114", Types.Bed, "Угловоый зелёный диван", "apa_mp_h_stn_sofacorn_06");
                new ItemData("furn_115", Types.Bed, "Угловой неоновый диван", "apa_mp_h_stn_sofacorn_07");
                new ItemData("furn_116", Types.Bed, "Угловой диван цвета какао", "apa_mp_h_stn_sofacorn_08");
                new ItemData("furn_117", Types.Bed, "Угловой строгий диван", "apa_mp_h_stn_sofacorn_09");
                new ItemData("furn_118", Types.Bed, "Угловой белый диван", "apa_mp_h_stn_sofacorn_10");
                new ItemData("furn_119", Types.Bed, "Лежак коричневый", "apa_mp_h_stn_sofa_daybed_01");
                new ItemData("furn_120", Types.Bed, "Лежак черный", "apa_mp_h_stn_sofa_daybed_02");
                new ItemData("furn_151", Types.Bed, "Кроваь двуспальная стильная", "apa_mp_h_yacht_bed_01");
                new ItemData("furn_152", Types.Bed, "Кровать двуспальная дизайнерская", "apa_mp_h_yacht_bed_02");
                new ItemData("furn_155", Types.Bed, "Диван большой кремовый", "apa_mp_h_yacht_sofa_01");
                new ItemData("furn_156", Types.Bed, "Диван с низкой посадкой (коричневый)", "apa_mp_h_yacht_sofa_02");
                new ItemData("furn_238", Types.Bed, "Лежак байкерский", "bkr_prop_biker_campbed_01");
                new ItemData("furn_253", Types.Bed, "Диван дизайнерский (черный)", "bkr_prop_clubhouse_sofa_01a");
                new ItemData("furn_287", Types.Bed, "Лежак пляжный", "gr_dlc_gr_yacht_props_lounger");
                new ItemData("furn_290", Types.Bed, "Диван кухонный (в полоску)", "gr_dlc_gr_yacht_props_seat_03");
                new ItemData("furn_294", Types.Bed, "Кровать обычная", "gr_prop_bunker_bed_01");
                new ItemData("furn_333", Types.Bed, "Диван с платформой", "hei_heist_stn_sofa2seat_02");
                new ItemData("furn_334", Types.Bed, "Диван обычный (коричневый)", "hei_heist_stn_sofa2seat_03");
                new ItemData("furn_335", Types.Bed, "Угловой лазурный диван", "hei_heist_stn_sofacorn_05");
                new ItemData("furn_336", Types.Bed, "Угловой салатовый диван", "hei_heist_stn_sofacorn_06");
                new ItemData("furn_369", Types.Bed, "Диван старый", "miss_rub_couch_01_l1");
                new ItemData("furn_428", Types.Bed, "Диван в стиле лофт", "prop_couch_01");
                new ItemData("furn_429", Types.Bed, "Диван классический", "prop_couch_03");
                new ItemData("furn_430", Types.Bed, "Диван в стиле лофт (№2)", "prop_couch_04");
                new ItemData("furn_431", Types.Bed, "Диван в стиле прованс (коричневый)", "prop_couch_lg_02");
                new ItemData("furn_432", Types.Bed, "Диван семейный", "prop_couch_lg_05");
                new ItemData("furn_433", Types.Bed, "Диван в стиле лофт (коричневый)", "prop_couch_lg_06");
                new ItemData("furn_434", Types.Bed, "Диван в стиле прованс (белый)", "prop_couch_lg_07");
                new ItemData("furn_435", Types.Bed, "Диван современный", "prop_couch_lg_08");
                new ItemData("furn_436", Types.Bed, "Дополнение к современному дивану (№1)", "prop_couch_sm1_07");
                new ItemData("furn_437", Types.Bed, "Дополнение к современному дивану (№2)", "prop_couch_sm2_07");
                new ItemData("furn_438", Types.Bed, "Диван в стиле лофт (кофейный)", "prop_couch_sm_06");
                new ItemData("furn_497", Types.Bed, "Лежак пляжный (деревянный)", "prop_patio_lounger1");
                new ItemData("furn_498", Types.Bed, "Лежак пляжный (камуфляж)", "prop_patio_lounger_2");
                new ItemData("furn_499", Types.Bed, "Лежак пляжный (пластмассовый)", "prop_patio_lounger_3");
                new ItemData("furn_554", Types.Bed, "Диван старый в стиле лофт", "prop_rub_couch01");
                new ItemData("furn_555", Types.Bed, "Диван старый (№2)", "prop_rub_couch02");
                new ItemData("furn_556", Types.Bed, "Диван старый (№3)", "prop_rub_couch04");
                new ItemData("furn_615", Types.Bed, "Кровать современная на платформе", "prop_t_sofa");
                new ItemData("furn_616", Types.Bed, "Кровать современная на платформе №2", "prop_t_sofa_02");
                new ItemData("furn_623", Types.Bed, "Диван кожаный без спинки (черный)", "prop_wait_bench_01");
                new ItemData("furn_638", Types.Bed, "Диван кухонный (желтый)", "prop_yaught_sofa_01");
                new ItemData("furn_646", Types.Bed, "Кровать застеленная", "p_lestersbed_s");
                new ItemData("furn_647", Types.Bed, "Диван раскладной (белый)", "p_lev_sofa_s");
                new ItemData("furn_648", Types.Bed, "Кровать графа", "p_mbbed_s");
                new ItemData("furn_650", Types.Bed, "Лежак пляжный (апельсиновый)", "p_patio_lounger1_s");
                new ItemData("furn_651", Types.Bed, "Диван ковровый", "p_res_sofa_l_s");
                new ItemData("furn_654", Types.Bed, "Диван длинный (кожаный)", "p_v_med_p_sofa_s");
                new ItemData("furn_655", Types.Bed, "Кровать застеленная (спальная)", "p_v_res_tt_bed_s");
                new ItemData("furn_666", Types.Bed, "Диван раскладной (снежный)", "v_ilev_m_sofa");
                new ItemData("furn_676", Types.Bed, "Кровать расправленная (красная)", "v_res_msonbed_s");
                new ItemData("furn_681", Types.Bed, "Диван классический (синий)", "v_res_tre_sofa_s");
                new ItemData("furn_696", Types.Bed, "Диван модный (белый)", "xm_lab_sofa_01");
                new ItemData("furn_697", Types.Bed, "Диван кожаный (черный)", "xm_lab_sofa_02");

                new ItemData("furn_61", Types.Table, "Стол стеклянный (журнальный)", "apa_mp_h_din_table_01");
                new ItemData("furn_62", Types.Table, "Стол стеклянный с контурами (журнальный)", "apa_mp_h_din_table_04");
                new ItemData("furn_63", Types.Table, "Стол стильный белый (журнальный)", "apa_mp_h_din_table_06");
                new ItemData("furn_64", Types.Table, "Стол стеклянный с контурами №2 (журнальный)", "apa_mp_h_din_table_11");
                new ItemData("furn_138", Types.Table, "Стол стеклянный", "apa_mp_h_str_sideboards_02");
                new ItemData("furn_139", Types.Table, "Стол стеклянный треугольник", "apa_mp_h_tab_coffee_07");
                new ItemData("furn_140", Types.Table, "Стол квадратный белый (журнальный)", "apa_mp_h_tab_coffee_08");
                new ItemData("furn_141", Types.Table, "Стол стеклянный квадрат (журнальный)", "apa_mp_h_tab_sidelrg_01");
                new ItemData("furn_142", Types.Table, "Стол стеклянный квадрат №2 (журнальный)", "apa_mp_h_tab_sidelrg_02");
                new ItemData("furn_143", Types.Table, "Барная стойка", "apa_mp_h_tab_sidelrg_04");
                new ItemData("furn_144", Types.Table, "Стол стеклянный квадрат", "apa_mp_h_tab_sidelrg_07");
                new ItemData("furn_145", Types.Table, "Стол стеклянный дизайнерский", "apa_mp_h_tab_sidesml_01");
                new ItemData("furn_146", Types.Table, "Стол стеклянный куб", "apa_mp_h_tab_sidesml_02");
                new ItemData("furn_153", Types.Table, "Стол в виде куба", "apa_mp_h_yacht_side_table_01");
                new ItemData("furn_154", Types.Table, "Стол дизайнерский (круглый)", "apa_mp_h_yacht_side_table_02");
                new ItemData("furn_246", Types.Table, "Стол на стойке", "bkr_prop_clubhouse_arm_wrestle_02a");
                new ItemData("furn_266", Types.Table, "Стол письменный Роскошь", "cls_sf_table_1");
                new ItemData("furn_291", Types.Table, "Стол обтянутый тканью", "gr_dlc_gr_yacht_props_table_01");
                new ItemData("furn_292", Types.Table, "Стол обтянутый тканью (журнальный)", "gr_dlc_gr_yacht_props_table_02");
                new ItemData("furn_293", Types.Table, "Стол обтянутый тканью №2", "gr_dlc_gr_yacht_props_table_03");
                new ItemData("furn_327", Types.Table, "Стол дизайнерский (журнальный)", "hei_heist_stn_benchshort");
                new ItemData("furn_480", Types.Table, "Стол с телевизором", "prop_ld_farm_table01");
                new ItemData("furn_481", Types.Table, "Стол с бутылками", "prop_ld_farm_table02");
                new ItemData("furn_577", Types.Table, "Стол журнальный с кофе", "prop_table_01");
                new ItemData("furn_579", Types.Table, "Стол деревянный (круглый)", "prop_table_02");
                new ItemData("furn_581", Types.Table, "Стол прямоугольный (белый)", "prop_table_03");
                new ItemData("furn_583", Types.Table, "Стол прямоугольный (деревянный)", "prop_table_04");
                new ItemData("furn_585", Types.Table, "Стол деревянный (круглый №2)", "prop_table_05");
                new ItemData("furn_587", Types.Table, "Стол из заведения", "prop_table_06");
                new ItemData("furn_589", Types.Table, "Барный стол", "prop_table_07");
                new ItemData("furn_665", Types.Table, "Стол дизайнерский (журнальный №2)", "v_ilev_liconftable_sml");
                new ItemData("furn_671", Types.Table, "Стол консоль тонкий", "v_res_mconsoletrad");
                new ItemData("furn_678", Types.Table, "Стол консоль современный", "v_res_m_console");
                new ItemData("furn_679", Types.Table, "Стол консоль резной", "v_res_m_h_console");

                new ItemData("furn_51", Types.Locker, "Комод классический (черный)", "apa_mp_h_bed_chestdrawer_02");
                new ItemData("furn_54", Types.Locker, "2 тумбочки (красные)", "apa_mp_h_bed_table_wide_12");
                new ItemData("furn_127", Types.Locker, "Шкаф классический", "apa_mp_h_str_shelffloorm_02");
                new ItemData("furn_128", Types.Locker, "Шкаф открытый (белый)", "apa_mp_h_str_shelffreel_01");
                new ItemData("furn_129", Types.Locker, "Шкаф открытый (коричневый)", "apa_mp_h_str_shelfwallm_01");
                new ItemData("furn_130", Types.Locker, "Тумбочка белая", "apa_mp_h_str_sideboardl_06");
                new ItemData("furn_131", Types.Locker, "Тумбочка серая", "apa_mp_h_str_sideboardl_09");
                new ItemData("furn_132", Types.Locker, "Тумба коричневая", "apa_mp_h_str_sideboardl_11");
                new ItemData("furn_133", Types.Locker, "Тумба серая", "apa_mp_h_str_sideboardl_13");
                new ItemData("furn_134", Types.Locker, "Тумба открытая", "apa_mp_h_str_sideboardl_14");
                new ItemData("furn_135", Types.Locker, "Тумба коричневая", "apa_mp_h_str_sideboardm_02");
                new ItemData("furn_136", Types.Locker, "Тумба белая №2", "apa_mp_h_str_sideboardm_03");
                new ItemData("furn_137", Types.Locker, "Тумба дизайнерская", "apa_mp_h_str_sideboards_01");
                new ItemData("furn_196", Types.Locker, "Антресоль темная (L)", "ap_m_cooking_17");
                new ItemData("furn_199", Types.Locker, "Антресоль белая с боковым шкафом (L)", "ap_m_cooking_2");
                new ItemData("furn_209", Types.Locker, "Полка деревянная одиночная", "ap_m_prop_10");
                new ItemData("furn_211", Types.Locker, "Стойка деревянная вытянутая", "ap_m_prop_12");
                new ItemData("furn_222", Types.Wardrobe, "Гардероб стеллаж Luxe (1)", "ap_m_shelf_1");
                new ItemData("furn_223", Types.Locker, "Стойка деревянная квадратная", "ap_m_shelf_2");
                new ItemData("furn_224", Types.Locker, "Полка деревянная двойная темная", "ap_m_shelf_3");
                new ItemData("furn_225", Types.Wardrobe, "Гардероб деревянный Бизнес", "ap_m_shelf_4");
                new ItemData("furn_226", Types.Locker, "Шкаф деревянный со светлой отделкой (слева)", "ap_m_shelf_5");
                new ItemData("furn_227", Types.Wardrobe, "Гардероб стеллаж Luxe (2)", "ap_m_shelf_6");
                new ItemData("furn_228", Types.Locker, "Шкаф деревянный со светлой отделкой (справа)", "ap_m_shelf_7");
                new ItemData("furn_242", Types.Locker, "Шкаф маленький с одеждой", "bkr_prop_biker_garage_locker_01");
                new ItemData("furn_244", Types.Locker, "Шкаф открытый", "bkr_prop_biker_safebody_01a");
                new ItemData("furn_264", Types.Locker, "Книжный шкаф угловой", "cls_sf_shelf_1");
                new ItemData("furn_265", Types.Locker, "Книжный шкаф прямой", "cls_sf_shelf_2");
                new ItemData("furn_317", Types.Locker, "Комод классический (кофейный)", "hei_heist_bed_chestdrawer_04");
                new ItemData("furn_318", Types.Locker, "2 тумбочки (кофейные)", "hei_heist_bed_table_dble_04");
                new ItemData("furn_340", Types.Locker, "Тумбочка классическая (белая)", "hei_heist_str_sideboardl_02");
                new ItemData("furn_341", Types.Locker, "Тумбочка классическая (коричневая)", "hei_heist_str_sideboardl_03");
                new ItemData("furn_342", Types.Locker, "Тумбочка классическая (синяя)", "hei_heist_str_sideboardl_04");
                new ItemData("furn_343", Types.Locker, "Тумбочка классическая (белая)", "hei_heist_str_sideboardl_05");
                new ItemData("furn_459", Types.Locker, "Ярмарочная тумбочка", "prop_funfair_zoltan");
                new ItemData("furn_551", Types.Locker, "Комод бюджетный", "prop_rub_cabinet01");
                new ItemData("furn_552", Types.Locker, "Шкаф открытый простой", "prop_rub_cabinet02");
                new ItemData("furn_553", Types.Locker, "Шкаф открытый широкий", "prop_rub_cabinet03");
                new ItemData("furn_604", Types.Locker, "Тумба под TV", "prop_tv_cabinet_03");
                new ItemData("furn_605", Types.Locker, "Тумба под TV простая", "prop_tv_cabinet_04");
                new ItemData("furn_606", Types.Locker, "Тумба под TV со свалки", "prop_tv_cabinet_05");
                new ItemData("furn_619", Types.Locker, "Тумбочка (кремовая)", "prop_venice_counter_01");
                new ItemData("furn_620", Types.Locker, "Тумбочка с бонгами", "prop_venice_counter_02");
                new ItemData("furn_621", Types.Locker, "Тумбочка с шампунем", "prop_venice_counter_04");
                new ItemData("furn_668", Types.Locker, "Шкаф резной застекленный", "v_res_cabinet");
                new ItemData("furn_669", Types.Locker, "Старинное трюмо", "v_res_d_dressingtable");
                new ItemData("furn_670", Types.Locker, "Комод резной старинный", "v_res_mconsolemod");
                new ItemData("furn_672", Types.Locker, "Трюмо кинозвезды", "v_res_mddresser");
                new ItemData("furn_677", Types.Locker, "Стенка гика с электроникой", "v_res_msoncabinet");
                new ItemData("furn_682", Types.Locker, "Комод крохотный", "v_res_tre_storageunit");
                new ItemData("furn_683", Types.Locker, "Шкаф белый простой", "v_res_tre_wardrobe");

                new ItemData("furn_29", Types.Plant, "Растение Хамедория", "apa_mp_h_acc_plant_palm_01");
                new ItemData("furn_30", Types.Plant, "Растение Сансевьера", "apa_mp_h_acc_plant_tall_01");
                new ItemData("furn_46", Types.Plant, "Цветы Каллы", "apa_mp_h_acc_vase_flowers_01");
                new ItemData("furn_47", Types.Plant, "Цветы Флоксы", "apa_mp_h_acc_vase_flowers_02");
                new ItemData("furn_48", Types.Plant, "Цветы Белокрыльники", "apa_mp_h_acc_vase_flowers_03");
                new ItemData("furn_49", Types.Plant, "Цветы Колокольчики", "apa_mp_h_acc_vase_flowers_04");
                new ItemData("furn_312", Types.Plant, "Цветы Белокрыльники №2", "hei_heist_acc_flowers_01");
                new ItemData("furn_313", Types.Plant, "Растение Физализ", "hei_heist_acc_flowers_02");
                new ItemData("furn_455", Types.Plant, "Комнатное декоративное растение", "prop_fib_plant_01");
                new ItemData("furn_507", Types.Plant, "Кувшинки в горшке", "prop_peyote_highland_02");
                new ItemData("furn_508", Types.Plant, "Кувшинка", "prop_peyote_water_01");
                new ItemData("furn_511", Types.Plant, "Растение Монстера", "prop_plant_int_01a");
                new ItemData("furn_512", Types.Plant, "Растение Монстера №2", "prop_plant_int_01b");
                new ItemData("furn_513", Types.Plant, "Декоративное комнатное растение в горшке", "prop_plant_int_02a");
                new ItemData("furn_514", Types.Plant, "Декоративное комнатное растение в вазе", "prop_plant_int_03a");
                new ItemData("furn_515", Types.Plant, "Декоративное комнатное растение в вазе №2", "prop_plant_int_03b");
                new ItemData("furn_516", Types.Plant, "Растение Монстера №3", "prop_plant_int_04a");
                new ItemData("furn_517", Types.Plant, "Декоративное комнатное дерево", "prop_plant_int_04b");
                new ItemData("furn_518", Types.Plant, "Растение Блехнум", "prop_plant_int_04c");
                new ItemData("furn_519", Types.Plant, "Декоративная пальма", "prop_plant_palm_01a");
                new ItemData("furn_520", Types.Plant, "Декоративные пальмы №2", "prop_plant_palm_01c");
                new ItemData("furn_526", Types.Plant, "Растение Хлорофитум", "prop_pot_plant_01a");
                new ItemData("furn_527", Types.Plant, "Растение Хлорофитум №2", "prop_pot_plant_01b");
                new ItemData("furn_528", Types.Plant, "Растение Аспарагус", "prop_pot_plant_01c");
                new ItemData("furn_529", Types.Plant, "Растение Хамедория Элеганс", "prop_pot_plant_01d");
                new ItemData("furn_530", Types.Plant, "Растение Революта", "prop_pot_plant_01e");
                new ItemData("furn_531", Types.Plant, "Декоративное комнатное растение", "prop_pot_plant_03a");
                new ItemData("furn_532", Types.Plant, "Декоративное комнатное растение №2", "prop_pot_plant_03c");
                new ItemData("furn_533", Types.Plant, "Декоративное комнатное растение №3", "prop_pot_plant_04a");
                new ItemData("furn_534", Types.Plant, "Декоративное комнатное растение №4", "prop_pot_plant_04b");
                new ItemData("furn_535", Types.Plant, "Декоративные комнатные пальмы в горшке", "prop_pot_plant_04c");
                new ItemData("furn_536", Types.Plant, "Растение Аспидистра", "prop_pot_plant_05a");
                new ItemData("furn_537", Types.Plant, "Декоративное комнатное растение №5", "prop_pot_plant_05b");
                new ItemData("furn_538", Types.Plant, "Декоративное комнатное растение №6", "prop_pot_plant_05c");
                new ItemData("furn_539", Types.Plant, "Декоративное комнатное растение №7", "prop_pot_plant_05d");
                new ItemData("furn_540", Types.Plant, "Декоративное комнатное растение №8", "prop_pot_plant_05d_l1");
                new ItemData("furn_541", Types.Plant, "Декоративное комнатное растение №9", "prop_pot_plant_6a");
                new ItemData("furn_542", Types.Plant, "Декоративное комнатное растение №10", "prop_pot_plant_6b");
                new ItemData("furn_543", Types.Plant, "Растение Аспарагус №2", "prop_pot_plant_bh1");
                new ItemData("furn_544", Types.Plant, "Декоративное комнатное растение №11", "prop_pot_plant_inter_03a");
                new ItemData("furn_634", Types.Plant, "Декоративное комнатное растение №12", "prop_windowbox_a");
                new ItemData("furn_635", Types.Plant, "Декоративное комнатное растение №13", "prop_windowbox_b");
                new ItemData("furn_643", Types.Plant, "Растение Хамедорея №2", "p_int_jewel_plant_01");
                new ItemData("furn_644", Types.Plant, "Растение Спатифиллум", "p_int_jewel_plant_02");

                new ItemData("furn_65", Types.Lamp, "Светильник напольный Рисовая бумага", "apa_mp_h_floorlamp_a");
                new ItemData("furn_66", Types.Lamp, "Светильник напольный Зигзаг", "apa_mp_h_floorlamp_b");
                new ItemData("furn_67", Types.Lamp, "Светильник напольный Черный", "apa_mp_h_floorlamp_c");
                new ItemData("furn_68", Types.Lamp, "Светильник напольный Гриб", "apa_mp_h_floor_lamp_int_08");
                new ItemData("furn_69", Types.Lamp, "Люстра из ламп Эдисона", "apa_mp_h_lampbulb_multiple_a");
                new ItemData("furn_70", Types.Lamp, "Светильник напольный Синий", "apa_mp_h_lit_floorlampnight_07");
                new ItemData("furn_71", Types.Lamp, "Светильник напольный Торшер", "apa_mp_h_lit_floorlampnight_14");
                new ItemData("furn_72", Types.Lamp, "Светильник напольный Pixar", "apa_mp_h_lit_floorlamp_01");
                new ItemData("furn_73", Types.Lamp, "Светильник напольный Строгий", "apa_mp_h_lit_floorlamp_03");
                new ItemData("furn_74", Types.Lamp, "Светильник напольный Черный (L)", "apa_mp_h_lit_floorlamp_05");
                new ItemData("furn_75", Types.Lamp, "Светильник напольный Простой", "apa_mp_h_lit_floorlamp_06");
                new ItemData("furn_76", Types.Lamp, "Светильник напольный Побеги", "apa_mp_h_lit_floorlamp_10");
                new ItemData("furn_77", Types.Lamp, "Светильник напольный Неон", "apa_mp_h_lit_floorlamp_13");
                new ItemData("furn_78", Types.Lamp, "Светильник напольный Тренога", "apa_mp_h_lit_floorlamp_17");
                new ItemData("furn_79", Types.Lamp, "Лампа настольная Тренога", "apa_mp_h_lit_lamptablenight_16");
                new ItemData("furn_80", Types.Lamp, "Лампа настольная Черная", "apa_mp_h_lit_lamptablenight_24");
                new ItemData("furn_81", Types.Lamp, "Лампа настольная Классика", "apa_mp_h_lit_lamptable_005");
                new ItemData("furn_82", Types.Lamp, "Лампа настольная Pixar", "apa_mp_h_lit_lamptable_02");
                new ItemData("furn_83", Types.Lamp, "Лампа настольная Пожилой депутат", "apa_mp_h_lit_lamptable_04");
                new ItemData("furn_84", Types.Lamp, "Лампа настольная Рисовая бумага", "apa_mp_h_lit_lamptable_09");
                new ItemData("furn_85", Types.Lamp, "Лампа настольная Али", "apa_mp_h_lit_lamptable_14");
                new ItemData("furn_86", Types.Lamp, "Лампа настольная Азия", "apa_mp_h_lit_lamptable_17");
                new ItemData("furn_87", Types.Lamp, "Лампа настольная Гриб", "apa_mp_h_lit_lamptable_21");
                new ItemData("furn_88", Types.Lamp, "Люстра Модерн", "apa_mp_h_lit_lightpendant_01");
                new ItemData("furn_89", Types.Lamp, "Люстра Тройка черная", "apa_mp_h_lit_lightpendant_05");
                new ItemData("furn_90", Types.Lamp, "Люстра Тройка дерево", "apa_mp_h_lit_lightpendant_05b");
                new ItemData("furn_158", Types.Lamp, "Лампа настольная Кожа", "apa_mp_h_yacht_table_lamp_01");
                new ItemData("furn_159", Types.Lamp, "Лампа настольная Бетон", "apa_mp_h_yacht_table_lamp_03");
                new ItemData("furn_320", Types.Lamp, "Светильник напольный Кольцо", "hei_heist_lit_floorlamp_02");
                new ItemData("furn_321", Types.Lamp, "Светильник напольный Наклон", "hei_heist_lit_floorlamp_04");
                new ItemData("furn_322", Types.Lamp, "Лампа настольная Наклон", "hei_heist_lit_lamptable_03");
                new ItemData("furn_323", Types.Lamp, "Лампа настольная Молодой депутат", "hei_heist_lit_lamptable_04");
                new ItemData("furn_324", Types.Lamp, "Лампа настольная Сфера", "hei_heist_lit_lamptable_06");
                new ItemData("furn_325", Types.Lamp, "Люстра Купол", "hei_heist_lit_lightpendant_003");
                new ItemData("furn_326", Types.Lamp, "Люстра Оранжевая", "hei_heist_lit_lightpendant_02");
                new ItemData("furn_344", Types.Lamp, "Набор прожекторов подвесной (L)", "hei_prop_carrier_lightset_1");
                new ItemData("furn_345", Types.Lamp, "Прожектор одиночный", "hei_prop_carrier_light_01");
                new ItemData("furn_348", Types.Lamp, "Лампа настольная Библиотека", "hei_prop_hei_bnk_lamp_01");
                new ItemData("furn_349", Types.Lamp, "Лампа настольная Готика", "hei_prop_hei_bnk_lamp_02");
                new ItemData("furn_572", Types.Lamp, "Софит одиночный", "prop_spot_01");
                new ItemData("furn_573", Types.Lamp, "Софит подвесной", "prop_spot_clamp_02");
                new ItemData("furn_574", Types.Lamp, "Набор прожекторов на треноге (L)", "prop_studio_light_01");
                new ItemData("furn_575", Types.Lamp, "Софит на треноге", "prop_studio_light_02");
                new ItemData("furn_576", Types.Lamp, "Фотозонт", "prop_studio_light_03");
                new ItemData("furn_624", Types.Lamp, "Фонарь Готика", "prop_wall_light_07a");

                new ItemData("furn_121", Types.TV, "Стенка с ТВ справа, дерево (XXL)", "apa_mp_h_str_avunitl_01_b");
                new ItemData("furn_122", Types.TV, "Стенка с ТВ, серая (XXL)", "apa_mp_h_str_avunitl_04");
                new ItemData("furn_123", Types.TV, "Домашний кинотеатр (желтые колонки) (XL)", "apa_mp_h_str_avunitm_01");
                new ItemData("furn_124", Types.TV, "Домашний кинотеатр (белые колонки) (XL)", "apa_mp_h_str_avunitm_03");
                new ItemData("furn_125", Types.TV, "Домашний кинотеатр (без колонок) (XL)", "apa_mp_h_str_avunits_01");
                new ItemData("furn_126", Types.TV, "Домашний кинотеатр (стойка) (XL)", "apa_mp_h_str_avunits_04");
                new ItemData("furn_267", Types.TV, "Плазменный телевизор", "des_tvsmash_start");
                new ItemData("furn_337", Types.TV, "Стенка с ТВ слева, дерево (XXL)", "hei_heist_str_avunitl_01");
                new ItemData("furn_338", Types.TV, "Домашний кинотеатр MAX, серый (XXL)", "hei_heist_str_avunitl_03");
                new ItemData("furn_339", Types.TV, "Домашний кинотеатр с LCD (XL)", "hei_heist_str_avunits_01");
                new ItemData("furn_405", Types.TV, "Телевизор Триколор", "prop_cctv_mon_02");
                new ItemData("furn_440", Types.TV, "ТВ на стойке", "prop_cs_tv_stand");
                new ItemData("furn_597", Types.TV, "Телевизор Горизонт", "prop_tv_01");
                new ItemData("furn_598", Types.TV, "Телевизор Компакт", "prop_tv_02");
                new ItemData("furn_599", Types.TV, "Телевизор Пасамоник серебро", "prop_tv_03");
                new ItemData("furn_600", Types.TV, "Телевизор Ретро", "prop_tv_04");
                new ItemData("furn_601", Types.TV, "Телевизор с антенной", "prop_tv_05");
                new ItemData("furn_602", Types.TV, "Телевизор Пасамоник черный", "prop_tv_06");
                new ItemData("furn_603", Types.TV, "Телевизор 2001", "prop_tv_07");
                new ItemData("furn_607", Types.TV, "ТВ на стену LCD (L)", "prop_tv_flat_01");
                new ItemData("furn_608", Types.TV, "ТВ на подставке серый (L)", "prop_tv_flat_02");
                new ItemData("furn_609", Types.TV, "ТВ на подставке черный (L)", "prop_tv_flat_02b");
                new ItemData("furn_610", Types.TV, "ТВ на подставке серый (M)", "prop_tv_flat_03");
                new ItemData("furn_611", Types.TV, "ТВ на стену серый (M)", "prop_tv_flat_03b");
                new ItemData("furn_612", Types.TV, "ТВ на стену Plasma (L)", "prop_tv_flat_michael");
                new ItemData("furn_660", Types.TV, "ТВ на подставке LCD (XL)", "sm_prop_smug_tv_flat_01");

                new ItemData("furn_28", Types.Electronics, "Телефон ретро", "apa_mp_h_acc_phone_01");
                new ItemData("furn_210", Types.Electronics, "Бойлер с бытовой химией", "ap_m_prop_11");
                new ItemData("furn_231", Types.Electronics, "Джойстик игровой (темный)", "ba_prop_battle_control_console");
                new ItemData("furn_248", Types.Electronics, "Музыкальный автомат настенный", "bkr_prop_clubhouse_jukebox_01a");
                new ItemData("furn_249", Types.Electronics, "Музыкальный автомат 80х", "bkr_prop_clubhouse_jukebox_01b");
                new ItemData("furn_250", Types.Electronics, "Музыкальный автомат 60х", "bkr_prop_clubhouse_jukebox_02a");
                new ItemData("furn_251", Types.Electronics, "Ноутбук iFruit серый", "bkr_prop_clubhouse_laptop_01a");
                new ItemData("furn_281", Types.Electronics, "Моноблок iFruit", "ex_prop_monitor_01_ex");
                new ItemData("furn_285", Types.Electronics, "Моноблок iFruit для работы", "ex_prop_trailer_monitor_01");
                new ItemData("furn_298", Types.Electronics, "Джойстик игровой (светлый)", "gr_prop_gr_console_01");
                new ItemData("furn_346", Types.Electronics, "ПК iFruit современный", "hei_prop_heist_pc_01");
                new ItemData("furn_347", Types.Electronics, "Телефон", "hei_prop_hei_bank_phone_01");
                new ItemData("furn_425", Types.Electronics, "Игровая консоль", "prop_console_01");
                new ItemData("furn_445", Types.Electronics, "ПК iFruit классический", "prop_dyn_pc_02");
                new ItemData("furn_449", Types.Electronics, "Кассетный магнитофон", "prop_el_tapeplayer_01");
                new ItemData("furn_452", Types.Electronics, "Напольный вентилятор", "prop_fan_01");
                new ItemData("furn_453", Types.Electronics, "Факс", "prop_fax_01");
                new ItemData("furn_468", Types.Electronics, "Камера", "prop_ing_camera_01");
                new ItemData("furn_471", Types.Electronics, "Клавиатура белая", "prop_keyboard_01a");
                new ItemData("furn_472", Types.Electronics, "Клавиатура черная", "prop_keyboard_01b");
                new ItemData("furn_479", Types.Electronics, "Ноутбук iFruit аниме", "prop_laptop_lester");
                new ItemData("furn_483", Types.Electronics, "Монитор Пасамоник", "prop_ld_monitor_01");
                new ItemData("furn_488", Types.Electronics, "Монитор современный космос", "prop_monitor_01a");
                new ItemData("furn_489", Types.Electronics, "Монитор современный", "prop_monitor_01b");
                new ItemData("furn_490", Types.Electronics, "Монитор iFruit эльф", "prop_monitor_01c");
                new ItemData("furn_491", Types.Electronics, "Монитор современный 18+", "prop_monitor_01d");
                new ItemData("furn_492", Types.Electronics, "Монитор iFruit", "prop_monitor_02");
                new ItemData("furn_493", Types.Electronics, "Монитор большой", "prop_monitor_w_large");
                new ItemData("furn_546", Types.Electronics, "Принтер", "prop_printer_02");
                new ItemData("furn_549", Types.Electronics, "Радиоприемник", "prop_radio_01");
                new ItemData("furn_564", Types.Electronics, "Колонка напольная (L)", "prop_speaker_01");
                new ItemData("furn_565", Types.Electronics, "Колонка дерево (M)", "prop_speaker_02");
                new ItemData("furn_566", Types.Electronics, "Колонка-монитор классика", "prop_speaker_03");
                new ItemData("furn_567", Types.Electronics, "Колонка-монитор современная", "prop_speaker_05");
                new ItemData("furn_568", Types.Electronics, "Колонка с сеткой черная", "prop_speaker_06");
                new ItemData("furn_569", Types.Electronics, "Колонка студийная", "prop_speaker_07");
                new ItemData("furn_570", Types.Electronics, "Колонка трехполосная", "prop_speaker_08");
                new ItemData("furn_640", Types.Electronics, "Ноутбук Пасамоник", "p_cs_laptop_02");
                new ItemData("furn_645", Types.Electronics, "Ноутбук iFruit белый", "p_laptop_02_s");
                new ItemData("furn_675", Types.Electronics, "Колонка центральная", "v_res_mm_audio");

                new ItemData("furn_22", Types.KitchenStuff, "Чайный набор", "apa_mp_h_acc_drink_tray_02");
                new ItemData("furn_23", Types.KitchenStuff, "Фруктовая тарелка (яблоки)", "apa_mp_h_acc_fruitbowl_01");
                new ItemData("furn_24", Types.KitchenStuff, "Фруктовая тарелка (ассорти)", "apa_mp_h_acc_fruitbowl_02");
                new ItemData("furn_188", Types.KitchenStuff, "Кухонный гарнитур полный Белый прямой", "ap_m_cooking_1");
                new ItemData("furn_189", Types.KitchenStuff, "Тумбочка с рабочей поверхностью серая 2 отсека", "ap_m_cooking_10");
                new ItemData("furn_190", Types.KitchenStuff, "Раковина угловая мрамор", "ap_m_cooking_11");
                new ItemData("furn_191", Types.KitchenStuff, "Кухонный гарнитур полный Черный прямой", "ap_m_cooking_12");
                new ItemData("furn_192", Types.KitchenStuff, "Остров кухонный профессиональный темный", "ap_m_cooking_13");
                new ItemData("furn_193", Types.KitchenStuff, "Кухонный гарнитур полный угловой белый с черным", "ap_m_cooking_14");
                new ItemData("furn_194", Types.KitchenStuff, "Тумбочка с рабочей поверхностью темная 1 отсек", "ap_m_cooking_15");
                new ItemData("furn_195", Types.KitchenStuff, "Подвесной шкаф темный 4 отсека", "ap_m_cooking_16");
                new ItemData("furn_197", Types.KitchenStuff, "Раковина угловая с рабочей поверхностью темная", "ap_m_cooking_18");
                new ItemData("furn_198", Types.KitchenStuff, "Кухонный гарнитур полный Черный прямой с деревом", "ap_m_cooking_19");
                new ItemData("furn_200", Types.KitchenStuff, "Остров кухонный профессиональный светлый", "ap_m_cooking_20");
                new ItemData("furn_201", Types.KitchenStuff, "Раковина угловая белая", "ap_m_cooking_3");

                new ItemData("furn_202", Types.Fridge, "Холодильник Ретро", "ap_m_cooking_4");

                new ItemData("furn_203", Types.KitchenStuff, "Раковина угловая с рабочей поверхностью белая", "ap_m_cooking_5");
                new ItemData("furn_204", Types.KitchenStuff, "Тумбочка с рабочей поверхностью 2 отсека", "ap_m_cooking_6");

                new ItemData("furn_205", Types.KitchenSet, "Газовая плита Стандарт", "ap_m_cooking_7");

                new ItemData("furn_206", Types.KitchenStuff, "Подвесной шкаф белый 5 отсеков", "ap_m_cooking_8");
                new ItemData("furn_207", Types.KitchenStuff, "Подвесной шкаф мрамор 5 отсеков", "ap_m_cooking_9");
                new ItemData("furn_208", Types.KitchenStuff, "Кухонные принадлежности настенные металл", "ap_m_prop_1");
                new ItemData("furn_232", Types.KitchenStuff, "Коллекция местного пива", "beerrow_local");
                new ItemData("furn_233", Types.KitchenStuff, "Коллекция мирового пива", "beerrow_world");
                new ItemData("furn_254", Types.KitchenStuff, "Дегидратор", "bkr_prop_coke_dehydrator_01");
                new ItemData("furn_268", Types.KitchenStuff, "Кофемашина эспрессо медь", "ex_mp_h_acc_coffeemachine_01");
                new ItemData("furn_319", Types.KitchenStuff, "Кофемашина эспрессо сталь", "hei_heist_kit_coffeemachine_01");
                new ItemData("furn_378", Types.KitchenStuff, "Барная салфетница", "prop_bar_caddy");

                new ItemData("furn_379", Types.Fridge, "Холодильник для вина", "prop_bar_fridge_01");
                new ItemData("furn_380", Types.Fridge, "Холодильник для пива (двойной)", "prop_bar_fridge_02");
                new ItemData("furn_381", Types.Fridge, "Холодильник для пива", "prop_bar_fridge_03");
                new ItemData("furn_382", Types.Fridge, "Холодильник для газировки (двойной)", "prop_bar_fridge_04");

                new ItemData("furn_383", Types.KitchenStuff, "Фруктовая тарелка (нарезка)", "prop_bar_fruit");
                new ItemData("furn_384", Types.KitchenStuff, "Ледогенератор", "prop_bar_ice_01");
                new ItemData("furn_385", Types.KitchenStuff, "Фруктовая тарелка (лимон)", "prop_bar_lemons");
                new ItemData("furn_386", Types.KitchenStuff, "Фруктовая тарелка (лайм)", "prop_bar_limes");
                new ItemData("furn_387", Types.KitchenStuff, "Мерный стакан", "prop_bar_measrjug");
                new ItemData("furn_388", Types.KitchenStuff, "Салфетница", "prop_bar_napkindisp");
                new ItemData("furn_389", Types.KitchenStuff, "Барная стойка с кранами", "prop_bar_pump_01");
                new ItemData("furn_390", Types.KitchenStuff, "Мойка профессиональная", "prop_bar_sink_01");
                new ItemData("furn_401", Types.KitchenStuff, "Мусорное ведро большое", "prop_bin_10b");
                new ItemData("furn_402", Types.KitchenStuff, "Мусорное ведро с крышкой", "prop_bin_11a");
                new ItemData("furn_403", Types.KitchenStuff, "Мусорное ведро малое", "prop_bin_11b");
                new ItemData("furn_423", Types.KitchenStuff, "Кофемашина профессиональная", "prop_coffee_mac_01");
                new ItemData("furn_424", Types.KitchenStuff, "Кофемашина капельная", "prop_coffee_mac_02");

                new ItemData("furn_426", Types.KitchenSet, "Газовая плита профессиональная", "prop_cooker_03");

                new ItemData("furn_427", Types.KitchenStuff, "Кастрюля медная", "prop_copper_pan");
                new ItemData("furn_454", Types.KitchenStuff, "Мойка простая", "prop_ff_sink_02");
                new ItemData("furn_469", Types.KitchenStuff, "Кебаб гриль", "prop_kebab_grill");
                new ItemData("furn_470", Types.KitchenStuff, "Чайник электрический", "prop_kettle");
                new ItemData("furn_473", Types.KitchenStuff, "Сковорода", "prop_kitch_pot_fry");
                new ItemData("furn_474", Types.KitchenStuff, "Кастрюля (XL)", "prop_kitch_pot_huge");
                new ItemData("furn_475", Types.KitchenStuff, "Ковш (L)", "prop_kitch_pot_lrg");
                new ItemData("furn_476", Types.KitchenStuff, "Ковш (S)", "prop_kitch_pot_sm");
                new ItemData("furn_477", Types.KitchenStuff, "Набор ножей в стойке, сталь", "prop_knife_stand");
                new ItemData("furn_485", Types.KitchenStuff, "Микроволновка сталь", "prop_microwave_1");
                new ItemData("furn_486", Types.KitchenStuff, "Микроволновка белая", "prop_micro_02");
                new ItemData("furn_487", Types.KitchenStuff, "Микроволновка дерево", "prop_micro_04");
                new ItemData("furn_545", Types.KitchenStuff, "Набор посуды подвесной (XL)", "prop_pot_rack");
                new ItemData("furn_618", Types.KitchenStuff, "Набор для готовки (на стену)", "prop_utensil");
                new ItemData("furn_628", Types.KitchenStuff, "Кулер", "prop_watercooler");
                new ItemData("furn_629", Types.KitchenStuff, "Бутыль для кулера", "prop_water_bottle");
                new ItemData("furn_673", Types.KitchenStuff, "Набор ножей в стойке, дерево", "v_res_mknifeblock");
                new ItemData("furn_674", Types.KitchenStuff, "Набор ножей (на стену)", "v_res_mkniferack");
                new ItemData("furn_684", Types.KitchenStuff, "Кастрюля чугунная", "v_res_tt_pot01");
                new ItemData("furn_685", Types.KitchenStuff, "Сковорода чугунная", "v_res_tt_pot02");
                new ItemData("furn_686", Types.KitchenStuff, "Ковш чугунный", "v_res_tt_pot03");
                new ItemData("furn_689", Types.KitchenStuff, "Коллекция вина", "winerow");

                new ItemData("furn_50", Types.Bath, "Ванная белая", "apa_mp_h_bathtub_01");
                new ItemData("furn_172", Types.Bath, "Ванная с занавеской", "ap_m_bath_1");
                new ItemData("furn_173", Types.Bath, "Душевая кабина Luxe (L)", "ap_m_bath_10");
                new ItemData("furn_174", Types.Bath, "Ванная Бизнес", "ap_m_bath_11");
                new ItemData("furn_175", Types.BathStuff, "Раковина двойная на инсталляции (L)", "ap_m_bath_12");
                new ItemData("furn_176", Types.Bath, "Ванная Luxe с душем закрытая", "ap_m_bath_13");
                new ItemData("furn_177", Types.Bath, "Ванная Luxe с душем дерево", "ap_m_bath_14");
                new ItemData("furn_178", Types.Bath, "Ванная Luxe с душем гранит", "ap_m_bath_15");
                new ItemData("furn_179", Types.BathStuff, "Раковина двойная на исталляции Luxe (XL)", "ap_m_bath_16");
                new ItemData("furn_180", Types.BathStuff, "Раковина белая с двумя кранами", "ap_m_bath_2");
                new ItemData("furn_183", Types.BathStuff, "Раковина камень на деревянной тумбе", "ap_m_bath_5");
                new ItemData("furn_185", Types.BathStuff, "Раковина на инсталляции (M)", "ap_m_bath_7");
                new ItemData("furn_186", Types.Bath, "Душевая кабина Luxe (M)", "ap_m_bath_8");
                new ItemData("furn_187", Types.Bath, "Душевая кабина Стандарт", "ap_m_bath_9");
                new ItemData("furn_216", Types.BathStuff, "Штанга с полотенцами", "ap_m_prop_4");
                new ItemData("furn_218", Types.BathStuff, "Сушилка для полотенец", "ap_m_prop_6");
                new ItemData("furn_219", Types.BathStuff, "Штанга с полотенцами двойная", "ap_m_prop_7");
                new ItemData("furn_466", Types.Bath, "Джакузи", "prop_hottub2");
                new ItemData("furn_559", Types.BathStuff, "Раковина под дерево", "prop_sink_02");
                new ItemData("furn_560", Types.BathStuff, "Раковина со столиком", "prop_sink_04");
                new ItemData("furn_561", Types.BathStuff, "Раковина белая настенная", "prop_sink_05");
                new ItemData("furn_562", Types.BathStuff, "Раковина белая напольная", "prop_sink_06");

                new ItemData("furn_181", Types.Toilet, "Унитаз белый с деревянным сидением", "ap_m_bath_3");
                new ItemData("furn_182", Types.Toilet, "Унитаз с отдельным бачком", "ap_m_bath_4");
                new ItemData("furn_184", Types.Toilet, "Унитаз современный без бачка", "ap_m_bath_6");
                new ItemData("furn_484", Types.Toilet, "Унитаз б/у", "prop_ld_toilet_01");
                new ItemData("furn_591", Types.Toilet, "Унитаз с бачком", "prop_toilet_01");
                new ItemData("furn_592", Types.Toilet, "Унитаз без бачка", "prop_toilet_02");
                new ItemData("furn_593", Types.Toilet, "Ершик", "prop_toilet_brush_01");
                new ItemData("furn_594", Types.Toilet, "Рулон бумаги", "prop_toilet_roll_01");
                new ItemData("furn_595", Types.Toilet, "Держатель для бумаги", "prop_toilet_roll_02");

                new ItemData("furn_212", Types.Washer, "Стиральная машина с вертикальной загрузкой", "ap_m_prop_13");
                new ItemData("furn_625", Types.Washer, "Стиральная машина б/у", "prop_washer_01");
                new ItemData("furn_626", Types.Washer, "Стиральная машина современная", "prop_washer_02");
                new ItemData("furn_627", Types.Washer, "Стиральная машина старая", "prop_washer_03");

                new ItemData("furn_0", Types.Painting, "Картина Закат абстракционизм (L)", "apa_mp_h_acc_artwalll_01");
                new ItemData("furn_1", Types.Painting, "Картина Утро абстракционизм (L)", "apa_mp_h_acc_artwalll_02");
                new ItemData("furn_2", Types.Painting, "Картина Глубины абстракионизмци (L)", "apa_mp_h_acc_artwalll_03");
                new ItemData("furn_3", Types.Painting, "Картина Небо абстракционизм (M)", "apa_mp_h_acc_artwallm_02");
                new ItemData("furn_4", Types.Painting, "Картина Безликие абстракционизм (M)", "apa_mp_h_acc_artwallm_03");
                new ItemData("furn_5", Types.Painting, "Картина Гнев абстракционизм (M)", "apa_mp_h_acc_artwallm_04");
                new ItemData("furn_161", Types.Painting, "Картина Солнце абстракционизм (L)", "apa_p_h_acc_artwalll_01");
                new ItemData("furn_162", Types.Painting, "Картина S абстракионизм (L)", "apa_p_h_acc_artwalll_02");
                new ItemData("furn_163", Types.Painting, "Картина Клаустрофобия абстракионизм (L)", "apa_p_h_acc_artwalll_03");
                new ItemData("furn_164", Types.Painting, "Картина Ночь абстракионизм (L)", "apa_p_h_acc_artwalll_04");
                new ItemData("furn_165", Types.Painting, "Картина объемная кубы серая (M)", "apa_p_h_acc_artwallm_01");
                new ItemData("furn_166", Types.Painting, "Картина Тлен абстракционизм (M)", "apa_p_h_acc_artwallm_03");
                new ItemData("furn_167", Types.Painting, "Картина Пересечение абстракционизм (M)", "apa_p_h_acc_artwallm_04");
                new ItemData("furn_168", Types.Painting, "Форма чемпиона Larsen#6 в рамке", "apa_p_h_acc_artwalls_03");
                new ItemData("furn_169", Types.Painting, "Форма чемпиона Palmer#3 в рамке", "apa_p_h_acc_artwalls_04");
                new ItemData("furn_170", Types.Painting, "Картина Ретро в деревянной рамке", "ap_m_art_1");
                new ItemData("furn_260", Types.Painting, "Картина в багетной рамке Загадочная Европа", "cls_sf_art_2a");
                new ItemData("furn_261", Types.Painting, "Картина в багетной рамке Вечерная прогулка", "cls_sf_art_3a");
                new ItemData("furn_262", Types.Painting, "Картина в багетной рамке Вольфганг", "cls_sf_art_9c");
                new ItemData("furn_277", Types.Painting, "Пара портретов в багетных рамах", "ex_office_swag_paintings01");
                new ItemData("furn_278", Types.Painting, "Три пейзажа в багетных рамах", "ex_office_swag_paintings02");
                new ItemData("furn_279", Types.Painting, "Два пейзажа в багетных рамах", "ex_office_swag_paintings03");
                new ItemData("furn_286", Types.Painting, "Картина Клаустрофобия абстракионизм (L)", "ex_p_h_acc_artwalll_03");
                new ItemData("furn_306", Types.Painting, "Золотая пластинка Corner Killahz", "hei_heist_acc_artgolddisc_01");
                new ItemData("furn_307", Types.Painting, "Золотая пластинка Kill D Sac", "hei_heist_acc_artgolddisc_02");
                new ItemData("furn_308", Types.Painting, "Золотая пластинка DG Loc", "hei_heist_acc_artgolddisc_03");
                new ItemData("furn_309", Types.Painting, "Золотая пластинка Madd Dogg", "hei_heist_acc_artgolddisc_04");
                new ItemData("furn_310", Types.Painting, "Картина Закат темный абстракционизм (L)", "hei_heist_acc_artwalll_01");
                new ItemData("furn_311", Types.Painting, "Картина объемная кубы белая (M)", "hei_heist_acc_artwallm_01");
                new ItemData("furn_351", Types.Painting, "Фотография автопортрет мужчины", "hei_prop_hei_pic_hl_gurkhas");
                new ItemData("furn_352", Types.Painting, "Фотография неизвестой девушки", "hei_prop_hei_pic_hl_keycodes");
                new ItemData("furn_353", Types.Painting, "Постер Valkyrie", "hei_prop_hei_pic_hl_valkyrie");
                new ItemData("furn_354", Types.Painting, "Фотография задержанного", "hei_prop_hei_pic_pb_break");
                new ItemData("furn_355", Types.Painting, "Фотография синего автобуса", "hei_prop_hei_pic_pb_bus");
                new ItemData("furn_356", Types.Painting, "Постер авиакомпании", "hei_prop_hei_pic_pb_plane");
                new ItemData("furn_357", Types.Painting, "Фотография Полицейского Департамента", "hei_prop_hei_pic_pb_station");
                new ItemData("furn_358", Types.Painting, "Фотография в гаражах", "hei_prop_hei_pic_ps_bike");
                new ItemData("furn_359", Types.Painting, "Фотография Банк", "hei_prop_hei_pic_ps_job");
                new ItemData("furn_360", Types.Painting, "Фотография мусоровозы", "hei_prop_hei_pic_ps_trucks");
                new ItemData("furn_361", Types.Painting, "Фотография автовыставки", "hei_prop_hei_pic_ub_prep");
                new ItemData("furn_362", Types.Painting, "Фотография банка на трассе", "hei_prop_hei_pic_ub_prep02");
                new ItemData("furn_363", Types.Painting, "Фотография банка на трассе зачеркнутая", "hei_prop_hei_pic_ub_prep02b");
                new ItemData("furn_500", Types.Painting, "Фотография хитрого мужчины", "prop_ped_pic_01");
                new ItemData("furn_501", Types.Painting, "Фотография серьезного мужчины", "prop_ped_pic_02");
                new ItemData("furn_502", Types.Painting, "Фотография доброго мужчины", "prop_ped_pic_04");
                new ItemData("furn_503", Types.Painting, "Фотография баскетболиста", "prop_ped_pic_05");
                new ItemData("furn_504", Types.Painting, "Фотография седого мужчины", "prop_ped_pic_06");
                new ItemData("furn_505", Types.Painting, "Фотография молодой девушки", "prop_ped_pic_07");
                new ItemData("furn_506", Types.Painting, "Фотография девушки в очках", "prop_ped_pic_08");
                new ItemData("furn_667", Types.Painting, "Фотография женщины в очках в рамке (M)", "v_ilev_trev_pictureframe");

                new ItemData("furn_6", Types.Decor, "Бутылка белая", "apa_mp_h_acc_bottle_01");
                new ItemData("furn_7", Types.Decor, "Бутылка черная", "apa_mp_h_acc_bottle_02");
                new ItemData("furn_8", Types.Decor, "Чаша черная керамическая", "apa_mp_h_acc_bowl_ceramic_01");
                new ItemData("furn_9", Types.Decor, "Шкатулка белая", "apa_mp_h_acc_box_trinket_01");
                new ItemData("furn_10", Types.Decor, "Шкатулка из черного дерева", "apa_mp_h_acc_box_trinket_02");
                new ItemData("furn_11", Types.Decor, "Набор свечей Геометрия", "apa_mp_h_acc_candles_01");
                new ItemData("furn_12", Types.Decor, "Набор розовых свечей", "apa_mp_h_acc_candles_02");
                new ItemData("furn_13", Types.Decor, "Свеча розовая", "apa_mp_h_acc_candles_04");
                new ItemData("furn_14", Types.Decor, "Свеча в черном стекле", "apa_mp_h_acc_candles_05");
                new ItemData("furn_15", Types.Decor, "Набор черных свечей", "apa_mp_h_acc_candles_06");
                new ItemData("furn_16", Types.Decor, "Скульптура Маска", "apa_mp_h_acc_dec_head_01");
                new ItemData("furn_17", Types.Decor, "Тарелка декоративная Майя", "apa_mp_h_acc_dec_plate_01");
                new ItemData("furn_18", Types.Decor, "Тарелка декоративная Каменный век", "apa_mp_h_acc_dec_plate_02");
                new ItemData("furn_19", Types.Decor, "Скульптура Открытие", "apa_mp_h_acc_dec_sculpt_01");
                new ItemData("furn_20", Types.Decor, "Скульптура Прорезь", "apa_mp_h_acc_dec_sculpt_02");
                new ItemData("furn_21", Types.Decor, "Скульптура Видение", "apa_mp_h_acc_dec_sculpt_03");
                new ItemData("furn_25", Types.Decor, "Контейнер двойной", "apa_mp_h_acc_jar_02");
                new ItemData("furn_26", Types.Decor, "Контейнер тройной", "apa_mp_h_acc_jar_03");
                new ItemData("furn_27", Types.Decor, "Контейнер голубой", "apa_mp_h_acc_jar_04");
                new ItemData("furn_31", Types.Decor, "Чаша с камнями", "apa_mp_h_acc_pot_pouri_01");
                new ItemData("furn_32", Types.Decor, "Ковер с длинным ворсом (XL)", "apa_mp_h_acc_rugwooll_03");
                new ItemData("furn_33", Types.Decor, "Ковер плетеный светлый (XL)", "apa_mp_h_acc_rugwooll_04");
                new ItemData("furn_34", Types.Decor, "Ковер Азия (L)", "apa_mp_h_acc_rugwoolm_01");
                new ItemData("furn_35", Types.Decor, "Ковер Море (L)", "apa_mp_h_acc_rugwoolm_02");
                new ItemData("furn_36", Types.Decor, "Ковер Черное-Белое (L)", "apa_mp_h_acc_rugwoolm_03");
                new ItemData("furn_37", Types.Decor, "Ковер Геометрия (L)", "apa_mp_h_acc_rugwoolm_04");
                new ItemData("furn_38", Types.Decor, "Ковер Клетки (M)", "apa_mp_h_acc_rugwools_01");
                new ItemData("furn_39", Types.Decor, "Ковер Триплет (M)", "apa_mp_h_acc_rugwools_03");
                new ItemData("furn_40", Types.Decor, "Ароматические свечи", "apa_mp_h_acc_scent_sticks_01");
                new ItemData("furn_41", Types.Decor, "Набор керамических урн", "apa_mp_h_acc_tray_01");
                new ItemData("furn_42", Types.Decor, "Ваза Зебра", "apa_mp_h_acc_vase_01");
                new ItemData("furn_43", Types.Decor, "Ваза красная", "apa_mp_h_acc_vase_02");
                new ItemData("furn_44", Types.Decor, "Ваза из черного стекла", "apa_mp_h_acc_vase_04");
                new ItemData("furn_45", Types.Decor, "Ваза капля черная", "apa_mp_h_acc_vase_05");
                new ItemData("furn_160", Types.Decor, "Беговая дорожка", "apa_p_apdlc_treadmill_s");
                new ItemData("furn_171", Types.Decor, "Зеркало маленькое", "ap_m_art_2");
                new ItemData("furn_213", Types.Decor, "Деревянная решетка", "ap_m_prop_14");
                new ItemData("furn_214", Types.Decor, "Вешалка настенная для прихожей", "ap_m_prop_2");
                new ItemData("furn_215", Types.Decor, "Коврик плетеный бежевый", "ap_m_prop_3");
                new ItemData("furn_217", Types.Decor, "Коврик овальный голубой с длинным ворсом", "ap_m_prop_5");
                new ItemData("furn_220", Types.Decor, "Коврик прямоугольный серый", "ap_m_prop_8");
                new ItemData("furn_221", Types.Decor, "Коврик овальный голубой с коротким ворсом", "ap_m_prop_9");
                new ItemData("furn_243", Types.Decor, "Кейс с винтовкой", "bkr_prop_biker_gcase_s");
                new ItemData("furn_255", Types.Decor, "Игрушка супергероя", "bkr_prop_coke_doll");
                new ItemData("furn_257", Types.Decor, "Ёлка-модерн", "ch_prop_tree_01a");
                new ItemData("furn_258", Types.Decor, "Покерный стол", "cls_casino_poker");
                new ItemData("furn_263", Types.Decor, "Ковер Персидский шик", "cls_sf_rug_1");
                new ItemData("furn_269", Types.Decor, "Скульптура Рог", "ex_office_swag_ivory3");
                new ItemData("furn_270", Types.Decor, "Шкура животного на мебель", "ex_office_swag_ivory4");
                new ItemData("furn_271", Types.Decor, "Коллекция часов", "ex_office_swag_jewelwatch");
                new ItemData("furn_272", Types.Decor, "Большая коллекция часов", "ex_office_swag_jewelwatch2");
                new ItemData("furn_273", Types.Decor, "Огромная коллекция часов", "ex_office_swag_jewelwatch3");
                new ItemData("furn_274", Types.Decor, "Набор медикаментов оптом", "ex_office_swag_med1");
                new ItemData("furn_275", Types.Decor, "Набор медикаментов в кейсе", "ex_office_swag_med2");
                new ItemData("furn_276", Types.Decor, "Набор медикаментов в ящике", "ex_office_swag_med3");
                new ItemData("furn_280", Types.Decor, "Награда SecuroServ", "ex_prop_exec_award_plastic");
                new ItemData("furn_296", Types.Decor, "Мастерская из дерева", "gr_prop_gr_bench_04a");
                new ItemData("furn_297", Types.Decor, "Мастерская из металла", "gr_prop_gr_bench_04b");
                new ItemData("furn_299", Types.Decor, "Токарный старинный", "gr_prop_gr_lathe_01a");
                new ItemData("furn_300", Types.Decor, "Токарный прошлого поколения", "gr_prop_gr_lathe_01b");
                new ItemData("furn_301", Types.Decor, "Токарный современный", "gr_prop_gr_lathe_01c");
                new ItemData("furn_302", Types.Decor, "Сварочный аппарат", "gr_prop_gr_prop_welder_01a");
                new ItemData("furn_303", Types.Decor, "Станок старинный", "gr_prop_gr_speeddrill_01a");
                new ItemData("furn_304", Types.Decor, "Станок прошлого поколения", "gr_prop_gr_speeddrill_01b");
                new ItemData("furn_305", Types.Decor, "Станок современный", "gr_prop_gr_speeddrill_01c");
                new ItemData("furn_314", Types.Decor, "Ковер из натуральной кожи (XL)", "hei_heist_acc_rughidel_01");
                new ItemData("furn_315", Types.Decor, "Ковер Море (XL)", "hei_heist_acc_rugwooll_01");
                new ItemData("furn_316", Types.Decor, "Ковер плетеный темный (XL)", "hei_heist_acc_rugwooll_02");
                new ItemData("furn_350", Types.Decor, "Бюст известного человека", "hei_prop_hei_bust_01");
                new ItemData("furn_365", Types.Decor, "Домкрат", "imp_prop_axel_stand_01a");
                new ItemData("furn_366", Types.Decor, "Огромная бомба", "imp_prop_bomb_ball");
                new ItemData("furn_368", Types.Decor, "Мяч для регби", "lr2_prop_ibi_01");
                new ItemData("furn_370", Types.Decor, "Гитара аккустическая", "prop_acc_guitar_01");
                new ItemData("furn_371", Types.Decor, "Аэрохоккей", "prop_airhockey_01");
                new ItemData("furn_372", Types.Decor, "Загадочное яйцо", "prop_alien_egg_01");
                new ItemData("furn_374", Types.Decor, "Гантеля тяжелая", "prop_barbell_01");
                new ItemData("furn_375", Types.Decor, "Штанга тяжелая", "prop_barbell_02");
                new ItemData("furn_376", Types.Decor, "Штанга малая", "prop_barbell_30kg");
                new ItemData("furn_377", Types.Decor, "Штанга средняя", "prop_barbell_80kg");
                new ItemData("furn_392", Types.Decor, "Коллекция пляжных сумок (M)", "prop_beachbag_combo_01");
                new ItemData("furn_393", Types.Decor, "Коллекция пляжных сумок (L)", "prop_beachbag_combo_02");
                new ItemData("furn_394", Types.Decor, "Надувной круг", "prop_beach_ring_01");
                new ItemData("furn_395", Types.Decor, "Неоновая вывеска COLD BEER", "prop_beerneon");
                new ItemData("furn_396", Types.Decor, "Неоновая вывеска LIQUOR", "prop_beer_neon_01");
                new ItemData("furn_397", Types.Decor, "Неоновая вывеска BEER с пистолетами", "prop_beer_neon_02");
                new ItemData("furn_398", Types.Decor, "Неоновая вывеска BEER", "prop_beer_neon_03");
                new ItemData("furn_399", Types.Decor, "Неоновая вывеска Logger", "prop_beer_neon_04");
                new ItemData("furn_400", Types.Decor, "Часы большие Звезда", "prop_big_clock_01");
                new ItemData("furn_404", Types.Decor, "Бонго", "prop_bongos_01");
                new ItemData("furn_419", Types.Decor, "Шампанское в ведерке", "prop_champset");
                new ItemData("furn_420", Types.Decor, "Набор с шампанским", "prop_champ_cool");
                new ItemData("furn_422", Types.Decor, "Неоновая вывеска Cocktails", "prop_cockneon");
                new ItemData("furn_439", Types.Decor, "Декоративная катана", "prop_cs_katana_01");
                new ItemData("furn_441", Types.Decor, "Дартс", "prop_dart_bd_cab_01");
                new ItemData("furn_442", Types.Decor, "Шкаф с бонгами", "prop_disp_cabinet_01");
                new ItemData("furn_443", Types.Decor, "Покрышка от грузовика на стену", "prop_dock_ropetyre2");
                new ItemData("furn_444", Types.Decor, "Манекен", "prop_dummy_01");
                new ItemData("furn_446", Types.Decor, "Гитара электронная Рок", "prop_el_guitar_01");
                new ItemData("furn_447", Types.Decor, "Гитара электронная Хардкор", "prop_el_guitar_02");
                new ItemData("furn_448", Types.Decor, "Гитара электронная Классика", "prop_el_guitar_03");
                new ItemData("furn_450", Types.Decor, "Велотренажер черный", "prop_exercisebike");
                new ItemData("furn_451", Types.Decor, "Велотренажер профессиональный", "prop_exer_bike_01");
                new ItemData("furn_456", Types.Decor, "Розовый фламинго", "prop_flamingo");
                new ItemData("furn_457", Types.Decor, "Гантеля малая", "prop_freeweight_02");
                new ItemData("furn_458", Types.Decor, "Плетеная корзина", "prop_fruit_basket");
                new ItemData("furn_460", Types.Decor, "Часы настенные с короной", "prop_game_clock_01");
                new ItemData("furn_461", Types.Decor, "Часы настенные KRONOS", "prop_game_clock_02");
                new ItemData("furn_462", Types.Decor, "Гном с удочкой", "prop_gnome1");
                new ItemData("furn_463", Types.Decor, "Гном с фонарем", "prop_gnome2");
                new ItemData("furn_464", Types.Decor, "Гном игривый", "prop_gnome3");
                new ItemData("furn_465", Types.Decor, "Автомат с жвачкой", "prop_gumball_03");
                new ItemData("furn_467", Types.Decor, "Чемодан с идолом", "prop_idol_case");
                new ItemData("furn_482", Types.Decor, "Сейф элитный", "prop_ld_int_safe_01");
                new ItemData("furn_509", Types.Decor, "Коктейль Пина Колада", "prop_pinacolada");
                new ItemData("furn_510", Types.Decor, "Ананас", "prop_pineapple");
                new ItemData("furn_521", Types.Decor, "Бильярдный стол зеленый", "prop_pooltable_02");
                new ItemData("furn_522", Types.Decor, "Бильярдный стол фиолетовый", "prop_pooltable_3b");
                new ItemData("furn_523", Types.Decor, "Кий", "prop_pool_cue");
                new ItemData("furn_524", Types.Decor, "Киевница", "prop_pool_rack_01");
                new ItemData("furn_525", Types.Decor, "Бильярдный треугольник", "prop_pool_tri");
                new ItemData("furn_547", Types.Decor, "Скамья для жима с фиксаторами", "prop_pris_bench_01");
                new ItemData("furn_548", Types.Decor, "Ковбойская шляпа", "prop_proxy_hat_01");
                new ItemData("furn_550", Types.Decor, "Старый BMX", "prop_rub_bike_01");
                new ItemData("furn_557", Types.Decor, "Бонг декоративный", "prop_sh_bong_01");
                new ItemData("furn_558", Types.Decor, "Роза", "prop_single_rose");
                new ItemData("furn_571", Types.Decor, "Часы PENDULUS (L)", "prop_sports_clock_01");
                new ItemData("furn_590", Types.Decor, "Настольный теннис", "prop_table_tennis");
                new ItemData("furn_596", Types.Decor, "Коллекция футболок (XL)", "prop_tshirt_shelf_1");
                new ItemData("furn_613", Types.Decor, "Коллекция футболок (L)", "prop_t_shirt_row_03");
                new ItemData("furn_614", Types.Decor, "Коллекция футболок (M)", "prop_t_shirt_row_04");
                new ItemData("furn_617", Types.Decor, "Телескоп", "prop_t_telescope_01b");
                new ItemData("furn_630", Types.Decor, "Скамья для жима профессиональная", "prop_weight_bench_02");
                new ItemData("furn_631", Types.Decor, "Набор блинов для штанги", "prop_weight_rack_01");
                new ItemData("furn_632", Types.Decor, "Набор гантелей", "prop_weight_rack_02");
                new ItemData("furn_633", Types.Decor, "Штанга для тяги", "prop_weight_squat");
                new ItemData("furn_636", Types.Decor, "Ёлка с подарками", "prop_xmas_tree_int");
                new ItemData("furn_649", Types.Decor, "Старая игрушка бывшей", "p_mr_raspberry_01_s");
                new ItemData("furn_653", Types.Decor, "Сейф", "p_v_43_safe_s");
                new ItemData("furn_656", Types.Decor, "Коврик для йоги PROlaps", "p_yoga_mat_01_s");
                new ItemData("furn_657", Types.Decor, "Коврик для йоги черный", "p_yoga_mat_02_s");
                new ItemData("furn_658", Types.Decor, "Коврик для йоги красный", "p_yoga_mat_03_s");
                new ItemData("furn_661", Types.Decor, "Гимнастический мяч синий", "v_ilev_exball_blue");
                new ItemData("furn_662", Types.Decor, "Гимнастический мяч серый", "v_ilev_exball_grey");

                new ItemData("furn_295", Types.Workbench, "Крафт", "gr_prop_gr_bench_01a");
                new ItemData("furn_478", Types.PC, "Ноутбук", "prop_laptop_01a");
                new ItemData("furn_698", Types.Storage, "Склад", "xm_prop_crates_rifles_01a");

                var lines = new List<string>();

                foreach (var x in AllData)
                {
                    lines.Add($"new Furniture(\"{x.Key}\", Types.{x.Value.Type}, \"{x.Value.Name}\", {x.Value.Model});");
                }

                Utils.FillFileToReplaceRegion(Settings.DIR_CLIENT_FURNITURE_DATA_PATH, "TO_REPLACE_FURN_LIST", lines);

                return AllData.Count;
            }
        }

        [JsonProperty(PropertyName = "U")]
        public uint UID { get; set; }

        [JsonProperty(PropertyName = "I")]
        public string ID { get; set; }

        [JsonProperty(PropertyName = "D")]
        public Utils.Vector4 Data { get; set; }

        [JsonIgnore]
        public ItemData FurnitureData => ItemData.Get(ID);

        [JsonIgnore]
        public Game.Items.Craft.FurnitureWorkbench WorkbenchInstance => Game.Items.Craft.FurnitureWorkbench.Get(UID);

        public Furniture(uint UID, string ID, Utils.Vector4 Data)
        {
            this.UID = UID;

            this.ID = ID;

            this.Data = Data;
        }

        public Furniture(string ID)
        {
            this.ID = ID;

            Data = new Utils.Vector4(0f, 0f, 0f, 0f);

            Add(this);
        }

        public void Setup(Game.Estates.HouseBase houseBase)
        {
            if (FurnitureData.WorkbenchType is Items.Craft.Workbench.WorkbenchTypes wbType)
            {
                if (WorkbenchInstance != null)
                    return;

                var wb = new Game.Items.Craft.FurnitureWorkbench(UID, houseBase, wbType);
            }
        }

        public void Delete(Game.Estates.HouseBase houseBase)
        {
            if (FurnitureData.WorkbenchType is Items.Craft.Workbench.WorkbenchTypes wbType)
            {
                if (WorkbenchInstance is Game.Items.Craft.FurnitureWorkbench furnWb)
                {
                    furnWb.DropAllItemsToGround(Data.Position, Utils.ZeroVector, houseBase.Dimension);

                    furnWb.Delete();
                }
            }
        }
    }
}
