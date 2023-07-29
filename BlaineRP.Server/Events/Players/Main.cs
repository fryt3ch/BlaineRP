using BlaineRP.Server.Sync;
using GTANetworkAPI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlaineRP.Server.Game.Animations;
using BlaineRP.Server.Game.Attachments;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.EntitiesData.Vehicles;
using BlaineRP.Server.Game.EntitiesData.Vehicles.Static;
using BlaineRP.Server.Game.Inventory;
using BlaineRP.Server.Game.Management;
using BlaineRP.Server.Game.Management.Chat;
using BlaineRP.Server.Game.Management.Misc;
using BlaineRP.Server.Game.Management.Punishments;
using BlaineRP.Server.Game.Management.Reports;
using BlaineRP.Server.Game.Offers;
using BlaineRP.Server.Game.Phone;
using BlaineRP.Server.Game.Quests;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Events.Players
{
    class Main : Script
    {
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

            if (!Enum.IsDefined(typeof(EmotionType), emotion))
                emotion = -1;

            if (!Enum.IsDefined(typeof(WalkstyleType), walkstyle))
                walkstyle = -1;

            pData.IsInvalid = isInvalid;
            pData.Emotion = (EmotionType)emotion;
            pData.Walkstyle = (WalkstyleType)walkstyle;

            pData.UpdateWeapons();

            if (pData.RentedJobVehicle is VehicleData jobVehicle)
            {
                jobVehicle.Job?.SetPlayerJob(pData, jobVehicle);
            }

            if (pData.Fraction != Game.Fractions.FractionType.None)
            {
                var fData = Game.Fractions.Fraction.Get(pData.Fraction);

                fData?.OnMemberJoined(pData);
            }

            return true;
        }

        #region Finger
        [RemoteEvent("fpsu")]
        public static void FingerUpdate(Player sender, float camPitch, float camHeading)
        {
            sender?.TriggerEventInDistance(Properties.Settings.Static.FREQ_UPDATE_DISTANCE, "fpsu", sender.Handle, camPitch, camHeading);
        }

        [RemoteEvent("Players::FingerPoint::Vehicle")]
        public static void PointAtVehicle(Player player, GTANetworkAPI.Vehicle veh)
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

            Game.Management.Chat.Service.SendLocal(MessageType.Me, player, Language.Strings.Get("CHAT_PLAYER_FP_0", vData.GetName(1)), null);
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

            Game.Management.Chat.Service.SendLocal(MessageType.Me, player, Language.Strings.Get("CHAT_PLAYER_FP_0"), target);
        }

        [RemoteEvent("Players::FingerPoint::Ped")]
        public static void PointAtPed(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            Game.Management.Chat.Service.SendLocal(MessageType.Me, player, Language.Strings.Get("CHAT_PLAYER_FP_1", null));
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
        public static void StartPushingVehicle(Player player, GTANetworkAPI.Vehicle veh, bool isInFront)
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
                veh.AttachEntity(player, AttachmentType.PushVehicle, "1");
            else
                veh.AttachEntity(player, AttachmentType.PushVehicle, "0");
        }

        [RemoteEvent("Players::StopPushingVehicleSync")]
        public static void StopPushingVehicle(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var atVeh = pData.IsAttachedToEntity as GTANetworkAPI.Vehicle;

            if (atVeh?.Exists != true)
                return;

            var attachData = atVeh.GetAttachmentData(player);

            if (attachData == null)
                return;

            if (attachData.Type != AttachmentType.PushVehicle)
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

                if (vData.Data.Type != VehicleTypes.Car)
                    return;
            }


            pData.BeltOn = isBeltOn;

            if (isBeltOn)
            {
                //player.SetClothes(5, 81, 0);

                Game.Management.Chat.Service.SendLocal(MessageType.Me, player, Language.Strings.Get("CHAT_VEHICLE_BELT_ON"));
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

                Game.Management.Chat.Service.SendLocal(MessageType.Me, player, Language.Strings.Get("CHAT_VEHICLE_BELT_OFF"));
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

            if (!vData.Data.HasCruiseControl || vData.IsAnchored)
                return;

            if (vData.ForcedSpeed >= Properties.Settings.Static.MIN_CRUISE_CONTROL_SPEED)
                vData.ForcedSpeed = 0f;
            else if (vData.EngineOn)
                vData.ForcedSpeed = speed > Properties.Settings.Static.MAX_CRUISE_CONTROL_SPEED ? Properties.Settings.Static.MAX_CRUISE_CONTROL_SPEED : speed;
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
    }
}
