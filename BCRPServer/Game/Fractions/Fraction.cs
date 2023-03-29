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

        PolicePaleto,
    }

    public interface IUniformable
    {
        public List<Game.Data.Customization.UniformTypes> UniformTypes { get; set; }

        public Vector3 LockerRoomPosition { get; set; }

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

        public uint Materials { get; set; }

        public ulong Balance { get; set; }

        public bool ContainerLocked { get => Sync.World.GetSharedData<bool?>($"FRAC::SL_{(int)Type}") ?? false; set { if (value) Sync.World.SetSharedData($"FRAC::SL_{(int)Type}", true); else Sync.World.ResetSharedData($"FRAC::SL_{(int)Type}"); } }

        public bool CreationWorkbenchLocked { get => Sync.World.GetSharedData<bool?>($"FRAC::CWL_{(int)Type}") ?? false; set { if (value) Sync.World.SetSharedData($"FRAC::CWL_{(int)Type}", true); else Sync.World.ResetSharedData($"FRAC::CWL_{(int)Type}"); } }

        public uint ContainerId { get; set; }

        public Game.Items.Container Container => Game.Items.Container.Get(ContainerId);

        public Utils.Vector4 ContainerPosition { get; set; }

        public Utils.Vector4 CreationWorkbenchPosition { get; set; }

        public Dictionary<string, uint> CreationWorkbenchPrices { get; set; }

        public NewsData News { get; set; }

        public Dictionary<VehicleData.VehicleInfo, VehicleProps> AllVehicles { get; set; }

        public List<PlayerData.PlayerInfo> AllMembers { get; set; } = new List<PlayerData.PlayerInfo>();

        public List<PlayerData.PlayerInfo> MembersOnline => AllMembers.Where(x => x.PlayerData != null).ToList();

        public PlayerData.PlayerInfo Leader { get; set; }

        public List<RankData> Ranks { get; set; }

        public List<uint> Salary { get; set; }

        public Utils.Vector4 SpawnPosition { get; set; }

        public abstract string ClientData { get; }

        public static int InitializeAll()
        {
            Game.Items.Container.AllSIDs.Add("f_storage", new Items.Container.Data(125, 100_000f, Items.Container.AllowedItemTypes.All, Items.Container.ContainerTypes.Storage));

            new Police(Types.PolicePaleto, "Полиция Палето-Бэй")
            {
                SpawnPosition = new Utils.Vector4(-438.325f, 5990.785f, 31.71619f, 310.7526f),

                ContainerPosition = new Utils.Vector4(-441.8761f, 5987.493f, 30.7162f, 2.5f),

                CreationWorkbenchPosition = new Utils.Vector4(-437.8557f, 5988.477f, 30.71618f, 1f),

                LockerRoomPosition = new Vector3(-439.1087f, 5993.017f, 30.71619f),

                UniformTypes = new List<Data.Customization.UniformTypes>()
                {
                    Data.Customization.UniformTypes.FractionPaletoPolice_0,
                    Data.Customization.UniformTypes.FractionPaletoPolice_1,
                    Data.Customization.UniformTypes.FractionPaletoPolice_2,
                },

                CreationWorkbenchPrices = new Dictionary<string, uint>()
                {
                    { "w_pistol", 100 },
                },

                Salary = new List<uint>()
                {
                    1_000,
                    2_000,
                }
            };

            foreach (var x in All.Values)
            {
                x.Initialize();
            }

            return All.Count;
        }

        public static void PostInitializeAll()
        {
            var lines = new List<string>();

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
            var t = PlayerData.All.Where(x => x.Value.Fraction == Type).Select(x => x.Key).ToArray();

            if (t.Length == 0)
                return;

            NAPI.ClientEvent.TriggerClientEventToPlayers(t, eventName, args);
        }

        public bool HasMemberPermission(PlayerData.PlayerInfo pInfo, uint permissionId, bool notify)
        {
            if (Ranks[pInfo.FractionRank].Permissions[permissionId] != 1)
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
            TriggerEventToMembers("Fraction::UMO", pData.CID, true);

            pData.Player.TriggerEvent("Player::SCF", (int)Type, News.SerializeToJson(), AllVehicles.Select(x => $"{x.Key.VID}&{x.Key.VID}&{x.Value.MinimalRank}"), AllMembers.Select(x => $"{x.CID}&{x.Name} {x.Surname}&{x.FractionRank}&{(x.IsOnline ? 1 : 0)}&{byte.MaxValue}&{x.LastJoinDate.GetUnixTimestamp()}"));
        }

        public virtual void OnMemberDisconnect(PlayerData pData)
        {
            TriggerEventToMembers("Fraction::UMO", pData.CID, false);
        }

        public virtual void SetPlayerFraction(PlayerData.PlayerInfo pInfo, byte rank)
        {
            if (pInfo.PlayerData != null)
            {
                pInfo.PlayerData.Fraction = Type;
            }
            else
            {
                pInfo.Fraction = Type;
            }

            pInfo.FractionRank = rank;

            AllMembers.Add(pInfo);

            TriggerEventToMembers("Fraction::MC", pInfo.CID, rank);
        }

        public virtual void SetPlayerNoFraction(PlayerData.PlayerInfo pInfo)
        {
            if (pInfo.PlayerData != null)
            {
                pInfo.PlayerData.Fraction = Types.None;
            }
            else
            {
                pInfo.Fraction = Types.None;
            }

            pInfo.FractionRank = 0;

            AllMembers.Remove(pInfo);

            TriggerEventToMembers("Fraction::MC", pInfo.CID);
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
    }
}
