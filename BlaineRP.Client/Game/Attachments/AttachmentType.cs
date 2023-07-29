namespace BlaineRP.Client.Game.Attachments
{
    public enum AttachmentType
    {
        // Entity-Object Attach | Типы, которые прикрепляют серверную сущность к клиентскому объекту (создается у всех клиентов в зоне стрима)

        // Static Types | Типы, которые не открепляются при телепорте и не влияют на возможность совершения игроком каких-либо действий

        PedRingLeft3,
        PedRingRight3,

        WeaponRightTight,
        WeaponLeftTight,
        WeaponRightBack,
        WeaponLeftBack,

        PhoneSync,

        ParachuteSync,

        // Object In Hand Types | Типы, наличие у игрока которых запрещает определенные действия (ведь предмет находится в руках)

        VehKey,

        Cuffs,
        CableCuffs,

        ItemFishingRodG,
        ItemFishG,

        ItemShovel,

        ItemMetalDetector,

        ItemCigHand,
        ItemCig1Hand,
        ItemCig2Hand,
        ItemCig3Hand,

        ItemCigMouth,
        ItemCig1Mouth,
        ItemCig2Mouth,
        ItemCig3Mouth,

        ItemBurger,
        ItemChips,
        ItemHotdog,
        ItemChocolate,
        ItemPizza,
        ItemCola,
        ItemBeer,
        ItemVodka,
        ItemRum,
        ItemVegSmoothie,
        ItemSmoothie,
        ItemMilkshake,
        ItemMilk,

        ItemBandage,
        ItemMedKit,

        FarmPlantSmallShovel,

        FarmWateringCan,

        FarmOrangeBoxCarry,

        FarmMilkBucketCarry,

        EmsHealingBedFakeAttach,

        // Entity-Entity Attach | Типы, которые прикрепляют серверную сущность с серверной сущности

        /// <summary>Прикрепление СЕРВЕРНОГО трейлера к СЕРВЕРНОМУ транспорту</summary>
        VehicleTrailer,

        /// <summary>Прикрепление ЛОКАЛЬНОГО трейлера (создается локально при прикреплении) к СЕРВЕРНОЙ лодке</summary>
        TrailerObjOnBoat,

        /// <summary>Прикрепление СЕРВЕРНОГО транспорта к СЕРВЕРНОЙ лодке (к которой должен быть прикреплен TrailerObjOnBoat)</summary>
        VehicleTrailerObjBoat,

        TractorTrailFarmHarv,

        PushVehicle,

        Carry,
        PiggyBack,
        Hostage,

        PoliceEscort,

        VehicleTrunk,

        PlayerResurrect,
    }
}