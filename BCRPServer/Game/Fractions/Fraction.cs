using GTANetworkAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPServer.Game.Fractions
{
    public enum Types
    {
        None = 0,

        COP_BLAINE = 1,
        COP_LS = 2,

        FIB_LS = 5,

        MEDIA_LS = 10,

        EMS_BLAINE = 20,
        EMS_LS = 21,

        GOV_LS = 30,

        GANG_MARA = 40,
        GANG_FAMS = 41,
        GANG_BALS = 42,
        GANG_VAGS = 43,

        MAFIA_RUSSIA = 60,
        MAFIA_ITALY = 61,
        MAFIA_JAPAN = 62,

        ARMY_FZ = 80,

        PRISON_BB = 90,
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

    public abstract partial class Fraction
    {
        public static Dictionary<Types, Fraction> All { get; set; } = new Dictionary<Types, Fraction>();

        public static Fraction Get(Types type) => All.GetValueOrDefault(type);

        public Types Type { get; set; }

        public string Name { get; set; }

        public uint Materials { get => Utils.ToUInt32(Sync.World.GetSharedData<object>($"FRAC::M_{(int)Type}")); set => Sync.World.SetSharedData($"FRAC::M_{(int)Type}", value); }

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

            if (state)
            {
                var cont = Game.Items.Container.Get(ContainerId);

                if (cont != null)
                {
                    cont.ClearAllObservers(x => !HasMemberPermission(x.Info, 0, false));
                }
            }
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

            TriggerEventToMembers("Fraction::UMR", pInfo.CID, rank);

            MySQL.CharacterFractionAndRankUpdate(pInfo);
        }

        protected virtual void FractionDataTriggerEvent(PlayerData pData)
        {
            pData.Player.TriggerEvent("Player::SCF", (int)Type, News.SerializeToJson(), AllVehicles.Select(x => $"{x.Key.VID}&{x.Key.VID}&{x.Value.MinimalRank}").ToList(), AllMembers.Select(x => $"{x.CID}&{x.Name} {x.Surname}&{x.FractionRank}&{(x.IsOnline ? 1 : 0)}&{GetMemberStatus(x)}&{x.LastJoinDate.GetUnixTimestamp()}").ToList());
        }

        public virtual void SetPlayerFraction(PlayerData.PlayerInfo pInfo, byte rank)
        {
            pInfo.FractionRank = rank;

            AllMembers.Add(pInfo);

            TriggerEventToMembers("Fraction::UM", pInfo.CID, $"{pInfo.Name} {pInfo.Surname}", rank, GetMemberStatus(pInfo), pInfo.LastJoinDate.GetUnixTimestamp());

            if (pInfo.PlayerData != null)
            {
                pInfo.PlayerData.Fraction = Type;

                FractionDataTriggerEvent(pInfo.PlayerData);
            }
            else
            {
                pInfo.Fraction = Type;
            }

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

            MySQL.CharacterFractionAndRankUpdate(pInfo);

            TriggerEventToMembers("Fraction::UM", pInfo.CID);
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

        public void SendFractionChatMessage(string msg)
        {
            TriggerEventToMembers("Chat::ShowServerMessage", $"[Фракция] {msg}");
        }

        public Utils.Vector4 GetSpawnPosition(byte idx) => idx >= SpawnPositions.Length ? null : SpawnPositions[idx];
        public Utils.Vector4 GetCreationWorkbenchPosition(byte idx) => idx >= CreationWorkbenchPositions.Length ? null : CreationWorkbenchPositions[idx];
        public Utils.Vector4 GetContainerPosition(byte idx) => idx >= ContainerPositions.Length ? null : ContainerPositions[idx];

        public static bool IsFractionGov(Types type) => type >= Types.COP_BLAINE;
    }
}
