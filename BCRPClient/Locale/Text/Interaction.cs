using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient
{
    public static partial class Locale
    {
        public static class Interaction
        {
            public static Dictionary<Additional.ExtraColshape.InteractionTypes, string> Names = new Dictionary<Additional.ExtraColshape.InteractionTypes, string>()
            {
                { Additional.ExtraColshape.InteractionTypes.HouseEnter, "для взаимодействия" },
                { Additional.ExtraColshape.InteractionTypes.HouseExit, "чтобы выйти" },
                { Additional.ExtraColshape.InteractionTypes.GarageExit, "чтобы выйти" },

                { Additional.ExtraColshape.InteractionTypes.Locker, "чтобы посмотреть шкаф" },
                { Additional.ExtraColshape.InteractionTypes.Wardrobe, "чтобы посмотреть гардероб" },
                { Additional.ExtraColshape.InteractionTypes.Fridge, "чтобы посмотреть холодильник" },

                { Additional.ExtraColshape.InteractionTypes.BusinessInfo, "для просмотра информации" },
                { Additional.ExtraColshape.InteractionTypes.BusinessEnter, "для взаимодействия" },

                { Additional.ExtraColshape.InteractionTypes.Interact, "для взаимодействия" },

                { Additional.ExtraColshape.InteractionTypes.NpcDialogue, "чтобы поговорить" },

                { Additional.ExtraColshape.InteractionTypes.ATM, "чтобы воспользоваться банкоматом" },

                { Additional.ExtraColshape.InteractionTypes.TuningEnter, "чтобы перейти к тюнингу" },

                { Additional.ExtraColshape.InteractionTypes.ShootingRangeEnter, "чтобы войти в тир [${0}]" },

                { Additional.ExtraColshape.InteractionTypes.ApartmentsRootEnter, "чтобы войти" },
                { Additional.ExtraColshape.InteractionTypes.ApartmentsRootExit, "чтобы выйти на улицу" },
                { Additional.ExtraColshape.InteractionTypes.ApartmentsRootElevator, "чтобы воспользоваться лифтом" },

                { Additional.ExtraColshape.InteractionTypes.GarageRootEnter, "для взаимодействия" },
            };
        }
    }
}
