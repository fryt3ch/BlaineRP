using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
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

                        List<AccountData.GlobalBan> result = new List<AccountData.GlobalBan>();

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

        public static List<PlayerData.Punishment> GetPunishmentsByCID(uint cid)
        {
            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM punishments WHERE CID=@CID";
                    cmd.Parameters.AddWithValue("@CID", cid);

                    using (var reader = cmd.ExecuteReader())
                    {
                        var result = new List<PlayerData.Punishment>();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                                result.Add(new PlayerData.Punishment((int)reader[0], (PlayerData.Punishment.Types)reader[2], (string)reader[5], (DateTime)reader[3], (DateTime)reader[4], (int)reader[6]));
                        }

                        return result;
                    }
                }
            }
        }
    }
}
