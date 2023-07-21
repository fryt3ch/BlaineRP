using BCRPServer.Sync;
using GTANetworkAPI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BCRPServer.Events.Players
{
    class Main : Script
    {
        [ServerEvent(Event.PlayerWeaponSwitch)]
        private static void OnPlayerWeaponSwitch(Player player, uint oldWeapon, uint newWeapon)
        {
            if (oldWeapon == 2725352035 && newWeapon == oldWeapon)
                return;

            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.WeaponComponents != null)
                player.TriggerEventToStreamed("Players::WCD::U", player);
        }

        [ServerEvent(Event.IncomingConnection)]
        private static void OnIncomingConnection(string ip, string serial, string rgscName, ulong rgscId, GameTypes gameType, CancelEventArgs cancel)
        {
            if (Events.Server.IsRestarting)
                cancel.Cancel = true;
        }

        [ServerEvent(Event.PlayerConnected)]
        private static async Task OnPlayerConnected(Player player)
        {
            if (player?.Exists != true || player.GetTempData() != null || player.GetMainData() != null)
                return;

            var scid = player.SocialClubId;
            var ip = player.Address;
            var hwid = player.Serial;

            AccountData.GlobalBan globalBan;

            using (var cts = new CancellationTokenSource(2_500))
            {
                try
                {
                    globalBan = await Web.SocketIO.Methods.Misc.GetPlayerGlobalBan(cts.Token, hwid, scid);
                }
                catch (Exception ex)
                {
                    NAPI.Task.Run(() =>
                    {
                        if (player?.Exists != true)
                            return;
                    });

                    return;
                }
            }

            NAPI.Task.Run(async () =>
            {
                if (player?.Exists != true)
                    return;

                if (globalBan != null)
                {
                    Utils.Kick(player, "todo");

                    return;
                }

                var tData = new TempData(player);

                player.SetTempData(tData);

                player.SetAlpha(0);
                player.Teleport(new Vector3(-749.78f, 5818.21f, 0), false, Utils.GetPrivateDimension(player));
                player.Name = player.SocialClubName;

                player.SkyCameraMove(Additional.SkyCamera.SwitchTypes.OutFromPlayer, true, "FadeScreen", false);

                uint aid;

                using (var cts = new CancellationTokenSource(2_500))
                {
                    try
                    {
                        aid = await Web.SocketIO.Methods.Account.GetIdBySCID(cts.Token, scid);
                    }
                    catch (Exception ex)
                    {
                        NAPI.Task.Run(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            //tData.BlockRemoteCalls = false;
                        });

                        return;
                    }
                }

                NAPI.Task.Run(() =>
                {
                    if (player?.Exists != true)
                        return;

                    tData.BlockRemoteCalls = false;

                    if (aid == 0)
                    {
                        tData.StepType = TempData.StepTypes.AuthRegistration;

                        player.TriggerEvent("Auth::Start::Show", JObject.FromObject(new { Type = 1, SCName = player.SocialClubName, }));
                    }
                    else
                    {
                        tData.StepType = TempData.StepTypes.AuthLogin;

                        player.TriggerEvent("Auth::Start::Show", JObject.FromObject(new { Type = 0, SCName = player.SocialClubName, }));
                    }
                });
            });
        }

        [ServerEvent(Event.PlayerDisconnected)]
        private static void OnPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            if (player?.Exists != true)
                return;

            var tData = player.GetTempData();

            if (tData != null)
            {
                if (tData.PlayerData != null)
                {
                    tData.PlayerData.Remove();
                }

                tData.Delete();
            }
            else
            {
                var pData = player.GetMainData();

                if (pData == null)
                    return;

                pData.StopUpdateTimer();

                if (pData.CurrentBusiness != null)
                    Sync.Players.ExitFromBuiness(pData, false);

                pData.ActiveOffer?.Cancel(false, true, Sync.Offers.ReplyTypes.AutoCancel, false);

                pData.ActiveCall?.Cancel(Sync.Phone.Call.CancelTypes.ServerAuto);

                var policeCall = Game.Fractions.Police.GetCallByCaller(player.Id);

                if (policeCall != null)
                    Game.Fractions.Police.RemoveCall(player.Id, policeCall, 0, null);

                Sync.Report.GetByStarterPlayer(pData.Info)?.Close(pData);

                var currentTaxiOrder = Game.Jobs.Cabbie.ActiveOrders.Where(x => x.Value.Entity == player).FirstOrDefault();

                if (currentTaxiOrder.Value != null)
                    Game.Jobs.Cabbie.RemoveOrder(currentTaxiOrder.Key, currentTaxiOrder.Value, false);

                if (pData.CurrentJob is Game.Jobs.Job curJob)
                    curJob.OnWorkerExit(pData);

                int rentedMarketStallIdx;

                var rentedMarketStall = Game.Misc.MarketStall.GetByRenter(player.Id, out rentedMarketStallIdx);

                if (rentedMarketStall != null)
                {
                    rentedMarketStall.SetCurrentRenter(rentedMarketStallIdx, null);
                }

                var attachedObjects = pData.AttachedObjects;

                var cuffsAttachment = attachedObjects.Where(x => x.Type == AttachSystem.Types.Cuffs).FirstOrDefault();

                if (cuffsAttachment != null)
                {
                    pData.Info.GetTempData<Timer>("CuffedQuitTimer")?.Dispose();

                    pData.Info.SetTempData("CuffedQuitTimer", new Timer((obj) =>
                    {
                        NAPI.Task.Run(() =>
                        {
                            var activePunishment = pData.Info.Punishments.Where(x => x.Type == Punishment.Types.Arrest || x.Type == Punishment.Types.FederalPrison || x.Type == Punishment.Types.NRPPrison).FirstOrDefault();

                            if (activePunishment != null)
                                return;


                        });
                    }, null, 300_000, Timeout.Infinite));
                }

                player.DetachAllObjects();

                player.DetachAllEntities();

                pData.IsAttachedToEntity?.DetachEntity(player);

                if (player.Vehicle is Vehicle veh)
                {
                    var vData = VehicleData.GetData(veh);

                    if (vData != null)
                        Sync.Vehicles.OnPlayerLeaveVehicle(pData, vData);
                }

                if (pData.Info.Quests.GetValueOrDefault(Quest.QuestData.Types.DRSCHOOL0) is Sync.Quest driveSchoolQuest && driveSchoolQuest.Step > 0)
                    driveSchoolQuest.Cancel(pData.Info);

                #region Check&Start Deletion of Owned Vehicles

                var vehsToStartDeletion = pData.OwnedVehicles.Where(x => x.VehicleData != null).ToList();

                for (int i = 0; i < pData.Items.Length; i++)
                {
                    if (pData.Items[i] is Game.Items.VehicleKey vKey)
                    {
                        var vInfo = vKey.VehicleInfo;

                        if (vInfo?.VehicleData == null)
                            continue;

                        if (vehsToStartDeletion.Contains(vInfo))
                            continue;

                        if (vKey.IsKeyValid(vInfo))
                            vehsToStartDeletion.Add(vInfo);
                    }
                }

                foreach (var x in PlayerData.All.Values)
                {
                    if (x == pData)
                        continue;

                    for (int i = 0; i < x.Items.Length; i++)
                    {
                        if (x.Items[i] is Game.Items.VehicleKey vKey)
                        {
                            var vInfo = vehsToStartDeletion.Where(x => x.VID == vKey.VID).FirstOrDefault();

                            if (vKey.IsKeyValid(vInfo))
                                vehsToStartDeletion.Remove(vInfo);
                        }
                    }
                }

                vehsToStartDeletion.ForEach(x => x.VehicleData.StartDeletionTask(Properties.Settings.Static.OWNED_VEHICLE_TIME_TO_AUTODELETE));
                #endregion

                if (pData.Armour != null)
                {
                    var arm = player.Armor;

                    if (arm < 0)
                        arm = 0;

                    if (arm < pData.Armour.Strength)
                    {
                        pData.Armour.Strength = arm;

                        if (pData.Armour.Strength == 0)
                        {
                            pData.Armour.Delete();

                            pData.Armour = null;
                        }
                        else
                            pData.Armour.Update();
                    }
                }

                Game.Items.Weapon activeWeapon;

                if (pData.TryGetActiveWeapon(out activeWeapon, out _, out _))
                    activeWeapon.Unequip(pData, false);

                for (int i = 0; i < pData.Items.Length; i++)
                {
                    var item = pData.Items[i];

                    if (item is Game.Items.IUsable ciiu)
                        if (ciiu.InUse)
                            ciiu.InUse = false;
                }

                foreach (var x in pData.Weapons)
                {
                    if (x == null)
                        continue;

                    if (x.AttachType != null)
                        x.AttachType = null;
                }

                pData.Info.LastData.Health = player.Health;

                if (pData.Info.LastData.Health < 0 || pData.IsKnocked)
                    pData.Info.LastData.Health = 0;

                pData.Info.LastData.UpdatePosition(new Utils.Vector4(player.Position, player.Heading), player.Dimension, false);

                MySQL.CharacterSaveOnExit(pData.Info);

                pData.Remove();
            }
        }

        [RemoteEvent("Players::ArmourBroken")]
        private static void ArmourBroken(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var arm = pData.Armour;

            if (arm == null)
                return;

            pData.Armour = null;

            arm.Unwear(pData);

            player.InventoryUpdate(Game.Items.Inventory.GroupTypes.Armour, Game.Items.Item.ToClientJson(null, Game.Items.Inventory.GroupTypes.Armour));

            MySQL.CharacterArmourUpdate(pData.Info);

            arm.Delete();
        }

        [RemoteEvent("Players::OnDeath")]
        private static void OnPlayerDeath(Player player, Player attacker)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            pData.StopAllAnims();

            player.DetachAllObjectsInHand();

            pData.StopUseCurrentItem();

            foreach (var x in pData.Punishments)
            {
                if (x.Type == Punishment.Types.NRPPrison)
                {
                    if (!x.IsActive())
                        continue;

                    if (pData.IsKnocked)
                    {
                        pData.SetAsNotKnocked();
                    }

                    var pos = Utils.Demorgan.GetNextPos();

                    player.Teleport(pos, false, null, null, false);

                    NAPI.Player.SpawnPlayer(player, pos, player.Heading);

                    player.SetHealth(50);

                    return;
                }
                else if (x.Type == Punishment.Types.Arrest)
                {
                    if (!x.IsActive())
                        continue;

                    if (pData.IsKnocked)
                    {
                        pData.SetAsNotKnocked();
                    }

                    var fData = Game.Fractions.Fraction.Get((Game.Fractions.Types)int.Parse(x.AdditionalData.Split('_')[1])) as Game.Fractions.Police;

                    if (fData != null)
                    {
                        var pos = fData.GetNextArrestCellPosition();

                        player.Teleport(pos, false, null, null, false);

                        NAPI.Player.SpawnPlayer(player, pos, player.Heading);

                        player.SetHealth(50);
                    }

                    return;
                }
                else if (x.Type == Punishment.Types.FederalPrison)
                {
                    if (!x.IsActive())
                        continue;

                    if (pData.IsKnocked)
                    {
                        pData.SetAsNotKnocked();
                    }

                    return;
                }
            }

            var pDim = player.Dimension;

/*            if (pDim == Utils.GetPrivateDimension(player))
            {
                Game.Fractions.EMS.SetPlayerToEmsAfterDeath(pData, pData.LastData.Position.Position);

                return;
            }*/

            if (pData.IsKnocked)
            {
                var pos = player.Position;

                if (pDim >= Properties.Settings.Profile.Current.Game.HouseDimensionBaseOffset)
                {
                    if (pDim < Properties.Settings.Profile.Current.Game.ApartmentsDimensionBaseOffset)
                    {
                        var house = Utils.GetHouseBaseByDimension(pDim) as Game.Estates.House;

                        if (house != null)
                            pos = house.PositionParams.Position;
                    }
                    else if (pDim < Properties.Settings.Profile.Current.Game.ApartmentsRootDimensionBaseOffset)
                    {
                        var aps = Utils.GetHouseBaseByDimension(pDim) as Game.Estates.Apartments;

                        if (aps != null)
                            pos = aps.Root.EnterParams.Position;
                    }
                    else if (pDim < Properties.Settings.Profile.Current.Game.GarageDimensionBaseOffset)
                    {
                        var apsRoot = Utils.GetApartmentsRootByDimension(pDim);

                        if (apsRoot != null)
                            pos = apsRoot.EnterParams.Position;
                    }
                    else
                    {
                        var garage = Utils.GetGarageByDimension(pDim);

                        if (garage != null)
                            pos = garage.Root.EnterPosition.Position;
                    }
                }

                Game.Fractions.EMS.SetPlayerToEmsAfterDeath(pData, pos);
            }
            else
            {
                player.Teleport(null, false, null, null, false);

                pData.ActiveCall?.Cancel(Sync.Phone.Call.CancelTypes.ServerAuto);

                NAPI.Player.SpawnPlayer(player, player.Position, player.Heading);

                pData.SetAsKnocked(attacker);

                player.SetHealth(50);

                if (Properties.Settings.Profile.Current.Game.KnockedDropWeaponsEnabled)
                {
                    for (int i = 0; i < pData.Weapons.Length; i++)
                        if (pData.Weapons[i] != null)
                            pData.InventoryDrop(Game.Items.Inventory.GroupTypes.Weapons, i, 1);
                }

                if (pData.Holster?.Items[0] != null)
                    pData.InventoryDrop(Game.Items.Inventory.GroupTypes.Holster, 0, 1);

                if (Properties.Settings.Profile.Current.Game.KnockedDropAmmoTotalPercentage > 0f && Properties.Settings.Profile.Current.Game.KnockedDropAmmoMaxAmount > 0)
                {
                    int droppedAmmo = 0;

                    for (int i = 0; i < pData.Items.Length; i++)
                        if (pData.Items[i] is Game.Items.Ammo)
                        {
                            var ammoToDrop = (int)Math.Floor((pData.Items[i] as Game.Items.Ammo).Amount * Properties.Settings.Profile.Current.Game.KnockedDropAmmoTotalPercentage);

                            if (ammoToDrop + droppedAmmo > Properties.Settings.Profile.Current.Game.KnockedDropAmmoMaxAmount)
                                ammoToDrop = Properties.Settings.Profile.Current.Game.KnockedDropAmmoMaxAmount - droppedAmmo;

                            if (ammoToDrop == 0)
                                break;

                            pData.InventoryDrop(Game.Items.Inventory.GroupTypes.Items, i, ammoToDrop);

                            droppedAmmo += ammoToDrop;

                            if (droppedAmmo == Properties.Settings.Profile.Current.Game.KnockedDropAmmoMaxAmount)
                                break;
                        }
                }
            }
        }

        [RemoteProc("Players::CRI")]
        public static bool CharacterReadyIndicate(Player player, bool isInvalid, int emotion, int walkstyle)
        {
            var sRes = player.CheckSpamAttack(5000, false);

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (!player.ResetData("CharacterNotReady"))
                return false;

            pData.ResetUpdateTimer();

            if (!Enum.IsDefined(typeof(Sync.Animations.EmotionTypes), emotion))
                emotion = -1;

            if (!Enum.IsDefined(typeof(Sync.Animations.WalkstyleTypes), walkstyle))
                walkstyle = -1;

            pData.IsInvalid = isInvalid;
            pData.Emotion = (Sync.Animations.EmotionTypes)emotion;
            pData.Walkstyle = (Sync.Animations.WalkstyleTypes)walkstyle;

            pData.UpdateWeapons();

            if (pData.RentedJobVehicle is VehicleData jobVehicle)
            {
                jobVehicle.Job?.SetPlayerJob(pData, jobVehicle);
            }

            if (pData.Fraction != Game.Fractions.Types.None)
            {
                var fData = Game.Fractions.Fraction.Get(pData.Fraction);

                fData?.OnMemberJoined(pData);
            }

            //var ped = new PedData((uint)PedHash.Hooker03SFY, new Utils.Vector4(player.Position, player.Heading), player.Dimension, null);

            //ped.IsInvincible = true;

            //ped.AttachObject(Sync.AttachSystem.Models.Cuffs, AttachSystem.Types.Cuffs, -1, null);

            //player.AttachEntity(ped.Ped, AttachSystem.Types.PoliceEscort);

            //player.AttachEntity(ped, AttachSystem.Types.Carry);

            return true;
        }

        #region Finger
        [RemoteEvent("fpsu")]
        public static void FingerUpdate(Player sender, float camPitch, float camHeading)
        {
            sender?.TriggerEventInDistance(Properties.Settings.Static.FREQ_UPDATE_DISTANCE, "fpsu", sender.Handle, camPitch, camHeading);
        }

        [RemoteEvent("Players::FingerPoint::Vehicle")]
        public static void PointAtVehicle(Player player, Vehicle veh)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (!player.IsNearToEntity(veh, 10f))
                return;

            Sync.Chat.SendLocal(Sync.Chat.MessageTypes.Me, player, Language.Strings.Get("CHAT_PLAYER_FP_0", vData.GetName(1)), null);
        }

        [RemoteEvent("Players::FingerPoint::Player")]
        public static void PointAtPlayer(Player player, Player target)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (target?.Exists != true)
                return;

            if (!player.IsNearToEntity(target, 10f))
                return;

            Sync.Chat.SendLocal(Sync.Chat.MessageTypes.Me, player, Language.Strings.Get("CHAT_PLAYER_FP_0"), target);
        }

        [RemoteEvent("Players::FingerPoint::Ped")]
        public static void PointAtPed(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            Sync.Chat.SendLocal(Sync.Chat.MessageTypes.Me, player, Language.Strings.Get("CHAT_PLAYER_FP_1", null));
        }
        #endregion

        #region Crouch
        [RemoteEvent("Players::ToggleCrouchingSync")]
        public static void ToggleCrouching(Player player, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.CrouchOn == state)
                return;

            pData.CrouchOn = state;
        }
        #endregion

        #region Crawl
        [RemoteEvent("Players::ToggleCrawlingSync")]
        public static void ToggleCrawling(Player player, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.CrawlOn == state)
                return;

            if (state)
            {
                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen || pData.IsAnyAnimOn() || pData.HasAnyHandAttachedObject)
                    return;
            }

            pData.CrawlOn = state;
        }
        #endregion

        #region Push Vehicle
        [RemoteEvent("Players::StartPushingVehicleSync")]
        public static void StartPushingVehicle(Player player, Vehicle veh, bool isInFront)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen || pData.IsAttachedToEntity != null || pData.IsAnyAnimOn() || pData.HasAnyHandAttachedObject)
                return;

            if (player.Vehicle != null)
                return;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (vData.EngineOn || !player.IsNearToEntity(veh, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                return;

            if (vData.ForcedSpeed != 0f)
                return;

            if (isInFront)
                veh.AttachEntity(player, AttachSystem.Types.PushVehicle, "1");
            else
                veh.AttachEntity(player, AttachSystem.Types.PushVehicle, "0");
        }

        [RemoteEvent("Players::StopPushingVehicleSync")]
        public static void StopPushingVehicle(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var atVeh = pData.IsAttachedToEntity as Vehicle;

            if (atVeh?.Exists != true)
                return;

            var attachData = atVeh.GetAttachmentData(player);

            if (attachData == null)
                return;

            if (attachData.Type != AttachSystem.Types.PushVehicle)
                return;

            atVeh.DetachEntity(player);
        }
        #endregion

        #region Belt
        [RemoteEvent("Players::ToggleBelt")]
        public static void ToggleBelt(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var veh = player.Vehicle;

            var isBeltOn = !pData.BeltOn;

            if (veh == null)
            {
                if (isBeltOn)
                    return;
            }
            else
            {
                var vData = veh.GetMainData();

                if (vData == null)
                    return;

                if (vData.Data.Type != Game.Data.Vehicles.Vehicle.Types.Car)
                    return;
            }


            pData.BeltOn = isBeltOn;

            if (isBeltOn)
            {
                //player.SetClothes(5, 81, 0);

                Sync.Chat.SendLocal(Sync.Chat.MessageTypes.Me, player, Language.Strings.Get("CHAT_VEHICLE_BELT_ON"));
            }
            else
            {
/*                if (pData.Items.Where(x => (x as Game.Items.Parachute)?.InUse == true).Any())
                {
                    Game.Items.Parachute.Wear(pData);
                }
                else
                {
                    if (pData.Bag != null)
                        pData.Bag.Wear(pData);
                    else
                        player.SetClothes(5, 0, 0);
                }*/

                Sync.Chat.SendLocal(Sync.Chat.MessageTypes.Me, player, Language.Strings.Get("CHAT_VEHICLE_BELT_OFF"));
            }
        }
        #endregion

        #region Cruise Control
        [RemoteEvent("Players::ToggleCruiseControl")]
        public static void ToggleCruiseControl(Player player, float speed)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            if (player.VehicleSeat != 0)
                return;

            var pData = sRes.Data;

            var veh = player.Vehicle;

            if (veh == null)
                return;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (vData.Data.HasCruiseControl || vData.IsAnchored)
                return;

            if (vData.ForcedSpeed >= Properties.Settings.Static.MIN_CRUISE_CONTROL_SPEED)
                vData.ForcedSpeed = 0f;
            else if (vData.EngineOn)
                vData.ForcedSpeed = speed > Properties.Settings.Static.MAX_CRUISE_CONTROL_SPEED ? Properties.Settings.Static.MAX_CRUISE_CONTROL_SPEED : speed;
        }
        #endregion

        #region Phone
        [RemoteEvent("Players::SPST")]
        public static void SetPhoneStateType(Player player, byte stateNum)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!Enum.IsDefined(typeof(Sync.Players.PhoneStateTypes), stateNum))
                return;

            var stateType = (Sync.Players.PhoneStateTypes)stateNum;

            var curStateType = pData.PhoneStateType;

            if (curStateType == stateType)
                return;

            if (stateType != Sync.Players.PhoneStateTypes.Off)
            {
                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return;

                pData.PhoneStateType = stateType;

                if (curStateType == Sync.Players.PhoneStateTypes.Off)
                {
                    Sync.Chat.SendLocal(Sync.Chat.MessageTypes.Me, player, Language.Strings.Get("CHAT_PLAYER_PHONE_ON"));

                    player.AttachObject(Sync.AttachSystem.Models.Phone, AttachSystem.Types.PhoneSync, -1, null);
                }
            }
            else
            {
                Sync.Players.StopUsePhone(pData);
            }
        }
        #endregion

        [RemoteEvent("Players::SetIsInvalid")]
        private static void SetIsInvalid(Player player, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            pData.IsInvalid = state;
        }

        [RemoteEvent("Players::PFA")]
        public static void PlayFastAnim(Player player, int anim)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!Enum.IsDefined(typeof(Sync.Animations.FastTypes), anim))
                return;

            var aType = (Sync.Animations.FastTypes)anim;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen || pData.HasAnyHandAttachedObject || pData.IsAnyAnimOn())
                return;

            if (aType == Animations.FastTypes.Whistle)
            {
                pData.PlayAnim(aType, Properties.Settings.Static.WhistleAnimationTime);
            }
        }

        [RemoteEvent("Players::SFTA")]
        public static void StopFastTimeoutedAnim(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            pData.StopFastAnim();
        }

        [RemoteEvent("Players::SetWalkstyle")]
        public static void SetWalkstyle(Player player, int walkstyle)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!Enum.IsDefined(typeof(Sync.Animations.WalkstyleTypes), walkstyle))
                return;

            pData.SetWalkstyle((Animations.WalkstyleTypes)walkstyle);
        }

        [RemoteEvent("Players::SetEmotion")]
        public static void SetEmotion(Player player, int emotion)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!Enum.IsDefined(typeof(Sync.Animations.EmotionTypes), emotion))
                return;

            pData.SetEmotion((Animations.EmotionTypes)emotion);
        }

        [RemoteEvent("Players::SetAnim")]
        public static void SetAnim(Player player, int anim)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!Enum.IsDefined(typeof(Sync.Animations.OtherTypes), anim))
                return;

            var aType = (Animations.OtherTypes)anim;

            if (pData.OtherAnim == aType)
                return;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen || pData.HasAnyHandAttachedObject || pData.GeneralAnim != Animations.GeneralTypes.None || pData.FastAnim != Animations.FastTypes.None)
                return;

            pData.PlayAnim((Animations.OtherTypes)anim);
        }

/*        [RemoteEvent("atsdme")]
        private static void AttachSystemDetachMe(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsAttachedToEntity is Entity entity)
            {
                if (!entity.Exists || !entity.AreEntitiesNearby(player, 150f))
                    entity.DetachEntity(player);
            }
        }*/

        [RemoteEvent("dmswme")]
        private static void DamageSystemWoundMe(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsWounded || pData.IsKnocked)
                return;

            pData.IsWounded = true;
        }

        [RemoteEvent("Player::UnpunishMe")]
        private static void UnpunishMe(Player player, uint punishmentId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var punishment = pData.Punishments.Where(x => x.Id == punishmentId).FirstOrDefault();

            if (punishment == null)
                return;

            if (punishment.IsActive())
                return;

            if (punishment.Type == Punishment.Types.Mute)
            {
                pData.IsMuted = false;

                player.TriggerEvent("Player::Punish", punishment.Id, (int)punishment.Type, ushort.MaxValue, -2, null);

                punishment.AmnestyInfo = new Punishment.Amnesty();

                MySQL.UpdatePunishmentAmnesty(punishment);
            }
            else if (punishment.Type == Punishment.Types.Warn)
            {
                player.TriggerEvent("Player::Punish", punishment.Id, (int)punishment.Type, ushort.MaxValue, -2, null);

                punishment.AmnestyInfo = new Punishment.Amnesty();

                MySQL.UpdatePunishmentAmnesty(punishment);
            }
            else if (punishment.Type == Punishment.Types.FractionMute)
            {
                player.TriggerEvent("Player::Punish", punishment.Id, (int)punishment.Type, ushort.MaxValue, -2, null);

                punishment.AmnestyInfo = new Punishment.Amnesty();

                MySQL.UpdatePunishmentAmnesty(punishment);
            }
        }
    }
}
