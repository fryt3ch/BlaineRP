using GTANetworkAPI;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.EntitiesData.Players;
using BlaineRP.Server.EntitiesData.Vehicles;
using BlaineRP.Server.Extensions.System;
using BlaineRP.Server.Game.Management;
using BlaineRP.Server.Game.Management.Punishments;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Fractions
{
    public abstract partial class Fraction
    {
        public static TimeSpan VehicleRespawnCooldownTime { get; } = TimeSpan.FromMinutes(2);

        public static Dictionary<FractionType, Fraction> All { get; set; } = new Dictionary<FractionType, Fraction>();

        public static Fraction Get(FractionType type) => All.GetValueOrDefault(type);

        public FractionType Type { get; set; }

        public string Name { get; set; }

        public uint Materials { get => Utils.ToUInt32(World.Service.GetSharedData<object>($"FRAC::M_{(int)Type}")); set => World.Service.SetSharedData($"FRAC::M_{(int)Type}", value); }

        public ulong Balance { get; set; }

        public bool ContainerLocked { get => World.Service.GetSharedData<bool?>($"FRAC::SL_{(int)Type}") ?? false; set { if (value) World.Service.SetSharedData($"FRAC::SL_{(int)Type}", true); else World.Service.ResetSharedData($"FRAC::SL_{(int)Type}"); } }

        public bool CreationWorkbenchLocked { get => World.Service.GetSharedData<bool?>($"FRAC::CWL_{(int)Type}") ?? false; set { if (value) World.Service.SetSharedData($"FRAC::CWL_{(int)Type}", true); else World.Service.ResetSharedData($"FRAC::CWL_{(int)Type}"); } }

        public uint ContainerId { get; set; }

        public Game.Items.Container Container => Game.Items.Container.Get(ContainerId);

        public Vector4[] ContainerPositions { get; set; }

        public Vector4[] CreationWorkbenchPositions { get; set; }

        public Dictionary<string, uint> CreationWorkbenchPrices { get; set; }

        public NewsData News { get; set; }

        public Dictionary<VehicleInfo, VehicleProps> AllVehicles { get; set; }

        public List<PlayerInfo> AllMembers { get; set; } = new List<PlayerInfo>();

        public List<PlayerInfo> MembersOnline => AllMembers.Where(x => x.PlayerData != null).ToList();

        public PlayerInfo Leader { get; set; }

        public List<RankData> Ranks { get; set; }

        public Vector4[] SpawnPositions { get; set; }

        public abstract string ClientData { get; }

        public string ItemTag { get; set; }

        public FlagTypes MetaFlags { get; set; }

        public void SetRankName(byte rank, string rankName, bool updateDb)
        {
            Ranks[rank].Name = rankName;

            World.Service.SetSharedData($"FRAC::RN_{(int)Type}_{rank}", rankName);

            if (updateDb)
                MySQL.FractionUpdateRanks(this);
        }

        public void SetVehicleMinRank(VehicleInfo vInfo, VehicleProps vProps, byte minRank, bool updateDb)
        {
            vProps.MinimalRank = minRank;

            TriggerEventToMembers("Fraction::UVEHMR", vInfo.VID, minRank);

            if (updateDb)
                MySQL.FractionUpdateVehicles(this);
        }

        public void SetLeader(PlayerInfo pInfo, bool onStart)
        {
            if (onStart)
            {
                Leader = pInfo;

                if (pInfo == null)
                {
                    World.Service.ResetSharedData($"FRAC::L_{(int)Type}");
                }
                else
                {
                    World.Service.SetSharedData($"FRAC::L_{(int)Type}", pInfo.CID);
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

        public bool HasMemberPermission(PlayerInfo pInfo, uint permissionId, bool notify)
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

        public bool IsLeader(PlayerInfo pInfo, bool notify)
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

        public bool IsLeaderOrWarden(PlayerInfo pInfo, bool notify)
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

        public virtual void OnMemberStatusChange(PlayerInfo pInfo, byte status)
        {
            TriggerEventToMembers("Fraction::UMS", pInfo.CID, status);
        }

        public virtual void SetPlayerRank(PlayerInfo pInfo, byte rank)
        {
            pInfo.FractionRank = rank;

            TriggerEventToMembers("Fraction::UMR", pInfo.CID, rank);

            MySQL.CharacterFractionAndRankUpdate(pInfo);
        }

        protected virtual void FractionDataTriggerEvent(PlayerData pData)
        {
            pData.Player.TriggerEvent("Player::SCF", (int)Type, News.SerializeToJson(), AllVehicles.Select(x => $"{x.Key.VID}&{x.Key.VID}&{x.Value.MinimalRank}").ToList(), AllMembers.Select(x => $"{x.CID}&{x.Name} {x.Surname}&{x.FractionRank}&{(x.IsOnline ? 1 : 0)}&{GetMemberStatus(x)}&{x.LastJoinDate.GetUnixTimestamp()}").ToList());
        }

        public virtual void SetPlayerFraction(PlayerInfo pInfo, byte rank)
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

        public virtual void SetPlayerNoFraction(PlayerInfo pInfo)
        {
            if (pInfo.PlayerData != null)
            {
                pInfo.PlayerData.Fraction = FractionType.None;

                pInfo.PlayerData.Player.TriggerEvent("Player::SCF");
            }
            else
            {
                pInfo.Fraction = FractionType.None;
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

        public Fraction(FractionType Type, string Name)
        {
            this.Type = Type;

            this.Name = Name;

            All.Add(Type, this);
        }

        public static bool IsMember(PlayerData pData, FractionType type, bool notify)
        {
            if (type == FractionType.None || pData.Fraction != type)
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
            if (pData.Fraction == FractionType.None)
            {
                if (notify)
                {
                    pData.Player.Notify("Fraction::NMA");
                }

                return false;
            }

            return true;
        }

        public virtual byte GetMemberStatus(PlayerInfo pInfo)
        {
            byte status = 0;

            foreach (var x in pInfo.Punishments)
            {
                if (x.Type == PunishmentType.Ban)
                {
                    if (x.IsActive())
                        return 1;
                }
                else if (x.Type == PunishmentType.NRPPrison)
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

        public Vector4 GetSpawnPosition(byte idx) => idx >= SpawnPositions.Length ? null : SpawnPositions[idx];
        public Vector4 GetCreationWorkbenchPosition(byte idx) => idx >= CreationWorkbenchPositions.Length ? null : CreationWorkbenchPositions[idx];
        public Vector4 GetContainerPosition(byte idx) => idx >= ContainerPositions.Length ? null : ContainerPositions[idx];

        public static bool IsFractionGov(FractionType type) => type >= FractionType.COP_BLAINE;
    }
}
