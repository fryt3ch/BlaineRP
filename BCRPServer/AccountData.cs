using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer
{
    class AccountData
    {
        public Player Player;
        public int ID { get; set; }
        public string SCID { get; set; }
        public string HWID { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Mail { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string RegistrationIP { get; set; }
        public string LastIP { get; set; }
        public int AdminLevel { get; set; }
        public int BCoins { get; set; }

        public AccountData(Player Player)
        {
            this.Player = Player;
        }

        public AccountData() { }

        public void Remove()
        {
            if (Player == null)
                return;

            Players.Remove(Player);
        }

        public void Delete()
        {

        }

        private static Dictionary<Player, AccountData> Players = new Dictionary<Player, AccountData>();

        public static AccountData Get(Player player)
        {
            if (player == null)
                return null;

            return Players.GetValueOrDefault(player);
        }

        public static void Set(Player player, AccountData data)
        {
            if (player == null)
                return;

            AccountData existing;

            if (Players.TryGetValue(player, out existing))
                existing = data;
            else
                Players.Add(player, data);
        }

        public class GlobalBan
        {
            public enum Types
            {
                IP = 0, HWID, SCID
            }

            public Types Type;
            public DateTime Date;
            public string Reason;
            public int AdminID;

            public GlobalBan(Types Type, DateTime Date, string Reason, int AdminID)
            {
                this.Type = Type;
                this.Date = Date;
                this.Reason = Reason;
                this.AdminID = AdminID;
            }
        }
    }
}
