using System.Collections.Generic;
using BlaineRP.Client.Game.Wrappers.Colshapes;
using BlaineRP.Client.Game.Wrappers.Colshapes.Enums;

namespace BlaineRP.Client
{
    public static partial class Locale
    {
        public static class Interaction
        {
            public static Dictionary<InteractionTypes, string> Names = new Dictionary<InteractionTypes, string>()
            {
                { InteractionTypes.HouseEnter, "для взаимодействия" },
                { InteractionTypes.HouseExit, "чтобы выйти" },
                { InteractionTypes.GarageExit, "чтобы выйти" },

                { InteractionTypes.Locker, "чтобы посмотреть шкаф" },
                { InteractionTypes.Wardrobe, "чтобы посмотреть гардероб" },
                { InteractionTypes.Fridge, "чтобы посмотреть холодильник" },

                { InteractionTypes.BusinessInfo, "для просмотра информации" },
                { InteractionTypes.BusinessEnter, "для взаимодействия" },

                { InteractionTypes.Interact, "для взаимодействия" },

                { InteractionTypes.NpcDialogue, "чтобы поговорить" },

                { InteractionTypes.ATM, "чтобы воспользоваться банкоматом" },

                { InteractionTypes.TuningEnter, "чтобы перейти к тюнингу" },

                { InteractionTypes.ShootingRangeEnter, "чтобы войти в тир [${0}]" },

                { InteractionTypes.ApartmentsRootEnter, "чтобы войти" },
                { InteractionTypes.ApartmentsRootExit, "чтобы выйти на улицу" },
                { InteractionTypes.ApartmentsRootElevator, "чтобы воспользоваться лифтом" },
                { InteractionTypes.ElevatorInteract, "чтобы воспользоваться лифтом" },

                { InteractionTypes.GarageRootEnter, "для взаимодействия" },

                { InteractionTypes.ContainerInteract, "для взаимодействия" },
                { InteractionTypes.FractionCreationWorkbenchInteract, "для взаимодействия" },
                { InteractionTypes.FractionLockerRoomInteract, "чтобы переодеться" },

                { InteractionTypes.DrivingSchoolInteract, "чтобы выбрать категорию и пройти тест" },

                { InteractionTypes.EstateAgencyInteract, "чтобы посмотреть список предложений" },

                { InteractionTypes.CasinoRouletteInteract, "чтобы встать за стол" },
                { InteractionTypes.CasinoBlackjackInteract, "чтобы сесть за стол" },
                { InteractionTypes.CasinoLuckyWheelInteract, "чтобы прокрутить колесо удачи" },
                { InteractionTypes.CasinoSlotMachineInteract, "чтобы сесть за автомат" },

                { InteractionTypes.MarketStallInteract, "чтобы начать торговать" },
            };
        }
    }
}
