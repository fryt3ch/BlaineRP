using BCRPServer.Sync;
using GTANetworkAPI;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BCRPServer.Game.Items.Inventory;

namespace BCRPServer.Events.Players
{
    class Main : Script
    {
        [ServerEvent(Event.PlayerWeaponSwitch)]
        private static void OnPlayerWeaponSwitch(Player player, uint oldWeapon, uint newWeapon)
        {
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

        #region Player Connected
        [ServerEvent(Event.PlayerConnected)]
        private static async Task OnPlayerConnected(Player player)
        {
            if (player?.Exists != true || player.GetTempData() != null || player.GetMainData() != null)
                return;

            var scId = player.SocialClubId;
            var ip = player.Address;
            var hwid = player.Serial;

            var bans = await MySQL.GlobalBansGet(hwid, ip, scId);

            NAPI.Task.Run(async () =>
            {
                if (player?.Exists != true)
                    return;

                if (bans.Count > 0)
                {
                    Utils.KickSilent(player, bans[0].ToString());

                    return;
                }

                var tData = new TempData(player);

                player.SetTempData(tData);

                player.SetAlpha(0);
                player.Teleport(new Vector3(-749.78f, 5818.21f, 0), false, Utils.GetPrivateDimension(player));
                player.Name = player.SocialClubName;

                player.SkyCameraMove(Additional.SkyCamera.SwitchTypes.OutFromPlayer, true, "FadeScreen", false);

                var account = await MySQL.AccountGet(scId);

                NAPI.Task.Run(() =>
                {
                    if (player?.Exists != true)
                        return;

                    tData.BlockRemoteCalls = false;

                    if (account == null)
                    {
                        player.TriggerEvent("Auth::ShowRegistrationPage", player.SocialClubName);
                    }
                    else
                    {
                        int i = -1;

                        foreach (var x in PlayerData.PlayerInfo.GetAllByAID(account.ID))
                        {
                            i++;

                            if (i >= tData.Characters.Length)
                                break;

                            tData.Characters[i] = x;
                        }

                        tData.AccountData = account;

                        tData.ActualToken = Events.Players.Auth.GenerateToken(account, hwid);

                        player.TriggerEvent("Auth::ShowLoginPage", player.SocialClubName);
                    }
                });
            });
        }
        #endregion

        #region Player Disconnected
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

                pData.ActiveOffer?.Cancel(false, true, Sync.Offers.ReplyTypes.AutoCancel, false);

                player.DetachAllEntities();

                pData.IsAttachedToEntity?.DetachEntity(player);

                if (player.Vehicle != null)
                {
                    player.WarpOutOfVehicle();
                }

                #region Check&Start Deletion of Owned Vehicles

                pData.RentedVehicle?.StartDeletionTask();

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

                vehsToStartDeletion.ForEach(x => x.VehicleData.StartDeletionTask());
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

                var aWeapon = pData.ActiveWeapon;

                if (aWeapon != null)
                {
                    aWeapon.Value.WeaponItem.Unequip(pData, true, false);
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

                pData.Info.LastData.Position.Position = player.Position;
                pData.Info.LastData.Dimension = player.Dimension;
                pData.Info.LastData.Position.RotationZ = player.Heading;

                MySQL.CharacterSaveOnExit(pData.Info);

                pData.Remove();
            }
        }
        #endregion

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

            player.InventoryUpdate(Game.Items.Inventory.Groups.Armour, Game.Items.Item.ToClientJson(null, Game.Items.Inventory.Groups.Armour));

            MySQL.CharacterArmourUpdate(pData.Info);

            arm.Delete();
        }

        [RemoteEvent("Players::OnDeath")]
        private static void OnPlayerDeath(Player player, Player killer)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked)
            {
                player.Teleport(null, false, null, null, false);

                player.SetHealth(10);

                pData.IsKnocked = false;
            }
            else
            {
                player.Teleport(null, false, null, null, false);

                var arm = player.Armor;

                NAPI.Player.SpawnPlayer(player, player.Position, player.Heading);

                player.Armor = arm;

                pData.IsKnocked = true;
                pData.IsWounded = false;

                player.SetHealth(50);

                pData.PlayAnim(Sync.Animations.GeneralTypes.Knocked);

                if (Settings.DROP_WEAPONS_AFTER_DEATH)
                {
                    for (int i = 0; i < pData.Weapons.Length; i++)
                        if (pData.Weapons[i] != null)
                            pData.InventoryDrop(Game.Items.Inventory.Groups.Weapons, i, 1);
                }

                if (pData.Holster?.Items[0] != null)
                    pData.InventoryDrop(Game.Items.Inventory.Groups.Holster, 0, 1);

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

                            pData.InventoryDrop(Game.Items.Inventory.Groups.Items, i, ammoToDrop);

                            droppedAmmo += ammoToDrop;

                            if (droppedAmmo == Settings.MAX_AMMO_TO_DROP_AFTER_DEATH)
                                break;
                        }
                }
            }
        }

        [RemoteEvent("Players::CharacterReady")]
        public static void CharacterReady(Player player, bool isInvalid, int emotion, int walkstyle)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!player.HasData("CharacterNotReady"))
                return;

            player.ResetData("CharacterNotReady");

            if (!Enum.IsDefined(typeof(Sync.Animations.EmotionTypes), emotion))
                emotion = -1;

            if (!Enum.IsDefined(typeof(Sync.Animations.WalkstyleTypes), walkstyle))
                walkstyle = -1;

            pData.IsInvalid = isInvalid;
            pData.Emotion = (Sync.Animations.EmotionTypes)emotion;
            pData.Walkstyle = (Sync.Animations.WalkstyleTypes)walkstyle;

            //player.Dimension = Utils.Dimensions.Main;

            pData.UpdateWeapons();
        }

        #region Finger
        [RemoteEvent("fpsu")]
        public static void FingerUpdate(Player sender, float camPitch, float camHeading)
        {
            sender?.TriggerEventInDistance(Settings.FREQ_UPDATE_DISTANCE, "fpsu", sender.Handle, camPitch, camHeading);
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

            if (!player.AreEntitiesNearby(veh, 10f))
                return;

            Sync.Chat.SendLocal(Sync.Chat.Types.Me, player, $"{Locale.Chat.Player.PointsAt} {vData.Data.Name} [{vData.Numberplate?.Tag ?? Locale.Chat.Vehicle.NoNumberplate}]", null);
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

            if (!player.AreEntitiesNearby(target, 10f))
                return;

            Sync.Chat.SendLocal(Sync.Chat.Types.MePlayer, player, Locale.Chat.Player.PointsAt, target);
        }

        [RemoteEvent("Players::FingerPoint::Ped")]
        public static void PointAtPed(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            Sync.Chat.SendLocal(Sync.Chat.Types.MePlayer, player, Locale.Chat.Player.PointsAtPerson, null);
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

            var vehData = veh.GetMainData();

            if (veh?.Exists != true || veh.EngineStatus || !player.AreEntitiesNearby(veh, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                return;

            if (pData.IsAttachedToEntity != null || vehData.ForcedSpeed != 0f)
                return;

            veh.AttachEntity(player, isInFront ? AttachSystem.Types.PushVehicleFront : AttachSystem.Types.PushVehicleBack);
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

            if (attachData.Type != AttachSystem.Types.PushVehicleFront && attachData.Type != AttachSystem.Types.PushVehicleBack)
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

                Sync.Chat.SendLocal(Sync.Chat.Types.Me, player, Locale.Chat.Vehicle.BeltOn);
            }
            else
            {
                player.SetClothes(5, 0, 0);
                pData.Bag?.Wear(pData);

                Sync.Chat.SendLocal(Sync.Chat.Types.Me, player, Locale.Chat.Vehicle.BeltOff);
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

            if (!Utils.IsCar(veh) || vData.IsAnchored)
                return;

            if (vData.ForcedSpeed >= Settings.MIN_CRUISE_CONTROL_SPEED)
                vData.ForcedSpeed = 0f;
            else if (vData.EngineOn)
                vData.ForcedSpeed = speed > Settings.MAX_CRUISE_CONTROL_SPEED ? Settings.MAX_CRUISE_CONTROL_SPEED : speed;
        }
        #endregion

        #region Phone
        [RemoteEvent("Players::TogglePhone")]
        public static void StartUsePhone(Player player, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var hadWeapon = pData.UnequipActiveWeapon();

            if (pData.PhoneOn == state)
                return;

            if (state)
            {
                if (hadWeapon)
                {
                    NAPI.Task.Run(() =>
                    {
                        if (player?.Exists != true)
                            return;

                        pData.PhoneOn = true;

                        player.AttachObject(Sync.AttachSystem.Models.Phone, AttachSystem.Types.Phone, -1, null);

                        Sync.Chat.SendLocal(Sync.Chat.Types.Me, player, Locale.Chat.Player.PhoneOn);
                    }, 250);
                }
                else
                {
                    pData.PhoneOn = true;

                    player.AttachObject(Sync.AttachSystem.Models.Phone, AttachSystem.Types.Phone, -1, null);

                    Sync.Chat.SendLocal(Sync.Chat.Types.Me, player, Locale.Chat.Player.PhoneOn);
                }
            }
            else
            {
                Sync.Players.StopUsePhone(pData);
            }
        }
        #endregion

        /// <summary>Получен урон от игрока</summary>
        [RemoteEvent("Players::GotDamage")]
        private static void GotDamage(Player player, int damage, bool isGun)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            pData.LastDamageTime = Utils.GetCurrentTime();

            if (isGun && !pData.IsWounded && !pData.IsKnocked && (new Random()).NextDouble() <= Settings.WOUND_CHANCE)
            {
                pData.IsWounded = true;
            }
        }

        [RemoteEvent("Player::UpdateTime")]
        private static void UpdateTime(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var currentTime = Utils.GetCurrentTime();

            if (currentTime.Subtract(pData.LastJoinDate).TotalSeconds < 60)
                return;

            pData.LastJoinDate = currentTime;

            pData.TimePlayed += 1;
            pData.LastData.SessionTime += 60;

            if (pData.TimePlayed % 2 == 0)
            {
                if (pData.Satiety > 0)
                    pData.Satiety--;

                if (pData.Mood > 0)
                    pData.Mood--;
            }
        }

        [RemoteEvent("Players::SetIsInvalid")]
        private static void SetIsInvalid(Player player, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            pData.IsInvalid = state;
        }

        [RemoteEvent("Players::PlayAnim")]
        public static void PlayAnim(Player player, bool fast, int anim)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!Enum.IsDefined(typeof(Sync.Animations.FastTypes), anim))
                return;

            pData.PlayAnim((Sync.Animations.FastTypes)anim);
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

            pData.PlayAnim((Animations.OtherTypes)anim);
        }

        [RemoteEvent("Players::StopCarry")]
        public static void StopCarry(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var atPlayer = pData.IsAttachedToEntity as Player;

            if (atPlayer?.Exists != true)
            {
                var aData = pData.AttachedEntities.Where(x => x.Type == AttachSystem.Types.Carry && x.EntityType == EntityType.Player && x.Id >= 0).FirstOrDefault();

                if (aData == null)
                    return;

                var target = Utils.FindReadyPlayerOnline((uint)aData.Id);

                if (target?.Player?.Exists != true)
                    return;

                player.DetachEntity(target.Player);
            }
            else
            {
                atPlayer.DetachEntity(player);
            }
        }

        [RemoteEvent("Players::GoToTrunk")]
        public static void GoToTrunk(Player player, Vehicle vehicle)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var attachData = pData.IsAttachedToEntity;

            if (attachData != null)
                return;

            var vData = vehicle.GetMainData();

            if (vData == null)
                return;

            vehicle.AttachEntity(player, AttachSystem.Types.VehicleTrunk);
        }

        [RemoteEvent("Players::StopInTrunk")]
        public static void StopInTrunk(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var atVeh = pData.IsAttachedToEntity as Vehicle;

            if (atVeh?.Exists != true)
                return;

            var atData = atVeh.GetAttachmentData(player);

            if (atData == null || atData.Type != AttachSystem.Types.VehicleTrunk)

            atVeh.DetachEntity(player);
        }

        [RemoteEvent("Players::Smoke::Stop")]
        public static void StopSmoke(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            foreach (var x in pData.AttachedObjects)
            {
                if (Game.Items.Cigarette.AttachTypes.Contains(x.Type))
                {
                    player.DetachObject(x.Type);

                    pData.StopAnim();

                    break;
                }
            }
        }

        [RemoteEvent("Players::Smoke::Puff")]
        public static void SmokeDoPuff(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            foreach (var x in pData.AttachedObjects)
            {
                if (Game.Items.Cigarette.AttachTypes.Contains(x.Type))
                {
                    pData.PlayAnim(Animations.FastTypes.SmokePuffCig);

                    player.TriggerEvent("Player::Smoke::Puff");

                    break;
                }
            }
        }

        [RemoteEvent("Players::Smoke::State")]
        public static void SmokeSetState(Player player, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            Sync.AttachSystem.AttachmentObjectNet attachData = null;

            foreach (var x in pData.AttachedObjects)
            {
                if (Game.Items.Cigarette.AttachTypes.Contains(x.Type))
                {
                    pData.PlayAnim(Animations.FastTypes.SmokeTransitionCig);

                    attachData = x;

                    break;
                }
            }

            if (attachData == null)
                return;

            var oppositeType = Game.Items.Cigarette.DependentTypes[attachData.Type];

            NAPI.Task.Run(() =>
            {
                if (player?.Exists != true)
                    return;

                player.DetachObject(attachData.Type, false);
                player.AttachObject(attachData.Model, oppositeType, -1, null);
            }, 500);
        }

        [RemoteProc("WSkins::Rm")]
        private static bool WeaponSkinsRemove(Player player, int wSkinTypeNum)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            if (!Enum.IsDefined(typeof(Game.Items.WeaponSkin.ItemData.Types), wSkinTypeNum))
                return false;

            var wSkinType = (Game.Items.WeaponSkin.ItemData.Types)wSkinTypeNum;

            var pData = sRes.Data;

            var ws = pData.Info.WeaponSkins.GetValueOrDefault(wSkinType);

            if (ws == null)
                return false;

            var freeIdx = -1;

            for (int i = 0; i < pData.Items.Length; i++)
            {
                if (pData.Items[i] == null)
                {
                    freeIdx = i;

                    break;
                }
            }

            if (freeIdx < 0)
            {
                player.Notify("Inventory::NoSpace");

                return false;
            }

            pData.Info.WeaponSkins.Remove(wSkinType);

            pData.Items[freeIdx] = ws;

            player.InventoryUpdate(Groups.Items, freeIdx, ws.ToClientJson(Groups.Items));

            player.TriggerEvent("Player::WSkins::Update", false, ws.ID);

            MySQL.CharacterWeaponSkinsUpdate(pData.Info);
            MySQL.CharacterItemsUpdate(pData.Info);

            for (int i = 0; i < pData.Weapons.Length; i++)
            {
                if (pData.Weapons[i] is Game.Items.Weapon weapon)
                {
                    weapon.UpdateWeaponComponents(pData);
                }
            }

            if (pData.Holster?.Items[0] is Game.Items.Weapon hWeapon)
            {
                hWeapon.UpdateWeaponComponents(pData);
            }

            return true;
        }
    }
}
