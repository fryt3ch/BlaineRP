using System;
using System.Collections.Generic;
using System.Text;

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

                    if (house.Owner == pData.Info || house.Settlers.GetValueOrDefault(pData.Info)?[2] == true)
                        return true;

                    return !house.ContainersLocked;
                }
            },

            {
                "h_wardrobe",

                (cont, pData) =>
                {
                    var house = pData.CurrentHouse;

                    if (house == null || house.Wardrobe != cont.ID)
                        return false;

                    if (house.Owner == pData.Info || house.Settlers.GetValueOrDefault(pData.Info)?[3] == true)
                        return true;

                    return !house.ContainersLocked;
                }
            },

            {
                "h_fridge",

                (cont, pData) =>
                {
                    var house = pData.CurrentHouse;

                    if (house == null || house.Fridge != cont.ID)
                        return false;

                    if (house.Owner == pData.Info || house.Settlers.GetValueOrDefault(pData.Info)?[4] == true)
                        return true;

                    return !house.ContainersLocked;
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
        };
    }
}
