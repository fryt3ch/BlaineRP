using BCRPServer.Game.Bank;
using BCRPServer.Game.Houses;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BCRPServer
{
    static class MySQL
    {
        private const string Host = "localhost";
        private const string User = "root";
        private const string Password = "";
        private const string GlobalDatabase = "bcrp";
        private const string LocalDatabase = "bcrp-1";

        private static string GlobalConnectionCredentials = $"SERVER={Host}; DATABASE={GlobalDatabase}; UID={User}; PASSWORD={Password}";
        private static string LocalConnectionCredentials = $"SERVER={Host}; DATABASE={LocalDatabase}; UID={User}; PASSWORD={Password}";

        private static SemaphoreSlim LocalConnectionSemaphore { get; set; }
        private static SemaphoreSlim GlobalConnectionSemaphore { get; set; }

        #region General

        #region Init Connection
        public static bool InitConnection()
        {
            MySqlConnection globalConnection = new MySqlConnection(GlobalConnectionCredentials);
            MySqlConnection localConnection = new MySqlConnection(LocalConnectionCredentials);

            GlobalConnectionSemaphore = new SemaphoreSlim(1, 1);
            LocalConnectionSemaphore = new SemaphoreSlim(1, 1);

            try
            {
                globalConnection.Open();
                localConnection.Open();

                return true;
            }
            catch (MySqlException ex)
            {
                Utils.ConsoleOutput(ex.Message);

                Environment.Exit(1);

                return false;
            }
        }
        #endregion

        #region Set Offline All
        public static int SetOfflineAll()
        {
            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE characters SET IsOnline=false";

                    return cmd.ExecuteNonQuery();
                }
            }
        }
        #endregion

        public static async Task WaitGlobal() => await GlobalConnectionSemaphore.WaitAsync();
        public static void ReleaseGlobal() => GlobalConnectionSemaphore.Release();

        public static async Task Wait() => await LocalConnectionSemaphore.WaitAsync();
        public static void Release() => LocalConnectionSemaphore.Release();

        #region Load Server Data
        public static void LoadPlayersInfo()
        {
            if (ServerEvents.AllPlayers != null)
                return;

            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT auto_increment FROM INFORMATION_SCHEMA.TABLES WHERE table_schema='{LocalDatabase}' AND table_name='characters';";

                    int nextAi = 0;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();

                            nextAi = Convert.ToInt32(reader[0]);
                        }
                    }

                    if (nextAi < Utils.FirstCID)
                    {
                        cmd.CommandText = $"ALTER TABLE chatacters AUTO_INCREMENT = {Utils.FirstCID};";

                        cmd.ExecuteNonQuery();
                    }

                    cmd.CommandText = $"SELECT * FROM characters;";

                    ServerEvents.AllPlayers = new Dictionary<int, PlayerData.PlayerInfo>();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                                ServerEvents.AllPlayers.Add(Convert.ToInt32(reader["CID"]), new PlayerData.PlayerInfo(Convert.ToInt32(reader["CID"]), (string)reader["Name"], (string)reader["Surname"], (DateTime)reader["LastJoinDate"], (PlayerData.FractionTypes)Convert.ToInt32(reader["Fraction"]), Convert.ToInt32(reader["OrgID"]), GetPunishmentsByCID(Convert.ToInt32(reader["CID"]))));
                        }
                    }
                }
            }
        }

        public static int LoadFreeItemsUIDs()
        {
            if (ServerEvents.FreeItemUIDs != null)
                return 0;

            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM items;";

                    ServerEvents.FreeItemUIDs = new Queue<uint>();

                    var allItems = new Dictionary<uint, List<uint>>();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                            while (reader.Read())
                            {
                                var uid = Convert.ToUInt32(reader["UID"]);

                                if (allItems.ContainsKey(uid))
                                    continue;
                                else if (allItems.Where(x => x.Value?.Contains(uid) == true).Any())
                                    continue;

                                if (reader["Items"] != DBNull.Value)
                                {
                                    var list = ((string)reader["Items"]).DeserializeFromJson<List<uint>>();

                                    list.RemoveAll(x => x == 0);

                                    foreach (var x in list)
                                    {
                                        if (allItems.ContainsKey(x))
                                            allItems.Remove(x);
                                    }

                                    allItems.Add(uid, list);
                                }
                                else
                                {
                                    allItems.Add(uid, null);
                                }
                            }
                    }

                    allItems.Remove(0);

                    cmd.CommandText = $"SELECT auto_increment FROM INFORMATION_SCHEMA.TABLES WHERE table_schema='{LocalDatabase}' AND table_name='items';";

                    uint nextAi = 1;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();

                            nextAi = Convert.ToUInt32(reader[0]);
                        }
                    }

                    cmd.CommandText = "SELECT * FROM inventories;";

                    var usedItems = new List<uint>();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                usedItems.AddRange(((string)reader["Items"]).DeserializeFromJson<uint[]>());
                                usedItems.AddRange(((string)reader["Clothes"]).DeserializeFromJson<uint[]>());
                                usedItems.AddRange(((string)reader["Accessories"]).DeserializeFromJson<uint[]>());
                                usedItems.AddRange(((string)reader["Weapons"]).DeserializeFromJson<uint[]>());

                                usedItems.Add(Convert.ToUInt32(reader["Holster"]));
                                usedItems.Add(Convert.ToUInt32(reader["Armour"]));
                                usedItems.Add(Convert.ToUInt32(reader["Bag"]));
                            }
                        }
                    }

                    cmd.CommandText = "SELECT Items FROM containers;";

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                usedItems.AddRange(((string)reader["Items"]).DeserializeFromJson<uint[]>());
                            }
                        }
                    }

                    usedItems.RemoveAll(x => x == 0);

                    foreach (var x in GetItems(usedItems.ToArray()))
                    {
                        //Console.WriteLine(x.Type);

                        if (x is Game.Items.Numberplate np)
                        {
                            ServerEvents.UsedNumberplates.Add(np.Tag);
                        }
                        else if (x is Game.Items.Weapon weapon)
                        {
                            ServerEvents.UsedWeaponTags.Add(weapon.Tag);
                        }
                    }

                    var toDelete = new List<uint>();

                    foreach (var x in allItems.Keys.Except(usedItems))
                    {
                        toDelete.Add(x);

                        if (allItems[x] != null)
                        {
                            toDelete.AddRange(allItems[x]);
                        }

                        allItems.Remove(x);
                    }

                    if (toDelete.Count > 0)
                    {
                        cmd.CommandText = $"DELETE FROM Items WHERE UID IN ({string.Join(", ", toDelete)})";

                        cmd.ExecuteNonQuery();
                    }

                    List<uint> temp = new List<uint>();

                    for (uint i = 1; i < nextAi; i++)
                        temp.Add(i);

                    foreach (var x in temp.Where(x => !allItems.ContainsKey(x) && !allItems.Where(y => y.Value?.Contains(x) == true).Any()))
                        ServerEvents.FreeItemUIDs.Enqueue(x);

                    return toDelete.Count;
                }
            }
        }

        public static void LoadFreeContainersUIDs()
        {
            if (ServerEvents.FreeContainersIDs != null)
                return;

            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM containers;";

                    Game.Items.Container.AllContainers = new Dictionary<uint, Game.Items.Container>();
                    ServerEvents.FreeContainersIDs = new Queue<uint>();

                    var allContainers = new List<uint>();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                            while (reader.Read())
                            {
                                var id = Convert.ToUInt32(reader["ID"]);
                                var itemsIds = ((string)reader["Items"]).DeserializeFromJson<uint[]>();

                                //Game.Items.Container.AllContainers.Add(id, new Game.Items.Container((string)reader["SID"]) { ID = id, Items = await GetItems(itemsIds) });

                                allContainers.Add(id);
                            }
                    }

                    cmd.CommandText = $"SELECT auto_increment FROM INFORMATION_SCHEMA.TABLES WHERE table_schema='{LocalDatabase}' AND table_name='containers';";

                    uint nextAi = 0;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();

                            nextAi = Convert.ToUInt32(reader[0]);
                        }
                    }

                    List<uint> temp = new List<uint>();

                    for (uint i = 1; i < nextAi; i++)
                        temp.Add(i);

                    foreach (var x in temp.Where(x => !allContainers.Contains(x)))
                        ServerEvents.FreeContainersIDs.Enqueue(x);
                }
            }
        }

        public static void LoadFreeVehiclesUIDs()
        {
            if (ServerEvents.FreeVehiclesIDs != null)
                return;

            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT VID FROM vehicles;";

                    ServerEvents.FreeVehiclesIDs = new Queue<int>();

                    var allItems = new List<int>();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            while (reader.Read())
                                allItems.Add(Convert.ToInt32(reader["VID"]));
                    }

                    cmd.CommandText = $"SELECT auto_increment FROM INFORMATION_SCHEMA.TABLES WHERE table_schema='{LocalDatabase}' AND table_name='vehicles';";

                    int nextAi = 0;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return;

                        reader.Read();

                        nextAi = Convert.ToInt32(reader[0]);
                    }

                    if (nextAi < Utils.FirstCID)
                    {
                        cmd.CommandText = $"ALTER TABLE vehicles AUTO_INCREMENT = {Utils.FirstVID};";

                        cmd.ExecuteNonQuery();
                    }

                    List<int> temp = new List<int>();

                    for (int i = Utils.FirstVID; i < nextAi; i++)
                        temp.Add(i);

                    foreach (var x in temp.Where(x => !allItems.Contains(x)))
                        ServerEvents.FreeVehiclesIDs.Enqueue(x);
                }
            }
        }
        #endregion

        #endregion

        #region Auth Stuff
        #region Is Login Free
        public static bool IsLoginFree(string login)
        {
            using (var conn = new MySqlConnection(GlobalConnectionCredentials))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT AID FROM accounts WHERE Login=@Login LIMIT 1";
                    cmd.Parameters.AddWithValue("@Login", login);

                    using (var reader = cmd.ExecuteReader())
                    {
                        return !reader.HasRows;
                    }
                }
            }
        }
        #endregion

        #region Is Mail Free
        public static bool IsMailFree(string mail)
        {
            using (var conn = new MySqlConnection(GlobalConnectionCredentials))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT AID FROM accounts WHERE Mail=@Mail LIMIT 1";
                    cmd.Parameters.AddWithValue("@Mail", mail);

                    using (var reader = cmd.ExecuteReader())
                    {
                        return !reader.HasRows;
                    }
                }
            }
        }
        #endregion
        #endregion

        #region Account

        #region Create
        public static int AddNewAccount(AccountData Account)
        {
            using (var conn = new MySqlConnection(GlobalConnectionCredentials))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO accounts (SCID, HWID, Login, Password, Mail, RegDate, RegIP, LastIP, AdminLevel) VALUES (@SCID, @HWID, @Login, @Password, @Mail, @RegDate, @RegIP, @LastIP, @AdminLevel); SELECT LAST_INSERT_ID();";

                    cmd.Parameters.AddWithValue("@SCID", Account.SCID);
                    cmd.Parameters.AddWithValue("@HWID", Account.HWID);
                    cmd.Parameters.AddWithValue("@Login", Account.Login);
                    cmd.Parameters.AddWithValue("@Password", Account.Password);
                    cmd.Parameters.AddWithValue("@Mail", Account.Mail);
                    cmd.Parameters.AddWithValue("@RegDate", Account.RegistrationDate);
                    cmd.Parameters.AddWithValue("@RegIP", Account.RegistrationIP);
                    cmd.Parameters.AddWithValue("@LastIP", Account.LastIP);
                    cmd.Parameters.AddWithValue("@AdminLevel", Account.AdminLevel);
                    cmd.Parameters.AddWithValue("@BCoins", Account.BCoins);

                    cmd.ExecuteNonQuery();

                    return Convert.ToInt32(cmd.LastInsertedId);
                }
            }
        }
        #endregion

        #region Get
        public static AccountData GetPlayerAccount(ulong socialClubId)
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
                                ID = (int)reader[0], // AID
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
        }
        #endregion

        #region Update
        public static void UpdateAccountOnEnter(AccountData aData)
        {
            using (var conn = new MySqlConnection(GlobalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE accounts SET LastIP=@LastIP WHERE AID=@AID";

                    cmd.Parameters.AddWithValue("@AID", aData.ID);
                    cmd.Parameters.AddWithValue("@LastIP", aData.LastIP);

                    cmd.ExecuteNonQuery();
                }
            }
        }
        #endregion

        #endregion

        #region Punishments

        #region Get Global Bans
        public static List<AccountData.GlobalBan> GetGlobalBans(string hwid, string ip, ulong scId)
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
                            result.Add(new AccountData.GlobalBan(AccountData.GlobalBan.Types.IP, (DateTime)reader[2], (string)reader[3], (int)reader[4]));
                    }

                    cmd.CommandText = "SELECT * FROM hwid_blacklist WHERE HWID=@HWID LIMIT 1";
                    cmd.Parameters.AddWithValue("@HWID", hwid);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            result.Add(new AccountData.GlobalBan(AccountData.GlobalBan.Types.HWID, (DateTime)reader[2], (string)reader[3], (int)reader[4]));
                    }

                    cmd.CommandText = "SELECT * FROM scid_blacklist WHERE SCID=@SCID LIMIT 1";
                    cmd.Parameters.AddWithValue("@SCID", scId.ToString());

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            result.Add(new AccountData.GlobalBan(AccountData.GlobalBan.Types.SCID, (DateTime)reader[2], (string)reader[3], (int)reader[4]));
                    }

                    return result;
                }
            }
        }
        #endregion

        #region Get Character Punishments
        public static List<PlayerData.Punishment> GetPunishmentsByCID(int cid)
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
        #endregion
        #endregion

        #region Character

        #region Create
        public static int AddNewCharacter(PlayerData pData, AccountData aData)
        {
            if (aData == null || pData == null)
                return -1;

            int cid = -1;

            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO characters (AID, CreationDate, AdminLevel, LastJoinDate, IsOnline, TimePlayed, " +
                    "Name, Surname, Sex, BirthDate, Licenses, Fraction, OrgID, Cash, BID, LastData, " +
                    "Satiety, Mood, Familiars, Skills) " +
                    "VALUES (@AID, @CreationDate, @AdminLevel, @LastJoinDate, @IsOnline, @TimePlayed, " +
                    "@Name, @Surname, @Sex, @BirthDate, @Licenses, @Fraction, @OrgID, @Cash, @BID, @LastData, " +
                    "@Satiety, @Mood, @Familiars, @Skills); ";

                    cmd.Parameters.AddWithValue("@AID", aData.ID);

                    cmd.Parameters.AddWithValue("@IsOnline", true);
                    cmd.Parameters.AddWithValue("@TimePlayed", 0);
                    cmd.Parameters.AddWithValue("@Fraction", PlayerData.FractionTypes.None);
                    cmd.Parameters.AddWithValue("@OrgID", -1);
                    cmd.Parameters.AddWithValue("@BID", -1);

                    cmd.Parameters.AddWithValue("@CreationDate", pData.CreationDate);
                    cmd.Parameters.AddWithValue("@AdminLevel", -1);
                    cmd.Parameters.AddWithValue("@LastJoinDate", pData.LastJoinDate);
                    cmd.Parameters.AddWithValue("@Cash", Settings.CHARACTER_DEFAULT_MONEY_CASH);
                    cmd.Parameters.AddWithValue("@LastData", pData.LastData.SerializeToJson());
                    cmd.Parameters.AddWithValue("@Satiety", Settings.CHARACTER_DEFAULT_SATIETY);
                    cmd.Parameters.AddWithValue("@Mood", Settings.CHARACTER_DEFAULT_MOOD);

                    cmd.Parameters.AddWithValue("@Name", pData.Name);
                    cmd.Parameters.AddWithValue("@Surname", pData.Surname);
                    cmd.Parameters.AddWithValue("@Sex", pData.Sex);
                    cmd.Parameters.AddWithValue("@BirthDate", pData.BirthDate);
                    cmd.Parameters.AddWithValue("@Licenses", JsonConvert.SerializeObject(pData.Licenses));

                    cmd.Parameters.AddWithValue("@Skills", JsonConvert.SerializeObject(pData.Skills));

                    cmd.Parameters.AddWithValue("@Familiars", JsonConvert.SerializeObject(pData.Familiars));

                    cmd.ExecuteNonQuery();

                    cid = Convert.ToInt32(cmd.LastInsertedId);

                    var pInfo = new PlayerData.PlayerInfo(cid, pData.Name, pData.Surname, pData.LastJoinDate, pData.Fraction, pData.OrganisationID, new List<PlayerData.Punishment>());

                    pData.Info = pInfo;

                    NAPI.Task.Run(() =>
                    {
                        ServerEvents.AllPlayers.Add(cid, pInfo);
                    });

                    cmd.CommandText = "INSERT INTO customizations (CID, HeadBlend, HeadOverlays, FaceFeatures, Decorations, HairStyle, EyeColor) VALUES (@CID, @HeadBlend, @HeadOverlays, @FaceFeatures, @Decorations, @HairStyle, @EyeColor); ";
                    cmd.CommandText += "INSERT INTO inventories (CID, Items, Clothes, Accessories, Bag, Holster, Weapons, Armour) VALUES (@CID, @Items, @Clothes, @Accessories, @Bag, @Holster, @Weapons, @Armour); ";

                    cmd.Parameters.AddWithValue("@CID", cid);

                    cmd.Parameters.AddWithValue("@Items", pData.Items.Select(x => x?.UID ?? 0).SerializeToJson());
                    cmd.Parameters.AddWithValue("@Clothes", pData.Clothes.Select(x => x?.UID ?? 0).SerializeToJson());
                    cmd.Parameters.AddWithValue("@Accessories", pData.Accessories.Select(x => x?.UID ?? 0).SerializeToJson());
                    cmd.Parameters.AddWithValue("@Weapons", pData.Weapons.Select(x => x?.UID ?? 0).SerializeToJson());
                    cmd.Parameters.AddWithValue("@Armour", pData.Armour?.UID ?? 0);
                    cmd.Parameters.AddWithValue("@Bag", pData.Bag?.UID ?? 0);
                    cmd.Parameters.AddWithValue("@Holster", pData.Holster?.UID ?? 0);

                    cmd.Parameters.AddWithValue("@HeadBlend", pData.HeadBlend.SerializeToJson());
                    cmd.Parameters.AddWithValue("@HeadOverlays", pData.HeadOverlays.SerializeToJson());
                    cmd.Parameters.AddWithValue("@FaceFeatures", pData.FaceFeatures.SerializeToJson());
                    cmd.Parameters.AddWithValue("@Decorations", pData.Decorations.SerializeToJson());
                    cmd.Parameters.AddWithValue("@HairStyle", pData.HairStyle.SerializeToJson());
                    cmd.Parameters.AddWithValue("@EyeColor", pData.EyeColor);

                    cmd.ExecuteNonQuery();
                }
            }

            return cid;
        }
        #endregion

        #region Update On Enter
        public static void UpdateCharacterOnEnter(PlayerData pData)
        {
            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                MySqlCommand cmd = conn.CreateCommand();

                cmd.CommandText = "UPDATE characters SET IsOnline=true, LastJoinDate=@LastJoinDate WHERE CID=@CID";

                var player = pData.Player;

                var res = NAPI.Task.RunAsync(() =>
                {
                    if (player?.Exists != true)
                        return false;

                    cmd.Parameters.AddWithValue("@CID", pData.CID);
                    cmd.Parameters.AddWithValue("@LastJoinDate", pData.LastJoinDate);

                    return true;
                }).GetAwaiter().GetResult();

                if (!res)
                    return;

                cmd.ExecuteNonQuery();
            }
        }
        #endregion

        #region Save On Exit
        public static void SaveCharacterOnExit(int cid, int timePlayed, int hp, uint dim, float heading, Vector3 pos, int sessionTime, bool knocked, int satiety, int mood)
        {
            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                MySqlCommand cmd = conn.CreateCommand();

                cmd.CommandText = "UPDATE characters SET IsOnline=false, TimePlayed=@TimePlayed, LastData=@LastData, Satiety=@Satiety, Mood=@Mood WHERE CID=@CID; ";
                //cmd.CommandText += "UPDATE inventories SET Items=@Items, Clothes=@Clothes, Accessories=@Accessories, Bag=@Bag, Holster=@Holster, Weapons=@Weapons, Armour=@Armour WHERE CID=@CID;";

                cmd.Parameters.AddWithValue("@CID", cid);
                cmd.Parameters.AddWithValue("@TimePlayed", timePlayed);
                cmd.Parameters.AddWithValue("@LastData", new PlayerData.LastPlayerData() { Health = hp, Dimension = dim, Heading = heading, Position = pos, SessionTime = sessionTime, Knocked = knocked }.SerializeToJson());
                cmd.Parameters.AddWithValue("@Satiety", satiety);
                cmd.Parameters.AddWithValue("@Mood", mood);

/*              cmd.Parameters.AddWithValue("@Items", data.Items.Select(x => x?.UID ?? 0).SerializeToJson());
                cmd.Parameters.AddWithValue("@Clothes", data.Clothes.Select(x => x?.UID ?? 0).SerializeToJson());
                cmd.Parameters.AddWithValue("@Accessories", data.Accessories.Select(x => x?.UID ?? 0).SerializeToJson());
                cmd.Parameters.AddWithValue("@Bag", data.Bag?.UID ?? 0);
                cmd.Parameters.AddWithValue("@Holster", data.Holster?.UID ?? 0);
                cmd.Parameters.AddWithValue("@Weapons", data.Weapons.Select(x => x?.UID ?? 0).SerializeToJson());
                cmd.Parameters.AddWithValue("@Armour", data.Armour?.UID ?? 0);*/
  
                cmd.ExecuteNonQuery();
            }
        }
        #endregion

        #region Get All Characters (by AID)
        public static PlayerData.Prototype[] GetAllCharacters(int aid)
        {
            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT CID, CreationDate, LastJoinDate, IsOnline, TimePlayed, Name, Surname, Sex, BirthDate, Fraction, OrgID, Cash, BID FROM characters WHERE AID=@AID LIMIT 3";

                    cmd.Parameters.AddWithValue("@AID", aid);

                    var result = new PlayerData.Prototype[3];

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return result;

                        int Counter = 0;

                        while (reader.Read())
                        {
                            int cid = (int)reader[0];

                            result[Counter++] = new PlayerData.Prototype()
                            {
                                CID = cid,
                                CreationDate = (DateTime)reader["CreationDate"],
                                LastJoinDate = (DateTime)reader["LastJoinDate"],
                                IsOnline = (bool)reader["IsOnline"],
                                TimePlayed = (int)reader["TimePlayed"],
                                Name = (string)reader["Name"],
                                Surname = (string)reader["Surname"],
                                Sex = (bool)reader["Sex"],
                                BirthDate = (DateTime)reader["BirthDate"],
                                Fraction = (PlayerData.FractionTypes)reader["Fraction"],
                                OrganisationID = (int)reader["OrgID"],
                                Cash = (int)reader["Cash"],
                                BankAccount = reader["BID"] != null ? GetBankAccountByCID(cid) : null,

                                Punishments = GetPunishmentsByCID(cid)
                            };
                        }

                        return result;
                    }
                }
            }
        }
        #endregion

        #region Get
        public static PlayerData GetCharacterByCID(this Player player, int CID)
        {
            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM characters WHERE CID=@CID; SELECT * FROM customizations WHERE CID=@CID; SELECT * FROM inventories WHERE CID=@CID; SELECT VID FROM vehicles WHERE CID=@CID; SELECT * FROM gifts WHERE CID=@CID;"; // LIMIT 1

                    cmd.Parameters.AddWithValue("@CID", CID);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return null;

                        reader.Read();

                        var CreationDate = (DateTime)reader["CreationDate"];
                        var AdminLevel = (int)reader["AdminLevel"];
                        var LastJoinDate = Utils.GetCurrentTime();
                        var TimePlayed = (int)reader["TimePlayed"];
                        var Name = (string)reader["Name"];
                        var Surname = (string)reader["Surname"];
                        var Sex = (bool)reader["Sex"];
                        var BirthDate = (DateTime)reader["BirthDate"];
                        var Licenses = JsonConvert.DeserializeObject<List<PlayerData.LicenseTypes>>((string)reader["Licenses"]);
                        var Fraction = (int)reader["Fraction"];
                        var OrganisationID = (int)reader["OrgID"];
                        var Cash = (int)reader["Cash"];
                        var BankAccount = (int)reader["BID"] != -1 ? GetBankAccountByCID(CID) : null;
                        var LastData = JsonConvert.DeserializeObject<PlayerData.LastPlayerData>((string)reader["LastData"]);
                        var Satiety = (int)reader["Satiety"];
                        var Mood = (int)reader["Mood"];

                        var Skills = JsonConvert.DeserializeObject<Dictionary<PlayerData.SkillTypes, int>>((string)reader["Skills"]);
                        var Familiars = JsonConvert.DeserializeObject<List<int>>((string)reader["Familiars"]);

                        if (!reader.NextResult())
                            return null;
                        else
                            reader.Read();

                        var HeadBlend = JsonConvert.DeserializeObject<HeadBlend>((string)reader["HeadBlend"]);
                        var HeadOverlays = JsonConvert.DeserializeObject<Dictionary<int, HeadOverlay>>((string)reader["HeadOverlays"]);
                        var FaceFeatures = JsonConvert.DeserializeObject<float[]>((string)reader["FaceFeatures"]);
                        var Decorations = JsonConvert.DeserializeObject<List<Decoration>>((string)reader["Decorations"]);
                        var EyeColor = (sbyte)reader["EyeColor"];
                        var HairStyle = JsonConvert.DeserializeObject<Game.Data.Customization.HairStyle>((string)reader["HairStyle"]);

                        if (!reader.NextResult())
                            return null;
                        else
                            reader.Read();

                        var Items = GetItems(JsonConvert.DeserializeObject<uint[]>((string)reader["Items"]));
                        var Clothes = GetItems(JsonConvert.DeserializeObject<uint[]>((string)reader["Clothes"]));
                        var Accessories = GetItems(JsonConvert.DeserializeObject<uint[]>((string)reader["Accessories"]));
                        var Bag = GetItem(Convert.ToUInt32(reader["Bag"]));
                        var Holster = GetItem(Convert.ToUInt32(reader["Holster"]));
                        var Weapons = GetItems(JsonConvert.DeserializeObject<uint[]>((string)reader["Weapons"]));
                        var Armour = GetItem(Convert.ToUInt32(reader["Armour"]));

                        var OwnedVehicles = new List<int>();

                        if (reader.NextResult())
                        {
                            while (reader.Read())
                                OwnedVehicles.Add((int)reader["VID"]);
                        }

                        var Gifts = new List<Game.Items.Gift>();

                        if (reader.NextResult())
                        {
                            while (reader.Read())
                                Gifts.Add(new Game.Items.Gift(CID, (int)reader["ID"], (Game.Items.Gift.SourceTypes)(int)reader["Reason"], (Game.Items.Gift.Types)(int)reader["Type"], reader["GID"] is DBNull ? null : (string)reader["GID"], (int)reader["Variation"], (int)reader["Amount"]));
                        }

                        var Punishments = GetPunishmentsByCID(CID);

                        return NAPI.Task.RunAsync<PlayerData>(() =>
                        {
                            if (player?.Exists != true)
                                return null;

                            var pData = new PlayerData(player)
                            {
                                CID = CID,
                                CreationDate = CreationDate,
                                AdminLevel = AdminLevel,
                                LastJoinDate = LastJoinDate,
                                TimePlayed = TimePlayed,
                                Name = Name,
                                Surname = Surname,
                                Sex = Sex,
                                BirthDate = BirthDate,
                                Licenses = Licenses,
                                Fraction = (PlayerData.FractionTypes)Fraction,
                                OrganisationID = OrganisationID,
                                Cash = Cash,
                                BankAccount = BankAccount,
                                LastData = LastData,
                                Satiety = Satiety,
                                Mood = Mood,

                                OwnedVehicles = OwnedVehicles,

                                Gifts = Gifts,

                                HeadBlend = HeadBlend,
                                HeadOverlays = HeadOverlays,
                                FaceFeatures = FaceFeatures,
                                Decorations = Decorations,
                                EyeColor = (byte)EyeColor,
                                HairStyle = HairStyle,

                                Items = Items,
                                Clothes = Clothes.Select(x => (Game.Items.Clothes)x).ToArray(),
                                Accessories = Accessories.Select(x => (Game.Items.Clothes)x).ToArray(),
                                Bag = Bag as Game.Items.Bag,
                                Holster = Holster as Game.Items.Holster,
                                Weapons = Weapons.Select(x => (Game.Items.Weapon)x).ToArray(),
                                Armour = (Game.Items.Armour)Armour,

                                Skills = Skills,

                                Familiars = Familiars,

                                Punishments = Punishments,

                                Info = ServerEvents.AllPlayers.ContainsKey(CID) ? ServerEvents.AllPlayers[CID] : null,
                            };

/*                            if (Punishments.Where(x => x.Type == PlayerData.Punishment.Types.Mute && x.GetSecondsLeft() > 0).Count() != 0)
                                pData.IsMuted = true;*/

                            return pData;
                        }).GetAwaiter().GetResult();
                    }
                }
            }
        }
        #endregion

        public static void SaveCharacter(PlayerData pData, bool cash = false)
        {
            if (pData == null)
                return;

            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE characters SET Cash=@Cash WHERE CID=@CID";

                    var res = NAPI.Task.RunAsync(() =>
                    {
                        if (pData.Player?.Exists != true)
                            return false;

                        cmd.Parameters.AddWithValue("@CID", pData.CID);
                        cmd.Parameters.AddWithValue("@Cash", pData.Cash);

                        return true;
                    }).GetAwaiter().GetResult();

                    if (!res)
                        return;

                    cmd.ExecuteNonQuery();
                }
            }
        }

        #endregion

        #region Money

        #region Get Bank Account
        public static Game.Bank.Account GetBankAccountByCID(int cid)
        {
            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM bank_accounts WHERE CID=@CID LIMIT 1";
                    cmd.Parameters.AddWithValue("@CID", cid);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return null;

                        reader.Read();

                        return new Game.Bank.Account()
                        {
                            ID = (int)reader[0],
                            CID = cid,
                            Balance = (int)reader[2],
                            Savings = (int)reader[3],
                            DepositBalance = (int)reader[4],
                            DepositDate = (DateTime)reader[5],
                        };
                    }
                }
            }
        }
        #endregion

        #endregion

        #region Items

        #region Get (one)
        public static Game.Items.Item GetItem(uint id)
        {
            if (id == 0)
                return null;

            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT UID, Data, Items FROM items WHERE UID=@UID;";
                    cmd.Parameters.AddWithValue("@UID", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return null;

                        reader.Read();

                        var data = reader["Data"];

                        if (data is DBNull)
                            return null;

                        var item = ((string)data).DeserializeFromJson<Game.Items.Item>();

                        if (item is Game.Items.IContainer)
                        {
                            var iData = reader["Items"];

                            if (!(iData is DBNull))
                                (item as Game.Items.IContainer).Items = GetItems(((string)iData).DeserializeFromJson<uint[]>());
                        }

                        item.UID = id;

                        return item;
                    }
                }
            }
        }
        #endregion

        #region Get (multiple)
        public static Game.Items.Item[] GetItems(uint[] ids)
        {
            if (ids == null || ids.Length == 0)
                return new Game.Items.Item[ids.Length];

            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    var items = new Game.Items.Item[ids.Length];

                    var idstocheck = ids.Where(x => x != 0);

                    if (idstocheck.Count() != 0)
                        cmd.CommandText = $"SELECT UID, Data, Items FROM items WHERE UID IN ({string.Join(", ", idstocheck)});";
                    else
                        return items;

                    using (var reader =  cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return items;

                        while (reader.Read())
                        {
                            var data = reader["Data"];
                            var item = data is DBNull ? null : ((string)data).DeserializeFromJson<Game.Items.Item>();

                            if (item != null && item is Game.Items.IContainer)
                            {
                                var iData = reader["Items"];

                                if (!(iData is DBNull))
                                    (item as Game.Items.IContainer).Items = GetItems(((string)iData).DeserializeFromJson<uint[]>());
                            }

                            var uid = Convert.ToUInt32(reader["UID"]);
                            item.UID = uid;

                            items[Array.FindIndex(ids, (x) => x == uid)] = item;
                        }

                        return items;
                    }
                }
            }
        }
        #endregion

        #region Update
        public static void UpdateItem(Game.Items.Item item)
        {
            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    bool isContainer = item is Game.Items.IContainer;

                    if (isContainer)
                    {
                        cmd.CommandText = "UPDATE items SET Data=@Data, Items=@Items WHERE UID=@UID;";

                        cmd.Parameters.AddWithValue("@Items", (item as Game.Items.IContainer).Items.Select(x => x?.UID ?? 0).SerializeToJson());
                    }
                    else
                        cmd.CommandText = "UPDATE items SET Data=@Data WHERE UID=@UID;";

                    cmd.Parameters.AddWithValue("@UID", item.UID);
                    cmd.Parameters.AddWithValue("@Data", item.SerializeToJson());

                    cmd.ExecuteNonQuery();
                }
            }
        }
        #endregion

        #region Create
        public static Game.Items.Item AddNewItem(Game.Items.Item item)
        {
            uint uid = 0;

            NAPI.Task.RunAsync(() =>
            {
                if (ServerEvents.FreeItemUIDs.Count != 0)
                    uid = ServerEvents.FreeItemUIDs.Dequeue();
            }).GetAwaiter().GetResult();

            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    if (uid != 0)
                    {
                        if (!(item is Game.Items.IContainer))
                            cmd.CommandText = "INSERT INTO items (UID, Data) VALUES (@UID, @Data);";
                        else
                        {
                            cmd.CommandText = "INSERT INTO items (UID, Data, Items) VALUES (@UID, @Data, @Items);";

                            cmd.Parameters.AddWithValue("@Items", (item as Game.Items.IContainer).Items.Select(x => x?.UID ?? 0).SerializeToJson());
                        }

                        cmd.Parameters.AddWithValue("@UID", uid);
                    }
                    else
                    {
                        if (!(item is Game.Items.IContainer))
                            cmd.CommandText = "INSERT INTO items (Data) VALUES (@Data);";
                        else
                        {
                            cmd.CommandText = "INSERT INTO items (Data, Items) VALUES (@Data, @Items);";

                            cmd.Parameters.AddWithValue("@Items", (item as Game.Items.IContainer).Items.Select(x => x?.UID ?? 0).SerializeToJson());
                        }
                    }

                    cmd.Parameters.AddWithValue("@Data", item.SerializeToJson());

                    cmd.ExecuteNonQuery();

                    item.UID = Convert.ToUInt32(cmd.LastInsertedId);

                    return item;
                }
            }
        }
        #endregion

        #region Delete
        public static void DeleteItem(Game.Items.Item item)
        {
            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM items WHERE UID=@UID;";
                    cmd.Parameters.AddWithValue("@UID", item.UID);

                    var itemsInCont = (item as Game.Items.IContainer)?.Items.Where(x => x != null).Select(x => x.UID);

                    if (itemsInCont?.Any() == true)
                        cmd.CommandText += $"DELETE FROM items WHERE UID IN ({string.Join(", ", itemsInCont)});)";

                    cmd.ExecuteNonQuery();
                }
            }

            NAPI.Task.RunSafe(() =>
            {
                ServerEvents.FreeItemUIDs.Enqueue(item.UID);

                if (item is Game.Items.IContainer)
                {
                    foreach (var x in (item as Game.Items.IContainer).Items)
                        if (x != null)
                            ServerEvents.FreeItemUIDs.Enqueue(x.UID);
                }
                else if (item is Game.Items.ITagged)
                {
                    if (item is Game.Items.Numberplate)
                        ServerEvents.UsedNumberplates.Remove((item as Game.Items.ITagged).Tag);
                    else if (item is Game.Items.Weapon)
                        ServerEvents.UsedWeaponTags.Remove((item as Game.Items.ITagged).Tag);
                }
            });
        }
        #endregion

        #endregion

        #region Player Inventory

        #region Update
        public static void UpdatePlayerInventory(PlayerData pData, bool items = true, bool clothes = false, bool accessories = false, bool bag = false, bool holster = false, bool weapons = false, bool armour = false)
        {
            var player = pData.Player;

            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE inventories SET ";

                    bool wasAdded = false;

                    if (items)
                    {
                        cmd.CommandText += "Items=@Items";
                        cmd.Parameters.AddWithValue("@Items", pData.Items.Select(x => x?.UID ?? 0).SerializeToJson());

                        wasAdded = true;
                    }

                    if (clothes)
                    {
                        if (wasAdded)
                            cmd.CommandText += " , ";
                        else
                            wasAdded = true;

                        cmd.CommandText += "Clothes=@Clothes";
                        cmd.Parameters.AddWithValue("@Clothes", pData.Clothes.Select(x => x?.UID ?? 0).SerializeToJson());
                    }

                    if (accessories)
                    {
                        if (wasAdded)
                            cmd.CommandText += " , ";
                        else
                            wasAdded = true;

                        cmd.CommandText += "Accessories=@Accessories";
                        cmd.Parameters.AddWithValue("@Accessories", pData.Accessories.Select(x => x?.UID ?? 0).SerializeToJson());
                    }

                    if (weapons)
                    {
                        if (wasAdded)
                            cmd.CommandText += " , ";
                        else
                            wasAdded = true;

                        cmd.CommandText += "Weapons=@Weapons";
                        cmd.Parameters.AddWithValue("@Weapons", pData.Weapons.Select(x => x?.UID ?? 0).SerializeToJson());
                    }

                    if (armour)
                    {
                        if (wasAdded)
                            cmd.CommandText += " , ";
                        else
                            wasAdded = true;

                        cmd.CommandText += "Armour=@Armour";
                        cmd.Parameters.AddWithValue("@Armour", pData.Armour?.UID ?? 0);
                    }

                    if (bag)
                    {
                        if (wasAdded)
                            cmd.CommandText += " , ";
                        else
                            wasAdded = true;

                        cmd.CommandText += "Bag=@Bag";
                        cmd.Parameters.AddWithValue("@Bag", pData.Bag?.UID ?? 0);
                    }

                    if (holster)
                    {
                        if (wasAdded)
                            cmd.CommandText += " , ";
                        else
                            wasAdded = true;

                        cmd.CommandText += "Holster=@Holster";
                        cmd.Parameters.AddWithValue("@Holster", pData.Holster?.UID ?? 0);
                    }

                    if (!wasAdded)
                        return;

                    cmd.CommandText += " WHERE CID=@CID";

                    cmd.Parameters.AddWithValue("@CID", pData.CID);

                    cmd.ExecuteNonQuery();
                }
            }
        }
        #endregion

        #endregion

        #region Containers

        #region Load
        public static Game.Items.Container LoadContainerByID(uint id)
        {
            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT SID, Items FROM containers WHERE ID=@ID;";

                    cmd.Parameters.AddWithValue("@ID", id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return null;

                        reader.Read();

                        var cont = new Game.Items.Container((string)reader["SID"])
                        {
                            ID = id,
                            Items = GetItems(((string)reader["Items"]).DeserializeFromJson<uint[]>()),
                        };

                        return cont;
                    }
                }
            }
        }
        #endregion

        #region Update
        public static void UpdateContainer(Game.Items.Container cont)
        {
            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE containers SET Items=@Items WHERE ID=@ID;";

                    cmd.Parameters.AddWithValue("@ID", cont.ID);
                    cmd.Parameters.AddWithValue("@Items", cont.Items.Select(x => x?.UID ?? 0).SerializeToJson());

                    cmd.ExecuteNonQuery();
                }
            }
        }
        #endregion

        #region Create
        public static Game.Items.Container AddContainer(Game.Items.Container cont)
        {
            uint? id = null;

            NAPI.Task.RunAsync(() =>
            {
                if (ServerEvents.FreeContainersIDs.Count != 0)
                    id = ServerEvents.FreeContainersIDs.Dequeue();
            }).GetAwaiter().GetResult();

            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    if (id != null)
                    {
                        cmd.CommandText = "INSERT INTO containers (ID, SID, Items) VALUES (@ID, @SID, @Items);";

                        cmd.Parameters.AddWithValue("@ID", id);
                    }
                    else
                        cmd.CommandText = "INSERT INTO containers (SID, Items) VALUES (@SID, @Items);";

                    cmd.Parameters.AddWithValue("@Items", cont.Items.Select(x => x?.UID ?? 0).SerializeToJson());
                    cmd.Parameters.AddWithValue("@SID", cont.SID);

                    cmd.ExecuteNonQuery();

                    cont.ID = Convert.ToUInt32(cmd.LastInsertedId);

                    return cont;
                }
            }
        }
        #endregion

        #region Delete
        public static void DeleteContainer(Game.Items.Container cont)
        {
            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM containers WHERE ID=@ID;";

                    cmd.Parameters.AddWithValue("@ID", cont.ID);

                    cmd.ExecuteNonQuery();
                }
            }

            NAPI.Task.RunSafe(() =>
            {
                ServerEvents.FreeContainersIDs.Enqueue(cont.ID);
            });
        }
        #endregion

        #endregion

        #region Vehicles

        #region Create
        public static int AddVehicle(VehicleData data)
        {
            int? id = null;

            NAPI.Task.RunAsync(() =>
            {
                if (ServerEvents.FreeVehiclesIDs.Count != 0)
                    id = ServerEvents.FreeVehiclesIDs.Dequeue();
            }).GetAwaiter().GetResult();

            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    if (id != null)
                    {
                        cmd.CommandText = "INSERT INTO vehicles (ID, CID, AllKeys, RegDate, Numberplate, Tuning, TID, LastData) VALUES (@ID, @CID, @AllKeys, @RegDate, @Numberplate, @Tuning, @TID, @LastData);";
                        cmd.Parameters.AddWithValue("@ID", data.ID);
                    }
                    else
                    {
                        cmd.CommandText = "INSERT INTO vehicles (CID, AllKeys, RegDate, Numberplate, Tuning, TID, LastData) VALUES (@CID, @AllKeys, @RegDate, @Numberplate, @Tuning, @TID, @LastData);";
                    }

                    cmd.Parameters.AddWithValue("@CID", data.Owner);
                    cmd.Parameters.AddWithValue("@AllKeys", data.Keys.SerializeToJson());
                    cmd.Parameters.AddWithValue("@RegDate", data.RegistrationDate);
                    cmd.Parameters.AddWithValue("@Numberplate", data.Numberplate?.UID ?? 0);
                    cmd.Parameters.AddWithValue("@Tuning", data.Tuning.SerializeToJson());

                    var res = NAPI.Task.RunAsync<bool>(() =>
                    {
                        if (data.Vehicle?.Exists != true)
                            return false;

                        cmd.Parameters.AddWithValue("@TID", data.TID ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@LastData", VehicleData.LastVehicleData.Get(data).SerializeToJson());

                        return true;
                    }).GetAwaiter().GetResult();

                    if (!res)
                        return -1;

                    cmd.ExecuteNonQuery();

                    return Convert.ToInt32(cmd.LastInsertedId);
                }
            }
        }
        #endregion

        #region Get

        public static VehicleData GetVehicle(int vid)
        {
            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM vehicles WHERE VID=@VID LIMIT 1;";

                    cmd.Parameters.AddWithValue("@VID", vid);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return null;

                        reader.Read();

                        var lastData = ((string)reader["LastData"]).DeserializeFromJson<VehicleData.LastVehicleData>();

                        if (lastData.Park == null)
                            lastData.Park = new VehicleData.ParkData(Utils.DefaultSpawnPosition, Utils.ZeroVector, Utils.Dimensions.Main);

                        string id = (string)reader["ID"];
                        int owner = (int)reader["CID"];
                        List<uint> keys = ((string)reader["AllKeys"]).DeserializeFromJson<List<uint>>();
                        DateTime regDate = (DateTime)reader["RegDate"];
                        uint? tid = reader["TID"] == DBNull.Value ? null : (uint?)Convert.ToUInt32(reader["TID"]);
                        var numberplate = (Game.Items.Numberplate)GetItem(Convert.ToUInt32(reader["Numberplate"]));
                        var tuning = ((string)reader["Tuning"]).DeserializeFromJson<Game.Data.Vehicles.Tuning>();

                        VehicleData vehData = NAPI.Task.RunAsync<VehicleData>(() =>
                        {
                            var vehData = Game.Data.Vehicles.Vehicle.Create(id, lastData.Park.Position, lastData.Park.Rotation, lastData.Park.Dimension);

                            if (vehData == null)
                                return null;

                            vehData.Tuning = tuning.Apply(vehData.Vehicle);
                            vehData.Owner = owner;
                            vehData.TID = tid;
                            vehData.VID = vid;

                            vehData.FuelLevel = lastData.FuelLevel;
                            vehData.Mileage = lastData.Mileage;

                            vehData.ID = id;
                            vehData.RegistrationDate = regDate;
                            vehData.Keys = keys;

                            numberplate?.Setup(vehData);

                            vehData.Vehicle.Rotation = lastData.Park.Rotation;

                            NAPI.Task.Run(() =>
                            {
                                if (vehData?.Vehicle?.Exists != true)
                                    return;

                                vehData.Vehicle.Dimension = lastData.Park.Dimension;
                            }, 1000);

                            return vehData;
                        }).GetAwaiter().GetResult();

                        if (vehData == null)
                            return null;

                        if (tid != null)
                            Game.Items.Container.Load((uint)tid, vehData.Vehicle);

                        return vehData;
                    }
                }
            }
        }

        #endregion

        #region Update
        public static void UpdateVehicle(VehicleData vData, bool keys = false, bool numberplate = false, bool owner = false, bool lastData = false, bool tuning = false)
        {
            var veh = vData.Vehicle;

            int vid = NAPI.Task.RunAsync<int>(() =>
            {
                if (veh?.Exists != true)
                    return -1;

                return vData.VID;
            }).GetAwaiter().GetResult();

            if (vid == -1)
                return;

            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE vehicles SET ";

                    bool wasAdded = false;

                    if (keys)
                    {
                        cmd.CommandText += "AllKeys=@AllKeys";
                        cmd.Parameters.AddWithValue("@Keys", vData.Keys.SerializeToJson());

                        wasAdded = true;
                    }

                    if (numberplate)
                    {
                        if (wasAdded)
                            cmd.CommandText += " , ";
                        else
                            wasAdded = true;

                        cmd.CommandText += "Numberplate=@Numberplate";
                        cmd.Parameters.AddWithValue("@Numberplate", vData.Numberplate?.UID ?? 0);
                    }

                    if (owner)
                    {
                        if (wasAdded)
                            cmd.CommandText += " , ";
                        else
                            wasAdded = true;

                        cmd.CommandText += "CID=@CID";
                        cmd.Parameters.AddWithValue("@CID", vData.Owner);
                    }

                    if (lastData)
                    {
                        if (wasAdded)
                            cmd.CommandText += " , ";
                        else
                            wasAdded = true;

                        cmd.CommandText += "LastData=@LastData";
                        cmd.Parameters.AddWithValue("@LastData", (NAPI.Task.RunAsync(() => VehicleData.LastVehicleData.Get(vData))).GetAwaiter().GetResult().SerializeToJson());
                    }

                    if (tuning)
                    {
                        if (wasAdded)
                            cmd.CommandText += " , ";
                        else
                            wasAdded = true;

                        cmd.CommandText += "Tuning=@Tuning";
                        cmd.Parameters.AddWithValue("@Tuning", vData.Tuning.SerializeToJson());
                    }

                    if (!wasAdded)
                        return;

                    cmd.CommandText += " WHERE VID=@VID LIMIT 1;";

                    cmd.Parameters.AddWithValue("@VID", vid);

                    cmd.ExecuteNonQuery();
                }
            }
        }
        #endregion

        #region Delete
        public static void DeleteVehicle(int vid)
        {
            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM vehicles WHERE VID=@VID;";

                    cmd.Parameters.AddWithValue("@VID", vid);

                    cmd.ExecuteNonQuery();
                }
            }

            NAPI.Task.RunSafe(() =>
            {
                ServerEvents.FreeVehiclesIDs.Enqueue(vid);
            });
        }
        #endregion

        #endregion

        #region Businesses
        public static Game.Businesses.Business GetBusiness(Game.Businesses.Business business)
        {
            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM businesses WHERE ID=@ID LIMIT 1;";

                    cmd.Parameters.AddWithValue("@ID", business.ID);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return null;

                        reader.Read();

                        business.Owner = (int)reader["CID"];
                        business.Name = (string)reader["Name"];

                        business.Cash = (int)reader["Cash"];
                        business.Bank = (int)reader["Bank"];

                        business.Materials = (int)reader["Materials"];
                        business.OrderedMaterials = (int)reader["OrderedMaterials"];

                        business.Margin = (float)reader["Margin"];

                        business.Statistics = ((string)reader["Statistics"]).DeserializeFromJson<int[]>();

/*                        if (business is Game.Businesses.Farm)
                        {

                        }*/

                        return business;
                    }
                }
            }
        }
        #region Load All
        #endregion
        #endregion

        #region Gifts

        public static Game.Items.Gift AddGift(Game.Items.Gift gift)
        {
            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO gifts (CID, Type, GID, Variation, Amount, Reason) VALUES (@CID, @Type, @GID, @Variation, @Amount, @Reason);";

                    cmd.Parameters.AddWithValue("@CID", gift.CID);
                    cmd.Parameters.AddWithValue("@Type", gift.Type);
                    cmd.Parameters.AddWithValue("@GID", (object)gift.GID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Variation", gift.Variation);
                    cmd.Parameters.AddWithValue("@Amount", gift.Amount);
                    cmd.Parameters.AddWithValue("@Reason", gift.SourceType);

                    cmd.ExecuteNonQuery();

                    gift.ID = Convert.ToInt32(cmd.LastInsertedId);

                    return gift;
                }
            }
        }

        public static async Task DeleteGift(Game.Items.Gift gift)
        {
            await Task.Run(async () =>
            {
                using (var conn = new MySqlConnection(LocalConnectionCredentials))
                {
                    await conn.OpenAsync();

                    MySqlCommand cmd = conn.CreateCommand();

                    cmd.CommandText = "DELETE FROM gifts WHERE ID = @ID;";

                    cmd.Parameters.AddWithValue("ID", gift.ID);

                    await cmd.ExecuteNonQueryAsync();

                    gift = null;
                }
            });
        }

        #endregion

        #region Houses
        public static Game.Houses.House GetHouse(Game.Houses.House house)
        {
            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM houses WHERE ID=@ID LIMIT 1;";

                    cmd.Parameters.AddWithValue("@ID", house.ID);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return null;

                        reader.Read();

                        house.Owner = (int)reader["CID"];
                        house.StyleType = (Game.Houses.HouseBase.Style.Types)(int)reader["StyleType"];
                        house.Settlers = NAPI.Util.FromJson<List<int>>((string)reader["Settlers"]);
                        house.IsLocked = (bool)reader["IsLocked"];
                        house.ContainersLocked = (bool)reader["ContainersLocked"];
                        house.Vehicles = NAPI.Util.FromJson<List<int>>((string)reader["Vehicles"]);

                        cmd.CommandText = "";

                        if (reader["Locker"] == DBNull.Value)
                        {
                            house.Locker = Game.Items.Container.Create(new Game.Items.Container("h_locker"), null).GetAwaiter().GetResult().ID;

                            cmd.CommandText += $"UPDATE houses SET Locker={house.Locker} WHERE ID={house.ID};";
                        }
                        else
                            house.Locker = Convert.ToUInt32(reader["Locker"]);

                        if (reader["Wardrobe"] == DBNull.Value)
                        {
                            house.Wardrobe = Game.Items.Container.Create(new Game.Items.Container("h_wardrobe"), null).GetAwaiter().GetResult().ID;

                            cmd.CommandText += $"UPDATE houses SET Wardrobe={house.Wardrobe} WHERE ID={house.ID};";
                        }
                        else
                            house.Wardrobe = Convert.ToUInt32(reader["Wardrobe"]);

                        if (reader["Fridge"] == DBNull.Value)
                        {
                            house.Fridge = Game.Items.Container.Create(new Game.Items.Container("h_fridge"), null).GetAwaiter().GetResult().ID;

                            cmd.CommandText += $"UPDATE houses SET Fridge={house.Fridge} WHERE ID={house.ID};";
                        }
                        else
                            house.Fridge = Convert.ToUInt32(reader["Fridge"]);

                        if (reader["DoorsStates"] == DBNull.Value)
                        {
                            house.DoorsStates = new bool[house.StyleData.DoorsCount];

                            cmd.CommandText += $"UPDATE houses SET DoorsStates='{house.DoorsStates.SerializeToJson()}' WHERE ID={house.ID};";
                        }
                        else
                            house.DoorsStates = NAPI.Util.FromJson<bool[]>((string)reader["DoorsStates"]);

                        if (reader["LightsStates"] == DBNull.Value)
                        {
                            house.LightsStates = new (Color Colour, bool State)[house.StyleData.LightsCount];

                            for (int i = 0; i < house.LightsStates.Length; i++)
                            {
                                house.LightsStates[i].Colour = House.DefaultLightColour;
                                house.LightsStates[i].State = true;
                            }

                            cmd.CommandText += $"UPDATE houses SET LightsStates='{house.LightsStates.SerializeToJson()}' WHERE ID={house.ID};";
                        }
                        else
                            house.LightsStates = NAPI.Util.FromJson<(Color, bool)[]>((string)reader["LightsStates"]);
                    }

                    if (cmd.CommandText.Length > 0)
                        cmd.ExecuteNonQuery();

                    return house;
                }
            }
        }

        public static Game.Houses.Apartments GetApartments(Game.Houses.Apartments apartments)
        {
            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM apartments WHERE ID=@ID LIMIT 1;";

                    cmd.Parameters.AddWithValue("@ID", apartments.ID);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                            return null;

                        reader.Read();

                        apartments.Owner = (int)reader["CID"];
                        apartments.StyleType = (Game.Houses.HouseBase.Style.Types)(int)reader["StyleType"];
                        apartments.Settlers = NAPI.Util.FromJson<List<int>>((string)reader["Settlers"]);
                        apartments.IsLocked = (bool)reader["IsLocked"];
                        apartments.Vehicles = NAPI.Util.FromJson<List<int>>((string)reader["Vehicles"]);

                        if (reader["Locker"] == DBNull.Value)
                            apartments.Locker = Game.Items.Container.Create(new Game.Items.Container("h_locker"), null).GetAwaiter().GetResult().ID;
                        else
                            apartments.Locker = Convert.ToUInt32(reader["Locker"]);

                        if (reader["Wardrobe"] == DBNull.Value)
                            apartments.Wardrobe = Game.Items.Container.Create(new Game.Items.Container("h_wardrobe"), null).GetAwaiter().GetResult().ID;
                        else
                            apartments.Wardrobe = Convert.ToUInt32(reader["Wardrobe"]);

                        if (reader["Fridge"] == DBNull.Value)
                            apartments.Fridge = Game.Items.Container.Create(new Game.Items.Container("h_fridge"), null).GetAwaiter().GetResult().ID;
                        else
                            apartments.Fridge = Convert.ToUInt32(reader["Fridge"]);

                        return apartments;
                    }
                }
            }
        }
        #endregion
    }
}
