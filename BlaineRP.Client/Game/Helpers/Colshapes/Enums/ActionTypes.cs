namespace BlaineRP.Client.Game.Helpers.Colshapes.Enums
{
    public enum ActionTypes
    {
        /// <summary>Никакой, в таком случае нужно в ручную прописывать действия через OnEnter/OnExit</summary>
        None = -1,

        /// <summary>Зеленая (безопасная) зона</summary>
        GreenZone,

        /// <summary>Область, в которой можно заправлять транспорт (на заправках)</summary>
        GasStation,

        HouseEnter,
        HouseExit,

        BusinessInfo,

        IPL,

        NpcDialogue,

        ATM,

        TuningEnter,

        ReachableBlip,
        ShootingRangeEnter,

        ApartmentsRootEnter,
        ApartmentsRootExit,

        GarageRootEnter,

        VehicleSpeedLimit,

        ContainerInteract,
        FractionInteract,

        DrivingSchoolInteract,
        EstateAgencyInteract,

        ElevatorInteract,

        CasinoInteract,

        MarketStallInteract,
    }
}