namespace BlaineRP.Client.Game.Helpers.Colshapes.Enums
{
    public enum InteractionTypes
    {
        None = -1,

        BusinessEnter,
        [Language.Localized("INTERACTION_L_INFO_0", "INTERACTION_TEXT_0")]
        BusinessInfo,

        HouseEnter,
        [Language.Localized("INTERACTION_L_EXIT_0", "INTERACTION_TEXT_0")]
        HouseExit,

        [Language.Localized("INTERACTION_L_EXIT_0", "INTERACTION_TEXT_0")]
        GarageExit,

        [Language.Localized("INTERACTION_L_LOCKER_0", "INTERACTION_TEXT_0")]
        Locker,
        [Language.Localized("INTERACTION_L_FRIDGE_0", "INTERACTION_TEXT_0")]
        Fridge,
        [Language.Localized("INTERACTION_L_WARDROBE_0", "INTERACTION_TEXT_0")]
        Wardrobe,

        [Language.Localized("INTERACTION_L_GEN_0", "INTERACTION_TEXT_0")]
        Interact,

        [Language.Localized("INTERACTION_L_TALK_0", "INTERACTION_TEXT_0")]
        NpcDialogue,

        [Language.Localized("INTERACTION_L_ATM_0", "INTERACTION_TEXT_0")]
        ATM,

        [Language.Localized("INTERACTION_L_TUNING_0", "INTERACTION_TEXT_0")]
        TuningEnter,
        [Language.Localized("INTERACTION_L_SHOOTING_RANGE_0", "INTERACTION_TEXT_0")]
        ShootingRangeEnter,

        [Language.Localized("INTERACTION_L_ENTER_0", "INTERACTION_TEXT_0")]
        ApartmentsRootEnter,
        [Language.Localized("INTERACTION_L_EXIT_1", "INTERACTION_TEXT_0")]
        ApartmentsRootExit,
        [Language.Localized("INTERACTION_L_ELEVATOR_0", "INTERACTION_TEXT_0")]
        ApartmentsRootElevator,

        GarageRootEnter,

        ContainerInteract,
        FractionCreationWorkbenchInteract,
        [Language.Localized("INTERACTION_L_CHANGE_CLOTHES_0", "INTERACTION_TEXT_0")]
        FractionLockerRoomInteract,

        [Language.Localized("INTERACTION_L_DRIVINGSCHOOL_TEST_0", "INTERACTION_TEXT_0")]
        DrivingSchoolInteract,

        [Language.Localized("INTERACTION_L_ESTATE_AGENCY_LOOK_0", "INTERACTION_TEXT_0")]
        EstateAgencyInteract,

        FractionPoliceArrestMenuInteract,

        ElevatorInteract,

        [Language.Localized("INTERACTION_L_CASINO_ROULETTE_0", "INTERACTION_TEXT_0")]
        CasinoRouletteInteract,
        [Language.Localized("INTERACTION_L_CASINO_LUCKYWHEEL_0", "INTERACTION_TEXT_0")]
        CasinoLuckyWheelInteract,
        [Language.Localized("INTERACTION_L_CASINO_SLOTMACHINE_0", "INTERACTION_TEXT_0")]
        CasinoSlotMachineInteract,
        [Language.Localized("INTERACTION_L_CASINO_BLACKJACK_0", "INTERACTION_TEXT_0")]
        CasinoBlackjackInteract,
        [Language.Localized("INTERACTION_L_CASINO_BLACKJACK_0", "INTERACTION_TEXT_0")]
        CasinoPockerInteract,

        [Language.Localized("INTERACTION_L_MARKETSTALL_0", "INTERACTION_TEXT_0")]
        MarketStallInteract,
    }
}