using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.Animations;
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
using BlaineRP.Client.Game.Quests;
using BlaineRP.Client.Game.Scripts.Misc;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Game.UI.CEF.Phone.Apps;
using BlaineRP.Client.Game.UI.CEF.Phone.Enums;
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
using Bank = BlaineRP.Client.Game.UI.CEF.Bank;
using Chat = BlaineRP.Client.Game.UI.CEF.Chat;
using Convert = BlaineRP.Client.Utils.Convert;
using Discord = BlaineRP.Client.Game.Management.Misc.Discord;
using Interaction = BlaineRP.Client.Game.Management.Interaction;
using Math = System.Math;
using NPC = BlaineRP.Client.Game.NPCs.NPC;
using Player = RAGE.Elements.Player;
using Scaleform = BlaineRP.Client.Game.Helpers.Scaleform;
using Streaming = BlaineRP.Client.Utils.Game.Streaming;
using Task = System.Threading.Tasks.Task;
using Vehicle = BlaineRP.Client.Game.Data.Vehicles.Vehicle;
using VehicleData = BlaineRP.Client.Game.EntitiesData.VehicleData;

namespace BlaineRP.Client.Game.Scripts.Sync
{
    [Script]
    public class Players
    {
        /// <summary>Готов ли персонаж к игре?</summary>
        public static bool CharacterLoaded { get; set; }

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

        private static AsyncTask WoundedTask { get; set; }
        private static AsyncTask HungerTask { get; set; }

        private static AsyncTask RentedVehiclesCheckTask { get; set; }

        private static readonly Dictionary<string, Action<PlayerData, object, object>> dataActions =
            new Dictionary<string, Action<PlayerData, object, object>>();

        private static void InvokeHandler(string dataKey, PlayerData pData, object value, object oldValue = null)
        {
            dataActions.GetValueOrDefault(dataKey)?.Invoke(pData, value, oldValue);
        }

        private static void AddDataHandler(string dataKey, Action<PlayerData, object, object> action)
        {
            Events.AddDataHandler(dataKey,
                async (entity, value, oldValue) =>
                {
                    if (entity is Player player)
                    {
                        // ugly fix rage bug when resetted data handler is ALWAYS triggered before setted data handlers (wrong order)
                        if (value != null)
                            await Invoker.WaitAsync();

                        var data = PlayerData.GetData(player);

                        if (data == null)
                            return;

                        action.Invoke(data, value, oldValue);
                    }
                });

            dataActions.Add(dataKey, action);
        }

        public static async Task OnPlayerStreamIn(Player player)
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
                Game.Management.Microphone.Core.AddTalker(player);

            if (data.CrouchOn)
                Crouch.On(true, player);

            var phoneStateType = data.PhoneStateType;

            if (phoneStateType != Game.Scripts.Misc.Phone.PhoneStateTypes.Off)
                Game.Scripts.Misc.Phone.SetState(player, phoneStateType);

            if (data.GeneralAnim != GeneralTypes.None)
                InvokeHandler("Anim::General", data, (int)data.GeneralAnim);
            else if (data.OtherAnim != OtherTypes.None)
                InvokeHandler("Anim::Other", data, (int)data.OtherAnim);
        }

        public static async Task OnPlayerStreamOut(Player player)
        {
            var data = PlayerData.GetData(player);

            if (data == null)
                return;

            data.Reset();
        }

        public Players()
        {
            CharacterLoaded = false;

            new AsyncTask(() =>
                {
                    var players = Entities.Players.Streamed;

                    for (var i = 0; i < players.Count; i++)
                    {
                        var pData = PlayerData.GetData(players[i]);

                        if (pData == null)
                            continue;

                        if (pData.ActualAnimation is Game.Animations.Animation anim)
                            if (!pData.Player.IsPlayingAnim(anim.Dict, anim.Name, 3))
                                Game.Animations.Core.Play(pData.Player, anim);

                        if (players[i].GetData<Action>("AttachMethod") is Action act)
                            act.Invoke();
                    }
                },
                2_500,
                true).Run();

            Events.Add("Players::CloseAuth", async args => { Auth.CloseAll(); });

            Events.Add("Players::CharacterPreload",
                async args =>
                {
                    if (CharacterLoaded)
                        return;

                    while (!Game.World.Core.Preloaded)
                    {
                        await Invoker.WaitAsync();
                    }

                    Player.LocalPlayer.AutoVolume = false;
                    Player.LocalPlayer.VoiceVolume = 0f;

                    await Browser.Render(Browser.IntTypes.Inventory_Full, true);

                    await Browser.Render(Browser.IntTypes.Chat, true);

                    await Browser.Render(Browser.IntTypes.Interaction, true);

                    await Browser.Render(Browser.IntTypes.NPC, true);

                    await Browser.Render(Browser.IntTypes.Phone, true);

                    Invoker.Invoke(0x95C0A5BBDC189AA1);

                    Browser.Window.ExecuteJs("Hud.createSpeedometer", 500);

                    var player = Player.LocalPlayer;

                    var data = new PlayerData(Player.LocalPlayer);

                    data.FastAnim = FastTypes.None;

                    var sData = (JObject)args[0];

                    data.Familiars = Json.Deserialize<HashSet<uint>>((string)sData["Familiars"]);

                    data.Licenses = Json.Deserialize<HashSet<LicenseTypes>>((string)sData["Licenses"]);

                    data.Skills = Json.Deserialize<Dictionary<SkillTypes, int>>((string)sData["Skills"]);

                    data.PhoneNumber = (uint)sData["PN"];

                    if (sData.TryGetValue("P", out var value))
                    {
                        foreach (var x in ((JArray)value).ToObject<List<string>>())
                        {
                            var t = x.Split('&');

                            Punishment.AddPunishment(new Punishment
                            {
                                Id = uint.Parse(t[0]),
                                Type = (Punishment.Types)int.Parse(t[1]),
                                EndDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(t[2])).DateTime,
                                AdditionalData = t[3].Length > 0 ? t[3] : null,
                            });
                        }

                        Punishment.StartCheckTask();
                    }

                    data.Contacts = sData.TryGetValue("Conts", out var value1)
                        ? ((JObject)value1).ToObject<Dictionary<uint, string>>()
                        : new Dictionary<uint, string>();

                    data.PhoneBlacklist = sData.TryGetValue("PBL", out var value2)
                        ? ((JArray)value2).ToObject<List<uint>>()
                        : new List<uint>();

                    data.AllSMS = sData.TryGetValue("SMS", out var value3)
                        ? Json.Deserialize<List<string>>((string)value3).Select(x => new SMS.Message(x)).ToList()
                        : new List<SMS.Message>();

                    data.OwnedVehicles = sData.TryGetValue("Vehicles", out var value4)
                        ? Json.Deserialize<List<string>>((string)value4)
                              .Select(x =>
                               {
                                   var data = x.Split('_');
                                   return (Convert.ToUInt32(data[0]), Game.Data.Vehicles.Core.GetById(data[1]));
                               })
                              .ToList()
                        : new List<(uint VID, Vehicle Data)>();

                    data.OwnedBusinesses = sData.TryGetValue("Businesses", out var value5)
                        ? Json.Deserialize<List<int>>((string)value5).Select(x => Business.All[x]).ToList()
                        : new List<Business>();

                    data.OwnedHouses = sData.TryGetValue("Houses", out var value6)
                        ? Json.Deserialize<List<uint>>((string)value6).Select(x => House.All[x]).ToList()
                        : new List<House>();

                    data.OwnedApartments = sData.TryGetValue("Apartments", out var value7)
                        ? Json.Deserialize<List<uint>>((string)value7).Select(x => Apartments.All[x]).ToList()
                        : new List<Apartments>();

                    data.OwnedGarages = sData.TryGetValue("Garages", out var value8)
                        ? Json.Deserialize<List<uint>>((string)value8).Select(x => Garage.All[x]).ToList()
                        : new List<Garage>();

                    if (sData.TryGetValue("MedCard", out var value9))
                        data.MedicalCard = Json.Deserialize<MedicalCard>((string)value9);

                    if (sData.TryGetValue("SHB", out var value10))
                    {
                        var shbData = ((string)value10).Split('_');

                        data.SettledHouseBase = (Game.Estates.Core.HouseTypes)int.Parse(shbData[0]) == Game.Estates.Core.HouseTypes.House
                            ? House.All[uint.Parse(shbData[1])]
                            : (HouseBase)Apartments.All[uint.Parse(shbData[1])];
                    }

                    var achievements = Json.Deserialize<List<string>>((string)sData["Achievements"])
                                           .ToDictionary(x => (AchievementTypes)Convert.ToInt32(x.Split('_')[0]),
                                                y =>
                                                {
                                                    var data = y.Split('_');

                                                    return (Convert.ToInt32(data[1]), Convert.ToInt32(data[2]));
                                                });

                    foreach (var x in achievements)
                    {
                        UpdateAchievement(data, x.Key, x.Value.Item1, x.Value.Item2);
                    }

                    data.Achievements = achievements.ToDictionary(x => x.Key,
                        y => (y.Value.Item1, y.Value.Item1 >= y.Value.Item2));

                    data.Quests = sData.TryGetValue("Quests", out var value11)
                        ? Json.Deserialize<List<string>>((string)value11)
                              .Select(y =>
                               {
                                   var data = y.Split('~');
                                   return new Quest((QuestTypes)int.Parse(data[0]),
                                       byte.Parse(data[1]),
                                       int.Parse(data[2]),
                                       data[3].Length > 0 ? data[3] : null);
                               })
                              .ToList()
                        : new List<Quest>();

                    data.Furniture = sData.TryGetValue("Furniture", out var value12)
                        ? Json.Deserialize<Dictionary<uint, string>>((string)value12)
                              .ToDictionary(x => x.Key, x => Furniture.GetData(x.Value))
                        : new Dictionary<uint, Furniture>();

                    data.WeaponSkins = sData.TryGetValue("WSkins", out var value13)
                        ? Json.Deserialize<List<string>>((string)value13)
                              .ToDictionary(x =>
                                       ((WeaponSkin.ItemData)Game.Items.Core.GetData(x, typeof(WeaponSkin))).Type,
                                   x => x)
                        : new Dictionary<WeaponSkin.ItemData.Types, string>();

                    if (sData.TryGetValue("RV", out var value14))
                    {
                        var vehs = ((JArray)value14).ToObject<List<string>>().Select(x => x.Split('&')).ToList();

                        foreach (var x in vehs)
                        {
                            Vehicles.RentedVehicle.All.Add(new Vehicles.RentedVehicle(ushort.Parse(x[0]),
                                Game.Data.Vehicles.Core.GetById(x[1])));
                        }

                        RentedVehiclesCheckTask = new AsyncTask(Vehicles.RentedVehicle.Check, 1000, true);

                        RentedVehiclesCheckTask.Run();
                    }

                    foreach (var x in data.Skills)
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
                        Json.Deserialize<Dictionary<uint, (int, string, int, int)>>((string)sData["Gifts"]));

                    Menu.SetOrganisation((string)sData["Org"]);

                    Menu.SetFraction(FractionTypes.None);

                    foreach (var x in data.Skills)
                    {
                        Menu.UpdateSkill(x.Key, x.Value);
                    }

                    while (data.CID == 0)
                    {
                        await Invoker.WaitAsync();
                    }

                    PlayerData.SetData(Player.LocalPlayer, data);

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

                    InvokeHandler("DCR", data, Player.LocalPlayer.GetSharedData<JArray>("DCR"));

                    new AsyncTask(() =>
                        {
                            //Events.CallRemote("Player::UpdateTime");

                            var minuteTimeSpan = TimeSpan.FromMinutes(1);

                            Menu.TimePlayed = Menu.TimePlayed.Add(minuteTimeSpan);
                        },
                        60_000,
                        true,
                        60_000).Run();

                    HUD.Menu.UpdateCurrentTypes(true,
                        HUD.Menu.Types.Menu,
                        HUD.Menu.Types.Documents,
                        HUD.Menu.Types.BlipsMenu);

                    if (data.WeaponSkins.Count > 0)
                        HUD.Menu.UpdateCurrentTypes(true, HUD.Menu.Types.WeaponSkinsMenu);

                    Settings.User.Initialization.Load();
                    Game.Input.Core.LoadAll();

                    Game.UI.CEF.Phone.Phone.Preload();

                    await UI.CEF.Animations.Load();

                    await Events.CallRemoteProc("Players::CRI",
                        data.IsInvalid,
                        Other.CurrentEmotion,
                        Other.CurrentWalkstyle);

                    CharacterLoaded = true;

                    Menu.UpdateSettingsData();
                    Menu.UpdateKeyBindsData();

                    Player.LocalPlayer.FreezePosition(false);

                    Player.LocalPlayer.SetInvincible(false);

                    Main.DisableAllControls(false);

                    var timeUpdateTask = new AsyncTask(() =>
                        {
                            HUD.UpdateTime();
                            Game.UI.CEF.Phone.Phone.UpdateTime();
                        },
                        1_000,
                        true);

                    timeUpdateTask.Run();

                    HUD.ShowHUD(!Interface.HideHUD);

                    Interaction.Enabled = true;
                    Game.World.Core.EnabledItemsOnGround = true;

                    Chat.Show(true);

                    ExtraLabel.Initialize();

                    ExtraColshape.Activate();

                    Discord.SetDefault();

                    await Invoker.WaitAsync(500);

                    foreach (var x in data.OwnedBusinesses)
                    {
                        x.ToggleOwnerBlip(true);
                    }

                    foreach (var x in data.OwnedHouses)
                    {
                        x.ToggleOwnerBlip(true);
                    }

                    foreach (var x in data.OwnedApartments)
                    {
                        x.ToggleOwnerBlip(true);
                    }

                    foreach (var x in data.OwnedGarages)
                    {
                        x.ToggleOwnerBlip(true);
                    }

                    data.SettledHouseBase?.ToggleOwnerBlip(true);

                    foreach (var x in data.Quests)
                    {
                        x.Initialize();
                    }

                    Game.Management.Attachments.Core.ReattachObjects(Player.LocalPlayer);
                });

            Events.Add("Player::Knocked",
                args =>
                {
                    var state = (bool)args[0];

                    if (state)
                    {
                        Main.DisableMove(true);

                        var attacker = Entities.Players.GetAtRemote(Convert.ToUInt16(args[1])) ?? Player.LocalPlayer;

                        Graphics.StartScreenEffect("DeathFailMPIn", 0, true);

                        var scaleformWaster = Scaleform.CreateShard("wasted",
                            Locale.Get("SCALEFORM_WASTED_HEADER"),
                            attacker.Handle == Player.LocalPlayer.Handle
                                ? Locale.Get("SCALEFORM_WASTED_ATTACKER_S")
                                : Locale.Get("SCALEFORM_WASTED_ATTACKER_P",
                                    attacker.GetName(true, false, true),
                                    PlayerData.GetData(attacker)?.CID ?? 0));

                        Death.Show();
                    }
                    else
                    {
                        Death.Close();

                        Main.DisableMove(false);

                        Graphics.StopScreenEffect("DeathFailMPIn");

                        Scaleform.Get("wasted")?.Destroy();
                    }
                });

            Events.Add("opday",
                args =>
                {
                    if (args == null || args.Length == 0)
                    {
                        Events.CallLocal("Chat::ShowServerMessage",
                            "Время зарплаты | Вы ничего не получаете, так как у Вас нет банковского счёта!");
                    }
                    else if (args.Length == 1)
                    {
                        var playedTime = TimeSpan.FromSeconds(Convert.ToInt64(args[0]));
                        var minTimeToGetPayday = TimeSpan.FromSeconds(Convert.ToInt64(args[1]));

                        Events.CallLocal("Chat::ShowServerMessage",
                            $"Время зарплаты | Вы ничего не получаете, так как за этот час Вы наиграли {playedTime.GetBeautyString()} (необходимо - {minTimeToGetPayday.GetBeautyString()})!");
                    }
                    else
                    {
                        var joblessBenefit = Convert.ToDecimal(args[0]);
                        var fractionSalary = Convert.ToDecimal(args[1]);
                        var organisationSalary = Convert.ToDecimal(args[2]);

                        if (joblessBenefit > 0)
                        {
                            Events.CallLocal("Chat::ShowServerMessage",
                                $"Время зарплаты | Вы получаете {Locale.Get("GEN_MONEY_0", joblessBenefit)} (пособие по безработице) на свой счёт!");
                        }
                        else
                        {
                            if (organisationSalary == 0)
                                Events.CallLocal("Chat::ShowServerMessage",
                                    $"Время зарплаты | Вы получаете {Locale.Get("GEN_MONEY_0", fractionSalary)} (от фракции) на свой счёт!");
                            else if (fractionSalary != 0)
                                Events.CallLocal("Chat::ShowServerMessage",
                                    $"Время зарплаты | Вы получаете {Locale.Get("GEN_MONEY_0", fractionSalary)} (от фракции) и {Locale.Get("GEN_MONEY_0", organisationSalary)} (от организации) на свой счёт!");
                            else
                                Events.CallLocal("Chat::ShowServerMessage",
                                    $"Время зарплаты | Вы получаете {Locale.Get("GEN_MONEY_0", fractionSalary)} (от организации) на свой счёт!");
                        }
                    }
                });

            Events.Add("Player::ParachuteS",
                args =>
                {
                    var parachuteWeaponHash = Joaat.Hash("gadget_parachute");

                    if (!(bool)args[0])
                    {
                        Player.LocalPlayer.RemoveWeaponFrom(parachuteWeaponHash);

                        if (!(bool)args[1])
                        {
                            Player.LocalPlayer.GetData<AsyncTask>("ParachuteATask")?.Cancel();

                            Player.LocalPlayer.ResetData("ParachuteATask");

                            if (Player.LocalPlayer.GetParachuteState() >= 0)
                                Player.LocalPlayer.ClearTasksImmediately();
                        }
                    }
                    else
                    {
                        Player.LocalPlayer.GetData<AsyncTask>("ParachuteATask")?.Cancel();

                        Player.LocalPlayer.RemoveWeaponFrom(parachuteWeaponHash);

                        Player.LocalPlayer.GiveWeaponTo(parachuteWeaponHash, 0, false, false);

                        RAGE.Game.Player.SetPlayerParachuteVariationOverride(66, 0, 2, false);
                        RAGE.Game.Player.SetPlayerParachuteTintIndex(6);

                        AsyncTask task = null;

                        var isInFly = false;

                        var lastSentState = int.MinValue;

                        task = new AsyncTask(() =>
                            {
                                var pState = Player.LocalPlayer.GetParachuteState();

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
                            true);

                        Player.LocalPlayer.SetData("ParachuteATask", task);

                        task.Run();
                    }
                });

            Events.Add("Player::RVehs::U",
                args =>
                {
                    var rId = (ushort)(int)args[0];

                    var rentedVehs = Vehicles.RentedVehicle.All;

                    if (args.Length > 1)
                    {
                        var vTypeId = (string)args[1];

                        var vTypeData = Game.Data.Vehicles.Core.GetById(vTypeId);

                        rentedVehs.Add(new Vehicles.RentedVehicle(rId, vTypeData));

                        if (RentedVehiclesCheckTask == null)
                        {
                            RentedVehiclesCheckTask = new AsyncTask(Vehicles.RentedVehicle.Check, 1000, true);

                            RentedVehiclesCheckTask.Run();
                        }
                    }
                    else
                    {
                        var rVeh = rentedVehs.Where(x => x.RemoteId == rId).FirstOrDefault();

                        if (rVeh == null)
                            return;

                        rentedVehs.Remove(rVeh);

                        if (rentedVehs.Count == 0)
                        {
                            RentedVehiclesCheckTask?.Cancel();

                            RentedVehiclesCheckTask = null;
                        }
                    }

                    if (Game.UI.CEF.Phone.Phone.CurrentApp == AppTypes.Vehicles)
                        Game.UI.CEF.Phone.Phone.ShowApp(null, AppTypes.Vehicles);
                });

            Events.Add("Player::Waypoint::Set",
                args =>
                {
                    var x = (float)args[0];
                    var y = (float)args[1];

                    Utils.Game.Misc.SetWaypoint(x, y);
                });

            Events.Add("Player::Smoke::Start",
                args =>
                {
                    var maxTime = (int)args[0];
                    var maxPuffs = (int)args[1];

                    Player.LocalPlayer.SetData("Smoke::Data::Puffs", maxPuffs);
                    Player.LocalPlayer.SetData("Smoke::Data::CTask",
                        new AsyncTask(() => Events.CallRemote("Players::Smoke::Stop"), maxTime));
                });

            Events.Add("Player::Smoke::Stop",
                args =>
                {
                    Player.LocalPlayer.ResetData("Smoke::Data::Puffs");

                    Player.LocalPlayer.GetData<AsyncTask>("Smoke::Data::CTask")?.Cancel();
                    Player.LocalPlayer.GetData<AsyncTask>("Smoke::Data::CTask1")?.Cancel();
                    Player.LocalPlayer.GetData<AsyncTask>("Smoke::Data::CTask2")?.Cancel();

                    Player.LocalPlayer.ResetData("Smoke::Data::CTask");
                    Player.LocalPlayer.ResetData("Smoke::Data::CTask1");
                    Player.LocalPlayer.ResetData("Smoke::Data::CTask2");
                });

            Events.Add("Player::Smoke::Puff",
                args =>
                {
                    var task1 = new AsyncTask(async () =>
                        {
                            await Streaming.RequestPtfx("core");

                            var fxHandle = Graphics.StartParticleFxLoopedOnEntityBone("exp_grd_bzgas_smoke",
                                Player.LocalPlayer.Handle,
                                0f,
                                0f,
                                0f,
                                0f,
                                0f,
                                0f,
                                Player.LocalPlayer.GetBoneIndex(20279),
                                0.15f,
                                false,
                                false,
                                false);

                            await Invoker.WaitAsync(1000);

                            Graphics.StopParticleFxLooped(fxHandle, false);
                        },
                        2000);

                    var task2 = new AsyncTask(() =>
                        {
                            if (!Player.LocalPlayer.HasData("Smoke::Data::Puffs"))
                                return;

                            Player.LocalPlayer.SetData("Smoke::Data::Puffs",
                                Player.LocalPlayer.GetData<int>("Smoke::Data::Puffs") - 1);
                        },
                        3000);

                    task1.Run();
                    task2.Run();

                    Player.LocalPlayer.SetData("Smoke::Data::CTask1", task1);
                    Player.LocalPlayer.SetData("Smoke::Data::CTask2", task2);
                });

            Events.Add("Player::CloseAll", args => CloseAll((bool)args[0]));

            Events.Add("Player::Quest::Upd",
                args =>
                {
                    var data = PlayerData.GetData(Player.LocalPlayer);

                    if (data == null)
                        return;

                    var qType = (QuestTypes)(int)args[0];

                    var quests = data.Quests;

                    var quest = quests.Where(x => x.Type == qType).FirstOrDefault();

                    if (args.Length < 3)
                    {
                        var success = (bool)args[1];

                        if (quest == null)
                            return;

                        quest.SetQuestAsCompleted(success, true);
                    }
                    else
                    {
                        var step = Convert.ToByte(args[1]);

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
                });

            Events.Add("Player::Achievements::Update",
                args =>
                {
                    var data = PlayerData.GetData(Player.LocalPlayer);

                    if (data == null)
                        return;

                    var aType = (AchievementTypes)(int)args[0];
                    var value = (int)args[1];
                    var maxValue = (int)args[2];

                    UpdateAchievement(data, aType, value, maxValue);

                    var achievementName = Locale.Get(Language.Strings.GetKeyFromTypeByMemberName(aType.GetType(), aType.ToString(), "NAME_0") ?? "null");

                    if (value >= maxValue)
                        Notification.Show(Notification.Types.Achievement,
                            achievementName,
                            Locale.Notifications.General.AchievementUnlockedText,
                            5000);
                });

            Events.Add("Player::Skills::Update",
                args =>
                {
                    var data = PlayerData.GetData(Player.LocalPlayer);

                    if (data == null)
                        return;

                    var sType = (SkillTypes)(int)args[0];
                    var value = (int)args[1];

                    var oldValue = data.Skills.GetValueOrDefault(sType, 0);

                    data.Skills[sType] = value;

                    Menu.UpdateSkill(sType, value);

                    UpdateSkill(sType, value);

                    Notification.Show(Notification.Types.Information,
                        Locale.Get("NOTIFICATION_HEADER_DEF"),
                        string.Format(
                            value >= oldValue
                                ? Locale.Notifications.General.SkillUp
                                : Locale.Notifications.General.SkillDown,
                            Locale.Get(Language.Strings.GetKeyFromTypeByMemberName(sType.GetType(), sType.ToString(), "NAME_1") ?? "null").ToLower(),
                            Math.Abs(value - oldValue),
                            value,
                            Static.PlayerMaxSkills[sType]));
                });

            Events.Add("Player::WSkins::Update",
                args =>
                {
                    var data = PlayerData.GetData(Player.LocalPlayer);

                    if (data == null)
                        return;

                    var add = (bool)args[0];

                    var id = (string)args[1];

                    var wSkins = data.WeaponSkins;

                    var type = ((WeaponSkin.ItemData)Game.Items.Core.GetData(id)).Type;

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
                });

            Events.Add("Player::MedCard::Update",
                args =>
                {
                    var data = PlayerData.GetData(Player.LocalPlayer);

                    if (data == null)
                        return;

                    if (args.Length > 0)
                        data.MedicalCard = Json.Deserialize<MedicalCard>((string)args[0]);
                    else
                        data.MedicalCard = null;
                });

            Events.Add("Player::Licenses::Update",
                args =>
                {
                    var data = PlayerData.GetData(Player.LocalPlayer);

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
                });

            Events.Add("Player::Familiars::Update",
                args =>
                {
                    var data = PlayerData.GetData(Player.LocalPlayer);

                    if (data == null)
                        return;

                    var add = (bool)args[0];
                    var cid = Convert.ToUInt32(args[1]);

                    if (add)
                        data.Familiars.Add(cid);
                    else
                        data.Familiars.Remove(cid);
                });

            Events.Add("Player::SettledHB",
                args =>
                {
                    var pType = (Game.Estates.Core.HouseTypes)(int)args[0];

                    var pId = (uint)(int)args[1];

                    var state = (bool)args[2];

                    var house = pType == Game.Estates.Core.HouseTypes.House
                        ? House.All[pId]
                        : (HouseBase)Apartments.All[pId];

                    house.ToggleOwnerBlip(state);

                    if (args.Length > 3)
                    {
                        var playerInit = Entities.Players.GetAtRemote((ushort)(int)args[3]);

                        if (state)
                        {
                            Notification.Show(Notification.Types.Information,
                                Locale.Get("NOTIFICATION_HEADER_DEF"),
                                string.Format(
                                    pType == Game.Estates.Core.HouseTypes.House
                                        ? Locale.Notifications.House.SettledHouse
                                        : Locale.Notifications.House.SettledApartments,
                                    playerInit.GetName(true, false, true)));
                        }
                        else
                        {
                            if (playerInit?.Handle == Player.LocalPlayer.Handle)
                                Notification.Show(Notification.Types.Information,
                                    Locale.Get("NOTIFICATION_HEADER_DEF"),
                                    pType == Game.Estates.Core.HouseTypes.House
                                        ? Locale.Notifications.House.ExpelledHouseSelf
                                        : Locale.Notifications.House.ExpelledApartmentsSelf);
                            else
                                Notification.Show(Notification.Types.Information,
                                    Locale.Get("NOTIFICATION_HEADER_DEF"),
                                    string.Format(
                                        pType == Game.Estates.Core.HouseTypes.House
                                            ? Locale.Notifications.House.ExpelledHouse
                                            : Locale.Notifications.House.ExpelledApartments,
                                        playerInit.GetName(true, false, true)));
                        }
                    }
                    else
                    {
                        if (state)
                            Notification.Show(Notification.Types.Information,
                                Locale.Get("NOTIFICATION_HEADER_DEF"),
                                pType == Game.Estates.Core.HouseTypes.House
                                    ? Locale.Notifications.House.SettledHouseAuto
                                    : Locale.Notifications.House.SettledApartmentsAuto);
                        else
                            Notification.Show(Notification.Types.Information,
                                Locale.Get("NOTIFICATION_HEADER_DEF"),
                                pType == Game.Estates.Core.HouseTypes.House
                                    ? Locale.Notifications.House.ExpelledHouseAuto
                                    : Locale.Notifications.House.ExpelledApartmentsAuto);
                    }
                });

            Events.Add("Player::Properties::Update",
                args =>
                {
                    var data = PlayerData.GetData(Player.LocalPlayer);

                    if (data == null)
                        return;

                    var add = (bool)args[0];

                    var pType = (PropertyTypes)(int)args[1];

                    if (pType == PropertyTypes.Vehicle)
                    {
                        var vid = Convert.ToUInt32(args[2]);

                        if (add)
                        {
                            var t = (vid, Game.Data.Vehicles.Core.GetById((string)args[3]));

                            if (!data.OwnedVehicles.Contains(t))
                                data.OwnedVehicles.Add(t);
                        }
                        else
                        {
                            var idx = data.OwnedVehicles.FindIndex(x => x.VID == vid);

                            if (idx < 0)
                                return;

                            data.OwnedVehicles.RemoveAt(idx);
                        }

                        if (Game.UI.CEF.Phone.Phone.CurrentApp == AppTypes.Vehicles)
                            Game.UI.CEF.Phone.Phone.ShowApp(null, AppTypes.Vehicles);
                    }
                    else if (pType == PropertyTypes.House)
                    {
                        var hid = Convert.ToUInt32(args[2]);

                        var t = House.All[hid];

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
                        var hid = Convert.ToUInt32(args[2]);

                        var t = Apartments.All[hid];

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
                        var gid = Convert.ToUInt32(args[2]);

                        var t = Garage.All[gid];

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

                        var t = Business.All[bid];

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
                });

            Events.Add("Player::Furniture::Update",
                args =>
                {
                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    var add = (bool)args[0];

                    if (add)
                    {
                        var furns = ((JArray)args[1]).ToObject<List<string>>();

                        foreach (var x in furns)
                        {
                            var d = x.Split('&');

                            var fUid = uint.Parse(d[0]);

                            var fId = d[1];

                            var fData = Furniture.GetData(fId);

                            if (pData.Furniture.TryAdd(fUid, fData))
                                if (HouseMenu.IsActive)
                                    HouseMenu.AddOwnedFurniture(fUid, fData);
                        }
                    }
                    else
                    {
                        var furns = ((JArray)args[1]).ToObject<List<string>>();

                        foreach (var x in furns)
                        {
                            var d = x.Split('&');

                            var fUid = uint.Parse(d[0]);

                            if (pData.Furniture.Remove(fUid))
                                if (HouseMenu.IsActive)
                                    HouseMenu.RemoveOwnedFurniture(fUid);
                        }
                    }
                });

            AddDataHandler("Fly",
                (pData, value, oldValue) =>
                {
                    if (pData.Player != Player.LocalPlayer)
                        return;

                    var state = (bool?)value ?? false;

                    Main.Render -= FlyRender;

                    if (state)
                    {
                        Player.LocalPlayer.ClearTasksImmediately();

                        Main.Render += FlyRender;
                    }
                });

            AddDataHandler("IsFrozen",
                (pData, value, oldValue) =>
                {
                    if (pData.Player.Handle != Player.LocalPlayer.Handle)
                        return;

                    var state = (bool?)value ?? false;

                    if (state)
                    {
                        Player.LocalPlayer.ClearTasksImmediately();

                        CloseAll();

                        Player.LocalPlayer.FreezePosition(true);

                        Main.DisableAllControls(true);

                        Game.Management.Weapons.Core.DisabledFiring = true;
                    }
                    else
                    {
                        Game.Management.Weapons.Core.DisabledFiring = false;

                        Player.LocalPlayer.FreezePosition(false);

                        Main.DisableAllControls(false);
                    }
                });

            AddDataHandler("Cash",
                (pData, value, oldValue) =>
                {
                    if (pData.Player.Handle != Player.LocalPlayer.Handle)
                        return;

                    var cash = Convert.ToUInt64(value);

                    HUD.SetCash(cash);
                    Menu.SetCash(cash);

                    var oldCash = oldValue == null ? cash : Convert.ToUInt64(oldValue);

                    if (cash == oldCash)
                        return;

                    if (cash > oldCash)
                        Notification.Show(Notification.Types.Cash,
                            Strings.Get("GEN_MONEY_ADD_0", cash - oldCash),
                            Locale.Get("NTFC_MONEY_CASH_0", Locale.Get("GEN_MONEY_0", cash)));
                    else
                        Notification.Show(Notification.Types.Cash,
                            Strings.Get("GEN_MONEY_REMOVE_0", oldCash - cash),
                            Locale.Get("NTFC_MONEY_CASH_0", Locale.Get("GEN_MONEY_0", cash)));
                });

            AddDataHandler("BankBalance",
                (pData, value, oldValue) =>
                {
                    if (pData.Player.Handle != Player.LocalPlayer.Handle)
                        return;

                    var bank = Convert.ToUInt64(value);

                    HUD.SetBank(bank);
                    Menu.SetBank(bank);

                    ATM.UpdateMoney(bank);
                    Bank.UpdateMoney(bank);
                    Game.UI.CEF.Phone.Apps.Bank.UpdateBalance(bank);

                    var oldBank = oldValue == null ? bank : Convert.ToUInt64(oldValue);

                    if (bank == oldBank)
                        return;

                    if (bank > oldBank)
                        Notification.Show(Notification.Types.Bank,
                            Strings.Get("GEN_MONEY_ADD_0", bank - oldBank),
                            Locale.Get("NTFC_MONEY_BANK_0", Locale.Get("GEN_MONEY_0", bank)));
                    else
                        Notification.Show(Notification.Types.Bank,
                            Strings.Get("GEN_MONEY_REMOVE_0", oldBank - bank),
                            Locale.Get("NTFC_MONEY_BANK_0", Locale.Get("GEN_MONEY_0", bank)));
                });

            AddDataHandler("IsWounded",
                (pData, value, oldValue) =>
                {
                    var player = pData.Player;

                    var state = (bool?)value ?? false;

                    if (player.Handle == Player.LocalPlayer.Handle)
                    {
                        if (state)
                        {
                            HUD.SwitchStatusIcon(HUD.StatusTypes.Wounded, true);

                            Notification.ShowHint(Locale.Notifications.Players.States.Wounded);

                            WoundedTask?.Cancel();

                            WoundedTask = new AsyncTask(() =>
                                {
                                    var pData = PlayerData.GetData(Player.LocalPlayer);

                                    if (pData == null || pData.IsInvincible)
                                        return;

                                    Player.LocalPlayer.SetRealHealth(Player.LocalPlayer.GetRealHealth() -
                                                                     WoundedReduceHP);
                                },
                                WoundedTime,
                                true,
                                WoundedTime / 2);

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
                });

            AddDataHandler("WCD",
                (pData, value, oldValue) =>
                {
                    var player = pData.Player;

                    if (value is string strData)
                        Game.Management.Weapons.Core.UpdateWeaponComponents(player, strData);
                });

            Events.Add("Players::WCD::U",
                args =>
                {
                    var player = (Player)args[0];

                    if (player?.Exists != true || player.Handle == Player.LocalPlayer.Handle)
                        return;

                    if (player.GetSharedData<string>("WCD") is string strData)
                        Game.Management.Weapons.Core.UpdateWeaponComponents(player, strData);
                });

            AddDataHandler("Mood",
                (pData, value, oldValue) =>
                {
                    if (pData.Player != Player.LocalPlayer)
                        return;

                    var mood = Convert.ToByte(value);

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
                });

            AddDataHandler("Satiety",
                (pData, value, oldValue) =>
                {
                    if (pData.Player != Player.LocalPlayer)
                        return;

                    var satiety = Convert.ToByte(value);

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
                                    var pData = PlayerData.GetData(Player.LocalPlayer);

                                    if (pData == null || pData.IsInvincible)
                                        return;

                                    var currentHp = Player.LocalPlayer.GetRealHealth();

                                    if (currentHp <= HungryLowestHP)
                                        return;

                                    if (currentHp - HungryReduceHP <= HungryLowestHP)
                                        Player.LocalPlayer.SetRealHealth(HungryLowestHP);
                                    else
                                        Player.LocalPlayer.SetRealHealth(currentHp - HungryReduceHP);
                                },
                                HungryTime,
                                true,
                                HungryTime / 2);
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
                });

            AddDataHandler("Emotion",
                (pData, value, oldValue) =>
                {
                    var player = pData.Player;

                    var emotion = (EmotionTypes)((int?)value ?? -1);

                    if (player.Handle == Player.LocalPlayer.Handle)
                    {
                        Other.CurrentEmotion = emotion;

                        UI.CEF.Animations.ToggleAnim("e-" + emotion, true);
                    }

                    Game.Animations.Core.Set(player, emotion);
                });

            AddDataHandler("Walkstyle",
                (pData, value, oldValue) =>
                {
                    var player = pData.Player;

                    var wStyle = (WalkstyleTypes)((int?)value ?? -1);

                    if (player.Handle == Player.LocalPlayer.Handle)
                    {
                        Other.CurrentWalkstyle = wStyle;

                        UI.CEF.Animations.ToggleAnim("w-" + wStyle, true);
                    }

                    if (!pData.CrouchOn)
                        Game.Animations.Core.Set(player, wStyle);
                });

            AddDataHandler("Anim::Other",
                (pData, value, oldValue) =>
                {
                    var player = pData.Player;

                    var anim = (OtherTypes)((int?)value ?? -1);

                    if (player.Handle == Player.LocalPlayer.Handle)
                    {
                        if (anim == OtherTypes.None)
                        {
                            if (oldValue is int oldAnim)
                                UI.CEF.Animations.ToggleAnim("a-" + (OtherTypes)oldAnim, false);

                            Main.Render -= UI.CEF.Animations.Render;

                            var cancelAnimKb = Game.Input.Core.Get(BindTypes.CancelAnimation);

                            if (!cancelAnimKb.IsDisabled)
                                Game.Input.Core.Get(BindTypes.CancelAnimation).Disable();
                        }
                        else
                        {
                            UI.CEF.Animations.ToggleAnim("a-" + anim, true);
                        }
                    }

                    if (anim == OtherTypes.None)
                    {
                        Game.Animations.Core.Stop(player);

                        pData.ActualAnimation = null;
                    }
                    else
                    {
                        var animData = Game.Animations.Core.OtherAnimsList[anim];

                        Game.Animations.Core.Play(player, animData);

                        pData.ActualAnimation = animData;
                    }
                });

            AddDataHandler("Anim::General",
                (pData, value, oldValue) =>
                {
                    var player = pData.Player;

                    var anim = (GeneralTypes)((int?)value ?? -1);

                    if (anim == GeneralTypes.None)
                    {
                        Game.Animations.Core.Stop(player);

                        pData.ActualAnimation = null;
                    }
                    else
                    {
                        var animData = Game.Animations.Core.GeneralAnimsList[anim];

                        Game.Animations.Core.Play(player, animData);

                        pData.ActualAnimation = animData;
                    }
                });

            AddDataHandler("DCR",
                (pData, value, oldValue) =>
                {
                    Customization.TattooData.ClearAll(pData.Player);

                    if (value is JArray jArr)
                        foreach (var x in jArr.ToObject<List<int>>())
                        {
                            var tattoo = Customization.GetTattooData(x);

                            tattoo.TryApply(pData.Player);
                        }
                });

            AddDataHandler("CHO",
                (pData, value, oldValue) =>
                {
                    Customization.HairOverlay.ClearAll(pData.Player);

                    if (value is int valueInt)
                        Customization.GetHairOverlay(pData.Sex, valueInt)?.Apply(pData.Player);
                });

            AddDataHandler("Belt::On",
                (pData, value, oldValue) =>
                {
                    if (pData.Player.Handle != Player.LocalPlayer.Handle)
                        return;

                    var state = (bool?)value ?? false;

                    var player = pData.Player;

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
                });

            AddDataHandler("PST",
                (pData, value, oldValue) =>
                {
                    var player = pData.Player;

                    var state = (Game.Scripts.Misc.Phone.PhoneStateTypes)((int?)value ?? 0);

                    Game.Scripts.Misc.Phone.SetState(player, state);
                });

            AddDataHandler("Crawl::On",
                (pData, value, oldValue) =>
                {
                    var player = pData.Player;

                    var state = (bool?)value ?? false;

                    if (player.Handle == Player.LocalPlayer.Handle)
                    {
                        if (state)
                            Crawl.On(true);
                        else
                            Crawl.Off(true);
                    }
                });

            AddDataHandler("Crouch::On",
                (pData, value, oldValue) =>
                {
                    var player = pData.Player;

                    var state = (bool?)value ?? false;

                    if (state)
                        Crouch.On(true, player);
                    else
                        Crouch.Off(true, player);
                });

            AddDataHandler("IsInvalid",
                (pData, value, oldValue) =>
                {
                    var player = pData.Player;
                    var state = (bool?)value ?? false;

                    if (player.Handle == Player.LocalPlayer.Handle)
                        Special.DisabledPerson = state;
                });

            AddDataHandler("IsInvisible",
                (pData, value, oldValue) =>
                {
                    var player = pData.Player;

                    var state = (bool?)value ?? false;

                    player.SetNoCollisionEntity(Player.LocalPlayer.Handle, !state);
                });

            AddDataHandler("Sex",
                (pData, value, oldValue) =>
                {
                    if (pData.Player.Handle != Player.LocalPlayer.Handle)
                        return;

                    var state = (bool)value;

                    Menu.SetSex(state);
                });

            AddDataHandler("Knocked",
                (pData, value, oldValue) =>
                {
                    var player = pData.Player;

                    var state = (bool?)value ?? false;

                    if (state)
                        player.SetCanRagdoll(false);
                    else
                        player.SetCanRagdoll(true);
                });

            AddDataHandler("AdminLevel",
                (pData, value, oldValue) =>
                {
                    if (pData.Player == Player.LocalPlayer)
                    {
                        var level = (int?)value ?? 0;

                        SetPlayerAsAdmin(level);
                    }
                });

            AddDataHandler("VoiceRange",
                (pData, value, oldValue) =>
                {
                    var player = pData.Player;

                    var vRange = (float)value;

                    if (player.Handle == Player.LocalPlayer.Handle)
                    {
                        // Voice Off
                        if (vRange > 0f)
                        {
                            Voice.Muted = false;

                            HUD.SwitchMicroIcon(true);

                            Main.Update -= Game.Management.Microphone.Core.OnTick;
                            Main.Update += Game.Management.Microphone.Core.OnTick;

                            Game.Management.Microphone.Core.StartUpdateListeners();

                            Game.Management.Microphone.Core.SetTalkingAnim(Player.LocalPlayer, true);
                        }
                        // Voice On / Muted
                        else if (vRange <= 0f)
                        {
                            Game.Management.Microphone.Core.StopUpdateListeners();

                            Voice.Muted = true;

                            HUD.SwitchMicroIcon(false);

                            Game.Management.Microphone.Core.SetTalkingAnim(Player.LocalPlayer, false);

                            Main.Update -= Game.Management.Microphone.Core.OnTick;

                            if (vRange < 0f)
                                HUD.SwitchMicroIcon(null);
                        }
                    }
                    else
                    {
                        if (vRange > 0f)
                            Game.Management.Microphone.Core.AddTalker(player);
                        else
                            Game.Management.Microphone.Core.RemoveTalker(player);
                    }
                });

            AddDataHandler("VehicleSeat",
                async (pData, value, oldValue) =>
                {
                    var player = pData.Player;

                    var seat = (int?)value ?? -1;

                    if (seat >= 0)
                    {
                        if (player.Vehicle?.Exists != true)
                            return;

                        player.SetIntoVehicle(player.Vehicle.Handle, seat - 1);

                        AsyncTask.Methods.Run(() => { UpdateHat(player); }, 250);

                        if (player.Handle == Player.LocalPlayer.Handle)
                        {
                            if (seat == 0 || seat == 1)
                            {
                                var veh = Player.LocalPlayer.Vehicle;

                                VehicleData vData = null;

                                do
                                {
                                    vData = VehicleData.GetData(veh);

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
                                while (veh?.Exists == true &&
                                       veh.GetPedInSeat(seat - 1, 0) == Player.LocalPlayer.Handle);
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
                });
        }

        public static void UpdateHat(Player player)
        {
            if (player == null)
                return;

            var pData = PlayerData.GetData(player);

            if (pData == null)
                return;

            var hData = pData.Hat?.Split('|');

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
                value = (int)Math.Round(0.5f * value); // 20 * a

                Stats.StatSetInt(Joaat.Hash("MP0_STAMINA"), value, true);
                Stats.StatSetInt(Joaat.Hash("MP0_STRENGTH"), value, true);
                Stats.StatSetInt(Joaat.Hash("MP0_LUNG_CAPACITY"), value, true);
            }
            else if (sType == SkillTypes.Shooting)
            {
                value = (int)Math.Round(0.25f * value); // 10 * a

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
                pData = PlayerData.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;
            }

            if (pData.Achievements?.ContainsKey(aType) == true)
            {
                Menu.UpdateAchievement(aType, value, maxValue);
            }
            else
            {
                var achievementName = Locale.Get(Language.Strings.GetKeyFromTypeByMemberName(aType.GetType(), aType.ToString(), "NAME_0") ?? "null");
                var achievementDesc = Locale.Get(Language.Strings.GetKeyFromTypeByMemberName(aType.GetType(), aType.ToString(), "DESC_0") ?? "null");

                Menu.AddAchievement(aType, value, maxValue, achievementName, achievementDesc);
            }
        }

        public static async void TryShowWeaponSkinsMenu()
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var wSkins = pData.WeaponSkins;

            if (wSkins.Count == 0)
            {
                Notification.ShowError(Locale.Notifications.Inventory.NoWeaponSkins);

                return;
            }

            await ActionBox.ShowSelect("WeaponSkinsMenuSelect",
                Locale.Actions.WeaponSkinsMenuSelectHeader,
                wSkins.Select(x => ((decimal)x.Key,
                           $"{Locale.Actions.WeaponSkinTypeNames.GetValueOrDefault(x.Key) ?? "null"} | {Game.Items.Core.GetName(x.Value).Split(' ')[0]}"))
                      .ToArray(),
                Locale.Actions.SelectOkBtn1,
                Locale.Actions.SelectCancelBtn1,
                ActionBox.DefaultBindAction,
                async (rType, id) =>
                {
                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    var wSkins = pData.WeaponSkins;

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
                });
        }

        public static void CloseAll(bool onlyInterfaces = false)
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            Ui.SetPauseMenuActive(false);

            ActionBox.Close();

            if (pData != null)
                Game.Scripts.Misc.Phone.CallChangeState(pData, Game.Scripts.Misc.Phone.PhoneStateTypes.Off);

            HUD.Menu.Switch(false);
            Inventory.Close();
            Game.UI.CEF.Interaction.CloseMenu();
            Menu.Close();
            UI.CEF.Animations.Close();
            Shop.Close(true);
            Gas.Close(true);

            Documents.Close();

            BlipsMenu.Close(true);
            ATM.Close();
            Bank.Close(true);

            Estate.Close(true);
            EstateAgency.Close(true);

            GarageMenu.Close();
            HouseMenu.Close(true);
            BusinessMenu.Close(true);

            FractionMenu.Close();
            PoliceTabletPC.Close();

            ShootingRange.Finish();

            CasinoMinigames.Close();

            NPC.CurrentNPC?.SwitchDialogue(false);

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
            var flyBindIdx = Player.LocalPlayer.GetData<int>("ADMIN::BINDS::FLY");

            if (flyBindIdx >= 0)
                Game.Input.Core.Unbind(flyBindIdx);

            if (aLvl <= 0)
            {
            }
            else
            {
                Player.LocalPlayer.SetData("ADMIN::BINDS::FLY",
                    Game.Input.Core.Bind(VirtualKeys.F5, true, () => Game.Management.Commands.Core.Fly()));
            }
        }

        private static float FlyF { get; set; } = 2f;
        private static float FlyW { get; set; } = 2f;
        private static float FlyH { get; set; } = 2f;

        private static void FlyRender()
        {
            var pos = Player.LocalPlayer.GetCoords(false);
            var dir = Geometry.RotationToDirection(Cam.GetGameplayCamRot(0));

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

            Player.LocalPlayer.SetCoordsNoOffset(pos.X, pos.Y, pos.Z, false, false, false);
        }
    }
}