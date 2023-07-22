using GTANetworkAPI;
using MySqlConnector;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlaineRP.Server
{
    public static partial class MySQL
    {
        private static MySqlConnectionStringBuilder _localConnectionCredentials => new MySqlConnectionStringBuilder()
        {
            Server = Properties.Settings.Profile.Current.DataBase.OwnDbHost,

            Database = Properties.Settings.Profile.Current.DataBase.OwnDbName,

            UserID = Properties.Settings.Profile.Current.DataBase.OwnDbUser,

            Password = Properties.Settings.Profile.Current.DataBase.OwnDbPassword,
        };

        private static SemaphoreSlim _localConnectionSemaphore { get; set; }

        private static ConcurrentQueue<MySqlCommand> QueriesQueue { get; set; } = new ConcurrentQueue<MySqlCommand>();

        #region General

        private static Timer QueriesServiceTimer;

        public static void StartService()
        {
            if (QueriesServiceTimer != null)
                return;

            QueriesServiceTimer = new Timer(async (obj) =>
            {
                NAPI.Task.Run(async () =>
                {
                    await DoAllQueries();
                });
            }, null, 0, 10_000);
        }

        public static async Task DoAllQueries()
        {
            var commands = new List<MySqlCommand>();

            MySqlCommand qCmd;

            while (QueriesQueue.TryDequeue(out qCmd))
            {
                if (qCmd != null)
                    commands.Add(qCmd);
            }

            if (commands.Count == 0)
                return;

            //Console.WriteLine($"AllQ: {string.Join('\n', commands.Select(x => x.CommandText))}");

            for (int i = commands.Count - 1; i >= 0; i--)
            {
                var cmd = commands[i];

                var cmdIdParam = cmd.Parameters.Contains("@ID") ? cmd.Parameters["@ID"].Value.ToString() : null;

                if (cmdIdParam != null)
                {
                    var cmdText = cmd.CommandText;

                    MySqlCommand lastCmd = null;

                    for (int j = commands.Count - 1; j >= 0; j--)
                    {
                        var sCmd = commands[j];

                        if (sCmd.Parameters.Contains("@ID") && sCmd.Parameters["@ID"].Value.ToString() == cmdIdParam && sCmd.CommandText == cmdText)
                        {
                            if (lastCmd == null)
                            {
                                lastCmd = sCmd;
                            }
                            else
                            {
                                using (sCmd)
                                {
                                    commands.RemoveAt(j);

                                    i--;

                                    //Console.WriteLine($"Removed: {cmdText}");
                                }
                            }
                        }
                    }
                }
            }

            using (var cts = new CancellationTokenSource(5_000))
            {
                try
                {
/*                    await Web.SocketIO.Methods.Misc.SqlTransactionLocalDbSend(cts.Token, commands.Select(x =>
                    {
                        var cmd = x.CommandText;

                        // todo
                    }));*/
                    using (var conn = new MySqlConnection(_localConnectionCredentials.ConnectionString))
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
        }

        public static void PushQuery(MySqlCommand cmd) => QueriesQueue.Enqueue(cmd);

        #region Init Connection
        public static bool InitConnection()
        {
            MySqlConnection localConnection = new MySqlConnection(_localConnectionCredentials.ConnectionString);

            _localConnectionSemaphore = new SemaphoreSlim(1, 1);

            try
            {
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

        public static async Task Wait() => await _localConnectionSemaphore.WaitAsync();

        public static void Release() => _localConnectionSemaphore.Release();

        public static int SetOfflineAll()
        {
            using (var conn = new MySqlConnection(_localConnectionCredentials.ConnectionString))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE characters SET IsOnline=false";

                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public static void LoadAll(out int loadedItemsAmount)
        {
            var currentTime = Utils.GetCurrentTime();

            int businessPrevStatsDayIdx = 0;

            var allItems = new Dictionary<uint, Game.Items.Item>();

            var usedItems = new HashSet<uint>();

            Game.Items.Item getItemAndRemove(uint uid)
            {
                Game.Items.Item returnItem;

                if (allItems.Remove(uid, out returnItem))
                {
                    Game.Items.Item.UidHandler.TryUpdateLastAddedMaxUid(uid);

                    usedItems.Add(uid);

                    if (returnItem is Game.Items.Numberplate np)
                    {
                        np.AddTagToUsed();
                    }

                    return returnItem;
                }

                return null;
            }

/*            using (var conn = new MySqlConnection($"SERVER={Host}; DATABASE=fiveup_main; UID={User}; PASSWORD={Password}"))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT houses.position AS house_position, garages.position AS garage_position, garages.rotation AS garage_rotation FROM houses JOIN garages ON houses.garage = garages.id;";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var pos = ((string)reader["house_position"]).DeserializeFromJson<Vector3>();
                            var garagePos = ((string)reader["garage_position"]).DeserializeFromJson<Vector3>();
                            var garageRot = ((string)reader["garage_rotation"]).DeserializeFromJson<Vector3>();

                            File.AppendAllText(@"houses_coords.txt", $"new Vector3({pos.X}f, {pos.Y}f, {pos.Z}f),\n");
                        }
                    }
                }
            }*/

            using (var conn = new MySqlConnection(_localConnectionCredentials.ConnectionString))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM server_data;";

                    var actionDict = new Dictionary<string, Action<string>>()
                    {
                        {
                            "LAST_LAUNCH_TIME",

                            (dataJson) =>
                            {
                                var llt = dataJson.DeserializeFromJson<DateTime>();

                                var businessMaxDayIdx = 14;

                                var timePassedSinceLastLaunch = currentTime.Subtract(llt);

                                var bTime = DateTime.MinValue;

                                businessPrevStatsDayIdx = ((byte)Math.Floor(llt.Subtract(bTime).TotalDays)) % businessMaxDayIdx;

                                Game.Businesses.Business.CurrentStatisticsDayIdx = (businessPrevStatsDayIdx + ((int)Math.Floor(timePassedSinceLastLaunch.TotalDays))) % businessMaxDayIdx;
                            }
                        },

                        {
                            "MARKETSTALL_RENT_PRICE",

                            (dataJson) =>
                            {
                                Game.Misc.MarketStall.RentPrice = dataJson.DeserializeFromJson<uint>();
                            }
                        },
                    };

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var id = (string)reader["ID"];

                                var dataJson = reader["Data"] is DBNull ? null : (string)reader["Data"];

                                actionDict.GetValueOrDefault(id)?.Invoke(dataJson);
                            }
                        }
                    }


                    UpdateServerData("LAST_LAUNCH_TIME", currentTime);
                }

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
                                var uid = Convert.ToUInt32(reader["ID"]);

                                var item = ((string)reader["Data"]).DeserializeFromJson<Game.Items.Item>();

                                if (item == null)
                                    continue;

                                if (reader["Items"] is string iDataStr)
                                    includedItems.Add(uid, iDataStr.DeserializeFromJson<uint[]>());

                                item.UID = uid;

                                allItems.Add(uid, item);
                            }
                        }
                    }

                    foreach (var x in includedItems.Keys)
                    {
                        if (allItems.GetValueOrDefault(x) is Game.Items.IContainer container)
                        {
                            container.Items = new Game.Items.Item[includedItems[x].Length];

                            for (int i = 0; i < includedItems[x].Length; i++)
                            {
                                if (includedItems[x][i] == 0)
                                    continue;

                                container.Items[i] = getItemAndRemove(includedItems[x][i]);
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

                                var items = ((string)reader["Items"]).DeserializeFromJson<List<uint>>().Select(x => x == 0 ? null : getItemAndRemove(x)).ToArray();

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

                                var keysUid = reader["KeysUid"] is Guid keysUidG ? keysUidG : Guid.Empty;

                                var tid = Convert.ToUInt32(reader["TID"]);

                                var numberplateUid = Convert.ToUInt32(reader["Numberplate"]);

                                var tuning = ((string)reader["Tuning"]).DeserializeFromJson<Game.Data.Vehicles.Tuning>();

                                var lastData = ((string)reader["LastData"]).DeserializeFromJson<VehicleData.LastVehicleData>();

                                var regDate = (DateTime)reader["RegDate"];

                                var ownersCount = Convert.ToUInt32(reader["OwnersCount"]);

                                var regNumberplate = reader["RegNumberplate"] is string str ? str : null;

                                var vInfo = new VehicleData.VehicleInfo()
                                {
                                    VID = vid,

                                    ID = sid,

                                    KeysUid = keysUid,

                                    OwnerType = oType,

                                    OwnerID = oId,

                                    RegistrationDate = regDate,

                                    TID = tid,

                                    Numberplate = numberplateUid == 0 ? null : getItemAndRemove(numberplateUid) as Game.Items.Numberplate,

                                    Tuning = tuning,

                                    LastData = lastData,

                                    Data = Game.Data.Vehicles.All[sid],

                                    OwnersCount = ownersCount,

                                    RegisteredNumberplate = regNumberplate,
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

                                    var achievement = new PlayerData.Achievement(x, Convert.ToUInt32(data["P"]), (bool)data["IR"]);

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

                    var allCooldowns = new Dictionary<uint, Dictionary<uint, Sync.Cooldown>>();

                    cmd.CommandText = "SELECT * FROM cooldowns;";

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var guid = (Guid)reader["ID"];
                                var cid = Convert.ToUInt32(reader["CID"]);
                                var hash = Convert.ToUInt32(reader["Hash"]);

                                var startDate = (DateTime)reader["StartDate"];
                                var time = TimeSpan.FromSeconds(Utils.ToUInt64(reader["Time"]));

                                var pCd = allCooldowns.GetValueOrDefault(cid);

                                var cdObj = new Sync.Cooldown(startDate, time, guid);

                                if (pCd == null)
                                {
                                    pCd = new Dictionary<uint, Sync.Cooldown>() { { hash, cdObj }, };

                                    allCooldowns.Add(cid, pCd);
                                }
                                else
                                {
                                    pCd.TryAdd(hash, cdObj);
                                }
                            }
                        }
                    }

                    var allPunishments = new Dictionary<Sync.Punishment, uint>();

                    cmd.CommandText = "SELECT * FROM punishments;";

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var id = Convert.ToUInt32(reader["ID"]);
                                var cid = Convert.ToUInt32(reader["CID"]);
                                var punisherCid = Convert.ToUInt32(reader["PunisherID"]);
                                var type = (Sync.Punishment.Types)Convert.ToInt32(reader["Type"]);
                                var startDate = (DateTime)reader["StartDate"];
                                var endDate = (DateTime)reader["EndDate"];
                                var reason = (string)reader["Reason"];

                                var obj = new Sync.Punishment(id, type, reason, startDate, endDate, punisherCid);

                                var amnestyObj = reader["Amnesty"];

                                var dataObj = reader["Data"];

                                if (dataObj != DBNull.Value)
                                    obj.AdditionalData = ((string)dataObj).DeserializeFromJson<string>();

                                if (amnestyObj != DBNull.Value)
                                    obj.AmnestyInfo = ((string)amnestyObj).DeserializeFromJson<Sync.Punishment.Amnesty>();
                                else if (!obj.IsActive())
                                    obj.AmnestyInfo = new Sync.Punishment.Amnesty();

                                allPunishments.Add(obj, cid);

                                if (id > Sync.Punishment.MaxAddedId)
                                    Sync.Punishment.MaxAddedId = id;
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

                                var timePlayed = TimeSpan.FromMinutes(Convert.ToUInt64(reader["TimePlayed"]));

                                var name = (string)reader["Name"];

                                var surname = (string)reader["Surname"];

                                var sex = (bool)reader["Sex"];

                                var birthDate = (DateTime)reader["BirthDate"];

                                var licenses = ((string)reader["Licenses"]).DeserializeFromJson<HashSet<PlayerData.LicenseTypes>>();

                                var medCard = reader["MedicalCard"] is DBNull ? null : ((string)reader["MedicalCard"]).DeserializeFromJson<PlayerData.MedicalCard>();

                                var lsa = (bool)reader["LosSantosAllowed"];

                                var fractionInfo = reader["Fraction"] is DBNull ? null : ((string)reader["Fraction"]).Split('_');

                                var orgId = (int)reader["OrgID"];

                                var cash = Convert.ToUInt64(reader["Cash"]);

                                var phoneNumber = Convert.ToUInt32(reader["PhoneNumber"]);

                                Sync.Players.UsedPhoneNumbers.Add(phoneNumber);

                                var phoneBalance = Convert.ToUInt32(reader["PhoneBalance"]);

                                var casinoChips = Convert.ToUInt32(reader["CasinoChips"]);

                                var contacts = ((string)reader["Contacts"]).DeserializeFromJson<Dictionary<uint, string>>();

                                var phoneBlacklist = ((string)reader["PhoneBL"]).DeserializeFromJson<List<uint>>();

                                var lastData = ((string)reader["LastData"]).DeserializeFromJson<PlayerData.LastPlayerData>();

                                var familiars = ((string)reader["Familiars"]).DeserializeFromJson<HashSet<uint>>();

                                var skills = ((string)reader["Skills"]).DeserializeFromJson<Dictionary<PlayerData.SkillTypes, int>>();

                                var punishments = allPunishments.Where(x => x.Value == cid).Select(x => x.Key).ToList();

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

                                    Fraction = fractionInfo == null ? Game.Fractions.Types.None : (Game.Fractions.Types)int.Parse(fractionInfo[0]),

                                    FractionRank = fractionInfo == null ? (byte)0 : byte.Parse(fractionInfo[1]),

                                    OrganisationID = orgId,

                                    Cash = cash,

                                    PhoneNumber = phoneNumber,

                                    PhoneBalance = phoneBalance,

                                    Contacts = contacts,

                                    PhoneBlacklist = phoneBlacklist,

                                    LastData = lastData,

                                    Familiars = familiars,

                                    Skills = skills,

                                    Punishments = punishments,

                                    CasinoChips = casinoChips,

                                    HeadBlend = (Game.Data.Customization.HeadBlend)customizations[cid][0],
                                    HeadOverlays = (Dictionary<int, Game.Data.Customization.HeadOverlay>)customizations[cid][1],
                                    FaceFeatures = (float[])customizations[cid][2],
                                    Decorations = (List<int>)customizations[cid][3],
                                    HairStyle = (Game.Data.Customization.HairStyle)customizations[cid][4],
                                    EyeColor = (byte)customizations[cid][5],

                                    Gifts = gifts,

                                    Achievements = achievements,

                                    Quests = quests,

                                    Cooldowns = allCooldowns.GetValueOrDefault(cid) ?? new Dictionary<uint, Sync.Cooldown>(),
                                };

                                PlayerData.PlayerInfo.AddOnLoad(pInfo);
                            }
                        }
                    }

                    cmd.CommandText = "SELECT * FROM bank_accounts;";

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var cid = Convert.ToUInt32(reader["ID"]);
                                var balance = Convert.ToUInt64(reader["Balance"]);
                                var savings = Convert.ToUInt64(reader["Savings"]);
                                var tariff = (Game.Bank.Tariff.Types)(int)reader["Tariff"];
                                var std = (bool)reader["STD"];

                                var pInfo = PlayerData.PlayerInfo.Get(cid);

                                if (pInfo != null)
                                {
                                    pInfo.BankAccount = new Game.Bank.Account()
                                    {
                                        Balance = balance,
                                        SavingsBalance = savings,
                                        Tariff = Game.Bank.Tariff.All[tariff],
                                        SavingsToDebit = std,
                                        MinSavingsBalance = savings,
                                        TotalDayTransactions = 0,

                                        PlayerInfo = pInfo,
                                    };
                                }
                            }
                        }
                        else
                        {

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

                                var pInfo = PlayerData.PlayerInfo.Get(cid);

                                if (pInfo == null)
                                    continue;

                                var holster = Convert.ToUInt32(reader["Holster"]);
                                var arm = Convert.ToUInt32(reader["Armour"]);
                                var bag = Convert.ToUInt32(reader["Bag"]);

                                pInfo.Items = ((string)reader["Items"]).DeserializeFromJson<List<uint>>().Select(x => x == 0 ? null : getItemAndRemove(x)).ToArray();
                                pInfo.Clothes = ((string)reader["Clothes"]).DeserializeFromJson<List<uint>>().Select(x => x == 0 ? null : getItemAndRemove(x) as Game.Items.Clothes).ToArray();
                                pInfo.Accessories = ((string)reader["Accessories"]).DeserializeFromJson<List<uint>>().Select(x => x == 0 ? null : getItemAndRemove(x) as Game.Items.Clothes).ToArray();
                                pInfo.Weapons = ((string)reader["Weapons"]).DeserializeFromJson<List<uint>>().Select(x => x == 0 ? null : getItemAndRemove(x) as Game.Items.Weapon).ToArray();

                                pInfo.Holster = holster == 0 ? null : getItemAndRemove(holster) as Game.Items.Holster;
                                pInfo.Armour = arm == 0 ? null : getItemAndRemove(arm) as Game.Items.Armour;
                                pInfo.Bag = bag == 0 ? null : getItemAndRemove(bag) as Game.Items.Bag;

                                pInfo.Furniture = ((string)reader["Furniture"]).DeserializeFromJson<List<uint>>().Select(x => Game.Estates.Furniture.Get(x)).Where(x => x != null).ToList();
                                
                                pInfo.WeaponSkins = ((string)reader["WSkins"]).DeserializeFromJson<List<uint>>().Select(x => getItemAndRemove(x) as Game.Items.WeaponSkin).Where(x => x != null).ToList();
                            }
                        }
                    }

                    var farmsData = new Dictionary<uint, string>();

                    var farmBiz = Game.Businesses.Business.Get(38) as Game.Businesses.Farm;

                    for (int i = 0; i < farmBiz.CropFields.Count; i++)
                    {
                        var key = $"FARM::CFI_{farmBiz.ID}_{i}";

                        cmd.CommandText = $"INSERT INTO farms_data (ID, Data) VALUES ({NAPI.Util.GetHashKey(key)}, @Data) ON DUPLICATE KEY UPDATE Data=@Data;";

                        cmd.Parameters.AddWithValue("@Data", DBNull.Value);

                        cmd.ExecuteNonQuery();

                        cmd.Parameters.Clear();

                        for (byte j = 0; j < farmBiz.CropFields[i].CropsData.Count; j++)
                        {
                            for (byte k = 0; k < farmBiz.CropFields[i].CropsData[j].Count; k++)
                            {
                                key = $"FARM::CF_{farmBiz.ID}_{i}_{j}_{k}";

                                cmd.CommandText = $"INSERT INTO farms_data (ID, Data) VALUES ({NAPI.Util.GetHashKey(key)}, 0) ON DUPLICATE KEY UPDATE Data=0;";

                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    for (int i = 0; i < farmBiz.OrangeTrees.Count; i++)
                    {
                        var key = $"FARM::OT_{farmBiz.ID}_{i}";

                        cmd.CommandText = $"INSERT INTO farms_data (ID, Data) VALUES ({NAPI.Util.GetHashKey(key)}, {0}) ON DUPLICATE KEY UPDATE Data=0;";

                        cmd.ExecuteNonQuery();
                    }

                    for (int i = 0; i < farmBiz.Cows.Count; i++)
                    {
                        var key = $"FARM::COW_{farmBiz.ID}_{i}";

                        cmd.CommandText = $"INSERT INTO farms_data (ID, Data) VALUES ({NAPI.Util.GetHashKey(key)}, {0}) ON DUPLICATE KEY UPDATE Data=0;";

                        cmd.ExecuteNonQuery();
                    }

                    cmd.CommandText = "SELECT * FROM farms_data;";

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var data = reader["Data"];

                                farmsData.Add(Convert.ToUInt32(reader["ID"]), data is DBNull ? null : (string)data);
                            }
                        }
                    }

                    cmd.CommandText = "SELECT * FROM businesses;";

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            var daysDiff = Game.Businesses.Business.CurrentStatisticsDayIdx - businessPrevStatsDayIdx;

                            while (reader.Read())
                            {
                                var bizId = Convert.ToInt32(reader["ID"]);

                                var business = Game.Businesses.Business.Get(bizId);

                                if (business == null)
                                    continue;

                                if (reader["CID"] is DBNull)
                                {
                                    business.UpdateOwner(null);
                                }
                                else
                                {
                                    business.UpdateOwner(PlayerData.PlayerInfo.Get(Convert.ToUInt32(reader["CID"])));
                                }

                                business.Cash = Convert.ToUInt64(reader["Cash"]);
                                business.Bank = Convert.ToUInt64(reader["Bank"]);

                                business.IncassationState = (bool)reader["IncassationState"];

                                business.Materials = Convert.ToUInt32(reader["Materials"]);
                                business.OrderedMaterials = Convert.ToUInt32(reader["OrderedMaterials"]);

                                business.Margin = (decimal)(float)reader["Margin"];

                                business.Tax = (decimal)(float)reader["Tax"];
                                business.Rent = Convert.ToUInt32(reader["Rent"]);

                                business.GovPrice = Convert.ToUInt32(reader["GovPrice"]);

                                business.Statistics = ((string)reader["Statistics"]).DeserializeFromJson<ulong[]>();

                                for (int j = 0; j < daysDiff; j++)
                                {
                                    for (int i = 0; i < business.Statistics.Length; i++)
                                    {
                                        if (i == business.Statistics.Length - 1)
                                        {
                                            business.Statistics[i] = 0;
                                        }
                                        else
                                        {
                                            business.Statistics[i] = business.Statistics[i + 1];
                                        }
                                    }
                                }

                                if (daysDiff > 0)
                                {
                                    MySQL.BusinessUpdateStatistics(business);
                                }

                                if (business.OrderedMaterials > 0)
                                {
                                    business.AddOrder(true, null);
                                }

                                if (business is Game.Businesses.Farm farm)
                                {
                                    //var time = ((DateTimeOffset)(Utils.GetCurrentTime().AddSeconds(30))).ToUnixTimeSeconds();

                                    // load crop fields data
                                    if (farm.CropFields != null)
                                    {
                                        for (int i = 0; i < farm.CropFields.Count; i++)
                                        {
                                            var key = $"FARM::CFI_{business.ID}_{i}";

                                            var data = farmsData.GetValueOrDefault(NAPI.Util.GetHashKey(key));

                                            farm.CropFields[i].UpdateIrrigationEndTime(farm, i, data == null ? (long?)null : long.Parse(data), false);

                                            for (byte j = 0; j < farm.CropFields[i].CropsData.Count; j++)
                                            {
                                                for (byte k = 0; k < farm.CropFields[i].CropsData[j].Count; k++)
                                                {
/*                                                    farm.CropFields[i].CropsData[j][k].UpdateGrowTime(farm, i, j, k, null, false);

                                                    continue;*/

                                                    key = $"FARM::CF_{business.ID}_{i}_{j}_{k}";

                                                    data = farmsData.GetValueOrDefault(NAPI.Util.GetHashKey(key));

                                                    if (data is string dataStr)
                                                    {
                                                        var t = dataStr.Split('_');

                                                        if (t.Length > 1)
                                                            farm.CropFields[i].CropsData[j][k].WasIrrigated = byte.Parse(t[1]) == 1;

                                                        farm.CropFields[i].CropsData[j][k].UpdateGrowTime(farm, i, j, k, long.Parse(t[0]), false);
                                                    }
                                                    else
                                                    {
                                                        farm.CropFields[i].CropsData[j][k].UpdateGrowTime(farm, i, j, k, null, false);
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (farm.OrangeTrees != null)
                                    {
                                        for (int i = 0; i < farm.OrangeTrees.Count; i++)
                                        {
                                            var key = $"FARM::OT_{business.ID}_{i}";

                                            var data = farmsData.GetValueOrDefault(NAPI.Util.GetHashKey(key));

                                            var growTime = data == null ? (long?)null : long.Parse(data);

                                            farm.OrangeTrees[i].UpdateGrowTime(farm, i, growTime, false);
                                        }
                                    }

                                    if (farm.Cows != null)
                                    {
                                        for (int i = 0; i < farm.Cows.Count; i++)
                                        {
                                            var key = $"FARM::COW_{business.ID}_{i}";

                                            var data = farmsData.GetValueOrDefault(NAPI.Util.GetHashKey(key));

                                            var growTime = data == null ? (long?)null : long.Parse(data);

                                            farm.Cows[i].UpdateGrowTime(farm, i, growTime, false);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    cmd.CommandText = "SELECT * FROM fractions;";

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var type = (Game.Fractions.Types)Convert.ToInt32(reader["ID"]);

                                var data = Game.Fractions.Fraction.Get(type);

                                if (data == null)
                                    continue;

                                data.Balance = Convert.ToUInt64(reader["Balance"]);
                                data.ContainerId = Convert.ToUInt32(reader["Storage"]);

                                var leaderObj = reader["Leader"];

                                data.SetLeader(leaderObj is DBNull ? null : PlayerData.PlayerInfo.Get(Convert.ToUInt32(leaderObj)), true);

                                data.SetMaterials(Convert.ToUInt32(reader["Materials"]), false);
                                data.SetStorageLocked((bool)reader["STL"], false);
                                data.SetCreationWorkbenchLocked((bool)reader["CWL"], false);

                                data.AllVehicles = (((string)reader["Vehicles"]).DeserializeFromJson<Dictionary<uint, Game.Fractions.VehicleProps>>()).ToDictionary(x => VehicleData.VehicleInfo.Get(x.Key), x => x.Value);

                                data.Ranks = ((string)reader["Ranks"]).DeserializeFromJson<List<Game.Fractions.RankData>>();

                                data.News = ((string)reader["News"]).DeserializeFromJson<Game.Fractions.NewsData>();

                                if (data.News.All.Count > 0)
                                {
                                    foreach (var x in Enumerable.Range(0, data.News.All.Keys.Max()))
                                    {
                                        if (!data.News.All.ContainsKey(x))
                                        {
                                            if (data.News.PinnedId == x)
                                                data.News.PinnedId = -1;

                                            data.News.FreeIdxes.Enqueue(x);
                                        }
                                    }
                                }
                                else
                                {
                                    if (data.News.PinnedId >= 0)
                                        data.News.PinnedId = -1;
                                }
                            }
                        }
                    }

                    foreach (var x in PlayerData.PlayerInfo.All)
                    {
                        if (x.Value.Fraction == Game.Fractions.Types.None)
                            continue;

                        var data = Game.Fractions.Fraction.Get(x.Value.Fraction);

                        if (data == null)
                        {
                            x.Value.Fraction = Game.Fractions.Types.None;

                            continue;
                        }

                        data.AllMembers.Add(x.Value);
                    }

                    cmd.CommandText = "SELECT * FROM police_apbs;";

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var id = Convert.ToUInt32(reader["ID"]);

                                var data = ((string)reader["Data"]).DeserializeFromJson<Game.Fractions.Police.APBInfo>();

                                Game.Fractions.Police.AddAPBOnLoad(id, data);
                            }
                        }
                    }

                    cmd.CommandText = "SELECT * FROM gang_zones;";

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var id = Convert.ToUInt16(reader["ID"]);
                                var ownerType = (Game.Fractions.Types)Convert.ToUInt32(reader["Owner"]);
                                var date = (DateTime)reader["Date"];

                                var gangZoneInfo = Game.Fractions.Gang.GangZone.GetZoneById(id);

                                if (gangZoneInfo == null)
                                    continue;

                                gangZoneInfo.OwnerType = ownerType;
                            }
                        }
                    }
                }

                UpdateFreeUIDs(allItems, usedItems);

                loadedItemsAmount = usedItems.Count;
            }
        }

        private static void UpdateFreeUIDs(Dictionary<uint, Game.Items.Item> notUsedItems, HashSet<uint> usedItems)
        {
            var curTime = Utils.GetCurrentTime();

            if (notUsedItems.Count > 0)
            {
                var updCmd = new MySqlCommand();

                updCmd.CommandText = $"DELETE FROM Items WHERE ID IN ({CreateParametersSubstringIN("@ID", notUsedItems.Count)});";

                int counter = 0;

                foreach (var x in notUsedItems)
                {
                    updCmd.Parameters.AddWithValue($"@ID{counter++}", x.Key);
                }

                PushQuery(updCmd);
            }

            for (uint i = 1; i <= Game.Items.Item.UidHandler.LastAddedMaxUid; i++)
            {
                if (!usedItems.Contains(i))
                    Game.Items.Item.UidHandler.SetUidAsFree(i);
            }

            for (uint i = Properties.Settings.Profile.Current.Game.CIDBaseOffset; i <= PlayerData.PlayerInfo.UidHandler.LastAddedMaxUid; i++)
            {
                if (PlayerData.PlayerInfo.Get(i) == null)
                    PlayerData.PlayerInfo.UidHandler.SetUidAsFree(i);
            }

            for (uint i = Properties.Settings.Profile.Current.Game.VIDBaseOffset; i <= VehicleData.VehicleInfo.UidHandler.LastAddedMaxUid; i++)
            {
                if (VehicleData.VehicleInfo.Get(i) == null)
                    VehicleData.VehicleInfo.UidHandler.SetUidAsFree(i);
            }

            for (uint i = 1; i <= Game.Items.Container.UidHandler.LastAddedMaxUid; i++)
            {
                if (Game.Items.Container.Get(i) == null)
                    Game.Items.Container.UidHandler.SetUidAsFree(i);
            }

            var allGifts = PlayerData.PlayerInfo.All.SelectMany(x => x.Value.Gifts.Select(x => x.ID)).ToHashSet();

            for (uint i = 1; i <= Game.Items.Gift.UidHandler.LastAddedMaxUid; i++)
            {
                if (!allGifts.Contains(i))
                    Game.Items.Gift.UidHandler.SetUidAsFree(i);
            }

            for (uint i = 1; i <= Game.Estates.Furniture.UidHandler.LastAddedMaxUid; i++)
            {
                if (Game.Estates.Furniture.Get(i) == null)
                    Game.Estates.Furniture.UidHandler.SetUidAsFree(i);
            }

            var apbsToDel = new HashSet<uint>();

            for (uint i = 1; i <= Game.Fractions.Police.APBUidHandler.LastAddedMaxUid; i++)
            {
                var apbInfo = Game.Fractions.Police.GetAPB(i);

                if (apbInfo == null)
                {
                    Game.Fractions.Police.APBUidHandler.SetUidAsFree(i);
                }
                else if (curTime.Subtract(apbInfo.Time).TotalHours >= 24)
                {
                    Game.Fractions.Police.RemoveAPBOnLoad(i);

                    apbsToDel.Add(i);

                    Game.Fractions.Police.APBUidHandler.SetUidAsFree(i);
                }
            }

            if (apbsToDel.Count > 0)
            {
                var apbUpdCmd = new MySqlCommand();

                apbUpdCmd.CommandText = $"DELETE FROM Items WHERE ID IN ({CreateParametersSubstringIN("@ID", apbsToDel.Count)});";

                int counter = 0;

                foreach (var x in apbsToDel)
                {
                    apbUpdCmd.Parameters.AddWithValue($"@ID{counter++}", x);
                }

                PushQuery(apbUpdCmd);
            }

            //Console.WriteLine(Game.Items.Item.UidHandler.HandlerStr);
            //Console.WriteLine(Game.Estates.Furniture.UidHandler.HandlerStr);
            //Console.WriteLine(Game.Items.Gift.UidHandler.HandlerStr);
            //Console.WriteLine(VehicleData.VehicleInfo.UidHandler.HandlerStr);
            //Console.WriteLine(PlayerData.PlayerInfo.UidHandler.HandlerStr);
        }

        public static void UpdateServerData(string key, object value)
        {
            var cmd = new MySqlCommand();

            cmd.CommandText = "UPDATE server_data SET Data=@Data WHERE ID=@ID;";

            cmd.Parameters.AddWithValue("@ID", key);

            if (value == null)
                cmd.Parameters.AddWithValue("@Data", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@Data", value.SerializeToJson());

            PushQuery(cmd);
        }

        public static string CreateParametersSubstringIN(string key, int amount)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < amount - 1; i++)
            {
                sb.Append(key);
                sb.Append(i);
                sb.Append(',');
            }

            sb.Append(amount - 1);

            return sb.ToString();
        }

        #endregion
    }
}
