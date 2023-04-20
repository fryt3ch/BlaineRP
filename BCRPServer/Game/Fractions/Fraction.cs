using GTANetworkAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace BCRPServer.Game.Fractions
{
    public enum Types
    {
        None = 0,

        BCPD,
        LSPD,

        WZLN,

        BCEMS,
        LSEMS,

        LSADM,
    }

    public interface IUniformable
    {
        public List<Game.Data.Customization.UniformTypes> UniformTypes { get; set; }

        public Vector3[] LockerRoomPositions { get; set; }

        public bool IsPlayerInAnyUniform(PlayerData pData);
    }

    public class RankData
    {
        [JsonProperty(PropertyName = "N")]
        public string Name { get; set; }

        /// <summary>Стандарные Id прав для всех фракций</summary>
        /// <remarks>0 - Доступ к складу, 1 - Приглашение, 2 - Повышение/понижение<br/>3 - Увольнение, 4 - Респавн транспорта</remarks>
        [JsonProperty(PropertyName = "P")]
        public Dictionary<uint, byte> Permissions { get; set; }
    }

    public class VehicleProps
    {
        [JsonProperty(PropertyName = "R")]
        public byte MinimalRank { get; set; }

        [JsonIgnore]
        public DateTime LastRespawnedTime { get; set; }
    }

    public class NewsData
    {
        [JsonProperty(PropertyName = "A")]
        public Dictionary<int, string> All { get; set; }

        [JsonProperty(PropertyName = "P")]
        public int PinnedId { get; set; }

        [JsonIgnore]
        public Queue<int> FreeIdxes { get; set; } = new Queue<int>();
    }

    public abstract class Fraction
    {
        public static Dictionary<Types, Fraction> All { get; set; } = new Dictionary<Types, Fraction>();

        public static Fraction Get(Types type) => All.GetValueOrDefault(type);

        public Types Type { get; set; }

        public string Name { get; set; }

        public uint Materials { get => Convert.ToUInt32(Sync.World.GetSharedData<object>($"FRAC::M_{(int)Type}")); set => Sync.World.SetSharedData($"FRAC::M_{(int)Type}", value); }

        public ulong Balance { get; set; }

        public bool ContainerLocked { get => Sync.World.GetSharedData<bool?>($"FRAC::SL_{(int)Type}") ?? false; set { if (value) Sync.World.SetSharedData($"FRAC::SL_{(int)Type}", true); else Sync.World.ResetSharedData($"FRAC::SL_{(int)Type}"); } }

        public bool CreationWorkbenchLocked { get => Sync.World.GetSharedData<bool?>($"FRAC::CWL_{(int)Type}") ?? false; set { if (value) Sync.World.SetSharedData($"FRAC::CWL_{(int)Type}", true); else Sync.World.ResetSharedData($"FRAC::CWL_{(int)Type}"); } }

        public uint ContainerId { get; set; }

        public Game.Items.Container Container => Game.Items.Container.Get(ContainerId);

        public Utils.Vector4[] ContainerPositions { get; set; }

        public Utils.Vector4[] CreationWorkbenchPositions { get; set; }

        public Dictionary<string, uint> CreationWorkbenchPrices { get; set; }

        public NewsData News { get; set; }

        public Dictionary<VehicleData.VehicleInfo, VehicleProps> AllVehicles { get; set; }

        public List<PlayerData.PlayerInfo> AllMembers { get; set; } = new List<PlayerData.PlayerInfo>();

        public List<PlayerData.PlayerInfo> MembersOnline => AllMembers.Where(x => x.PlayerData != null).ToList();

        public PlayerData.PlayerInfo Leader { get; set; }

        public List<RankData> Ranks { get; set; }

        public List<uint> Salary { get; set; }

        public Utils.Vector4[] SpawnPositions { get; set; }

        public abstract string ClientData { get; }

        public string ItemTag { get; set; }

        public static int InitializeAll()
        {
            Game.Items.Container.AllSIDs.Add("f_storage", new Items.Container.Data(125, 100_000f, Items.Container.AllowedItemTypes.All, Items.Container.ContainerTypes.Storage));

            new Police(Types.BCPD, "Полиция Округа Блэйн")
            {
                SpawnPositions = new Utils.Vector4[]
                {
                    new Utils.Vector4(-438.325f, 5990.785f, 31.71619f, 310.7526f),
                    new Utils.Vector4(1853.851f, 3686.156f, 34.26704f, 212.9599f),
                },

                ContainerPositions = new Utils.Vector4[]
                {
                     new Utils.Vector4(-441.8761f, 5987.493f, 30.7162f, 2.5f),
                },

                CreationWorkbenchPositions = new Utils.Vector4[]
                {
                    new Utils.Vector4(-437.8557f, 5988.477f, 30.71618f, 1f),
                },

                LockerRoomPositions = new Vector3[]
                {
                    new Vector3(-439.1087f, 5993.017f, 30.71619f),
                },

                UniformTypes = new List<Data.Customization.UniformTypes>()
                {
                    Data.Customization.UniformTypes.FractionPaletoPolice_0,
                    Data.Customization.UniformTypes.FractionPaletoPolice_1,
                    Data.Customization.UniformTypes.FractionPaletoPolice_2,
                },

                ArrestCellsPositions = new Utils.Vector4[]
                {
                    new Utils.Vector4(-429.6015f, 6001.549f, 31.71618f, 3f),
                    new Utils.Vector4(-426.6064f, 5998.16f, 31.71618f, 3f),
                },

                ArrestFreePosition = new Utils.Vector4(-442.0793f, 6017.475f, 31.67867f, 314.3072f),

                ArrestMenuPosition = new Vector3(-435.4453f, 5997.362f, 31.71618f),

                CreationWorkbenchPrices = new Dictionary<string, uint>()
                {
                    { "w_pistol", 100 },
                },

                Salary = new List<uint>()
                {
                    1_000,
                    2_000,
                },

                ItemTag = "BCPD",
            };

            new Police(Types.LSPD, "Полиция Лос-Сантоса")
            {
                SpawnPositions = new Utils.Vector4[]
                {
                    new Utils.Vector4(455.8622f, -991.1062f, 30.68932f, 88.41425f),
                },

                ContainerPositions = new Utils.Vector4[]
                {
                    new Utils.Vector4(477.5417f, -989.4244f, 23.91471f, 2.5f),
                },

                CreationWorkbenchPositions = new Utils.Vector4[]
                {
                    new Utils.Vector4(472.8697f, -989.4165f, 23.91472f, 1f),
                },

                LockerRoomPositions = new Vector3[]
                {
                    new Vector3(451.3782f, -992.9793f, 29.68934f),
                },

                UniformTypes = new List<Data.Customization.UniformTypes>()
                {
                    Data.Customization.UniformTypes.FractionPaletoPolice_0,
                    Data.Customization.UniformTypes.FractionPaletoPolice_1,
                    Data.Customization.UniformTypes.FractionPaletoPolice_2,
                },

                ArrestCellsPositions = new Utils.Vector4[]
                {

                },

                ArrestFreePosition = new Utils.Vector4(433.1303f, -981.7498f, 30.71028f, 86.3075f),

                ArrestMenuPosition = new Vector3(0f, 0f, 0f),

                CreationWorkbenchPrices = new Dictionary<string, uint>()
                {
                    { "w_pistol", 100 },
                },

                Salary = new List<uint>()
                {
                    1_000,
                    2_000,
                },

                ItemTag = "LSPD",
            };

            new WeazelNews(Types.WZLN, "Weazel News")
            {
                SpawnPositions = new Utils.Vector4[]
                {
                    new Utils.Vector4(-594.6252f, -930.1151f, 28.15707f, 266.3295f),
                },

                ContainerPositions = new Utils.Vector4[]
                {
                    new Utils.Vector4(-578.9703f, -915.7086f, 27.15708f, 1.5f),
                },

                CreationWorkbenchPositions = new Utils.Vector4[]
                {
                    new Utils.Vector4(-601.053f, -917.2875f, 27.15707f, 1f),
                },

                CreationWorkbenchPrices = new Dictionary<string, uint>()
                {

                },

                Salary = new List<uint>()
                {
                    1_000,
                    2_000,
                },

                ItemTag = "WZLN",
            };

            new EMS(Types.BCEMS, "Больница Округа Блэйн")
            {
                SpawnPositions = new Utils.Vector4[]
                {
                    new Utils.Vector4(-258.9563f, 6330.19f, 32.42728f, 218.3881f),
                    new Utils.Vector4(1842.196f, 3679.172f, 34.27489f, 118.615f),
                },

                LockerRoomPositions = new Vector3[]
                {
                    new Vector3(-256.1902f, 6327.726f, 31.42725f),
                },

                UniformTypes = new List<Data.Customization.UniformTypes>()
                {
                    Data.Customization.UniformTypes.FractionPaletoEMS_0,
                },

                ContainerPositions = new Utils.Vector4[]
                {
                    new Utils.Vector4(-264.9498f, 6321.589f, 31.4273f, 1.5f),
                },

                CreationWorkbenchPositions = new Utils.Vector4[]
                {
                    new Utils.Vector4(-262.7031f, 6319.312f, 31.4273f, 1f),
                },

                CreationWorkbenchPrices = new Dictionary<string, uint>()
                {

                },

                Salary = new List<uint>()
                {
                    1_000,
                    2_000,
                },

                ItemTag = "BCEMS",

                BedPositions = new Vector3()
                {

                },
            };

            new EMS(Types.LSEMS, "Больница Лос-Сантоса")
            {
                SpawnPositions = new Utils.Vector4[]
                {
                    new Utils.Vector4(306.8656f, -572.422f, 43.28408f, 161.3542f),
                },

                LockerRoomPositions = new Vector3[]
                {
                    new Vector3(302.8677f, -572.0403f, 42.28407f),
                },

                UniformTypes = new List<Data.Customization.UniformTypes>()
                {
                    Data.Customization.UniformTypes.FractionPaletoEMS_0,
                },

                ContainerPositions = new Utils.Vector4[]
                {
                    new Utils.Vector4(338.8343f, -595.1829f, 42.2841f, 1.5f),
                },

                CreationWorkbenchPositions = new Utils.Vector4[]
                {
                    new Utils.Vector4(341.1305f, -588.7886f, 42.2841f, 1f),
                },

                CreationWorkbenchPrices = new Dictionary<string, uint>()
                {

                },

                Salary = new List<uint>()
                {
                    1_000,
                    2_000,
                },

                ItemTag = "LSEMS",

                BedPositions = new Vector3()
                {

                },
            };

            new Government(Types.LSADM, "Правительство Лос-Сантоса")
            {
                SpawnPositions = new Utils.Vector4[]
                {
                    new Utils.Vector4(-540.1923f, -197.8367f, 47.42305f, 0.4962777f),
                },

                LockerRoomPositions = new Vector3[]
                {
                    new Vector3(-541.5637f, -192.8628f, 46.42308f),
                },

                UniformTypes = new List<Data.Customization.UniformTypes>()
                {
                    Data.Customization.UniformTypes.FractionPaletoEMS_0,
                },

                CreationWorkbenchPositions = new Utils.Vector4[]
                {
                    new Utils.Vector4(-549.821f, -202.9997f, 46.41494f, 1.5f),
                },

                ContainerPositions = new Utils.Vector4[]
                {
                    new Utils.Vector4(-543.8552f, -198.2676f, 46.41494f, 1f),
                },

                CreationWorkbenchPrices = new Dictionary<string, uint>()
                {

                },

                Salary = new List<uint>()
                {
                    1_000,
                    2_000,
                },

                ItemTag = "LSGOV",
            };

            Events.NPC.NPC.AddNpc($"cop0_{(int)Game.Fractions.Types.BCPD}", new Vector3(-448.2888f, 6012.634f, 31.71635f)); // cop0_1

            foreach (var x in All.Values)
            {
                x.Initialize();
            }

            return All.Count;
        }

        public static void PostInitializeAll()
        {
            var lines = new List<string>();

            lines.Add($"Fractions.Police.NumberplatePrices = RAGE.Util.Json.Deserialize<Dictionary<string, uint[]>>(\"{Police.NumberplatePrices.SerializeToJson().Replace('"', '\'')}\");");

            foreach (var x in All.Values)
            {
                x.PostInitialize();

                lines.Add($"new Fractions.{x.GetType().Name}({x.ClientData});");
            }

            Utils.FillFileToReplaceRegion(Settings.DIR_CLIENT_LOCATIONS_DATA_PATH, "FRACTIONS_TO_REPLACE", lines);
        }

        public void SetRankName(byte rank, string rankName, bool updateDb)
        {
            Ranks[rank].Name = rankName;

            Sync.World.SetSharedData($"FRAC::RN_{(int)Type}_{rank}", rankName);

            if (updateDb)
                MySQL.FractionUpdateRanks(this);
        }

        public void SetVehicleMinRank(VehicleData.VehicleInfo vInfo, VehicleProps vProps, byte minRank, bool updateDb)
        {
            vProps.MinimalRank = minRank;

            TriggerEventToMembers("Fraction::UVEHMR", vInfo.VID, minRank);

            if (updateDb)
                MySQL.FractionUpdateVehicles(this);
        }

        public void SetLeader(PlayerData.PlayerInfo pInfo, bool onStart)
        {
            if (onStart)
            {
                Leader = pInfo;

                if (pInfo == null)
                {
                    Sync.World.ResetSharedData($"FRAC::L_{(int)Type}");
                }
                else
                {
                    Sync.World.SetSharedData($"FRAC::L_{(int)Type}", pInfo.CID);
                }
            }
            else
            {
                //MySQL.FractionUpdateLeader(this);
            }
        }

        public bool TryAddMoney(ulong amount, out ulong newBalance, bool notifyOnFault = true, PlayerData tData = null)
        {
            if (!Balance.TryAdd(amount, out newBalance))
            {
                if (notifyOnFault)
                {

                }

                return false;
            }

            return true;
        }

        public bool TryRemoveMoney(ulong amount, out ulong newBalance, bool notifyOnFault = true, PlayerData tData = null)
        {
            if (!Balance.TrySubtract(amount, out newBalance))
            {
                if (notifyOnFault)
                {
                    if (tData != null)
                    {
                        tData.Player.Notify("Fraction::NEMB", Balance);
                    }
                }

                return false;
            }

            return true;
        }

        public bool TryAddMaterials(uint amount, out uint newBalance, bool notifyOnFault = true, PlayerData tData = null)
        {
            if (!Materials.TryAdd(amount, out newBalance))
            {
                if (notifyOnFault)
                {

                }

                return false;
            }

            return true;
        }

        public bool TryRemoveMaterials(uint amount, out uint newBalance, bool notifyOnFault = true, PlayerData tData = null)
        {
            if (!Materials.TrySubtract(amount, out newBalance))
            {
                if (notifyOnFault)
                {
                    if (tData != null)
                    {
                        tData.Player.Notify("Fraction::NEMA", Materials);
                    }
                }

                return false;
            }

            return true;
        }

        public void SetMaterials(uint value, bool updateDb)
        {
            Materials = value;

            if (updateDb)
                MySQL.FractionUpdateMaterials(this);
        }

        public void SetBalance(ulong value, bool updateDb)
        {
            Balance = value;

            if (updateDb)
                MySQL.FractionUpdateBalance(this);
        }

        public void SetStorageLocked(bool state, bool updateDb)
        {
            ContainerLocked = state;

            if (updateDb)
                MySQL.FractionUpdateLockStates(this);
        }

        public void SetCreationWorkbenchLocked(bool state, bool updateDb)
        {
            CreationWorkbenchLocked = state;

            if (updateDb)
                MySQL.FractionUpdateLockStates(this);
        }

        public virtual void Initialize()
        {

        }

        public virtual void PostInitialize()
        {
            foreach (var x in AllVehicles)
            {
                x.Key.Spawn();
            }

            for (byte i = 0; i < Ranks.Count; i++)
            {
                SetRankName(i, Ranks[i].Name, false);
            }
        }

        public void TriggerEventToMembers(string eventName, params object[] args)
        {
            var t = AllMembers.Where(x => x.PlayerData != null).Select(x => x.PlayerData.Player).ToArray();

            if (t.Length == 0)
                return;

            NAPI.ClientEvent.TriggerClientEventToPlayers(t, eventName, args);
        }

        public bool HasMemberPermission(PlayerData.PlayerInfo pInfo, uint permissionId, bool notify)
        {
            if (Ranks[pInfo.FractionRank].Permissions.GetValueOrDefault(permissionId) == 0)
            {
                if (notify)
                {
                    if (pInfo.PlayerData != null)
                    {
                        pInfo.PlayerData.Player.Notify("Fraction::NA");
                    }
                }

                return false;
            }

            return true;
        }

        public bool IsLeader(PlayerData.PlayerInfo pInfo, bool notify)
        {
            if (Leader == null || Leader != pInfo)
            {
                if (notify)
                {
                    if (pInfo.PlayerData != null)
                    {
                        pInfo.PlayerData.Player.Notify("Fraction::NAL");
                    }
                }

                return false;
            }

            return true;
        }

        public bool IsLeaderOrWarden(PlayerData.PlayerInfo pInfo, bool notify)
        {
            if (pInfo.FractionRank < Ranks.Count - 1 && !IsLeader(pInfo, false))
            {
                if (notify)
                {
                    if (pInfo.PlayerData != null)
                    {
                        pInfo.PlayerData.Player.Notify("Fraction::NAL");
                    }
                }

                return false;
            }

            return true;
        }

        public virtual void OnMemberJoined(PlayerData pData)
        {
            TriggerEventToMembers("Fraction::UMO", pData.CID, true, GetMemberStatus(pData.Info));

            FractionDataTriggerEvent(pData);
        }

        public virtual void OnMemberDisconnect(PlayerData pData)
        {
            TriggerEventToMembers("Fraction::UMO", pData.CID, false);
        }

        public virtual void OnMemberStatusChange(PlayerData.PlayerInfo pInfo, byte status)
        {
            TriggerEventToMembers("Fraction::UMS", pInfo.CID, status);
        }

        public virtual void SetPlayerRank(PlayerData.PlayerInfo pInfo, byte rank)
        {
            pInfo.FractionRank = rank;

            TriggerEventToMembers("Fraction::MC", pInfo.CID, rank);

            MySQL.CharacterFractionAndRankUpdate(pInfo);
        }

        protected virtual void FractionDataTriggerEvent(PlayerData pData)
        {
            pData.Player.TriggerEvent("Player::SCF", (int)Type, News.SerializeToJson(), AllVehicles.Select(x => $"{x.Key.VID}&{x.Key.VID}&{x.Value.MinimalRank}").ToList(), AllMembers.Select(x => $"{x.CID}&{x.Name} {x.Surname}&{x.FractionRank}&{(x.IsOnline ? 1 : 0)}&{GetMemberStatus(x)}&{x.LastJoinDate.GetUnixTimestamp()}").ToList());
        }

        public virtual void SetPlayerFraction(PlayerData.PlayerInfo pInfo, byte rank)
        {
            if (pInfo.PlayerData != null)
            {
                pInfo.PlayerData.Fraction = Type;

                FractionDataTriggerEvent(pInfo.PlayerData);
            }
            else
            {
                pInfo.Fraction = Type;
            }

            pInfo.FractionRank = rank;

            TriggerEventToMembers("Fraction::MC", pInfo.CID, rank);

            AllMembers.Add(pInfo);

            MySQL.CharacterFractionAndRankUpdate(pInfo);
        }

        public virtual void SetPlayerNoFraction(PlayerData.PlayerInfo pInfo)
        {
            if (pInfo.PlayerData != null)
            {
                pInfo.PlayerData.Fraction = Types.None;

                pInfo.PlayerData.Player.TriggerEvent("Player::SCF");
            }
            else
            {
                pInfo.Fraction = Types.None;
            }

            pInfo.FractionRank = 0;

            AllMembers.Remove(pInfo);

            TriggerEventToMembers("Fraction::MC", pInfo.CID);

            MySQL.CharacterFractionAndRankUpdate(pInfo);
        }

        public void AddNews(string text, bool updateDb)
        {
            int idx;

            if (!News.FreeIdxes.TryDequeue(out idx))
                idx = News.All.Count == 0 ? 0 : (News.All.Keys.Max() + 1);

            News.All.Add(idx, text);

            TriggerEventToMembers("Fraction::NEWSC", idx, text);

            if (updateDb)
                MySQL.FractionUpdateNews(this);
        }

        public void EditNews(int idx, string text, bool updateDb)
        {
            News.All[idx] = text;

            TriggerEventToMembers("Fraction::NEWSC", idx, text);

            if (updateDb)
                MySQL.FractionUpdateNews(this);
        }

        public bool DeleteNews(int idx, bool updateDb)
        {
            if (News.All.Remove(idx))
            {
                if (News.PinnedId == idx)
                {
                    News.PinnedId = -1;
                }

                News.FreeIdxes.Enqueue(idx);

                TriggerEventToMembers("Fraction::NEWSC", idx);

                if (updateDb)
                    MySQL.FractionUpdateNews(this);

                return true;
            }

            return false;
        }

        public void PinNews(int idx, bool updateDb)
        {
            News.PinnedId = idx;

            TriggerEventToMembers("Fraction::NEWSP", idx);

            if (updateDb)
                MySQL.FractionUpdateNews(this);
        }

        public Fraction(Types Type, string Name)
        {
            this.Type = Type;

            this.Name = Name;

            All.Add(Type, this);
        }

        public static bool IsMember(PlayerData pData, Types type, bool notify)
        {
            if (type == Types.None || pData.Fraction != type)
            {
                if (notify)
                {
                    pData.Player.Notify("Fraction::NM");
                }

                return false;
            }

            return true;
        }

        public static bool IsMemberOfAnyFraction(PlayerData pData, bool notify)
        {
            if (pData.Fraction == Types.None)
            {
                if (notify)
                {
                    pData.Player.Notify("Fraction::NMA");
                }

                return false;
            }

            return true;
        }

        public virtual byte GetMemberStatus(PlayerData.PlayerInfo pInfo)
        {
            byte status = 0;

            foreach (var x in pInfo.Punishments)
            {
                if (x.Type == Sync.Punishment.Types.Ban)
                {
                    if (x.IsActive())
                        return 1;
                }
                else if (x.Type == Sync.Punishment.Types.NRPPrison)
                {
                    if (x.IsActive())
                        status = 2;
                }
            }

            return status;
        }

        public Utils.Vector4 GetSpawnPosition(byte idx) => idx >= SpawnPositions.Length ? null : SpawnPositions[idx];
        public Utils.Vector4 GetCreationWorkbenchPosition(byte idx) => idx >= CreationWorkbenchPositions.Length ? null : CreationWorkbenchPositions[idx];
        public Utils.Vector4 GetContainerPosition(byte idx) => idx >= ContainerPositions.Length ? null : ContainerPositions[idx];

        public static bool IsFractionGov(Types type) => type >= Types.BCPD;
    }
}
