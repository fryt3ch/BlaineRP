using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BCRPServer.Sync
{
    class Players : Script
    {
        [ServerEvent(Event.IncomingConnection)]
        private static void OnIncomingConnection(string ip, string serial, string rgscName, ulong rgscId, GameTypes gameType, CancelEventArgs cancel)
        {
            if (ServerEvents.IsRestarting)
                cancel.Cancel = true;
        }

        #region Player Connected
        [ServerEvent(Event.PlayerConnected)]
        private static async Task OnPlayerConnected(Player player)
        {
            if (player?.Exists != true || TempData.Get(player) != null)
                return;

            var scId = player.SocialClubId;
            var ip = player.Address;
            var hwid = player.Serial;

            var bans = await Task.Run(() => MySQL.GetGlobalBans(hwid, ip, scId));

            if (bans.Count > 0)
            {
                Utils.KickSilent(player, string.Join("\n", bans));

                return;
            }

            NAPI.Task.RunSafe(async () =>
            {
                if (player?.Exists != true)
                    return;

                var tData = new TempData(player);

                TempData.Set(player, tData);

                if (!await tData.WaitAsync())
                    return;

                await NAPI.Task.RunAsync(async () =>
                {
                    if (player?.Exists != true)
                        return;

                    player.SetData("Spam::Counter", 0);

                    player.SetTransparency(0);
                    player.Teleport(new Vector3(-749.78f, 5818.21f, 0), false, Utils.GetPrivateDimension(player));
                    player.Name = player.SocialClubName;

                    player.SkyCameraMove(Additional.SkyCamera.SwitchTypes.OutFromPlayer, true, "FadeScreen", false);

                    var account = await Task.Run(() => MySQL.GetPlayerAccount(scId));

                    await NAPI.Task.RunAsync(() =>
                    {
                        if (player?.Exists != true)
                            return;

                        player.SetAccountData(account);

                        if (account == null)
                        {
                            player.TriggerEvent("Auth::ShowRegistrationPage", player.SocialClubName);
                        }
                        else
                        {
                            account.Player = player;

                            tData.ActualToken = CEF.Auth.GenerateToken(account, hwid);

                            player.TriggerEvent("Auth::ShowLoginPage", player.SocialClubName);
                        }
                    });
                });

                tData.Release();
            });
        }
        #endregion

        #region Player Disconnected
        [ServerEvent(Event.PlayerDisconnected)]
        private static void OnPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            if (player?.Exists != true)
                return;

            var tData = TempData.Get(player);
            var data = player.GetMainData();
            var aData = player.GetAccountData();

            if (tData != null)
            {
                tData.Remove();

                if (aData != null)
                {
                    aData.Remove();
                }

                Task.Run(async () =>
                {
                    if (!await tData.WaitAsync())
                        return;

                    tData.Delete();
                    aData.Delete();
                });
            }
            else if (data != null)
            {
                if (aData != null)
                {
                    aData.Remove();
                }

                int cid = data.CID;
                int hp = player.Health;
                int mood = data.Mood;
                int satiety = data.Satiety;
                Vector3 pos = player.Position;
                uint dim = player.Dimension;
                float heading = player.Heading;
                bool knocked = data.Knocked;

                int arm = player.Armor;
                WeaponHash weapon = player.CurrentWeapon;
                int ammo = player.GetWeaponAmmo(weapon);

                player.DetachAllEntities();

                data.IsAttachedTo?.Entity?.DetachEntity(player);

                if (player.Vehicle != null)
                {
                    var vehData = player.Vehicle.GetMainData();

                    if (vehData != null)
                    {
                        vehData.RemovePassenger(data.VehicleSeat);
                    }
                }

                var items = data.Items;
                var ownedVehs = data.OwnedVehicles;

                var allVehs = NAPI.Pools.GetAllVehicles();
                var allPlayers = NAPI.Pools.GetAllPlayers();

                var keysVehs = items.Where(x => x is Game.Items.VehicleKey && !ownedVehs.Contains((x as Game.Items.VehicleKey).VID)).GroupBy(x => (x as Game.Items.VehicleKey).VID).Select(x => x.First() as Game.Items.VehicleKey).ToList();

                #region Check&Start Deletion of Owned Vehicles
                foreach (var vid in ownedVehs)
                {
                    var veh = allVehs.Where(x => x.GetMainData()?.VID == vid).FirstOrDefault();

                    if (veh == null)
                        continue;

                    var vData = veh.GetMainData();
                    var keys = vData.Keys;

                    bool foundDescendant = false;

                    foreach (var x in allPlayers)
                    {
                        var pItems = x.GetMainData()?.Items;

                        if (pItems == null)
                            continue;

                        for (int j = 0; j < keys.Count; j++)
                            if ((pItems[j] is Game.Items.VehicleKey) && pItems[j].UID == keys[j])
                            {
                                foundDescendant = true;

                                break;
                            }

                        if (foundDescendant)
                            break;
                    }

                    if (!foundDescendant)
                        vData.StartDeletionTask();
                }
                #endregion

                #region Check&Start Deletion of Vehicles By Keys
                foreach (var key in keysVehs)
                {
                    var foundDescendant = false;

                    var veh = allVehs.Where(x => x.GetMainData()?.VID == key.VID && x.GetMainData().Keys.Contains(key.UID)).FirstOrDefault();

                    if (veh == null)
                        continue;

                    var vData = veh.GetMainData();

                    var keys = vData.Keys;

                    foreach (var x in allPlayers)
                    {
                        var pItems = x.GetMainData()?.Items;

                        if (items == null)
                            continue;

                        for (int j = 0; j < keys.Count; j++)
                            if ((pItems[j] is Game.Items.VehicleKey) && pItems[j].UID == keys[j])
                            {
                                foundDescendant = true;

                                break;
                            }

                        if (foundDescendant)
                            break;
                    }

                    if (!foundDescendant)
                        vData.StartDeletionTask();
                }
                #endregion

                data.Remove();

                Task.Run(async () =>
                {
                    if (!await data.WaitAsync())
                        return;

                    if (data.Armour != null)
                    {
                        if (arm < 0 || arm > data.Armour.Strength)
                            arm = 0;

                        data.Armour.Strength = arm;

                        if (data.Armour.Strength == 0)
                            data.Armour.Delete();
                        else
                            data.Armour.Update();
                    }

                    var weapon = data.ActiveWeapon;

                    if (weapon != null)
                    {
                        if (ammo < 0 || ammo > weapon.Value.WeaponItem.Ammo)
                            ammo = 0;

                        weapon.Value.WeaponItem.Ammo = ammo;

                        weapon.Value.WeaponItem.Update();
                    }

                    MySQL.SaveCharacterOnExit(cid, data.TimePlayed, hp, dim, heading, pos, data.LastData.SessionTime, knocked, satiety, mood);

                    data.Delete();
                    aData.Delete();
                });
            }
        }
        #endregion

        [RemoteEvent("Players::ArmourBroken")]
        private static async Task ArmourBroken(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                var arm = pData.Armour;

                if (arm == null)
                    return;

                pData.Armour = null;

                await NAPI.Task.RunAsync(() =>
                {
                    if (player?.Exists != true)
                        return;

                    arm.Unwear(pData);

                    player.TriggerEvent("Inventory::Update", (int)CEF.Inventory.Groups.Armour, Game.Items.Item.ToClientJson(null, CEF.Inventory.Groups.Armour));
                });

                MySQL.UpdatePlayerInventory(pData, false, false, false, false, false, false, true);

                arm.Delete();
            });

            pData.Release();
        }

        [ServerEvent(Event.PlayerDeath)]
        private static async Task OnPlayerDeath(Player player, Player killer, uint reason)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(async () =>
            {
                if (player?.Exists != true)
                    return;

                player.CloseAll();

                if (pData.Knocked)
                {
                    player.SetHealth(10);

                    pData.Respawn(player.Position, player.Heading);

                    pData.Knocked = false;
                }
                else
                {
                    pData.Respawn(player.Position, player.Heading);

                    pData.Knocked = true;
                    pData.IsWounded = false;

                    player.SetHealth(50);

                    pData.PlayAnim(Sync.Animations.GeneralTypes.Knocked);

                    if (Settings.DROP_WEAPONS_AFTER_DEATH)
                    {
                        for (int i = 0; i < pData.Weapons.Length; i++)
                            if (pData.Weapons[i] != null)
                                await pData.InventoryDrop(CEF.Inventory.Groups.Weapons, i, 1);
                    }

                    if (pData.Holster?.Items[0] != null)
                        await pData.InventoryDrop(CEF.Inventory.Groups.Holster, 0, 1);

                    if (Settings.PERCENT_OF_AMMO_TO_DROP_AFTER_DEATH > 0f && Settings.MAX_AMMO_TO_DROP_AFTER_DEATH > 0)
                    {
                        int droppedAmmo = 0;

                        for (int i = 0; i < pData.Items.Length; i++)
                            if (pData.Items[i] is Game.Items.Ammo)
                            {
                                var ammoToDrop = (int)Math.Floor((pData.Items[i] as Game.Items.Ammo).Amount * Settings.PERCENT_OF_AMMO_TO_DROP_AFTER_DEATH);

                                if (ammoToDrop + droppedAmmo > Settings.MAX_AMMO_TO_DROP_AFTER_DEATH)
                                    ammoToDrop = Settings.MAX_AMMO_TO_DROP_AFTER_DEATH - droppedAmmo;

                                if (ammoToDrop == 0)
                                    break;

                                await pData.InventoryDrop(CEF.Inventory.Groups.Items, i, ammoToDrop);

                                droppedAmmo += ammoToDrop;

                                if (droppedAmmo == Settings.MAX_AMMO_TO_DROP_AFTER_DEATH)
                                    break;
                            }
                    }
                }
            });

            pData.Release();
        }

        [RemoteEvent("Players::CharacterReady")]
        public static async Task CharacterReady(Player player, bool isInvalid, int emotion, int walkstyle)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(() =>
            {
                if (player?.Exists != true)
                    return;

                if (player.HasData("CharacterReady"))
                    return;

                player.SetData("CharacterReady", true);

                pData.IsInvalid = isInvalid;
                pData.Emotion = (Sync.Animations.EmotionTypes)emotion;
                pData.Walkstyle = (Sync.Animations.WalkstyleTypes)walkstyle;

                player.Teleport(pData.LastData.Position, true, Utils.Dimensions.Main);

                pData.UpdateWeapons();
            });

            pData.Release();
        }

        #region Finger
        [RemoteEvent("Players::ToggleFingerPointSync")]
        public static async Task ToggleFingerPointing(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(() =>
            {
                if (player?.Exists != true)
                    return;

                pData.IsFingerPointing = !pData.IsFingerPointing;
            });

            pData.Release();
        }

        [RemoteEvent("fpsu")]
        public static void FingerUpdate(Player sender, float camPitch, float camHeading)
        {
            sender?.TriggerEventInDistance(Settings.FREQ_UPDATE_DISTANCE, "fpsu", sender.Handle, camPitch, camHeading);
        }

        [RemoteEvent("Players::FingerPoint::Vehicle")]
        public static async Task PointAtVehicle(Player player, Vehicle veh)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(() =>
            {
                var vData = veh.GetMainData();

                if (vData == null)
                    return;

                if (player?.Exists != true || veh?.Exists != true)
                    return;

                if (!player.AreEntitiesNearby(veh, 10f))
                    return;

                Chat.SendLocal(Chat.Type.Me, player, $"{Locale.Chat.Player.PointsAt} {vData.Data.Name} [{vData.Numberplate?.Tag ?? Locale.Chat.Vehicle.NoNumberplate}]", null);
            });

            pData.Release();
        }

        [RemoteEvent("Players::FingerPoint::Player")]
        public static async Task PointAtPlayer(Player player, Player target)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(() =>
            {
                if (player?.Exists != true || target?.Exists != true)
                    return;

                if (!player.AreEntitiesNearby(target, 10f))
                    return;

                Chat.SendLocal(Chat.Type.MePlayer, player, Locale.Chat.Player.PointsAt, target);
            });

            pData.Release();
        }

        [RemoteEvent("Players::FingerPoint::Ped")]
        public static async Task PointAtPed(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(() =>
            {
                if (player?.Exists != true)
                    return;

                Chat.SendLocal(Chat.Type.MePlayer, player, Locale.Chat.Player.PointsAtPerson, null);
            });

            pData.Release();
        }
        #endregion

        #region Crouch
        [RemoteEvent("Players::ToggleCrouchingSync")]
        public static async Task ToggleCrouching(Player player, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(() =>
            {
                if (player?.Exists != true)
                    return;

                if (pData.CrouchOn == state)
                    return;

                pData.CrouchOn = state;
            });

            pData.Release();
        }
        #endregion

        #region Crawl
        [RemoteEvent("Players::ToggleCrawlingSync")]
        public static async Task ToggleCrawling(Player player, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(() =>
            {
                if (player?.Exists != true)
                    return;

                if (pData.CrawlOn == state)
                    return;

                pData.CrawlOn = state;
            });

            pData.Release();
        }
        #endregion

        #region Push Vehicle
        [RemoteEvent("Players::StartPushingVehicleSync")]
        public static async Task StartPushingVehicle(Player player, Vehicle veh, bool isInFront)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var vehData = veh.GetMainData();

            if (!await pData.WaitAsync())
                return;

            if (!await vehData.WaitAsync())
            {
                pData.Release();

                return;
            }

            await NAPI.Task.RunAsync(() =>
            {
                if (player?.Exists != true)
                    return;

                if (veh?.Exists != true || veh.EngineStatus || !player.AreEntitiesNearby(veh, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                    return;

                if (pData.IsAttachedTo != null || vehData.ForcedSpeed != 0f)
                    return;

                veh.AttachEntity(player, isInFront ? AttachSystem.Types.PushVehicleFront : AttachSystem.Types.PushVehicleBack);
            });

            vehData.Release();

            pData.Release();
        }

        [RemoteEvent("Players::StopPushingVehicleSync")]
        public static async Task StopPushingVehicle(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            var attachData = pData.IsAttachedTo;

            if (attachData == null)
            {
                pData.Release();

                return;
            }

            var vehicle = attachData.Value.Entity;

            await NAPI.Task.RunAsync(() =>
            {
                if (player?.Exists != true)
                    return;

                if (vehicle?.Exists != true || vehicle.Type != EntityType.Vehicle)
                    return;

                if (attachData.Value.Type != AttachSystem.Types.PushVehicleFront && attachData.Value.Type != AttachSystem.Types.PushVehicleBack)
                    return;

                vehicle.DetachEntity(player);
            });

            pData.Release();
        }
        #endregion

        #region Belt
        [RemoteEvent("Players::ToggleBelt")]
        public static async Task ToggleBelt(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(() =>
            {
                if (player?.Exists != true)
                    return;

                Vehicle veh = player.Vehicle;

                bool isBeltOn = !pData.BeltOn;

                if (veh == null && isBeltOn)
                    return;

                if (veh != null && !Utils.IsCar(veh))
                    return;

                pData.BeltOn = isBeltOn;
                player.TriggerEvent("Players::ToggleBelt", isBeltOn);

                if (isBeltOn)
                {
                    player.SetClothes(5, 81, 0);

                    Chat.SendLocal(Chat.Type.Me, player, Locale.Chat.Vehicle.BeltOn);
                }
                else
                {
                    player.SetClothes(5, 0, 0);
                    pData.Bag?.Wear(pData);

                    Chat.SendLocal(Chat.Type.Me, player, Locale.Chat.Vehicle.BeltOff);
                }
            });

            pData.Release();
        }
        #endregion

        #region Cruise Control
        [RemoteEvent("Players::ToggleCruiseControl")]
        public static async Task ToggleCruiseControl(Player player, float speed)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var veh = player?.Vehicle;

            if (veh == null || !await pData.WaitAsync())
                return;

            var vData = veh.GetMainData();

            if (vData == null || !await vData.WaitAsync())
            {
                pData.Release();

                return;
            }

            await NAPI.Task.RunAsync(() =>
            {
                if (player?.Exists != true || veh?.Exists != true)
                    return;

                if (!Utils.IsCar(veh))
                    return;

                if (vData.ForcedSpeed >= Settings.MIN_CRUISE_CONTROL_SPEED)
                    vData.ForcedSpeed = 0f;
                else if (vData.EngineOn && pData.VehicleSeat == 0)
                    vData.ForcedSpeed = speed > Settings.MAX_CRUISE_CONTROL_SPEED ? Settings.MAX_CRUISE_CONTROL_SPEED : speed;
            });

            vData.Release();

            pData.Release();
        }
        #endregion

        #region Phone
        [RemoteEvent("Players::TogglePhone")]
        public static async Task StartUsePhone(Player player, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                var hadWeapon = await pData.UnequipActiveWeapon();

                await NAPI.Task.RunAsync(() =>
                {
                    if (player?.Exists != true)
                        return;

                    if (pData.PhoneOn == state)
                        return;

                    if (state)
                    {
                        if (hadWeapon)
                        {
                            NAPI.Task.Run(() =>
                            {
                                pData.PhoneOn = true;

                                player.AttachObject(Sync.AttachSystem.Models.Phone, AttachSystem.Types.Phone);

                                Chat.SendLocal(Chat.Type.Me, player, Locale.Chat.Player.PhoneOn);
                            }, 250);
                        }
                        else
                        {
                            pData.PhoneOn = true;

                            player.AttachObject(Sync.AttachSystem.Models.Phone, AttachSystem.Types.Phone);

                            Chat.SendLocal(Chat.Type.Me, player, Locale.Chat.Player.PhoneOn);
                        }
                    }
                    else
                    {
                        StopUsePhone(player);
                    }
                });
            });

            pData.Release();
        }

        public static void StopUsePhone(Player player)
        {
            var data = player.GetMainData();

            if (data == null)
                return;

            if (!data.PhoneOn)
                return;

            data.PhoneOn = false;

            var attachedPhone = data.AttachedObjects.Where(x => x.Type == AttachSystem.Types.Phone).FirstOrDefault();

            if (attachedPhone != null)
                player.DetachObject(attachedPhone.Id);

            Chat.SendLocal(Chat.Type.Me, player, Locale.Chat.Player.PhoneOff);
        }
        #endregion

        /// <summary>Получен урон от игрока</summary>
        [RemoteEvent("Players::GotDamage")]
        private static async Task GotDamage(Player player, int damage, bool isGun)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            pData.LastDamageTime = Utils.GetCurrentTime();

            await NAPI.Task.RunAsync(() =>
            {
                if (player?.Exists != true)
                    return;

                if (isGun && !pData.IsWounded && !pData.Knocked && (new Random()).NextDouble() <= Settings.WOUND_CHANCE)
                {
                    pData.IsWounded = true;
                }
            });

            pData.Release();
        }

        [RemoteEvent("Players::UpdateTime")]
        private static async Task UpdateTime(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                var currentTime = Utils.GetCurrentTime();

                if (currentTime.Subtract(pData.LastJoinDate).TotalSeconds < 60)
                    return;

                pData.LastJoinDate = currentTime;

                pData.TimePlayed += 1;
                pData.LastData.SessionTime += 60;

                await NAPI.Task.RunAsync(() =>
                {
                    if (player?.Exists != true)
                        return;

                    if (pData.TimePlayed % 120 == 0)
                    {
                        if (pData.Satiety > 0)
                            pData.Satiety--;

                        if (pData.Mood > 0)
                            pData.Mood--;
                    }
                });
            });

            pData.Release();
        }

        [RemoteEvent("Players::SetIsInvalid")]
        private static async Task SetIsInvalid(Player player, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(() =>
            {
                if (player?.Exists != true)
                    return;

                pData.IsInvalid = state;
            });

            pData.Release();
        }

        [RemoteEvent("Business::Info")]
        public static async Task BusinessInfo(Player player, int id)
        {

        }

        [RemoteEvent("Business::Enter")]
        public static async Task BusinessEnter(Player player, int id)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                if (pData.CurrentBusiness != null)
                    return;

                var business = Game.Businesses.Business.Get(id);

                if (business == null)
                    return;

                await pData.UnequipActiveWeapon();

                await NAPI.Task.RunAsync(() =>
                {
                    if (player?.Exists != true)
                        return;

                    if (player.Dimension != Utils.Dimensions.Main || Vector3.Distance(player.Position, business.Position) > 50f)
                        return;

                    pData.CurrentBusiness = id;

                    player.CloseAll();

                    Sync.Microphone.DisableMicrophone(pData);

                    if (business.ViewParams != null)
                    {
                        pData.IsInvincible = true;

                        NAPI.Task.Run(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.Heading = business.ViewParams.Value.Heading;

                            player.Teleport(business.ViewParams.Value.Position, false, Utils.GetPrivateDimension(player));

                        }, 1000);

                        player.TriggerEvent("Shop::Show", (int)business.Type, business.Margin, business.ViewParams.Value.Heading);
                    }
                    else
                        player.TriggerEvent("Shop::Show", (int)business.Type, business.Margin);
                });
            });

            pData.Release();
        }

        [RemoteEvent("Business::Exit")]
        public static async Task BusinessExit(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                if (pData.CurrentBusiness == null)
                    return;

                var business = Game.Businesses.Business.Get((int)pData.CurrentBusiness);

                if (business == null)
                    return;

                pData.CurrentBusiness = null;

                NAPI.Task.Run(() =>
                {
                    if (player?.Exists != true)
                        return;

                    if (business.ViewParams != null)
                    {
                        pData.IsInvincible = false;

                        NAPI.Task.Run(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.Teleport(business.Position, true, Utils.Dimensions.Main);
                        }, 1000);
                    }

                    player.TriggerEvent("Shop::Close::Server");
                });
            });

            pData.Release();
        }

        [RemoteEvent("Shop::Buy")]
        public static async Task ShopBuy(Player player, string id, int variation, int amount, bool useCash)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                if (pData.CurrentBusiness == null || amount <= 0)
                    return;

                var business = Game.Businesses.Business.Get((int)pData.CurrentBusiness);

                if (business == null)
                    return;

                int price = business.GetItemPrice(id, true);

                if (price == -1)
                    return;

                price *= amount;

                bool paid = await Task.Run(async () =>
                {
                    if (business.Owner != -1)
                    {
                        // operations with materials
                    }

                    if (useCash)
                        return await pData.AddCash(-price, true);
                    else
                        return false;

                });

                if (!paid)
                    return;

                var item = await Game.Items.Items.GiveItem(pData, id, variation, amount, false);

                if (item == null)
                    pData.Gifts.Add(await Game.Items.Gift.Give(pData, Game.Items.Gift.Types.Item, id, variation, amount, Game.Items.Gift.SourceTypes.Shop, true, true));
            });

            pData.Release();
        }

        [RemoteEvent("House::Enter")]
        public static async Task HouseEnter(Player player, int id)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                if (pData.CurrentHouse != null)
                    return;

                var house = Game.Houses.House.Get(id);

                if (house == null)
                    return;

                NAPI.Task.Run(() =>
                {
                    if (player?.Exists != true)
                        return;

                    if (player.Dimension != Utils.Dimensions.Main || Vector3.Distance(player.Position, house.GlobalPosition) > Settings.ENTITY_INTERACTION_MAX_DISTANCE)
                        return;

                    pData.CurrentHouse = id;

                    player.CloseAll();

                    var sData = house.StyleData;

                    NAPI.Task.Run(() =>
                    {
                        if (player?.Exists != true)
                            return;

                        player.Heading = sData.Heading;
                        player.Teleport(sData.Position, false, house.Dimension);
                    }, 1000);

                    player.TriggerEvent("House::Enter", id, sData.Type, NAPI.Util.ToJson(house.Dimension), NAPI.Util.ToJson((house.DoorsStates, house.LightsStates)));
                });
            });

            pData.Release();
        }

        [RemoteEvent("House::Exit")]
        public static async Task HouseExit(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                if (pData.CurrentHouse == null)
                    return;

                var house = Game.Houses.House.Get((int)pData.CurrentHouse);

                if (house == null)
                    return;

                NAPI.Task.Run(() =>
                {
                    if (player?.Exists != true)
                        return;

                    if (player.Dimension != house.Dimension)
                        return;

                    pData.CurrentHouse = null;

                    //player.CloseAll();

                    player.TriggerEvent("House::Exit");

                    NAPI.Task.Run(() =>
                    {
                        if (player?.Exists != true)
                            return;

                        player.Heading = house.ExitHeading;
                        player.Teleport(house.GlobalPosition, false, Utils.Dimensions.Main);
                    }, 1000);
                });
            });

            pData.Release();
        }

        [RemoteEvent("Players::PlayAnim")]
        public static async Task PlayAnim(Player player, bool fast, int anim)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                if (!Enum.IsDefined(typeof(Sync.Animations.FastTypes), anim))
                    return;

                await NAPI.Task.RunAsync(() =>
                {
                    if (player?.Exists != true)
                        return;

                    pData.PlayAnim((Sync.Animations.FastTypes)anim);
                });
            });

            pData.Release();
        }

        [RemoteEvent("Players::SetWalkstyle")]
        public static async Task SetWalkstyle(Player player, int walkstyle)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                if (!Enum.IsDefined(typeof(Sync.Animations.WalkstyleTypes), walkstyle))
                    return;

                await NAPI.Task.RunAsync(() =>
                {
                    pData.SetWalkstyle((Animations.WalkstyleTypes)walkstyle);
                });
            });

            pData.Release();
        }

        [RemoteEvent("Players::SetEmotion")]
        public static async Task SetEmotion(Player player, int emotion)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                if (!Enum.IsDefined(typeof(Sync.Animations.EmotionTypes), emotion))
                    return;

                await NAPI.Task.RunAsync(() =>
                {
                    pData.SetEmotion((Animations.EmotionTypes)emotion);
                });
            });

            pData.Release();
        }

        [RemoteEvent("Players::SetAnim")]
        public static async Task SetAnim(Player player, int anim)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                if (!Enum.IsDefined(typeof(Sync.Animations.OtherTypes), anim))
                    return;

                await NAPI.Task.RunAsync(() =>
                {
                    pData.PlayAnim((Animations.OtherTypes)anim);
                });
            });

            pData.Release();
        }

        [RemoteEvent("Players::StopCarry")]
        public static async Task StopCarry(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(() =>
            {
                if (player?.Exists != true)
                    return;

                var attachData = pData.IsAttachedTo;

                if (attachData == null)
                {
                    var aData = pData.AttachedEntities.Where(x => x.Type == AttachSystem.Types.Carry).FirstOrDefault();

                    if (aData == null || aData.EntityType != EntityType.Player)
                        return;

                    var target = Utils.FindReadyPlayerOnline(aData.Id);

                    if (target == null || target.Player?.Exists != true)
                        return;

                    player.DetachEntity(target.Player);
                }
                else
                {
                    if (attachData.Value.Entity?.Exists != true)
                        return;

                    attachData.Value.Entity.DetachEntity(player);
                }
            });

            pData.Release();
        }

        [RemoteEvent("Players::GoToTrunk")]
        public static async Task GoToTrunk(Player player, Vehicle vehicle)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                var attachData = pData.IsAttachedTo;

                if (attachData != null)
                    return;

                var vData = vehicle.GetMainData();

                if (!await vData.WaitAsync())
                    return;

                await NAPI.Task.RunAsync(() =>
                {
                    if (player?.Exists != true || vehicle?.Exists != true)
                        return;

                    vehicle.AttachEntity(player, AttachSystem.Types.VehicleTrunk);
                });

                vData.Release();
            });

            pData.Release();
        }

        [RemoteEvent("Players::StopInTrunk")]
        public static async Task StopInTrunk(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(() =>
            {
                if (player?.Exists != true)
                    return;

                var attachData = pData.IsAttachedTo;

                if (attachData == null || attachData.Value.Type != AttachSystem.Types.VehicleTrunk || attachData.Value.Entity?.Exists != true)
                    return;

                attachData.Value.Entity.DetachEntity(player);
            });

            pData.Release();
        }

        [RemoteEvent("Players::Smoke::Stop")]
        public static async Task StopSmoke(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(() =>
            {
                if (player?.Exists != true)
                    return;

                var attachData = pData.AttachedObjects.Where(x => x.Type == AttachSystem.Types.ItemCigHand || x.Type == AttachSystem.Types.ItemCigMouth).FirstOrDefault();

                if (attachData == null)
                    return;

                player.DetachObject(attachData.Id);

                pData.StopAnim();
            });

            pData.Release();
        }

        [RemoteEvent("Players::Smoke::Puff")]
        public static async Task SmokeDoPuff(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await NAPI.Task.RunAsync(() =>
            {
                if (player?.Exists != true)
                    return;

                var attachData = pData.AttachedObjects.Where(x => x.Type == AttachSystem.Types.ItemCigHand || x.Type == AttachSystem.Types.ItemCigMouth).FirstOrDefault();

                if (attachData == null)
                    return;

                pData.PlayAnim(Animations.FastTypes.SmokePuffCig);

                player.TriggerEvent("Player::Smoke::Puff");
            });

            pData.Release();
        }

        [RemoteEvent("Players::Smoke::State")]
        public static async Task SmokeSetState(Player player, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            var attachData = await NAPI.Task.RunAsync(() =>
            {
                if (player?.Exists != true)
                    return null;

                var attachData = pData.AttachedObjects.Where(x => x.Type == AttachSystem.Types.ItemCigHand || x.Type == AttachSystem.Types.ItemCigMouth).FirstOrDefault();

                if (attachData == null)
                    return null;

                pData.PlayAnim(Animations.FastTypes.SmokeTransitionCig);

                return attachData;
            });

            if (attachData == null)
            {
                pData.Release();

                return;
            }

            if (attachData.Type == AttachSystem.Types.ItemCigHand)
            {
                await NAPI.Task.RunAsync(() =>
                {
                    if (player?.Exists != true)
                        return;

                    player.DetachObject(attachData.Id, false);
                    player.AttachObject(attachData.Model, AttachSystem.Types.ItemCigMouth);
                }, 500);
            }
            else
            {
                await NAPI.Task.RunAsync(() =>
                {
                    if (player?.Exists != true)
                        return;

                    player.DetachObject(attachData.Id, false);
                    player.AttachObject(attachData.Model, AttachSystem.Types.ItemCigHand);
                }, 500);
            }

            pData.Release();
        }
    }
}
