﻿using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;

namespace BCRPServer.Game.Businesses
{
    public class FurnitureShop : Shop
    {
        public static Types DefaultType => Types.FurnitureShop;

        public static MaterialsData InitMaterialsData => new MaterialsData(5, 7, 50)
        {
            Prices = new Dictionary<string, uint>()
            {
                // Chairs
{ "furn_57", 10 },
{ "furn_58", 10 },
{ "furn_59", 10 },
{ "furn_60", 10 },
{ "furn_91", 10 },
{ "furn_92", 10 },
{ "furn_93", 10 },
{ "furn_94", 10 },
{ "furn_95", 10 },
{ "furn_96", 10 },
{ "furn_97", 10 },
{ "furn_98", 10 },
{ "furn_99", 10 },
{ "furn_100", 10 },
{ "furn_101", 10 },
{ "furn_102", 10 },
{ "furn_103", 10 },
{ "furn_104", 10 },
{ "furn_105", 10 },
{ "furn_106", 10 },
{ "furn_107", 10 },
{ "furn_108", 10 },
{ "furn_109", 10 },
{ "furn_110", 10 },
{ "furn_147", 10 },
{ "furn_148", 10 },
{ "furn_149", 10 },
{ "furn_150", 10 },
{ "furn_157", 10 },
{ "furn_229", 10 },
{ "furn_230", 10 },
{ "furn_234", 10 },
{ "furn_235", 10 },
{ "furn_236", 10 },
{ "furn_237", 10 },
{ "furn_239", 10 },
{ "furn_240", 10 },
{ "furn_241", 10 },
{ "furn_245", 10 },
{ "furn_247", 10 },
{ "furn_252", 10 },
{ "furn_256", 10 },
{ "furn_259", 10 },
{ "furn_282", 10 },
{ "furn_283", 10 },
{ "furn_284", 10 },
{ "furn_288", 10 },
{ "furn_289", 10 },
{ "furn_328", 10 },
{ "furn_329", 10 },
{ "furn_330", 10 },
{ "furn_331", 10 },
{ "furn_332", 10 },
{ "furn_364", 10 },
{ "furn_367", 10 },
{ "furn_373", 10 },
{ "furn_391", 10 },
{ "furn_406", 10 },
{ "furn_407", 10 },
{ "furn_408", 10 },
{ "furn_409", 10 },
{ "furn_410", 10 },
{ "furn_411", 10 },
{ "furn_412", 10 },
{ "furn_413", 10 },
{ "furn_414", 10 },
{ "furn_415", 10 },
{ "furn_416", 10 },
{ "furn_417", 10 },
{ "furn_418", 10 },
{ "furn_421", 10 },
{ "furn_494", 10 },
{ "furn_495", 10 },
{ "furn_496", 10 },
{ "furn_563", 10 },
{ "furn_578", 10 },
{ "furn_580", 10 },
{ "furn_582", 10 },
{ "furn_584", 10 },
{ "furn_586", 10 },
{ "furn_588", 10 },
{ "furn_622", 10 },
{ "furn_637", 10 },
{ "furn_639", 10 },
{ "furn_641", 10 },
{ "furn_642", 10 },
{ "furn_652", 10 },
{ "furn_659", 10 },
{ "furn_663", 10 },
{ "furn_664", 10 },
{ "furn_680", 10 },
{ "furn_687", 10 },
{ "furn_688", 10 },
{ "furn_690", 10 },
{ "furn_691", 10 },
{ "furn_692", 10 },
{ "furn_693", 10 },
{ "furn_694", 10 },
{ "furn_695", 10 },

                // Tables
{ "furn_61", 10 },
{ "furn_62", 10 },
{ "furn_63", 10 },
{ "furn_64", 10 },
{ "furn_138", 10 },
{ "furn_139", 10 },
{ "furn_140", 10 },
{ "furn_141", 10 },
{ "furn_142", 10 },
{ "furn_143", 10 },
{ "furn_144", 10 },
{ "furn_145", 10 },
{ "furn_146", 10 },
{ "furn_153", 10 },
{ "furn_154", 10 },
{ "furn_246", 10 },
{ "furn_266", 10 },
{ "furn_291", 10 },
{ "furn_292", 10 },
{ "furn_293", 10 },
{ "furn_327", 10 },
{ "furn_480", 10 },
{ "furn_481", 10 },
{ "furn_577", 10 },
{ "furn_579", 10 },
{ "furn_581", 10 },
{ "furn_583", 10 },
{ "furn_585", 10 },
{ "furn_587", 10 },
{ "furn_589", 10 },
{ "furn_665", 10 },
{ "furn_671", 10 },
{ "furn_678", 10 },
{ "furn_679", 10 },


                // Beds
{ "furn_52", 10 },
{ "furn_53", 10 },
{ "furn_55", 10 },
{ "furn_56", 10 },
{ "furn_111", 10 },
{ "furn_112", 10 },
{ "furn_113", 10 },
{ "furn_114", 10 },
{ "furn_115", 10 },
{ "furn_116", 10 },
{ "furn_117", 10 },
{ "furn_118", 10 },
{ "furn_119", 10 },
{ "furn_120", 10 },
{ "furn_151", 10 },
{ "furn_152", 10 },
{ "furn_155", 10 },
{ "furn_156", 10 },
{ "furn_238", 10 },
{ "furn_253", 10 },
{ "furn_287", 10 },
{ "furn_290", 10 },
{ "furn_294", 10 },
{ "furn_333", 10 },
{ "furn_334", 10 },
{ "furn_335", 10 },
{ "furn_336", 10 },
{ "furn_369", 10 },
{ "furn_428", 10 },
{ "furn_429", 10 },
{ "furn_430", 10 },
{ "furn_431", 10 },
{ "furn_432", 10 },
{ "furn_433", 10 },
{ "furn_434", 10 },
{ "furn_435", 10 },
{ "furn_436", 10 },
{ "furn_437", 10 },
{ "furn_438", 10 },
{ "furn_497", 10 },
{ "furn_498", 10 },
{ "furn_499", 10 },
{ "furn_554", 10 },
{ "furn_555", 10 },
{ "furn_556", 10 },
{ "furn_615", 10 },
{ "furn_616", 10 },
{ "furn_623", 10 },
{ "furn_638", 10 },
{ "furn_646", 10 },
{ "furn_647", 10 },
{ "furn_648", 10 },
{ "furn_650", 10 },
{ "furn_651", 10 },
{ "furn_654", 10 },
{ "furn_655", 10 },
{ "furn_666", 10 },
{ "furn_676", 10 },
{ "furn_681", 10 },
{ "furn_696", 10 },
{ "furn_697", 10 },

                // Closets
{ "furn_51", 10 },
{ "furn_54", 10 },
{ "furn_127", 10 },
{ "furn_128", 10 },
{ "furn_129", 10 },
{ "furn_130", 10 },
{ "furn_131", 10 },
{ "furn_132", 10 },
{ "furn_133", 10 },
{ "furn_134", 10 },
{ "furn_135", 10 },
{ "furn_136", 10 },
{ "furn_137", 10 },
{ "furn_196", 10 },
{ "furn_199", 10 },
{ "furn_209", 10 },
{ "furn_211", 10 },
{ "furn_222", 10 },
{ "furn_223", 10 },
{ "furn_224", 10 },
{ "furn_225", 10 },
{ "furn_226", 10 },
{ "furn_227", 10 },
{ "furn_228", 10 },
{ "furn_242", 10 },
{ "furn_244", 10 },
{ "furn_264", 10 },
{ "furn_265", 10 },
{ "furn_317", 10 },
{ "furn_318", 10 },
{ "furn_340", 10 },
{ "furn_341", 10 },
{ "furn_342", 10 },
{ "furn_343", 10 },
{ "furn_459", 10 },
{ "furn_551", 10 },
{ "furn_552", 10 },
{ "furn_553", 10 },
{ "furn_604", 10 },
{ "furn_605", 10 },
{ "furn_606", 10 },
{ "furn_619", 10 },
{ "furn_620", 10 },
{ "furn_621", 10 },
{ "furn_668", 10 },
{ "furn_669", 10 },
{ "furn_670", 10 },
{ "furn_672", 10 },
{ "furn_677", 10 },
{ "furn_682", 10 },
{ "furn_683", 10 },

                // Plants
{ "furn_29", 10 },
{ "furn_30", 10 },
{ "furn_46", 10 },
{ "furn_47", 10 },
{ "furn_48", 10 },
{ "furn_49", 10 },
{ "furn_312", 10 },
{ "furn_313", 10 },
{ "furn_455", 10 },
{ "furn_507", 10 },
{ "furn_508", 10 },
{ "furn_511", 10 },
{ "furn_512", 10 },
{ "furn_513", 10 },
{ "furn_514", 10 },
{ "furn_515", 10 },
{ "furn_516", 10 },
{ "furn_517", 10 },
{ "furn_518", 10 },
{ "furn_519", 10 },
{ "furn_520", 10 },
{ "furn_526", 10 },
{ "furn_527", 10 },
{ "furn_528", 10 },
{ "furn_529", 10 },
{ "furn_530", 10 },
{ "furn_531", 10 },
{ "furn_532", 10 },
{ "furn_533", 10 },
{ "furn_534", 10 },
{ "furn_535", 10 },
{ "furn_536", 10 },
{ "furn_537", 10 },
{ "furn_538", 10 },
{ "furn_539", 10 },
{ "furn_540", 10 },
{ "furn_541", 10 },
{ "furn_542", 10 },
{ "furn_543", 10 },
{ "furn_544", 10 },
{ "furn_634", 10 },
{ "furn_635", 10 },
{ "furn_643", 10 },
{ "furn_644", 10 },

                // Lamps
{ "furn_65", 10 },
{ "furn_66", 10 },
{ "furn_67", 10 },
{ "furn_68", 10 },
{ "furn_69", 10 },
{ "furn_70", 10 },
{ "furn_71", 10 },
{ "furn_72", 10 },
{ "furn_73", 10 },
{ "furn_74", 10 },
{ "furn_75", 10 },
{ "furn_76", 10 },
{ "furn_77", 10 },
{ "furn_78", 10 },
{ "furn_79", 10 },
{ "furn_80", 10 },
{ "furn_81", 10 },
{ "furn_82", 10 },
{ "furn_83", 10 },
{ "furn_84", 10 },
{ "furn_85", 10 },
{ "furn_86", 10 },
{ "furn_87", 10 },
{ "furn_88", 10 },
{ "furn_89", 10 },
{ "furn_90", 10 },
{ "furn_158", 10 },
{ "furn_159", 10 },
{ "furn_320", 10 },
{ "furn_321", 10 },
{ "furn_322", 10 },
{ "furn_323", 10 },
{ "furn_324", 10 },
{ "furn_325", 10 },
{ "furn_326", 10 },
{ "furn_344", 10 },
{ "furn_345", 10 },
{ "furn_348", 10 },
{ "furn_349", 10 },
{ "furn_572", 10 },
{ "furn_573", 10 },
{ "furn_574", 10 },
{ "furn_575", 10 },
{ "furn_576", 10 },
{ "furn_624", 10 },


                // Electronics
{ "furn_28", 10 },
{ "furn_121", 10 },
{ "furn_122", 10 },
{ "furn_123", 10 },
{ "furn_124", 10 },
{ "furn_125", 10 },
{ "furn_126", 10 },
{ "furn_210", 10 },
{ "furn_231", 10 },
{ "furn_248", 10 },
{ "furn_249", 10 },
{ "furn_250", 10 },
{ "furn_251", 10 },
{ "furn_267", 10 },
{ "furn_281", 10 },
{ "furn_285", 10 },
{ "furn_298", 10 },
{ "furn_337", 10 },
{ "furn_338", 10 },
{ "furn_339", 10 },
{ "furn_346", 10 },
{ "furn_347", 10 },
{ "furn_405", 10 },
{ "furn_425", 10 },
{ "furn_440", 10 },
{ "furn_445", 10 },
{ "furn_449", 10 },
{ "furn_452", 10 },
{ "furn_453", 10 },
{ "furn_468", 10 },
{ "furn_471", 10 },
{ "furn_472", 10 },
{ "furn_478", 10 },
{ "furn_479", 10 },
{ "furn_483", 10 },
{ "furn_488", 10 },
{ "furn_489", 10 },
{ "furn_490", 10 },
{ "furn_491", 10 },
{ "furn_492", 10 },
{ "furn_493", 10 },
{ "furn_546", 10 },
{ "furn_549", 10 },
{ "furn_564", 10 },
{ "furn_565", 10 },
{ "furn_566", 10 },
{ "furn_567", 10 },
{ "furn_568", 10 },
{ "furn_569", 10 },
{ "furn_570", 10 },
{ "furn_597", 10 },
{ "furn_598", 10 },
{ "furn_599", 10 },
{ "furn_600", 10 },
{ "furn_601", 10 },
{ "furn_602", 10 },
{ "furn_603", 10 },
{ "furn_607", 10 },
{ "furn_608", 10 },
{ "furn_609", 10 },
{ "furn_610", 10 },
{ "furn_611", 10 },
{ "furn_612", 10 },
{ "furn_640", 10 },
{ "furn_645", 10 },
{ "furn_660", 10 },
{ "furn_675", 10 },

                // Kitchen
{ "furn_202", 10 },
{ "furn_379", 10 },
{ "furn_380", 10 },
{ "furn_381", 10 },
{ "furn_382", 10 },
{ "furn_188", 10 },
{ "furn_191", 10 },
{ "furn_192", 10 },
{ "furn_193", 10 },
{ "furn_198", 10 },
{ "furn_200", 10 },
{ "furn_205", 10 },
{ "furn_426", 10 },
{ "furn_22", 10 },
{ "furn_23", 10 },
{ "furn_24", 10 },
{ "furn_189", 10 },
{ "furn_190", 10 },
{ "furn_194", 10 },
{ "furn_195", 10 },
{ "furn_197", 10 },
{ "furn_201", 10 },
{ "furn_203", 10 },
{ "furn_204", 10 },
{ "furn_206", 10 },
{ "furn_207", 10 },
{ "furn_208", 10 },
{ "furn_232", 10 },
{ "furn_233", 10 },
{ "furn_254", 10 },
{ "furn_268", 10 },
{ "furn_319", 10 },
{ "furn_378", 10 },
{ "furn_383", 10 },
{ "furn_384", 10 },
{ "furn_385", 10 },
{ "furn_386", 10 },
{ "furn_387", 10 },
{ "furn_388", 10 },
{ "furn_389", 10 },
{ "furn_390", 10 },
{ "furn_401", 10 },
{ "furn_402", 10 },
{ "furn_403", 10 },
{ "furn_423", 10 },
{ "furn_424", 10 },
{ "furn_427", 10 },
{ "furn_454", 10 },
{ "furn_469", 10 },
{ "furn_470", 10 },
{ "furn_473", 10 },
{ "furn_474", 10 },
{ "furn_475", 10 },
{ "furn_476", 10 },
{ "furn_477", 10 },
{ "furn_485", 10 },
{ "furn_486", 10 },
{ "furn_487", 10 },
{ "furn_545", 10 },
{ "furn_618", 10 },
{ "furn_628", 10 },
{ "furn_629", 10 },
{ "furn_673", 10 },
{ "furn_674", 10 },
{ "furn_684", 10 },
{ "furn_685", 10 },
{ "furn_686", 10 },
{ "furn_689", 10 },

                // Bath
{ "furn_50", 10 },
{ "furn_172", 10 },
{ "furn_173", 10 },
{ "furn_174", 10 },
{ "furn_176", 10 },
{ "furn_177", 10 },
{ "furn_178", 10 },
{ "furn_186", 10 },
{ "furn_187", 10 },
{ "furn_466", 10 },
{ "furn_181", 10 },
{ "furn_182", 10 },
{ "furn_184", 10 },
{ "furn_484", 10 },
{ "furn_591", 10 },
{ "furn_592", 10 },
{ "furn_593", 10 },
{ "furn_594", 10 },
{ "furn_595", 10 },
{ "furn_212", 10 },
{ "furn_625", 10 },
{ "furn_626", 10 },
{ "furn_627", 10 },
{ "furn_175", 10 },
{ "furn_179", 10 },
{ "furn_180", 10 },
{ "furn_183", 10 },
{ "furn_185", 10 },
{ "furn_216", 10 },
{ "furn_218", 10 },
{ "furn_219", 10 },
{ "furn_559", 10 },
{ "furn_560", 10 },
{ "furn_561", 10 },
{ "furn_562", 10 },



                // Pictures
{ "furn_0", 10 },
{ "furn_1", 10 },
{ "furn_2", 10 },
{ "furn_3", 10 },
{ "furn_4", 10 },
{ "furn_5", 10 },
{ "furn_161", 10 },
{ "furn_162", 10 },
{ "furn_163", 10 },
{ "furn_164", 10 },
{ "furn_165", 10 },
{ "furn_166", 10 },
{ "furn_167", 10 },
{ "furn_168", 10 },
{ "furn_169", 10 },
{ "furn_170", 10 },
{ "furn_260", 10 },
{ "furn_261", 10 },
{ "furn_262", 10 },
{ "furn_277", 10 },
{ "furn_278", 10 },
{ "furn_279", 10 },
{ "furn_286", 10 },
{ "furn_306", 10 },
{ "furn_307", 10 },
{ "furn_308", 10 },
{ "furn_309", 10 },
{ "furn_310", 10 },
{ "furn_311", 10 },
{ "furn_351", 10 },
{ "furn_352", 10 },
{ "furn_353", 10 },
{ "furn_354", 10 },
{ "furn_355", 10 },
{ "furn_356", 10 },
{ "furn_357", 10 },
{ "furn_358", 10 },
{ "furn_359", 10 },
{ "furn_360", 10 },
{ "furn_361", 10 },
{ "furn_362", 10 },
{ "furn_363", 10 },
{ "furn_500", 10 },
{ "furn_501", 10 },
{ "furn_502", 10 },
{ "furn_503", 10 },
{ "furn_504", 10 },
{ "furn_505", 10 },
{ "furn_506", 10 },
{ "furn_667", 10 },


                // Decores
{ "furn_6", 10 },
{ "furn_7", 10 },
{ "furn_8", 10 },
{ "furn_9", 10 },
{ "furn_10", 10 },
{ "furn_11", 10 },
{ "furn_12", 10 },
{ "furn_13", 10 },
{ "furn_14", 10 },
{ "furn_15", 10 },
{ "furn_16", 10 },
{ "furn_17", 10 },
{ "furn_18", 10 },
{ "furn_19", 10 },
{ "furn_20", 10 },
{ "furn_21", 10 },
{ "furn_25", 10 },
{ "furn_26", 10 },
{ "furn_27", 10 },
{ "furn_31", 10 },
{ "furn_32", 10 },
{ "furn_33", 10 },
{ "furn_34", 10 },
{ "furn_35", 10 },
{ "furn_36", 10 },
{ "furn_37", 10 },
{ "furn_38", 10 },
{ "furn_39", 10 },
{ "furn_40", 10 },
{ "furn_41", 10 },
{ "furn_42", 10 },
{ "furn_43", 10 },
{ "furn_44", 10 },
{ "furn_45", 10 },
{ "furn_160", 10 },
{ "furn_171", 10 },
{ "furn_213", 10 },
{ "furn_214", 10 },
{ "furn_215", 10 },
{ "furn_217", 10 },
{ "furn_220", 10 },
{ "furn_221", 10 },
{ "furn_243", 10 },
{ "furn_255", 10 },
{ "furn_257", 10 },
{ "furn_263", 10 },
{ "furn_269", 10 },
{ "furn_270", 10 },
{ "furn_271", 10 },
{ "furn_272", 10 },
{ "furn_273", 10 },
{ "furn_274", 10 },
{ "furn_275", 10 },
{ "furn_276", 10 },
{ "furn_280", 10 },
{ "furn_296", 10 },
{ "furn_297", 10 },
{ "furn_299", 10 },
{ "furn_300", 10 },
{ "furn_301", 10 },
{ "furn_302", 10 },
{ "furn_303", 10 },
{ "furn_304", 10 },
{ "furn_305", 10 },
{ "furn_314", 10 },
{ "furn_315", 10 },
{ "furn_316", 10 },
{ "furn_350", 10 },
{ "furn_365", 10 },
{ "furn_366", 10 },
{ "furn_368", 10 },
{ "furn_370", 10 },
{ "furn_371", 10 },
{ "furn_372", 10 },
{ "furn_374", 10 },
{ "furn_375", 10 },
{ "furn_376", 10 },
{ "furn_377", 10 },
{ "furn_392", 10 },
{ "furn_393", 10 },
{ "furn_394", 10 },
{ "furn_395", 10 },
{ "furn_396", 10 },
{ "furn_397", 10 },
{ "furn_398", 10 },
{ "furn_399", 10 },
{ "furn_400", 10 },
{ "furn_404", 10 },
{ "furn_419", 10 },
{ "furn_420", 10 },
{ "furn_422", 10 },
{ "furn_439", 10 },
{ "furn_441", 10 },
{ "furn_442", 10 },
{ "furn_443", 10 },
{ "furn_444", 10 },
{ "furn_446", 10 },
{ "furn_447", 10 },
{ "furn_448", 10 },
{ "furn_450", 10 },
{ "furn_451", 10 },
{ "furn_456", 10 },
{ "furn_457", 10 },
{ "furn_458", 10 },
{ "furn_460", 10 },
{ "furn_461", 10 },
{ "furn_462", 10 },
{ "furn_463", 10 },
{ "furn_464", 10 },
{ "furn_465", 10 },
{ "furn_467", 10 },
{ "furn_482", 10 },
{ "furn_509", 10 },
{ "furn_510", 10 },
{ "furn_521", 10 },
{ "furn_522", 10 },
{ "furn_523", 10 },
{ "furn_524", 10 },
{ "furn_525", 10 },
{ "furn_547", 10 },
{ "furn_548", 10 },
{ "furn_550", 10 },
{ "furn_557", 10 },
{ "furn_558", 10 },
{ "furn_571", 10 },
{ "furn_590", 10 },
{ "furn_596", 10 },
{ "furn_613", 10 },
{ "furn_614", 10 },
{ "furn_617", 10 },
{ "furn_630", 10 },
{ "furn_631", 10 },
{ "furn_632", 10 },
{ "furn_633", 10 },
{ "furn_636", 10 },
{ "furn_649", 10 },
{ "furn_653", 10 },
{ "furn_656", 10 },
{ "furn_657", 10 },
{ "furn_658", 10 },
{ "furn_661", 10 },
{ "furn_662", 10 },

            }
        };

        public override string ClientData => $"{ID}, {PositionInfo.ToCSharpStr()}, {GovPrice}, {Rent}, {Tax}f, {PositionInteract.ToCSharpStr()}";

        public FurnitureShop(int ID, Vector3 PositionInfo, Utils.Vector4 PositionInteract) : base(ID, PositionInfo, PositionInteract, DefaultType)
        {

        }

        public override bool TryBuyItem(PlayerData pData, bool useCash, string itemId)
        {
            if (pData.Furniture.Count + 1 >= Settings.HOUSE_MAX_FURNITURE)
            {
                pData.Player.Notify("Inv::PMPF", Settings.HOUSE_MAX_FURNITURE);

                return false;
            }

            uint newMats;
            ulong newBalance, newPlayerBalance;

            if (!TryProceedPayment(pData, useCash, itemId, 1, out newMats, out newBalance, out newPlayerBalance))
                return false;

            var furn = new Game.Estates.Furniture(itemId);

            pData.Info.AddFurniture(furn);

            ProceedPayment(pData, useCash, newMats, newBalance, newPlayerBalance);

            return true;
        }
    }
}