using GTANetworkAPI;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.Game.Attachments;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Sync;

namespace BlaineRP.Server.Game.Items
{
    public partial class Weapon : Item, ITagged, IWearable, IContainer
    {
        public new class ItemData : Item.ItemData
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

            private static readonly HashSet<uint> _mk2WeaponHashes = new HashSet<uint>()
            {
                0x6A6C02E0, 0xA914799, 0xDBBD7280, 0x84D6FAFD, 0x969C3D67, 0xFAD1F1C9, 0x394F415C, 0x555AF99A, 0x78A97CD0, 0xCB96392F, 0x88374054, 0xBFE256D4,
            };

            public bool IsMk2Weapon => _mk2WeaponHashes.Contains(Hash);

            /// <summary>ID подходящих патронов</summary>
            /// <value>Тип патронов, если оружие способно стрелять и заряжаться, null - в противном случае</value>
            public string AmmoId { get; set; }

            /// <summary>Высший тип оружия</summary>
            public TopTypes TopType { get; set; }

            /// <summary>Максимальное кол-во патронов в обойме</summary>
            public int MaxAmmo { get; set; }

            /// <summary>Хэш оружия</summary>
            public uint Hash { get; set; }

            /// <summary>Может ли использоваться в транспорте?</summary>
            public bool CanUseInVehicle { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {(AmmoId == null ? "null" : $"\"{AmmoId}\"")}, {MaxAmmo}, {Hash}";

            /// <summary>Создать новое оружие</summary>
            /// <param name="ItemType">Глобальный тип предмета</param>
            /// <param name="topType">Тип оружия</param>
            /// <param name="ammoId">Тип патронов (если нет - null)</param>
            /// <param name="hash">Хэш оружия</param>
            /// <param name="maxAmmo">Максимальное кол-во патронов</param>
            /// <param name="canUseInVehicle">Может ли использоваться в транспорте?</param>
            public ItemData(string name, float weight, string model, TopTypes topType, string ammoId, uint hash, int maxAmmo, bool canUseInVehicle = false) : base(name, weight, model)
            {
                TopType = topType;
                AmmoId = ammoId;

                CanUseInVehicle = canUseInVehicle;

                Hash = hash;

                MaxAmmo = maxAmmo;
            }

            /// <inheritdoc cref="ItemData.ItemData(Types, TopTypes, Types?, uint, int, bool)"/>
            public ItemData(string name, float weight, string model, TopTypes topType, string ammoType, WeaponHash hash, int maxAmmo, bool canUseInVehicle = false) : this(name, weight, model, topType, ammoType, (uint)hash, maxAmmo, canUseInVehicle) { }

            public AttachmentType? GetAttachType(PlayerData pData)
            {
                if (TopType == TopTypes.Shotgun || TopType == TopTypes.AssaultRifle || TopType == TopTypes.SniperRifle || TopType == TopTypes.HeavyWeapon)
                {
                    return pData.AttachedObjects.Where(x => x.Type == AttachmentType.WeaponLeftBack).Any() ? AttachmentType.WeaponRightBack : AttachmentType.WeaponLeftBack;
                }
                else if (TopType == TopTypes.SubMachine)
                {
                    return pData.AttachedObjects.Where(x => x.Type == AttachmentType.WeaponLeftTight).Any() ? AttachmentType.WeaponRightTight : AttachmentType.WeaponLeftTight;
                }

                return null;
            }
        }

        [JsonIgnore]
        public new ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        /// <summary>Обший вес оружия (вместе с патронами в обойме)</summary>
        [JsonIgnore]
        public override float Weight { get => Items.Sum(x => x?.Weight ?? 0f) + (Data.AmmoId == null ? BaseWeight : BaseWeight + Ammo * (Game.Items.Ammo.IdList[Data.AmmoId].Weight)); }

        public string Tag { get; set; }

        [JsonIgnore]
        public Item[] Items { get; set; }

        /// <summary>Используется ли оружие?</summary>
        [JsonIgnore]
        public bool Equiped { get; set; }

        /// <summary>Тип привязки к игроку объекта оружия</summary>
        [JsonIgnore]
        public AttachmentType? AttachType { get; set; }

        [JsonProperty(PropertyName = "A")]

        /// <summary>Кол-во патронов в обойме</summary>
        public int Ammo { get; set; }

        /// <summary>Метод для выдачи оружия игроку</summary>
        /// <param name="pData">Сущность игрока</param>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public void Equip(PlayerData pData)
        {
            if (Equiped)
                return;

            var data = Data;

            var player = pData.Player;

            Unwear(pData);

            Equiped = true;

            player.SetWeapon(data.Hash, Ammo);

            UpdateWeaponComponents(pData);
        }

        /// <summary>Метод, чтобы забрать оружие у игрока</summary>
        /// <param name="pData">Сущность игрока</param>
        /// <param name="updateLastAmmoFirst">Обновить ли кол-во патронов в обойме?</param>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public void Unequip(PlayerData pData, bool wearAfter = true)
        {
            var player = pData.Player;

            if (!Equiped)
                return;

            Update();

            player.SetWeapon((uint)WeaponHash.Unarmed);

            pData.WeaponComponents = null;

            Equiped = false;

            if (wearAfter)
                Wear(pData);
        }

        /// <summary>Метод, чтобы выдать патроны игроку</summary>
        /// <param name="pData">Сущность игрока</param>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public void UpdateAmmo(PlayerData pData)
        {
            if (!Equiped)
                return;

            var player = pData.Player;

            player.SetAmmo(Ammo);
            player.TriggerEvent("Weapon::TaskReload");
        }

        public string GenerateTag(string prefix) => $"{prefix}-{NAPI.Util.GetHashSha256(UID.ToString()).ToUpper()}";

        public void Wear(PlayerData pData)
        {
            if (AttachType != null || Equiped)
                return;

            var player = pData.Player;

            if (pData.Holster?.Items[0] == this)
            {
                pData.Holster.WearWeapon(pData);

                return;
            }

            var data = Data;

            if (data.GetAttachType(pData) is AttachmentType attachType)
            {
                if (player.AttachObject(data.Hash, attachType, -1, $"{GetCurrentSkinVariation(pData)}_{GetWeaponComponentsString()}"))
                    AttachType = attachType;
            }
        }

        public void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            if (pData.Holster != null && (pData.Holster.Items[0] == null || pData.Holster.Items[0] == this))
                pData.Holster.UnwearWeapon(pData);

            if (AttachType is AttachmentType aType)
            {
                player.DetachObject(aType);

                AttachType = null;
            }
        }

        public string GetWeaponComponentsString()
        {
            var t = Items.Where(x => x is WeaponComponent).Select(x => (int)((WeaponComponent)x).Data.Type).ToList();

            return t.Count > 0 ? string.Join('_', t) : "";
        }

        public int GetCurrentSkinVariation(PlayerData pData)
        {
            if (Data.IsMk2Weapon)
            {
                return pData.Info.WeaponSkins.Where(x => x.Data.Type == WeaponSkin.ItemData.Types.UniMk2).FirstOrDefault()?.Data.Variation ?? 0;
            }
            else
            {
                return pData.Info.WeaponSkins.Where(x => x.Data.Type == WeaponSkin.ItemData.Types.UniDef).FirstOrDefault()?.Data.Variation ?? 0;
            }
        }

        public void UpdateWeaponComponents(PlayerData pData)
        {
            if (Equiped)
            {
                pData.WeaponComponents = $"{Data.Hash}_{GetCurrentSkinVariation(pData)}_{GetWeaponComponentsString()}";
            }
            else if (AttachType is AttachmentType aType)
            {
                var atObjects = pData.AttachedObjects;

                var atObj = atObjects.Where(x => x.Type == aType).FirstOrDefault();

                if (atObj == null)
                    return;

                atObj.SyncData = $"{GetCurrentSkinVariation(pData)}_{GetWeaponComponentsString()}";

                pData.AttachedObjects = atObjects;
            }
        }

        public Weapon(string id) : base(id, IdList[id], typeof(Weapon))
        {
            Items = new Item[5];
        }
    }
}
