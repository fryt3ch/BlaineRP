using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BCRPServer
{
    public static partial class MySQL
    {
        public static async Task<(AuthResults Result, AccountData AccountData)> AccountAdd(string scid, string hwid, string login, string password, string mail, DateTime currentDate, string ip)
        {
            return await Task.Run(() =>
            {
                using (var conn = new MySqlConnection(GlobalConnectionCredentials))
                {
                    conn.Open();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT ID FROM accounts WHERE Mail=@Mail LIMIT 1";
                        cmd.Parameters.AddWithValue("@Mail", mail);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                                return (AuthResults.RegMailNotFree, null);
                        }

                        cmd.CommandText = "SELECT ID FROM accounts WHERE Login=@Login LIMIT 1";
                        cmd.Parameters.AddWithValue("@Login", login);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                                return (AuthResults.RegLoginNotFree, null);
                        }
                    }

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "INSERT INTO accounts (SCID, HWID, Login, Password, Mail, RegDate, RegIP, LastIP, AdminLevel) VALUES (@SCID, @HWID, @Login, @Password, @Mail, @RegDate, @RegIP, @LastIP, @AdminLevel); SELECT LAST_INSERT_ID();";

                        cmd.Parameters.AddWithValue("@SCID", scid);
                        cmd.Parameters.AddWithValue("@HWID", hwid);
                        cmd.Parameters.AddWithValue("@Login", login);
                        cmd.Parameters.AddWithValue("@Password", password);
                        cmd.Parameters.AddWithValue("@Mail", mail);
                        cmd.Parameters.AddWithValue("@RegDate", currentDate);
                        cmd.Parameters.AddWithValue("@RegIP", ip);
                        cmd.Parameters.AddWithValue("@LastIP", ip);
                        cmd.Parameters.AddWithValue("@AdminLevel", -1);
                        cmd.Parameters.AddWithValue("@BCoins", 0);

                        cmd.ExecuteNonQuery();

                        var id = Convert.ToUInt32(cmd.LastInsertedId);

                        return (AuthResults.RegOk, new AccountData() { AdminLevel = -1, BCoins = 0, HWID = hwid, ID = id, LastIP = ip, Login = login, Mail = mail, Password = password, RegistrationDate = currentDate, RegistrationIP = ip, SCID = ip });
                    }
                }
            });
        }

        public static async Task<AccountData> AccountGet(ulong socialClubId)
        {
            return await Task.Run(() =>
            {
                using (var conn = new MySqlConnection(GlobalConnectionCredentials))
                {
                    conn.Open();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT * FROM accounts WHERE SCID=@SCID LIMIT 1";
                        cmd.Parameters.AddWithValue("@SCID", socialClubId.ToString());

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                return null;
                            }
                            else
                            {
                                reader.Read();

                                return new AccountData()
                                {
                                    ID = Convert.ToUInt32(reader[0]), // AID
                                    SCID = (string)reader[1], // SCID
                                    HWID = (string)reader[2], // HWID
                                    Login = (string)reader[3], // Login
                                    Password = (string)reader[4], // Password
                                    Mail = (string)reader[5], // Password
                                    RegistrationDate = (DateTime)reader[6], // RegDate
                                    RegistrationIP = (string)reader[7], // RegIP
                                    LastIP = (string)reader[8], // LastIP
                                    AdminLevel = (int)reader[9], // AdminLevel
                                    BCoins = (int)reader[10], // BCoins
                                };
                            }
                        }
                    }
                }
            });
        }

        public static void AccountUpdateOnEnter(AccountData aData)
        {
            using (var conn = new MySqlConnection(GlobalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE accounts SET LastIP=@LastIP WHERE ID=@ID";

                    cmd.Parameters.AddWithValue("@ID", aData.ID);
                    cmd.Parameters.AddWithValue("@LastIP", aData.LastIP);

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
