using System.Collections.Generic;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Items
{
    public partial class Weapon
    {
        public static readonly Dictionary<string, Item.ItemData> IdList = new Dictionary<string, Item.ItemData>()
        {
            { "w_asrifle", new ItemData("AK-47", 1.5f, "w_ar_assaultrifle", ItemData.TopTypes.AssaultRifle, "am_7.62", WeaponHash.Assaultrifle, 30, false) },
            { "w_asrifle_mk2", new ItemData("AK-47 (улучш.)", 1.5f, "w_ar_assaultriflemk2", ItemData.TopTypes.AssaultRifle, "am_7.62", WeaponHash.Assaultrifle_mk2, 35, false) },
            { "w_advrifle", new ItemData("TAR-21", 1f, "w_ar_advancedrifle", ItemData.TopTypes.AssaultRifle, "am_7.62", WeaponHash.Advancedrifle, 30, false) },
            { "w_carbrifle", new ItemData("AR-15", 1f, "w_ar_carbinerifle", ItemData.TopTypes.AssaultRifle, "am_7.62", WeaponHash.Carbinerifle, 30, false) },
            { "w_comprifle", new ItemData("AK-47 (укороч.)", 1f, "w_ar_assaultrifle_smg", ItemData.TopTypes.AssaultRifle, "am_7.62", WeaponHash.Compactrifle, 30, false) },
            { "w_heavyrifle", new ItemData("SCAR", 1f, "w_ar_heavyrifle", ItemData.TopTypes.AssaultRifle, "am_7.62", 0xC78D71B4, 30, false) },

            { "w_microsmg", new ItemData("UZI", 1f, "w_sb_microsmg", ItemData.TopTypes.SubMachine, "am_5.56", WeaponHash.Microsmg, 15, true) },
            { "w_minismg", new ItemData("Мини SMG", 1f, "w_sb_minismg", ItemData.TopTypes.SubMachine, "am_5.56", WeaponHash.Minismg, 15, true) },
            { "w_smg", new ItemData("MP5", 1f, "w_sb_smg", ItemData.TopTypes.SubMachine, "am_5.56", WeaponHash.Smg, 15, true) },
            { "w_smg_mk2", new ItemData("MP5 (улучш.)", 1f, "w_sb_smgmk2", ItemData.TopTypes.SubMachine, "am_5.56", WeaponHash.Smg_mk2, 15, true) },
            { "w_asmsg", new ItemData("Штурмовой SMG", 1f, "w_sb_assaultsmg", ItemData.TopTypes.SubMachine, "am_5.56", WeaponHash.Assaultsmg, 15, false) },
            { "w_combpdw", new ItemData("Боевой PDW", 1f, "w_sb_pdw", ItemData.TopTypes.SubMachine, "am_5.56", WeaponHash.Combatpdw, 15, false) },

            { "w_combmg", new ItemData("M249", 1f, "w_mg_combatmg", ItemData.TopTypes.LightMachine, "am_9", WeaponHash.Combatmg, 15, false) },
            { "w_gusenberg", new ItemData("ПП Томпсона", 1f, "w_sb_gusenberg", ItemData.TopTypes.LightMachine, "am_9", WeaponHash.Gusenberg, 15, false) },

            { "w_heavysnp", new ItemData("Barrett M82", 1f, "w_sr_heavysniper", ItemData.TopTypes.SniperRifle, "am_12.7", WeaponHash.Heavysniper, 15, false) },
            { "w_markrifle", new ItemData("Винтовка Марксмана", 1f, "w_sr_marksmanrifle", ItemData.TopTypes.SniperRifle, "am_12.7", WeaponHash.Marksmanrifle, 15, false) },
            { "w_musket", new ItemData("Мушкет", 1f, "w_ar_musket", ItemData.TopTypes.SniperRifle, "am_12.7", WeaponHash.Musket, 15, false) },

            { "w_assgun", new ItemData("Штурмовой дробовик", 1f, "w_sg_assaultshotgun", ItemData.TopTypes.Shotgun, "am_12", WeaponHash.Assaultshotgun, 15, false) },
            { "w_heavysgun", new ItemData("Тяжелый дробовик", 1f, "w_sg_heavyshotgun", ItemData.TopTypes.Shotgun, "am_12", WeaponHash.Heavyshotgun, 15, false) },
            { "w_pumpsgun", new ItemData("Помповый дробовик", 1f, "w_sg_pumpshotgun", ItemData.TopTypes.Shotgun, "am_12", WeaponHash.Pumpshotgun, 15, false) },
            { "w_pumpsgun_mk2", new ItemData("Помповый дробовик (улучш.)", 1f, "w_sg_pumpshotgunmk2", ItemData.TopTypes.Shotgun, "am_12", WeaponHash.Pumpshotgun_mk2, 15, false) },
            { "w_sawnsgun", new ItemData("Обрез", 1f, "w_sg_sawnoff", ItemData.TopTypes.Shotgun, "am_12", WeaponHash.Sawnoffshotgun, 15, false) },

            { "w_pistol", new ItemData("Пистолет", 0.5f, "w_pi_pistol", ItemData.TopTypes.HandGun, "am_5.56", WeaponHash.Pistol, 15, true) },
            { "w_pistol_mk2", new ItemData("Пистолет (улучш.)", 1f, "w_pi_pistolmk2", ItemData.TopTypes.HandGun, "am_5.56", WeaponHash.Pistol_mk2, 20, true) },
            { "w_appistol", new ItemData("Автоматический пистолет", 1f, "w_pi_appistol", ItemData.TopTypes.HandGun, "am_5.56", WeaponHash.Appistol, 20, true) },
            { "w_combpistol", new ItemData("P2000", 1f, "w_pi_combatpistol", ItemData.TopTypes.HandGun, "am_5.56", WeaponHash.Combatpistol, 20, true) },
            { "w_heavypistol", new ItemData("Remington 1911", 1f, "w_pi_heavypistol", ItemData.TopTypes.HandGun, "am_5.56", WeaponHash.Heavypistol, 20, true) },
            { "w_machpistol", new ItemData("TEC-9", 1f, "w_sb_smgmk2", ItemData.TopTypes.HandGun, "am_5.56", WeaponHash.Machinepistol, 20, true) },
            { "w_markpistol", new ItemData("Пистолет Марксмана", 1f, "w_pi_singleshot", ItemData.TopTypes.HandGun, "am_5.56", WeaponHash.Marksmanpistol, 20, true) },
            { "w_vintpistol", new ItemData("Винтажный пистолет", 1f, "w_pi_vintage_pistol", ItemData.TopTypes.HandGun, "am_5.56", WeaponHash.Vintagepistol, 20, true) },

            { "w_revolver", new ItemData("Револьвер", 1f, "w_pi_revolver", ItemData.TopTypes.HandGun, "am_11.43", WeaponHash.Revolver, 20, true) },
            { "w_revolver_mk2", new ItemData("Револьвер (улучш.)", 1f, "w_pi_revolvermk2", ItemData.TopTypes.HandGun, "am_11.43", WeaponHash.Revolver_mk2, 20, true) },

            { "w_bat", new ItemData("Бита", 1f, "w_me_bat", ItemData.TopTypes.Melee, null, WeaponHash.Bat, 0, false) },
            { "w_bottle", new ItemData("'Розочка'", 1f, "w_me_bottle", ItemData.TopTypes.Melee, null, WeaponHash.Bottle, 0, false) },
            { "w_crowbar", new ItemData("Гвоздодёр", 1f, "w_me_crowbar", ItemData.TopTypes.Melee, null, WeaponHash.Crowbar, 0, false) },
            { "w_dagger", new ItemData("Клинок", 1f, "w_me_dagger", ItemData.TopTypes.Melee, null, WeaponHash.Dagger, 0, false) },
            { "w_flashlight", new ItemData("Фонарик", 1f, "w_me_flashlight", ItemData.TopTypes.Melee, null, WeaponHash.Flashlight, 0, false) },
            { "w_golfclub", new ItemData("Клюшка", 1f, "w_me_gclub", ItemData.TopTypes.Melee, null, WeaponHash.Golfclub, 0, false) },
            { "w_hammer", new ItemData("Молоток", 1f, "w_me_hammer", ItemData.TopTypes.Melee, null, WeaponHash.Hammer, 0, false) },
            { "w_hatchet", new ItemData("Топор", 1f, "w_me_hatchet", ItemData.TopTypes.Melee, null, WeaponHash.Hatchet, 0, false) },
            { "w_knuckles", new ItemData("Кастет", 1f, "w_me_knuckle", ItemData.TopTypes.Melee, null, WeaponHash.Knuckle, 0, false) },
            { "w_machete", new ItemData("Мачете", 1f, "prop_ld_w_me_machette", ItemData.TopTypes.Melee, null, WeaponHash.Machete, 0, false) },
            { "w_nightstick", new ItemData("Резиновая дубинка", 1f, "w_me_nightstick", ItemData.TopTypes.Melee, null, WeaponHash.Nightstick, 0, false) },
            { "w_poolcue", new ItemData("Кий", 1f, "prop_pool_cue", ItemData.TopTypes.Melee, null, WeaponHash.Poolcue, 0, false) },
            { "w_switchblade", new ItemData("Складной нож", 1f, "w_me_switchblade", ItemData.TopTypes.Melee, null, WeaponHash.Switchblade, 0, false) },
            { "w_wrench", new ItemData("Гаечный ключ", 0.75f, "prop_cs_wrench", ItemData.TopTypes.Melee, null, WeaponHash.Wrench, 0, false) },
        };
    }
}