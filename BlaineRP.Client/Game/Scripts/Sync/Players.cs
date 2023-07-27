using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.Businesses;
using BlaineRP.Client.Game.Data.Customization;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Components;
using BlaineRP.Client.Game.EntitiesData.Enums;
using BlaineRP.Client.Game.Estates;
using BlaineRP.Client.Game.Fractions;
using BlaineRP.Client.Game.Helpers;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Input.Enums;
using BlaineRP.Client.Game.Items;
using BlaineRP.Client.Game.Management.Animations;
using BlaineRP.Client.Game.Quests;
using BlaineRP.Client.Game.Scripts.Misc;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Game.UI.CEF.Phone;
using BlaineRP.Client.Game.UI.CEF.Phone.Apps;
using BlaineRP.Client.Language;
using BlaineRP.Client.Settings.App;
using BlaineRP.Client.Settings.User;
using BlaineRP.Client.Utils;
using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using RAGE.Game;
using RAGE.Ui;
using RAGE.Util;

namespace BlaineRP.Client.Game.Scripts.Sync
{
    [Script]
    public class Players
    {
        /// <summary>Интервал, в котором будет отниматься здоровье, если игрок ранен</summary>
        private const int WoundedTime = 30000;

        /// <summary>Кол-во здоровья, которое будет отниматься каждые WoundedTime мсек., если игрок ранен</summary>
        private const int WoundedReduceHP = 5;

        /// <summary>Интервал, в котором будет отниматься здоровье, если игрок голоден</summary>
        private const int HungryTime = 120000;

        /// <summary>Кол-во здоровья, которое будет отниматься каждые WoundedTime мсек., если игрок голоден</summary>
        private const int HungryReduceHP = 5;

        /// <summary>Кол-во здоровья, ниже которого оно не будет отниматься, если игрок голоден</summary>
        private const int HungryLowestHP = 10;

        private static readonly Dictionary<string, Action<PlayerData, object, object>> dataActions = new Dictionary<string, Action<PlayerData, object, object>>();

        public Players()
        {
            CharacterLoaded = false;

            new AsyncTask(() =>
                {
                    List<RAGE.Elements.Player> players = Entities.Players.Streamed;

                    for (var i = 0; i < players.Count; i++)
                    {
                        var pData = PlayerData.GetData(players[i]);

                        if (pData == null)
                            continue;

                        if (pData.ActualAnimation is Animation anim)
                            if (!pData.Player.IsPlayingAnim(anim.Dict, anim.Name, 3))
                                Management.Animations.Core.Play(pData.Player, anim);

                        if (players[i].GetData<Action>("AttachMethod") is Action act)
                            act.Invoke();
                    }
                },
                2_500,
                true
            ).Run();

            Events.Add("Players::CloseAuth",
                async args =>
                {
                    Auth.CloseAll();
                }
            );

            Events.Add("Players::CharacterPreload",
                async args =>
                {
                    if (CharacterLoaded)
                        return;

                    while (!World.Core.Preloaded)
                    {
                        await Invoker.WaitAsync();
                    }

                    RAGE.Elements.Player.LocalPlayer.AutoVolume = false;
                    RAGE.Elements.Player.LocalPlayer.VoiceVolume = 0f;

                    await Browser.Render(Browser.IntTypes.Inventory_Full, true);

                    await Browser.Render(Browser.IntTypes.Chat, true);

                    await Browser.Render(Browser.IntTypes.Interaction, true);

                    await Browser.Render(Browser.IntTypes.NPC, true);

                    await Browser.Render(Browser.IntTypes.Phone, true);

                    Invoker.Invoke(0x95C0A5BBDC189AA1);

                    Browser.Window.ExecuteJs("Hud.createSpeedometer", 500);

                    RAGE.Elements.Player player = RAGE.Elements.Player.LocalPlayer;

                    var data = new PlayerData(RAGE.Elements.Player.LocalPlayer);

                    data.FastAnim = FastType.None;

                    var sData = (JObject)args[0];

                    data.Familiars = Json.Deserialize<HashSet<uint>>((string)sData["Familiars"]);

                    data.Licenses = Json.Deserialize<HashSet<LicenseTypes>>((string)sData["Licenses"]);

                    data.Skills = Json.Deserialize<Dictionary<SkillTypes, int>>((string)sData["Skills"]);

                    data.PhoneNumber = (uint)sData["PN"];

                    if (sData.TryGetValue("P", out JToken value))
                    {
                        foreach (string x in ((JArray)value).ToObject<List<string>>())
                        {
                            string[] t = x.Split('&');

                            Punishment.AddPunishment(new Punishment
                                {
                                    Id = uint.Parse(t[0]),
                                    Type = (Punishment.Types)int.Parse(t[1]),
                                    EndDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(t[2])).DateTime,
                                    AdditionalData = t[3].Length > 0 ? t[3] : null,
                                }
                            );
                        }

                        Punishment.StartCheckTask();
                    }

                    data.Contacts = sData.TryGetValue("Conts", out JToken value1) ? ((JObject)value1).ToObject<Dictionary<uint, string>>() : new Dictionary<uint, string>();

                    data.PhoneBlacklist = sData.TryGetValue("PBL", out JToken value2) ? ((JArray)value2).ToObject<List<uint>>() : new List<uint>();

                    data.AllSMS = sData.TryGetValue("SMS", out JToken value3)
                        ? Json.Deserialize<List<string>>((string)value3).Select(x => new SMS.Message(x)).ToList()
                        : new List<SMS.Message>();

                    data.OwnedVehicles = sData.TryGetValue("Vehicles", out JToken value4)
                        ? Json.Deserialize<List<string>>((string)value4)
                              .Select(x =>
                                   {
                                       string[] data = x.Split('_');
                                       return (Utils.Convert.ToUInt32(data[0]), Data.Vehicles.Core.GetById(data[1]));
                                   }
                               )
                              .ToList()
                        : new List<(uint VID, Data.Vehicles.Vehicle Data)>();

                    data.OwnedBusinesses = sData.TryGetValue("Businesses", out JToken value5)
                        ? Json.Deserialize<List<int>>((string)value5).Select(x => Business.All[x]).ToList()
                        : new List<Business>();

                    data.OwnedHouses = sData.TryGetValue("Houses", out JToken value6)
                        ? Json.Deserialize<List<uint>>((string)value6).Select(x => House.All[x]).ToList()
                        : new List<House>();

                    data.OwnedApartments = sData.TryGetValue("Apartments", out JToken value7)
                        ? Json.Deserialize<List<uint>>((string)value7).Select(x => Apartments.All[x]).ToList()
                        : new List<Apartments>();

                    data.OwnedGarages = sData.TryGetValue("Garages", out JToken value8)
                        ? Json.Deserialize<List<uint>>((string)value8).Select(x => Garage.All[x]).ToList()
                        : new List<Garage>();

                    if (sData.TryGetValue("MedCard", out JToken value9))
                        data.MedicalCard = Json.Deserialize<MedicalCard>((string)value9);

                    if (sData.TryGetValue("SHB", out JToken value10))
                    {
                        string[] shbData = ((string)value10).Split('_');

                        data.SettledHouseBase = (Estates.HouseBase.Types)int.Parse(shbData[0]) == Estates.HouseBase.Types.House
                            ? House.All[uint.Parse(shbData[1])]
                            : (HouseBase)Apartments.All[uint.Parse(shbData[1])];
                    }

                    var achievements = Json.Deserialize<List<string>>((string)sData["Achievements"])
                                           .ToDictionary(x => (AchievementTypes)Utils.Convert.ToInt32(x.Split('_')[0]),
                                                y =>
                                                {
                                                    string[] data = y.Split('_');

                                                    return (Utils.Convert.ToInt32(data[1]), Utils.Convert.ToInt32(data[2]));
                                                }
                                            );

                    foreach (KeyValuePair<AchievementTypes, (int, int)> x in achievements)
                    {
                        UpdateAchievement(data, x.Key, x.Value.Item1, x.Value.Item2);
                    }

                    data.Achievements = achievements.ToDictionary(x => x.Key, y => (y.Value.Item1, y.Value.Item1 >= y.Value.Item2));

                    data.Quests = sData.TryGetValue("Quests", out JToken value11)
                        ? Json.Deserialize<List<string>>((string)value11)
                              .Select(y =>
                                   {
                                       string[] data = y.Split('~');
                                       return new Quest((QuestTypes)int.Parse(data[0]), byte.Parse(data[1]), int.Parse(data[2]), data[3].Length > 0 ? data[3] : null);
                                   }
                               )
                              .ToList()
                        : new List<Quest>();

                    data.Furniture = sData.TryGetValue("Furniture", out JToken value12)
                        ? Json.Deserialize<Dictionary<uint, string>>((string)value12).ToDictionary(x => x.Key, x => Furniture.GetData(x.Value))
                        : new Dictionary<uint, Furniture>();

                    data.WeaponSkins = sData.TryGetValue("WSkins", out JToken value13)
                        ? Json.Deserialize<List<string>>((string)value13).ToDictionary(x => ((WeaponSkin.ItemData)Items.Core.GetData(x, typeof(WeaponSkin))).Type, x => x)
                        : new Dictionary<WeaponSkin.ItemData.Types, string>();

                    if (sData.TryGetValue("RV", out JToken value14))
                    {
                        var vehs = ((JArray)value14).ToObject<List<string>>().Select(x => x.Split('&')).ToList();

                        foreach (string[] x in vehs)
                        {
                            Vehicles.RentedVehicle.All.Add(new Vehicles.RentedVehicle(ushort.Parse(x[0]), Data.Vehicles.Core.GetById(x[1])));
                        }

                        RentedVehiclesCheckTask = new AsyncTask(Vehicles.RentedVehicle.Check, 1000, true);

                        RentedVehiclesCheckTask.Run();
                    }

                    foreach (KeyValuePair<SkillTypes, int> x in data.Skills)
                    {
                        UpdateSkill(x.Key, x.Value);
                    }

                    UpdateDrivingSkill(data.Licenses.Contains(LicenseTypes.B));
                    UpdateBikeSkill(data.Licenses.Contains(LicenseTypes.A));
                    UpdateFlyingSkill(data.Licenses.Contains(LicenseTypes.Fly));

                    Inventory.Load((JArray)sData["Inventory"]);

                    Menu.Load(data,
                        (TimeSpan)sData["TimePlayed"],
                        (DateTime)sData["CreationDate"],
                        (DateTime)sData["BirthDate"],
                        Json.Deserialize<Dictionary<uint, (int, string, int, int)>>((string)sData["Gifts"])
                    );

                    Menu.SetOrganisation((string)sData["Org"]);

                    Menu.SetFraction(FractionTypes.None);

                    foreach (KeyValuePair<SkillTypes, int> x in data.Skills)
                    {
                        Menu.UpdateSkill(x.Key, x.Value);
                    }

                    while (data.CID == 0)
                    {
                        await Invoker.WaitAsync();
                    }

                    PlayerData.SetData(RAGE.Elements.Player.LocalPlayer, data);

                    Menu.SetCID(data.CID);

                    InvokeHandler("AdminLevel", data, data.AdminLevel);

                    InvokeHandler("Cash", data, data.Cash);
                    InvokeHandler("BankBalance", data, data.BankBalance);

                    InvokeHandler("Sex", data, data.Sex);

                    InvokeHandler("Fraction", data, data.Fraction);

                    InvokeHandler("Mood", data, data.Mood);
                    InvokeHandler("Satiety", data, data.Satiety);

                    InvokeHandler("Knocked", data, data.IsKnocked);

                    InvokeHandler("Wounded", data, data.IsWounded);

                    InvokeHandler("VoiceRange", data, data.VoiceRange);

                    InvokeHandler("Anim::General", data, (int)data.GeneralAnim);

                    InvokeHandler("CHO", data, player.GetSharedData("CHO"));

                    InvokeHandler("DCR", data, RAGE.Elements.Player.LocalPlayer.GetSharedData<JArray>("DCR"));

                    new AsyncTask(() =>
                        {
                            //Events.CallRemote("Player::UpdateTime");

                            var minuteTimeSpan = TimeSpan.FromMinutes(1);

                            Menu.TimePlayed = Menu.TimePlayed.Add(minuteTimeSpan);
                        },
                        60_000,
                        true,
                        60_000
                    ).Run();

                    HUD.Menu.UpdateCurrentTypes(true, HUD.Menu.Types.Menu, HUD.Menu.Types.Documents, HUD.Menu.Types.BlipsMenu);

                    if (data.WeaponSkins.Count > 0)
                        HUD.Menu.UpdateCurrentTypes(true, HUD.Menu.Types.WeaponSkinsMenu);

                    Settings.User.Initialization.Load();
                    Input.Core.LoadAll();

                    UI.CEF.Phone.Phone.Preload();

                    await UI.CEF.Animations.Load();

                    await Events.CallRemoteProc("Players::CRI", data.IsInvalid, Other.CurrentEmotion, Other.CurrentWalkstyle);

                    CharacterLoaded = true;

                    Menu.UpdateSettingsData();
                    Menu.UpdateKeyBindsData();

                    RAGE.Elements.Player.LocalPlayer.FreezePosition(false);

                    RAGE.Elements.Player.LocalPlayer.SetInvincible(false);

                    Main.DisableAllControls(false);

                    var timeUpdateTask = new AsyncTask(() =>
                        {
                            HUD.UpdateTime();
                            UI.CEF.Phone.Phone.UpdateTime();
                        },
                        1_000,
                        true
                    );

                    timeUpdateTask.Run();

                    HUD.ShowHUD(!Interface.HideHUD);

                    Management.Interaction.Enabled = true;
                    World.Core.EnabledItemsOnGround = true;

                    UI.CEF.Chat.Show(true);

                    ExtraLabel.Initialize();

                    ExtraColshape.Activate();

                    Management.Misc.Discord.SetDefault();

                    await Invoker.WaitAsync(500);

                    foreach (Business x in data.OwnedBusinesses)
                    {
                        x.ToggleOwnerBlip(true);
                    }

                    foreach (House x in data.OwnedHouses)
                    {
                        x.ToggleOwnerBlip(true);
                    }

                    foreach (Apartments x in data.OwnedApartments)
                    {
                        x.ToggleOwnerBlip(true);
                    }

                    foreach (Garage x in data.OwnedGarages)
                    {
                        x.ToggleOwnerBlip(true);
                    }

                    data.SettledHouseBase?.ToggleOwnerBlip(true);

                    foreach (Quest x in data.Quests)
                    {
                        x.Initialize();
                    }

                    Management.Attachments.Core.ReattachObjects(RAGE.Elements.Player.LocalPlayer);
                }
            );

            Events.Add("Player::Knocked",
                args =>
                {
                    var state = (bool)args[0];

                    if (state)
                    {
                        Main.DisableMove(true);

                        RAGE.Elements.Player attacker = Entities.Players.GetAtRemote(Utils.Convert.ToUInt16(args[1])) ?? RAGE.Elements.Player.LocalPlayer;

                        Graphics.StartScreenEffect("DeathFailMPIn", 0, true);

                        var scaleformWaster = Helpers.Scaleform.CreateShard("wasted",
                            Locale.Get("SCALEFORM_WASTED_HEADER"),
                            attacker.Handle == RAGE.Elements.Player.LocalPlayer.Handle
                                ? Locale.Get("SCALEFORM_WASTED_ATTACKER_S")
                                : Locale.Get("SCALEFORM_WASTED_ATTACKER_P", attacker.GetName(true, false, true), PlayerData.GetData(attacker)?.CID ?? 0)
                        );

                        Death.Show();
                    }
                    else
                    {
                        Death.Close();

                        Main.DisableMove(false);

                        Graphics.StopScreenEffect("DeathFailMPIn");

                        Helpers.Scaleform.Get("wasted")?.Destroy();
                    }
                }
            );

            Events.Add("opday",
                args =>
                {
                    if (args == null || args.Length == 0)
                    {
                        Events.CallLocal("Chat::ShowServerMessage", "Время зарплаты | Вы ничего не получаете, так как у Вас нет банковского счёта!");
                    }
                    else if (args.Length == 1)
                    {
                        var playedTime = TimeSpan.FromSeconds(Utils.Convert.ToInt64(args[0]));
                        var minTimeToGetPayday = TimeSpan.FromSeconds(Utils.Convert.ToInt64(args[1]));

                        Events.CallLocal("Chat::ShowServerMessage",
                            $"Время зарплаты | Вы ничего не получаете, так как за этот час Вы наиграли {playedTime.GetBeautyString()} (необходимо - {minTimeToGetPayday.GetBeautyString()})!"
                        );
                    }
                    else
                    {
                        var joblessBenefit = Utils.Convert.ToDecimal(args[0]);
                        var fractionSalary = Utils.Convert.ToDecimal(args[1]);
                        var organisationSalary = Utils.Convert.ToDecimal(args[2]);

                        if (joblessBenefit > 0)
                        {
                            Events.CallLocal("Chat::ShowServerMessage",
                                $"Время зарплаты | Вы получаете {Locale.Get("GEN_MONEY_0", joblessBenefit)} (пособие по безработице) на свой счёт!"
                            );
                        }
                        else
                        {
                            if (organisationSalary == 0)
                                Events.CallLocal("Chat::ShowServerMessage",
                                    $"Время зарплаты | Вы получаете {Locale.Get("GEN_MONEY_0", fractionSalary)} (от фракции) на свой счёт!"
                                );
                            else if (fractionSalary != 0)
                                Events.CallLocal("Chat::ShowServerMessage",
                                    $"Время зарплаты | Вы получаете {Locale.Get("GEN_MONEY_0", fractionSalary)} (от фракции) и {Locale.Get("GEN_MONEY_0", organisationSalary)} (от организации) на свой счёт!"
                                );
                            else
                                Events.CallLocal("Chat::ShowServerMessage",
                                    $"Время зарплаты | Вы получаете {Locale.Get("GEN_MONEY_0", fractionSalary)} (от организации) на свой счёт!"
                                );
                        }
                    }
                }
            );

            Events.Add("Player::ParachuteS",
                args =>
                {
                    uint parachuteWeaponHash = Joaat.Hash("gadget_parachute");

                    if (!(bool)args[0])
                    {
                        RAGE.Elements.Player.LocalPlayer.RemoveWeaponFrom(parachuteWeaponHash);

                        if (!(bool)args[1])
                        {
                            RAGE.Elements.Player.LocalPlayer.GetData<AsyncTask>("ParachuteATask")?.Cancel();

                            RAGE.Elements.Player.LocalPlayer.ResetData("ParachuteATask");

                            if (RAGE.Elements.Player.LocalPlayer.GetParachuteState() >= 0)
                                RAGE.Elements.Player.LocalPlayer.ClearTasksImmediately();
                        }
                    }
                    else
                    {
                        RAGE.Elements.Player.LocalPlayer.GetData<AsyncTask>("ParachuteATask")?.Cancel();

                        RAGE.Elements.Player.LocalPlayer.RemoveWeaponFrom(parachuteWeaponHash);

                        RAGE.Elements.Player.LocalPlayer.GiveWeaponTo(parachuteWeaponHash, 0, false, false);

                        RAGE.Game.Player.SetPlayerParachuteVariationOverride(66, 0, 2, false);
                        RAGE.Game.Player.SetPlayerParachuteTintIndex(6);

                        AsyncTask task = null;

                        var isInFly = false;

                        int lastSentState = int.MinValue;

                        task = new AsyncTask(() =>
                            {
                                int pState = RAGE.Elements.Player.LocalPlayer.GetParachuteState();

                                if (isInFly)
                                {
                                    if (pState < 0 || pState == 3)
                                    {
                                        if (lastSentState != -1)
                                        {
                                            Events.CallRemote("Player::ParachuteS", false);

                                            if (lastSentState == 1 || lastSentState == 2)
                                                task?.Cancel();

                                            lastSentState = -1;

                                            isInFly = false;
                                        }
                                    }
                                    else if (pState == 1 || pState == 2)
                                    {
                                        if (lastSentState != 1)
                                        {
                                            Events.CallRemote("Player::ParachuteS", true);

                                            Notification.ShowHint("Используйте F, чтобы открепиться от парашюта", true);

                                            lastSentState = 1;
                                        }
                                    }
                                }
                                else
                                {
                                    if (pState < 0 || pState == 3)
                                        return;

                                    isInFly = true;

                                    if (lastSentState != 0)
                                    {
                                        //Events.CallRemote("Player::ParachuteS", 0);

                                        lastSentState = 0;

                                        Notification.ShowHint("Используйте ЛКМ или F, чтобы раскрыть парашют", true);
                                    }
                                }
                            },
                            25,
                            true
                        );

                        RAGE.Elements.Player.LocalPlayer.SetData("ParachuteATask", task);

                        task.Run();
                    }
                }
            );

            Events.Add("Player::RVehs::U",
                args =>
                {
                    var rId = (ushort)(int)args[0];

                    List<Vehicles.RentedVehicle> rentedVehs = Vehicles.RentedVehicle.All;

                    if (args.Length > 1)
                    {
                        var vTypeId = (string)args[1];

                        Data.Vehicles.Vehicle vTypeData = Data.Vehicles.Core.GetById(vTypeId);

                        rentedVehs.Add(new Vehicles.RentedVehicle(rId, vTypeData));

                        if (RentedVehiclesCheckTask == null)
                        {
                            RentedVehiclesCheckTask = new AsyncTask(Vehicles.RentedVehicle.Check, 1000, true);

                            RentedVehiclesCheckTask.Run();
                        }
                    }
                    else
                    {
                        Vehicles.RentedVehicle rVeh = rentedVehs.Where(x => x.RemoteId == rId).FirstOrDefault();

                        if (rVeh == null)
                            return;

                        rentedVehs.Remove(rVeh);

                        if (rentedVehs.Count == 0)
                        {
                            RentedVehiclesCheckTask?.Cancel();

                            RentedVehiclesCheckTask = null;
                        }
                    }

                    if (UI.CEF.Phone.Phone.CurrentApp == AppType.Vehicles)
                        UI.CEF.Phone.Phone.ShowApp(null, AppType.Vehicles);
                }
            );

            Events.Add("Player::Waypoint::Set",
                args =>
                {
                    var x = (float)args[0];
                    var y = (float)args[1];

                    Utils.Game.Misc.SetWaypoint(x, y);
                }
            );

            Events.Add("Player::Smoke::Start",
                args =>
                {
                    var maxTime = (int)args[0];
                    var maxPuffs = (int)args[1];

                    RAGE.Elements.Player.LocalPlayer.SetData("Smoke::Data::Puffs", maxPuffs);
                    RAGE.Elements.Player.LocalPlayer.SetData("Smoke::Data::CTask", new AsyncTask(() => Events.CallRemote("Players::Smoke::Stop"), maxTime));
                }
            );

            Events.Add("Player::Smoke::Stop",
                args =>
                {
                    RAGE.Elements.Player.LocalPlayer.ResetData("Smoke::Data::Puffs");

                    RAGE.Elements.Player.LocalPlayer.GetData<AsyncTask>("Smoke::Data::CTask")?.Cancel();
                    RAGE.Elements.Player.LocalPlayer.GetData<AsyncTask>("Smoke::Data::CTask1")?.Cancel();
                    RAGE.Elements.Player.LocalPlayer.GetData<AsyncTask>("Smoke::Data::CTask2")?.Cancel();

                    RAGE.Elements.Player.LocalPlayer.ResetData("Smoke::Data::CTask");
                    RAGE.Elements.Player.LocalPlayer.ResetData("Smoke::Data::CTask1");
                    RAGE.Elements.Player.LocalPlayer.ResetData("Smoke::Data::CTask2");
                }
            );

            Events.Add("Player::Smoke::Puff",
                args =>
                {
                    var task1 = new AsyncTask(async () =>
                        {
                            await Utils.Game.Streaming.RequestPtfx("core");

                            int fxHandle = Graphics.StartParticleFxLoopedOnEntityBone("exp_grd_bzgas_smoke",
                                RAGE.Elements.Player.LocalPlayer.Handle,
                                0f,
                                0f,
                                0f,
                                0f,
                                0f,
                                0f,
                                RAGE.Elements.Player.LocalPlayer.GetBoneIndex(20279),
                                0.15f,
                                false,
                                false,
                                false
                            );

                            await Invoker.WaitAsync(1000);

                            Graphics.StopParticleFxLooped(fxHandle, false);
                        },
                        2000
                    );

                    var task2 = new AsyncTask(() =>
                        {
                            if (!RAGE.Elements.Player.LocalPlayer.HasData("Smoke::Data::Puffs"))
                                return;

                            RAGE.Elements.Player.LocalPlayer.SetData("Smoke::Data::Puffs", RAGE.Elements.Player.LocalPlayer.GetData<int>("Smoke::Data::Puffs") - 1);
                        },
                        3000
                    );

                    task1.Run();
                    task2.Run();

                    RAGE.Elements.Player.LocalPlayer.SetData("Smoke::Data::CTask1", task1);
                    RAGE.Elements.Player.LocalPlayer.SetData("Smoke::Data::CTask2", task2);
                }
            );

            Events.Add("Player::CloseAll", args => CloseAll((bool)args[0]));

            Events.Add("Player::Quest::Upd",
                args =>
                {
                    var data = PlayerData.GetData(RAGE.Elements.Player.LocalPlayer);

                    if (data == null)
                        return;

                    var qType = (QuestTypes)(int)args[0];

                    List<Quest> quests = data.Quests;

                    Quest quest = quests.Where(x => x.Type == qType).FirstOrDefault();

                    if (args.Length < 3)
                    {
                        var success = (bool)args[1];

                        if (quest == null)
                            return;

                        quest.SetQuestAsCompleted(success, true);
                    }
                    else
                    {
                        var step = Utils.Convert.ToByte(args[1]);

                        var sProgress = (int)args[2];

                        if (quest == null)
                        {
                            quest = new Quest(qType, step, sProgress, args.Length > 3 ? (string)args[3] : null);

                            quest.SetQuestAsStarted(true);
                        }
                        else
                        {
                            if (args.Length > 3)
                                quest.SetQuestAsUpdated(step, sProgress, (string)args[3], true);
                            else
                                quest.SetQuestAsUpdatedKeepOldData(step, sProgress, true);
                        }
                    }
                }
            );

            Events.Add("Player::Achievements::Update",
                args =>
                {
                    var data = PlayerData.GetData(RAGE.Elements.Player.LocalPlayer);

                    if (data == null)
                        return;

                    var aType = (AchievementTypes)(int)args[0];
                    var value = (int)args[1];
                    var maxValue = (int)args[2];

                    UpdateAchievement(data, aType, value, maxValue);

                    string achievementName = Locale.Get(Strings.GetKeyFromTypeByMemberName(aType.GetType(), aType.ToString(), "NAME_0") ?? "null");

                    if (value >= maxValue)
                        Notification.Show(Notification.Types.Achievement, achievementName, Locale.Notifications.General.AchievementUnlockedText, 5000);
                }
            );

            Events.Add("Player::Skills::Update",
                args =>
                {
                    var data = PlayerData.GetData(RAGE.Elements.Player.LocalPlayer);

                    if (data == null)
                        return;

                    var sType = (SkillTypes)(int)args[0];
                    var value = (int)args[1];

                    int oldValue = data.Skills.GetValueOrDefault(sType, 0);

                    data.Skills[sType] = value;

                    Menu.UpdateSkill(sType, value);

                    UpdateSkill(sType, value);

                    Notification.Show(Notification.Types.Information,
                        Locale.Get("NOTIFICATION_HEADER_DEF"),
                        string.Format(value >= oldValue ? Locale.Notifications.General.SkillUp : Locale.Notifications.General.SkillDown,
                            Locale.Get(Strings.GetKeyFromTypeByMemberName(sType.GetType(), sType.ToString(), "NAME_1") ?? "null").ToLower(),
                            System.Math.Abs(value - oldValue),
                            value,
                            Static.PlayerMaxSkills[sType]
                        )
                    );
                }
            );

            Events.Add("Player::WSkins::Update",
                args =>
                {
                    var data = PlayerData.GetData(RAGE.Elements.Player.LocalPlayer);

                    if (data == null)
                        return;

                    var add = (bool)args[0];

                    var id = (string)args[1];

                    Dictionary<WeaponSkin.ItemData.Types, string> wSkins = data.WeaponSkins;

                    WeaponSkin.ItemData.Types type = ((WeaponSkin.ItemData)Items.Core.GetData(id)).Type;

                    if (add)
                    {
                        wSkins[type] = id;

                        HUD.Menu.UpdateCurrentTypes(true, HUD.Menu.Types.WeaponSkinsMenu);
                    }
                    else
                    {
                        wSkins.Remove(type);

                        if (wSkins.Count == 0)
                            HUD.Menu.UpdateCurrentTypes(false, HUD.Menu.Types.WeaponSkinsMenu);
                    }

                    data.WeaponSkins = wSkins;
                }
            );

            Events.Add("Player::MedCard::Update",
                args =>
                {
                    var data = PlayerData.GetData(RAGE.Elements.Player.LocalPlayer);

                    if (data == null)
                        return;

                    if (args.Length > 0)
                        data.MedicalCard = Json.Deserialize<MedicalCard>((string)args[0]);
                    else
                        data.MedicalCard = null;
                }
            );

            Events.Add("Player::Licenses::Update",
                args =>
                {
                    var data = PlayerData.GetData(RAGE.Elements.Player.LocalPlayer);

                    if (data == null)
                        return;

                    var state = (bool)args[0];

                    var lType = (LicenseTypes)(int)args[1];

                    if (state)
                        data.Licenses.Add(lType);
                    else
                        data.Licenses.Remove(lType);

                    if (lType == LicenseTypes.B)
                        UpdateDrivingSkill(state);
                    else if (lType == LicenseTypes.A)
                        UpdateBikeSkill(state);
                    else if (lType == LicenseTypes.Fly)
                        UpdateFlyingSkill(state);
                }
            );

            Events.Add("Player::Familiars::Update",
                args =>
                {
                    var data = PlayerData.GetData(RAGE.Elements.Player.LocalPlayer);

                    if (data == null)
                        return;

                    var add = (bool)args[0];
                    var cid = Utils.Convert.ToUInt32(args[1]);

                    if (add)
                        data.Familiars.Add(cid);
                    else
                        data.Familiars.Remove(cid);
                }
            );

            Events.Add("Player::SettledHB",
                args =>
                {
                    var pType = (Estates.HouseBase.Types)(int)args[0];

                    var pId = (uint)(int)args[1];

                    var state = (bool)args[2];

                    HouseBase house = pType == Estates.HouseBase.Types.House ? House.All[pId] : (HouseBase)Apartments.All[pId];

                    house.ToggleOwnerBlip(state);

                    if (args.Length > 3)
                    {
                        RAGE.Elements.Player playerInit = Entities.Players.GetAtRemote((ushort)(int)args[3]);

                        if (state)
                        {
                            Notification.Show(Notification.Types.Information,
                                Locale.Get("NOTIFICATION_HEADER_DEF"),
                                string.Format(pType == Estates.HouseBase.Types.House ? Locale.Notifications.House.SettledHouse : Locale.Notifications.House.SettledApartments,
                                    playerInit.GetName(true, false, true)
                                )
                            );
                        }
                        else
                        {
                            if (playerInit?.Handle == RAGE.Elements.Player.LocalPlayer.Handle)
                                Notification.Show(Notification.Types.Information,
                                    Locale.Get("NOTIFICATION_HEADER_DEF"),
                                    pType == Estates.HouseBase.Types.House ? Locale.Notifications.House.ExpelledHouseSelf : Locale.Notifications.House.ExpelledApartmentsSelf
                                );
                            else
                                Notification.Show(Notification.Types.Information,
                                    Locale.Get("NOTIFICATION_HEADER_DEF"),
                                    string.Format(pType == Estates.HouseBase.Types.House ? Locale.Notifications.House.ExpelledHouse : Locale.Notifications.House.ExpelledApartments,
                                        playerInit.GetName(true, false, true)
                                    )
                                );
                        }
                    }
                    else
                    {
                        if (state)
                            Notification.Show(Notification.Types.Information,
                                Locale.Get("NOTIFICATION_HEADER_DEF"),
                                pType == Estates.HouseBase.Types.House ? Locale.Notifications.House.SettledHouseAuto : Locale.Notifications.House.SettledApartmentsAuto
                            );
                        else
                            Notification.Show(Notification.Types.Information,
                                Locale.Get("NOTIFICATION_HEADER_DEF"),
                                pType == Estates.HouseBase.Types.House ? Locale.Notifications.House.ExpelledHouseAuto : Locale.Notifications.House.ExpelledApartmentsAuto
                            );
                    }
                }
            );

            Events.Add("Player::Properties::Update",
                args =>
                {
                    var data = PlayerData.GetData(RAGE.Elements.Player.LocalPlayer);

                    if (data == null)
                        return;

                    var add = (bool)args[0];

                    var pType = (PropertyTypes)(int)args[1];

                    if (pType == PropertyTypes.Vehicle)
                    {
                        var vid = Utils.Convert.ToUInt32(args[2]);

                        if (add)
                        {
                            (uint vid, Data.Vehicles.Vehicle) t = (vid, Data.Vehicles.Core.GetById((string)args[3]));

                            if (!data.OwnedVehicles.Contains(t))
                                data.OwnedVehicles.Add(t);
                        }
                        else
                        {
                            int idx = data.OwnedVehicles.FindIndex(x => x.VID == vid);

                            if (idx < 0)
                                return;

                            data.OwnedVehicles.RemoveAt(idx);
                        }

                        if (UI.CEF.Phone.Phone.CurrentApp == AppType.Vehicles)
                            UI.CEF.Phone.Phone.ShowApp(null, AppType.Vehicles);
                    }
                    else if (pType == PropertyTypes.House)
                    {
                        var hid = Utils.Convert.ToUInt32(args[2]);

                        House t = House.All[hid];

                        if (add)
                        {
                            if (!data.OwnedHouses.Contains(t))
                                data.OwnedHouses.Add(t);
                        }
                        else
                        {
                            data.OwnedHouses.Remove(t);
                        }

                        t.ToggleOwnerBlip(add);
                    }
                    else if (pType == PropertyTypes.Apartments)
                    {
                        var hid = Utils.Convert.ToUInt32(args[2]);

                        Apartments t = Apartments.All[hid];

                        if (add)
                        {
                            if (!data.OwnedApartments.Contains(t))
                                data.OwnedApartments.Add(t);
                        }
                        else
                        {
                            data.OwnedApartments.Remove(t);
                        }

                        t.ToggleOwnerBlip(add);
                    }
                    else if (pType == PropertyTypes.Garage)
                    {
                        var gid = Utils.Convert.ToUInt32(args[2]);

                        Garage t = Garage.All[gid];

                        if (add)
                        {
                            if (!data.OwnedGarages.Contains(t))
                                data.OwnedGarages.Add(t);
                        }
                        else
                        {
                            data.OwnedGarages.Remove(t);
                        }

                        t.ToggleOwnerBlip(add);
                    }
                    else if (pType == PropertyTypes.Business)
                    {
                        var bid = (int)args[2];

                        Business t = Business.All[bid];

                        if (add)
                        {
                            if (!data.OwnedBusinesses.Contains(t))
                                data.OwnedBusinesses.Add(t);
                        }
                        else
                        {
                            data.OwnedBusinesses.Remove(t);
                        }

                        t.ToggleOwnerBlip(add);
                    }

                    Menu.UpdateProperties(data);
                }
            );

            Events.Add("Player::Furniture::Update",
                args =>
                {
                    var pData = PlayerData.GetData(RAGE.Elements.Player.LocalPlayer);

                    if (pData == null)
                        return;

                    var add = (bool)args[0];

                    if (add)
                    {
                        List<string> furns = ((JArray)args[1]).ToObject<List<string>>();

                        foreach (string x in furns)
                        {
                            string[] d = x.Split('&');

                            var fUid = uint.Parse(d[0]);

                            string fId = d[1];

                            var fData = Furniture.GetData(fId);

                            if (pData.Furniture.TryAdd(fUid, fData))
                                if (HouseMenu.IsActive)
                                    HouseMenu.AddOwnedFurniture(fUid, fData);
                        }
                    }
                    else
                    {
                        List<string> furns = ((JArray)args[1]).ToObject<List<string>>();

                        foreach (string x in furns)
                        {
                            string[] d = x.Split('&');

                            var fUid = uint.Parse(d[0]);

                            if (pData.Furniture.Remove(fUid))
                                if (HouseMenu.IsActive)
                                    HouseMenu.RemoveOwnedFurniture(fUid);
                        }
                    }
                }
            );

            AddDataHandler("Fly",
                (pData, value, oldValue) =>
                {
                    if (pData.Player != RAGE.Elements.Player.LocalPlayer)
                        return;

                    bool state = (bool?)value ?? false;

                    Main.Render -= FlyRender;

                    if (state)
                    {
                        RAGE.Elements.Player.LocalPlayer.ClearTasksImmediately();

                        Main.Render += FlyRender;
                    }
                }
            );

            AddDataHandler("IsFrozen",
                (pData, value, oldValue) =>
                {
                    if (pData.Player.Handle != RAGE.Elements.Player.LocalPlayer.Handle)
                        return;

                    bool state = (bool?)value ?? false;

                    if (state)
                    {
                        RAGE.Elements.Player.LocalPlayer.ClearTasksImmediately();

                        CloseAll();

                        RAGE.Elements.Player.LocalPlayer.FreezePosition(true);

                        Main.DisableAllControls(true);

                        Management.Weapons.Core.DisabledFiring = true;
                    }
                    else
                    {
                        Management.Weapons.Core.DisabledFiring = false;

                        RAGE.Elements.Player.LocalPlayer.FreezePosition(false);

                        Main.DisableAllControls(false);
                    }
                }
            );

            AddDataHandler("Cash",
                (pData, value, oldValue) =>
                {
                    if (pData.Player.Handle != RAGE.Elements.Player.LocalPlayer.Handle)
                        return;

                    var cash = Utils.Convert.ToUInt64(value);

                    HUD.SetCash(cash);
                    Menu.SetCash(cash);

                    ulong oldCash = oldValue == null ? cash : Utils.Convert.ToUInt64(oldValue);

                    if (cash == oldCash)
                        return;

                    if (cash > oldCash)
                        Notification.Show(Notification.Types.Cash,
                            Strings.Get("GEN_MONEY_ADD_0", cash - oldCash),
                            Locale.Get("NTFC_MONEY_CASH_0", Locale.Get("GEN_MONEY_0", cash))
                        );
                    else
                        Notification.Show(Notification.Types.Cash,
                            Strings.Get("GEN_MONEY_REMOVE_0", oldCash - cash),
                            Locale.Get("NTFC_MONEY_CASH_0", Locale.Get("GEN_MONEY_0", cash))
                        );
                }
            );

            AddDataHandler("BankBalance",
                (pData, value, oldValue) =>
                {
                    if (pData.Player.Handle != RAGE.Elements.Player.LocalPlayer.Handle)
                        return;

                    var bank = Utils.Convert.ToUInt64(value);

                    HUD.SetBank(bank);
                    Menu.SetBank(bank);

                    ATM.UpdateMoney(bank);
                    UI.CEF.Bank.UpdateMoney(bank);
                    UI.CEF.Phone.Apps.Bank.UpdateBalance(bank);

                    ulong oldBank = oldValue == null ? bank : Utils.Convert.ToUInt64(oldValue);

                    if (bank == oldBank)
                        return;

                    if (bank > oldBank)
                        Notification.Show(Notification.Types.Bank,
                            Strings.Get("GEN_MONEY_ADD_0", bank - oldBank),
                            Locale.Get("NTFC_MONEY_BANK_0", Locale.Get("GEN_MONEY_0", bank))
                        );
                    else
                        Notification.Show(Notification.Types.Bank,
                            Strings.Get("GEN_MONEY_REMOVE_0", oldBank - bank),
                            Locale.Get("NTFC_MONEY_BANK_0", Locale.Get("GEN_MONEY_0", bank))
                        );
                }
            );

            AddDataHandler("IsWounded",
                (pData, value, oldValue) =>
                {
                    RAGE.Elements.Player player = pData.Player;

                    bool state = (bool?)value ?? false;

                    if (player.Handle == RAGE.Elements.Player.LocalPlayer.Handle)
                    {
                        if (state)
                        {
                            HUD.SwitchStatusIcon(HUD.StatusTypes.Wounded, true);

                            Notification.ShowHint(Locale.Notifications.Players.States.Wounded);

                            WoundedTask?.Cancel();

                            WoundedTask = new AsyncTask(() =>
                                {
                                    var pData = PlayerData.GetData(RAGE.Elements.Player.LocalPlayer);

                                    if (pData == null || pData.IsInvincible)
                                        return;

                                    RAGE.Elements.Player.LocalPlayer.SetRealHealth(RAGE.Elements.Player.LocalPlayer.GetRealHealth() - WoundedReduceHP);
                                },
                                WoundedTime,
                                true,
                                WoundedTime / 2
                            );

                            WoundedTask.Run();

                            Graphics.StartScreenEffect("DeathFailMPDark", 0, true);
                        }
                        else
                        {
                            Graphics.StopScreenEffect("DeathFailMPDark");

                            HUD.SwitchStatusIcon(HUD.StatusTypes.Wounded, false);

                            if (WoundedTask != null)
                            {
                                WoundedTask.Cancel();

                                WoundedTask = null;
                            }
                        }
                    }
                }
            );

            AddDataHandler("WCD",
                (pData, value, oldValue) =>
                {
                    RAGE.Elements.Player player = pData.Player;

                    if (value is string strData)
                        Management.Weapons.Core.UpdateWeaponComponents(player, strData);
                }
            );

            Events.Add("Players::WCD::U",
                args =>
                {
                    var player = (RAGE.Elements.Player)args[0];

                    if (player?.Exists != true || player.Handle == RAGE.Elements.Player.LocalPlayer.Handle)
                        return;

                    if (player.GetSharedData<string>("WCD") is string strData)
                        Management.Weapons.Core.UpdateWeaponComponents(player, strData);
                }
            );

            AddDataHandler("Mood",
                (pData, value, oldValue) =>
                {
                    if (pData.Player != RAGE.Elements.Player.LocalPlayer)
                        return;

                    var mood = Utils.Convert.ToByte(value);

                    if (mood <= 25)
                    {
                        HUD.SwitchStatusIcon(HUD.StatusTypes.Mood, true);

                        if (mood % 5 == 0)
                            Notification.ShowHint(Locale.Notifications.Players.States.LowMood, false, 5_000);
                    }
                    else
                    {
                        HUD.SwitchStatusIcon(HUD.StatusTypes.Mood, false);
                    }

                    Inventory.UpdateStates();
                }
            );

            AddDataHandler("Satiety",
                (pData, value, oldValue) =>
                {
                    if (pData.Player != RAGE.Elements.Player.LocalPlayer)
                        return;

                    var satiety = Utils.Convert.ToByte(value);

                    if (satiety <= 25)
                    {
                        HUD.SwitchStatusIcon(HUD.StatusTypes.Food, true);

                        if (satiety % 5 == 0)
                            Notification.ShowHint(Locale.Notifications.Players.States.LowSatiety, false, 5_000);

                        if (satiety == 0)
                        {
                            if (HungerTask != null)
                                HungerTask.Cancel();

                            HungerTask = new AsyncTask(() =>
                                {
                                    var pData = PlayerData.GetData(RAGE.Elements.Player.LocalPlayer);

                                    if (pData == null || pData.IsInvincible)
                                        return;

                                    int currentHp = RAGE.Elements.Player.LocalPlayer.GetRealHealth();

                                    if (currentHp <= HungryLowestHP)
                                        return;

                                    if (currentHp - HungryReduceHP <= HungryLowestHP)
                                        RAGE.Elements.Player.LocalPlayer.SetRealHealth(HungryLowestHP);
                                    else
                                        RAGE.Elements.Player.LocalPlayer.SetRealHealth(currentHp - HungryReduceHP);
                                },
                                HungryTime,
                                true,
                                HungryTime / 2
                            );
                        }
                    }
                    else
                    {
                        if (HungerTask != null)
                        {
                            HungerTask.Cancel();

                            HungerTask = null;
                        }

                        HUD.SwitchStatusIcon(HUD.StatusTypes.Food, false);
                    }

                    Inventory.UpdateStates();
                }
            );

            AddDataHandler("Emotion",
                (pData, value, oldValue) =>
                {
                    RAGE.Elements.Player player = pData.Player;

                    var emotion = (EmotionType)((int?)value ?? -1);

                    if (player.Handle == RAGE.Elements.Player.LocalPlayer.Handle)
                    {
                        Other.CurrentEmotion = emotion;

                        UI.CEF.Animations.ToggleAnim("e-" + emotion, true);
                    }

                    Management.Animations.Core.Set(player, emotion);
                }
            );

            AddDataHandler("Walkstyle",
                (pData, value, oldValue) =>
                {
                    RAGE.Elements.Player player = pData.Player;

                    var wStyle = (WalkstyleType)((int?)value ?? -1);

                    if (player.Handle == RAGE.Elements.Player.LocalPlayer.Handle)
                    {
                        Other.CurrentWalkstyle = wStyle;

                        UI.CEF.Animations.ToggleAnim("w-" + wStyle, true);
                    }

                    if (!pData.CrouchOn)
                        Management.Animations.Core.Set(player, wStyle);
                }
            );

            AddDataHandler("Anim::Other",
                (pData, value, oldValue) =>
                {
                    RAGE.Elements.Player player = pData.Player;

                    var anim = (OtherType)((int?)value ?? -1);

                    if (player.Handle == RAGE.Elements.Player.LocalPlayer.Handle)
                    {
                        if (anim == OtherType.None)
                        {
                            if (oldValue is int oldAnim)
                                UI.CEF.Animations.ToggleAnim("a-" + (OtherType)oldAnim, false);

                            Main.Render -= UI.CEF.Animations.Render;

                            Input.Core.ExtraBind cancelAnimKb = Input.Core.Get(BindTypes.CancelAnimation);

                            if (!cancelAnimKb.IsDisabled)
                                Input.Core.Get(BindTypes.CancelAnimation).Disable();
                        }
                        else
                        {
                            UI.CEF.Animations.ToggleAnim("a-" + anim, true);
                        }
                    }

                    if (anim == OtherType.None)
                    {
                        Management.Animations.Core.Stop(player);

                        pData.ActualAnimation = null;
                    }
                    else
                    {
                        Animation animData = Management.Animations.Core.OtherAnimsList[anim];

                        Management.Animations.Core.Play(player, animData);

                        pData.ActualAnimation = animData;
                    }
                }
            );

            AddDataHandler("Anim::General",
                (pData, value, oldValue) =>
                {
                    RAGE.Elements.Player player = pData.Player;

                    var anim = (GeneralType)((int?)value ?? -1);

                    if (anim == GeneralType.None)
                    {
                        Management.Animations.Core.Stop(player);

                        pData.ActualAnimation = null;
                    }
                    else
                    {
                        Animation animData = Management.Animations.Core.GeneralAnimsList[anim];

                        Management.Animations.Core.Play(player, animData);

                        pData.ActualAnimation = animData;
                    }
                }
            );

            AddDataHandler("DCR",
                (pData, value, oldValue) =>
                {
                    Customization.TattooData.ClearAll(pData.Player);

                    if (value is JArray jArr)
                        foreach (int x in jArr.ToObject<List<int>>())
                        {
                            Customization.TattooData tattoo = Customization.GetTattooData(x);

                            tattoo.TryApply(pData.Player);
                        }
                }
            );

            AddDataHandler("CHO",
                (pData, value, oldValue) =>
                {
                    Customization.HairOverlay.ClearAll(pData.Player);

                    if (value is int valueInt)
                        Customization.GetHairOverlay(pData.Sex, valueInt)?.Apply(pData.Player);
                }
            );

            AddDataHandler("Belt::On",
                (pData, value, oldValue) =>
                {
                    if (pData.Player.Handle != RAGE.Elements.Player.LocalPlayer.Handle)
                        return;

                    bool state = (bool?)value ?? false;

                    RAGE.Elements.Player player = pData.Player;

                    if (state)
                    {
                        Main.Update -= Vehicles.BeltTick;
                        Main.Update += Vehicles.BeltTick;
                    }
                    else
                    {
                        Main.Update -= Vehicles.BeltTick;
                    }

                    player.SetConfigFlag(32, !state);

                    HUD.SwitchBeltIcon(state);
                }
            );

            AddDataHandler("PST",
                (pData, value, oldValue) =>
                {
                    RAGE.Elements.Player player = pData.Player;

                    var state = (Misc.Phone.PhoneStateTypes)((int?)value ?? 0);

                    Misc.Phone.SetState(player, state);
                }
            );

            AddDataHandler("Crawl::On",
                (pData, value, oldValue) =>
                {
                    RAGE.Elements.Player player = pData.Player;

                    bool state = (bool?)value ?? false;

                    if (player.Handle == RAGE.Elements.Player.LocalPlayer.Handle)
                    {
                        if (state)
                            Crawl.On(true);
                        else
                            Crawl.Off(true);
                    }
                }
            );

            AddDataHandler("Crouch::On",
                (pData, value, oldValue) =>
                {
                    RAGE.Elements.Player player = pData.Player;

                    bool state = (bool?)value ?? false;

                    if (state)
                        Crouch.On(true, player);
                    else
                        Crouch.Off(true, player);
                }
            );

            AddDataHandler("IsInvalid",
                (pData, value, oldValue) =>
                {
                    RAGE.Elements.Player player = pData.Player;
                    bool state = (bool?)value ?? false;

                    if (player.Handle == RAGE.Elements.Player.LocalPlayer.Handle)
                        Special.DisabledPerson = state;
                }
            );

            AddDataHandler("IsInvisible",
                (pData, value, oldValue) =>
                {
                    RAGE.Elements.Player player = pData.Player;

                    bool state = (bool?)value ?? false;

                    player.SetNoCollisionEntity(RAGE.Elements.Player.LocalPlayer.Handle, !state);
                }
            );

            AddDataHandler("Sex",
                (pData, value, oldValue) =>
                {
                    if (pData.Player.Handle != RAGE.Elements.Player.LocalPlayer.Handle)
                        return;

                    var state = (bool)value;

                    Menu.SetSex(state);
                }
            );

            AddDataHandler("Knocked",
                (pData, value, oldValue) =>
                {
                    RAGE.Elements.Player player = pData.Player;

                    bool state = (bool?)value ?? false;

                    if (state)
                        player.SetCanRagdoll(false);
                    else
                        player.SetCanRagdoll(true);
                }
            );

            AddDataHandler("AdminLevel",
                (pData, value, oldValue) =>
                {
                    if (pData.Player == RAGE.Elements.Player.LocalPlayer)
                    {
                        int level = (int?)value ?? 0;

                        SetPlayerAsAdmin(level);
                    }
                }
            );

            AddDataHandler("VoiceRange",
                (pData, value, oldValue) =>
                {
                    RAGE.Elements.Player player = pData.Player;

                    var vRange = (float)value;

                    if (player.Handle == RAGE.Elements.Player.LocalPlayer.Handle)
                    {
                        // Voice Off
                        if (vRange > 0f)
                        {
                            Voice.Muted = false;

                            HUD.SwitchMicroIcon(true);

                            Main.Update -= Management.Microphone.Core.OnTick;
                            Main.Update += Management.Microphone.Core.OnTick;

                            Management.Microphone.Core.StartUpdateListeners();

                            Management.Microphone.Core.SetTalkingAnim(RAGE.Elements.Player.LocalPlayer, true);
                        }
                        // Voice On / Muted
                        else if (vRange <= 0f)
                        {
                            Management.Microphone.Core.StopUpdateListeners();

                            Voice.Muted = true;

                            HUD.SwitchMicroIcon(false);

                            Management.Microphone.Core.SetTalkingAnim(RAGE.Elements.Player.LocalPlayer, false);

                            Main.Update -= Management.Microphone.Core.OnTick;

                            if (vRange < 0f)
                                HUD.SwitchMicroIcon(null);
                        }
                    }
                    else
                    {
                        if (vRange > 0f)
                            Management.Microphone.Core.AddTalker(player);
                        else
                            Management.Microphone.Core.RemoveTalker(player);
                    }
                }
            );

            AddDataHandler("VehicleSeat",
                async (pData, value, oldValue) =>
                {
                    RAGE.Elements.Player player = pData.Player;

                    int seat = (int?)value ?? -1;

                    if (seat >= 0)
                    {
                        if (player.Vehicle?.Exists != true)
                            return;

                        player.SetIntoVehicle(player.Vehicle.Handle, seat - 1);

                        AsyncTask.Methods.Run(() =>
                            {
                                UpdateHat(player);
                            },
                            250
                        );

                        if (player.Handle == RAGE.Elements.Player.LocalPlayer.Handle)
                        {
                            if (seat == 0 || seat == 1)
                            {
                                RAGE.Elements.Vehicle veh = RAGE.Elements.Player.LocalPlayer.Vehicle;

                                EntitiesData.VehicleData vData = null;

                                do
                                {
                                    vData = EntitiesData.VehicleData.GetData(veh);

                                    if (vData == null)
                                    {
                                        await Invoker.WaitAsync(100);

                                        continue;
                                    }

                                    HUD.SwitchSpeedometer(true);

                                    if (seat == 0)
                                        Vehicles.StartDriverSync();

                                    break;
                                }
                                while (veh?.Exists == true && veh.GetPedInSeat(seat - 1, 0) == RAGE.Elements.Player.LocalPlayer.Handle);
                            }
                            else
                            {
                                HUD.SwitchSpeedometer(false);
                            }
                        }
                    }
                    else
                    {
                        UpdateHat(player);
                    }
                }
            );
        }

        /// <summary>Готов ли персонаж к игре?</summary>
        public static bool CharacterLoaded { get; set; }

        private static AsyncTask WoundedTask { get; set; }
        private static AsyncTask HungerTask { get; set; }

        private static AsyncTask RentedVehiclesCheckTask { get; set; }

        private static float FlyF { get; set; } = 2f;
        private static float FlyW { get; set; } = 2f;
        private static float FlyH { get; set; } = 2f;

        private static void InvokeHandler(string dataKey, PlayerData pData, object value, object oldValue = null)
        {
            dataActions.GetValueOrDefault(dataKey)?.Invoke(pData, value, oldValue);
        }

        private static void AddDataHandler(string dataKey, Action<PlayerData, object, object> action)
        {
            Events.AddDataHandler(dataKey,
                async (entity, value, oldValue) =>
                {
                    if (entity is RAGE.Elements.Player player)
                    {
                        // ugly fix rage bug when resetted data handler is ALWAYS triggered before setted data handlers (wrong order)
                        if (value != null)
                            await Invoker.WaitAsync();

                        var data = PlayerData.GetData(player);

                        if (data == null)
                            return;

                        action.Invoke(data, value, oldValue);
                    }
                }
            );

            dataActions.Add(dataKey, action);
        }

        public static async System.Threading.Tasks.Task OnPlayerStreamIn(RAGE.Elements.Player player)
        {
            if (player.IsLocal)
                return;

            var data = PlayerData.GetData(player);

            data?.Reset();

            player.AutoVolume = false;
            player.Voice3d = false;
            player.VoiceVolume = 0f;

            data = new PlayerData(player);

            if (data.CID == 0)
                return;

            PlayerData.SetData(player, data);

            if (data.VehicleSeat >= 0)
                InvokeHandler("VehicleSeat", data, data.VehicleSeat);

            InvokeHandler("IsInvisible", data, data.IsInvisible);

            InvokeHandler("CHO", data, player.GetSharedData("CHO"));

            InvokeHandler("DCR", data, player.GetSharedData<JArray>("DCR"));

            InvokeHandler("WCD", data, data.WeaponComponents);

            if (data.VoiceRange > 0f)
                Management.Microphone.Core.AddTalker(player);

            if (data.CrouchOn)
                Crouch.On(true, player);

            Misc.Phone.PhoneStateTypes phoneStateType = data.PhoneStateType;

            if (phoneStateType != Misc.Phone.PhoneStateTypes.Off)
                Misc.Phone.SetState(player, phoneStateType);

            if (data.GeneralAnim != GeneralType.None)
                InvokeHandler("Anim::General", data, (int)data.GeneralAnim);
            else if (data.OtherAnim != OtherType.None)
                InvokeHandler("Anim::Other", data, (int)data.OtherAnim);
        }

        public static async System.Threading.Tasks.Task OnPlayerStreamOut(RAGE.Elements.Player player)
        {
            var data = PlayerData.GetData(player);

            if (data == null)
                return;

            data.Reset();
        }

        public static void UpdateHat(RAGE.Elements.Player player)
        {
            if (player == null)
                return;

            var pData = PlayerData.GetData(player);

            if (pData == null)
                return;

            string[] hData = pData.Hat?.Split('|');

            if (hData == null)
            {
                player.ClearProp(0);

                return;
            }

            player.SetPropIndex(0, int.Parse(hData[0]), int.Parse(hData[1]), true);
        }

        private static void UpdateSkill(SkillTypes sType, int value)
        {
            if (sType == SkillTypes.Strength)
            {
                value = (int)System.Math.Round(0.5f * value); // 20 * a

                Stats.StatSetInt(Joaat.Hash("MP0_STAMINA"), value, true);
                Stats.StatSetInt(Joaat.Hash("MP0_STRENGTH"), value, true);
                Stats.StatSetInt(Joaat.Hash("MP0_LUNG_CAPACITY"), value, true);
            }
            else if (sType == SkillTypes.Shooting)
            {
                value = (int)System.Math.Round(0.25f * value); // 10 * a

                Stats.StatSetInt(Joaat.Hash("MP0_SHOOTING_ABILITY"), value, true);
            }
        }

        private static void UpdateDrivingSkill(bool hasLicense)
        {
            Stats.StatSetInt(Joaat.Hash("MP0_DRIVING_ABILITY"), hasLicense ? 50 : 0, true);
        }

        private static void UpdateFlyingSkill(bool hasLicense)
        {
            Stats.StatSetInt(Joaat.Hash("MP0_FLYING_ABILITY"), hasLicense ? 50 : 0, true);
        }

        private static void UpdateBikeSkill(bool hasLicense)
        {
            Stats.StatSetInt(Joaat.Hash("MP0_WHEELIE_ABILITY"), hasLicense ? 50 : 0, true);
        }

        private static void UpdateAchievement(PlayerData pData, AchievementTypes aType, int value, int maxValue)
        {
            if (pData == null)
            {
                pData = PlayerData.GetData(RAGE.Elements.Player.LocalPlayer);

                if (pData == null)
                    return;
            }

            if (pData.Achievements?.ContainsKey(aType) == true)
            {
                Menu.UpdateAchievement(aType, value, maxValue);
            }
            else
            {
                string achievementName = Locale.Get(Strings.GetKeyFromTypeByMemberName(aType.GetType(), aType.ToString(), "NAME_0") ?? "null");
                string achievementDesc = Locale.Get(Strings.GetKeyFromTypeByMemberName(aType.GetType(), aType.ToString(), "DESC_0") ?? "null");

                Menu.AddAchievement(aType, value, maxValue, achievementName, achievementDesc);
            }
        }

        public static async void TryShowWeaponSkinsMenu()
        {
            var pData = PlayerData.GetData(RAGE.Elements.Player.LocalPlayer);

            if (pData == null)
                return;

            Dictionary<WeaponSkin.ItemData.Types, string> wSkins = pData.WeaponSkins;

            if (wSkins.Count == 0)
            {
                Notification.ShowError(Locale.Notifications.Inventory.NoWeaponSkins);

                return;
            }

            await ActionBox.ShowSelect("WeaponSkinsMenuSelect",
                Locale.Actions.WeaponSkinsMenuSelectHeader,
                wSkins.Select(x => ((decimal)x.Key, $"{Locale.Get(Language.Strings.GetKeyFromTypeByMemberName(x.Key.GetType(), x.Key.ToString(), "CHOOSE_TEXT_0") ?? "null")} | {Items.Core.GetName(x.Value).Split(' ')[0]}"))
                      .ToArray(),
                Locale.Actions.SelectOkBtn1,
                Locale.Actions.SelectCancelBtn1,
                ActionBox.DefaultBindAction,
                async (rType, id) =>
                {
                    var pData = PlayerData.GetData(RAGE.Elements.Player.LocalPlayer);

                    if (pData == null)
                        return;

                    Dictionary<WeaponSkin.ItemData.Types, string> wSkins = pData.WeaponSkins;

                    if (rType == ActionBox.ReplyTypes.OK)
                    {
                        if (!wSkins.Keys.Where(x => (int)x == id).Any())
                        {
                            ActionBox.Close();

                            return;
                        }

                        if (ActionBox.LastSent.IsSpam(500, false, true))
                            return;

                        if ((bool)await Events.CallRemoteProc("WSkins::Rm", id))
                            ActionBox.Close();
                    }
                    else if (rType == ActionBox.ReplyTypes.Cancel)
                    {
                        ActionBox.Close();
                    }
                }
            );
        }

        public static void CloseAll(bool onlyInterfaces = false)
        {
            var pData = PlayerData.GetData(RAGE.Elements.Player.LocalPlayer);

            Ui.SetPauseMenuActive(false);

            ActionBox.Close();

            if (pData != null)
                Misc.Phone.CallChangeState(pData, Misc.Phone.PhoneStateTypes.Off);

            HUD.Menu.Switch(false);
            Inventory.Close();
            Interaction.CloseMenu();
            Menu.Close();
            UI.CEF.Animations.Close();
            Shop.Close(true);
            Gas.Close(true);

            Documents.Close();

            BlipsMenu.Close(true);
            ATM.Close();
            UI.CEF.Bank.Close(true);

            Estate.Close(true);
            EstateAgency.Close(true);

            GarageMenu.Close();
            HouseMenu.Close(true);
            BusinessMenu.Close(true);

            FractionMenu.Close();
            PoliceTabletPC.Close();

            ShootingRange.Finish();

            CasinoMinigames.Close();

            NPCs.NPC.CurrentNPC?.SwitchDialogue(false);

            if (!onlyInterfaces)
            {
                PushVehicle.Off();
                Crouch.Off();
                Crawl.Off();

                Finger.Stop();
            }
        }

        private static void SetPlayerAsAdmin(int aLvl)
        {
            int flyBindIdx = RAGE.Elements.Player.LocalPlayer.GetData<int>("ADMIN::BINDS::FLY");

            if (flyBindIdx >= 0)
                Input.Core.Unbind(flyBindIdx);

            if (aLvl <= 0)
            {
            }
            else
            {
                RAGE.Elements.Player.LocalPlayer.SetData("ADMIN::BINDS::FLY", Input.Core.Bind(VirtualKeys.F5, true, () => Management.Commands.Core.Fly()));
            }
        }

        private static void FlyRender()
        {
            Vector3 pos = RAGE.Elements.Player.LocalPlayer.GetCoords(false);
            Vector3 dir = Geometry.RotationToDirection(Cam.GetGameplayCamRot(0));

            if (Pad.IsControlPressed(32, 32)) // W
            {
                if (FlyF < 8f)
                    FlyF *= 1.025f;

                pos.X += dir.X * FlyF;
                pos.Y += dir.Y * FlyF;
                pos.Z += dir.Z * FlyF;
            }
            else if (Pad.IsControlPressed(32, 33)) // S
            {
                if (FlyF < 8f)
                    FlyF *= 1.025f;

                pos.X -= dir.X * FlyF;
                pos.Y -= dir.Y * FlyF;
                pos.Z -= dir.Z * FlyF;
            }
            else
            {
                FlyF = 2f;
            }

            if (Pad.IsControlPressed(32, 34)) // A
            {
                if (FlyW < 8f)
                    FlyW *= 1.025f;

                pos.X += -dir.Y * FlyW;
                pos.Y += dir.X * FlyW;
            }
            else if (Pad.IsControlPressed(32, 35)) // D
            {
                if (FlyW < 8f)
                    FlyW *= 1.05f;

                pos.X -= -dir.Y * FlyW;
                pos.Y -= dir.X * FlyW;
            }
            else
            {
                FlyW = 2f;
            }

            if (Pad.IsControlPressed(32, 321)) // Space
            {
                if (FlyH < 8f)
                    FlyH *= 1.025f;

                pos.Z += FlyH;
            }
            else if (Pad.IsControlPressed(32, 326)) // LCtrl
            {
                if (FlyH < 8f)
                    FlyH *= 1.05f;

                pos.Z -= FlyH;
            }
            else
            {
                FlyH = 2f;
            }

            RAGE.Elements.Player.LocalPlayer.SetCoordsNoOffset(pos.X, pos.Y, pos.Z, false, false, false);
        }
    }
}