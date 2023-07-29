using System;
using System.Collections.Generic;
using BlaineRP.Server.Game.Achievements;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.Inventory;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Businesses
{
    public partial class WeaponShop
    {
        internal class RemoteEvents : Script
        {
            [RemoteEvent("SRange::Enter::Shop")]
            private static void ShootingRangeEnterShop(Player player, int id)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                if (player.Dimension != Properties.Settings.Static.MainDimension)
                    return;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return;

                var ws = Get(id) as WeaponShop;

                if (ws == null)
                    return;

                if (!ws.IsPlayerNearShootingRangeEnterPosition(player))
                    return;

                if (!pData.CanUseInventory(true) || player.HasData("INSRANGE"))
                    return;

                DateTime curTime = Utils.GetCurrentTime();

                uint cdSRangeHash = NAPI.Util.GetHashKey("SRANGE_SHOP");

                if (pData.Info.HasCooldown(cdSRangeHash, curTime, out _, out _, out _, 1d))
                {
                    player.NotifyError(Language.Strings.Get("NTFC_COOLDOWN_GEN_1"));

                    return;
                }

                if (!ws.TryBuyShootingRange(pData))
                    return;

                pData.Info.LastData.UpdatePosition(new Vector4(player.Position, player.Heading), player.Dimension, false);

                uint pDim = Utils.GetPrivateDimension(player);

                pData.StopUseCurrentItem();
                player.DetachAllObjectsInHand();
                pData.StopAllAnims();

                player.Teleport(ShootingRangePosition.Position, false, pDim, ShootingRangePosition.RotationZ, true);

                player.TriggerEvent("SRange::Start", 0);

                pData.IsInventoryBlocked = true;

                pData.TakeWeapons();

                pData.GiveTempWeapon("w_pistol", -1);

                pData.CurrentBusiness = ws;

                pData.Info.SetCooldown(cdSRangeHash, curTime, ShootingRangeTryCooldownTime, true);

                player.SetData("INSRANGE", true);
            }

            [RemoteEvent("SRange::Exit::Shop")]
            private static void ShootingRangeExitShop(Player player, int score, int maxScore, float accuracy)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                var ws = pData.CurrentBusiness as WeaponShop;

                if (ws == null)
                    return;

                if (!player.ResetData("INSRANGE"))
                    return;

                pData.CurrentBusiness = null;

                player.Teleport(ws.PositionShootingRangeEnter.Position, false, Properties.Settings.Static.MainDimension, ws.PositionShootingRangeEnter.RotationZ, true);

                pData.IsInventoryBlocked = false;

                if (pData.UnequipActiveWeapon())
                    if (pData.Weapons[0] is Items.Weapon weapon)
                    {
                        pData.Weapons[0] = null;

                        weapon.Delete();

                        Inventory.Service.ClearSlot(pData, GroupTypes.Weapons, 0);
                    }

                pData.GiveTakenItems();

                int diff = score < maxScore ? -1 : 1;

                int currentSkill = pData.Info.Skills[SkillTypes.Shooting];

                diff = Utils.CalculateDifference(currentSkill, diff, 0, Properties.Settings.Static.PlayerMaxSkills.GetValueOrDefault(SkillTypes.Shooting));

                if (diff == 0)
                    return;

                currentSkill += diff;

                pData.Info.UpdateSkill(SkillTypes.Shooting, diff);

                pData.Info.Achievements[AchievementType.SR1].UpdateProgress(pData.Info, (uint)currentSkill);

                if (currentSkill == 100)
                    pData.Info.Achievements[AchievementType.SR2].UpdateProgress(pData.Info, (uint)Math.Round(accuracy < 0 ? 0f : accuracy > 100f ? 100f : accuracy));
            }
        }
    }
}