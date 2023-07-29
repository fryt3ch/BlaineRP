using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Game.Achievements;
using BlaineRP.Client.Game.Animations;
using BlaineRP.Client.Game.Attachments;
using BlaineRP.Client.Game.Businesses;
using BlaineRP.Client.Game.Estates;
using BlaineRP.Client.Game.Fractions;
using BlaineRP.Client.Game.Items;
using BlaineRP.Client.Game.Jobs;
using BlaineRP.Client.Game.Quests;
using BlaineRP.Client.Game.UI.CEF.Phone.Apps;
using Newtonsoft.Json.Linq;
using RAGE.Elements;

namespace BlaineRP.Client.Game.EntitiesData.Players
{
    public class PlayerData
    {
        public PlayerData(Player player)
        {
            Player = player;
        }

        public Player Player { get; private set; }

        public uint CID => Utils.Convert.ToUInt32(Player.GetSharedData<object>("CID", 0));

        public ulong Cash => Utils.Convert.ToUInt64(Player.GetSharedData<object>("Cash", 0));

        public ulong BankBalance => Utils.Convert.ToUInt64(Player.GetSharedData<object>("BankBalance", 0));

        public bool Sex => Player.GetSharedData<bool>("Sex", true);

        public FractionTypes Fraction => (FractionTypes)Player.GetSharedData<int>("Fraction", 0);

        public int Satiety => Utils.Convert.ToByte(Player.GetSharedData<object>("Satiety", 0));

        public int Mood => Utils.Convert.ToByte(Player.GetSharedData<object>("Mood", 0));

        public bool IsMasked => Player.GetDrawableVariation(1) > 0;

        public bool IsKnocked => Player.GetSharedData<bool>("Knocked", false);

        public bool CrouchOn => Player.GetSharedData<bool>("Crouch::On", false);

        public bool CrawlOn => Player.GetSharedData<bool>("Crawl::On", false);

        public string WeaponComponents => Player.GetSharedData<string>("WCD", null);

        public float VoiceRange => Player.GetSharedData<float>("VoiceRange", 0f);

        public bool IsMuted => VoiceRange < 0f;

        public bool IsCuffed => AttachedObjects?.Where(x => x.Type == AttachmentType.Cuffs || x.Type == AttachmentType.CableCuffs).Any() == true;

        public bool IsInvalid => Player.GetSharedData<bool>("IsInvalid", false);

        public bool IsJailed => Player.GetSharedData<bool>("IsJailed", false);

        public bool IsFrozen => Player.GetSharedData<bool>("IsFrozen", false);

        public string Hat => Player.GetSharedData<string>("Hat", null);

        public bool IsWounded => Player.GetSharedData<bool>("IsWounded", false);

        public bool BeltOn => Player.GetSharedData<bool>("Belt::On", false);

        public Scripts.Misc.Phone.PhoneStateTypes PhoneStateType => (Scripts.Misc.Phone.PhoneStateTypes)Player.GetSharedData<int>("PST", 0);

        public int AdminLevel => Player.GetSharedData<int>("AdminLevel", -1);

        public int VehicleSeat => Player.GetSharedData<int>("VehicleSeat", -1);

        public Vehicle AutoPilot
        {
            get => Player.GetData<Vehicle>("AutoPilot::State");
            set
            {
                if (value == null)
                    Player.ResetData("AutoPilot::State");
                else
                    Player.SetData("AutoPilot::State", value);
            }
        }

        public GeneralType GeneralAnim => (GeneralType)Player.GetSharedData<int>("Anim::General", -1);

        public FastType FastAnim
        {
            get => Player.GetData<FastType>("Anim::Fast");
            set => Player.SetData("Anim::Fast", value);
        }

        public OtherType OtherAnim => (OtherType)Player.GetSharedData<int>("Anim::Other", -1);

        public WalkstyleType Walkstyle => (WalkstyleType)Player.GetSharedData<int>("Walkstyle", -1);

        public EmotionType Emotion => (EmotionType)Player.GetSharedData<int>("Emotion", -1);

        public bool IsInvisible => Player.GetSharedData<bool>("IsInvisible", false);

        public bool IsInvincible => Player.GetSharedData<bool>("IsInvincible", false);

        public bool IsFlyOn => Player.GetSharedData<bool>("Fly", false);

        public int MuteTime
        {
            get => Player.GetData<int>("MuteTime");
            set => Player.SetData("MuteTime", value);
        }

        public int JailTime
        {
            get => Player.GetData<int>("JailTime");
            set => Player.SetData("JailTime", value);
        }

        public List<(uint VID, Data.Vehicles.Vehicle Data)> OwnedVehicles
        {
            get => Player.LocalPlayer.GetData<List<(uint VID, Data.Vehicles.Vehicle Data)>>("OwnedVehicles");
            set => Player.LocalPlayer.SetData("OwnedVehicles", value);
        }

        public List<Business> OwnedBusinesses
        {
            get => Player.LocalPlayer.GetData<List<Business>>("OwnedBusinesses");
            set => Player.LocalPlayer.SetData("OwnedBusinesses", value);
        }

        public List<House> OwnedHouses
        {
            get => Player.LocalPlayer.GetData<List<House>>("OwnedHouses");
            set => Player.LocalPlayer.SetData("OwnedHouses", value);
        }

        public List<Apartments> OwnedApartments
        {
            get => Player.LocalPlayer.GetData<List<Apartments>>("OwnedApartments");
            set => Player.LocalPlayer.SetData("OwnedApartments", value);
        }

        public List<Garage> OwnedGarages
        {
            get => Player.LocalPlayer.GetData<List<Garage>>("OwnedGarages");
            set => Player.LocalPlayer.SetData("OwnedGarages", value);
        }

        public HouseBase SettledHouseBase
        {
            get => Player.LocalPlayer.GetData<HouseBase>("SettledHouseBase");
            set
            {
                if (value == null)
                    Player.LocalPlayer.ResetData("SettledHouseBase");
                else
                    Player.LocalPlayer.SetData("SettledHouseBase", value);
            }
        }

        public Dictionary<uint, Furniture> Furniture
        {
            get => Player.LocalPlayer.GetData<Dictionary<uint, Furniture>>("Furniture");
            set => Player.LocalPlayer.SetData("Furniture", value);
        }

        public Dictionary<WeaponSkin.ItemData.Types, string> WeaponSkins
        {
            get => Player.LocalPlayer.GetData<Dictionary<WeaponSkin.ItemData.Types, string>>("WeaponSkins");
            set => Player.LocalPlayer.SetData("WeaponSkins", value);
        }

        public HashSet<uint> Familiars
        {
            get => Player.LocalPlayer.GetData<HashSet<uint>>("Familiars");
            set => Player.LocalPlayer.SetData("Familiars", value);
        }

        public Dictionary<SkillTypes, int> Skills
        {
            get => Player.LocalPlayer.GetData<Dictionary<SkillTypes, int>>("Skills");
            set => Player.LocalPlayer.SetData("Skills", value);
        }

        public HashSet<LicenseTypes> Licenses
        {
            get => Player.LocalPlayer.GetData<HashSet<LicenseTypes>>("Licenses");
            set => Player.LocalPlayer.SetData("Licenses", value);
        }

        public MedicalCard MedicalCard
        {
            get => Player.LocalPlayer.GetData<MedicalCard>("MedicalCard");
            set
            {
                if (value == null)
                    Player.LocalPlayer.ResetData("MedicalCard");
                else
                    Player.LocalPlayer.SetData("MedicalCard", value);
            }
        }

        public Dictionary<AchievementTypes, (int Progress, bool IsRecieved)> Achievements
        {
            get => Player.LocalPlayer.GetData<Dictionary<AchievementTypes, (int, bool)>>("Achievements");
            set => Player.LocalPlayer.SetData("Achievements", value);
        }

        public List<Quest> Quests
        {
            get => Player.LocalPlayer.GetData<List<Quest>>("Quests");
            set => Player.LocalPlayer.SetData("Quests", value);
        }

        public Entity IsAttachedTo
        {
            get => Player.GetData<Entity>("IsAttachedTo::Entity");
            set
            {
                if (value == null)
                    Player.ResetData("IsAttachedTo::Entity");
                else
                    Player.SetData("IsAttachedTo::Entity", value);
            }
        }

        public List<AttachmentObject> AttachedObjects => Attachments.Service.GetEntityObjectAttachments(Player);

        public List<AttachmentEntity> AttachedEntities => Attachments.Service.GetEntityEntityAttachments(Player);

        public List<int> Decorations => Player.GetSharedData<JArray>("DCR", null)?.ToObject<List<int>>();

        public Data.Customization.Customization.HairOverlay HairOverlay => Data.Customization.Customization.GetHairOverlay(Sex, Player.GetSharedData<int>("CHO", 0));

        public AttachmentObject WearedRing => AttachedObjects.Where(x => x.Type >= AttachmentType.PedRingLeft3 && x.Type <= AttachmentType.PedRingRight3).FirstOrDefault();

        public Animation ActualAnimation
        {
            get => Player.GetData<Animation>("ActualAnim");

            set
            {
                if (value == null)
                    Player.ResetData("ActualAnim");

                Player.SetData("ActualAnim", value);
            }
        }

        public List<SMS.Message> AllSMS
        {
            get => Player.GetData<List<SMS.Message>>("AllSMS");
            set => Player.SetData("AllSMS", value);
        }

        public Dictionary<uint, string> Contacts
        {
            get => Player.GetData<Dictionary<uint, string>>("Contacts");
            set => Player.SetData("Contacts", value);
        }

        public List<uint> PhoneBlacklist
        {
            get => Player.GetData<List<uint>>("PBL");
            set => Player.SetData("PBL", value);
        }

        public uint PhoneNumber
        {
            get => Player.GetData<uint>("PhoneNumber");
            set => Player.SetData("PhoneNumber", value);
        }

        public Phone.CallInfo ActiveCall
        {
            get => Player.GetData<Phone.CallInfo>("ActiveCall");
            set
            {
                if (value == null)
                    Player.ResetData("ActiveCall");
                Player.SetData("ActiveCall", value);
            }
        }

        public Job CurrentJob
        {
            get => Player.GetData<Job>("CJob");
            set
            {
                if (value == null)
                    Player.ResetData("CJob");
                Player.SetData("CJob", value);
            }
        }

        public Fraction CurrentFraction
        {
            get => Player.GetData<Fraction>("CFraction");
            set
            {
                if (value == null)
                    Player.ResetData("CFraction");
                Player.SetData("CFraction", value);
            }
        }

        public void Reset()
        {
            if (Player == null)
                return;

            Player.ClearTasksImmediately();

            Player.SetNoCollisionEntity(Player.LocalPlayer.Handle, false);

            Management.Microphone.Service.RemoveTalker(Player);
            Management.Microphone.Service.RemoveListener(Player, false);

            Player.ResetData();
        }

        public static PlayerData GetData(Player player)
        {
            return player?.GetData<PlayerData>("SyncedData");
        }

        public static void SetData(Player player, PlayerData data)
        {
            player?.SetData("SyncedData", data);
        }
    }
}