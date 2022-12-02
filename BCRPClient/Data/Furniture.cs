using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.Data
{
    public class Furniture
    {
        public static Dictionary<string, Furniture> All { get; set; } = new Dictionary<string, Furniture>();

        public static Furniture GetData(string id) => All.GetValueOrDefault(id);

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

        private static Dictionary<Types, Action<MapObject, object[]>> CreateActions = new Dictionary<Types, Action<MapObject, object[]>>()
        {
            {
                Types.Locker,

                (obj, args) =>
                {
                    obj.SetData("ContainerID", (uint)args[0]);
                }
            },

            {
                Types.Wardrobe,

                (obj, args) =>
                {
                    obj.SetData("ContainerID", (uint)args[0]);
                }
            },

            {
                Types.Fridge,

                (obj, args) =>
                {
                    obj.SetData("ContainerID", (uint)args[0]);
                }
            },
        };

        private static Dictionary<Types, Action<MapObject>> InteractionActions = new Dictionary<Types, Action<MapObject>>()
        {
            {
                Types.Locker,

                (obj) =>
                {
                    if (obj.GetData<uint?>("ContainerID") is uint contId)
                    {
                        CEF.Inventory.Show(CEF.Inventory.Types.Container, contId);
                    }
                }
            },

            {
                Types.Wardrobe,

                (obj) =>
                {
                    if (obj.GetData<uint?>("ContainerID") is uint contId)
                    {
                        CEF.Inventory.Show(CEF.Inventory.Types.Container, contId);
                    }
                }
            },

            {
                Types.Fridge,

                (obj) =>
                {
                    if (obj.GetData<uint?>("ContainerID") is uint contId)
                    {
                        CEF.Inventory.Show(CEF.Inventory.Types.Container, contId);
                    }
                }
            },
        };

        public string Id { get; set; }

        public string Name { get; set; }

        public int Price { get; set; }

        public Types Type { get; set; }

        public uint Model { get; set; }

        public Action<MapObject> InteractionAction => InteractionActions.GetValueOrDefault(Type);

        public Furniture(string Id, Types Type, string Name, string Model, int Price)
        {
            this.Id = Id;
            this.Type = Type;
            this.Name = Name;

            this.Model = RAGE.Util.Joaat.Hash(Model);

            this.Price = Price;

            All.Add(Id, this);
        }

        public static Action<MapObject> GetIntareactionAction(string id) => InteractionActions.GetValueOrDefault(All[id].Type);

        public static Action<MapObject, object[]> GetCreateAction(string id) => CreateActions.GetValueOrDefault(All[id].Type);

        public async System.Threading.Tasks.Task<MapObject> CreateObject(Vector3 pos, Vector3 rot, uint dim, uint uid, params object[] args)
        {
            await Utils.RequestModel(Model);

            var obj = new MapObject(Model, pos, rot, 255, dim);

            obj.SetData("UID", uid);

            var cAct = CreateActions.GetValueOrDefault(Type);

            if (cAct == null)
                return obj;

            obj.SetData("Interactive", true);

            obj.SetData("Furniture", this);

            cAct.Invoke(obj, args);

            return obj;
        }

        public MapObject CreateTempObject(Vector3 pos, Vector3 rot, uint dim) => new MapObject(Model, pos, rot, 125, dim);

        public static void LoadAll()
        {
            new Furniture("furn_91", Types.Chair, "Позднеренессансное кресло", "apa_mp_h_stn_chairarm_01", 100);
            new Furniture("furn_96", Types.Chair, "Кресло в стиле модерн", "apa_mp_h_stn_chairarm_12", 100);
            new Furniture("furn_98", Types.Chair, "Раскладное кресло (малиновое)", "apa_mp_h_stn_chairarm_23", 100);
            new Furniture("furn_99", Types.Chair, "Кресло обычное", "apa_mp_h_stn_chairarm_24", 100);
            new Furniture("furn_100", Types.Chair, "Пуфик оранжевый", "apa_mp_h_stn_chairarm_25", 100);
            new Furniture("furn_101", Types.Chair, "Современное кресло (фиолотовое)", "apa_mp_h_stn_chairarm_26", 100);
            new Furniture("furn_103", Types.Chair, "Раскладное кресло (оранжевое)", "apa_mp_h_stn_chairstrip_01", 100);
            new Furniture("furn_104", Types.Chair, "Позднеренессансное кресло (оранжевое)", "apa_mp_h_stn_chairstrip_02", 100);
            new Furniture("furn_105", Types.Chair, "Раскладное кресло (вишневое)", "apa_mp_h_stn_chairstrip_03", 100);
            new Furniture("furn_106", Types.Chair, "Раскладное кресло (бордовое)", "apa_mp_h_stn_chairstrip_04", 100);
            new Furniture("furn_107", Types.Chair, "Раскладное кресло (белое)", "apa_mp_h_stn_chairstrip_05", 100);
            new Furniture("furn_108", Types.Chair, "Позднеренессансное кресло (ярко оранжевое)", "apa_mp_h_stn_chairstrip_06", 100);
            new Furniture("furn_109", Types.Chair, "Позднеренессансное кресло (черное)", "apa_mp_h_stn_chairstrip_07", 100);
            new Furniture("furn_110", Types.Chair, "Позднеренессансное кресло (бирюзовое)", "apa_mp_h_stn_chairstrip_08", 100);
            new Furniture("furn_147", Types.Chair, "Раскладное кресло (кремовое)", "apa_mp_h_yacht_armchair_01", 100);
            new Furniture("furn_148", Types.Chair, "Дизайнерское кресло", "apa_mp_h_yacht_armchair_03", 100);
            new Furniture("furn_149", Types.Chair, "Кресло низкое (коричневое)", "apa_mp_h_yacht_armchair_04", 100);
            new Furniture("furn_157", Types.Chair, "Раскладное кресло (снежное)", "apa_mp_h_yacht_strip_chair_01", 100);
            new Furniture("furn_229", Types.Chair, "Кресло босса", "ba_prop_battle_club_chair_02", 100);
            new Furniture("furn_230", Types.Chair, "Кресло офисное серое", "ba_prop_battle_club_chair_03", 100);
            new Furniture("furn_239", Types.Chair, "Раскладное кресло (коричневое)", "bkr_prop_biker_chairstrip_01", 100);
            new Furniture("furn_240", Types.Chair, "Кресло старое", "bkr_prop_biker_chairstrip_02", 100);
            new Furniture("furn_284", Types.Chair, "Кресло кожаное", "ex_prop_offchair_exec_04", 100);
            new Furniture("furn_289", Types.Chair, "Кресло кухонное (линии)", "gr_dlc_gr_yacht_props_seat_02", 100);
            new Furniture("furn_328", Types.Chair, "Позднеренессансное кресло (пестрое)", "hei_heist_stn_chairarm_01", 100);
            new Furniture("furn_329", Types.Chair, "Кресло элегантное", "hei_heist_stn_chairarm_03", 100);
            new Furniture("furn_330", Types.Chair, "Кресло в стиле модерн (сливовое)", "hei_heist_stn_chairarm_04", 100);
            new Furniture("furn_331", Types.Chair, "Кресло крапо", "hei_heist_stn_chairarm_06", 100);
            new Furniture("furn_332", Types.Chair, "Позднеренессансное кресло (апельсиновое)", "hei_heist_stn_chairstrip_01", 100);
            new Furniture("furn_367", Types.Chair, "Коресло офисное", "imp_prop_impexp_offchair_01a", 100);
            new Furniture("furn_563", Types.Chair, "Кресло босса черное", "prop_sol_chair", 100);
            new Furniture("furn_637", Types.Chair, "Кресло кухонное( желтое)", "prop_yaught_chair_01", 100);
            new Furniture("furn_639", Types.Chair, "Кресло модное (белое)", "p_armchair_01_s", 100);
            new Furniture("furn_642", Types.Chair, "Кресло стильное (серое)", "p_ilev_p_easychair_s", 100);
            new Furniture("furn_652", Types.Chair, "Кресло руководителя", "p_soloffchair_s", 100);
            new Furniture("furn_659", Types.Chair, "Кресло руководителя черное", "sm_prop_offchair_smug_01", 100);
            new Furniture("furn_663", Types.Chair, "Кресло парикмахерское", "v_ilev_hd_chair", 100);
            new Furniture("furn_664", Types.Chair, "Кресло руководителя кожаное", "v_ilev_leath_chr", 100);
            new Furniture("furn_693", Types.Chair, "Кресло в стиле модерн (черное)", "xm_lab_chairarm_12", 100);
            new Furniture("furn_695", Types.Chair, "Современное кресло (серое)", "xm_lab_chairarm_26", 100);

            new Furniture("furn_57", Types.Chair, "Стул белый офисный", "apa_mp_h_din_chair_04", 100);
            new Furniture("furn_58", Types.Chair, "Стул белый офисный №2", "apa_mp_h_din_chair_08", 100);
            new Furniture("furn_59", Types.Chair, "Стул Карлос", "apa_mp_h_din_chair_09", 100);
            new Furniture("furn_60", Types.Chair, "Стул деревянный", "apa_mp_h_din_chair_12", 100);
            new Furniture("furn_92", Types.Chair, "Стул история", "apa_mp_h_stn_chairarm_02", 100);
            new Furniture("furn_93", Types.Chair, "Стул Самба", "apa_mp_h_stn_chairarm_03", 100);
            new Furniture("furn_94", Types.Chair, "Стул круговой", "apa_mp_h_stn_chairarm_09", 100);
            new Furniture("furn_95", Types.Chair, "Стул портфино", "apa_mp_h_stn_chairarm_11", 100);
            new Furniture("furn_97", Types.Chair, "Стул компьютерный", "apa_mp_h_stn_chairarm_13", 100);
            new Furniture("furn_102", Types.Chair, "Стул дизайнерский", "apa_mp_h_stn_chairstool_12", 100);
            new Furniture("furn_150", Types.Chair, "Стул клубный", "apa_mp_h_yacht_barstool_01", 100);
            new Furniture("furn_234", Types.Chair, "Стул клубный №2", "bkr_prop_biker_barstool_02", 100);
            new Furniture("furn_235", Types.Chair, "Стул барный", "bkr_prop_biker_barstool_03", 100);
            new Furniture("furn_236", Types.Chair, "Стул барный №2", "bkr_prop_biker_barstool_04", 100);
            new Furniture("furn_237", Types.Chair, "Стул офисный", "bkr_prop_biker_boardchair01", 100);
            new Furniture("furn_241", Types.Chair, "Стул деревянный №2", "bkr_prop_biker_chair_01", 100);
            new Furniture("furn_245", Types.Chair, "Стул старый", "bkr_prop_clubhouse_arm_wrestle_01a", 100);
            new Furniture("furn_247", Types.Chair, "Стул клубный", "bkr_prop_clubhouse_chair_01", 100);
            new Furniture("furn_252", Types.Chair, "Стул компьютерный старый", "bkr_prop_clubhouse_offchair_01a", 100);
            new Furniture("furn_256", Types.Chair, "Стул Магнат", "bkr_prop_weed_chair_01a", 100);
            new Furniture("furn_259", Types.Chair, "Стул роскошный красный", "cls_h4_int_04_desk_chair", 100);
            new Furniture("furn_282", Types.Chair, "Стул офисный деловой", "ex_prop_offchair_exec_01", 100);
            new Furniture("furn_283", Types.Chair, "Стул офисный №2 (черный)", "ex_prop_offchair_exec_03", 100);
            new Furniture("furn_288", Types.Chair, "Стул кухонный (в полоску)", "gr_dlc_gr_yacht_props_seat_01", 100);
            new Furniture("furn_364", Types.Chair, "Стул для отдыха", "hei_prop_hei_skid_chair", 100);
            new Furniture("furn_373", Types.Chair, "Стул старый вязаный", "prop_armchair_01", 100);
            new Furniture("furn_391", Types.Chair, "Стул барный №3", "prop_bar_stool_01", 100);
            new Furniture("furn_406", Types.Chair, "Стул открытый", "prop_chair_01a", 100);
            new Furniture("furn_407", Types.Chair, "Стул открытый №2", "prop_chair_01b", 100);
            new Furniture("furn_408", Types.Chair, "Стул Виктор", "prop_chair_02", 100);
            new Furniture("furn_409", Types.Chair, "Стул греческий", "prop_chair_03", 100);
            new Furniture("furn_410", Types.Chair, "Стул с тигровым принтом", "prop_chair_04a", 100);
            new Furniture("furn_411", Types.Chair, "Стул динко (белый)", "prop_chair_04b", 100);
            new Furniture("furn_412", Types.Chair, "Стул кухонный (старый)", "prop_chair_05", 100);
            new Furniture("furn_413", Types.Chair, "Стул столовый", "prop_chair_06", 100);
            new Furniture("furn_414", Types.Chair, "Стул классический", "prop_chair_07", 100);
            new Furniture("furn_415", Types.Chair, "Стул пластмассовый", "prop_chair_08", 100);
            new Furniture("furn_416", Types.Chair, "Стул садовый", "prop_chair_09", 100);
            new Furniture("furn_417", Types.Chair, "Стул садовый №2", "prop_chair_10", 100);
            new Furniture("furn_418", Types.Chair, "Стулья пластмассовые ", "prop_chair_pile_01", 100);
            new Furniture("furn_421", Types.Chair, "Стул Chateau", "prop_chateau_chair_01", 100);
            new Furniture("furn_494", Types.Chair, "Стул офисный №3", "prop_off_chair_04", 100);
            new Furniture("furn_495", Types.Chair, "Стул офисный №4", "prop_off_chair_04_s", 100);
            new Furniture("furn_496", Types.Chair, "Стул офисный №5", "prop_off_chair_05", 100);
            new Furniture("furn_578", Types.Chair, "Стул деревянный №2", "prop_table_01_chr_a", 100);
            new Furniture("furn_580", Types.Chair, "Стул деревяный №3", "prop_table_02_chr", 100);
            new Furniture("furn_582", Types.Chair, "Стул пластмассовый №2", "prop_table_03_chr", 100);
            new Furniture("furn_584", Types.Chair, "Стул садовый №3", "prop_table_04_chr", 100);
            new Furniture("furn_586", Types.Chair, "Стул в переплете", "prop_table_05_chr", 100);
            new Furniture("furn_588", Types.Chair, "Стул Генрих", "prop_table_06_chr", 100);
            new Furniture("furn_622", Types.Chair, "Стул кожаный", "prop_waiting_seat_01", 100);
            new Furniture("furn_641", Types.Chair, "Стул Dine", "p_dinechair_01_s", 100);
            new Furniture("furn_680", Types.Chair, "Стул колониальный", "v_res_m_l_chair1", 100);
            new Furniture("furn_687", Types.Chair, "Стул колониальный красный", "v_ret_chair", 100);
            new Furniture("furn_688", Types.Chair, "Стул колониальный белый", "v_ret_chair_white", 100);
            new Furniture("furn_690", Types.Chair, "Стул Лициор", "xm_lab_chairarm_02", 100);
            new Furniture("furn_691", Types.Chair, "Стул рико", "xm_lab_chairarm_03", 100);
            new Furniture("furn_692", Types.Chair, "Стул рико №2", "xm_lab_chairarm_11", 100);
            new Furniture("furn_694", Types.Chair, "Стул офисный дизайнерский", "xm_lab_chairarm_24", 100);

            new Furniture("furn_52", Types.Bed, "Кровать двуспальная", "apa_mp_h_bed_double_08", 100);
            new Furniture("furn_53", Types.Bed, "Кровать двуспальная на платформе", "apa_mp_h_bed_double_09", 100);
            new Furniture("furn_55", Types.Bed, "Кровать двуспальная на красной платформе", "apa_mp_h_bed_wide_05", 100);
            new Furniture("furn_56", Types.Bed, "Кровать двуспальная современная", "apa_mp_h_bed_with_table_02", 100);
            new Furniture("furn_111", Types.Bed, "Диван обычный с подушками", "apa_mp_h_stn_sofa2seat_02", 100);
            new Furniture("furn_112", Types.Bed, "Угловой серый диван", "apa_mp_h_stn_sofacorn_01", 100);
            new Furniture("furn_113", Types.Bed, "Угловой серый дизайнерский диван", "apa_mp_h_stn_sofacorn_05", 100);
            new Furniture("furn_114", Types.Bed, "Угловоый зелёный диван", "apa_mp_h_stn_sofacorn_06", 100);
            new Furniture("furn_115", Types.Bed, "Угловой неоновый диван", "apa_mp_h_stn_sofacorn_07", 100);
            new Furniture("furn_116", Types.Bed, "Угловой диван цвета какао", "apa_mp_h_stn_sofacorn_08", 100);
            new Furniture("furn_117", Types.Bed, "Угловой строгий диван", "apa_mp_h_stn_sofacorn_09", 100);
            new Furniture("furn_118", Types.Bed, "Угловой белый диван", "apa_mp_h_stn_sofacorn_10", 100);
            new Furniture("furn_119", Types.Bed, "Лежак коричневый", "apa_mp_h_stn_sofa_daybed_01", 100);
            new Furniture("furn_120", Types.Bed, "Лежак черный", "apa_mp_h_stn_sofa_daybed_02", 100);
            new Furniture("furn_151", Types.Bed, "Кроваь двуспальная стильная", "apa_mp_h_yacht_bed_01", 100);
            new Furniture("furn_152", Types.Bed, "Кровать двуспальная дизайнерская", "apa_mp_h_yacht_bed_02", 100);
            new Furniture("furn_155", Types.Bed, "Диван большой кремовый", "apa_mp_h_yacht_sofa_01", 100);
            new Furniture("furn_156", Types.Bed, "Диван с низкой посадкой (коричневый)", "apa_mp_h_yacht_sofa_02", 100);
            new Furniture("furn_238", Types.Bed, "Лежак байкерский", "bkr_prop_biker_campbed_01", 100);
            new Furniture("furn_253", Types.Bed, "Диван дизайнерский (черный)", "bkr_prop_clubhouse_sofa_01a", 100);
            new Furniture("furn_287", Types.Bed, "Лежак пляжный", "gr_dlc_gr_yacht_props_lounger", 100);
            new Furniture("furn_290", Types.Bed, "Диван кухонный (в полоску)", "gr_dlc_gr_yacht_props_seat_03", 100);
            new Furniture("furn_294", Types.Bed, "Кровать обычная", "gr_prop_bunker_bed_01", 100);
            new Furniture("furn_333", Types.Bed, "Диван с платформой", "hei_heist_stn_sofa2seat_02", 100);
            new Furniture("furn_334", Types.Bed, "Диван обычный (коричневый)", "hei_heist_stn_sofa2seat_03", 100);
            new Furniture("furn_335", Types.Bed, "Угловой лазурный диван", "hei_heist_stn_sofacorn_05", 100);
            new Furniture("furn_336", Types.Bed, "Угловой салатовый диван", "hei_heist_stn_sofacorn_06", 100);
            new Furniture("furn_369", Types.Bed, "Диван старый", "miss_rub_couch_01_l1", 100);
            new Furniture("furn_428", Types.Bed, "Диван в стиле лофт", "prop_couch_01", 100);
            new Furniture("furn_429", Types.Bed, "Диван классический", "prop_couch_03", 100);
            new Furniture("furn_430", Types.Bed, "Диван в стиле лофт (№2)", "prop_couch_04", 100);
            new Furniture("furn_431", Types.Bed, "Диван в стиле прованс (коричневый)", "prop_couch_lg_02", 100);
            new Furniture("furn_432", Types.Bed, "Диван семейный", "prop_couch_lg_05", 100);
            new Furniture("furn_433", Types.Bed, "Диван в стиле лофт (коричневый)", "prop_couch_lg_06", 100);
            new Furniture("furn_434", Types.Bed, "Диван в стиле прованс (белый)", "prop_couch_lg_07", 100);
            new Furniture("furn_435", Types.Bed, "Диван современный", "prop_couch_lg_08", 100);
            new Furniture("furn_436", Types.Bed, "Дополнение к современному дивану (№1)", "prop_couch_sm1_07", 100);
            new Furniture("furn_437", Types.Bed, "Дополнение к современному дивану (№2)", "prop_couch_sm2_07", 100);
            new Furniture("furn_438", Types.Bed, "Диван в стиле лофт (кофейный)", "prop_couch_sm_06", 100);
            new Furniture("furn_497", Types.Bed, "Лежак пляжный (деревянный)", "prop_patio_lounger1", 100);
            new Furniture("furn_498", Types.Bed, "Лежак пляжный (камуфляж)", "prop_patio_lounger_2", 100);
            new Furniture("furn_499", Types.Bed, "Лежак пляжный (пластмассовый)", "prop_patio_lounger_3", 100);
            new Furniture("furn_554", Types.Bed, "Диван старый в стиле лофт", "prop_rub_couch01", 100);
            new Furniture("furn_555", Types.Bed, "Диван старый (№2)", "prop_rub_couch02", 100);
            new Furniture("furn_556", Types.Bed, "Диван старый (№3)", "prop_rub_couch04", 100);
            new Furniture("furn_615", Types.Bed, "Кровать современная на платформе", "prop_t_sofa", 100);
            new Furniture("furn_616", Types.Bed, "Кровать современная на платформе №2", "prop_t_sofa_02", 100);
            new Furniture("furn_623", Types.Bed, "Диван кожаный без спинки (черный)", "prop_wait_bench_01", 100);
            new Furniture("furn_638", Types.Bed, "Диван кухонный (желтый)", "prop_yaught_sofa_01", 100);
            new Furniture("furn_646", Types.Bed, "Кровать застеленная", "p_lestersbed_s", 100);
            new Furniture("furn_647", Types.Bed, "Диван раскладной (белый)", "p_lev_sofa_s", 100);
            new Furniture("furn_648", Types.Bed, "Кровать графа", "p_mbbed_s", 100);
            new Furniture("furn_650", Types.Bed, "Лежак пляжный (апельсиновый)", "p_patio_lounger1_s", 100);
            new Furniture("furn_651", Types.Bed, "Диван ковровый", "p_res_sofa_l_s", 100);
            new Furniture("furn_654", Types.Bed, "Диван длинный (кожаный)", "p_v_med_p_sofa_s", 100);
            new Furniture("furn_655", Types.Bed, "Кровать застеленная (спальная)", "p_v_res_tt_bed_s", 100);
            new Furniture("furn_666", Types.Bed, "Диван раскладной (снежный)", "v_ilev_m_sofa", 100);
            new Furniture("furn_676", Types.Bed, "Кровать расправленная (красная)", "v_res_msonbed_s", 100);
            new Furniture("furn_681", Types.Bed, "Диван классический (синий)", "v_res_tre_sofa_s", 100);
            new Furniture("furn_696", Types.Bed, "Диван модный (белый)", "xm_lab_sofa_01", 100);
            new Furniture("furn_697", Types.Bed, "Диван кожаный (черный)", "xm_lab_sofa_02", 100);

            new Furniture("furn_61", Types.Table, "Стол стеклянный (журнальный)", "apa_mp_h_din_table_01", 100);
            new Furniture("furn_62", Types.Table, "Стол стеклянный с контурами (журнальный)", "apa_mp_h_din_table_04", 100);
            new Furniture("furn_63", Types.Table, "Стол стильный белый (журнальный)", "apa_mp_h_din_table_06", 100);
            new Furniture("furn_64", Types.Table, "Стол стеклянный с контурами №2 (журнальный)", "apa_mp_h_din_table_11", 100);
            new Furniture("furn_138", Types.Table, "Стол стеклянный", "apa_mp_h_str_sideboards_02", 100);
            new Furniture("furn_139", Types.Table, "Стол стеклянный треугольник", "apa_mp_h_tab_coffee_07", 100);
            new Furniture("furn_140", Types.Table, "Стол квадратный белый (журнальный)", "apa_mp_h_tab_coffee_08", 100);
            new Furniture("furn_141", Types.Table, "Стол стеклянный квадрат (журнальный)", "apa_mp_h_tab_sidelrg_01", 100);
            new Furniture("furn_142", Types.Table, "Стол стеклянный квадрат №2 (журнальный)", "apa_mp_h_tab_sidelrg_02", 100);
            new Furniture("furn_143", Types.Table, "Барная стойка", "apa_mp_h_tab_sidelrg_04", 100);
            new Furniture("furn_144", Types.Table, "Стол стеклянный квадрат", "apa_mp_h_tab_sidelrg_07", 100);
            new Furniture("furn_145", Types.Table, "Стол стеклянный дизайнерский", "apa_mp_h_tab_sidesml_01", 100);
            new Furniture("furn_146", Types.Table, "Стол стеклянный куб", "apa_mp_h_tab_sidesml_02", 100);
            new Furniture("furn_153", Types.Table, "Стол в виде куба", "apa_mp_h_yacht_side_table_01", 100);
            new Furniture("furn_154", Types.Table, "Стол дизайнерский (круглый)", "apa_mp_h_yacht_side_table_02", 100);
            new Furniture("furn_246", Types.Table, "Стол на стойке", "bkr_prop_clubhouse_arm_wrestle_02a", 100);
            new Furniture("furn_266", Types.Table, "Стол письменный Роскошь", "cls_sf_table_1", 100);
            new Furniture("furn_291", Types.Table, "Стол обтянутый тканью", "gr_dlc_gr_yacht_props_table_01", 100);
            new Furniture("furn_292", Types.Table, "Стол обтянутый тканью (журнальный)", "gr_dlc_gr_yacht_props_table_02", 100);
            new Furniture("furn_293", Types.Table, "Стол обтянутый тканью №2", "gr_dlc_gr_yacht_props_table_03", 100);
            new Furniture("furn_327", Types.Table, "Стол дизайнерский (журнальный)", "hei_heist_stn_benchshort", 100);
            new Furniture("furn_480", Types.Table, "Стол с телевизором", "prop_ld_farm_table01", 100);
            new Furniture("furn_481", Types.Table, "Стол с бутылками", "prop_ld_farm_table02", 100);
            new Furniture("furn_577", Types.Table, "Стол журнальный с кофе", "prop_table_01", 100);
            new Furniture("furn_579", Types.Table, "Стол деревянный (круглый)", "prop_table_02", 100);
            new Furniture("furn_581", Types.Table, "Стол прямоугольный (белый)", "prop_table_03", 100);
            new Furniture("furn_583", Types.Table, "Стол прямоугольный (деревянный)", "prop_table_04", 100);
            new Furniture("furn_585", Types.Table, "Стол деревянный (круглый №2)", "prop_table_05", 100);
            new Furniture("furn_587", Types.Table, "Стол из заведения", "prop_table_06", 100);
            new Furniture("furn_589", Types.Table, "Барный стол", "prop_table_07", 100);
            new Furniture("furn_665", Types.Table, "Стол дизайнерский (журнальный №2)", "v_ilev_liconftable_sml", 100);
            new Furniture("furn_671", Types.Table, "Стол консоль тонкий", "v_res_mconsoletrad", 100);
            new Furniture("furn_678", Types.Table, "Стол консоль современный", "v_res_m_console", 100);
            new Furniture("furn_679", Types.Table, "Стол консоль резной", "v_res_m_h_console", 100);

            new Furniture("furn_51", Types.Locker, "Комод классический (черный)", "apa_mp_h_bed_chestdrawer_02", 100);
            new Furniture("furn_54", Types.Locker, "2 тумбочки (красные)", "apa_mp_h_bed_table_wide_12", 100);
            new Furniture("furn_127", Types.Locker, "Шкаф классический", "apa_mp_h_str_shelffloorm_02", 100);
            new Furniture("furn_128", Types.Locker, "Шкаф открытый (белый)", "apa_mp_h_str_shelffreel_01", 100);
            new Furniture("furn_129", Types.Locker, "Шкаф открытый (коричневый)", "apa_mp_h_str_shelfwallm_01", 100);
            new Furniture("furn_130", Types.Locker, "Тумбочка белая", "apa_mp_h_str_sideboardl_06", 100);
            new Furniture("furn_131", Types.Locker, "Тумбочка серая", "apa_mp_h_str_sideboardl_09", 100);
            new Furniture("furn_132", Types.Locker, "Тумба коричневая", "apa_mp_h_str_sideboardl_11", 100);
            new Furniture("furn_133", Types.Locker, "Тумба серая", "apa_mp_h_str_sideboardl_13", 100);
            new Furniture("furn_134", Types.Locker, "Тумба открытая", "apa_mp_h_str_sideboardl_14", 100);
            new Furniture("furn_135", Types.Locker, "Тумба коричневая", "apa_mp_h_str_sideboardm_02", 100);
            new Furniture("furn_136", Types.Locker, "Тумба белая №2", "apa_mp_h_str_sideboardm_03", 100);
            new Furniture("furn_137", Types.Locker, "Тумба дизайнерская", "apa_mp_h_str_sideboards_01", 100);
            new Furniture("furn_196", Types.Locker, "Антресоль темная (L)", "ap_m_cooking_17", 100);
            new Furniture("furn_199", Types.Locker, "Антресоль белая с боковым шкафом (L)", "ap_m_cooking_2", 100);
            new Furniture("furn_209", Types.Locker, "Полка деревянная одиночная", "ap_m_prop_10", 100);
            new Furniture("furn_211", Types.Locker, "Стойка деревянная вытянутая", "ap_m_prop_12", 100);
            new Furniture("furn_222", Types.Wardrobe, "Гардероб стеллаж Luxe (1)", "ap_m_shelf_1", 100);
            new Furniture("furn_223", Types.Locker, "Стойка деревянная квадратная", "ap_m_shelf_2", 100);
            new Furniture("furn_224", Types.Locker, "Полка деревянная двойная темная", "ap_m_shelf_3", 100);
            new Furniture("furn_225", Types.Wardrobe, "Гардероб деревянный Бизнес", "ap_m_shelf_4", 100);
            new Furniture("furn_226", Types.Locker, "Шкаф деревянный со светлой отделкой (слева)", "ap_m_shelf_5", 100);
            new Furniture("furn_227", Types.Wardrobe, "Гардероб стеллаж Luxe (2)", "ap_m_shelf_6", 100);
            new Furniture("furn_228", Types.Locker, "Шкаф деревянный со светлой отделкой (справа)", "ap_m_shelf_7", 100);
            new Furniture("furn_242", Types.Locker, "Шкаф маленький с одеждой", "bkr_prop_biker_garage_locker_01", 100);
            new Furniture("furn_244", Types.Locker, "Шкаф открытый", "bkr_prop_biker_safebody_01a", 100);
            new Furniture("furn_264", Types.Locker, "Книжный шкаф угловой", "cls_sf_shelf_1", 100);
            new Furniture("furn_265", Types.Locker, "Книжный шкаф прямой", "cls_sf_shelf_2", 100);
            new Furniture("furn_317", Types.Locker, "Комод классический (кофейный)", "hei_heist_bed_chestdrawer_04", 100);
            new Furniture("furn_318", Types.Locker, "2 тумбочки (кофейные)", "hei_heist_bed_table_dble_04", 100);
            new Furniture("furn_340", Types.Locker, "Тумбочка классическая (белая)", "hei_heist_str_sideboardl_02", 100);
            new Furniture("furn_341", Types.Locker, "Тумбочка классическая (коричневая)", "hei_heist_str_sideboardl_03", 100);
            new Furniture("furn_342", Types.Locker, "Тумбочка классическая (синяя)", "hei_heist_str_sideboardl_04", 100);
            new Furniture("furn_343", Types.Locker, "Тумбочка классическая (белая)", "hei_heist_str_sideboardl_05", 100);
            new Furniture("furn_459", Types.Locker, "Ярмарочная тумбочка", "prop_funfair_zoltan", 100);
            new Furniture("furn_551", Types.Locker, "Комод бюджетный", "prop_rub_cabinet01", 100);
            new Furniture("furn_552", Types.Locker, "Шкаф открытый простой", "prop_rub_cabinet02", 100);
            new Furniture("furn_553", Types.Locker, "Шкаф открытый широкий", "prop_rub_cabinet03", 100);
            new Furniture("furn_604", Types.Locker, "Тумба под TV", "prop_tv_cabinet_03", 100);
            new Furniture("furn_605", Types.Locker, "Тумба под TV простая", "prop_tv_cabinet_04", 100);
            new Furniture("furn_606", Types.Locker, "Тумба под TV со свалки", "prop_tv_cabinet_05", 100);
            new Furniture("furn_619", Types.Locker, "Тумбочка (кремовая)", "prop_venice_counter_01", 100);
            new Furniture("furn_620", Types.Locker, "Тумбочка с бонгами", "prop_venice_counter_02", 100);
            new Furniture("furn_621", Types.Locker, "Тумбочка с шампунем", "prop_venice_counter_04", 100);
            new Furniture("furn_668", Types.Locker, "Шкаф резной застекленный", "v_res_cabinet", 100);
            new Furniture("furn_669", Types.Locker, "Старинное трюмо", "v_res_d_dressingtable", 100);
            new Furniture("furn_670", Types.Locker, "Комод резной старинный", "v_res_mconsolemod", 100);
            new Furniture("furn_672", Types.Locker, "Трюмо кинозвезды", "v_res_mddresser", 100);
            new Furniture("furn_677", Types.Locker, "Стенка гика с электроникой", "v_res_msoncabinet", 100);
            new Furniture("furn_682", Types.Locker, "Комод крохотный", "v_res_tre_storageunit", 100);
            new Furniture("furn_683", Types.Locker, "Шкаф белый простой", "v_res_tre_wardrobe", 100);

            new Furniture("furn_29", Types.Plant, "Растение Хамедория", "apa_mp_h_acc_plant_palm_01", 100);
            new Furniture("furn_30", Types.Plant, "Растение Сансевьера", "apa_mp_h_acc_plant_tall_01", 100);
            new Furniture("furn_46", Types.Plant, "Цветы Каллы", "apa_mp_h_acc_vase_flowers_01", 100);
            new Furniture("furn_47", Types.Plant, "Цветы Флоксы", "apa_mp_h_acc_vase_flowers_02", 100);
            new Furniture("furn_48", Types.Plant, "Цветы Белокрыльники", "apa_mp_h_acc_vase_flowers_03", 100);
            new Furniture("furn_49", Types.Plant, "Цветы Колокольчики", "apa_mp_h_acc_vase_flowers_04", 100);
            new Furniture("furn_312", Types.Plant, "Цветы Белокрыльники №2", "hei_heist_acc_flowers_01", 100);
            new Furniture("furn_313", Types.Plant, "Растение Физализ", "hei_heist_acc_flowers_02", 100);
            new Furniture("furn_455", Types.Plant, "Комнатное декоративное растение", "prop_fib_plant_01", 100);
            new Furniture("furn_507", Types.Plant, "Кувшинки в горшке", "prop_peyote_highland_02", 100);
            new Furniture("furn_508", Types.Plant, "Кувшинка", "prop_peyote_water_01", 100);
            new Furniture("furn_511", Types.Plant, "Растение Монстера", "prop_plant_int_01a", 100);
            new Furniture("furn_512", Types.Plant, "Растение Монстера №2", "prop_plant_int_01b", 100);
            new Furniture("furn_513", Types.Plant, "Декоративное комнатное растение в горшке", "prop_plant_int_02a", 100);
            new Furniture("furn_514", Types.Plant, "Декоративное комнатное растение в вазе", "prop_plant_int_03a", 100);
            new Furniture("furn_515", Types.Plant, "Декоративное комнатное растение в вазе №2", "prop_plant_int_03b", 100);
            new Furniture("furn_516", Types.Plant, "Растение Монстера №3", "prop_plant_int_04a", 100);
            new Furniture("furn_517", Types.Plant, "Декоративное комнатное дерево", "prop_plant_int_04b", 100);
            new Furniture("furn_518", Types.Plant, "Растение Блехнум", "prop_plant_int_04c", 100);
            new Furniture("furn_519", Types.Plant, "Декоративная пальма", "prop_plant_palm_01a", 100);
            new Furniture("furn_520", Types.Plant, "Декоративные пальмы №2", "prop_plant_palm_01c", 100);
            new Furniture("furn_526", Types.Plant, "Растение Хлорофитум", "prop_pot_plant_01a", 100);
            new Furniture("furn_527", Types.Plant, "Растение Хлорофитум №2", "prop_pot_plant_01b", 100);
            new Furniture("furn_528", Types.Plant, "Растение Аспарагус", "prop_pot_plant_01c", 100);
            new Furniture("furn_529", Types.Plant, "Растение Хамедория Элеганс", "prop_pot_plant_01d", 100);
            new Furniture("furn_530", Types.Plant, "Растение Революта", "prop_pot_plant_01e", 100);
            new Furniture("furn_531", Types.Plant, "Декоративное комнатное растение", "prop_pot_plant_03a", 100);
            new Furniture("furn_532", Types.Plant, "Декоративное комнатное растение №2", "prop_pot_plant_03c", 100);
            new Furniture("furn_533", Types.Plant, "Декоративное комнатное растение №3", "prop_pot_plant_04a", 100);
            new Furniture("furn_534", Types.Plant, "Декоративное комнатное растение №4", "prop_pot_plant_04b", 100);
            new Furniture("furn_535", Types.Plant, "Декоративные комнатные пальмы в горшке", "prop_pot_plant_04c", 100);
            new Furniture("furn_536", Types.Plant, "Растение Аспидистра", "prop_pot_plant_05a", 100);
            new Furniture("furn_537", Types.Plant, "Декоративное комнатное растение №5", "prop_pot_plant_05b", 100);
            new Furniture("furn_538", Types.Plant, "Декоративное комнатное растение №6", "prop_pot_plant_05c", 100);
            new Furniture("furn_539", Types.Plant, "Декоративное комнатное растение №7", "prop_pot_plant_05d", 100);
            new Furniture("furn_540", Types.Plant, "Декоративное комнатное растение №8", "prop_pot_plant_05d_l1", 100);
            new Furniture("furn_541", Types.Plant, "Декоративное комнатное растение №9", "prop_pot_plant_6a", 100);
            new Furniture("furn_542", Types.Plant, "Декоративное комнатное растение №10", "prop_pot_plant_6b", 100);
            new Furniture("furn_543", Types.Plant, "Растение Аспарагус №2", "prop_pot_plant_bh1", 100);
            new Furniture("furn_544", Types.Plant, "Декоративное комнатное растение №11", "prop_pot_plant_inter_03a", 100);
            new Furniture("furn_634", Types.Plant, "Декоративное комнатное растение №12", "prop_windowbox_a", 100);
            new Furniture("furn_635", Types.Plant, "Декоративное комнатное растение №13", "prop_windowbox_b", 100);
            new Furniture("furn_643", Types.Plant, "Растение Хамедорея №2", "p_int_jewel_plant_01", 100);
            new Furniture("furn_644", Types.Plant, "Растение Спатифиллум", "p_int_jewel_plant_02", 100);

            new Furniture("furn_65", Types.Lamp, "Светильник напольный Рисовая бумага", "apa_mp_h_floorlamp_a", 100);
            new Furniture("furn_66", Types.Lamp, "Светильник напольный Зигзаг", "apa_mp_h_floorlamp_b", 100);
            new Furniture("furn_67", Types.Lamp, "Светильник напольный Черный", "apa_mp_h_floorlamp_c", 100);
            new Furniture("furn_68", Types.Lamp, "Светильник напольный Гриб", "apa_mp_h_floor_lamp_int_08", 100);
            new Furniture("furn_69", Types.Lamp, "Люстра из ламп Эдисона", "apa_mp_h_lampbulb_multiple_a", 100);
            new Furniture("furn_70", Types.Lamp, "Светильник напольный Синий", "apa_mp_h_lit_floorlampnight_07", 100);
            new Furniture("furn_71", Types.Lamp, "Светильник напольный Торшер", "apa_mp_h_lit_floorlampnight_14", 100);
            new Furniture("furn_72", Types.Lamp, "Светильник напольный Pixar", "apa_mp_h_lit_floorlamp_01", 100);
            new Furniture("furn_73", Types.Lamp, "Светильник напольный Строгий", "apa_mp_h_lit_floorlamp_03", 100);
            new Furniture("furn_74", Types.Lamp, "Светильник напольный Черный (L)", "apa_mp_h_lit_floorlamp_05", 100);
            new Furniture("furn_75", Types.Lamp, "Светильник напольный Простой", "apa_mp_h_lit_floorlamp_06", 100);
            new Furniture("furn_76", Types.Lamp, "Светильник напольный Побеги", "apa_mp_h_lit_floorlamp_10", 100);
            new Furniture("furn_77", Types.Lamp, "Светильник напольный Неон", "apa_mp_h_lit_floorlamp_13", 100);
            new Furniture("furn_78", Types.Lamp, "Светильник напольный Тренога", "apa_mp_h_lit_floorlamp_17", 100);
            new Furniture("furn_79", Types.Lamp, "Лампа настольная Тренога", "apa_mp_h_lit_lamptablenight_16", 100);
            new Furniture("furn_80", Types.Lamp, "Лампа настольная Черная", "apa_mp_h_lit_lamptablenight_24", 100);
            new Furniture("furn_81", Types.Lamp, "Лампа настольная Классика", "apa_mp_h_lit_lamptable_005", 100);
            new Furniture("furn_82", Types.Lamp, "Лампа настольная Pixar", "apa_mp_h_lit_lamptable_02", 100);
            new Furniture("furn_83", Types.Lamp, "Лампа настольная Пожилой депутат", "apa_mp_h_lit_lamptable_04", 100);
            new Furniture("furn_84", Types.Lamp, "Лампа настольная Рисовая бумага", "apa_mp_h_lit_lamptable_09", 100);
            new Furniture("furn_85", Types.Lamp, "Лампа настольная Али", "apa_mp_h_lit_lamptable_14", 100);
            new Furniture("furn_86", Types.Lamp, "Лампа настольная Азия", "apa_mp_h_lit_lamptable_17", 100);
            new Furniture("furn_87", Types.Lamp, "Лампа настольная Гриб", "apa_mp_h_lit_lamptable_21", 100);
            new Furniture("furn_88", Types.Lamp, "Люстра Модерн", "apa_mp_h_lit_lightpendant_01", 100);
            new Furniture("furn_89", Types.Lamp, "Люстра Тройка черная", "apa_mp_h_lit_lightpendant_05", 100);
            new Furniture("furn_90", Types.Lamp, "Люстра Тройка дерево", "apa_mp_h_lit_lightpendant_05b", 100);
            new Furniture("furn_158", Types.Lamp, "Лампа настольная Кожа", "apa_mp_h_yacht_table_lamp_01", 100);
            new Furniture("furn_159", Types.Lamp, "Лампа настольная Бетон", "apa_mp_h_yacht_table_lamp_03", 100);
            new Furniture("furn_320", Types.Lamp, "Светильник напольный Кольцо", "hei_heist_lit_floorlamp_02", 100);
            new Furniture("furn_321", Types.Lamp, "Светильник напольный Наклон", "hei_heist_lit_floorlamp_04", 100);
            new Furniture("furn_322", Types.Lamp, "Лампа настольная Наклон", "hei_heist_lit_lamptable_03", 100);
            new Furniture("furn_323", Types.Lamp, "Лампа настольная Молодой депутат", "hei_heist_lit_lamptable_04", 100);
            new Furniture("furn_324", Types.Lamp, "Лампа настольная Сфера", "hei_heist_lit_lamptable_06", 100);
            new Furniture("furn_325", Types.Lamp, "Люстра Купол", "hei_heist_lit_lightpendant_003", 100);
            new Furniture("furn_326", Types.Lamp, "Люстра Оранжевая", "hei_heist_lit_lightpendant_02", 100);
            new Furniture("furn_344", Types.Lamp, "Набор прожекторов подвесной (L)", "hei_prop_carrier_lightset_1", 100);
            new Furniture("furn_345", Types.Lamp, "Прожектор одиночный", "hei_prop_carrier_light_01", 100);
            new Furniture("furn_348", Types.Lamp, "Лампа настольная Библиотека", "hei_prop_hei_bnk_lamp_01", 100);
            new Furniture("furn_349", Types.Lamp, "Лампа настольная Готика", "hei_prop_hei_bnk_lamp_02", 100);
            new Furniture("furn_572", Types.Lamp, "Софит одиночный", "prop_spot_01", 100);
            new Furniture("furn_573", Types.Lamp, "Софит подвесной", "prop_spot_clamp_02", 100);
            new Furniture("furn_574", Types.Lamp, "Набор прожекторов на треноге (L)", "prop_studio_light_01", 100);
            new Furniture("furn_575", Types.Lamp, "Софит на треноге", "prop_studio_light_02", 100);
            new Furniture("furn_576", Types.Lamp, "Фотозонт", "prop_studio_light_03", 100);
            new Furniture("furn_624", Types.Lamp, "Фонарь Готика", "prop_wall_light_07a", 100);

            new Furniture("furn_121", Types.TV, "Стенка с ТВ справа, дерево (XXL)", "apa_mp_h_str_avunitl_01_b", 100);
            new Furniture("furn_122", Types.TV, "Стенка с ТВ, серая (XXL)", "apa_mp_h_str_avunitl_04", 100);
            new Furniture("furn_123", Types.TV, "Домашний кинотеатр (желтые колонки) (XL)", "apa_mp_h_str_avunitm_01", 100);
            new Furniture("furn_124", Types.TV, "Домашний кинотеатр (белые колонки) (XL)", "apa_mp_h_str_avunitm_03", 100);
            new Furniture("furn_125", Types.TV, "Домашний кинотеатр (без колонок) (XL)", "apa_mp_h_str_avunits_01", 100);
            new Furniture("furn_126", Types.TV, "Домашний кинотеатр (стойка) (XL)", "apa_mp_h_str_avunits_04", 100);
            new Furniture("furn_267", Types.TV, "Плазменный телевизор", "des_tvsmash_start", 100);
            new Furniture("furn_337", Types.TV, "Стенка с ТВ слева, дерево (XXL)", "hei_heist_str_avunitl_01", 100);
            new Furniture("furn_338", Types.TV, "Домашний кинотеатр MAX, серый (XXL)", "hei_heist_str_avunitl_03", 100);
            new Furniture("furn_339", Types.TV, "Домашний кинотеатр с LCD (XL)", "hei_heist_str_avunits_01", 100);
            new Furniture("furn_405", Types.TV, "Телевизор Триколор", "prop_cctv_mon_02", 100);
            new Furniture("furn_440", Types.TV, "ТВ на стойке", "prop_cs_tv_stand", 100);
            new Furniture("furn_597", Types.TV, "Телевизор Горизонт", "prop_tv_01", 100);
            new Furniture("furn_598", Types.TV, "Телевизор Компакт", "prop_tv_02", 100);
            new Furniture("furn_599", Types.TV, "Телевизор Пасамоник серебро", "prop_tv_03", 100);
            new Furniture("furn_600", Types.TV, "Телевизор Ретро", "prop_tv_04", 100);
            new Furniture("furn_601", Types.TV, "Телевизор с антенной", "prop_tv_05", 100);
            new Furniture("furn_602", Types.TV, "Телевизор Пасамоник черный", "prop_tv_06", 100);
            new Furniture("furn_603", Types.TV, "Телевизор 2001", "prop_tv_07", 100);
            new Furniture("furn_607", Types.TV, "ТВ на стену LCD (L)", "prop_tv_flat_01", 100);
            new Furniture("furn_608", Types.TV, "ТВ на подставке серый (L)", "prop_tv_flat_02", 100);
            new Furniture("furn_609", Types.TV, "ТВ на подставке черный (L)", "prop_tv_flat_02b", 100);
            new Furniture("furn_610", Types.TV, "ТВ на подставке серый (M)", "prop_tv_flat_03", 100);
            new Furniture("furn_611", Types.TV, "ТВ на стену серый (M)", "prop_tv_flat_03b", 100);
            new Furniture("furn_612", Types.TV, "ТВ на стену Plasma (L)", "prop_tv_flat_michael", 100);
            new Furniture("furn_660", Types.TV, "ТВ на подставке LCD (XL)", "sm_prop_smug_tv_flat_01", 100);

            new Furniture("furn_28", Types.Electronics, "Телефон ретро", "apa_mp_h_acc_phone_01", 100);
            new Furniture("furn_210", Types.Electronics, "Бойлер с бытовой химией", "ap_m_prop_11", 100);
            new Furniture("furn_231", Types.Electronics, "Джойстик игровой (темный)", "ba_prop_battle_control_console", 100);
            new Furniture("furn_248", Types.Electronics, "Музыкальный автомат настенный", "bkr_prop_clubhouse_jukebox_01a", 100);
            new Furniture("furn_249", Types.Electronics, "Музыкальный автомат 80х", "bkr_prop_clubhouse_jukebox_01b", 100);
            new Furniture("furn_250", Types.Electronics, "Музыкальный автомат 60х", "bkr_prop_clubhouse_jukebox_02a", 100);
            new Furniture("furn_251", Types.Electronics, "Ноутбук iFruit серый", "bkr_prop_clubhouse_laptop_01a", 100);
            new Furniture("furn_281", Types.Electronics, "Моноблок iFruit", "ex_prop_monitor_01_ex", 100);
            new Furniture("furn_285", Types.Electronics, "Моноблок iFruit для работы", "ex_prop_trailer_monitor_01", 100);
            new Furniture("furn_298", Types.Electronics, "Джойстик игровой (светлый)", "gr_prop_gr_console_01", 100);
            new Furniture("furn_346", Types.Electronics, "ПК iFruit современный", "hei_prop_heist_pc_01", 100);
            new Furniture("furn_347", Types.Electronics, "Телефон", "hei_prop_hei_bank_phone_01", 100);
            new Furniture("furn_425", Types.Electronics, "Игровая консоль", "prop_console_01", 100);
            new Furniture("furn_445", Types.Electronics, "ПК iFruit классический", "prop_dyn_pc_02", 100);
            new Furniture("furn_449", Types.Electronics, "Кассетный магнитофон", "prop_el_tapeplayer_01", 100);
            new Furniture("furn_452", Types.Electronics, "Напольный вентилятор", "prop_fan_01", 100);
            new Furniture("furn_453", Types.Electronics, "Факс", "prop_fax_01", 100);
            new Furniture("furn_468", Types.Electronics, "Камера", "prop_ing_camera_01", 100);
            new Furniture("furn_471", Types.Electronics, "Клавиатура белая", "prop_keyboard_01a", 100);
            new Furniture("furn_472", Types.Electronics, "Клавиатура черная", "prop_keyboard_01b", 100);
            new Furniture("furn_479", Types.Electronics, "Ноутбук iFruit аниме", "prop_laptop_lester", 100);
            new Furniture("furn_483", Types.Electronics, "Монитор Пасамоник", "prop_ld_monitor_01", 100);
            new Furniture("furn_488", Types.Electronics, "Монитор современный космос", "prop_monitor_01a", 100);
            new Furniture("furn_489", Types.Electronics, "Монитор современный", "prop_monitor_01b", 100);
            new Furniture("furn_490", Types.Electronics, "Монитор iFruit эльф", "prop_monitor_01c", 100);
            new Furniture("furn_491", Types.Electronics, "Монитор современный 18+", "prop_monitor_01d", 100);
            new Furniture("furn_492", Types.Electronics, "Монитор iFruit", "prop_monitor_02", 100);
            new Furniture("furn_493", Types.Electronics, "Монитор большой", "prop_monitor_w_large", 100);
            new Furniture("furn_546", Types.Electronics, "Принтер", "prop_printer_02", 100);
            new Furniture("furn_549", Types.Electronics, "Радиоприемник", "prop_radio_01", 100);
            new Furniture("furn_564", Types.Electronics, "Колонка напольная (L)", "prop_speaker_01", 100);
            new Furniture("furn_565", Types.Electronics, "Колонка дерево (M)", "prop_speaker_02", 100);
            new Furniture("furn_566", Types.Electronics, "Колонка-монитор классика", "prop_speaker_03", 100);
            new Furniture("furn_567", Types.Electronics, "Колонка-монитор современная", "prop_speaker_05", 100);
            new Furniture("furn_568", Types.Electronics, "Колонка с сеткой черная", "prop_speaker_06", 100);
            new Furniture("furn_569", Types.Electronics, "Колонка студийная", "prop_speaker_07", 100);
            new Furniture("furn_570", Types.Electronics, "Колонка трехполосная", "prop_speaker_08", 100);
            new Furniture("furn_640", Types.Electronics, "Ноутбук Пасамоник", "p_cs_laptop_02", 100);
            new Furniture("furn_645", Types.Electronics, "Ноутбук iFruit белый", "p_laptop_02_s", 100);
            new Furniture("furn_675", Types.Electronics, "Колонка центральная", "v_res_mm_audio", 100);

            new Furniture("furn_22", Types.KitchenStuff, "Чайный набор", "apa_mp_h_acc_drink_tray_02", 100);
            new Furniture("furn_23", Types.KitchenStuff, "Фруктовая тарелка (яблоки)", "apa_mp_h_acc_fruitbowl_01", 100);
            new Furniture("furn_24", Types.KitchenStuff, "Фруктовая тарелка (ассорти)", "apa_mp_h_acc_fruitbowl_02", 100);
            new Furniture("furn_188", Types.KitchenStuff, "Кухонный гарнитур полный Белый прямой", "ap_m_cooking_1", 100);
            new Furniture("furn_189", Types.KitchenStuff, "Тумбочка с рабочей поверхностью серая 2 отсека", "ap_m_cooking_10", 100);
            new Furniture("furn_190", Types.KitchenStuff, "Раковина угловая мрамор", "ap_m_cooking_11", 100);
            new Furniture("furn_191", Types.KitchenStuff, "Кухонный гарнитур полный Черный прямой", "ap_m_cooking_12", 100);
            new Furniture("furn_192", Types.KitchenStuff, "Остров кухонный профессиональный темный", "ap_m_cooking_13", 100);
            new Furniture("furn_193", Types.KitchenStuff, "Кухонный гарнитур полный угловой белый с черным", "ap_m_cooking_14", 100);
            new Furniture("furn_194", Types.KitchenStuff, "Тумбочка с рабочей поверхностью темная 1 отсек", "ap_m_cooking_15", 100);
            new Furniture("furn_195", Types.KitchenStuff, "Подвесной шкаф темный 4 отсека", "ap_m_cooking_16", 100);
            new Furniture("furn_197", Types.KitchenStuff, "Раковина угловая с рабочей поверхностью темная", "ap_m_cooking_18", 100);
            new Furniture("furn_198", Types.KitchenStuff, "Кухонный гарнитур полный Черный прямой с деревом", "ap_m_cooking_19", 100);
            new Furniture("furn_200", Types.KitchenStuff, "Остров кухонный профессиональный светлый", "ap_m_cooking_20", 100);
            new Furniture("furn_201", Types.KitchenStuff, "Раковина угловая белая", "ap_m_cooking_3", 100);

            new Furniture("furn_202", Types.Fridge, "Холодильник Ретро", "ap_m_cooking_4", 100);

            new Furniture("furn_203", Types.KitchenStuff, "Раковина угловая с рабочей поверхностью белая", "ap_m_cooking_5", 100);
            new Furniture("furn_204", Types.KitchenStuff, "Тумбочка с рабочей поверхностью 2 отсека", "ap_m_cooking_6", 100);
            new Furniture("furn_205", Types.KitchenStuff, "Газовая плита Стандарт", "ap_m_cooking_7", 100);
            new Furniture("furn_206", Types.KitchenStuff, "Подвесной шкаф белый 5 отсеков", "ap_m_cooking_8", 100);
            new Furniture("furn_207", Types.KitchenStuff, "Подвесной шкаф мрамор 5 отсеков", "ap_m_cooking_9", 100);
            new Furniture("furn_208", Types.KitchenStuff, "Кухонные принадлежности настенные металл", "ap_m_prop_1", 100);
            new Furniture("furn_232", Types.KitchenStuff, "Коллекция местного пива", "beerrow_local", 100);
            new Furniture("furn_233", Types.KitchenStuff, "Коллекция мирового пива", "beerrow_world", 100);
            new Furniture("furn_254", Types.KitchenStuff, "Дегидратор", "bkr_prop_coke_dehydrator_01", 100);
            new Furniture("furn_268", Types.KitchenStuff, "Кофемашина эспрессо медь", "ex_mp_h_acc_coffeemachine_01", 100);
            new Furniture("furn_319", Types.KitchenStuff, "Кофемашина эспрессо сталь", "hei_heist_kit_coffeemachine_01", 100);
            new Furniture("furn_378", Types.KitchenStuff, "Барная салфетница", "prop_bar_caddy", 100);

            new Furniture("furn_379", Types.Fridge, "Холодильник для вина", "prop_bar_fridge_01", 100);
            new Furniture("furn_380", Types.Fridge, "Холодильник для пива (двойной)", "prop_bar_fridge_02", 100);
            new Furniture("furn_381", Types.Fridge, "Холодильник для пива", "prop_bar_fridge_03", 100);
            new Furniture("furn_382", Types.Fridge, "Холодильник для газировки (двойной)", "prop_bar_fridge_04", 100);

            new Furniture("furn_383", Types.KitchenStuff, "Фруктовая тарелка (нарезка)", "prop_bar_fruit", 100);
            new Furniture("furn_384", Types.KitchenStuff, "Ледогенератор", "prop_bar_ice_01", 100);
            new Furniture("furn_385", Types.KitchenStuff, "Фруктовая тарелка (лимон)", "prop_bar_lemons", 100);
            new Furniture("furn_386", Types.KitchenStuff, "Фруктовая тарелка (лайм)", "prop_bar_limes", 100);
            new Furniture("furn_387", Types.KitchenStuff, "Мерный стакан", "prop_bar_measrjug", 100);
            new Furniture("furn_388", Types.KitchenStuff, "Салфетница", "prop_bar_napkindisp", 100);
            new Furniture("furn_389", Types.KitchenStuff, "Барная стойка с кранами", "prop_bar_pump_01", 100);
            new Furniture("furn_390", Types.KitchenStuff, "Мойка профессиональная", "prop_bar_sink_01", 100);
            new Furniture("furn_401", Types.KitchenStuff, "Мусорное ведро большое", "prop_bin_10b", 100);
            new Furniture("furn_402", Types.KitchenStuff, "Мусорное ведро с крышкой", "prop_bin_11a", 100);
            new Furniture("furn_403", Types.KitchenStuff, "Мусорное ведро малое", "prop_bin_11b", 100);
            new Furniture("furn_423", Types.KitchenStuff, "Кофемашина профессиональная", "prop_coffee_mac_01", 100);
            new Furniture("furn_424", Types.KitchenStuff, "Кофемашина капельная", "prop_coffee_mac_02", 100);
            new Furniture("furn_426", Types.KitchenStuff, "Газовая плита профессиональная", "prop_cooker_03", 100);
            new Furniture("furn_427", Types.KitchenStuff, "Кастрюля медная", "prop_copper_pan", 100);
            new Furniture("furn_454", Types.KitchenStuff, "Мойка простая", "prop_ff_sink_02", 100);
            new Furniture("furn_469", Types.KitchenStuff, "Кебаб гриль", "prop_kebab_grill", 100);
            new Furniture("furn_470", Types.KitchenStuff, "Чайник электрический", "prop_kettle", 100);
            new Furniture("furn_473", Types.KitchenStuff, "Сковорода", "prop_kitch_pot_fry", 100);
            new Furniture("furn_474", Types.KitchenStuff, "Кастрюля (XL)", "prop_kitch_pot_huge", 100);
            new Furniture("furn_475", Types.KitchenStuff, "Ковш (L)", "prop_kitch_pot_lrg", 100);
            new Furniture("furn_476", Types.KitchenStuff, "Ковш (S)", "prop_kitch_pot_sm", 100);
            new Furniture("furn_477", Types.KitchenStuff, "Набор ножей в стойке, сталь", "prop_knife_stand", 100);
            new Furniture("furn_485", Types.KitchenStuff, "Микроволновка сталь", "prop_microwave_1", 100);
            new Furniture("furn_486", Types.KitchenStuff, "Микроволновка белая", "prop_micro_02", 100);
            new Furniture("furn_487", Types.KitchenStuff, "Микроволновка дерево", "prop_micro_04", 100);
            new Furniture("furn_545", Types.KitchenStuff, "Набор посуды подвесной (XL)", "prop_pot_rack", 100);
            new Furniture("furn_618", Types.KitchenStuff, "Набор для готовки (на стену)", "prop_utensil", 100);
            new Furniture("furn_628", Types.KitchenStuff, "Кулер", "prop_watercooler", 100);
            new Furniture("furn_629", Types.KitchenStuff, "Бутыль для кулера", "prop_water_bottle", 100);
            new Furniture("furn_673", Types.KitchenStuff, "Набор ножей в стойке, дерево", "v_res_mknifeblock", 100);
            new Furniture("furn_674", Types.KitchenStuff, "Набор ножей (на стену)", "v_res_mkniferack", 100);
            new Furniture("furn_684", Types.KitchenStuff, "Кастрюля чугунная", "v_res_tt_pot01", 100);
            new Furniture("furn_685", Types.KitchenStuff, "Сковорода чугунная", "v_res_tt_pot02", 100);
            new Furniture("furn_686", Types.KitchenStuff, "Ковш чугунный", "v_res_tt_pot03", 100);
            new Furniture("furn_689", Types.KitchenStuff, "Коллекция вина", "winerow", 100);

            new Furniture("furn_50", Types.Bath, "Ванная белая", "apa_mp_h_bathtub_01", 100);
            new Furniture("furn_172", Types.Bath, "Ванная с занавеской", "ap_m_bath_1", 100);
            new Furniture("furn_173", Types.Bath, "Душевая кабина Luxe (L)", "ap_m_bath_10", 100);
            new Furniture("furn_174", Types.Bath, "Ванная Бизнес", "ap_m_bath_11", 100);
            new Furniture("furn_175", Types.BathStuff, "Раковина двойная на инсталляции (L)", "ap_m_bath_12", 100);
            new Furniture("furn_176", Types.Bath, "Ванная Luxe с душем закрытая", "ap_m_bath_13", 100);
            new Furniture("furn_177", Types.Bath, "Ванная Luxe с душем дерево", "ap_m_bath_14", 100);
            new Furniture("furn_178", Types.Bath, "Ванная Luxe с душем гранит", "ap_m_bath_15", 100);
            new Furniture("furn_179", Types.BathStuff, "Раковина двойная на исталляции Luxe (XL)", "ap_m_bath_16", 100);
            new Furniture("furn_180", Types.BathStuff, "Раковина белая с двумя кранами", "ap_m_bath_2", 100);
            new Furniture("furn_183", Types.BathStuff, "Раковина камень на деревянной тумбе", "ap_m_bath_5", 100);
            new Furniture("furn_185", Types.BathStuff, "Раковина на инсталляции (M)", "ap_m_bath_7", 100);
            new Furniture("furn_186", Types.Bath, "Душевая кабина Luxe (M)", "ap_m_bath_8", 100);
            new Furniture("furn_187", Types.Bath, "Душевая кабина Стандарт", "ap_m_bath_9", 100);
            new Furniture("furn_216", Types.BathStuff, "Штанга с полотенцами", "ap_m_prop_4", 100);
            new Furniture("furn_218", Types.BathStuff, "Сушилка для полотенец", "ap_m_prop_6", 100);
            new Furniture("furn_219", Types.BathStuff, "Штанга с полотенцами двойная", "ap_m_prop_7", 100);
            new Furniture("furn_466", Types.Bath, "Джакузи", "prop_hottub2", 100);
            new Furniture("furn_559", Types.BathStuff, "Раковина под дерево", "prop_sink_02", 100);
            new Furniture("furn_560", Types.BathStuff, "Раковина со столиком", "prop_sink_04", 100);
            new Furniture("furn_561", Types.BathStuff, "Раковина белая настенная", "prop_sink_05", 100);
            new Furniture("furn_562", Types.BathStuff, "Раковина белая напольная", "prop_sink_06", 100);

            new Furniture("furn_181", Types.Toilet, "Унитаз белый с деревянным сидением", "ap_m_bath_3", 100);
            new Furniture("furn_182", Types.Toilet, "Унитаз с отдельным бачком", "ap_m_bath_4", 100);
            new Furniture("furn_184", Types.Toilet, "Унитаз современный без бачка", "ap_m_bath_6", 100);
            new Furniture("furn_484", Types.Toilet, "Унитаз б/у", "prop_ld_toilet_01", 100);
            new Furniture("furn_591", Types.Toilet, "Унитаз с бачком", "prop_toilet_01", 100);
            new Furniture("furn_592", Types.Toilet, "Унитаз без бачка", "prop_toilet_02", 100);
            new Furniture("furn_593", Types.Toilet, "Ершик", "prop_toilet_brush_01", 100);
            new Furniture("furn_594", Types.Toilet, "Рулон бумаги", "prop_toilet_roll_01", 100);
            new Furniture("furn_595", Types.Toilet, "Держатель для бумаги", "prop_toilet_roll_02", 100);

            new Furniture("furn_212", Types.Washer, "Стиральная машина с вертикальной загрузкой", "ap_m_prop_13", 100);
            new Furniture("furn_625", Types.Washer, "Стиральная машина б/у", "prop_washer_01", 100);
            new Furniture("furn_626", Types.Washer, "Стиральная машина современная", "prop_washer_02", 100);
            new Furniture("furn_627", Types.Washer, "Стиральная машина старая", "prop_washer_03", 100);

            new Furniture("furn_0", Types.Painting, "Картина Закат абстракционизм (L)", "apa_mp_h_acc_artwalll_01", 100);
            new Furniture("furn_1", Types.Painting, "Картина Утро абстракционизм (L)", "apa_mp_h_acc_artwalll_02", 100);
            new Furniture("furn_2", Types.Painting, "Картина Глубины абстракионизмци (L)", "apa_mp_h_acc_artwalll_03", 100);
            new Furniture("furn_3", Types.Painting, "Картина Небо абстракционизм (M)", "apa_mp_h_acc_artwallm_02", 100);
            new Furniture("furn_4", Types.Painting, "Картина Безликие абстракционизм (M)", "apa_mp_h_acc_artwallm_03", 100);
            new Furniture("furn_5", Types.Painting, "Картина Гнев абстракционизм (M)", "apa_mp_h_acc_artwallm_04", 100);
            new Furniture("furn_161", Types.Painting, "Картина Солнце абстракционизм (L)", "apa_p_h_acc_artwalll_01", 100);
            new Furniture("furn_162", Types.Painting, "Картина S абстракионизм (L)", "apa_p_h_acc_artwalll_02", 100);
            new Furniture("furn_163", Types.Painting, "Картина Клаустрофобия абстракионизм (L)", "apa_p_h_acc_artwalll_03", 100);
            new Furniture("furn_164", Types.Painting, "Картина Ночь абстракионизм (L)", "apa_p_h_acc_artwalll_04", 100);
            new Furniture("furn_165", Types.Painting, "Картина объемная кубы серая (M)", "apa_p_h_acc_artwallm_01", 100);
            new Furniture("furn_166", Types.Painting, "Картина Тлен абстракционизм (M)", "apa_p_h_acc_artwallm_03", 100);
            new Furniture("furn_167", Types.Painting, "Картина Пересечение абстракционизм (M)", "apa_p_h_acc_artwallm_04", 100);
            new Furniture("furn_168", Types.Painting, "Форма чемпиона Larsen#6 в рамке", "apa_p_h_acc_artwalls_03", 100);
            new Furniture("furn_169", Types.Painting, "Форма чемпиона Palmer#3 в рамке", "apa_p_h_acc_artwalls_04", 100);
            new Furniture("furn_170", Types.Painting, "Картина Ретро в деревянной рамке", "ap_m_art_1", 100);
            new Furniture("furn_260", Types.Painting, "Картина в багетной рамке Загадочная Европа", "cls_sf_art_2a", 100);
            new Furniture("furn_261", Types.Painting, "Картина в багетной рамке Вечерная прогулка", "cls_sf_art_3a", 100);
            new Furniture("furn_262", Types.Painting, "Картина в багетной рамке Вольфганг", "cls_sf_art_9c", 100);
            new Furniture("furn_277", Types.Painting, "Пара портретов в багетных рамах", "ex_office_swag_paintings01", 100);
            new Furniture("furn_278", Types.Painting, "Три пейзажа в багетных рамах", "ex_office_swag_paintings02", 100);
            new Furniture("furn_279", Types.Painting, "Два пейзажа в багетных рамах", "ex_office_swag_paintings03", 100);
            new Furniture("furn_286", Types.Painting, "Картина Клаустрофобия абстракионизм (L)", "ex_p_h_acc_artwalll_03", 100);
            new Furniture("furn_306", Types.Painting, "Золотая пластинка Corner Killahz", "hei_heist_acc_artgolddisc_01", 100);
            new Furniture("furn_307", Types.Painting, "Золотая пластинка Kill D Sac", "hei_heist_acc_artgolddisc_02", 100);
            new Furniture("furn_308", Types.Painting, "Золотая пластинка DG Loc", "hei_heist_acc_artgolddisc_03", 100);
            new Furniture("furn_309", Types.Painting, "Золотая пластинка Madd Dogg", "hei_heist_acc_artgolddisc_04", 100);
            new Furniture("furn_310", Types.Painting, "Картина Закат темный абстракционизм (L)", "hei_heist_acc_artwalll_01", 100);
            new Furniture("furn_311", Types.Painting, "Картина объемная кубы белая (M)", "hei_heist_acc_artwallm_01", 100);
            new Furniture("furn_351", Types.Painting, "Фотография автопортрет мужчины", "hei_prop_hei_pic_hl_gurkhas", 100);
            new Furniture("furn_352", Types.Painting, "Фотография неизвестой девушки", "hei_prop_hei_pic_hl_keycodes", 100);
            new Furniture("furn_353", Types.Painting, "Постер Valkyrie", "hei_prop_hei_pic_hl_valkyrie", 100);
            new Furniture("furn_354", Types.Painting, "Фотография задержанного", "hei_prop_hei_pic_pb_break", 100);
            new Furniture("furn_355", Types.Painting, "Фотография синего автобуса", "hei_prop_hei_pic_pb_bus", 100);
            new Furniture("furn_356", Types.Painting, "Постер авиакомпании", "hei_prop_hei_pic_pb_plane", 100);
            new Furniture("furn_357", Types.Painting, "Фотография Полицейского Департамента", "hei_prop_hei_pic_pb_station", 100);
            new Furniture("furn_358", Types.Painting, "Фотография в гаражах", "hei_prop_hei_pic_ps_bike", 100);
            new Furniture("furn_359", Types.Painting, "Фотография Банк", "hei_prop_hei_pic_ps_job", 100);
            new Furniture("furn_360", Types.Painting, "Фотография мусоровозы", "hei_prop_hei_pic_ps_trucks", 100);
            new Furniture("furn_361", Types.Painting, "Фотография автовыставки", "hei_prop_hei_pic_ub_prep", 100);
            new Furniture("furn_362", Types.Painting, "Фотография банка на трассе", "hei_prop_hei_pic_ub_prep02", 100);
            new Furniture("furn_363", Types.Painting, "Фотография банка на трассе зачеркнутая", "hei_prop_hei_pic_ub_prep02b", 100);
            new Furniture("furn_500", Types.Painting, "Фотография хитрого мужчины", "prop_ped_pic_01", 100);
            new Furniture("furn_501", Types.Painting, "Фотография серьезного мужчины", "prop_ped_pic_02", 100);
            new Furniture("furn_502", Types.Painting, "Фотография доброго мужчины", "prop_ped_pic_04", 100);
            new Furniture("furn_503", Types.Painting, "Фотография баскетболиста", "prop_ped_pic_05", 100);
            new Furniture("furn_504", Types.Painting, "Фотография седого мужчины", "prop_ped_pic_06", 100);
            new Furniture("furn_505", Types.Painting, "Фотография молодой девушки", "prop_ped_pic_07", 100);
            new Furniture("furn_506", Types.Painting, "Фотография девушки в очках", "prop_ped_pic_08", 100);
            new Furniture("furn_667", Types.Painting, "Фотография женщины в очках в рамке (M)", "v_ilev_trev_pictureframe", 100);

            new Furniture("furn_6", Types.Decor, "Бутылка белая", "apa_mp_h_acc_bottle_01", 100);
            new Furniture("furn_7", Types.Decor, "Бутылка черная", "apa_mp_h_acc_bottle_02", 100);
            new Furniture("furn_8", Types.Decor, "Чаша черная керамическая", "apa_mp_h_acc_bowl_ceramic_01", 100);
            new Furniture("furn_9", Types.Decor, "Шкатулка белая", "apa_mp_h_acc_box_trinket_01", 100);
            new Furniture("furn_10", Types.Decor, "Шкатулка из черного дерева", "apa_mp_h_acc_box_trinket_02", 100);
            new Furniture("furn_11", Types.Decor, "Набор свечей Геометрия", "apa_mp_h_acc_candles_01", 100);
            new Furniture("furn_12", Types.Decor, "Набор розовых свечей", "apa_mp_h_acc_candles_02", 100);
            new Furniture("furn_13", Types.Decor, "Свеча розовая", "apa_mp_h_acc_candles_04", 100);
            new Furniture("furn_14", Types.Decor, "Свеча в черном стекле", "apa_mp_h_acc_candles_05", 100);
            new Furniture("furn_15", Types.Decor, "Набор черных свечей", "apa_mp_h_acc_candles_06", 100);
            new Furniture("furn_16", Types.Decor, "Скульптура Маска", "apa_mp_h_acc_dec_head_01", 100);
            new Furniture("furn_17", Types.Decor, "Тарелка декоративная Майя", "apa_mp_h_acc_dec_plate_01", 100);
            new Furniture("furn_18", Types.Decor, "Тарелка декоративная Каменный век", "apa_mp_h_acc_dec_plate_02", 100);
            new Furniture("furn_19", Types.Decor, "Скульптура Открытие", "apa_mp_h_acc_dec_sculpt_01", 100);
            new Furniture("furn_20", Types.Decor, "Скульптура Прорезь", "apa_mp_h_acc_dec_sculpt_02", 100);
            new Furniture("furn_21", Types.Decor, "Скульптура Видение", "apa_mp_h_acc_dec_sculpt_03", 100);
            new Furniture("furn_25", Types.Decor, "Контейнер двойной", "apa_mp_h_acc_jar_02", 100);
            new Furniture("furn_26", Types.Decor, "Контейнер тройной", "apa_mp_h_acc_jar_03", 100);
            new Furniture("furn_27", Types.Decor, "Контейнер голубой", "apa_mp_h_acc_jar_04", 100);
            new Furniture("furn_31", Types.Decor, "Чаша с камнями", "apa_mp_h_acc_pot_pouri_01", 100);
            new Furniture("furn_32", Types.Decor, "Ковер с длинным ворсом (XL)", "apa_mp_h_acc_rugwooll_03", 100);
            new Furniture("furn_33", Types.Decor, "Ковер плетеный светлый (XL)", "apa_mp_h_acc_rugwooll_04", 100);
            new Furniture("furn_34", Types.Decor, "Ковер Азия (L)", "apa_mp_h_acc_rugwoolm_01", 100);
            new Furniture("furn_35", Types.Decor, "Ковер Море (L)", "apa_mp_h_acc_rugwoolm_02", 100);
            new Furniture("furn_36", Types.Decor, "Ковер Черное-Белое (L)", "apa_mp_h_acc_rugwoolm_03", 100);
            new Furniture("furn_37", Types.Decor, "Ковер Геометрия (L)", "apa_mp_h_acc_rugwoolm_04", 100);
            new Furniture("furn_38", Types.Decor, "Ковер Клетки (M)", "apa_mp_h_acc_rugwools_01", 100);
            new Furniture("furn_39", Types.Decor, "Ковер Триплет (M)", "apa_mp_h_acc_rugwools_03", 100);
            new Furniture("furn_40", Types.Decor, "Ароматические свечи", "apa_mp_h_acc_scent_sticks_01", 100);
            new Furniture("furn_41", Types.Decor, "Набор керамических урн", "apa_mp_h_acc_tray_01", 100);
            new Furniture("furn_42", Types.Decor, "Ваза Зебра", "apa_mp_h_acc_vase_01", 100);
            new Furniture("furn_43", Types.Decor, "Ваза красная", "apa_mp_h_acc_vase_02", 100);
            new Furniture("furn_44", Types.Decor, "Ваза из черного стекла", "apa_mp_h_acc_vase_04", 100);
            new Furniture("furn_45", Types.Decor, "Ваза капля черная", "apa_mp_h_acc_vase_05", 100);
            new Furniture("furn_160", Types.Decor, "Беговая дорожка", "apa_p_apdlc_treadmill_s", 100);
            new Furniture("furn_171", Types.Decor, "Зеркало маленькое", "ap_m_art_2", 100);
            new Furniture("furn_213", Types.Decor, "Деревянная решетка", "ap_m_prop_14", 100);
            new Furniture("furn_214", Types.Decor, "Вешалка настенная для прихожей", "ap_m_prop_2", 100);
            new Furniture("furn_215", Types.Decor, "Коврик плетеный бежевый", "ap_m_prop_3", 100);
            new Furniture("furn_217", Types.Decor, "Коврик овальный голубой с длинным ворсом", "ap_m_prop_5", 100);
            new Furniture("furn_220", Types.Decor, "Коврик прямоугольный серый", "ap_m_prop_8", 100);
            new Furniture("furn_221", Types.Decor, "Коврик овальный голубой с коротким ворсом", "ap_m_prop_9", 100);
            new Furniture("furn_243", Types.Decor, "Кейс с винтовкой", "bkr_prop_biker_gcase_s", 100);
            new Furniture("furn_255", Types.Decor, "Игрушка супергероя", "bkr_prop_coke_doll", 100);
            new Furniture("furn_257", Types.Decor, "Ёлка-модерн", "ch_prop_tree_01a", 100);
            new Furniture("furn_258", Types.Decor, "Покерный стол", "cls_casino_poker", 100);
            new Furniture("furn_263", Types.Decor, "Ковер Персидский шик", "cls_sf_rug_1", 100);
            new Furniture("furn_269", Types.Decor, "Скульптура Рог", "ex_office_swag_ivory3", 100);
            new Furniture("furn_270", Types.Decor, "Шкура животного на мебель", "ex_office_swag_ivory4", 100);
            new Furniture("furn_271", Types.Decor, "Коллекция часов", "ex_office_swag_jewelwatch", 100);
            new Furniture("furn_272", Types.Decor, "Большая коллекция часов", "ex_office_swag_jewelwatch2", 100);
            new Furniture("furn_273", Types.Decor, "Огромная коллекция часов", "ex_office_swag_jewelwatch3", 100);
            new Furniture("furn_274", Types.Decor, "Набор медикаментов оптом", "ex_office_swag_med1", 100);
            new Furniture("furn_275", Types.Decor, "Набор медикаментов в кейсе", "ex_office_swag_med2", 100);
            new Furniture("furn_276", Types.Decor, "Набор медикаментов в ящике", "ex_office_swag_med3", 100);
            new Furniture("furn_280", Types.Decor, "Награда SecuroServ", "ex_prop_exec_award_plastic", 100);
            new Furniture("furn_296", Types.Decor, "Мастерская из дерева", "gr_prop_gr_bench_04a", 100);
            new Furniture("furn_297", Types.Decor, "Мастерская из металла", "gr_prop_gr_bench_04b", 100);
            new Furniture("furn_299", Types.Decor, "Токарный старинный", "gr_prop_gr_lathe_01a", 100);
            new Furniture("furn_300", Types.Decor, "Токарный прошлого поколения", "gr_prop_gr_lathe_01b", 100);
            new Furniture("furn_301", Types.Decor, "Токарный современный", "gr_prop_gr_lathe_01c", 100);
            new Furniture("furn_302", Types.Decor, "Сварочный аппарат", "gr_prop_gr_prop_welder_01a", 100);
            new Furniture("furn_303", Types.Decor, "Станок старинный", "gr_prop_gr_speeddrill_01a", 100);
            new Furniture("furn_304", Types.Decor, "Станок прошлого поколения", "gr_prop_gr_speeddrill_01b", 100);
            new Furniture("furn_305", Types.Decor, "Станок современный", "gr_prop_gr_speeddrill_01c", 100);
            new Furniture("furn_314", Types.Decor, "Ковер из натуральной кожи (XL)", "hei_heist_acc_rughidel_01", 100);
            new Furniture("furn_315", Types.Decor, "Ковер Море (XL)", "hei_heist_acc_rugwooll_01", 100);
            new Furniture("furn_316", Types.Decor, "Ковер плетеный темный (XL)", "hei_heist_acc_rugwooll_02", 100);
            new Furniture("furn_350", Types.Decor, "Бюст известного человека", "hei_prop_hei_bust_01", 100);
            new Furniture("furn_365", Types.Decor, "Домкрат", "imp_prop_axel_stand_01a", 100);
            new Furniture("furn_366", Types.Decor, "Огромная бомба", "imp_prop_bomb_ball", 100);
            new Furniture("furn_368", Types.Decor, "Мяч для регби", "lr2_prop_ibi_01", 100);
            new Furniture("furn_370", Types.Decor, "Гитара аккустическая", "prop_acc_guitar_01", 100);
            new Furniture("furn_371", Types.Decor, "Аэрохоккей", "prop_airhockey_01", 100);
            new Furniture("furn_372", Types.Decor, "Загадочное яйцо", "prop_alien_egg_01", 100);
            new Furniture("furn_374", Types.Decor, "Гантеля тяжелая", "prop_barbell_01", 100);
            new Furniture("furn_375", Types.Decor, "Штанга тяжелая", "prop_barbell_02", 100);
            new Furniture("furn_376", Types.Decor, "Штанга малая", "prop_barbell_30kg", 100);
            new Furniture("furn_377", Types.Decor, "Штанга средняя", "prop_barbell_80kg", 100);
            new Furniture("furn_392", Types.Decor, "Коллекция пляжных сумок (M)", "prop_beachbag_combo_01", 100);
            new Furniture("furn_393", Types.Decor, "Коллекция пляжных сумок (L)", "prop_beachbag_combo_02", 100);
            new Furniture("furn_394", Types.Decor, "Надувной круг", "prop_beach_ring_01", 100);
            new Furniture("furn_395", Types.Decor, "Неоновая вывеска COLD BEER", "prop_beerneon", 100);
            new Furniture("furn_396", Types.Decor, "Неоновая вывеска LIQUOR", "prop_beer_neon_01", 100);
            new Furniture("furn_397", Types.Decor, "Неоновая вывеска BEER с пистолетами", "prop_beer_neon_02", 100);
            new Furniture("furn_398", Types.Decor, "Неоновая вывеска BEER", "prop_beer_neon_03", 100);
            new Furniture("furn_399", Types.Decor, "Неоновая вывеска Logger", "prop_beer_neon_04", 100);
            new Furniture("furn_400", Types.Decor, "Часы большие Звезда", "prop_big_clock_01", 100);
            new Furniture("furn_404", Types.Decor, "Бонго", "prop_bongos_01", 100);
            new Furniture("furn_419", Types.Decor, "Шампанское в ведерке", "prop_champset", 100);
            new Furniture("furn_420", Types.Decor, "Набор с шампанским", "prop_champ_cool", 100);
            new Furniture("furn_422", Types.Decor, "Неоновая вывеска Cocktails", "prop_cockneon", 100);
            new Furniture("furn_439", Types.Decor, "Декоративная катана", "prop_cs_katana_01", 100);
            new Furniture("furn_441", Types.Decor, "Дартс", "prop_dart_bd_cab_01", 100);
            new Furniture("furn_442", Types.Decor, "Шкаф с бонгами", "prop_disp_cabinet_01", 100);
            new Furniture("furn_443", Types.Decor, "Покрышка от грузовика на стену", "prop_dock_ropetyre2", 100);
            new Furniture("furn_444", Types.Decor, "Манекен", "prop_dummy_01", 100);
            new Furniture("furn_446", Types.Decor, "Гитара электронная Рок", "prop_el_guitar_01", 100);
            new Furniture("furn_447", Types.Decor, "Гитара электронная Хардкор", "prop_el_guitar_02", 100);
            new Furniture("furn_448", Types.Decor, "Гитара электронная Классика", "prop_el_guitar_03", 100);
            new Furniture("furn_450", Types.Decor, "Велотренажер черный", "prop_exercisebike", 100);
            new Furniture("furn_451", Types.Decor, "Велотренажер профессиональный", "prop_exer_bike_01", 100);
            new Furniture("furn_456", Types.Decor, "Розовый фламинго", "prop_flamingo", 100);
            new Furniture("furn_457", Types.Decor, "Гантеля малая", "prop_freeweight_02", 100);
            new Furniture("furn_458", Types.Decor, "Плетеная корзина", "prop_fruit_basket", 100);
            new Furniture("furn_460", Types.Decor, "Часы настенные с короной", "prop_game_clock_01", 100);
            new Furniture("furn_461", Types.Decor, "Часы настенные KRONOS", "prop_game_clock_02", 100);
            new Furniture("furn_462", Types.Decor, "Гном с удочкой", "prop_gnome1", 100);
            new Furniture("furn_463", Types.Decor, "Гном с фонарем", "prop_gnome2", 100);
            new Furniture("furn_464", Types.Decor, "Гном игривый", "prop_gnome3", 100);
            new Furniture("furn_465", Types.Decor, "Автомат с жвачкой", "prop_gumball_03", 100);
            new Furniture("furn_467", Types.Decor, "Чемодан с идолом", "prop_idol_case", 100);
            new Furniture("furn_482", Types.Decor, "Сейф элитный", "prop_ld_int_safe_01", 100);
            new Furniture("furn_509", Types.Decor, "Коктейль Пина Колада", "prop_pinacolada", 100);
            new Furniture("furn_510", Types.Decor, "Ананас", "prop_pineapple", 100);
            new Furniture("furn_521", Types.Decor, "Бильярдный стол зеленый", "prop_pooltable_02", 100);
            new Furniture("furn_522", Types.Decor, "Бильярдный стол фиолетовый", "prop_pooltable_3b", 100);
            new Furniture("furn_523", Types.Decor, "Кий", "prop_pool_cue", 100);
            new Furniture("furn_524", Types.Decor, "Киевница", "prop_pool_rack_01", 100);
            new Furniture("furn_525", Types.Decor, "Бильярдный треугольник", "prop_pool_tri", 100);
            new Furniture("furn_547", Types.Decor, "Скамья для жима с фиксаторами", "prop_pris_bench_01", 100);
            new Furniture("furn_548", Types.Decor, "Ковбойская шляпа", "prop_proxy_hat_01", 100);
            new Furniture("furn_550", Types.Decor, "Старый BMX", "prop_rub_bike_01", 100);
            new Furniture("furn_557", Types.Decor, "Бонг декоративный", "prop_sh_bong_01", 100);
            new Furniture("furn_558", Types.Decor, "Роза", "prop_single_rose", 100);
            new Furniture("furn_571", Types.Decor, "Часы PENDULUS (L)", "prop_sports_clock_01", 100);
            new Furniture("furn_590", Types.Decor, "Настольный теннис", "prop_table_tennis", 100);
            new Furniture("furn_596", Types.Decor, "Коллекция футболок (XL)", "prop_tshirt_shelf_1", 100);
            new Furniture("furn_613", Types.Decor, "Коллекция футболок (L)", "prop_t_shirt_row_03", 100);
            new Furniture("furn_614", Types.Decor, "Коллекция футболок (M)", "prop_t_shirt_row_04", 100);
            new Furniture("furn_617", Types.Decor, "Телескоп", "prop_t_telescope_01b", 100);
            new Furniture("furn_630", Types.Decor, "Скамья для жима профессиональная", "prop_weight_bench_02", 100);
            new Furniture("furn_631", Types.Decor, "Набор блинов для штанги", "prop_weight_rack_01", 100);
            new Furniture("furn_632", Types.Decor, "Набор гантелей", "prop_weight_rack_02", 100);
            new Furniture("furn_633", Types.Decor, "Штанга для тяги", "prop_weight_squat", 100);
            new Furniture("furn_636", Types.Decor, "Ёлка с подарками", "prop_xmas_tree_int", 100);
            new Furniture("furn_649", Types.Decor, "Старая игрушка бывшей", "p_mr_raspberry_01_s", 100);
            new Furniture("furn_653", Types.Decor, "Сейф", "p_v_43_safe_s", 100);
            new Furniture("furn_656", Types.Decor, "Коврик для йоги PROlaps", "p_yoga_mat_01_s", 100);
            new Furniture("furn_657", Types.Decor, "Коврик для йоги черный", "p_yoga_mat_02_s", 100);
            new Furniture("furn_658", Types.Decor, "Коврик для йоги красный", "p_yoga_mat_03_s", 100);
            new Furniture("furn_661", Types.Decor, "Гимнастический мяч синий", "v_ilev_exball_blue", 100);
            new Furniture("furn_662", Types.Decor, "Гимнастический мяч серый", "v_ilev_exball_grey", 100);

            new Furniture("furn_295", Types.Workbench, "Крафт", "gr_prop_gr_bench_01a", 100);
            new Furniture("furn_478", Types.PC, "Ноутбук", "prop_laptop_01a", 100);
            new Furniture("furn_698", Types.Storage, "Склад", "xm_prop_crates_rifles_01a", 100);
        }
    }
}
