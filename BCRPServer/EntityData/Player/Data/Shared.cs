using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer
{
    public partial class PlayerData
    {
        /// <summary>Сытость игрока</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>От 0 до 100</value>
        public int Satiety { get => Info.LastData.Satiety; set { Player.SetOwnSharedData("Satiety", value); Info.LastData.Satiety = value; } }

        /// <summary>Настроение игрока</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>От 0 до 100</value>
        public int Mood { get => Info.LastData.Mood; set { Player.SetOwnSharedData("Mood", value); Info.LastData.Mood = value; } }

        public ulong BankBalance { get => Info.BankAccount?.Balance ?? 0; set { Player.SetOwnSharedData("BankBalance", value); if (Info.BankAccount != null) Info.BankAccount.Balance = value; } }

        /// <summary>Наличные игрока</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>Кол-во наличных средств</value>
        public ulong Cash { get => Info.Cash; set { Player.SetOwnSharedData("Cash", value); Info.Cash = value; } }

        /// <summary>Организация игрока</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>ID организации, -1 - если отсутствует</value>
        public int OrganisationID { get => Info.OrganisationID; set { Player.SetOwnSharedData("OrganisationID", value); Info.OrganisationID = value; } }

        /// <summary>Пристёгнут ли игрок?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool BeltOn { get => Player.GetOwnSharedData<bool?>("Belt::On") ?? false; set { if (value) Player.SetOwnSharedData("Belt::On", value); else Player.ResetOwnSharedData("Belt::On"); } }

        /// <summary>Ранен ли игрок?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool IsWounded { get => Player.GetOwnSharedData<bool?>("IsWounded") ?? false; set { if (value) Player.SetOwnSharedData("IsWounded", value); else Player.ResetOwnSharedData("IsWounded"); } }

        /// <summary>Ползет ли игрок?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool CrawlOn { get => Player.GetOwnSharedData<bool?>("Crawl::On") ?? false; set { if (value) Player.SetOwnSharedData("Crawl::On", value); else Player.ResetOwnSharedData("Crawl::On"); } }

        /// <summary>Текущая анимация игрока (Fast)</summary>
        /// <remarks>НЕ синхронизуется с игроками ВНЕ зоны стрима (т.к. проигрывается быстро)</remarks>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public Sync.Animations.FastTypes FastAnim { get => (Sync.Animations.FastTypes)(Player.GetData<int?>("Anim::Fast") ?? -1); set { if (value > Sync.Animations.FastTypes.None) Player.SetData("Anim::Fast", (int)value); else Player.ResetData("Anim::Fast"); } }

        /// <summary>CID игрока</summary>
        /// <remarks>Т.к. может использоваться для сохранения данных в БД, set - в основном потоке, get - в любом</remarks>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public uint CID { get => Info.CID; set { Player.SetSharedData("CID", value); Info.CID = value; } }

        /// <summary>Пол игрока</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>true - мужчина, false - женщина</value>
        public bool Sex { get => Info.Sex; set { Player.SetSharedData("Sex", value); Player.SetSkin(value ? PedHash.FreemodeMale01 : PedHash.FreemodeFemale01); Info.Sex = value; } }

        /// <summary>Фракция игрока</summary>
        /// <remarks>Также вызывает событие Players::SetFraction на клиенте игроков в зоне стрима</remarks>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public FractionTypes Fraction { get => Info.Fraction; set { Player.SetSharedData("Fraction", value); Info.Fraction = value; } }

        /// <summary>В маске ли игрок?</summary>
        public Game.Items.Mask WearedMask => Accessories[1] as Game.Items.Mask;

        /// <summary>Без сознания ли игрок?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool IsKnocked { get => Player.GetSharedData<bool?>("Knocked") ?? false; set { if (value) Player.SetSharedData("Knocked", value); else Player.ResetSharedData("Knocked"); } }

        public bool IsFrozen { get => Player.GetOwnSharedData<bool?>("IsFrozen") ?? false; set { if (value) Player.SetOwnSharedData("IsFrozen", value); else Player.ResetOwnSharedData("IsFrozen"); } }

        public bool IsCuffed { get => Player.GetOwnSharedData<bool?>("IsCuffed") ?? false; set { if (value) Player.SetOwnSharedData("IsCuffed", value); else Player.ResetOwnSharedData("IsCuffed"); } }

        public int VehicleSeat { get => Player.GetSharedData<int?>("VehicleSeat") ?? -1; set { if (value >= 0) Player.SetSharedData("VehicleSeat", value); else Player.ResetSharedData("VehicleSeat"); } }

        /// <summary>Приседает ли игрок?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool CrouchOn { get => Player.GetSharedData<bool?>("Crouch::On") ?? false; set { if (value) Player.SetSharedData("Crouch::On", value); else Player.ResetSharedData("Crouch::On"); } }

        /// <summary>Дальность микрофона игрока</summary>
        /// <remarks>Если микрофон игроком не используется: 0, если в муте: -1</remarks>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public float VoiceRange { get => Player.GetSharedData<float>("VoiceRange"); set { Player.SetSharedData("VoiceRange", value); } }

        /// <summary>В муте ли игрок?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool IsMuted { get => VoiceRange < 0f; set { Sync.Players.DisableMicrophone(this); VoiceRange = -1; } }

        /// <summary>Проблемы ли у игрока со слухом/речью?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool IsInvalid { get => Player.GetSharedData<bool?>("IsInvalid") ?? false; set { if (value) Player.SetSharedData("IsInvalid", value); else Player.ResetSharedData("IsInvalid"); } }

        /// <summary>Использует ли игрок телефон?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool PhoneOn { get => Player.GetSharedData<bool?>("Phone::On") ?? false; set { if (value) Player.SetSharedData("Phone::On", value); else Player.ResetSharedData("Phone::On"); } }

        public string WeaponComponents { get => Player.GetSharedData<string>("WCD"); set { if (value != null) Player.SetSharedData("WCD", value); else Player.ResetSharedData("WCD"); } }

        /// <summary>Уровень администратора игрока</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public int AdminLevel { get => Info.AdminLevel; set { Player.SetSharedData("AdminLevel", value); Info.AdminLevel = value; } }

        /// <summary>Текущая шапка игрока, необходимо для нормального отображения в игре при входе/выходе из транспорта</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public string Hat { get => Player.GetSharedData<string>("Hat"); set { if (value == null) Player.ResetSharedData("Hat"); else Player.SetSharedData("Hat", value); } }

        /// <summary>Является ли игрок невидимым?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool IsInvisible { get => Player.GetSharedData<bool?>("IsInvisible") ?? false; set { if (value) Player.SetSharedData("IsInvisible", value); else Player.ResetSharedData("IsInvisible"); Player.SetAlpha(value ? 0 : 255); } }

        /// <summary>Является ли игрок бессмертным?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool IsInvincible { get => Player.GetSharedData<bool?>("IsInvincible") ?? false; set { if (value) Player.SetSharedData("IsInvincible", value); else Player.ResetSharedData("IsInvincible"); Player.SetInvincible(value); } }

        /// <summary>Текущая анимация игрока (General)</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public Sync.Animations.GeneralTypes GeneralAnim { get => (Sync.Animations.GeneralTypes)(Player.GetSharedData<int?>("Anim::General") ?? -1); set { if (value > Sync.Animations.GeneralTypes.None) Player.SetSharedData("Anim::General", (int)value); else Player.ResetSharedData("Anim::General"); } }

        /// <summary>Текущая анимация игрока (Other)</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public Sync.Animations.OtherTypes OtherAnim { get => (Sync.Animations.OtherTypes)(Player.GetSharedData<int?>("Anim::Other") ?? -1); set { if (value > Sync.Animations.OtherTypes.None) Player.SetSharedData("Anim::Other", (int)value); else Player.ResetSharedData("Anim::Other"); } }

        /// <summary>Текущая походка игрока</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public Sync.Animations.WalkstyleTypes Walkstyle { get => (Sync.Animations.WalkstyleTypes)(Player.GetSharedData<int?>("Walkstyle") ?? -1); set { if (value > Sync.Animations.WalkstyleTypes.None) Player.SetSharedData("Walkstyle", (int)value); else Player.ResetSharedData("Walkstyle"); } }

        /// <summary>Текущая эмоция игрока</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public Sync.Animations.EmotionTypes Emotion { get => (Sync.Animations.EmotionTypes)(Player.GetSharedData<int?>("Emotion") ?? -1); set { if (value > Sync.Animations.EmotionTypes.None) Player.SetSharedData("Emotion", (int)value); else Player.ResetSharedData("Emotion"); } }

        /// <summary>Прикрепленные объекты к игроку</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>Список объектов класса Sync.AttachSystem.AttachmentNet</value>
        public List<Sync.AttachSystem.AttachmentObjectNet> AttachedObjects { get => Player.GetSharedData<Newtonsoft.Json.Linq.JArray>(Sync.AttachSystem.AttachedObjectsKey).ToList<Sync.AttachSystem.AttachmentObjectNet>(); set { Player.SetSharedData(Sync.AttachSystem.AttachedObjectsKey, value); } }

        /// <summary>Прикрепленные сущности к игроку</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>Список объектов класса Sync.AttachSystem.AttachmentNet</value>
        public List<Sync.AttachSystem.AttachmentEntityNet> AttachedEntities { get => Player.GetSharedData<Newtonsoft.Json.Linq.JArray>(Sync.AttachSystem.AttachedEntitiesKey).ToList<Sync.AttachSystem.AttachmentEntityNet>(); set { Player.SetSharedData(Sync.AttachSystem.AttachedEntitiesKey, value); } }

        /// <summary>Прикрепленные объекты к игроку, которые находятся в руках</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>Список объектов класса Sync.AttachSystem.AttachmentNet</value>
        public bool HasAnyHandAttachedObject => AttachedObjects.FirstOrDefault(x => Sync.AttachSystem.IsTypeObjectInHand(x.Type)) != null;
    }
}
