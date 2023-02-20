using GTANetworkAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public static partial class MySQL
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

        private static ConcurrentQueue<MySqlCommand> QueriesQueue { get; set; } = new ConcurrentQueue<MySqlCommand>();

        public enum AuthResults
        {
            RegMailNotFree = 0,
            RegLoginNotFree,

            RegOk,
        }

        #region General

        public static void StartService()
        {
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

            if (commands.Count == 0)
                return;

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

            try
            {
                using (var conn = new MySqlConnection(LocalConnectionCredentials))
                {
                    conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            foreach (var x in commands)
                            {
                                using (x)
                                {
                                    x.Connection = conn;
                                    x.Transaction = transaction;

                                    x.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();

                            Console.WriteLine($"{commands.Count} local queries were done!");
                        }
                        catch (Exception exC)
                        {
                            try
                            {
                                transaction.Rollback();
                            }
                            catch (Exception exTr)
                            {

                            }
                        }
                    }
                }
            }
            catch (Exception exG)
            {

            }
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

        public static void UpdateServerData()
        {
            using (var conn = new MySqlConnection(LocalConnectionCredentials))
            {
                conn.Open();

                var currentTime = Utils.GetCurrentTime();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM server_data; UPDATE server_data SET LastLaunchTime=@LLT";

                    cmd.Parameters.AddWithValue("@LLT", currentTime);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var llt = (DateTime)reader["LastLaunchTime"];

                                var businessMaxDayIdx = 14;

                                var timePassedSinceLastLaunch = currentTime.Subtract(llt);

                                var bTime = DateTime.MinValue;

                                Game.Businesses.Business.PreviousStatisticsDayIdx = ((int)Math.Floor(llt.Subtract(bTime).TotalDays)) % businessMaxDayIdx;

                                Game.Businesses.Business.CurrentStatisticsDayIdx = (Game.Businesses.Business.PreviousStatisticsDayIdx + ((int)Math.Floor(timePassedSinceLastLaunch.TotalDays))) % businessMaxDayIdx;
                            }
                        }
                    }
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
                                var data = ((string)reader["Data"]).DeserializeFromJson<Utils.Vector4>();

                                Game.Estates.Furniture.AddOnLoad(new Game.Estates.Furniture(uid, id, data));
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

                                var ownersCount = Convert.ToUInt32(reader["OwnersCount"]);

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

                                    OwnersCount = ownersCount,
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

                                var hBlend = ((string)reader["HeadBlend"]).DeserializeFromJson<Game.Data.Customization.HeadBlend>();
                                var hOverlays = ((string)reader["HeadOverlays"]).DeserializeFromJson<Dictionary<int, Game.Data.Customization.HeadOverlay>>();
                                var fFeatures = ((string)reader["FaceFeatures"]).DeserializeFromJson<float[]>();
                                var decors = ((string)reader["Decorations"]).DeserializeFromJson<List<int>>();
                                var hStyle = ((string)reader["HairStyle"]).DeserializeFromJson<Game.Data.Customization.HairStyle>();
                                var eyeColour = Convert.ToByte(reader["EyeColor"]);

                                customizations.Add(cid, new object[] { hBlend, hOverlays, fFeatures, decors, hStyle, eyeColour });
                            }
                        }
                    }

                    cmd.CommandText = "SELECT * FROM gifts;";

                    var allGifts = new Dictionary<Game.Items.Gift, uint>();

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

                                var gift = new Game.Items.Gift(id, reason, type, gid, variation, amount);

                                Game.Items.Gift.AddOnLoad(gift);

                                allGifts.Add(gift, cid);
                            }
                        }
                    }

                    var allAchievements = new Dictionary<PlayerData.Achievement, uint>();

                    cmd.CommandText = "SELECT * FROM achievements;";

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            var types = Enum.GetValues(typeof(PlayerData.Achievement.Types)).Cast<PlayerData.Achievement.Types>();

                            while (reader.Read())
                            {
                                var cid = Convert.ToUInt32(reader["ID"]);

                                foreach (var x in types)
                                {
                                    var data = ((string)reader[x.ToString()]).DeserializeFromJson<JObject>();

                                    var achievement = new PlayerData.Achievement(x, (int)data["P"], (bool)data["IR"]);

                                    allAchievements.Add(achievement, cid);
                                }
                            }
                        }
                    }

                    var allQuests = new Dictionary<Sync.Quest, uint>();

                    cmd.CommandText = "SELECT * FROM quests;";

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            var types = Enum.GetValues(typeof(Sync.Quest.QuestData.Types)).Cast<Sync.Quest.QuestData.Types>().Where(x => !Sync.Quest.IsQuestTemp(x)).ToList();

                            while (reader.Read())
                            {
                                var cid = Convert.ToUInt32(reader["ID"]);

                                foreach (var x in types)
                                {
                                    var obj = reader[x.ToString()];

                                    if (obj == DBNull.Value)
                                        continue;

                                    var data = ((string)obj).DeserializeFromJson<JObject>();

                                    var quest = new Sync.Quest(x, (bool)data["C"], (byte)data["S"], (int)data["SP"]);

                                    allQuests.Add(quest, cid);
                                }
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

                                var medCard = reader["MedicalCard"] is DBNull ? null : ((string)reader["MedicalCard"]).DeserializeFromJson<PlayerData.MedicalCard>();

                                var lsa = (bool)reader["LosSantosAllowed"];

                                var fraction = (PlayerData.FractionTypes)(int)reader["Fraction"];

                                var orgId = (int)reader["OrgID"];

                                var cash = Convert.ToUInt64(reader["Cash"]);

                                var phoneNumber = Convert.ToUInt32(reader["PhoneNumber"]);

                                Sync.Players.UsedPhoneNumbers.Add(phoneNumber);

                                var phoneBalance = Convert.ToUInt32(reader["PhoneBalance"]);

                                var contacts = ((string)reader["Contacts"]).DeserializeFromJson<Dictionary<uint, string>>();

                                var phoneBlacklist = ((string)reader["PhoneBL"]).DeserializeFromJson<List<uint>>();

                                var lastData = ((string)reader["LastData"]).DeserializeFromJson<PlayerData.LastPlayerData>();

                                var familiars = ((string)reader["Familiars"]).DeserializeFromJson<List<uint>>();

                                var skills = ((string)reader["Skills"]).DeserializeFromJson<Dictionary<PlayerData.SkillTypes, int>>();

                                var punishments = GetPunishmentsByCID(cid);

                                var gifts = allGifts.Where(x => x.Value == cid).Select(x => x.Key).ToList();

                                foreach (var x in gifts)
                                    allGifts.Remove(x);

                                var achievements = allAchievements.Where(x => x.Value == cid).ToDictionary(x => x.Key.Type, y => y.Key);

                                foreach (var x in achievements.Values)
                                    allAchievements.Remove(x);

                                var quests = allQuests.Where(x => x.Value == cid).ToDictionary(x => x.Key.Type, y => y.Key);

                                foreach (var x in quests.Values)
                                    allQuests.Remove(x);

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

                                    MedicalCard = medCard,

                                    LosSantosAllowed = lsa,

                                    Fraction = fraction,

                                    OrganisationID = orgId,

                                    Cash = cash,

                                    PhoneNumber = phoneNumber,

                                    PhoneBalance = phoneBalance,

                                    Contacts = contacts,

                                    PhoneBlacklist = phoneBlacklist,

                                    BankAccount = GetBankAccountByCID(cid),

                                    LastData = lastData,

                                    Familiars = familiars,

                                    Skills = skills,

                                    Punishments = punishments,

                                    HeadBlend = (Game.Data.Customization.HeadBlend)customizations[cid][0],
                                    HeadOverlays = (Dictionary<int, Game.Data.Customization.HeadOverlay>)customizations[cid][1],
                                    FaceFeatures = (float[])customizations[cid][2],
                                    Decorations = (List<int>)customizations[cid][3],
                                    HairStyle = (Game.Data.Customization.HairStyle)customizations[cid][4],
                                    EyeColor = (byte)customizations[cid][5],

                                    Gifts = gifts,

                                    Achievements = achievements,

                                    Quests = quests,
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

                                var wskins = ((string)reader["WSkins"]).DeserializeFromJson<Dictionary<Game.Items.WeaponSkin.ItemData.Types, uint>>();

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

                                pInfo.Furniture = furniture.Select(x => Game.Estates.Furniture.Get(x)).Where(x => x != null).ToList();

                                pInfo.WeaponSkins = wskins.Where(x => Game.Items.Item.Get(x.Value) is Game.Items.WeaponSkin).ToDictionary(x => x.Key, x => (Game.Items.WeaponSkin)Game.Items.Item.Get(x.Value));
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
                    var usedItems = new List<Game.Items.Item>();

                    usedItems.AddRange(Game.Items.Container.All.Values.SelectMany(x => x.Items));

                    usedItems.AddRange(PlayerData.PlayerInfo.All.Values.SelectMany(x => x.Items.Concat(x.Clothes).Concat(x.Weapons).Concat(x.Accessories).Concat(x.WeaponSkins.Values).Concat(new Game.Items.Item[] { x.Bag, x.Holster, x.Armour })));

                    usedItems.AddRange(VehicleData.VehicleInfo.All.Values.Select(x => x.Numberplate));

                    usedItems.RemoveAll(x => x == null);

                    foreach (var x in usedItems.ToList())
                    {
                        if (x is Game.Items.IContainer cont)
                        {
                            usedItems.AddRange(cont.Items);

                            foreach (var y in cont.Items)
                            {
                                if (y is Game.Items.IContainer cont1)
                                {
                                    usedItems.AddRange(cont1.Items);
                                }
                            }
                        }
                    }

                    var toDel = Game.Items.Item.All.Values.Except(usedItems).ToList();

                    toDel.ForEach(x => Game.Items.Item.RemoveOnLoad(x));

                    if (toDel.Count > 0)
                    {
                        if (toDel.Count > 1)
                            cmd.CommandText = $"DELETE FROM Items WHERE ID IN ({string.Join(',', toDel.Select(x => x.UID))})";
                        else
                            cmd.CommandText = $"DELETE FROM Items WHERE ID={toDel[0].UID}";

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
                        if (Game.Items.Item.Get(i) == null && !Game.Items.Item.FreeIDs.Contains(i))
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

                    var allGifts = PlayerData.PlayerInfo.All.SelectMany(x => x.Value.Gifts).ToDictionary(x => x.ID, y => y);

                    for (uint i = 1; i < nextAi; i++)
                    {
                        if (!allGifts.ContainsKey(i))
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
                        if (Game.Estates.Furniture.Get(i) == null)
                            Game.Estates.Furniture.AddFreeId(i);
                    }
                }

                foreach (var x in Game.Items.Item.All.Values)
                {
                    if (x is Game.Items.Numberplate np)
                    {
                        np.AddTagToUsed();
                    }
                }
            }
        }

        #endregion
    }
}
