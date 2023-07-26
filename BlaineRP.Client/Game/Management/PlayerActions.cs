using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Game.Animations;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.Scripts.Misc;
using BlaineRP.Client.Game.UI.CEF;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Management
{
    internal static class PlayerActions
    {
        public enum Types
        {
            Knocked = 0,
            Frozen,
            InVehicle,
            IsSwimming,
            InWater,
            HasWeapon,
            Crouch,
            Crawl,
            Shooting,
            Climbing,
            Cuffed,
            Falling,
            Jumping,
            Ragdoll,
            Scenario,
            OtherAnimation,
            Animation,
            FastAnimation,
            PushingVehicle,
            NotOnFoot,
            Reloading,
            Finger,
            HasItemInHands,
            IsAttachedTo,
            MeleeCombat,
        }

        private static readonly Dictionary<Types, Func<PlayerData, bool, bool>> _actionsFuncs = new Dictionary<Types, Func<PlayerData, bool, bool>>()
        {
            {
                Types.Knocked, (pData, notify) =>
                {
                    if (pData?.IsKnocked ?? false)
                        return true;

                    return false;
                }
            },
            {
                Types.Frozen, (pData, notify) =>
                {
                    if (pData?.IsFrozen ?? false)
                        return true;

                    return false;
                }
            },
            { Types.Crouch, (pData, notify) => Crouch.Toggled },
            { Types.Crawl, (pData, notify) => Crawl.Toggled },
            { Types.Finger, (pData, notify) => Finger.Toggled },
            { Types.PushingVehicle, (pData, notify) => PushVehicle.Toggled },
            {
                Types.OtherAnimation, (pData, notify) =>
                {
                    if (pData == null)
                        return false;

                    if (pData.OtherAnim != OtherTypes.None)
                        return true;

                    return false;
                }
            },
            {
                Types.Animation, (pData, notify) =>
                {
                    if (pData == null)
                        return false;

                    if (pData.GeneralAnim != GeneralTypes.None)
                        return true;

                    return false;
                }
            },
            {
                Types.FastAnimation, (pData, notify) =>
                {
                    if (pData == null)
                        return false;

                    if (pData.FastAnim != FastTypes.None)
                        return true;

                    return false;
                }
            },
            {
                Types.Scenario, (pData, notify) =>
                {
                    return false;
                }
            },
            { Types.InVehicle, (pData, notify) => Player.LocalPlayer.IsInAnyVehicle(true) || Player.LocalPlayer.IsInAnyVehicle(false) },
            { Types.IsSwimming, (pData, notify) => Player.LocalPlayer.IsSwimming() || Player.LocalPlayer.IsSwimmingUnderWater() || Player.LocalPlayer.IsDiving() },
            { Types.InWater, (pData, notify) => Player.LocalPlayer.IsInWater() },
            { Types.HasWeapon, (pData, notify) => Player.LocalPlayer.HasWeapon() },
            { Types.Shooting, (pData, notify) => Player.LocalPlayer.IsShooting() },
            { Types.MeleeCombat, (pData, notify) => Player.LocalPlayer.IsInMeleeCombat() },
            { Types.Cuffed, (pData, notify) => pData?.IsCuffed ?? false },
            { Types.Climbing, (pData, notify) => Player.LocalPlayer.IsClimbing() },
            { Types.Falling, (pData, notify) => Player.LocalPlayer.IsFalling() || Player.LocalPlayer.IsJumpingOutOfVehicle() || Player.LocalPlayer.IsInParachuteFreeFall() },
            { Types.Jumping, (pData, notify) => Player.LocalPlayer.IsJumping() },
            { Types.Ragdoll, (pData, notify) => Player.LocalPlayer.IsRagdoll() },
            { Types.NotOnFoot, (pData, notify) => !Player.LocalPlayer.IsOnFoot() },
            {
                Types.Reloading, (pData, notify) =>
                {
                    if (Weapons.Core.Reloading)
                        return true;

                    return false;
                }
            },
            {
                Types.IsAttachedTo, (pData, notify) =>
                {
                    if (pData.IsAttachedTo != null)
                        return true;

                    return false;
                }
            },
            {
                Types.HasItemInHands, (pData, notify) =>
                {
                    if (pData == null)
                        return false;

                    if (pData.AttachedObjects.Where(x => Attachments.Core.IsTypeObjectInHand(x.Type)).Any())
                        return true;

                    return false;
                }
            },
        };

        /// <summary>Метод для проверки, может ли локальный игрок делать что-либо в данный момент</summary>
        /// <returns>Возврвает true, есле выполняются следующие условия, false - в противном случае</returns>
        public static bool IsAnyActionActive(bool notify, params Types[] actions)
        {
            foreach (Types x in actions)
            {
                if (!_actionsFuncs[x].Invoke(PlayerData.GetData(Player.LocalPlayer), notify))
                    continue;

                if (notify)
                    Notification.Show("ASP::ARN");

                return true;
            }

            return false;
        }
    }
}