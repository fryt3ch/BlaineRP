using System;
using System.Collections.Generic;

namespace BCRPServer.Game.Items
{
    public partial class Container
    {
        private static Dictionary<string, Func<Container, PlayerData, bool>> PermissionCheckFuncs = new Dictionary<string, Func<Container, PlayerData, bool>>()
        {
            {
                "h_locker",

                (cont, pData) =>
                {
                    var house = pData.CurrentHouse;

                    if (house == null || house.Locker != cont.ID)
                        return false;

                    if (house.Owner == pData.Info)
                        return true;

                    if (!house.ContainersLocked)
                        return true;

                    if (house.Settlers.GetValueOrDefault(pData.Info)?[2] == true)
                        return true;

                    pData.Player.Notify("House::NotAllowed");

                    return false;
                }
            },

            {
                "h_wardrobe",

                (cont, pData) =>
                {
                    var house = pData.CurrentHouse;

                    if (house == null || house.Wardrobe != cont.ID)
                        return false;

                    if (!house.ContainersLocked)
                        return true;

                    if (house.Settlers.GetValueOrDefault(pData.Info)?[3] == true)
                        return true;

                    pData.Player.Notify("House::NotAllowed");

                    return false;
                }
            },

            {
                "h_fridge",

                (cont, pData) =>
                {
                    var house = pData.CurrentHouse;

                    if (house == null || house.Fridge != cont.ID)
                        return false;

                    if (!house.ContainersLocked)
                        return true;

                    if (house.Settlers.GetValueOrDefault(pData.Info)?[4] == true)
                        return true;

                    pData.Player.Notify("House::NotAllowed");

                    return false;
                }
            },

            {
                "f_storage",

                (cont, pData) =>
                {
                    var fractionData = Game.Fractions.Fraction.Get(pData.Fraction);

                    if (fractionData == null || fractionData.ContainerId != cont.ID)
                    {
                        pData.Player.Notify("Fraction::NM");

                        return false;
                    }

                    if (fractionData.ContainerLocked && !fractionData.IsLeaderOrWarden(pData.Info, false))
                    {
                        pData.Player.Notify("Container::CF");

                        return false;
                    }

                    return true;
                }
            },
        };

        private static Dictionary<string, Func<Container, PlayerData, bool>> NearnessCheckFuncs = new Dictionary<string, Func<Container, PlayerData, bool>>()
        {
            {
                "h_locker",

                (cont, pData) =>
                {
                    var house = pData.CurrentHouse;

                    return house != null && house.Locker == cont.ID;
                }
            },

            {
                "h_wardrobe",

                (cont, pData) =>
                {
                    var house = pData.CurrentHouse;

                    return house != null && house.Wardrobe == cont.ID;
                }
            },

            {
                "h_fridge",

                (cont, pData) =>
                {
                    var house = pData.CurrentHouse;

                    return house != null && house.Fridge == cont.ID;
                }
            },

            {
                "f_storage",

                (cont, pData) =>
                {
                    var fractionData = Game.Fractions.Fraction.Get(pData.Fraction);

                    if (fractionData == null || fractionData.ContainerId != cont.ID)
                    {
                        pData.Player.Notify("Fraction::NM");

                        return false;
                    }

                    return fractionData.ContainerPosition.Position.DistanceTo(pData.Player.Position) <= fractionData.ContainerPosition.RotationZ + 2.5f;
                }
            },
        };
    }
}
