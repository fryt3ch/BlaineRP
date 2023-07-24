﻿using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Animations;
using BlaineRP.Client.Animations.Enums;
using BlaineRP.Client.CEF.Phone.Apps;
using BlaineRP.Client.EntitiesData.Components;
using BlaineRP.Client.EntitiesData.Enums;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Quests;
using BlaineRP.Client.Sync;
using Newtonsoft.Json.Linq;
using RAGE.Elements;

namespace BlaineRP.Client.EntitiesData
{
    public class PlayerData
    {
        public Player Player { get; private set; }

        public PlayerData(Player player)
        {
            Player = player;
        }

        public uint CID => Utils.Convert.ToUInt32(Player.GetSharedData<object>("CID", 0));

        public ulong Cash => Utils.Convert.ToUInt64(Player.GetSharedData<object>("Cash", 0));

        public ulong BankBalance => Utils.Convert.ToUInt64(Player.GetSharedData<object>("BankBalance", 0));

        public bool Sex => Player.GetSharedData<bool>("Sex", true);

        public Data.Fractions.Types Fraction => (Data.Fractions.Types)Player.GetSharedData<int>("Fraction", 0);

        public int Satiety => Utils.Convert.ToByte(Player.GetSharedData<object>("Satiety", 0));

        public int Mood => Utils.Convert.ToByte(Player.GetSharedData<object>("Mood", 0));

        public bool IsMasked => Player.GetDrawableVariation(1) > 0;

        public bool IsKnocked => Player.GetSharedData<bool>("Knocked", false);

        public bool CrouchOn => Player.GetSharedData<bool>("Crouch::On", false);

        public bool CrawlOn => Player.GetSharedData<bool>("Crawl::On", false);

        public string WeaponComponents => Player.GetSharedData<string>("WCD", null);

        public float VoiceRange => Player.GetSharedData<float>("VoiceRange", 0f);

        public bool IsMuted => VoiceRange < 0f;

        public bool IsCuffed => AttachedObjects?.Where(x => x.Type == AttachSystem.Types.Cuffs || x.Type == AttachSystem.Types.CableCuffs).Any() == true;

        public bool IsInvalid => Player.GetSharedData<bool>("IsInvalid", false);

        public bool IsJailed => Player.GetSharedData<bool>("IsJailed", false);

        public bool IsFrozen => Player.GetSharedData<bool>("IsFrozen", false);

        public string Hat => Player.GetSharedData<string>("Hat", null);

        public bool IsWounded => Player.GetSharedData<bool>("IsWounded", false);

        public bool BeltOn => Player.GetSharedData<bool>("Belt::On", false);

        public Sync.Phone.PhoneStateTypes PhoneStateType => (Sync.Phone.PhoneStateTypes)Player.GetSharedData<int>("PST", 0);

        public int AdminLevel => Player.GetSharedData<int>("AdminLevel", -1);

        public int VehicleSeat => Player.GetSharedData<int>("VehicleSeat", -1);

        public Vehicle AutoPilot
        {
            get => Player.GetData<Vehicle>("AutoPilot::State");
            set
            {
                if (value == null) Player.ResetData("AutoPilot::State");
                else Player.SetData("AutoPilot::State", value);
            }
        }

        public GeneralTypes GeneralAnim => (GeneralTypes)Player.GetSharedData<int>("Anim::General", -1);

        public FastTypes FastAnim
        {
            get => Player.GetData<FastTypes>("Anim::Fast");
            set => Player.SetData("Anim::Fast", value);
        }

        public OtherTypes OtherAnim => (OtherTypes)Player.GetSharedData<int>("Anim::Other", -1);

        public WalkstyleTypes Walkstyle => (WalkstyleTypes)Player.GetSharedData<int>("Walkstyle", -1);

        public EmotionTypes Emotion => (EmotionTypes)Player.GetSharedData<int>("Emotion", -1);

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

        public List<Data.Locations.Business> OwnedBusinesses
        {
            get => Player.LocalPlayer.GetData<List<Data.Locations.Business>>("OwnedBusinesses");
            set => Player.LocalPlayer.SetData("OwnedBusinesses", value);
        }

        public List<Data.Locations.House> OwnedHouses
        {
            get => Player.LocalPlayer.GetData<List<Data.Locations.House>>("OwnedHouses");
            set => Player.LocalPlayer.SetData("OwnedHouses", value);
        }

        public List<Data.Locations.Apartments> OwnedApartments
        {
            get => Player.LocalPlayer.GetData<List<Data.Locations.Apartments>>("OwnedApartments");
            set => Player.LocalPlayer.SetData("OwnedApartments", value);
        }

        public List<Data.Locations.Garage> OwnedGarages
        {
            get => Player.LocalPlayer.GetData<List<Data.Locations.Garage>>("OwnedGarages");
            set => Player.LocalPlayer.SetData("OwnedGarages", value);
        }

        public Data.Locations.HouseBase SettledHouseBase
        {
            get => Player.LocalPlayer.GetData<Data.Locations.HouseBase>("SettledHouseBase");
            set
            {
                if (value == null) Player.LocalPlayer.ResetData("SettledHouseBase");
                else Player.LocalPlayer.SetData("SettledHouseBase", value);
            }
        }

        public Dictionary<uint, Data.Furniture> Furniture
        {
            get => Player.LocalPlayer.GetData<Dictionary<uint, Data.Furniture>>("Furniture");
            set => Player.LocalPlayer.SetData("Furniture", value);
        }

        public Dictionary<Data.Items.WeaponSkin.ItemData.Types, string> WeaponSkins
        {
            get => Player.LocalPlayer.GetData<Dictionary<Data.Items.WeaponSkin.ItemData.Types, string>>("WeaponSkins");
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
                if (value == null) Player.LocalPlayer.ResetData("MedicalCard");
                else Player.LocalPlayer.SetData("MedicalCard", value);
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
                if (value == null) Player.ResetData("IsAttachedTo::Entity");
                else Player.SetData("IsAttachedTo::Entity", value);
            }
        }

        public List<AttachSystem.AttachmentObject> AttachedObjects => AttachSystem.GetEntityObjectAttachments(Player);

        public List<AttachSystem.AttachmentEntity> AttachedEntities => AttachSystem.GetEntityEntityAttachments(Player);

        public List<int> Decorations => Player.GetSharedData<JArray>("DCR", null)?.ToObject<List<int>>();

        public Data.Customization.HairOverlay HairOverlay => Data.Customization.GetHairOverlay(Sex, Player.GetSharedData<int>("CHO", 0));

        public AttachSystem.AttachmentObject WearedRing =>
            AttachedObjects.Where(x => x.Type >= AttachSystem.Types.PedRingLeft3 && x.Type <= AttachSystem.Types.PedRingRight3).FirstOrDefault();

        public Animation ActualAnimation
        {
            get => Player.GetData<Animation>("ActualAnim");

            set
            {
                if (value == null) Player.ResetData("ActualAnim");

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

        public CEF.Phone.Apps.Phone.CallInfo ActiveCall
        {
            get => Player.GetData<CEF.Phone.Apps.Phone.CallInfo>("ActiveCall");
            set
            {
                if (value == null) Player.ResetData("ActiveCall");
                Player.SetData("ActiveCall", value);
            }
        }

        public Data.Jobs.Job CurrentJob
        {
            get => Player.GetData<Data.Jobs.Job>("CJob");
            set
            {
                if (value == null) Player.ResetData("CJob");
                Player.SetData("CJob", value);
            }
        }

        public Data.Fractions.Fraction CurrentFraction
        {
            get => Player.GetData<Data.Fractions.Fraction>("CFraction");
            set
            {
                if (value == null) Player.ResetData("CFraction");
                Player.SetData("CFraction", value);
            }
        }

        public void Reset()
        {
            if (Player == null)
                return;

            Player.ClearTasksImmediately();

            Player.SetNoCollisionEntity(Player.LocalPlayer.Handle, false);

            Microphone.RemoveTalker(Player);
            Microphone.RemoveListener(Player, false);

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