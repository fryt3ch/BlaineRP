using MySqlConnector;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace BCRPServer
{
    public static partial class MySQL
    {
        public static async Task<(AuthResultTypes Result, AccountData AccountData)> AccountAdd(string scid, string hwid, string login, string password, string mail, DateTime currentDate, string ip)
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
                                return (AuthResultTypes.RegMailNotFree, null);
                        }

                        cmd.CommandText = "SELECT ID FROM accounts WHERE Login=@Login LIMIT 1";
                        cmd.Parameters.AddWithValue("@Login", login);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                                return (AuthResultTypes.RegLoginNotFree, null);
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

                        return (AuthResultTypes.OK, new AccountData() { AdminLevel = -1, BCoins = 0, HWID = hwid, ID = id, LastIP = ip, Login = login, Mail = mail, Password = password, RegistrationDate = currentDate, RegistrationIP = ip, SCID = ip });
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

        public static async Task<(byte, AccountData)> AccountAddTest(string scid, string hwid, string login, string password, string mail, string ip)
        {
            byte result = 255;
            AccountData aData = null;

            try
            {
                await Web.Service.Connection.EmitAsync("Auth::Account::Add", (resp) =>
                {
                    var data = JObject.Parse(resp.GetValue(0).GetString());

                    result = Convert.ToByte(data.GetValue("status"));

                    if (result == 0)
                    {
                        var aDataJson = JObject.Parse((string)data.GetValue("res"));

                        aData = new AccountData()
                        {
                            ID = Convert.ToUInt32(aDataJson["ID"]),
                            SCID = scid,
                            HWID = hwid,
                            Login = login,
                            Password = password,
                            Mail = mail,
                            RegistrationDate = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64((aDataJson["RegDate"]))).DateTime,
                            RegistrationIP = ip,
                            LastIP = ip,
                            AdminLevel = -1,
                            BCoins = 0,
                        };
                    }
                }, scid, hwid, login, password, mail, ip);
            }
            catch (Exception ex)
            {
                result = 255;
                aData = null;
            }

            return (result, aData);
        }

        public static async Task<(byte, AccountData)> AccountGetTest(ulong socialClubId)
        {
            byte result = 255;
            AccountData aData = null;

            try
            {
                await Web.Service.Connection.EmitAsync("Auth::Account::Get", (resp) =>
                {
                    var data = JObject.Parse(resp.GetValue(0).GetString());

                    result = Convert.ToByte(data.GetValue("status"));

                    if (result == 0)
                    {
                        var aDataJson = JObject.Parse((string)data.GetValue("adata"));

                        aData = new AccountData()
                        {
                            ID = Convert.ToUInt32(aDataJson["ID"]),
                            SCID = socialClubId.ToString(),
                            HWID = (string)aDataJson["HWID"],
                            Login = (string)aDataJson["Login"],
                            Password = (string)aDataJson["Password"],
                            Mail = (string)aDataJson["Mail"],
                            RegistrationDate = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64((aDataJson["RegDate"]))).DateTime,
                            RegistrationIP = (string)aDataJson["RegIP"],
                            LastIP = (string)aDataJson["LastIP"],
                            AdminLevel = (int)aDataJson["AdminLevel"],
                            BCoins = Convert.ToInt32(aDataJson["BCoins"]),
                        };
                    }
                }, socialClubId);
            }
            catch (Exception ex)
            {
                result = 255;
                aData = null;
            }

            return (result, aData);
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
