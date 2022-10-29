using GTANetworkAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BCRPServer.Game.Items
{
    #region Item Class
    [JsonConverter(typeof(ItemConverter))]
    public abstract class Item
    {
        #region All Types
        public enum Types
        {
            NotAssigned = -1,

            #region Clothes
            Hat = 0,
            Top,
            Under,
            Gloves,
            Pants,
            Shoes,
            Mask,
            Accessory,
            Glasses,
            Ears,
            Watches,
            Bracelet,
            Ring,
            #endregion

            BArmShop,
            Bag, Holster,

            #region Weapons
            Stungun,
            Pistol,
            CombatPistol,
            HeavyPistol,
            VintagePistol,
            MarksmanPistol,
            Revolver,
            RevolverMk2,
            APPistol,
            PistolMk2,
            SMG,
            MicroSMG,
            AssaultSMG,
            CombatPDW,
            Gusenberg,
            MiniSMG,
            SMGMk2,
            CombatMG,
            MachinePistol,
            AssaultRifle,
            AssaultRifleMk2,
            CarbineRifle,
            AdvancedRifle,
            CompactRifle,
            HeavyRifle,
            HeavySniper,
            MarksmanRifle,
            PumpShotgun,
            SawnOffShotgun,
            AssaultShotgun,
            Musket,
            HeavyShotgun,
            PumpShotgunMk2,
            Knife,
            Nightstick,
            Hammer,
            Bat,
            Crowbar,
            GolfClub,
            Bottle,
            Dagger,
            Hatchet,
            Knuckles,
            Machete,
            Flashlight,
            SwitchBlade,
            PoolCue,
            Wrench,
            #endregion

            #region Other Stuff
            Ammo5_56, Ammo7_62, Ammo9, Ammo11_43, Ammo12, Ammo12_7,

            Numberplate0, Numberplate1, Numberplate2, Numberplate3, Numberplate4, Numberplate5,

            VehKey,
            #endregion,

            Burger,
            Chips,
            Hotdog,
            Chocolate,
            Pizza,
            Cola,
            Cigarettes,
            Joint,
            Beer,
            Vodka,
            Rum,
            Smoothie,
            VegSmoothie,
            Milkshake,
            Milk,
        }
        #endregion

        public class ItemData
        {
            /// <summary>Стандартная модель</summary>
            public static uint DefaultModel = NAPI.Util.GetHashKey("prop_drug_package_02");

            /// <summary>Вес единицы предмета</summary>
            public float Weight { get; set; }

            /// <summary>Основная модель</summary>
            public uint Model { get => Models[0]; }

            /// <summary>Все модели</summary>
            private uint[] Models { get; set; }

            public ItemData(float Weight, params uint[] Models)
            {
                this.Weight = Weight;

                this.Models = Models.Length > 0 ? Models : new uint[] { DefaultModel };
            }

            public ItemData(float Weight, params string[] Models) : this(Weight, Models.Select(x => NAPI.Util.GetHashKey(x)).ToArray()) { }

            public uint GetModelAt(int idx) => idx < 0 || idx >= Models.Length ? Model : Models[idx];
        }

        private static Dictionary<Types, ItemData> AllData = new Dictionary<Types, ItemData>()
        {
            { Types.NotAssigned, new ItemData(0f, ItemData.DefaultModel) },

            #region Clothes
            { Types.Hat, new ItemData(0.1f, "prop_proxy_hat_01") },
            { Types.Top, new ItemData(0.3f, "prop_ld_shirt_01") },
            { Types.Under, new ItemData(0.2f, "prop_ld_tshirt_02") },
            { Types.Gloves, new ItemData(0.1f, ItemData.DefaultModel) },
            { Types.Pants, new ItemData(0.4f, "p_laz_j02_s") },
            { Types.Shoes, new ItemData(0.3f, "prop_ld_shoe_01") },
            { Types.Mask, new ItemData(0.2f, "p_trev_ski_mask_s") },
            { Types.Accessory, new ItemData(0.2f, "p_jewel_necklace_02") },
            { Types.Glasses, new ItemData(0.2f, "prop_cs_sol_glasses") },
            { Types.Ears, new ItemData(0.1f, "p_tmom_earrings_s") },
            { Types.Watches, new ItemData(0.1f, "prop_jewel_02b") },
            { Types.Bracelet, new ItemData(0.1f, "prop_jewel_02b") },
            #endregion

            { Types.BArmShop, new ItemData(0.5f, "prop_armour_pickup") },
            { Types.Bag, new ItemData(0.25f, "prop_cs_heist_bag_01") },
            { Types.Holster, new ItemData(0.1f, "prop_holster_01") },

            #region Weapons
            { Types.Stungun, new ItemData(1f, "w_pi_stungun") },
            { Types.Pistol,  new ItemData(0.5f, "w_pi_pistol") },
            { Types.CombatPistol, new ItemData(1f, "w_pi_combatpistol") },
            { Types.HeavyPistol, new ItemData(1f, "w_pi_heavypistol") },
            { Types.VintagePistol, new ItemData(1f, "w_pi_vintage_pistol") },
            { Types.MarksmanPistol, new ItemData(1f, "w_pi_singleshot") },
            { Types.Revolver, new ItemData(1f, "w_pi_revolver") },
            { Types.APPistol, new ItemData(1f, "w_pi_appistol") },
            { Types.PistolMk2, new ItemData(1f, "w_pi_pistolmk2") },
            { Types.RevolverMk2, new ItemData(1f, "w_pi_revolvermk2") },
            { Types.MicroSMG, new ItemData(1f, "w_sb_microsmg") },
            { Types.SMG, new ItemData(1f, "w_sb_smg") },
            { Types.AssaultSMG, new ItemData(1f, "w_sb_assaultsmg") },
            { Types.CombatPDW, new ItemData(1f, "w_sb_pdw") },
            { Types.Gusenberg, new ItemData(1f, "w_sb_gusenberg") },
            { Types.MiniSMG, new ItemData(1f, "w_sb_minismg") },
            { Types.SMGMk2, new ItemData(1f, "w_sb_smgmk2") },
            { Types.CombatMG, new ItemData(1f, "w_mg_combatmg") },
            { Types.MachinePistol, new ItemData(1f, "w_sb_smgmk2") }, // ?
            { Types.AssaultRifle, new ItemData(1.5f, "w_ar_assaultrifle") },
            { Types.CarbineRifle, new ItemData(1f, "w_ar_carbinerifle") },
            { Types.AdvancedRifle, new ItemData(1f, "w_ar_advancedrifle") },
            { Types.CompactRifle, new ItemData(1f, "w_ar_assaultrifle_smg") },
            { Types.AssaultRifleMk2, new ItemData(1.5f, "w_ar_assaultriflemk2") },
            { Types.HeavyRifle, new ItemData(1f, "w_ar_heavyrifle") },
            { Types.HeavySniper, new ItemData(1f, "w_sr_heavysniper") },
            { Types.MarksmanRifle, new ItemData(1f, "w_sr_marksmanrifle") },
            { Types.PumpShotgun, new ItemData(1f, "w_sg_pumpshotgun") },
            { Types.SawnOffShotgun, new ItemData(1f, "w_sg_sawnoff") },
            { Types.AssaultShotgun, new ItemData(1f, "w_sg_assaultshotgun") },
            { Types.Musket, new ItemData(1f, "w_ar_musket") },
            { Types.HeavyShotgun, new ItemData(1f, "w_sg_heavyshotgun") },
            { Types.PumpShotgunMk2, new ItemData(1f, "w_sg_pumpshotgunmk2") },
            { Types.Knife, new ItemData(1f, "w_me_knife_01") },
            { Types.Nightstick, new ItemData(1f, "w_me_nightstick") },
            { Types.Hammer, new ItemData(1f, "w_me_hammer") },
            { Types.Bat, new ItemData(1f, "w_me_bat") },
            { Types.Crowbar, new ItemData(1f, "w_me_crowbar") },
            { Types.GolfClub, new ItemData(1f, "w_me_gclub") },
            { Types.Bottle, new ItemData(1f, "w_me_bottle") },
            { Types.Dagger, new ItemData(1f, "w_me_dagger") },
            { Types.Hatchet, new ItemData(1f, "w_me_hatchet") },
            { Types.Knuckles, new ItemData(1f, "w_me_knuckle") },
            { Types.Machete, new ItemData(1f, "prop_ld_w_me_machette") },
            { Types.Flashlight, new ItemData(1f, "w_me_flashlight") },
            { Types.SwitchBlade, new ItemData(1f, "w_me_switchblade") },
            { Types.PoolCue, new ItemData(1f, "prop_pool_cue") },
            { Types.Wrench, new ItemData(0.75f, "prop_cs_wrench") },
            #endregion

            // Патроны
            { Types.Ammo5_56, new ItemData(0.01f, "w_am_case") },
            { Types.Ammo7_62, new ItemData(0.01f, "w_am_case") },
            { Types.Ammo9, new ItemData(0.01f, "w_am_case") },
            { Types.Ammo11_43, new ItemData(0.015f, "w_am_case") },
            { Types.Ammo12, new ItemData(0.015f, "w_am_case") },
            { Types.Ammo12_7, new ItemData(0.015f, "w_am_case") },

            { Types.Numberplate0, new ItemData(0.15f, "p_num_plate_01") },
            { Types.Numberplate1, new ItemData(0.15f, "p_num_plate_04") },
            { Types.Numberplate2, new ItemData(0.15f, "p_num_plate_02") },
            { Types.Numberplate3, new ItemData(0.15f, "p_num_plate_02") },
            { Types.Numberplate4, new ItemData(0.15f, "p_num_plate_01") },

            { Types.VehKey, new ItemData(0.05f, "p_car_keys_01") },

            { Types.Cigarettes, new ItemData(0.01f, "prop_cigar_pack_01", "prop_amb_ciggy_01", "ng_proc_cigarette01a") },

            { Types.Burger, new ItemData(0.15f, "prop_cs_burger_01") },
            { Types.Chips, new ItemData(0.1f, "prop_food_bs_chips") },
            { Types.Hotdog, new ItemData(0.15f, "prop_cs_hotdog_01") },
            { Types.Chocolate, new ItemData(0.1f, "prop_candy_pqs") },
            { Types.Pizza, new ItemData(0.25f, "v_res_tt_pizzaplate") },
            { Types.Cola, new ItemData(0.25f, "prop_food_juice01") },
            { Types.Joint, new ItemData(0.01f, "p_amb_joint_01") },
            { Types.Beer, new ItemData(0.25f, "prop_sh_beer_pissh_01") },
            { Types.Rum, new ItemData(0.25f, "prop_rum_bottle") },
            { Types.Vodka, new ItemData(0.25f, "prop_vodka_bottle") },
            { Types.VegSmoothie, new ItemData(0.25f, "prop_wheat_grass_glass") },
            { Types.Smoothie, new ItemData(0.25f, "p_cs_shot_glass_2_s") },
            { Types.Milkshake, new ItemData(0.25f, "prop_drink_whtwine") },
        };

        public static float GetWeight(Types type) => AllData[type].Weight;
        public static ItemData GetData(Types type) => AllData[type];

        [JsonIgnore]
        public ItemData Data { get; set; }

        /// <summary>Стандартный вес предмета (1 единица)</summary>
        [JsonIgnore]
        public float Weight { get => Data.Weight; }

        /// <summary>ID модели предмета</summary>
        [JsonIgnore]
        public uint Model { get => Data.Model; }

        /// <summary>Тип предмета (см. Game.Items.Item -> Types enum)</summary>
        [JsonIgnore]
        public Types Type { get; set; }
        /// <summary>Является ли предмет временным?</summary>
        [JsonIgnore]
        public bool IsTemp { get => UID == 0; }

        /// <summary>Уникальный ID предмета</summary>
        /// <value>UID предмета, 0 - если предмет временный и не хранится в базе данных</value>
        [JsonIgnore]
        public uint UID { get; set; }

        /// <summary>ID предмета (см. Game.Items.Items.LoadAll)</summary>
        public string ID { get; set; }

        /// <summary>Метод для удаления предмета из базы данных</summary>
        public void Delete()
        {
            if (IsTemp)
                return;

            MySQL.DeleteItem(this);
        }

        /// <summary>Метод для обновления предмета в базе данных</summary>
        public void Update()
        {
            if (IsTemp)
                return;

            MySQL.UpdateItem(this);
        }
    }
    #endregion

    #region Interfaces
    /// <summary>Этот интерфейс реализуют классы таких предметов, которые могут хранить в себе другие предметы</summary>
    public interface IContainer
    {
        /// <summary>Предметы в контейнере</summary>
        /// <value>Массив объектов класса Game.Items.Item, в котором null - пустой слот</value>
        public Item[] Items { get; set; }

        /// <summary>Вес контейнера</summary>
        [JsonIgnore]
        public float Weight { get; }
    }

    /// <summary>Этот интерфейс реализуют классы таких предметов, которые могут надеваться на игрока</summary>
    public interface IWearable
    {
        /// <summary>Метод для того, чтобы надеть предмет на игрока</summary>
        /// <param name="player">Сущность игрока</param>
        void Wear(Player player);

        /// <summary>Метод для того, чтобы снять предмет с игрока</summary>
        /// <param name="player">Сущность игрока</param>
        void Unwear(Player player);
    }

    /// <summary>Этот интерфейс реализуют классы таких предметов, которые способны стакаться</summary>
    public interface IStackable
    {
        /// <summary>Максимальное кол-во предметов в стаке</summary>
        [JsonIgnore]
        public int MaxAmount { get; }

        /// <summary>Общий вес стака</summary>
        [JsonIgnore]
        public float Weight { get; }

        /// <summary>Кол-во предметов в стаке</summary>
        public int Amount { get; set; }
    }

    /// <summary>Этот интерфейс реализуют классы таких предметов, которые, помимо названия, имеют уникальный тэг</summary>
    public interface ITagged
    {
        /// <summary>Тэг</summary>
        public string Tag { get; set; }

        /// <summary>Метод для генерации тэга</summary>
        /// <returns>Тэг</returns>
        public string GenerateTag();
    }

    /// <summary>Этот интерфейс реализуют классы таких предметов, которые, помимо названия, имеют уникальный тэг</summary>
    /// <remarks>В данный момент используется как пометка, реализации нет</remarks>
    public interface IActionable
    {
/*        [JsonIgnore]
        public Dictionary<int, Action<Player>> Actions { get; set; }*/
    }
    #endregion

    #region Weapon
    public class Weapon : Item, IActionable, ITagged, IWearable
    {
        public class ItemData
        {
            public enum TopTypes
            {
                /// <summary>Рукопашное</summary>
                Melee = 0,
                /// <summary>Одноручное</summary>
                HandGun,
                /// <summary>Полуавтоматическое</summary>
                SubMachine,
                /// <summary>Пулемёты</summary>
                LightMachine,
                /// <summary>Дробовик</summary>
                Shotgun,
                /// <summary>Штурмовая винтовка</summary>
                AssaultRifle,
                /// <summary>Снайперская винтовка</summary>
                SniperRifle,
                /// <summary>Тяжелое оружие</summary>
                HeavyWeapon,
                /// <summary>Метательное</summary>
                Throwable,
                /// <summary>Прочее</summary>
                Misc
            }

            /// <summary>Тип предмета</summary>
            public Types ItemType { get; set; }

            /// <summary>Тип патронов</summary>
            /// <value>Тип патронов, если оружие способно стрелять и заряжаться, null - в противном случае</value>
            public Types? AmmoType { get; set; }

            /// <summary>Высший тип оружия</summary>
            public TopTypes TopType { get; set; }

            /// <summary>Максимальное кол-во патронов в обойме</summary>
            public int MaxAmmo { get; set; }

            /// <summary>Хэш оружия</summary>
            public uint Hash { get; set; }

            /// <summary>Может ли использоваться в транспорте?</summary>
            public bool CanUseInVehicle { get; set; }

            /// <summary>Подходящие типы для крепления оружия на игрока</summary>
            [JsonIgnore]
            public Sync.AttachSystem.Types[] AttachTypes { get; set; }

            /// <summary>Создать новое оружие</summary>
            /// <param name="ItemType">Глобальный тип предмета</param>
            /// <param name="TopType">Тип оружия</param>
            /// <param name="AmmoType">Тип патронов (если нет - null)</param>
            /// <param name="Hash">Хэш оружия</param>
            /// <param name="MaxAmmo">Максимальное кол-во патронов</param>
            /// <param name="CanUseInVehicle">Может ли использоваться в транспорте?</param>
            public ItemData(Types ItemType, TopTypes TopType, Types? AmmoType, uint Hash, int MaxAmmo, bool CanUseInVehicle = false)
            {
                this.ItemType = ItemType;
                this.TopType = TopType;
                this.AmmoType = AmmoType;

                this.CanUseInVehicle = CanUseInVehicle;

                this.Hash = Hash;

                this.MaxAmmo = MaxAmmo;

                if (TopType == TopTypes.Shotgun || TopType == TopTypes.AssaultRifle || TopType == TopTypes.SniperRifle || TopType == TopTypes.HeavyWeapon)
                    this.AttachTypes = new Sync.AttachSystem.Types[] { Sync.AttachSystem.Types.WeaponLeftBack, Sync.AttachSystem.Types.WeaponRightBack };
                else if (TopType == TopTypes.SubMachine)
                    this.AttachTypes = new Sync.AttachSystem.Types[] { Sync.AttachSystem.Types.WeaponLeftTight, Sync.AttachSystem.Types.WeaponRightTight };
                else
                    this.AttachTypes = null;
            }

            /// <inheritdoc cref="ItemData.ItemData(Types, TopTypes, Types?, uint, int, bool)"/>
            public ItemData(Types ItemType, TopTypes TopType, Types? AmmoType, WeaponHash Hash, int MaxAmmo, bool CanUseInVehicle = false) : this(ItemType, TopType, AmmoType, (uint)Hash, MaxAmmo, CanUseInVehicle) { }
        }

        public static Dictionary<string, ItemData> IDList;

        [JsonIgnore]
        public ItemData Data { get; set; }

        /// <summary>Обший вес оружия (вместе с патронами в обойме)</summary>
        [JsonIgnore]
        public float Weight { get => Data.AmmoType == null ? (this as Item).Weight : (this as Item).Weight + Ammo * Item.GetWeight((Types)Data.AmmoType); }

        public string Tag { get; set; }

        /// <summary>Используется ли оружие?</summary>
        [JsonIgnore]
        public bool Equiped { get; set; }

        /// <summary>ID привязанного к игроку объекта оружия</summary>
        [JsonIgnore]
        public int AttachID { get; set; }

        /// <summary>Кол-во патронов в обойме</summary>
        public int Ammo { get; set; }

        /// <summary>Метод для выдачи оружия игроку</summary>
        /// <param name="player">Сущность игрока</param>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public void Equip(Player player)
        {
            var pData = player.GetMainData();

            if (Equiped || pData == null || player?.Exists != true)
                return;

            if (pData.PhoneOn)
                Sync.Players.StopUsePhone(player);

            Unwear(player);

            Equiped = true;

            player.SetWeapon(Data.Hash, Ammo);

            NAPI.Task.Run(() =>
            {
                var weap = pData.ActiveWeapon;

                if (weap != null && weap.Value.WeaponItem == this)
                    player.SetWeapon(Data.Hash, Ammo);
            }, 250);
        }

        /// <summary>Метод, чтобы забрать оружие у игрока</summary>
        /// <param name="player">Сущность игрока</param>
        /// <param name="updateLastAmmoFirst">Обновить ли кол-во патронов в обойме?</param>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public void Unequip(Player player, bool updateLastAmmoFirst = true, bool wearAfter = true)
        {
            if (player == null || !Equiped)
                return;

            if (updateLastAmmoFirst)
            {
                var amount = NAPI.Player.GetPlayerWeaponAmmo(player, this.Data.Hash);

                if (amount < 0)
                    amount = 0;

                if (amount <= this.Ammo)
                {
                    this.Ammo = amount;

                    this.Update();
                }
            }

            player.SetWeapon((uint)WeaponHash.Unarmed);

            Equiped = false;

            if (wearAfter)
                Wear(player);
        }

        /// <summary>Метод, чтобы выдать патроны игроку</summary>
        /// <param name="player">Сущность игрока</param>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public void UpdateAmmo(Player player)
        {
            if (!Equiped || player == null)
                return;

            player.SetAmmo(Ammo);
            player.TriggerEvent("Weapon::TaskReload");
        }

        public string GenerateTag()
        {
            return "";
        }

        public void Wear(Player player)
        {
            if (player?.Exists != true)
                return;

            if (AttachID != -1 || Equiped)
                return;

            var pData = player.GetMainData();

            if (pData == null)
                return;

            if (pData.Holster?.Items[0] == this)
            {
                pData.Holster.WearWeapon(player);

                return;
            }

            var attachTypes = this.Data.AttachTypes;

            if (attachTypes == null)
                return;

            if (pData.AttachedObjects.Where(x => x.Type == attachTypes[0]).Any())
                AttachID = player.AttachObject(Model, attachTypes[1], -1);
            else
                AttachID = player.AttachObject(Model, attachTypes[0], -1);
        }

        public void Unwear(Player player)
        {
            if (player?.Exists != true)
                return;

            var pData = player.GetMainData();

            if (pData == null)
                return;

            if (pData.Holster != null && (pData.Holster.Items[0] == null || pData.Holster.Items[0] == this))
                pData.Holster.UnwearWeapon(player);

            if (AttachID == -1)
                return;

            player.DetachObject(AttachID);

            AttachID = -1;
        }

        public Weapon(string ID)
        {
            this.ID = ID;

            this.Data = IDList[ID];
            this.Type = Data.ItemType;

            base.Data = Item.GetData(Type);

            this.Tag = "";
            this.Ammo = 0;

            this.Equiped = false;

            this.AttachID = -1;
        }
    }
    #endregion

    #region Ammo
    public class Ammo : Item, IStackable
    {
        public static Dictionary<string, Item.Types> IDList;

        [JsonIgnore]
        public float Weight { get => Amount * base.Weight; }

        [JsonIgnore]
        public int MaxAmount => 999;

        public int Amount { get; set; }

        public Ammo(string ID)
        {
            this.Amount = 0;
            this.ID = ID;

            this.Type = IDList[ID];

            base.Data = Item.GetData(Type);
        }
    }
    #endregion

    #region Clothes
    public class Clothes : Item, IWearable, IActionable
    {
        [JsonIgnore]
        public Game.Data.Clothes.Data Data { get; set; }
        [JsonIgnore]
        public Game.Data.Clothes.Data SexAlternativeData { get; set; }

        #region Wear
        public void Wear(Player player)
        {
            if (player?.Exists != true)
                return;

            var pData = player.GetMainData();

            if (pData == null)
                return;

            var data = Data;

            if (data.Sex != pData.Sex)
                data = SexAlternativeData;

            if (data == null)
                return;

            var variation = Var < data.Textures.Length && Var >= 0 ? data.Textures[Var] : 0;

            var slot = Game.Data.Clothes.GetSlot(Type);

            if (!data.IsProp)
            {
                if (data.ItemType == Types.Top)
                {
                    var tData = data as Game.Data.Clothes.Top;

                    if (Toggled)
                    {
                        player.SetClothes(slot, tData.ExtraData.Drawable, variation);
                        player.SetClothes(3, tData.ExtraData.BestTorso, 0);
                    }
                    else
                    {
                        player.SetClothes(slot, data.Drawable, variation);
                        player.SetClothes(3, tData.BestTorso, 0);
                    }

                    if (pData.Armour != null)
                    {
                        var aData = pData.Armour.Data;

                        player.SetClothes(9, aData.Drawables[pData.Sex].DrawableTop, aData.Texture);
                    }

                    if (pData.Clothes[2] != null)
                        pData.Clothes[2].Wear(player);
                    else
                        pData.Accessories[7]?.Wear(player);
                }
                else if (data.ItemType == Types.Under)
                {
                    var uData = data as Game.Data.Clothes.Under;

                    if (pData.Clothes[1] == null && uData.BestTop != null)
                    {
                        if (Toggled && uData.BestTop.ExtraData != null)
                        {
                            player.SetClothes(11, uData.BestTop.ExtraData.Drawable, variation);
                            player.SetClothes(3, uData.BestTop.ExtraData.BestTorso, 0);
                        }
                        else
                        {
                            player.SetClothes(11, uData.BestTop.Drawable, variation);
                            player.SetClothes(3, uData.BestTop.BestTorso, 0);
                        }
                    }
                    else
                    {
                        if (Toggled && uData.ExtraData != null)
                        {
                            player.SetClothes(slot, uData.ExtraData.Drawable, variation);
                            player.SetClothes(3, uData.ExtraData.BestTorso, 0);
                        }
                        else
                        {
                            player.SetClothes(slot, data.Drawable, variation);
                            player.SetClothes(3, uData.BestTorso, 0);
                        }
                    }

                    pData.Accessories[7]?.Wear(player);
                }
                else if (data.ItemType == Types.Gloves)
                {
                    var gData = data as Game.Data.Clothes.Gloves;

                    int curTorso = player.GetClothesDrawable(slot);

                    if (gData.BestTorsos.ContainsKey(curTorso))
                        player.SetClothes(slot, gData.BestTorsos[curTorso], variation);
                }
                else
                    player.SetClothes(slot, data.Drawable, variation);
            }
            else
            {
                if (data.ItemType == Types.Hat)
                {
                    var hData = data as Game.Data.Clothes.Hat;

                    if (hData.ExtraData != null)
                        player.SetAccessories(slot, Toggled ? hData.ExtraData.Drawable : data.Drawable, variation);
                    else
                        player.SetAccessories(slot, data.Drawable, variation);

                    pData.Hat = $"{this.ID}|{variation}|{(Toggled ? 1 : 0)}";
                }
                else
                    player.SetAccessories(slot, data.Drawable, variation);
            }
        }
        #endregion

        #region Unwear
        public void Unwear(Player player)
        {
            if (player?.Exists != true)
                return;

            var pData = player.GetMainData();

            if (pData == null)
                return;

            var data = Data;

            var slot = Game.Data.Clothes.GetSlot(Type);

            if (!data.IsProp)
            {
                if (data.ItemType == Types.Top)
                {
                    player.SetClothes(slot, Game.Data.Clothes.GetNudeDrawable(Types.Top, pData.Sex), 0);
                    player.SetClothes(3, Game.Data.Clothes.GetNudeDrawable(Types.Gloves, pData.Sex), 0);

                    if (pData.Armour != null)
                    {
                        var aData = pData.Armour.Data;

                        player.SetClothes(9, aData.Drawables[pData.Sex].Drawable, aData.Texture);
                    }

                    if (pData.Clothes[2] != null)
                        pData.Clothes[2].Wear(player);
                    else
                        pData.Accessories[7]?.Wear(player);
                }
                else if (data.ItemType == Types.Under)
                {
                    if (pData.Clothes[1] == null)
                    {
                        player.SetClothes(11, Game.Data.Clothes.GetNudeDrawable(Types.Top, pData.Sex), 0);
                        player.SetClothes(8, Game.Data.Clothes.GetNudeDrawable(Types.Top, pData.Sex), 0);
                        player.SetClothes(3, Game.Data.Clothes.GetNudeDrawable(Types.Gloves, pData.Sex), 0);

                        pData.Accessories[7]?.Wear(player);
                    }
                    else
                    {
                        player.SetClothes(slot, Game.Data.Clothes.GetNudeDrawable(Types.Under, pData.Sex), 0);

                        pData.Clothes[1].Wear(player);
                    }
                }
                else if (data.ItemType == Types.Gloves)
                {
                    if (player.GetClothesDrawable(11) == Game.Data.Clothes.GetNudeDrawable(Types.Top, pData.Sex))
                        player.SetClothes(slot, Game.Data.Clothes.GetNudeDrawable(Types.Gloves, pData.Sex), 0);

                    if (pData.Clothes[1] != null)
                        pData.Clothes[1].Wear(player);
                    else
                        pData.Clothes[2]?.Wear(player);
                }
                else
                {
                    player.SetClothes(slot, Game.Data.Clothes.GetNudeDrawable(Type, pData.Sex), 0);
                }
            }
            else
            {
                player.ClearAccessory(slot);

                if (data.ItemType == Types.Hat)
                    pData.Hat = null;
            }
        }
        #endregion

        #region Action

        public void Action(Player player)
        {
            if (Type == Types.Hat)
            {
                var hData = Data as Game.Data.Clothes.Hat;

                if (hData.ExtraData == null)
                    return;

                Toggled = !Toggled;

                Wear(player);
            }
            else if (Type == Types.Top)
            {
                var tData = Data as Game.Data.Clothes.Top;

                if (tData.ExtraData == null)
                    return;

                Toggled = !Toggled;

                Wear(player);
            }
            else if (Type == Types.Under)
            {
                var uData = Data as Game.Data.Clothes.Under;

                if (uData.ExtraData == null)
                    return;

                Toggled = !Toggled;

                Wear(player);
            }

            return;
        }

        #endregion

        /// <summary>Вариация одежды</summary>
        public int Var { get; set; }

        [JsonIgnore]
        /// <summary>Переключено ли состояние одежды (если поддерживается)</summary>
        public bool Toggled { get; set; }

        public Clothes(string ID, int Variation)
        {
            this.ID = ID;

            this.Var = Variation;
            this.Toggled = false;

            this.Data = Game.Data.Clothes.GetData(ID);
            this.SexAlternativeData = Game.Data.Clothes.GetSexAlternative(ID);

            this.Type = Data.ItemType;

            base.Data = Item.GetData(Type);
        }
    }
    #endregion

    #region BodyArmour
    public class BodyArmour : Item, IWearable, IActionable
    {
        public class ItemData
        {
            /// <summary>ID текстуры разных цветов</summary>
            /// <remarks>Работает только для Drawable 28!</remarks>
            public enum Colours
            {
                Green = 0,
                Orange = 1,
                Purple = 2,
                Pink = 3,
                Red = 4,
                Blue = 5,
                Grey = 6,
                LightGrey = 7,
                White = 8,
                Black = 9
            }

            public Types ItemType { get; set; }
            public int MaxStrength { get; set; }
            public Dictionary<bool, (int Drawable, int DrawableTop)> Drawables { get; set; }
            public int Texture { get; set; }

            public ItemData(Types ItemType, int MaxStrength, int DrawableMale, int DrawableMaleTop, int DrawableFemale, int DrawableFemaleTop, int Texture)
            {
                this.ItemType = ItemType;

                this.MaxStrength = MaxStrength;

                this.Drawables = new Dictionary<bool, (int Drawable, int DrawableTop)>()
                {
                    { true, (DrawableMale, DrawableMaleTop) },
                    { false, (DrawableFemale, DrawableFemaleTop) },
                };

                this.Texture = Texture;
            }

            public ItemData(Types ItemType, int MaxStrength, int DrawableMale, int DrawableMaleTop, int DrawableFemale, int DrawableFemaleTop, Colours Colour) : this(ItemType, MaxStrength, DrawableMale, DrawableMaleTop, DrawableFemale, DrawableFemaleTop, (int)Colour) { }
        }

        public static Dictionary<string, ItemData> IDList;

        [JsonIgnore]
        public ItemData Data { get; set; }

        public int Strength { get; set; }

        /// <summary>Метод для надевания брони на игрока</summary>
        /// <param name="player">Сущность игрока</param>
        public void Wear(Player player)
        {
            if (player?.Exists != true)
                return;

            var data = Data;

            var pData = player.GetMainData();

            if (pData == null)
                return;

            player.SetClothes(9, pData.Clothes[1] == null ? data.Drawables[player.GetSex()].Drawable : data.Drawables[player.GetSex()].DrawableTop, data.Texture);
            player.SetArmour(Strength);
        }

        /// <summary>Метод для снятия брони с игрока</summary>
        /// <param name="player">Сущность игрока</param>
        public void Unwear(Player player)
        {
            if (player?.Exists != true)
                return;

            var value = player.Armor;

            if (value < Strength)
            {
                Strength = value;

                this.Update();
            }

            player.SetClothes(9, 0, 0);
            player.SetArmour(0);
        }

        /// <summary>Метод для обновления прочности брони</summary>
        /// <param name="player">Сущность игрока</param>
        public void UpdateStrength(Player player)
        {
            if (player?.Exists != true)
                return;

            var value = player.Armor;

            if (value < Strength)
            {
                Strength = value;

                this.Update();
            }
        }
        public BodyArmour(string ID)
        {
            this.ID = ID;
            this.Strength = 100;

            this.Data = IDList[ID];
            this.Type = Data.ItemType;

            base.Data = Item.GetData(Type);
        }
    }
    #endregion

    #region Bag
    public class Bag : Item, IWearable, IContainer, IActionable
    {
        public class ItemData
        {
            /// <summary>Тип предмета</summary>
            public static Types ItemType = Types.Bag;

            public Dictionary<bool, int> Drawables;

            /// <summary>Текстуры</summary>
            public int[] Textures { get; set; }

            /// <summary>Максимальное кол-во слотов</summary>
            public byte MaxSlots { get; set; }
            /// <summary>Максимальный вес содержимого</summary>
            public float MaxWeight { get; set; }

            public ItemData(int DrawableMale, int DrawableFemale, int[] Textures, byte MaxSlots, float MaxWeight)
            {
                this.Drawables = new Dictionary<bool, int>()
                {
                    { true, DrawableMale },
                    { false, DrawableFemale },
                };

                this.Textures = Textures;

                this.MaxSlots = MaxSlots;
                this.MaxWeight = MaxWeight;
            }
        }

        public static Dictionary<string, ItemData> IDList;

        /// <summary>Данные предмета</summary>
        [JsonIgnore]
        public ItemData Data { get; set; }
        /// <summary>Предметы внутри</summary>
        [JsonIgnore]
        public Item[] Items { get; set; }

        public void Wear(Player player)
        {
            if (player == null)
                return;

            var pData = player.GetMainData();

            if (pData == null || pData.BeltOn)
                return;

            var iData = Data;

            player.SetClothes(5, iData.Drawables[player.GetSex()], iData.Textures[Var]);
        }

        public void Unwear(Player player)
        {
            if (player == null)
                return;

            var pData = player.GetMainData();

            if (pData == null || pData.BeltOn)
                return;

            player.SetClothes(5, 0, 0);
        }

        /// <summary>Итоговый вес</summary>
        /// <remarks>Включает в себя вес самой сумки!</remarks>
        [JsonIgnore]
        public float Weight { get => base.Weight + Items.Sum(x => (x == null ? 0 : (x is Weapon ? (x as Weapon).Weight : (x is IContainer ? (x as IContainer).Weight : (x is IStackable ? (x as IStackable).Weight : x.Weight))))); }

        public int Var { get; set; }

        public Bag(string ID, int Variation = 0)
        {
            this.ID = ID;
            this.Var = Variation;

            this.Data = IDList[ID];

            this.Items = new Item[Data.MaxSlots];

            this.Type = ItemData.ItemType;

            base.Data = Item.GetData(Type);
        }
    }
    #endregion

    #region Holster
    public class Holster : Item, IWearable, IContainer, IActionable
    {
        public class ItemData
        {
            public static Types ItemType => Types.Holster;

            public Dictionary<bool, (int Drawable, int DrawableWeapon)> Drawables;

            public int[] Textures { get; set; }

            public ItemData(int DrawableMale, int DrawableWeaponMale, int DrawableFemale, int DrawableWeaponFemale, int[] Textures)
            {
                this.Drawables = new Dictionary<bool, (int Drawable, int DrawableWeapon)>()
                {
                    { true, (DrawableMale, DrawableWeaponMale) },
                    { false, (DrawableFemale, DrawableWeaponFemale) },
                };

                this.Textures = Textures;
            }
        }

        public static Dictionary<string, ItemData> IDList;

        [JsonIgnore]
        public ItemData Data { get; set; }
        [JsonIgnore]
        public Item[] Items { get; set; }

        public void Wear(Player player)
        {
            if (player?.Exists != true)
                return;

            var iData = Data;
            var drawables = iData.Drawables[player.GetSex()];

            player.SetClothes(10, Items[0] == null ? drawables.Drawable : drawables.DrawableWeapon, iData.Textures[Var]);
        }

        public void Unwear(Player player)
        {
            if (player?.Exists != true)
                return;

            player.SetClothes(10, 0, 0);
        }

        public void WearWeapon(Player player)
        {
            if (player?.Exists != true)
                return;

            var iData = Data;
            var drawables = iData.Drawables[player.GetSex()];

            player.SetClothes(10, drawables.DrawableWeapon, iData.Textures[Var]);
        }

        public void UnwearWeapon(Player player)
        {
            if (player?.Exists != true)
                return;

            var iData = Data;
            var drawables = iData.Drawables[player.GetSex()];

            player.SetClothes(10, drawables.Drawable, iData.Textures[Var]);
        }

        [JsonIgnore]
        public float Weight { get => (this as Item).Weight + ((Items[0] as Weapon)?.Weight ?? 0); }

        public int Var { get; set; }

        public Holster(string ID, int Variation = 0)
        {
            this.ID = ID;
            this.Var = Variation;

            this.Items = new Item[1];

            this.Data = IDList[ID];
            this.Type = ItemData.ItemType;

            base.Data = Item.GetData(Type);
        }
    }
    #endregion

    #region Numberplate
    public class Numberplate : Item, ITagged
    {
        public class ItemData
        {
            public Types ItemType { get; set; }
            public int Number { get; set; }

            public ItemData(Types ItemType, int Number)
            {
                this.ItemType = ItemType;
                this.Number = Number;
            }
        }

        public static Dictionary<string, ItemData> IDList { get; set; }

        [JsonIgnore]
        public ItemData Data { get; set; }

        public string Tag { get; set; }

        public void Setup(Vehicle veh)
        {
            veh.NumberPlateStyle = Data.Number;
            veh.NumberPlate = Tag;
        }

        public void Take(Vehicle veh)
        {
            veh.NumberPlateStyle = 0;
            veh.NumberPlate = "";
        }

        private static char[] Chars = new char[]
        {
            'Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P', 'A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'Z', 'X', 'C', 'V', 'B', 'N', 'M',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
        };

        public string GenerateTag()
        {
            Random rand = new Random();
            StringBuilder str = new StringBuilder();

            do
            {
                str.Clear();

                for (int i = 0; i < 9; i++)
                    str.Append(Chars[rand.Next(0, Chars.Length - 1)]);
            }
            while (ServerEvents.UsedNumberplates.Contains(str.ToString()));

            var retStr = str.ToString();

            ServerEvents.UsedNumberplates.Add(retStr);

            return retStr;
        }

        public Numberplate(string ID)
        {
            this.ID = ID;

            this.Data = IDList[ID];
            this.Type = Data.ItemType;

            base.Data = Item.GetData(Type);

            this.Tag = "";
        }
    }
    #endregion

    #region Vehicle Key
    public class VehicleKey : Item, ITagged
    {
        public static Dictionary<string, Types> IDList { get; set; }

        public void Setup(Vehicle veh)
        {

        }

        public void Take(Vehicle veh)
        {

        }

        public string GenerateTag()
        {
            return null;
        }

        public bool IsKeyValid(Vehicle veh)
        {
            var vData = veh.GetMainData();

            if (vData == null)
                return false;

            if (vData.Keys.Contains(UID))
                return true;

            return false;
        }

        public string Tag { get; set; }
        public int VID { get; set; }

        public VehicleKey(string ID)
        {
            this.ID = ID;
            this.Type = IDList[ID];

            base.Data = Item.GetData(Type);

            this.Tag = "";

            this.VID = -1;
        }
    }
    #endregion

    public class StatusChanger : Item, IStackable, IActionable
    {
        public class ItemData
        {
            public enum SubTypes
            {
                Food = 0,
                Drink,
            }
            public Types ItemType { get; set; }

            public int Satiety { get; set; }

            public int Mood { get; set; }

            public int EffectTime { get; set; }

            public Sync.Animations.FastTypes Animation { get; set; }

            public Sync.AttachSystem.Types? AttachType { get; set; }

            public int AttachTime { get; set; }

            public int AttachModelIdx { get; set; }

            public ItemData(Types ItemType, int Satiety = 0, int Mood = 0, int EffectTime = 0, Sync.Animations.FastTypes Animation = Sync.Animations.FastTypes.None, Sync.AttachSystem.Types? AttachType = null, int AttachTime = -1, int AttachModelIdx = 0)
            {
                this.ItemType = ItemType;

                this.Satiety = Satiety;
                this.Mood = Mood;

                this.EffectTime = EffectTime;

                this.Animation = Animation;

                this.AttachType = AttachType;
                this.AttachTime = AttachTime;

                this.AttachModelIdx = AttachModelIdx;
            }
        }

        public static Dictionary<string, ItemData> IDList;

        [JsonIgnore]
        public ItemData Data { get; set; }

        [JsonIgnore]
        public float Weight { get => Amount * base.Weight; }

        [JsonIgnore]
        public int MaxAmount => 999;

        public int Amount { get; set; }

        public void Apply(PlayerData pData)
        {
            var player = pData.Player;

            if (player?.Exists != true)
                return;

            if (Data.Satiety > 0)
            {
                var satietyDiff = Utils.GetCorrectDiff(pData.Satiety, Data.Satiety, 0, 100);

                if (satietyDiff != 0)
                {
                    pData.Satiety += satietyDiff;
                }
            }

            if (Data.Mood > 0)
            {
                var moodDiff = Utils.GetCorrectDiff(pData.Mood, Data.Mood, 0, 100);

                if (moodDiff != 0)
                {
                    pData.Mood += moodDiff;
                }
            }

            if (Data.AttachType != null)
            {
                player.AttachObject(base.Data.GetModelAt(Data.AttachModelIdx), (Sync.AttachSystem.Types)Data.AttachType, Data.AttachTime);
            }

            if (Data.Animation != Sync.Animations.FastTypes.None)
            {
                pData.PlayAnim(Data.Animation);
            }
        }

        public StatusChanger(string ID)
        {
            this.ID = ID;

            this.Data = IDList[ID];

            this.Type = Data.ItemType;

            base.Data = Item.GetData(Type);
        }
    }

    public class Items : Script
    {
        #region Give
        /// <summary>Создать и выдать предмет игроку</summary>
        /// <param name="player">Сущность игрока, которому необходимо выдать предмет</param>
        /// <inheritdoc cref="CreateItem(string, int, int, bool)"/>
        public static async Task<Item> GiveItem(PlayerData pData, string id, int variation = 0, int amount = 1, bool isTemp = false)
        {
            if (pData == null)
                return null;

            var player = pData.Player;

            var item = await Task.Run(async () =>
            {
                var type = GetClass(id);

                if (type == null)
                    return null;

                var interfaces = type.GetInterfaces();

                var inv = pData.Items;

                var totalWeight = 0f;
                var totalFreeSlots = 0;

                bool stackable = interfaces.Contains(typeof(IStackable));
                bool weapon = type == typeof(Weapon);

                var iType = GetType(id);

                if (iType == Item.Types.NotAssigned)
                    return null;

                if (!weapon && amount == 0)
                    return null;

                foreach (var x in inv)
                    if (x != null)
                        totalWeight += GetItemWeight(x, true);
                    else
                        totalFreeSlots += 1;

                bool weightOk = false;

                if (weapon)
                {
                    var ammoType = Weapon.IDList[id].AmmoType;

                    weightOk = totalWeight + Item.GetWeight(iType) + (ammoType == null ? 0 : Item.GetWeight((Item.Types)ammoType) * amount) < Settings.MAX_INVENTORY_WEIGHT;
                }
                else
                    weightOk = totalWeight + Item.GetWeight(iType) * amount < Settings.MAX_INVENTORY_WEIGHT;

                if (totalFreeSlots <= 0 || !weightOk)
                {
                    NAPI.Task.Run(() =>
                    {
                        if (player?.Exists != true)
                            return;

                        player.Notify("Inventory::NoSpace");
                    });

                    return null;

                }

                var item = await CreateItem(id, variation, amount, isTemp);

                if (item == null)
                    return null;

                var freeIdx = -1;

                for (int i = 0; i < pData.Items.Length; i++)
                    if (pData.Items[i] == null)
                    {
                        freeIdx = i;

                        pData.Items[i] = item;

                        break;
                    }

                amount = (item as Game.Items.IStackable)?.Amount ?? 1;

                await NAPI.Task.RunAsync(() =>
                {
                    if (player?.Exists != true)
                        return;

                    player.TriggerEvent("Item::Added", item.ID, amount);
                    player.TriggerEvent("Inventory::Update", 0, freeIdx, (item.ID, amount, GetItemWeight(item, false)).SerializeToJson());
                });

                MySQL.UpdatePlayerInventory(pData, true);

                return item;
            });

            return item;
        }
        #endregion

        #region Create
        /// <summary>Метод для создания нового предмета</summary>
        /// <param name="id">ID предмета (см. Game.Items.Item.LoadAll</param>
        /// <param name="variation">Вариация предмета (только для IWearable, в противном случае - игнорируется)</param>
        /// <param name="amount">Кол-во предмета (только для IStakable и Weapon, в противном случае - игнорируется)</param>
        /// <param name="isTemp">Временный ли предмет?<br/>Такой предмет не будет сохраняться в базу данных и его будет нельзя: <br/><br/>1) Разделять (если IStackable или Weapon)<br/>2) Перемещать в IContainer и Game.Items.Container<br/>3) Выбрасывать (предмет удалится, но не появится на земле)<br/>4) Передавать другим игрокам</param>
        /// <returns>Объект класса Item, если предмет был создан, null - в противном случае</returns>
        public static async Task<Item> CreateItem(string id, int variation = 0, int amount = 1, bool isTemp = false)
        {
            return await Task.Run(async () =>
            {
                Type type = GetClass(id);

                if (type?.BaseType != typeof(Item))
                    return null;

                var interfaces = type.GetInterfaces();

                bool stackable = interfaces.Contains(typeof(IStackable));
                bool weapon = type == typeof(Weapon);

                if (!weapon && amount == 0)
                    return null;

                if (!stackable && !weapon)
                    amount = 1;

                Item item = null;

                if (type == typeof(Clothes))
                {
                    var textures = Data.Clothes.GetData(id).Textures.Length - 1;

                    if (textures < variation || variation < 0)
                        variation = 0;

                    item = (Clothes)Activator.CreateInstance(type, id, variation);
                }
                else if (type == typeof(Holster))
                {
                    var textures = Holster.IDList[id].Textures.Length - 1;

                    if (textures < variation || variation < 0)
                        variation = 0;

                    item = (Holster)Activator.CreateInstance(type, id, variation);
                }
                else if (type == typeof(Bag))
                {
                    var textures = Bag.IDList[id].Textures.Length - 1;

                    if (textures < variation || variation < 0)
                        variation = 0;

                    item = (Bag)Activator.CreateInstance(type, id, variation);
                }
                else
                    item = (Item)Activator.CreateInstance(type, id);

                if (stackable)
                {
                    var maxAmount = (item as IStackable).MaxAmount;

                    (item as IStackable).Amount = amount > maxAmount ? maxAmount : amount;
                }            
                else if (weapon)
                {
                    var maxAmount = (item as Weapon).Data.MaxAmmo;

                    (item as Weapon).Ammo = amount > maxAmount ? maxAmount : amount;
                }

                /*            if (!temp && interfaces.Contains(typeof(ITagged)))
                                (item as ITagged).Tag = (item as ITagged).GenerateTag();*/

                if (!isTemp)
                    return MySQL.AddNewItem(item);
                else
                {
                    item.UID = 0;
                    return item;
                }
            });
        }
        #endregion

        #region Stuff
        public static float GetItemWeight(Item item, bool calculateStackable = false)
        {
            if (item is Game.Items.IContainer)
                return (item as Game.Items.IContainer).Weight;

            if (calculateStackable && item is Game.Items.IStackable)
                return (item as Game.Items.IStackable).Weight;

            if (item is Game.Items.Weapon)
                return (item as Game.Items.Weapon).Weight;

            return item.Weight;
        }

        public static int GetItemAmount(Game.Items.Item item)
        {
            if (item is Game.Items.IStackable)
                return (item as Game.Items.IStackable).Amount;

            return 1;
        }

        public static float GetWeight(Game.Items.Item[] items) => items.Sum(x => x == null ? 0 : GetItemWeight(x, true));

        public static Item.Types GetType(string id)
        {
            var data = id.Split('_');

            switch (data[0])
            {
                case "w":
                    return Weapon.IDList.ContainsKey(id) ? Weapon.IDList[id].ItemType : Item.Types.NotAssigned;

                case "am":
                    return Ammo.IDList.ContainsKey(id) ? Ammo.IDList[id] : Item.Types.NotAssigned;

                case "top":
                case "under":
                case "hat":
                case "pants":
                case "shoes":
                case "accs":
                case "watches":
                case "glasses":
                case "bracelet":
                case "gloves":
                case "ears":
                    return Game.Data.Clothes.GetData(id)?.ItemType ?? Item.Types.NotAssigned;

                case "bag":
                    return Bag.IDList.ContainsKey(id) ? Item.Types.Bag : Item.Types.NotAssigned;

                case "arm":
                    return BodyArmour.IDList.ContainsKey(id) ? BodyArmour.IDList[id].ItemType : Item.Types.NotAssigned;

                case "holster":
                    return Holster.IDList.ContainsKey(id) ? Holster.ItemData.ItemType : Item.Types.NotAssigned;

                case "np":
                    return Numberplate.IDList.ContainsKey(id) ? Numberplate.IDList[id].ItemType : Item.Types.NotAssigned;

                case "vehkey":
                    return VehicleKey.IDList.ContainsKey(id) ? VehicleKey.IDList[id] : Item.Types.NotAssigned;

                case "sc":
                    return StatusChanger.IDList.ContainsKey(id) ? StatusChanger.IDList[id].ItemType : Item.Types.NotAssigned;

                default:
                    return Item.Types.NotAssigned;
            }
        }

        public static Type GetClass(string id)
        {
            var data = id.Split('_');

            switch (data[0])
            {
                case "w":
                    return Weapon.IDList.ContainsKey(id) ? typeof(Weapon) : null;

                case "am":
                    return Ammo.IDList.ContainsKey(id) ? typeof(Ammo) : null;

                case "top":
                case "under":
                case "hat":
                case "pants":
                case "shoes":
                case "accs":
                case "watches":
                case "glasses":
                case "bracelet":
                case "gloves":
                case "ears":
                    return Game.Data.Clothes.GetData(id) != null ? typeof(Clothes) : null;

                case "bag":
                    return Bag.IDList.ContainsKey(id) ? typeof(Bag) : null;

                case "arm":
                    return BodyArmour.IDList.ContainsKey(id) ? typeof(BodyArmour) : null;

                case "holster":
                    return Holster.IDList.ContainsKey(id) ? typeof(Holster) : null;

                case "np":
                    return Numberplate.IDList.ContainsKey(id) ? typeof(Numberplate) : null;

                case "vehkey":
                    return VehicleKey.IDList.ContainsKey(id) ? typeof(VehicleKey) : null;

                case "sc":
                    return StatusChanger.IDList.ContainsKey(id) ? typeof(StatusChanger) : null;

                default:
                    return null;
            }
        }
        #endregion

        public static int LoadAll()
        {
            int counter = 0;

            Game.Data.Clothes.LoadAll();

            #region All Bags
            Bag.IDList = new Dictionary<string, Bag.ItemData>()
            {
                { "bag_0", new Bag.ItemData(81, 81, new int[] { 0 }, 10, 5f) }
            };
            #endregion

            #region All Holsters
            Holster.IDList = new Dictionary<string, Holster.ItemData>()
            {
                { "holster_0", new Holster.ItemData(136, 134, 0, 0, new int[] { 0, 1 }) },
                { "holster_1", new Holster.ItemData(135, 137, 0, 0, new int[] { 0, 1 }) },
            };
            #endregion

            #region All BodyArmours
            BodyArmour.IDList = new Dictionary<string, BodyArmour.ItemData>()
            {
                { "arm_shop", new BodyArmour.ItemData(Item.Types.BArmShop, 100, 28, 19, 0, 0, BodyArmour.ItemData.Colours.Grey) }
            };
            #endregion

            #region All Weapons
            Weapon.IDList = new Dictionary<string, Weapon.ItemData>()
            {
                { "w_asrifle", new Weapon.ItemData(Item.Types.AssaultRifle, Weapon.ItemData.TopTypes.AssaultRifle, Item.Types.Ammo7_62, WeaponHash.Assaultrifle, 30, false) },
                { "w_asrifle_mk2", new Weapon.ItemData(Item.Types.AssaultRifleMk2, Weapon.ItemData.TopTypes.AssaultRifle, Item.Types.Ammo7_62, WeaponHash.Assaultrifle_mk2, 35, false) },
                { "w_advrifle", new Weapon.ItemData(Item.Types.AdvancedRifle, Weapon.ItemData.TopTypes.AssaultRifle, Item.Types.Ammo7_62, WeaponHash.Advancedrifle, 30, false) },
                { "w_carbrifle", new Weapon.ItemData(Item.Types.CarbineRifle, Weapon.ItemData.TopTypes.AssaultRifle, Item.Types.Ammo7_62, WeaponHash.Carbinerifle, 30, false) },
                { "w_comprifle", new Weapon.ItemData(Item.Types.CompactRifle, Weapon.ItemData.TopTypes.AssaultRifle, Item.Types.Ammo7_62, WeaponHash.Compactrifle, 30, false) },
                { "w_heavyrifle", new Weapon.ItemData(Item.Types.HeavyRifle, Weapon.ItemData.TopTypes.AssaultRifle, Item.Types.Ammo7_62, 0xC78D71B4, 30, false) },

                { "w_microsmg", new Weapon.ItemData(Item.Types.MicroSMG, Weapon.ItemData.TopTypes.SubMachine, Item.Types.Ammo5_56, WeaponHash.Microsmg, 15, true) },
                { "w_minismg", new Weapon.ItemData(Item.Types.MiniSMG, Weapon.ItemData.TopTypes.SubMachine, Item.Types.Ammo5_56, WeaponHash.Minismg, 15, true) },
                { "w_smg", new Weapon.ItemData(Item.Types.SMG, Weapon.ItemData.TopTypes.SubMachine, Item.Types.Ammo5_56, WeaponHash.Smg, 15, true) },
                { "w_smg_mk2", new Weapon.ItemData(Item.Types.SMGMk2, Weapon.ItemData.TopTypes.SubMachine, Item.Types.Ammo5_56, WeaponHash.Smg_mk2, 15, true) },
                { "w_asmsg", new Weapon.ItemData(Item.Types.AssaultSMG, Weapon.ItemData.TopTypes.SubMachine, Item.Types.Ammo5_56, WeaponHash.Assaultsmg, 15, false) },
                { "w_combpdw", new Weapon.ItemData(Item.Types.CombatPDW, Weapon.ItemData.TopTypes.SubMachine, Item.Types.Ammo5_56, WeaponHash.Combatpdw, 15, false) },

                { "w_combmg", new Weapon.ItemData(Item.Types.CombatMG, Weapon.ItemData.TopTypes.LightMachine, Item.Types.Ammo9, WeaponHash.Combatmg, 15, false) },
                { "w_gusenberg", new Weapon.ItemData(Item.Types.Gusenberg, Weapon.ItemData.TopTypes.LightMachine, Item.Types.Ammo9, WeaponHash.Gusenberg, 15, false) },

                { "w_heavysnp", new Weapon.ItemData(Item.Types.Gusenberg, Weapon.ItemData.TopTypes.SniperRifle, Item.Types.Ammo12_7, WeaponHash.Heavysniper, 15, false) },
                { "w_markrifle", new Weapon.ItemData(Item.Types.MarksmanRifle, Weapon.ItemData.TopTypes.SniperRifle, Item.Types.Ammo12_7, WeaponHash.Marksmanrifle, 15, false) },
                { "w_musket", new Weapon.ItemData(Item.Types.Musket, Weapon.ItemData.TopTypes.SniperRifle, Item.Types.Ammo12_7, WeaponHash.Musket, 15, false) },

                { "w_assgun", new Weapon.ItemData(Item.Types.AssaultShotgun, Weapon.ItemData.TopTypes.Shotgun, Item.Types.Ammo12, WeaponHash.Assaultshotgun, 15, false) },
                { "w_heavysgun", new Weapon.ItemData(Item.Types.HeavyShotgun, Weapon.ItemData.TopTypes.Shotgun, Item.Types.Ammo12, WeaponHash.Heavyshotgun, 15, false) },
                { "w_pumpsgun", new Weapon.ItemData(Item.Types.PumpShotgun, Weapon.ItemData.TopTypes.Shotgun, Item.Types.Ammo12, WeaponHash.Pumpshotgun, 15, false) },
                { "w_pumpsgun_mk2", new Weapon.ItemData(Item.Types.PumpShotgunMk2, Weapon.ItemData.TopTypes.Shotgun, Item.Types.Ammo12, WeaponHash.Pumpshotgun_mk2, 15, false) },
                { "w_sawnsgun", new Weapon.ItemData(Item.Types.SawnOffShotgun, Weapon.ItemData.TopTypes.Shotgun, Item.Types.Ammo12, WeaponHash.Sawnoffshotgun, 15, false) },

                { "w_pistol", new Weapon.ItemData(Item.Types.Pistol, Weapon.ItemData.TopTypes.HandGun, Item.Types.Ammo5_56, WeaponHash.Pistol, 15, true) },
                { "w_pistol_mk2", new Weapon.ItemData(Item.Types.PistolMk2, Weapon.ItemData.TopTypes.HandGun, Item.Types.Ammo5_56, WeaponHash.Pistol_mk2, 20, true) },
                { "w_appistol", new Weapon.ItemData(Item.Types.APPistol, Weapon.ItemData.TopTypes.HandGun, Item.Types.Ammo5_56, WeaponHash.Appistol, 20, true) },
                { "w_combpistol", new Weapon.ItemData(Item.Types.CombatPistol, Weapon.ItemData.TopTypes.HandGun, Item.Types.Ammo5_56, WeaponHash.Combatpistol, 20, true) },
                { "w_heavypistol", new Weapon.ItemData(Item.Types.HeavyPistol, Weapon.ItemData.TopTypes.HandGun, Item.Types.Ammo5_56, WeaponHash.Heavypistol, 20, true) },
                { "w_machpistol", new Weapon.ItemData(Item.Types.MachinePistol, Weapon.ItemData.TopTypes.HandGun, Item.Types.Ammo5_56, WeaponHash.Machinepistol, 20, true) },
                { "w_markpistol", new Weapon.ItemData(Item.Types.MarksmanPistol, Weapon.ItemData.TopTypes.HandGun, Item.Types.Ammo5_56, WeaponHash.Marksmanpistol, 20, true) },
                { "w_vintpistol", new Weapon.ItemData(Item.Types.VintagePistol, Weapon.ItemData.TopTypes.HandGun, Item.Types.Ammo5_56, WeaponHash.Vintagepistol, 20, true) },

                { "w_revolver", new Weapon.ItemData(Item.Types.Revolver, Weapon.ItemData.TopTypes.HandGun, Item.Types.Ammo11_43, WeaponHash.Revolver, 20, true) },
                { "w_revolver_mk2", new Weapon.ItemData(Item.Types.RevolverMk2, Weapon.ItemData.TopTypes.HandGun, Item.Types.Ammo11_43, WeaponHash.Revolver_mk2, 20, true) },

                { "w_bat", new Weapon.ItemData(Item.Types.Bat, Weapon.ItemData.TopTypes.Melee, null, WeaponHash.Bat, 0, false) },
                { "w_bottle", new Weapon.ItemData(Item.Types.Bottle, Weapon.ItemData.TopTypes.Melee, null, WeaponHash.Bottle, 0, false) },
                { "w_crowbar", new Weapon.ItemData(Item.Types.Crowbar, Weapon.ItemData.TopTypes.Melee, null, WeaponHash.Crowbar, 0, false) },
                { "w_dagger", new Weapon.ItemData(Item.Types.Dagger, Weapon.ItemData.TopTypes.Melee, null, WeaponHash.Dagger, 0, false) },
                { "w_flashlight", new Weapon.ItemData(Item.Types.Flashlight, Weapon.ItemData.TopTypes.Melee, null, WeaponHash.Flashlight, 0, false) },
                { "w_golfclub", new Weapon.ItemData(Item.Types.GolfClub, Weapon.ItemData.TopTypes.Melee, null, WeaponHash.Golfclub, 0, false) },
                { "w_hammer", new Weapon.ItemData(Item.Types.Hammer, Weapon.ItemData.TopTypes.Melee, null, WeaponHash.Hammer, 0, false) },
                { "w_hatchet", new Weapon.ItemData(Item.Types.Hatchet, Weapon.ItemData.TopTypes.Melee, null, WeaponHash.Hatchet, 0, false) },
                { "w_knuckles", new Weapon.ItemData(Item.Types.Knuckles, Weapon.ItemData.TopTypes.Melee, null, WeaponHash.Knuckle, 0, false) },
                { "w_machete", new Weapon.ItemData(Item.Types.Machete, Weapon.ItemData.TopTypes.Melee, null, WeaponHash.Machete, 0, false) },
                { "w_nightstick", new Weapon.ItemData(Item.Types.Nightstick, Weapon.ItemData.TopTypes.Melee, null, WeaponHash.Nightstick, 0, false) },
                { "w_poolcue", new Weapon.ItemData(Item.Types.PoolCue, Weapon.ItemData.TopTypes.Melee, null, WeaponHash.Poolcue, 0, false) },
                { "w_switchblade", new Weapon.ItemData(Item.Types.SwitchBlade, Weapon.ItemData.TopTypes.Melee, null, WeaponHash.Switchblade, 0, false) },
                { "w_wrench", new Weapon.ItemData(Item.Types.Wrench, Weapon.ItemData.TopTypes.Melee, null, WeaponHash.Wrench, 0, false) },
            };
            #endregion

            #region All Ammos
            Ammo.IDList = new Dictionary<string, Item.Types>()
            {
                { "am_5.56", Item.Types.Ammo5_56 },
                { "am_7.62", Item.Types.Ammo7_62 },
                { "am_9", Item.Types.Ammo9 },
                { "am_11.43", Item.Types.Ammo11_43 },
                { "am_12", Item.Types.Ammo12 },
                { "am_12.7", Item.Types.Ammo12_7 },
            };
            #endregion

            #region All Numberplates
            Numberplate.IDList = new Dictionary<string, Numberplate.ItemData>()
            {
                { "np_0", new Numberplate.ItemData(Item.Types.Numberplate0, 0) },
                { "np_1", new Numberplate.ItemData(Item.Types.Numberplate1, 1) },
                { "np_2", new Numberplate.ItemData(Item.Types.Numberplate2, 2) },
                { "np_3", new Numberplate.ItemData(Item.Types.Numberplate3, 3) },
                { "np_4", new Numberplate.ItemData(Item.Types.Numberplate4, 4) },
                { "np_5", new Numberplate.ItemData(Item.Types.Numberplate5, 5) },
            };
            #endregion

            #region Other Stuff
            VehicleKey.IDList = new Dictionary<string, Item.Types>()
            {
                { "vehkey", Item.Types.VehKey },
            };
            #endregion

            StatusChanger.IDList = new Dictionary<string, StatusChanger.ItemData>()
            {
                { "sc_burger", new StatusChanger.ItemData(Item.Types.Burger, 25, 0, 0, Sync.Animations.FastTypes.ItemBurger, Sync.AttachSystem.Types.ItemBurger, 6000) },
                { "sc_chips", new StatusChanger.ItemData(Item.Types.Chips, 15, 0, 0, Sync.Animations.FastTypes.ItemChips, Sync.AttachSystem.Types.ItemChips, 6000) },
                { "sc_pizza", new StatusChanger.ItemData(Item.Types.Pizza, 50, 15, 0, Sync.Animations.FastTypes.ItemPizza, Sync.AttachSystem.Types.ItemPizza, 6000) },
                { "sc_chocolate", new StatusChanger.ItemData(Item.Types.Chocolate, 10, 20, 0, Sync.Animations.FastTypes.ItemChocolate, Sync.AttachSystem.Types.ItemChocolate, 6000) },
                { "sc_hotdog", new StatusChanger.ItemData(Item.Types.Hotdog, 10, 20, 0, Sync.Animations.FastTypes.ItemChocolate, Sync.AttachSystem.Types.ItemChocolate, 6000) },

                { "sc_cola", new StatusChanger.ItemData(Item.Types.Cola, 5, 20, 0, Sync.Animations.FastTypes.ItemCola, Sync.AttachSystem.Types.ItemCola, 6000) },

                { "sc_cigs", new StatusChanger.ItemData(Item.Types.Cigarettes, 0, 25, 0, Sync.Animations.FastTypes.None, Sync.AttachSystem.Types.ItemCigHand, -1, 2) },
                //{ "sc_joint", new StatusChanger.ItemData(Item.Types.Cigarettes, 0, 50, 0, Sync.Animations.FastTypes.None, Sync.AttachSystem.Types.ItemJoint, 6000) },

                { "sc_beer", new StatusChanger.ItemData(Item.Types.Beer, 5, 50, 0, Sync.Animations.FastTypes.ItemBeer, Sync.AttachSystem.Types.ItemBeer, 6000) },
            };

            counter += Game.Data.Clothes.AllClothes.Count;
            counter += Bag.IDList.Count;
            counter += Holster.IDList.Count;
            counter += BodyArmour.IDList.Count;
            counter += Weapon.IDList.Count;
            counter += Numberplate.IDList.Count;
            counter += Ammo.IDList.Count;

            return counter;
        }

        public Items()
        {

        }
    }

    #region Item JSON Converter

    public class BaseSpecifiedConcreteClassConverter : DefaultContractResolver
    {
        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (typeof(Item).IsAssignableFrom(objectType) && !objectType.IsAbstract)
                return null; // pretend TableSortRuleConvert is not specified (thus avoiding a stack overflow)
            return base.ResolveContractConverter(objectType);
        }
    }

    public class ItemConverter : JsonConverter
    {
        static JsonSerializerSettings SpecifiedSubclassConversion = new JsonSerializerSettings() { ContractResolver = new BaseSpecifiedConcreteClassConverter() };

        public override bool CanConvert(Type objectType) => objectType == typeof(Item);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            JObject jo = JObject.Load(reader);

            var type = Items.GetClass(jo["ID"].Value<string>());

            return JsonConvert.DeserializeObject(jo.ToString(), type, SpecifiedSubclassConversion);
        }

        public override bool CanWrite => false;
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {}
    }
    #endregion
}
