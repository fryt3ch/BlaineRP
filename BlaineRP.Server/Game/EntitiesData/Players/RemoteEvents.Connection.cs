using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlaineRP.Server.Game.Attachments;
using BlaineRP.Server.Game.EntitiesData.Vehicles;
using BlaineRP.Server.Game.Management.Misc;
using BlaineRP.Server.Game.Management.Punishments;
using BlaineRP.Server.Game.Management.Reports;
using BlaineRP.Server.Game.Offers;
using BlaineRP.Server.Game.Phone;
using BlaineRP.Server.Game.Quests;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;
using Newtonsoft.Json.Linq;

namespace BlaineRP.Server.Game.EntitiesData.Players
{
    internal partial class RemoteEvents : Script
    {
        [ServerEvent(Event.IncomingConnection)]
        private static void OnIncomingConnection(string ip, string serial, string rgscName, ulong rgscId, GameTypes gameType, CancelEventArgs cancel)
        {
            if (Server.Main.IsRestarting)
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

                player.SkyCameraMove(SkyCamera.SwitchType.OutFromPlayer, true, "FadeScreen", false);

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
                    Sync.Players.ExitFromBusiness(pData, false);

                pData.ActiveOffer?.Cancel(false, true, ReplyTypes.AutoCancel, false);

                pData.ActiveCall?.Cancel(Call.CancelTypes.ServerAuto);

                var policeCall = Game.Fractions.Police.GetCallByCaller(player.Id);

                if (policeCall != null)
                    Game.Fractions.Police.RemoveCall(player.Id, policeCall, 0, null);

                Report.GetByStarterPlayer(pData.Info)?.Close(pData);

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

                var cuffsAttachment = attachedObjects.Where(x => x.Type == AttachmentType.Cuffs).FirstOrDefault();

                if (cuffsAttachment != null)
                {
                    pData.Info.GetTempData<Timer>("CuffedQuitTimer")?.Dispose();

                    pData.Info.SetTempData("CuffedQuitTimer", new Timer((obj) =>
                    {
                        NAPI.Task.Run(() =>
                        {
                            var activePunishment = pData.Info.Punishments.Where(x => x.Type == PunishmentType.Arrest || x.Type == PunishmentType.FederalPrison || x.Type == PunishmentType.NRPPrison).FirstOrDefault();

                            if (activePunishment != null)
                                return;


                        });
                    }, null, 300_000, Timeout.Infinite));
                }

                player.DetachAllObjects();

                player.DetachAllEntities();

                pData.IsAttachedToEntity?.DetachEntity(player);

                if (player.Vehicle is GTANetworkAPI.Vehicle veh)
                {
                    var vData = VehicleData.GetData(veh);

                    if (vData != null)
                        Sync.Vehicles.OnPlayerLeaveVehicle(pData, vData);
                }

                if (pData.Info.Quests.GetValueOrDefault(QuestType.DRSCHOOL0) is Quest driveSchoolQuest && driveSchoolQuest.Step > 0)
                    driveSchoolQuest.Cancel(pData.Info);
                
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
                            var vInfo = vehsToStartDeletion.Where(x => x.VID == vKey.Vid).FirstOrDefault();

                            if (vKey.IsKeyValid(vInfo))
                                vehsToStartDeletion.Remove(vInfo);
                        }
                    }
                }

                vehsToStartDeletion.ForEach(x => x.VehicleData.StartDeletionTask(Properties.Settings.Static.OWNED_VEHICLE_TIME_TO_AUTODELETE));

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

                pData.Info.LastData.UpdatePosition(new Vector4(player.Position, player.Heading), player.Dimension, false);

                MySQL.CharacterSaveOnExit(pData.Info);

                pData.Remove();
            }
        }
    }
}