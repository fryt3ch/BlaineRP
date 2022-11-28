using BCRPServer.Game.Houses;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
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

        private static ConcurrentQueue<MySqlCommand> QueriesQueue { get; set; }

        public enum AuthResults
        {
            RegMailNotFree = 0,
            RegLoginNotFree,

            RegOk,
        }

        #region General

        public static void StartService()
        {
            QueriesQueue = new ConcurrentQueue<MySqlCommand>();

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(10000);

                    await Wait();

                    DoAllQueries();

                    Release();
                }
            });
        }

        public static void DoAllQueries()
        {
            var commands = new List<MySqlCommand>();

            MySqlCommand cmd;

            while (QueriesQueue.TryDequeue(out cmd))
            {
                if (cmd != null)
                    commands.Add(cmd);
            }

/*            for (int i = 0; i < commands.Count; i++)
            {
                cmd = commands[i];

                if (cmd.Parameters["ID"].Value is uint id)
                {
                    var text = cmd.CommandText;

                    var sameCommands = new List<MySqlCommand>();

                    for (int j = 0; j < commands.Count; j++)
                    {
                        var cmd1 = commands[j];

                        if (cmd1.Parameters["ID"].Value is uint && cmd1.CommandText == text)
                            sameCommands.Add(cmd1);
                    }

                    for (int j = 0; j < sameCommands.Count - 1; j++)
                        commands.Remove(sameCommands[j]);
                }
            }*/

            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                foreach (var x in commands)
                {
                    x.Connection = conn;

                    x.ExecuteNonQuery();
                }
            }

            Console.WriteLine($"{commands.Count} local queries were done!");
        }

        public static void PushQuery(MySqlCommand cmd) => QueriesQueue.Enqueue(cmd);

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

        public static async Task WaitGlobal() => await GlobalConnectionSemaphore.WaitAsync();
        public static void ReleaseGlobal() => GlobalConnectionSemaphore.Release();

        public static async Task Wait() => await LocalConnectionSemaphore.WaitAsync();
        public static void Release() => LocalConnectionSemaphore.Release();

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

        public static void LoadAll()
        {
            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM furniture;";

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var uid = Convert.ToUInt32(reader["ID"]);
                                var id = (string)reader["Type"];
                                var data = ((string)reader["Data"]).DeserializeFromJson<Game.Houses.Furniture.FurnitureData>();

                                Game.Houses.Furniture.AddOnLoad(new Furniture(uid, id, data));
                            }
                        }
                    }

                    cmd.CommandText = "SELECT * FROM items;";

                    var includedItems = new Dictionary<uint, uint[]>();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var data = reader["Data"];

                                if (data is DBNull)
                                    continue;

                                var uid = Convert.ToUInt32(reader["ID"]);

                                var item = ((string)data).DeserializeFromJson<Game.Items.Item>();

                                if (item == null)
                                    continue;

                                var iData = reader["Items"];

                                if (!(iData is DBNull))
                                    includedItems.Add(uid, ((string)iData).DeserializeFromJson<uint[]>());

                                item.UID = uid;

                                Game.Items.Item.AddOnLoad(item);
                            }
                        }
                    }

                    foreach (var x in includedItems.Keys)
                    {
                        if (Game.Items.Item.Get(x) is Game.Items.IContainer container)
                        {
                            container.Items = new Game.Items.Item[includedItems[x].Length];

                            for (int i = 0; i < includedItems[x].Length; i++)
                            {
                                if (includedItems[x][i] == 0)
                                    continue;

                                container.Items[i] = Game.Items.Item.Get(includedItems[x][i]);
                            }
                        }
                    }

                    cmd.CommandText = "SELECT * FROM containers;";

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var id = Convert.ToUInt32(reader["ID"]);
                                var sid = (string)reader["SID"];

                                var items = (((string)reader["Items"]).DeserializeFromJson<uint[]>()).Select(x => x == 0 ? null : Game.Items.Item.Get(x)).ToArray();

                                var cont = new Game.Items.Container(sid, id, items);

                                Game.Items.Container.AddOnLoad(cont);
                            }
                        }
                    }

                    cmd.CommandText = "SELECT * FROM vehicles;";

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var vid = Convert.ToUInt32(reader["ID"]);
                                var sid = (string)reader["SID"];

                                var oType = (VehicleData.OwnerTypes)(int)reader["OwnerType"];

                                var oId = Convert.ToUInt32(reader["OwnerID"]);

                                var allKeys = ((string)reader["AllKeys"]).DeserializeFromJson<List<uint>>();

                                var tid = reader["TID"] is DBNull ? null : (uint?)Convert.ToUInt32(reader["TID"]);

                                var numberplate = Convert.ToUInt32(reader["Numberplate"]);

                                var tuning = ((string)reader["Tuning"]).DeserializeFromJson<Game.Data.Vehicles.Tuning>();

                                var lastData = ((string)reader["LastData"]).DeserializeFromJson<VehicleData.LastVehicleData>();

                                var regDate = (DateTime)reader["RegDate"];

                                var vInfo = new VehicleData.VehicleInfo()
                                {
                                    VID = vid,

                                    ID = sid,

                                    AllKeys = allKeys,

                                    OwnerType = oType,

                                    OwnerID = oId,

                                    RegistrationDate = regDate,

                                    TID = tid,

                                    Numberplate = numberplate == 0 ? null : Game.Items.Item.Get(numberplate) as Game.Items.Numberplate,

                                    Tuning = tuning,

                                    LastData = lastData,

                                    Data = Game.Data.Vehicles.All[sid],
                                };

                                VehicleData.VehicleInfo.AddOnLoad(vInfo);
                            }
                        }
                    }

                    cmd.CommandText = "SELECT * FROM customizations;";

                    var customizations = new Dictionary<uint, object[]>();

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var cid = Convert.ToUInt32(reader["ID"]);

                                var hBlend = ((string)reader["HeadBlend"]).DeserializeFromJson<HeadBlend>();
                                var hOverlays = ((string)reader["HeadOverlays"]).DeserializeFromJson<Dictionary<int, HeadOverlay>>();
                                var fFeatures = ((string)reader["FaceFeatures"]).DeserializeFromJson<float[]>();
                                var decors = ((string)reader["Decorations"]).DeserializeFromJson<List<Decoration>>();
                                var hStyle = ((string)reader["HairStyle"]).DeserializeFromJson<Game.Data.Customization.HairStyle>();
                                var eyeColour = Convert.ToByte(reader["EyeColor"]);

                                customizations.Add(cid, new object[] { hBlend, hOverlays, fFeatures, decors, hStyle, eyeColour });
                            }
                        }
                    }

                    cmd.CommandText = "SELECT * FROM gifts;";

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var id = Convert.ToUInt32(reader["ID"]);

                                var cid = Convert.ToUInt32(reader["CID"]);
                                var gid = reader["GID"] is DBNull ? null : (string)reader["GID"];
                                var reason = (Game.Items.Gift.SourceTypes)(int)reader["Reason"];
                                var type = (Game.Items.Gift.Types)(int)reader["Type"];
                                int variation = (int)reader["Variation"];
                                int amount = (int)reader["Amount"];

                                var gift = new Game.Items.Gift(cid, id, reason, type, gid, variation, amount);

                                Game.Items.Gift.AddOnLoad(gift);
                            }
                        }
                    }

                    cmd.CommandText = "SELECT * FROM characters;";

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var cid = Convert.ToUInt32(reader["ID"]);

                                var aid = Convert.ToUInt32(reader["AID"]);

                                var creationDate = (DateTime)reader["CreationDate"];

                                var aLvl = (int)reader["AdminLevel"];

                                var lastJoinDate = (DateTime)reader["LastJoinDate"];

                                var timePlayed = (int)reader["TimePlayed"];

                                var name = (string)reader["Name"];

                                var surname = (string)reader["Surname"];

                                var sex = (bool)reader["Sex"];

                                var birthDate = (DateTime)reader["BirthDate"];

                                var licenses = ((string)reader["Licenses"]).DeserializeFromJson<List<PlayerData.LicenseTypes>>();

                                var fraction = (PlayerData.FractionTypes)(int)reader["Fraction"];

                                var orgId = (int)reader["OrgID"];

                                var cash = (int)reader["Cash"];

                                var lastData = ((string)reader["LastData"]).DeserializeFromJson<PlayerData.LastPlayerData>();

                                var familiars = ((string)reader["Familiars"]).DeserializeFromJson<List<uint>>();

                                var skills = ((string)reader["Skills"]).DeserializeFromJson<Dictionary<PlayerData.SkillTypes, int>>();

                                var punishments = GetPunishmentsByCID(cid);

                                var ownedVehicles = VehicleData.VehicleInfo.GetAllByCID(cid);

                                var pInfo = new PlayerData.PlayerInfo()
                                {
                                    CID = cid,

                                    AID = aid,

                                    CreationDate = creationDate,

                                    AdminLevel = aLvl,

                                    LastJoinDate = lastJoinDate,

                                    TimePlayed = timePlayed,

                                    Name = name,

                                    Surname = surname,

                                    Sex = sex,

                                    BirthDate = birthDate,

                                    Licenses = licenses,

                                    Fraction = fraction,

                                    OrganisationID = orgId,

                                    Cash = cash,

                                    BankAccount = GetBankAccountByCID(cid),

                                    LastData = lastData,

                                    Familiars = familiars,

                                    Skills = skills,

                                    Punishments = punishments,

                                    OwnedVehicles = ownedVehicles,

                                    OwnedBusinesses = new List<Game.Businesses.Business>(),
                                    OwnedGarages = new List<Garage>(),
                                    OwnedHouses = new List<House>(),
                                    OwnedApartments = new List<Apartments>(),

                                    SettledHouses = new List<House>(),
                                    SettledApartments = new List<Apartments>(),

                                    HeadBlend = (HeadBlend)customizations[cid][0],
                                    HeadOverlays = (Dictionary<int, HeadOverlay>)customizations[cid][1],
                                    FaceFeatures = (float[])customizations[cid][2],
                                    Decorations = (List<Decoration>)customizations[cid][3],
                                    HairStyle = (Game.Data.Customization.HairStyle)customizations[cid][4],
                                    EyeColor = (byte)customizations[cid][5],

                                    Gifts = Game.Items.Gift.GetAllByCID(cid),
                                };

                                if (pInfo.BankAccount != null)
                                    pInfo.BankAccount.PlayerInfo = pInfo;

                                PlayerData.PlayerInfo.AddOnLoad(pInfo);
                            }
                        }
                    }

                    cmd.CommandText = "SELECT * FROM inventories;";

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var cid = Convert.ToUInt32(reader["ID"]);

                                var items = ((string)reader["Items"]).DeserializeFromJson<uint[]>();
                                var clothes = ((string)reader["Clothes"]).DeserializeFromJson<uint[]>();
                                var accs = ((string)reader["Accessories"]).DeserializeFromJson<uint[]>();
                                var weapons = ((string)reader["Weapons"]).DeserializeFromJson<uint[]>();

                                var holster = Convert.ToUInt32(reader["Holster"]);
                                var arm = Convert.ToUInt32(reader["Armour"]);
                                var bag = Convert.ToUInt32(reader["Bag"]);

                                var furniture = ((string)reader["Furniture"]).DeserializeFromJson<uint[]>();

                                var pInfo = PlayerData.PlayerInfo.Get(cid);

                                if (pInfo == null)
                                    continue;

                                pInfo.Items = items.Select(x => x == 0 ? null : Game.Items.Item.Get(x)).ToArray();
                                pInfo.Clothes = clothes.Select(x => x == 0 ? null : Game.Items.Item.Get(x) as Game.Items.Clothes).ToArray();
                                pInfo.Accessories = accs.Select(x => x == 0 ? null : Game.Items.Item.Get(x) as Game.Items.Clothes).ToArray();
                                pInfo.Weapons = weapons.Select(x => x == 0 ? null : Game.Items.Item.Get(x) as Game.Items.Weapon).ToArray();

                                pInfo.Holster = holster == 0 ? null : Game.Items.Item.Get(holster) as Game.Items.Holster;
                                pInfo.Armour = arm == 0 ? null : Game.Items.Item.Get(arm) as Game.Items.Armour;
                                pInfo.Bag = bag == 0 ? null : Game.Items.Item.Get(bag) as Game.Items.Bag;

                                pInfo.Furniture = furniture.Select(x => Game.Houses.Furniture.Get(x)).Where(x => x != null).ToList();
                            }
                        }
                    }
                }
            }
        }

        public static void UpdateFreeUIDs()
        {
            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    var toDelete = new List<uint>();

                    var usedItems = new List<Game.Items.Item>();

                    usedItems.AddRange(Game.Items.Container.All.Values.SelectMany(x => x.Items));

                    usedItems.AddRange(PlayerData.PlayerInfo.All.Values.SelectMany(x => x.Items.Concat(x.Clothes).Concat(x.Weapons).Concat(x.Accessories).Concat(new Game.Items.Item[] { x.Bag, x.Holster, x.Armour})));

                    usedItems.AddRange(VehicleData.VehicleInfo.All.Values.Select(x => x.Numberplate));

                    usedItems.RemoveAll(x => x == null);

                    foreach (var x in Game.Items.Item.All.Values.Except(usedItems))
                    {
                        toDelete.Add(x.UID);

                        if (x is Game.Items.IContainer cont)
                        {
                            foreach (var y in cont.Items.Where(x => x != null))
                            {
                                toDelete.Add(y.UID);

                                Game.Items.Item.RemoveOnLoad(y);
                            }
                        }

                        Game.Items.Item.RemoveOnLoad(x);
                    }

                    if (toDelete.Count > 0)
                    {
                        cmd.CommandText = $"DELETE FROM Items WHERE ID IN ({string.Join(", ", toDelete)})";

                        cmd.ExecuteNonQuery();
                    }

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

                    for (uint i = 1; i < nextAi; i++)
                    {
                        if (Game.Items.Item.Get(i) == null)
                            Game.Items.Item.AddFreeId(i);
                    }
                }

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT auto_increment FROM INFORMATION_SCHEMA.TABLES WHERE table_schema='{LocalDatabase}' AND table_name='characters';";

                    var nextAi = Utils.FirstCID;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();

                            nextAi = Convert.ToUInt32(reader[0]);
                        }
                    }

                    for (uint i = Utils.FirstCID; i < nextAi; i++)
                    {
                        if (PlayerData.PlayerInfo.Get(i) == null)
                            PlayerData.PlayerInfo.AddFreeId(i);
                    }
                }

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT auto_increment FROM INFORMATION_SCHEMA.TABLES WHERE table_schema='{LocalDatabase}' AND table_name='vehicles';";

                    var nextAi = Utils.FirstVID;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();

                            nextAi = Convert.ToUInt32(reader[0]);
                        }
                    }

                    for (uint i = Utils.FirstVID; i < nextAi; i++)
                    {
                        if (VehicleData.VehicleInfo.Get(i) == null)
                            VehicleData.VehicleInfo.AddFreeId(i);
                    }
                }

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT auto_increment FROM INFORMATION_SCHEMA.TABLES WHERE table_schema='{LocalDatabase}' AND table_name='containers';";

                    uint nextAi = 1;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();

                            nextAi = Convert.ToUInt32(reader[0]);
                        }
                    }

                    for (uint i = 1; i < nextAi; i++)
                    {
                        if (Game.Items.Container.Get(i) == null)
                            Game.Items.Container.AddFreeId(i);
                    }
                }

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT auto_increment FROM INFORMATION_SCHEMA.TABLES WHERE table_schema='{LocalDatabase}' AND table_name='gifts';";

                    uint nextAi = 1;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();

                            nextAi = Convert.ToUInt32(reader[0]);
                        }
                    }

                    for (uint i = 1; i < nextAi; i++)
                    {
                        if (Game.Items.Gift.Get(i) == null)
                            Game.Items.Gift.AddFreeId(i);
                    }
                }

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = $"SELECT auto_increment FROM INFORMATION_SCHEMA.TABLES WHERE table_schema='{LocalDatabase}' AND table_name='furniture';";

                    uint nextAi = 1;

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();

                            nextAi = Convert.ToUInt32(reader[0]);
                        }
                    }

                    for (uint i = 1; i < nextAi; i++)
                    {
                        if (Game.Houses.Furniture.Get(i) == null)
                            Game.Houses.Furniture.AddFreeId(i);
                    }
                }

                foreach (var x in Game.Items.Item.All.Values)
                {
                    if (x is Game.Items.Numberplate np)
                    {
                        Game.Items.Numberplate.UsedTags.Add(np.Tag);
                    }
                    else if (x is Game.Items.Weapon weapon)
                    {
                        Game.Items.Weapon.UsedTags.Add(weapon.Tag);
                    }
                }
            }
        }

        #endregion

        #region Account
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

        #endregion

        #region Punishments

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

        public static Bank.Account GetBankAccountByCID(uint cid)
        {
            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM bank_accounts WHERE CID=@CID";
                    cmd.Parameters.AddWithValue("@CID", cid);

                    using (var reader = cmd.ExecuteReader())
                    {
                        var result = new List<PlayerData.Punishment>();

                        if (reader.HasRows)
                        {
                            reader.Read();

                            var balance = (int)reader["Balance"];
                            var savings = (int)reader["Savings"];
                            var tariff = (Bank.Tariff.Types)(int)reader["Tariff"];
                            var std = (bool)reader["STD"];

                            return new Bank.Account()
                            {
                                Balance = balance,
                                SavingsBalance = savings,
                                Tariff = Bank.Tariff.All[tariff],
                                SavingsToDebit = std,
                                MinSavingsBalance = savings,
                                TotalDayTransactions = 0,
                            };
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }

        public static void BankAccountAdd(Bank.Account account)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "INSERT INTO bank_accounts (CID, Balance, Savings, Tariff, STD) VALUES (@CID, @Balance, @Savings, @Tariff, @STD);";

            cmd.Parameters.AddWithValue("CID", account.PlayerInfo.CID);

            cmd.Parameters.AddWithValue("Balance", account.Balance);
            cmd.Parameters.AddWithValue("Savings", account.SavingsBalance);
            cmd.Parameters.AddWithValue("Tariff", (int)account.Tariff.Type);
            cmd.Parameters.AddWithValue("STD", account.SavingsToDebit);

            PushQuery(cmd);
        }

        public static void BankAccountUpdate(Bank.Account account)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE bank_accounts SET Balance=@Balance, Savings=@Savings, Tariff=@Tariff, STD=@STD WHERE CID=@CID;";

            cmd.Parameters.AddWithValue("CID", account.PlayerInfo.CID);

            cmd.Parameters.AddWithValue("Balance", account.Balance);
            cmd.Parameters.AddWithValue("Savings", account.SavingsBalance);
            cmd.Parameters.AddWithValue("Tariff", (int)account.Tariff.Type);
            cmd.Parameters.AddWithValue("STD", account.SavingsToDebit);

            PushQuery(cmd);
        }

        public static void BankAccountDelete(Bank.Account account)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "DELETE FROM bank_accounts WHERE CID=@CID;";

            cmd.Parameters.AddWithValue("CID", account.PlayerInfo.CID);

            PushQuery(cmd);
        }
        #endregion

        #region Character

        public static void CharacterAdd(PlayerData.PlayerInfo pInfo)
        {
            MySqlCommand cmd = new MySqlCommand();

            cmd.CommandText = @"INSERT INTO characters (ID, AID, CreationDate, AdminLevel, LastJoinDate, IsOnline, TimePlayed, 
                    Name, Surname, Sex, BirthDate, Licenses, Fraction, OrgID, Cash, LastData, 
                    Satiety, Mood, Familiars, Skills) 
                    VALUES (@CID, @AID, @CreationDate, @AdminLevel, @LastJoinDate, @IsOnline, @TimePlayed, 
                    @Name, @Surname, @Sex, @BirthDate, @Licenses, @Fraction, @OrgID, @Cash, @LastData, 
                    @Satiety, @Mood, @Familiars, @Skills); 

                    INSERT INTO customizations (ID, HeadBlend, HeadOverlays, FaceFeatures, Decorations, HairStyle, EyeColor) VALUES (@CID, @HeadBlend, @HeadOverlays, @FaceFeatures, @Decorations, @HairStyle, @EyeColor); 

                    INSERT INTO inventories (ID, Items, Clothes, Accessories, Bag, Holster, Weapons, Armour) VALUES (@CID, @Items, @Clothes, @Accessories, @Bag, @Holster, @Weapons, @Armour); ";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);

            cmd.Parameters.AddWithValue("@AID", pInfo.AID);

            cmd.Parameters.AddWithValue("@IsOnline", true);
            cmd.Parameters.AddWithValue("@TimePlayed", pInfo.TimePlayed);
            cmd.Parameters.AddWithValue("@Fraction", pInfo.Fraction);
            cmd.Parameters.AddWithValue("@OrgID", pInfo.OrganisationID);

            cmd.Parameters.AddWithValue("@CreationDate", pInfo.CreationDate);
            cmd.Parameters.AddWithValue("@AdminLevel", pInfo.AdminLevel);
            cmd.Parameters.AddWithValue("@LastJoinDate", pInfo.LastJoinDate);
            cmd.Parameters.AddWithValue("@Cash", pInfo.Cash);
            cmd.Parameters.AddWithValue("@LastData", pInfo.LastData.SerializeToJson());

            cmd.Parameters.AddWithValue("@Name", pInfo.Name);
            cmd.Parameters.AddWithValue("@Surname", pInfo.Surname);
            cmd.Parameters.AddWithValue("@Sex", pInfo.Sex);
            cmd.Parameters.AddWithValue("@BirthDate", pInfo.BirthDate);

            cmd.Parameters.AddWithValue("@Licenses", JsonConvert.SerializeObject(pInfo.Licenses));

            cmd.Parameters.AddWithValue("@Skills", JsonConvert.SerializeObject(pInfo.Skills));

            cmd.Parameters.AddWithValue("@Familiars", JsonConvert.SerializeObject(pInfo.Familiars));

            cmd.Parameters.AddWithValue("@Items", pInfo.Items.Select(x => x?.UID ?? 0).SerializeToJson());
            cmd.Parameters.AddWithValue("@Clothes", pInfo.Clothes.Select(x => x?.UID ?? 0).SerializeToJson());
            cmd.Parameters.AddWithValue("@Accessories", pInfo.Accessories.Select(x => x?.UID ?? 0).SerializeToJson());
            cmd.Parameters.AddWithValue("@Weapons", pInfo.Weapons.Select(x => x?.UID ?? 0).SerializeToJson());
            cmd.Parameters.AddWithValue("@Armour", pInfo.Armour?.UID ?? 0);
            cmd.Parameters.AddWithValue("@Bag", pInfo.Bag?.UID ?? 0);
            cmd.Parameters.AddWithValue("@Holster", pInfo.Holster?.UID ?? 0);

            cmd.Parameters.AddWithValue("@HeadBlend", pInfo.HeadBlend.SerializeToJson());
            cmd.Parameters.AddWithValue("@HeadOverlays", pInfo.HeadOverlays.SerializeToJson());
            cmd.Parameters.AddWithValue("@FaceFeatures", pInfo.FaceFeatures.SerializeToJson());
            cmd.Parameters.AddWithValue("@Decorations", pInfo.Decorations.SerializeToJson());
            cmd.Parameters.AddWithValue("@HairStyle", pInfo.HairStyle.SerializeToJson());
            cmd.Parameters.AddWithValue("@EyeColor", pInfo.EyeColor);

            PushQuery(cmd);
        }

        public static void CharacterUpdateOnEnter(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE characters SET IsOnline=true, LastJoinDate=@LastJoinDate WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);
            cmd.Parameters.AddWithValue("@LastJoinDate", pInfo.LastJoinDate);

            PushQuery(cmd);
        }

        public static void CharacterSaveOnExit(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE characters SET IsOnline=false, TimePlayed=@TimePlayed, LastData=@LastData, Familiars=@Familiars WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);
            cmd.Parameters.AddWithValue("@TimePlayed", pInfo.TimePlayed);
            cmd.Parameters.AddWithValue("@LastData", pInfo.LastData.SerializeToJson());
            cmd.Parameters.AddWithValue("@Familiars", pInfo.Familiars.SerializeToJson());

            PushQuery(cmd);

            CharacterItemsUpdate(pInfo);
            CharacterClothesUpdate(pInfo);
            CharacterAccessoriesUpdate(pInfo);
            CharacterWeaponsUpdate(pInfo);
            CharacterHolsterUpdate(pInfo);
            CharacterBagUpdate(pInfo);
            CharacterArmourUpdate(pInfo);
        }

        public static void CharacterUpdateCash(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE characters SET Cash=@Cash WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);
            cmd.Parameters.AddWithValue("@Cash", pInfo.Cash);

            PushQuery(cmd);
        }

        public static void CharacterItemsUpdate(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE inventories SET Items=@Items WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);

            cmd.Parameters.AddWithValue("@Items", pInfo.Items.Select(x => x?.UID ?? 0).SerializeToJson());

            PushQuery(cmd);
        }

        public static void CharacterClothesUpdate(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE inventories SET Clothes=@Clothes WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);

            cmd.Parameters.AddWithValue("@Clothes", pInfo.Clothes.Select(x => x?.UID ?? 0).SerializeToJson());

            PushQuery(cmd);
        }

        public static void CharacterAccessoriesUpdate(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE inventories SET Accessories=@Accessories WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);

            cmd.Parameters.AddWithValue("@Accessories", pInfo.Accessories.Select(x => x?.UID ?? 0).SerializeToJson());

            PushQuery(cmd);
        }

        public static void CharacterWeaponsUpdate(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE inventories SET Weapons=@Weapons WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);

            cmd.Parameters.AddWithValue("@Weapons", pInfo.Weapons.Select(x => x?.UID ?? 0).SerializeToJson());

            PushQuery(cmd);
        }

        public static void CharacterBagUpdate(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE inventories SET Bag=@Bag WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);

            cmd.Parameters.AddWithValue("@Bag", pInfo.Bag?.UID ?? 0);

            PushQuery(cmd);
        }

        public static void CharacterHolsterUpdate(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE inventories SET Holster=@Holster WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);

            cmd.Parameters.AddWithValue("@Holster", pInfo.Holster?.UID ?? 0);

            PushQuery(cmd);
        }

        public static void CharacterArmourUpdate(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE inventories SET Armour=@Armour WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);

            cmd.Parameters.AddWithValue("@Armour", pInfo.Armour?.UID ?? 0);

            PushQuery(cmd);
        }

        public static void CharacterFurnitureUpdate(PlayerData.PlayerInfo pInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE inventories SET Furniture=@Furniture WHERE ID=@ID";

            cmd.Parameters.AddWithValue("@ID", pInfo.CID);

            cmd.Parameters.AddWithValue("@Furniture", pInfo.Furniture.SerializeToJson());

            PushQuery(cmd);
        }

        #endregion

        #region Items
        public static void ItemUpdate(Game.Items.Item item)
        {
            var cmd = new MySqlCommand();

            if (item is Game.Items.IContainer cont)
            {
                cmd.CommandText = "UPDATE items SET Data=@Data, Items=@Items WHERE ID=@ID;";

                cmd.Parameters.AddWithValue("@Items", cont.Items.Select(x => x?.UID ?? 0).SerializeToJson());
            }
            else
                cmd.CommandText = "UPDATE items SET Data=@Data WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", item.UID);
            cmd.Parameters.AddWithValue("@Data", item.SerializeToJson());

            PushQuery(cmd);
        }

        #region Create
        public static void ItemAdd(Game.Items.Item item)
        {
            var cmd = new MySqlCommand();

            if (item is Game.Items.IContainer cont)
            {
                cmd.CommandText = "INSERT INTO items (ID, Data, Items) VALUES (@ID, @Data, @Items);";

                cmd.Parameters.AddWithValue("@Items", cont.Items.Select(x => x?.UID ?? 0).SerializeToJson());
            }
            else
            {
                cmd.CommandText = "INSERT INTO items (ID, Data) VALUES (@ID, @Data);";
            }

            cmd.Parameters.AddWithValue("@ID", item.UID);

            cmd.Parameters.AddWithValue("@Data", item.SerializeToJson());

            PushQuery(cmd);
        }
        #endregion

        public static void ItemDelete(Game.Items.Item item)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = $"DELETE FROM items WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", item.UID);

            PushQuery(cmd);
        }

        #endregion

        #region Containers
        public static void ContainerUpdate(Game.Items.Container cont)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE containers SET Items=@Items WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", cont.ID);
            cmd.Parameters.AddWithValue("@Items", cont.Items.Select(x => x?.UID ?? 0).SerializeToJson());

            PushQuery(cmd);
        }

        public static void ContainerAdd(Game.Items.Container cont)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "INSERT INTO containers (ID, SID, Items) VALUES (@ID, @SID, @Items);";

            cmd.Parameters.AddWithValue("@ID", cont.ID);

            cmd.Parameters.AddWithValue("@Items", cont.Items.Select(x => x?.UID ?? 0).SerializeToJson());
            cmd.Parameters.AddWithValue("@SID", cont.SID);

            PushQuery(cmd);
        }

        public static void ContainerDelete(Game.Items.Container cont)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "DELETE FROM containers WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", cont.ID);

            PushQuery(cmd);
        }

        #endregion

        #region Vehicles
        public static void VehicleAdd(VehicleData.VehicleInfo vInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "INSERT INTO vehicles (ID, SID, OwnerType, OwnerID, AllKeys, RegDate, Numberplate, Tuning, TID, LastData) VALUES (@ID, @SID, @OwnerType, @OwnerID, @AllKeys, @RegDate, @Numberplate, @Tuning, @TID, @LastData);";

            cmd.Parameters.AddWithValue("@ID", vInfo.VID);
            cmd.Parameters.AddWithValue("@SID", vInfo.ID);

            cmd.Parameters.AddWithValue("@OwnerType", (int)vInfo.OwnerType);
            cmd.Parameters.AddWithValue("@OwnerID", vInfo.OwnerID);

            cmd.Parameters.AddWithValue("@AllKeys", vInfo.AllKeys.SerializeToJson());
            cmd.Parameters.AddWithValue("@RegDate", vInfo.RegistrationDate);
            cmd.Parameters.AddWithValue("@Numberplate", vInfo.Numberplate?.UID ?? 0);
            cmd.Parameters.AddWithValue("@Tuning", vInfo.Tuning.SerializeToJson());

            cmd.Parameters.AddWithValue("@TID", vInfo.TID ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@LastData", vInfo.LastData.SerializeToJson());

            PushQuery(cmd);
        }

        public static void VehicleKeysUpdate(VehicleData.VehicleInfo vInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE vehicles SET AllKeys=@Keys WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", vInfo.VID);

            cmd.Parameters.AddWithValue("@Keys", vInfo.AllKeys.SerializeToJson());

            PushQuery(cmd);
        }

        public static void VehicleNumberplateUpdate(VehicleData.VehicleInfo vInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE vehicles SET Numberplate=@Numberplate WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", vInfo.VID);

            cmd.Parameters.AddWithValue("@Numberplate", vInfo.Numberplate?.UID ?? 0);

            PushQuery(cmd);
        }

        public static void VehicleOwnerUpdate(VehicleData.VehicleInfo vInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE vehicles SET OwnerType=@OwnerType, OwnerID=@OwnerID WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", vInfo.VID);

            cmd.Parameters.AddWithValue("@OwnerType", (int)vInfo.OwnerType);
            cmd.Parameters.AddWithValue("@OwnerID", vInfo.OwnerID);

            PushQuery(cmd);
        }

        public static void VehicleDeletionUpdate(VehicleData.VehicleInfo vInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE vehicles SET LastData=@LastData WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", vInfo.VID);

            cmd.Parameters.AddWithValue("@LastData", vInfo.LastData.SerializeToJson());

            PushQuery(cmd);
        }

        public static void VehicleTuningUpdate(VehicleData.VehicleInfo vInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE vehicles SET Tuning=@TuningWHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", vInfo.VID);

            cmd.Parameters.AddWithValue("@Tuning", vInfo.Tuning.SerializeToJson());

            PushQuery(cmd);
        }

        public static void VehicleDelete(VehicleData.VehicleInfo vInfo)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "DELETE FROM vehicles WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", vInfo.VID);

            PushQuery(cmd);
        }

        #endregion

        #region Businesses
        public static void LoadBusiness(Game.Businesses.Business business)
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
                            return;

                        reader.Read();

                        if (reader["CID"] is DBNull)
                            business.UpdateOwner(null);
                        else
                        {
                            business.UpdateOwner(PlayerData.PlayerInfo.Get(Convert.ToUInt32(reader["CID"])));

                            business.Owner?.OwnedBusinesses.Add(business);
                        }

                        business.Cash = (int)reader["Cash"];
                        business.Bank = (int)reader["Bank"];

                        business.Materials = (int)reader["Materials"];
                        business.OrderedMaterials = (int)reader["OrderedMaterials"];

                        business.Margin = (float)reader["Margin"];

                        business.Statistics = ((string)reader["Statistics"]).DeserializeFromJson<int[]>();

/*                        if (business is Game.Businesses.Farm)
                        {

                        }*/
                    }
                }
            }
        }
        #endregion

        #region Gifts

        public static void GiftAdd(Game.Items.Gift gift)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "INSERT INTO gifts (ID, CID, Type, GID, Variation, Amount, Reason) VALUES (@ID, @CID, @Type, @GID, @Variation, @Amount, @Reason);";

            cmd.Parameters.AddWithValue("@ID", gift.ID);

            cmd.Parameters.AddWithValue("@CID", gift.CID);
            cmd.Parameters.AddWithValue("@Type", gift.Type);
            cmd.Parameters.AddWithValue("@GID", (object)gift.GID ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Variation", gift.Variation);
            cmd.Parameters.AddWithValue("@Amount", gift.Amount);
            cmd.Parameters.AddWithValue("@Reason", gift.SourceType);

            PushQuery(cmd);
        }

        public static void GiftDelete(Game.Items.Gift gift)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "DELETE FROM gifts WHERE ID = @ID;";

            cmd.Parameters.AddWithValue("ID", gift.ID);

            PushQuery(cmd);
        }

        #endregion

        #region Houses
        public static void LoadHouse(Game.Houses.House house)
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
                            return;

                        reader.Read();

                        if (reader["CID"] is DBNull)
                            house.UpdateOwner(null);
                        else
                        {
                            var pInfo = PlayerData.PlayerInfo.Get(Convert.ToUInt32(reader["CID"]));

                            pInfo?.OwnedHouses.Add(house);

                            house.UpdateOwner(pInfo);
                        }

                        house.StyleData = Game.Houses.HouseBase.Style.Get(house.Type, house.RoomType, (Game.Houses.HouseBase.Style.Types)(int)reader["StyleType"]);

                        house.Settlers = NAPI.Util.FromJson<Dictionary<uint, bool[]>>((string)reader["Settlers"]).ToDictionary(x => PlayerData.PlayerInfo.Get(x.Key), x => x.Value);

                        foreach (var x in house.Settlers.Keys)
                            x.SettledHouses.Add(house);

                        house.IsLocked = (bool)reader["IsLocked"];
                        house.ContainersLocked = (bool)reader["ContainersLocked"];

                        house.Furniture = ((string)reader["Furniture"]).DeserializeFromJson<List<uint>>().Select(x => Game.Houses.Furniture.Get(x)).Where(x => x != null).ToList();

                        cmd.CommandText = "";

                        if (reader["Locker"] == DBNull.Value)
                        {
                            house.Locker = Game.Items.Container.Create("h_locker", null).ID;

                            cmd.CommandText += $"UPDATE houses SET Locker={house.Locker} WHERE ID={house.ID};";
                        }
                        else
                            house.Locker = Convert.ToUInt32(reader["Locker"]);

                        if (reader["Wardrobe"] == DBNull.Value)
                        {
                            house.Wardrobe = Game.Items.Container.Create("h_wardrobe", null).ID;

                            cmd.CommandText += $"UPDATE houses SET Wardrobe={house.Wardrobe} WHERE ID={house.ID};";
                        }
                        else
                            house.Wardrobe = Convert.ToUInt32(reader["Wardrobe"]);

                        if (reader["Fridge"] == DBNull.Value)
                        {
                            house.Fridge = Game.Items.Container.Create("h_fridge", null).ID;

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
                }
            }
        }

        public static void FurnitureAdd(Game.Houses.Furniture furn)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "INSERT INTO furniture (ID, Type, Data) VALUES (@ID, @Type, @Data);";

            cmd.Parameters.AddWithValue("@ID", furn.UID);
            cmd.Parameters.AddWithValue("@Type", furn.ID);
            cmd.Parameters.AddWithValue("@Data", furn.Data.SerializeToJson());

            PushQuery(cmd);
        }

        public static void FurnitureDelete(Game.Houses.Furniture furn)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "DELETE FROM furniture WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", furn.UID);

            PushQuery(cmd);
        }

        /*        public static Game.Houses.Apartments GetApartments(Game.Houses.Apartments apartments)
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

                                apartments.Owner = Convert.ToUInt32(reader["CID"]);
                                apartments.StyleType = (Game.Houses.HouseBase.Style.Types)(int)reader["StyleType"];
                                apartments.Settlers = NAPI.Util.FromJson<List<uint>>((string)reader["Settlers"]);
                                apartments.IsLocked = (bool)reader["IsLocked"];
                                apartments.Vehicles = NAPI.Util.FromJson<List<int>>((string)reader["Vehicles"]);

                                if (reader["Locker"] == DBNull.Value)
                                    apartments.Locker = Game.Items.Container.Create("h_locker", null).ID;
                                else
                                    apartments.Locker = Convert.ToUInt32(reader["Locker"]);

                                if (reader["Wardrobe"] == DBNull.Value)
                                    apartments.Wardrobe = Game.Items.Container.Create("h_wardrobe", null).ID;
                                else
                                    apartments.Wardrobe = Convert.ToUInt32(reader["Wardrobe"]);

                                if (reader["Fridge"] == DBNull.Value)
                                    apartments.Fridge = Game.Items.Container.Create("h_fridge", null).ID;
                                else
                                    apartments.Fridge = Convert.ToUInt32(reader["Fridge"]);

                                return apartments;
                            }
                        }
                    }
                }*/
        #endregion
    }
}
