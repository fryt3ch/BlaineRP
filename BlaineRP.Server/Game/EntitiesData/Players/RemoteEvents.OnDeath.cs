using System;
using BlaineRP.Server.Game.Estates;
using BlaineRP.Server.Game.Inventory;
using BlaineRP.Server.Game.Management.Punishments;
using BlaineRP.Server.Game.Phone;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.EntitiesData.Players
{
    internal partial class RemoteEvents
    {
        [RemoteEvent("Players::OnDeath")]
        private static void OnPlayerDeath(Player player, Player attacker)
        {
            (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            PlayerData pData = sRes.Data;

            pData.StopAllAnims();

            player.DetachAllObjectsInHand();

            pData.StopUseCurrentItem();

            foreach (Punishment x in pData.Punishments)
            {
                if (x.Type == PunishmentType.NRPPrison)
                {
                    if (!x.IsActive())
                        continue;

                    if (pData.IsKnocked)
                        pData.SetAsNotKnocked();

                    Vector3 pos = Utils.Demorgan.GetNextPos();

                    player.Teleport(pos, false, null, null, false);

                    NAPI.Player.SpawnPlayer(player, pos, player.Heading);

                    player.SetHealth(50);

                    return;
                }
                else if (x.Type == PunishmentType.Arrest)
                {
                    if (!x.IsActive())
                        continue;

                    if (pData.IsKnocked)
                        pData.SetAsNotKnocked();

                    var fData = Fractions.Fraction.Get((Fractions.FractionType)int.Parse(x.AdditionalData.Split('_')[1])) as Fractions.Police;

                    if (fData != null)
                    {
                        Vector3 pos = fData.GetNextArrestCellPosition();

                        player.Teleport(pos, false, null, null, false);

                        NAPI.Player.SpawnPlayer(player, pos, player.Heading);

                        player.SetHealth(50);
                    }

                    return;
                }
                else if (x.Type == PunishmentType.FederalPrison)
                {
                    if (!x.IsActive())
                        continue;

                    if (pData.IsKnocked)
                        pData.SetAsNotKnocked();

                    return;
                }
            }

            uint pDim = player.Dimension;

            /*            if (pDim == Utils.GetPrivateDimension(player))
                        {
                            Game.Fractions.EMS.SetPlayerToEmsAfterDeath(pData, pData.LastData.Position.Position);

                            return;
                        }*/

            if (pData.IsKnocked)
            {
                Vector3 pos = player.Position;

                if (pDim >= Properties.Settings.Profile.Current.Game.HouseDimensionBaseOffset)
                {
                    if (pDim < Properties.Settings.Profile.Current.Game.ApartmentsDimensionBaseOffset)
                    {
                        var house = Utils.GetHouseBaseByDimension(pDim) as Estates.House;

                        if (house != null)
                            pos = house.PositionParams.Position;
                    }
                    else if (pDim < Properties.Settings.Profile.Current.Game.ApartmentsRootDimensionBaseOffset)
                    {
                        var aps = Utils.GetHouseBaseByDimension(pDim) as Estates.Apartments;

                        if (aps != null)
                            pos = aps.Root.EnterParams.Position;
                    }
                    else if (pDim < Properties.Settings.Profile.Current.Game.GarageDimensionBaseOffset)
                    {
                        ApartmentsRoot apsRoot = Utils.GetApartmentsRootByDimension(pDim);

                        if (apsRoot != null)
                            pos = apsRoot.EnterParams.Position;
                    }
                    else
                    {
                        Garage garage = Utils.GetGarageByDimension(pDim);

                        if (garage != null)
                            pos = garage.Root.EnterPosition.Position;
                    }
                }

                Fractions.EMS.SetPlayerToEmsAfterDeath(pData, pos);
            }
            else
            {
                player.Teleport(null, false, null, null, false);

                pData.ActiveCall?.Cancel(Call.CancelTypes.ServerAuto);

                NAPI.Player.SpawnPlayer(player, player.Position, player.Heading);

                pData.SetAsKnocked(attacker);

                player.SetHealth(50);

                if (Properties.Settings.Profile.Current.Game.KnockedDropWeaponsEnabled)
                    for (var i = 0; i < pData.Weapons.Length; i++)
                    {
                        if (pData.Weapons[i] != null)
                            pData.InventoryDrop(GroupTypes.Weapons, i, 1);
                    }

                if (pData.Holster?.Items[0] != null)
                    pData.InventoryDrop(GroupTypes.Holster, 0, 1);

                if (Properties.Settings.Profile.Current.Game.KnockedDropAmmoTotalPercentage > 0f && Properties.Settings.Profile.Current.Game.KnockedDropAmmoMaxAmount > 0)
                {
                    var droppedAmmo = 0;

                    for (var i = 0; i < pData.Items.Length; i++)
                    {
                        if (pData.Items[i] is Items.Ammo)
                        {
                            var ammoToDrop = (int)Math.Floor((pData.Items[i] as Items.Ammo).Amount * Properties.Settings.Profile.Current.Game.KnockedDropAmmoTotalPercentage);

                            if (ammoToDrop + droppedAmmo > Properties.Settings.Profile.Current.Game.KnockedDropAmmoMaxAmount)
                                ammoToDrop = Properties.Settings.Profile.Current.Game.KnockedDropAmmoMaxAmount - droppedAmmo;

                            if (ammoToDrop == 0)
                                break;

                            pData.InventoryDrop(GroupTypes.Items, i, ammoToDrop);

                            droppedAmmo += ammoToDrop;

                            if (droppedAmmo == Properties.Settings.Profile.Current.Game.KnockedDropAmmoMaxAmount)
                                break;
                        }
                    }
                }
            }
        }
    }
}