using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace BCRPServer
{
    public static partial class MySQL
    {
        public static async Task<List<AccountData.GlobalBan>> GlobalBansGet(string hwid, string ip, ulong scId)
        {
            return await Task.Run(() =>
            {
                using (var conn = new MySqlConnection(GlobalConnectionCredentials))
                {
                    conn.Open();

                    using (MySqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT * FROM ip_blacklist WHERE IP=@IP LIMIT 1";
                        cmd.Parameters.AddWithValue("@IP", ip);

                        var result = new List<AccountData.GlobalBan>();

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                result.Add(new AccountData.GlobalBan(Convert.ToUInt32(reader[0]), AccountData.GlobalBan.Types.IP, (DateTime)reader[2], (string)reader[3], (int)reader[4]));
                        }

                        cmd.CommandText = "SELECT * FROM hwid_blacklist WHERE HWID=@HWID LIMIT 1";
                        cmd.Parameters.AddWithValue("@HWID", hwid);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                result.Add(new AccountData.GlobalBan(Convert.ToUInt32(reader[0]), AccountData.GlobalBan.Types.HWID, (DateTime)reader[2], (string)reader[3], (int)reader[4]));
                        }

                        cmd.CommandText = "SELECT * FROM scid_blacklist WHERE SCID=@SCID LIMIT 1";
                        cmd.Parameters.AddWithValue("@SCID", scId.ToString());

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                                result.Add(new AccountData.GlobalBan(Convert.ToUInt32(reader[0]), AccountData.GlobalBan.Types.SCID, (DateTime)reader[2], (string)reader[3], (int)reader[4]));
                        }

                        return result;
                    }
                }
            });
        }

        public static void AddPunishment(PlayerData.PlayerInfo punishedInfo, PlayerData.PlayerInfo punisherInfo, Sync.Punishment punishment)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = $"INSERT INTO punishments (ID, CID, Type, StartDate, EndDate, Reason, PunisherID, Data) VALUES (@ID, @CID, @T, @SD, @ED, @R, @PCID, @Data);";

            cmd.Parameters.AddWithValue("@ID", punishment.Id);
            cmd.Parameters.AddWithValue("@CID", punishedInfo.CID);
            cmd.Parameters.AddWithValue("@PCID", punisherInfo.CID);
            cmd.Parameters.AddWithValue("@T", (int)punishment.Type);
            cmd.Parameters.AddWithValue("@SD", punishment.StartDate);
            cmd.Parameters.AddWithValue("@ED", punishment.EndDate);
            cmd.Parameters.AddWithValue("@R", punishment.Reason);

            if (punishment.AdditionalData == null)
                cmd.Parameters.AddWithValue("@Data", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@Data", punishment.AdditionalData);

            PushQuery(cmd);
        }

        public static void UpdatePunishmentAmnesty(Sync.Punishment punishment)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = $"UPDATE punishments SET Amnesty=@Amnesty WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", punishment.Id);

            if (punishment.AmnestyInfo == null)
                cmd.Parameters.AddWithValue("@Amnesty", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@Amnesty", punishment.AmnestyInfo.SerializeToJson());

            PushQuery(cmd);
        }

        public static void RemovePunishment(uint id)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = $"DELETE FROM punishments WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", id);

            PushQuery(cmd);
        }
    }
}
