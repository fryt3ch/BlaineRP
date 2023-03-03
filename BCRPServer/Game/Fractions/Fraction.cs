using GTANetworkAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace BCRPServer.Game.Fractions
{
    public enum Types
    {
        None = 0,

        PolicePaleto,
    }

    public abstract class Fraction
    {
        public class RankData
        {
            [JsonProperty(PropertyName = "N")]
            public string Name { get; set; }

            /// <summary>Стандарные Id прав для всех фракций</summary>
            /// <remarks>0 - Доступ к складу, 1 - Приглашение, 2 - Повышение/понижение<br/>3 - Увольнение, 4 - Респавн транспорта</remarks>
            [JsonProperty(PropertyName = "P")]
            public Dictionary<uint, bool> Permissions { get; set; }
        }

        public class VehicleProps
        {
            [JsonProperty(PropertyName = "R")]
            public byte MinimalRank { get; set; }
        }

        public class NewsData
        {
            [JsonProperty(PropertyName = "A")]
            public Dictionary<int, string> All { get; set; }

            [JsonProperty(PropertyName = "P")]
            public int PinnedId { get; set; }
        }

        public static Dictionary<Types, Fraction> All { get; set; } = new Dictionary<Types, Fraction>();

        public Types Type { get; set; }

        public string Name { get; set; }

        public uint Materials { get; set; }

        public ulong Balance { get; set; }

        public bool StorageLocked { get; set; }

        public NewsData News { get; set; }

        public Dictionary<VehicleData.VehicleInfo, VehicleProps> AllVehicles { get; set; }

        public List<PlayerData.PlayerInfo> AllMembers { get; set; } = new List<PlayerData.PlayerInfo>();

        public List<PlayerData.PlayerInfo> MembersOnline => AllMembers.Where(x => x.PlayerData != null).ToList();

        public PlayerData.PlayerInfo Leader { get; set; }

        public List<RankData> Ranks { get; set; }

        public Vector3 MainPosition { get; set; }

        public void TriggerEventToMembers(string eventName, params object[] args)
        {
            var t = PlayerData.All.Where(x => x.Value.Fraction == Type).Select(x => x.Key).ToArray();

            if (t.Length == 0)
                return;

            NAPI.ClientEvent.TriggerClientEventToPlayers(t, eventName, args);
        }

        public bool HasMemberPermission(PlayerData.PlayerInfo pInfo, uint permissionId)
        {
            if (pInfo == Leader)
                return true;

            if (pInfo.FractionRank >= Ranks.Count)
                return false;

            return Ranks[pInfo.FractionRank].Permissions[permissionId];
        }

        public virtual void SetPlayerFraction(PlayerData.PlayerInfo pInfo, byte rank = 0)
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

        public Fraction(Types Type, string Name, Vector3 MainPosition)
        {
            this.Type = Type;

            this.Name = Name;

            All.Add(Type, this);

            this.MainPosition = MainPosition;
        }
    }
}
